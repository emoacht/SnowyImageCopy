using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SnowyImageCopy.Models.ImageFile;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// File signatures
	/// </summary>
	internal class Signatures
	{
		/// <summary>
		/// The maximum number of signatures to be handled
		/// </summary>
		/// <remarks>
		/// The number of signatures can go beyond this number during a lifetime of this application,
		/// while the excess ones (from older) will not be restored next time.
		/// </remarks>
		public static int MaxCount
		{
			get => _maxCount;
			set { if (value > 0) { _maxCount = value; } }
		}
		private static int _maxCount = 10000;

		private static List<Signatures> _instances;
		private static readonly Lazy<SemaphoreSlim> _semaphore = new(() => new(1, 1));

		public static async Task<Signatures> GetAsync(string indexString, CancellationToken cancellationToken)
		{
			try
			{
				await _semaphore.Value.WaitAsync(cancellationToken);

				_instances ??= new List<Signatures>();

				var instance = _instances.FirstOrDefault(x => x.IndexString == indexString);
				if (instance is null)
				{
					var signatures = await LoadAsync(indexString, valueSize: HashItem.Size, maxCount: MaxCount, cancellationToken);
					instance = new Signatures(indexString, signatures);
					_instances.Add(instance);
				}
				return instance;
			}
			finally
			{
				_semaphore.Value.Release();
			}
		}

		public static void Close(string indexString)
		{
			int index = _instances?.FindIndex(x => x.IndexString == indexString) ?? -1;
			if (index >= 0)
			{
				_instances[index].Close();
				_instances.RemoveAt(index);
			}
		}

		#region Instance

		private string IndexString { get; }
		private HashSet<HashItem> _signatures;

		private Signatures(string indexString, HashItem[] signatures)
		{
			this.IndexString = indexString;
			this._signatures = new HashSet<HashItem>(signatures);
		}

		/// <summary>
		/// The number of signatures currently handled
		/// </summary>
		public int CurrentCount => _signatures?.Count ?? 0;

		public bool Contains(HashItem value)
		{
			return (_signatures?.Contains(value) is true);
		}

		private List<HashItem> _appendValues;

		public bool Append(HashItem value)
		{
			if (_signatures?.Add(value) is not true)
				return false;

			_appendValues ??= new List<HashItem>();
			_appendValues.Add(value);
			return true;
		}

		public void Append(IEnumerable<HashItem> values)
		{
			if (values is null)
				return;

			foreach (var value in values)
				Append(value);
		}

		public async Task FlushAsync(CancellationToken cancellationToken)
		{
			if (_appendValues is null or { Count: 0 })
				return;

			await SaveAsync(IndexString, _appendValues, _signatures, valueSize: HashItem.Size, maxCount: MaxCount, cancellationToken);

			_appendValues.Clear();
		}

		public void Close()
		{
			_signatures?.Clear();
			_signatures = null;
		}

		#endregion

		#region Load/Save/Delete

		private static string GetSignaturesFileName(in string value) => $"signatures{value}.bin";
		private static string GetSignaturesFilePath(in string value) => FolderService.GetAppDataFilePath(GetSignaturesFileName(value));

		private const int BufferSize = 81920;
		private const float ExcessFactor = 1.2F;

		private static async Task<HashItem[]> LoadAsync(string indexString, int valueSize, int maxCount, CancellationToken cancellationToken)
		{
			var filePath = GetSignaturesFilePath(indexString);
			var fileInfo = new FileInfo(filePath);

			if (!CanLoad(fileInfo, valueSize))
				return new HashItem[0];

			try
			{
				using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					var offset = fs.Length - (valueSize * maxCount);
					if (offset > 0)
						fs.Seek(offset, SeekOrigin.Begin);

					using (var ms = new MemoryStream((int)(fs.Length - fs.Position)))
					{
						await fs.CopyToAsync(ms, BufferSize, cancellationToken).ConfigureAwait(false); // Read the file at once.
						ms.Seek(0, SeekOrigin.Begin);

						IEnumerable<HashItem> Enumerate()
						{
							var buffer = new byte[valueSize];

							while (ms.Read(buffer, 0, valueSize) == valueSize)
								yield return HashItem.Restore(buffer);
						}

						return Enumerate().ToArray(); // To catch an exception, it must be consumed here.
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load signatures.\r\n{ex}");
				throw;
			}
		}

		private static async Task SaveAsync(string indexString, IList<HashItem> appendValues, ISet<HashItem> wholeValues, int valueSize, int maxCount, CancellationToken cancellationToken)
		{
			var filePath = GetSignaturesFilePath(indexString);
			var fileInfo = new FileInfo(filePath);

			var canAppend = CanLoad(fileInfo, valueSize);
			if (canAppend)
			{
				var existingValuesCount = fileInfo.Length / valueSize;
				if (existingValuesCount + appendValues.Count > maxCount * ExcessFactor)
					canAppend = false;
			}

			var fileMode = canAppend ? FileMode.Append : FileMode.Create;
			var values = canAppend ? appendValues : wholeValues.Skip(Math.Max(0, wholeValues.Count - maxCount));

			try
			{
				FolderService.AssureAppDataFolder();

				using (var fs = new FileStream(filePath, fileMode, FileAccess.Write))
				{
					foreach (var value in values)
						await fs.WriteAsync(value.ToByteArray(), 0, valueSize, cancellationToken).ConfigureAwait(false);

					await fs.FlushAsync(cancellationToken).ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to save signatures.\r\n{ex}");
				throw;
			}
		}

		private static bool CanLoad(FileInfo fileInfo, int valueSize)
		{
			return fileInfo.Exists
				&& (fileInfo.Length > 0)
				&& (fileInfo.Length % valueSize == 0);
		}

		internal static void Delete(string indexString) => FolderService.Delete(GetSignaturesFilePath(indexString));

		#endregion
	}
}
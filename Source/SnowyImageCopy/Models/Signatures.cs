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

		/// <summary>
		/// The number of signatures currently handled
		/// </summary>
		public static int CurrentCount => _signatures?.Count ?? 0;

		private static HashSet<HashItem> _signatures;

		public static async Task PrepareAsync(string indexString, CancellationToken cancellationToken)
		{
			_signatures ??= new HashSet<HashItem>(await LoadAsync(indexString, valueSize: HashItem.Size, maxCount: MaxCount, cancellationToken));
		}

		public static bool Contains(HashItem value)
		{
			return (_signatures?.Contains(value) == true);
		}

		private static List<HashItem> _appendValues;

		public static bool Append(HashItem value)
		{
			if (!(_signatures?.Add(value) == true))
				return false;

			_appendValues ??= new List<HashItem>();
			_appendValues.Add(value);
			return true;
		}

		public static void Append(IEnumerable<HashItem> values)
		{
			if (values is null)
				return;

			foreach (var value in values)
				Append(value);
		}

		public static async Task FlushAsync(string indexString, CancellationToken cancellationToken)
		{
			if ((_appendValues?.Count ?? 0) == 0)
				return;

			await SaveAsync(indexString, _appendValues, _signatures, valueSize: HashItem.Size, maxCount: MaxCount, cancellationToken);

			_appendValues.Clear();
		}

		public static void Clear()
		{
			_signatures?.Clear();
			_signatures = null;
		}

		#region Load/Save/Delete

		private static string GetSignaturesFileName(in string value) => $"signatures{value}.bin";
		private static string GetSignaturesFilePath(in string value) => FolderService.GetAppDataFilePath(GetSignaturesFileName(value));

		private const int BufferSize = 81920;
		private const float ExcessFactor = 1.2F;

		private static async Task<HashItem[]> LoadAsync(string indexString, int valueSize, int maxCount, CancellationToken cancellationToken)
		{
			var filePath = GetSignaturesFilePath(indexString);
			var fileInfo = new FileInfo(filePath);
			if (!fileInfo.Exists || (fileInfo.Length == 0) || (fileInfo.Length % valueSize != 0))
				return new HashItem[0];

			try
			{
				using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
				{
					var offset = fs.Length - valueSize * maxCount;
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
			var isAppend = fileInfo.Exists && (fileInfo.Length > 0) && (fileInfo.Length % valueSize == 0)
				&& ((fileInfo.Length / valueSize + appendValues.Count) < (maxCount * ExcessFactor));

			var fileMode = isAppend ? FileMode.Append : FileMode.Create;
			var values = isAppend ? appendValues : wholeValues.Skip(Math.Max(0, wholeValues.Count - maxCount));

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

		internal static void Delete(string indexString) => FolderService.Delete(GetSignaturesFilePath(indexString));

		#endregion
	}
}
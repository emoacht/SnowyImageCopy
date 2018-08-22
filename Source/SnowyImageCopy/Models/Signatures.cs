using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
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

		public static async Task PrepareAsync()
		{
			if (_signatures != null)
				return;

			_signatures = await Task.Run(() => new HashSet<HashItem>(Load(valueSize: HashItem.Size, maxCount: MaxCount)));
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

			if (_appendValues == null)
				_appendValues = new List<HashItem>();

			_appendValues.Add(value);
			return true;
		}

		public static void Append(IEnumerable<HashItem> values)
		{
			if (values == null)
				return;

			foreach (var value in values)
				Append(value);
		}

		public static async Task FlushAsync()
		{
			if ((_appendValues?.Count ?? 0) == 0)
				return;

			await SaveAsync(_appendValues, _signatures, valueSize: HashItem.Size, maxCount: MaxCount);

			_appendValues.Clear();
		}

		public static void Clear()
		{
			_signatures?.Clear();
			_signatures = null;
			//Delete();
		}

		#region Load/Save

		private const string SignaturesFileName = "signatures.bin";
		private static readonly string _signaturesFilePath = Path.Combine(FolderService.AppDataFolderPath, SignaturesFileName);

		private const float ExcessFactor = 1.2F;

		private static HashItem[] Load(int valueSize, int maxCount)
		{
			var fileInfo = new FileInfo(_signaturesFilePath);
			if (!(fileInfo.Exists && (0 < fileInfo.Length) && (fileInfo.Length % valueSize == 0)))
				return new HashItem[0];

			try
			{
				return Read().ToArray(); // To catch an exception, it must be consumed here.
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load signatures.\r\n{ex}");
				throw;
			}

			IEnumerable<HashItem> Read()
			{
				using (var fs = new FileStream(_signaturesFilePath, FileMode.Open, FileAccess.Read))
				{
					var offset = fs.Length - valueSize * maxCount;
					if (offset > 0)
						fs.Seek(offset, SeekOrigin.Begin);

					using (var ms = new MemoryStream((int)(fs.Length - fs.Position)))
					{
						fs.CopyTo(ms); // Read the file at once.
						ms.Seek(0, SeekOrigin.Begin);

						var buff = new byte[valueSize];

						while (ms.Read(buff, 0, valueSize) == valueSize)
							yield return HashItem.Restore(buff);
					}
				}
			};
		}

		private static async Task SaveAsync(IList<HashItem> appendValues, ISet<HashItem> wholeValues, int valueSize, int maxCount)
		{
			var fileInfo = new FileInfo(_signaturesFilePath);
			var isAppend = fileInfo.Exists && (fileInfo.Length > 0) && (fileInfo.Length % valueSize == 0)
				&& ((fileInfo.Length / valueSize + appendValues.Count) < (maxCount * ExcessFactor));

			var fileMode = isAppend ? FileMode.Append : FileMode.Create;
			var values = isAppend ? appendValues : wholeValues.Skip(Math.Max(0, wholeValues.Count - maxCount));

			try
			{
				FolderService.AssureAppDataFolder();

				using (var fs = new FileStream(_signaturesFilePath, fileMode, FileAccess.Write))
				{
					foreach (var value in values)
						await fs.WriteAsync(value.ToByteArray(), 0, valueSize);

					await fs.FlushAsync();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to save signatures.\r\n{ex}");
				throw;
			}
		}

		private static void Delete() => File.Delete(_signaturesFilePath);

		#endregion
	}
}
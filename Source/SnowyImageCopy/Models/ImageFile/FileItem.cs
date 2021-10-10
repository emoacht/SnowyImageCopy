using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SnowyImageCopy.Helper;
using Extension = SnowyImageCopy.Models.ImageFile.FileExtension;

namespace SnowyImageCopy.Models.ImageFile
{
	/// <summary>
	/// File item in FlashAir card
	/// </summary>
	/// <remarks>This class should be immutable.</remarks>
	internal class FileItem : IFileItem
	{
		#region Basic

		public string Directory { get; private set; }
		public string FileName { get; private set; }
		public string FileExtension { get; private set; }
		public int Size { get; private set; } // In bytes

		public bool IsReadOnly { get; private set; }
		public bool IsHidden { get; private set; }
		public bool IsSystem { get; private set; }
		public bool IsVolume { get; private set; }
		public bool IsDirectory { get; private set; }
		public bool IsArchive { get; private set; }

		public DateTime Date { get; private set; }

		#endregion

		#region Supplementary

		public bool IsImported { get; private set; }

		public string FilePath { get; private set; }
		public HashItem Signature { get; private set; }

		public bool IsImageFile { get; private set; }
		public bool IsJpeg { get; private set; }
		public bool IsTiff { get; private set; }
		public bool IsLoadable { get; private set; }

		public bool IsFlashAirSystem =>
			string.Equals(FileName, "GUPIXINF", StringComparison.OrdinalIgnoreCase)
			|| string.Equals(FileName, "SD_WLAN", StringComparison.OrdinalIgnoreCase)
			|| (string.Equals(FileName, "FA000001.JPG", StringComparison.OrdinalIgnoreCase) /* Control image file */
				&& Directory.EndsWith("100__TSB", StringComparison.OrdinalIgnoreCase));

		#endregion

		#region Order

		private enum Order : int
		{
			Ascending = 1,
			Descending = -1
		}

		internal static bool OrderByAscendingDate
		{
			get => (_orderByAscendingDate == Order.Ascending);
			set => _orderByAscendingDate = value ? Order.Ascending : Order.Descending;
		}
		private static Order _orderByAscendingDate = Order.Ascending; // Default

		#endregion

		#region Constructor

		public FileItem(string fileEntry, string directoryPath)
		{
			IsImported = Import(fileEntry, directoryPath);
		}

		#endregion

		#region Import

		private const char Separator = ','; // Separator character (comma)
		private static readonly Regex _asciiPattern = new Regex(@"^[\x20-\x7F]+$", RegexOptions.Compiled); // Pattern for ASCII code (alphanumeric symbols)

		/// <summary>
		/// Imports file entry from a file list in FlashAir card.
		/// </summary>
		/// <param name="fileEntry">File entry from the list</param>
		/// <param name="directoryPath">Remote directory path used to get the list</param>
		/// <returns>True if successfully imported</returns>
		private bool Import(string fileEntry, string directoryPath)
		{
			if (string.IsNullOrWhiteSpace(fileEntry))
				return false;

			var fileEntryWithoutDirectory = fileEntry.Trim();

			if (string.IsNullOrWhiteSpace(directoryPath))
			{
				Directory = string.Empty;
			}
			else
			{
				// Check if the leading part of file entry matches directory path. Be aware that the length of
				// file entry like "WLANSD_FILELIST" may be shorter than that of directory path.
				if (!fileEntry.StartsWith(directoryPath, StringComparison.OrdinalIgnoreCase))
					return false;

				Directory = directoryPath;

				// Check if directory path is valid.
				if (!_asciiPattern.IsMatch(Directory) || // This ASCII checking may be needless because response from FlashAir card seems to be encoded by ASCII.
					Path.GetInvalidPathChars().Concat(new[] { '?' }).Any(x => Directory.Contains(x))) // '?' appears typically when byte array is not correctly encoded.
					return false;

				fileEntryWithoutDirectory = fileEntry.Substring(directoryPath.Length).TrimStart();
			}

			if (!fileEntryWithoutDirectory.ElementAt(0).Equals(Separator))
				return false;

			var elements = fileEntryWithoutDirectory.TrimStart(Separator)
				.Split(new[] { Separator }, StringSplitOptions.None)
				.ToList();

			if (elements.Count < 5) // 5 means file name, size, raw attribute, raw data and raw time.
				return false;

			while (elements.Count > 5) // In the case that file name includes separator character
			{
				elements[0] = $"{elements[0]}{Separator}{elements[1]}";
				elements.RemoveAt(1);
			}

			FileName = elements[0].Trim();

			// Check if file name is valid.
			if (string.IsNullOrWhiteSpace(FileName) ||
				!_asciiPattern.IsMatch(FileName) || // This ASCII checking may be needless because response from FlashAir card seems to be encoded by ASCII.
				Path.GetInvalidFileNameChars().Any(x => FileName.Contains(x)))
				return false;

			FilePath = $"{Directory}/{FileName}".ToLower();

			// Determine size, attribute and date.
			int rawDate = 0;
			int rawTime = 0;

			for (int i = 1; i <= 4; i++)
			{
				if (!int.TryParse(elements[i], out var num))
					return false;

				switch (i)
				{
					case 1:
						// In the case that file size is larger than 2GiB (Int32.MaxValue in bytes), it cannot pass
						// Int32.TryParse method and so such file will be ignored.
						Size = num;
						break;
					case 2:
						SetAttributes(num);
						break;
					case 3:
						rawDate = num;
						break;
					case 4:
						rawTime = num;
						break;
				}
			}

			Date = FatDateTime.ConvertFromDateIntAndTimeIntToDateTime(rawDate, rawTime, DateTimeKind.Local);

			// Determine file extension.
			if (!IsDirectory && !IsVolume)
			{
				SetFileExtension(FileName);
			}

			if (IsImageFile)
				Signature = GetSignature(Date, FilePath, Size);

			return true;
		}

		private void SetAttributes(int rawAttribute)
		{
			var ba = new BitArray(new[] { rawAttribute }); // This length is always 32 because value is int.

			IsReadOnly = ba[0];  // Bit 0
			IsHidden = ba[1];    // Bit 1
			IsSystem = ba[2];    // Bit 2
			IsVolume = ba[3];    // Bit 3
			IsDirectory = ba[4]; // Bit 4
			IsArchive = ba[5];   // Bit 5
		}

		private static readonly Lazy<HashSet<Extension>> _extensions = new(() => new(Enum.GetValues(typeof(Extension)).Cast<Extension>()));

		private void SetFileExtension(string fileName)
		{
			FileExtension = Path.GetExtension(fileName);

			var extension = _extensions.Value.FirstOrDefault(x => string.Equals(FileExtension, $".{x}", StringComparison.OrdinalIgnoreCase));
			if (extension == Extension.other)
				return;

			IsImageFile = true;

			switch (extension)
			{
				case Extension.jpg:
				case Extension.jpeg:
					IsLoadable = IsJpeg = true;
					break;
				case Extension.tif:
				case Extension.tiff:
					IsLoadable = IsTiff = true;
					break;
				case Extension.bmp:
				case Extension.png:
				case Extension.raw:
				case Extension.dng:
				case Extension.cr2:
				case Extension.crw:
				case Extension.erf:
				case Extension.raf:
				case Extension.kdc:
				case Extension.nef:
				case Extension.orf:
				case Extension.rw2:
				case Extension.pef:
				case Extension.srw:
				case Extension.arw:
					IsLoadable = true;
					break;
			}
		}

		private static HashItem GetSignature(DateTime date, string filePath, int size)
		{
			var dateBytes = BitConverter.GetBytes(date.Ticks);
			var filePathBytes = Encoding.UTF8.GetBytes(filePath.ToLower());
			var sizeBytes = BitConverter.GetBytes(size);

			return HashItem.Compute(sizeBytes.Concat(filePathBytes).Concat(dateBytes));
		}

		#endregion

		#region IComparable

		public int CompareTo(IFileItem other)
		{
			if (other is null)
				return 1;

			var dateComparison = this.Date.CompareTo(other.Date);
			if (dateComparison != 0)
				return dateComparison * (int)_orderByAscendingDate;

			var filePathComparison = string.Compare(this.FilePath, other.FilePath, StringComparison.Ordinal);
			if (filePathComparison != 0)
				return filePathComparison;

			return this.Size.CompareTo(other.Size);
		}

		public override bool Equals(object obj) => this.Equals(obj as IFileItem);

		public virtual bool Equals(IFileItem other)
		{
			if (other is null)
				return false;

			if (object.ReferenceEquals(this, other))
				return true;

			if ((this.Signature is not null) &&
				(this.Signature == other.Signature))
				return true;

			return (this.CompareTo(other) == 0);
		}

		public override int GetHashCode()
		{
			return Signature?.GetHashCode() ?? (Date, FilePath, Size).GetHashCode();
		}

		#endregion
	}
}
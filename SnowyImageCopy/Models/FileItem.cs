using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Models
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
		public int Size { get; private set; } // In bytes

		public bool IsReadOnly { get; private set; }
		public bool IsHidden { get; private set; }
		public bool IsSystemFile { get; private set; }
		public bool IsVolume { get; private set; }
		public bool IsDirectory { get; private set; }
		public bool IsArchive { get; private set; }

		public DateTime Date { get; private set; }
		public FileExtension FileExtension { get; private set; }

		#endregion

		#region Supplementary

		public bool IsImported { get; private set; }

        public string FilePath
		{
			get { return _filePath ?? (_filePath = String.Format("{0}/{1}", Directory, FileName)); }
		}
		private string _filePath;

		public string Signature
		{
			get { return _signature ?? (_signature = String.Format("{0:yyyyMMddHHmmss}{1}{2}", Date, FilePath, Size)); }
		}
		private string _signature;

		public bool IsImageFile
		{
			get { return (FileExtension != FileExtension.other); }
		}

        // NYGG
        public bool IsJpeg
        {
            get { return FileExtension == FileExtension.jpg || FileExtension == FileExtension.jpeg; }
        }

        private static readonly string[] _flashAirSystemFolders =
		{
			"GUPIXINF",
			"SD_WLAN",
			"100__TSB",
		};

		public bool IsFlashAirSystemFolder
		{
			get { return _flashAirSystemFolders.Contains(FileName, StringComparer.OrdinalIgnoreCase); }
		}

		#endregion

		#region Constructor

		public FileItem(string fileEntry, string directoryPath)
		{
			IsImported = Import(fileEntry, directoryPath);
		}

		#endregion

		#region Import

		private const char _separator = ','; // Separator character (comma)
		private static readonly Regex _asciiPattern = new Regex(@"^[\x20-\x7F]+$", RegexOptions.Compiled); // Pattern for ASCII code (alphanumeric symbols)

		/// <summary>
		/// Imports file entry from a file list in FlashAir card.
		/// </summary>
		/// <param name="fileEntry">File entry from the list</param>
		/// <param name="directoryPath">Remote directory path used to get the list</param>
		/// <returns>True if successfully imported</returns>
		private bool Import(string fileEntry, string directoryPath)
		{
			if (String.IsNullOrWhiteSpace(fileEntry))
				return false;

			var fileEntryWithoutDirectory = fileEntry.Trim();

			if (!String.IsNullOrWhiteSpace(directoryPath))
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
			else
			{
				Directory = String.Empty;
			}

			if (!fileEntryWithoutDirectory.ElementAt(0).Equals(_separator))
				return false;

			var elements = fileEntryWithoutDirectory.TrimStart(_separator)
				.Split(new[] { _separator }, StringSplitOptions.None)
				.ToList();

			if (elements.Count < 5) // 5 means file name, size, raw attribute, raw data and raw time.
				return false;

			while (elements.Count > 5) // In the case that file name includes separator character
			{
				elements[0] = String.Format("{0}{1}{2}", elements[0], _separator, elements[1]);
				elements.RemoveAt(1);
			}

			FileName = elements[0].Trim();

			// Check if file name is valid.
			if (String.IsNullOrWhiteSpace(FileName) ||
				!_asciiPattern.IsMatch(FileName) || // This ASCII checking may be needless because response from FlashAir card seems to be encoded by ASCII.
				Path.GetInvalidFileNameChars().Any(x => FileName.Contains(x)))
				return false;

			// Determine size, attribute and date.
			int rawDate = 0;
			int rawTime = 0;

			for (int i = 1; i <= 4; i++)
			{
				int num;
				if (!int.TryParse(elements[i], out num))
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

			Date = FatDateTime.ConvertFromDateIntAndTimeIntToDateTime(rawDate, rawTime);

			// Determine file extension.
			if ((0 < Size) && !IsDirectory && !IsVolume)
			{
				FileExtension = Enum.GetValues(typeof(FileExtension))
					.Cast<FileExtension>()
					.FirstOrDefault(x =>
					{
						var extension = Path.GetExtension(FileName); // Extension will not be null as long as FileName is not null.
						return extension.Equals(String.Format(".{0}", x), StringComparison.OrdinalIgnoreCase);
					});
			}

			return true;
		}

		private void SetAttributes(int rawAttribute)
		{
			var ba = new BitArray(new[] { rawAttribute }); // This length is always 32 because value is int.

			IsReadOnly = ba[0];   // Bit 0
			IsHidden = ba[1];     // Bit 1
			IsSystemFile = ba[2]; // Bit 2
			IsVolume = ba[3];     // Bit 3
			IsDirectory = ba[4];  // Bit 4
			IsArchive = ba[5];    // Bit 5
		}

		#endregion

		#region IComparable member

		public int CompareTo(IFileItem other)
		{
			if (other == null)
				return 1;

			var dateComparison = this.Date.CompareTo(other.Date);
			if (dateComparison != 0)
				return dateComparison;

			var filePathComparison = String.Compare(this.FilePath, other.FilePath, StringComparison.Ordinal);
			if (filePathComparison != 0)
				return filePathComparison;

			return this.Size.CompareTo(other.Size);
		}

		#endregion
	}
}
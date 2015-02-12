using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Models;
using SnowyImageCopy.Views.Controls;

namespace SnowyImageCopy.ViewModels
{
	public class FileItemViewModel : ViewModel, IComparable<FileItemViewModel>
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

		#endregion


		#region Thumbnail

		private static readonly BitmapImage _defaultThumbnail =
			ImageManager.ConvertFrameworkElementToBitmapImage(new ThumbnailBox());

		public BitmapImage Thumbnail
		{
			get { return _thumbnail ?? _defaultThumbnail; }
			set
			{
				_thumbnail = value;
				RaisePropertyChanged();
			}
		}
		private BitmapImage _thumbnail;

		public bool HasThumbnail
		{
			get { return _thumbnail != null; }
		}

		#endregion


		#region Supplementary

		internal bool IsImported { get; private set; }

		internal string FilePath
		{
			get { return String.Format("{0}/{1}", Directory, FileName); }
		}

		internal string FileNameWithCaseExtension
		{
			get
			{
				if (!Settings.Current.MakesFileExtensionLowerCase)
					return FileName;

				var extension = Path.GetExtension(FileName);
				if (String.IsNullOrEmpty(extension))
					return FileName;

				return Path.GetFileNameWithoutExtension(FileName) + extension.ToLower();
			}
		}

		internal string Signature
		{
			get { return String.Format("{0}-{1}-{2:yyyyMMddHHmmss}", FilePath, Size, Date); }
		}

		internal FileExtension FileExtension { get; private set; }

		internal bool IsImageFile
		{
			get { return (FileExtension != FileExtension.other); }
		}

		/// <summary>
		/// Whether can read Exif metadata
		/// </summary>
		internal bool CanReadExif
		{
			get
			{
				switch (FileExtension)
				{
					case FileExtension.jpg:
					case FileExtension.jpeg:
					case FileExtension.tif:
					case FileExtension.tiff:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Whether can get a thumbnail of a remote file
		/// </summary>
		/// <remarks>FlashAir card can provide a thumbnail only from JPEG format file.</remarks>
		internal bool CanGetThumbnailRemote
		{
			get
			{
				if (_canGetThumbnailRemote.HasValue)
					return _canGetThumbnailRemote.Value;

				switch (FileExtension)
				{
					case FileExtension.jpg:
					case FileExtension.jpeg:
						return true;
					default:
						return false;
				}
			}
			set { _canGetThumbnailRemote = value; }
		}
		private bool? _canGetThumbnailRemote;

		/// <summary>
		/// Whether can load image data from a local file
		/// </summary>
		/// <remarks>As for raw images, actual loadability depends on Microsoft Camera Codec Pack.</remarks>
		internal bool CanLoadDataLocal
		{
			get
			{
				if (_canLoadDataLocal.HasValue)
					return _canLoadDataLocal.Value;

				switch (FileExtension)
				{
					case FileExtension.jpg:
					case FileExtension.jpeg:
					case FileExtension.bmp:
					case FileExtension.png:
					case FileExtension.tif:
					case FileExtension.tiff:
					case FileExtension.raw:
					case FileExtension.dng:
					case FileExtension.cr2:
					case FileExtension.crw:
					case FileExtension.erf:
					case FileExtension.raf:
					case FileExtension.kdc:
					case FileExtension.nef:
					case FileExtension.orf:
					case FileExtension.rw2:
					case FileExtension.pef:
					case FileExtension.srw:
					case FileExtension.arw:
						return true;
					default:
						return false;
				}
			}
			set { _canLoadDataLocal = value; }
		}
		private bool? _canLoadDataLocal;

		private static readonly string[] _flashAirSystemFolders =
		{
			"GUPIXINF",
			"SD_WLAN",
			"100__TSB",
		};

		internal bool IsFlashAirSystemFolder
		{
			get { return _flashAirSystemFolders.Contains(FileName, StringComparer.OrdinalIgnoreCase); }
		}

		#endregion


		#region Operation

		public bool IsTarget
		{
			get
			{
				switch (Settings.Current.TargetPeriod)
				{
					case FilePeriod.Today:
						return Date.Date == DateTime.Today;

					case FilePeriod.Select:
						return Settings.Current.TargetDates.Contains(Date.Date);

					default: // FilePeriod.All
						return true;
				}
			}
		}

		public bool IsDescendant
		{
			get { return Directory.StartsWith(Settings.Current.RemoteDescendant, StringComparison.OrdinalIgnoreCase); }
		}

		public bool IsAliveRemote { get; set; }
		public bool IsAliveLocal { get; set; }

		public FileStatus Status
		{
			get { return _status; }
			set
			{
				_status = value;
				RaisePropertyChanged();
			}
		}
		private FileStatus _status = FileStatus.Unknown;

		public bool IsSelected
		{
			get { return false; } // Always false
			set
			{
				if (!value)
					return;

				RaisePropertyChanged();
			}
		}

		public DateTime CopiedTime { get; set; }

		#endregion


		#region Constructor

		internal FileItemViewModel()
			: this(String.Empty, String.Empty)
		{ }

		internal FileItemViewModel(string source, string directoryPath)
		{
			Import(source, directoryPath);

			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_resourcesPropertyChangedListener = new PropertyChangedEventListener(ReactResourcesPropertyChanged);
				PropertyChangedEventManager.AddListener(ResourceService.Current, _resourcesPropertyChangedListener, "Resources");
			}
		}

		#endregion


		#region IComparable member

		public int CompareTo(FileItemViewModel other)
		{
			if (other == null)
				return 1;

			var comparisonDate = this.Date.CompareTo(other.Date);
			var comparisonFilePath = String.Compare(this.FilePath, other.FilePath, StringComparison.Ordinal);

			return (comparisonDate != 0) ? comparisonDate : comparisonFilePath;
		}

		#endregion


		#region Event Listener

		private PropertyChangedEventListener _resourcesPropertyChangedListener;

		private void ReactResourcesPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Resources property changed: {0} {1}", sender, e.PropertyName);

			RaisePropertyChanged(() => Status);
		}

		#endregion


		#region Import

		private const char _separator = ','; // Separator character (comma)
		private static readonly Regex _asciiPattern = new Regex(@"^[\x20-\x7F]+$", RegexOptions.Compiled); // Pattern for ASCII code (alphanumeric symbols)

		/// <summary>
		/// Import file information from a file list in FlashAir card.
		/// </summary>
		/// <param name="source">Source string in the list</param>
		/// <param name="remoteDirectoryPath">Remote directory path used to get the list</param>
		internal void Import(string source, string remoteDirectoryPath)
		{
			if (String.IsNullOrWhiteSpace(source))
				return;

			var sourceWithoutDirectory = source.Trim();

			if (!String.IsNullOrWhiteSpace(remoteDirectoryPath))
			{
				// Check if the leading part of source string matches directory path. Be aware that length of 
				// source string like "WLANSD_FILELIST" may be shorter than that of directory path.
				if (!source.StartsWith(remoteDirectoryPath, StringComparison.OrdinalIgnoreCase))
					return;

				Directory = remoteDirectoryPath;

				// Check if directory path is valid.
				if (!_asciiPattern.IsMatch(Directory) || // This ASCII checking may be needless because response from FlashAir card seems to be encoded by ASCII.
					Path.GetInvalidPathChars().Concat(new[] { '?' }).Any(x => Directory.Contains(x))) // '?' appears typically when byte array was not correctly encoded.
					return;

				sourceWithoutDirectory = source.Substring(remoteDirectoryPath.Length).TrimStart();
			}
			else
			{
				Directory = String.Empty;
			}

			if (!sourceWithoutDirectory.ElementAt(0).Equals(_separator))
				return;

			var elements = sourceWithoutDirectory.TrimStart(_separator)
				.Split(new[] { _separator }, StringSplitOptions.None)
				.ToList();

			if (elements.Count < 5) // 5 means file name, size, raw attribute, raw data and raw time.
				return;

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
				return;

			// Determine size, attribute and date.
			int rawDate = 0;
			int rawTime = 0;

			for (int i = 1; i <= 4; i++)
			{
				int num;
				if (!int.TryParse(elements[i], out num))
					return;

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

			IsImported = true;
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
	}
}
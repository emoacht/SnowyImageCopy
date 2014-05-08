using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
		#region Directory, FileName, Size

		public string Directory { get; private set; }
		public string FileName { get; private set; }
		public int Size { get; private set; }

		#endregion


		#region Attribute

		public bool IsReadOnly { get; private set; }   // Bit 0
		public bool IsHidden { get; private set; }     // Bit 1
		public bool IsSystemFile { get; private set; } // Bit 2
		public bool IsVolume { get; private set; }     // Bit 3
		public bool IsDirectory { get; private set; }  // Bit 4
		public bool IsArchive { get; private set; }    // Bit 5

		public int RawAttribute
		{
			get { return _rawAttribute; }
			private set
			{
				_rawAttribute = value;
				SetAttributes(_rawAttribute);
			}
		}
		private int _rawAttribute;

		private void SetAttributes(int source)
		{
			var ba = new BitArray(new int[] { source });

			for (int i = 0; i < ba.Length; i++)
			{
				switch (i)
				{
					case 0:
						IsReadOnly = ba[i];
						break;
					case 1:
						IsHidden = ba[i];
						break;
					case 2:
						IsSystemFile = ba[i];
						break;
					case 3:
						IsVolume = ba[i];
						break;
					case 4:
						IsDirectory = ba[i];
						break;
					case 5:
						IsArchive = ba[i];
						break;
				}
			}
		}

		#endregion


		#region Date

		public DateTime Date { get; private set; }

		public int RawDate
		{
			get { return _rawDate; }
			private set
			{
				_rawDate = value;
				SetDateTime(_rawDate, RawTime);
			}
		}
		private int _rawDate;

		public int RawTime
		{
			get { return _rawTime; }
			private set
			{
				_rawTime = value;
				SetDateTime(RawDate, _rawTime);
			}
		}
		private int _rawTime;

		private void SetDateTime(int rawDate, int rawTime)
		{
			if ((rawDate <= 0) || (rawTime <= 0))
				return;

			var baDate = new BitArray(new int[] { rawDate }).Cast<bool>().ToArray();
			var baTime = new BitArray(new int[] { rawTime }).Cast<bool>().ToArray();

			var year = ConvertFromBitArrayToInt(baDate.Skip(9)) + 1980;
			var month = ConvertFromBitArrayToInt(baDate.Skip(5).Take(4));
			var day = ConvertFromBitArrayToInt(baDate.Take(5));
			var hour = ConvertFromBitArrayToInt(baTime.Skip(11));
			var minute = ConvertFromBitArrayToInt(baTime.Skip(5).Take(6));
			var second = ConvertFromBitArrayToInt(baTime.Take(5)) * 2;

			Date = new DateTime(year, month, day, hour, minute, second);
		}

		private int ConvertFromBitArrayToInt(IEnumerable<bool> source)
		{
			var target = new int[1];
			new BitArray(source.ToArray()).CopyTo(target, 0);
			return target[0];
		}

		#endregion


		#region Thumbnail

		private static readonly BitmapImage defaultThumbnail =
			ImageManager.ConvertFrameworkElementToBitmapImage(new ThumbnailBox());

		public BitmapImage Thumbnail
		{
			get { return _thumbnail ?? defaultThumbnail; }
			set
			{
				_thumbnail = value;
				RaisePropertyChanged();
			}
		}
		private BitmapImage _thumbnail = null;

		public bool HasThumbnail
		{
			get { return _thumbnail != null; }
		}

		#endregion


		#region Added information

		internal bool IsImported { get; private set; }

		internal string FilePath
		{
			get { return String.Format("{0}/{1}", Directory, FileName); }
		}

		internal FileExtension FileExtension { get; private set; }

		internal bool IsImageFile
		{
			get { return (FileExtension != FileExtension.other); }
		}

		/// <summary>
		/// Whether can read Exif metadata when in local.
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
		/// Whether can get a thumbnail by FlashAir card
		/// </summary>
		/// <remarks>FlashAir card can provide a thumbnail only from JPEG image.</remarks>
		internal bool CanGetThumbnailRemote
		{
			get
			{
				switch (FileExtension)
				{
					case FileExtension.jpg:
					case FileExtension.jpeg:
						return true;
					default:
						return false;
				}
			}
		}

		/// <summary>
		/// Whether can load image data when in local.
		/// </summary>
		/// <remarks>As for raw images, actual loadability depends on Microsoft Camera Codec Pack.</remarks>
		internal bool CanLoadLocal
		{
			get
			{
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
		}

		private static readonly string[] flashAirSystemFolders =
			new string[]
			{
				"GUPIXINF",
				"SD_WLAN",
				"100__TSB",
			};

		internal bool IsFlashAirSystemFolder
		{
			get { return flashAirSystemFolders.Contains(FileName, StringComparer.OrdinalIgnoreCase); }
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

		public bool IsRemoteAlive { get; set; }
		public bool IsLocalAlive { get; set; }

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

		public DateTime FileCopiedTime { get; set; }

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
				resourcesPropertyChangedListener = new PropertyChangedEventListener(ReactResourcesPropertyChanged);
				PropertyChangedEventManager.AddListener(ResourceService.Current, resourcesPropertyChangedListener, "Resources");
			}
		}

		#endregion
		

		#region IComparable member

		public int CompareTo(FileItemViewModel other)
		{
			if (other == null)
				return 1;

			int comparisonDate = this.Date.CompareTo(other.Date);
			int comparisonFilePath = this.FilePath.CompareTo(other.FilePath);

			return (comparisonDate != 0) ? comparisonDate : comparisonFilePath;
		}

		#endregion


		#region Event listener

		private PropertyChangedEventListener resourcesPropertyChangedListener;

		private void ReactResourcesPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Resources property changed: {0} {1}", sender, e.PropertyName);

			RaisePropertyChanged("Status");
		}

		#endregion


		#region Import

		private const char separator = ','; // Separator character (comma)
		private static readonly Regex asciiPattern = new Regex(@"^[\x20-\x7F]+$", RegexOptions.Compiled); // Pattern for ASCII code (alphanumeric symbols)

		/// <summary>
		/// Import file information from a list of files in FlashAir card.
		/// </summary>
		/// <param name="source">Source string in the list</param>
		/// <param name="remoteDirectoryPath">Remote directory path used to get the list</param>
		internal void Import(string source, string remoteDirectoryPath)
		{
			if (String.IsNullOrEmpty(source))
				return;

			var sourceWithoutDirectory = source.Trim();

			if (!String.IsNullOrEmpty(remoteDirectoryPath))
			{
				// Check if source string has enough length (typically in the case of WLANSD_FILELIST).
				if (source.Length <= remoteDirectoryPath.Length)
					return;

				if (!source.Substring(0, remoteDirectoryPath.Length).Equals(remoteDirectoryPath, StringComparison.OrdinalIgnoreCase))
					return;

				Directory = remoteDirectoryPath;

				// Check if directory path is valid
				if (!asciiPattern.IsMatch(Directory) || // Directory path must be ASCII characters only (If byte array is decoded by ASCII, this part is non-sense).
					Path.GetInvalidPathChars().Concat(new Char[] { '?' }).Any(x => Directory.Contains(x))) // '?' appears typically when byte array was not correctly decoded.
					return;

				sourceWithoutDirectory = source.Substring(remoteDirectoryPath.Length).TrimStart();
			}
			else
			{
				Directory = String.Empty;
			}

			if (!sourceWithoutDirectory.ElementAt(0).Equals(separator))
				return;

			var elements = sourceWithoutDirectory.Substring(1) // 1 means length of separator.
				.Split(new Char[] { separator }, StringSplitOptions.None)
				.ToList();

			if (elements.Count < 5) // 5 means file name, size, raw attribute, raw data and raw time 
				return;

			while (5 < elements.Count) // In the case that file name includes separator character
			{
				elements[0] = String.Format("{0}{1}{2}", elements[0], separator, elements[1]);
				elements.RemoveAt(1);
			}

			FileName = elements[0].Trim();

			// Check if file name is valid
			if (String.IsNullOrWhiteSpace(FileName) || // File name must not be empty.
				!asciiPattern.IsMatch(FileName) || // File name must be ASCII characters only (If byte array is decoded by ASCII, this part is non-sense).
				Path.GetInvalidFileNameChars().Any(x => FileName.Contains(x)))
				return;

			// Determine size, attribute and date.
			for (int i = 1; i <= 4; i++)
			{
				int num;
				if (int.TryParse(elements[i], out num))
				{
					switch (i)
					{
						case 1:
							Size = num;
							break;
						case 2:
							RawAttribute = num;
							break;
						case 3:
							RawDate = num;
							break;
						case 4:
							RawTime = num;
							break;
					}
				}
				else
				{
					return;
				}
			}

			// Determine file extension.
			if ((0 < Size) && !IsDirectory && !IsVolume)
			{
				FileExtension = Enum.GetValues(typeof(FileExtension))
					.Cast<FileExtension>()
					.FirstOrDefault(x => Path.GetExtension(FileName).Equals(String.Format(".{0}", x), StringComparison.OrdinalIgnoreCase));
			}

			IsImported = true;
		}

		#endregion
	}
}

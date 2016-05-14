using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
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

		public string Directory { get { return _fileItem.Directory; } }
		public string FileName { get { return _fileItem.FileName; } }
		public int Size { get { return _fileItem.Size; } } // In bytes

		public bool IsReadOnly { get { return _fileItem.IsReadOnly; } }
		public bool IsHidden { get { return _fileItem.IsHidden; } }
		public bool IsSystemFile { get { return _fileItem.IsSystemFile; } }
		public bool IsVolume { get { return _fileItem.IsVolume; } }
		public bool IsDirectory { get { return _fileItem.IsDirectory; } }
		public bool IsArchive { get { return _fileItem.IsArchive; } }

		public DateTime Date { get { return _fileItem.Date; } }

		#endregion

		#region Supplementary

		internal string FilePath { get { return _fileItem.FilePath; } }
		internal string Signature { get { return _fileItem.Signature; } }

		internal string FileNameWithCaseExtension
		{
			get
			{
				if (!Settings.Current.MakesFileExtensionLowercase)
					return this.FileName;

				var extension = Path.GetExtension(this.FileName);
				if (String.IsNullOrEmpty(extension))
					return this.FileName;

				return Path.GetFileNameWithoutExtension(this.FileName) + extension.ToLower();
			}
		}

		/// <summary>
		/// Whether can read Exif metadata
		/// </summary>
		internal bool CanReadExif
		{
			get { return _fileItem.IsJpeg || _fileItem.IsTiff; }
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

				return _fileItem.IsJpeg;
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

				return _fileItem.IsLoadable;
			}
			set { _canLoadDataLocal = value; }
		}
		private bool? _canLoadDataLocal;

		public override string ToString()
		{
			return this.FileName; // For the case where being called for binding
		}

		#endregion

		#region Thumbnail

		private static readonly BitmapImage _defaultThumbnail =
			ImageManager.ConvertFrameworkElementToBitmapImage(new ThumbnailBox());

		public BitmapSource Thumbnail
		{
			get { return _thumbnail ?? _defaultThumbnail; }
			set
			{
				_thumbnail = value;
				RaisePropertyChanged();
			}
		}
		private BitmapSource _thumbnail;

		public bool HasThumbnail
		{
			get { return _thumbnail != null; }
		}

		#endregion

		#region Operation

		public bool IsTarget
		{
			get
			{
				if (Settings.Current.HandlesJpegFileOnly && !_fileItem.IsJpeg)
					return false;

				switch (Settings.Current.TargetPeriod)
				{
					case FilePeriod.Today:
						return (this.Date.Date == DateTime.Today);

					case FilePeriod.Select:
						return Settings.Current.TargetDates.Contains(this.Date.Date);

					default: // FilePeriod.All
						return true;
				}
			}
		}

		public bool IsDescendant
		{
			get { return this.Directory.StartsWith(Settings.Current.RemoteDescendant, StringComparison.OrdinalIgnoreCase); }
		}

		public bool IsAliveRemote { get; set; }
		public bool IsAliveLocal { get; set; }
		public bool IsAvailableLocal { get; set; }

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

		internal IFileItem FileItem
		{
			get { return _fileItem; }
			set { if (value != null) { _fileItem = value; } }
		}
		private IFileItem _fileItem;

		internal FileItemViewModel()
			: this(String.Empty, String.Empty)
		{ }

		internal FileItemViewModel(string fileEntry, string directoryPath)
			: this(new FileItem(fileEntry, directoryPath))
		{ }

		internal FileItemViewModel(IFileItem fileItem)
		{
			if (fileItem == null)
				throw new ArgumentNullException("fileItem");

			_fileItem = fileItem;

			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_resourcesPropertyChangedListener = new PropertyChangedEventListener(ReactResourcesPropertyChanged);
				PropertyChangedEventManager.AddListener(ResourceService.Current, _resourcesPropertyChangedListener, "Resources");
			}
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

		#region IComparable member

		public int CompareTo(FileItemViewModel other)
		{
			if (other == null)
				return 1;

			return this.FileItem.CompareTo(other.FileItem);
		}

		#endregion
	}
}
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

		public string Directory => _fileItem.Directory;
		public string FileName => _fileItem.FileName;
		public int Size => _fileItem.Size; // In bytes

		public bool IsReadOnly => _fileItem.IsReadOnly;
		public bool IsHidden => _fileItem.IsHidden;
		public bool IsSystemFile => _fileItem.IsSystemFile;
		public bool IsVolume => _fileItem.IsVolume;
		public bool IsDirectory => _fileItem.IsDirectory;
		public bool IsArchive => _fileItem.IsArchive;

		public DateTime Date => _fileItem.Date;

		#endregion

		#region Supplementary

		internal string FilePath => _fileItem.FilePath;
		internal string Signature => _fileItem.Signature;

		internal string FileNameWithCaseExtension
		{
			get
			{
				if (!Settings.Current.MakesFileExtensionLowercase)
					return this.FileName;

				var extension = Path.GetExtension(this.FileName);
				if (string.IsNullOrEmpty(extension))
					return this.FileName;

				return Path.GetFileNameWithoutExtension(this.FileName) + extension.ToLower();
			}
		}

		/// <summary>
		/// Whether can read Exif metadata
		/// </summary>
		internal bool CanReadExif => _fileItem.IsJpeg || _fileItem.IsTiff;

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

		public override string ToString() => this.FileName; // For the case when being called for binding

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

		public bool HasThumbnail => (_thumbnail != null);

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

		public bool IsDescendant =>
			this.Directory.StartsWith(Settings.Current.RemoteDescendant, StringComparison.OrdinalIgnoreCase);

		public bool IsAliveRemote { get; set; }
		public bool IsAliveLocal { get; set; }
		public bool IsAvailableLocal { get; set; }

		public FileStatus Status
		{
			get { return _status; }
			set { SetPropertyValue(ref _status, value); }
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
			: this(string.Empty, string.Empty)
		{ }

		internal FileItemViewModel(string fileEntry, string directoryPath)
			: this(new FileItem(fileEntry, directoryPath))
		{ }

		internal FileItemViewModel(IFileItem fileItem)
		{
			if (fileItem == null)
				throw new ArgumentNullException(nameof(fileItem));

			this._fileItem = fileItem;

			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_resourcesPropertyChangedListener = new PropertyChangedEventListener(ReactResourcesPropertyChanged);
				PropertyChangedEventManager.AddListener(ResourceService.Current, _resourcesPropertyChangedListener, nameof(ResourceService.Resources));
			}
		}

		#endregion

		#region Event listener

		private PropertyChangedEventListener _resourcesPropertyChangedListener;

		private void ReactResourcesPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			RaisePropertyChanged(nameof(Status));
		}

		#endregion

		#region IComparable member

		public int CompareTo(FileItemViewModel other) => _fileItem.CompareTo(other?.FileItem);

		#endregion
	}
}
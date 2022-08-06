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
using SnowyImageCopy.Lexicon;
using SnowyImageCopy.Models;
using SnowyImageCopy.Models.ImageFile;
using SnowyImageCopy.Views.Controls;

namespace SnowyImageCopy.ViewModels
{
	public class FileItemViewModel : NotificationObject, IComparable<FileItemViewModel>
	{
		#region Basic

		public string Directory => _fileItem.Directory;
		public string FileName => _fileItem.FileName;
		public string FileExtension => _fileItem.FileExtension;
		public int Size => _fileItem.Size; // In bytes

		public bool IsReadOnly => _fileItem.IsReadOnly;
		public bool IsHidden => _fileItem.IsHidden;
		public bool IsSystem => _fileItem.IsSystem;
		public bool IsVolume => _fileItem.IsVolume;
		public bool IsDirectory => _fileItem.IsDirectory;
		public bool IsArchive => _fileItem.IsArchive;

		public DateTime Date => _fileItem.Date;

		#endregion

		#region Supplementary

		internal string FilePath => _fileItem.FilePath;
		internal HashItem Signature => _fileItem.Signature;

		internal ushort FileIndex { get; set; } = 0;

		private string _fileNameWithoutExtension;

		internal string FileNameWithCaseExtension
		{
			get
			{
				if (!_settings.MakesFileExtensionLowercase &&
					!_settings.LeavesExistingFile)
					return FileName;

				_fileNameWithoutExtension ??= Path.GetFileNameWithoutExtension(FileName);
				var buffer = new StringBuilder(_fileNameWithoutExtension);

				if (_settings.LeavesExistingFile && (0 < FileIndex))
					buffer.AppendFormat(" ({0})", FileIndex);

				if (!string.IsNullOrEmpty(FileExtension))
				{
					if (_settings.MakesFileExtensionLowercase)
						buffer.Append(FileExtension.ToLower());
					else
						buffer.Append(FileExtension);
				}

				return buffer.ToString();
			}
		}

		internal bool IsImageFile => _fileItem.IsImageFile;
		internal bool IsVideoFile => _fileItem.IsVideoFile;
		internal bool IsImageOrVideoFile => IsImageFile || IsVideoFile;

		/// <summary>
		/// Whether can read Exif metadata
		/// </summary>
		internal bool CanReadExif => _fileItem.IsJpeg || _fileItem.IsTiff;

		public override string ToString() => FileName; // For the case when being called for binding

		#endregion

		#region Thumbnail

		private static readonly BitmapImage _defaultThumbnail =
			ImageManager.ConvertFrameworkElementToBitmapImage(new ThumbnailBox());

		public BitmapSource Thumbnail
		{
			get => _thumbnail ?? _defaultThumbnail;
			set
			{
				_thumbnail = value;
				OnPropertyChanged();
			}
		}
		private BitmapSource _thumbnail;

		public bool HasThumbnail => (_thumbnail is not null);

		#endregion

		#region Operation

		public bool IsTarget
		{
			get
			{
				bool IsTargetFile()
				{
					if (_settings.LimitsFileExtensions)
						return _settings.FileExtensions.Contains(FileExtension.ToLower());

					if (IsImageFile)
						return true;

					if (_settings.HandlesVideoFile && IsVideoFile)
						return true;

					return false;
				}

				bool IsTargetDate(DateTime date)
				{
					return _settings.TargetPeriod switch
					{
						FilePeriod.All => true,
						FilePeriod.Today => (date.Date == DateTime.Today),
						FilePeriod.Recent => (date.Date >= (DateTime.Today - _settings.TargetBackLength)),
						FilePeriod.Select => _settings.TargetDates.Contains(date.Date),
						_ => throw new InvalidOperationException()
					};
				}

				return IsTargetFile()
					&& IsTargetDate(Date);
			}
		}

		public bool IsDescendant => Directory.StartsWith(_settings.RemoteDescendant, StringComparison.OrdinalIgnoreCase);

		public bool IsAliveRemote { get; set; }
		public bool IsAliveLocal { get; set; }
		public bool IsAvailableLocal { get; set; }
		public bool? IsOnceCopied { get; set; } = null;

		internal FileStatus ResolveStatus()
		{
			FileStatus Resolve()
			{
				if (IsAliveLocal)
					return FileStatus.Copied;

				if (IsOnceCopied is true)
					return FileStatus.OnceCopied;

				return FileStatus.NotCopied;
			}

			return Status = Resolve();
		}

		public FileStatus Status
		{
			get => _status;
			set => SetProperty(ref _status, value);
		}
		private FileStatus _status = FileStatus.Unknown;

		/// <summary>
		/// Whether can get a thumbnail of a remote file
		/// </summary>
		/// <remarks>FlashAir card can provide a thumbnail only from JPEG format file.</remarks>
		internal bool CanGetThumbnailRemote
		{
			get => (_canGetThumbnailRemote is not false) && _fileItem.IsJpeg && IsAliveRemote;
			set => _canGetThumbnailRemote = value;
		}
		private bool? _canGetThumbnailRemote;

		/// <summary>
		/// Whether can load image data from a local file
		/// </summary>
		/// <remarks>
		/// As for raw images, whether an image is actually loadable depends on Microsoft Camera Codec Pack.
		/// </remarks>
		internal bool CanLoadDataLocal
		{
			get => (_canLoadDataLocal is not false) && IsImageOrVideoFile && IsAliveLocal && IsAvailableLocal;
			set => _canLoadDataLocal = value;
		}
		private bool? _canLoadDataLocal;

		public bool IsSelected
		{
			get => false; // Always false
			set
			{
				if (!value)
					return;

				OnPropertyChanged();
			}
		}

		public DateTime CopiedTime { get; set; }

		#endregion

		#region Constructor

		private readonly Settings _settings;

		internal IFileItem FileItem
		{
			get => _fileItem;
			set { if (value is not null) { _fileItem = value; } }
		}
		private IFileItem _fileItem;

		internal FileItemViewModel(Settings settings, string fileEntry, string directoryPath) : this(settings, new FileItem(fileEntry, directoryPath))
		{ }

		internal FileItemViewModel(Settings settings, IFileItem fileItem)
		{
			this._settings = settings ?? throw new ArgumentNullException(nameof(settings));
			this._fileItem = fileItem ?? throw new ArgumentNullException(nameof(fileItem));

			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_resourcesPropertyChangedListener = new PropertyChangedEventListener(OnResourcesPropertyChanged);
				PropertyChangedEventManager.AddListener(ResourceService.Current, _resourcesPropertyChangedListener, nameof(ResourceService.Resources));
			}
		}

		#endregion

		#region Event listener

		private readonly PropertyChangedEventListener _resourcesPropertyChangedListener;

		private void OnResourcesPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			OnPropertyChanged(nameof(Status));
		}

		#endregion

		#region IComparable

		public int CompareTo(FileItemViewModel other) => _fileItem.CompareTo(other?.FileItem);

		public override bool Equals(object obj) => _fileItem.Equals((obj as FileItemViewModel)?.FileItem);

		public virtual bool Equals(FileItemViewModel other) => _fileItem.Equals(other?.FileItem);

		public override int GetHashCode() => _fileItem.GetHashCode();

		#endregion
	}
}
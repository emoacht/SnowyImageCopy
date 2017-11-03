using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Models;
using SnowyImageCopy.Models.Exceptions;
using SnowyImageCopy.Views.Controls;

namespace SnowyImageCopy.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		#region Interaction

		public string OperationStatus
		{
			get { return _operationStatus; }
			set { SetPropertyValue(ref _operationStatus, value); }
		}
		private string _operationStatus;

		public Settings SettingsCurrent => Settings.Current;

		public event EventHandler ActivateRequested;

		#endregion

		#region Operation

		public Operator Op { get; }

		public ItemObservableCollection<FileItemViewModel> FileListCore
		{
			get { return _fileListCore ?? (_fileListCore = new ItemObservableCollection<FileItemViewModel>()); }
		}
		private ItemObservableCollection<FileItemViewModel> _fileListCore;

		public ListCollectionView FileListCoreView
		{
			get
			{
				return _fileListCoreView ?? (_fileListCoreView =
					new ListCollectionView(FileListCore) { Filter = item => ((FileItemViewModel)item).IsTarget });
			}
		}
		private ListCollectionView _fileListCoreView;

		public int FileListCoreViewIndex
		{
			get { return _fileListCoreViewIndex; }
			set { SetPropertyValue(ref _fileListCoreViewIndex, value); }
		}
		private int _fileListCoreViewIndex = -1; // No selection

		public bool IsCheckAndCopyOngoing => Op.IsChecking && Op.IsCopying;
		public bool IsCheckAndCopyAutoOngoing => Op.IsAutoRunning;
		public bool IsCheckOrCopyOngoing => Op.IsChecking || Op.IsCopying;
		public bool IsCheckOngoing => Op.IsChecking && !Op.IsCopying;
		public bool IsCopyOngoing => !Op.IsChecking && Op.IsCopying;

		private bool IsNoOngoing => !Op.IsChecking && !Op.IsCopying && !Op.IsAutoRunning;
		private bool IsAnyOngoing => Op.IsChecking || Op.IsCopying || Op.IsAutoRunning;

		private void ManageOperationState()
		{
			RaisePropertyChanged(nameof(IsCheckAndCopyOngoing));
			RaisePropertyChanged(nameof(IsCheckAndCopyAutoOngoing));
			RaisePropertyChanged(nameof(IsCheckOrCopyOngoing));
			RaisePropertyChanged(nameof(IsCheckOngoing));
			RaisePropertyChanged(nameof(IsCopyOngoing));

			RaiseCanExecuteChanged();
		}

		#endregion

		#region Command

		#region Check & Copy Command

		public DelegateCommand CheckAndCopyCommand
		{
			get { return _checkAndCopyCommand ?? (_checkAndCopyCommand = new DelegateCommand(CheckAndCopyExecute, CanCheckAndCopyExecute)); }
		}
		private DelegateCommand _checkAndCopyCommand;

		private async void CheckAndCopyExecute() => await Op.CheckCopyFileAsync();
		private bool CanCheckAndCopyExecute() => IsNoOngoing;

		#endregion

		#region Check & Copy Auto Command

		public DelegateCommand CheckAndCopyAutoCommand
		{
			get { return _checkAndCopyAutoCommand ?? (_checkAndCopyAutoCommand = new DelegateCommand(CheckAndCopyAutoExecute, CanCheckAndCopyAutoExecute)); }
		}
		private DelegateCommand _checkAndCopyAutoCommand;

		private void CheckAndCopyAutoExecute() => Op.StartAutoTimer();
		private bool CanCheckAndCopyAutoExecute() => IsNoOngoing;

		#endregion

		#region Check Command

		public DelegateCommand CheckCommand
		{
			get { return _checkFileCommand ?? (_checkFileCommand = new DelegateCommand(CheckExecute, CanCheckExecute)); }
		}
		private DelegateCommand _checkFileCommand;

		private async void CheckExecute() => await Op.CheckFileAsync();
		private bool CanCheckExecute() => IsNoOngoing;

		#endregion

		#region Copy Command

		public DelegateCommand CopyCommand
		{
			get { return _copyCommand ?? (_copyCommand = new DelegateCommand(CopyExecute, CanCopyExecute)); }
		}
		private DelegateCommand _copyCommand;

		private async void CopyExecute() => await Op.CopyFileAsync();
		private bool CanCopyExecute() => IsNoOngoing;

		#endregion

		#region Stop Command

		public DelegateCommand StopCommand
		{
			get { return _stopCommand ?? (_stopCommand = new DelegateCommand(StopExecute, CanStopExecute)); }
		}
		private DelegateCommand _stopCommand;

		private void StopExecute() => Op.Stop();
		private bool CanStopExecute() => IsAnyOngoing;

		#endregion

		#region Save Desktop Command

		public DelegateCommand SaveDesktopCommand
		{
			get { return _saveDesktopCommand ?? (_saveDesktopCommand = new DelegateCommand(SaveDesktopExecute, CanSaveDesktopExecute)); }
		}
		private DelegateCommand _saveDesktopCommand;

		private async void SaveDesktopExecute() => await Op.SaveDesktopAsync();
		private bool CanSaveDesktopExecute() => (CurrentImageData != null) && !Op.IsSavingDesktop;

		#endregion

		#region Send Clipboard Command

		public DelegateCommand SendClipboardCommand
		{
			get { return _sendClipboardCommand ?? (_sendClipboardCommand = new DelegateCommand(SendClipboardExecute, CanSendClipboardExecute)); }
		}
		private DelegateCommand _sendClipboardCommand;

		private async void SendClipboardExecute() => await Op.SendClipboardAsync();
		private bool CanSendClipboardExecute() => (CurrentImageData != null) && !Op.IsSendingClipboard;

		#endregion

		private void RaiseCanExecuteChanged() => DelegateCommand.RaiseCanExecuteChanged();

		#endregion

		#region Current image

		public bool IsCurrentImageVisible
		{
			get { return Settings.Current.IsCurrentImageVisible; }
			set
			{
				Settings.Current.IsCurrentImageVisible = value;
				RaisePropertyChanged();

				if (!Designer.IsInDesignMode)
					SetCurrentImage();
			}
		}

		public double CurrentImageWidth
		{
			get { return Settings.Current.CurrentImageWidth; }
			set
			{
				Settings.Current.CurrentImageWidth = value;
				RaisePropertyChanged();
			}
		}

		public Size CurrentFrameSize
		{
			get { return _currentFrameSize; }
			set
			{
				if (_currentFrameSize == value) // This check is necessary to prevent resizing loop.
					return;

				_currentFrameSize = value;
				_currentFrameSizeChanged?.Invoke();
			}
		}
		private Size _currentFrameSize = Size.Empty;

		private event Action _currentFrameSizeChanged;

		public FileItemViewModel CurrentItem { get; set; }

		private readonly ReaderWriterLockSlim _dataLocker = new ReaderWriterLockSlim();
		private bool _isCurrentImageDataGiven;

		public byte[] CurrentImageData
		{
			get
			{
				_dataLocker.EnterReadLock();
				try
				{
					return _currentImageData;
				}
				finally
				{
					_dataLocker.ExitReadLock();
				}
			}
			set
			{
				_dataLocker.EnterWriteLock();
				try
				{
					_currentImageData = value;
				}
				finally
				{
					_dataLocker.ExitWriteLock();
				}

				if (!Designer.IsInDesignMode)
					SetCurrentImage();

				if (!_isCurrentImageDataGiven &&
					(_isCurrentImageDataGiven = (value != null)))
					RaiseCanExecuteChanged();
			}
		}
		private byte[] _currentImageData;

		public BitmapSource CurrentImage
		{
			get { return _currentImage ?? (_currentImage = GetDefaultCurrentImage()); }
			set
			{
				_currentImage = value;
				if (_currentImage != null)
				{
					// Width and PixelWidth of BitmapImage are almost the same except fractional part
					// while those of BitmapSource are not always close and can be much different.
					CurrentImageWidth = Math.Round(_currentImage.Width);
				}

				RaisePropertyChanged();
			}
		}
		private BitmapSource _currentImage;

		/// <summary>
		/// Sets current image.
		/// </summary>
		/// <remarks>In Design mode, this method causes NullReferenceException.</remarks>
		private async void SetCurrentImage()
		{
			if (!IsCurrentImageVisible)
			{
				CurrentImage = null;
				return;
			}

			BitmapSource image = null;

			if ((CurrentImageData != null) && (CurrentItem != null))
			{
				try
				{
					image = !CurrentFrameSize.IsEmpty
						? await ImageManager.ConvertBytesToBitmapSourceUniformAsync(CurrentImageData, CurrentFrameSize, CurrentItem.CanReadExif, DestinationColorProfile)
						: await ImageManager.ConvertBytesToBitmapSourceAsync(CurrentImageData, CurrentImageWidth, CurrentItem.CanReadExif, DestinationColorProfile);
				}
				catch (ImageNotSupportedException)
				{
					CurrentItem.CanLoadDataLocal = false;
				}
			}

			if (image == null)
				image = GetDefaultCurrentImage();

			CurrentImage = image;
		}

		private BitmapImage GetDefaultCurrentImage()
		{
			return !CurrentFrameSize.IsEmpty
				? ImageManager.ConvertFrameworkElementToBitmapImage(new ThumbnailBox(), CurrentFrameSize)
				: ImageManager.ConvertFrameworkElementToBitmapImage(new ThumbnailBox(), CurrentImageWidth);
		}

		public ColorContext DestinationColorProfile
		{
			get { return _destinationColorProfile ?? new ColorContext(PixelFormats.Bgra32); }
			set
			{
				_destinationColorProfile = value;

				if (value != null)
					SetCurrentImage();
			}
		}
		private ColorContext _destinationColorProfile;

		#endregion

		#region Browser

		public bool IsBrowserOpen
		{
			get { return _isBrowserOpen; }
			set
			{
				SetPropertyValue(ref _isBrowserOpen, value);

				if (value)
					Op.Stop();
			}
		}
		private bool _isBrowserOpen;

		private void ManageBrowserOpen(bool isOngoing)
		{
			if (isOngoing)
				IsBrowserOpen = false;
		}

		#endregion

		#region Constructor

		public MainWindowViewModel()
		{
			Op = new Operator(this);

			// Add event listeners.
			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_fileListPropertyChangedListener = new PropertyChangedEventListener(FileListPropertyChanged);
				PropertyChangedEventManager.AddListener(FileListCore, _fileListPropertyChangedListener, string.Empty);

				_settingsPropertyChangedListener = new PropertyChangedEventListener(ReactSettingsPropertyChanged);
				PropertyChangedEventManager.AddListener(Settings.Current, _settingsPropertyChangedListener, string.Empty);

				_operationPropertyChangedListener = new PropertyChangedEventListener(ReactOperationPropertyChanged);
				PropertyChangedEventManager.AddListener(Op, _operationPropertyChangedListener, string.Empty);
			}

			// Subscribe event handlers.
			Subscription.Add(Observable.FromEvent
			  (
				  handler => _currentFrameSizeChanged += handler,
				  handler => _currentFrameSizeChanged -= handler
			  )
			  .Throttle(TimeSpan.FromMilliseconds(50))
			  .ObserveOn(SynchronizationContext.Current)
			  .Subscribe(_ => SetCurrentImage()));

			Subscription.Add(Observable.FromEvent
				(
					handler => _autoCheckIntervalChanged += handler,
					handler => _autoCheckIntervalChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(200))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => Op.ResetAutoTimer()));

			Subscription.Add(Observable.FromEvent
				(
					handler => _targetConditionChanged += handler,
					handler => _targetConditionChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(200))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ =>
				{
					FileListCoreView.Refresh();
					Op.UpdateProgress();
				}));

			Subscription.Add(Observable.FromEventPattern
				(
					handler => Op.ActivateRequested += handler,
					handler => Op.ActivateRequested -= handler
				)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => ActivateRequested?.Invoke(this, EventArgs.Empty)));

			SetSample(1);
		}

		private void SetSample(int number = 1)
		{
			Enumerable.Range(0, number)
				.Select(x => (1 < number) ? x.ToString(CultureInfo.InvariantCulture) : string.Empty)
				.Select(x => new FileItemViewModel($"/DCIM,SAMPLE{x}.JPG,0,0,0,0", "/DCIM"))
				.ToList()
				.ForEach(x => FileListCore.Insert(x));
		}

		#endregion

		#region Event Listener

		#region FileItem

		private PropertyChangedEventListener _fileListPropertyChangedListener;

		private async void FileListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine($"File List property changed: {sender} {e.PropertyName}");

			if (e.PropertyName != nameof(ItemObservableCollection<FileItemViewModel>.ItemPropertyChangedSender))
				return;

			var item = ((ItemObservableCollection<FileItemViewModel>)sender).ItemPropertyChangedSender;
			var propertyName = ((ItemObservableCollection<FileItemViewModel>)sender).ItemPropertyChangedEventArgs.PropertyName;

			//Debug.WriteLine($"ItemPropertyChanged: {item.FileName} {propertyName}");

			if (propertyName == nameof(FileItemViewModel.IsSelected))
			{
				switch (item.Status)
				{
					case FileStatus.NotCopied:
						// Make remote file as to be copied.
						if (!item.IsAliveRemote)
							break;

						item.Status = FileStatus.ToBeCopied;
						break;

					case FileStatus.ToBeCopied:
						// Make remote file as not to be copied.
						item.Status = item.IsAliveLocal ? FileStatus.Copied : FileStatus.NotCopied;
						break;

					case FileStatus.Copied:
						// Load image data from local file.
						if (!IsCurrentImageVisible || Op.IsCopying)
							break;

						await Op.LoadSetAsync(item);
						break;
				}
			}
			else if (propertyName == nameof(FileItemViewModel.Status))
			{
				switch (item.Status)
				{
					case FileStatus.ToBeCopied:
						// Trigger instant copy.
						if (!Settings.Current.InstantCopy || Op.IsChecking || Op.IsCopying)
							break;

						await Op.CopyFileAsync();
						break;
				}
			}
		}

		#endregion

		#region Settings

		private PropertyChangedEventListener _settingsPropertyChangedListener;

		private event Action _autoCheckIntervalChanged;
		private event Action _targetConditionChanged;

		private void ReactSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine($"Settings property changed: {sender} {e.PropertyName}");

			var propertyName = e.PropertyName;

			if (propertyName == nameof(Settings.AutoCheckInterval))
			{
				_autoCheckIntervalChanged?.Invoke();
			}
			else if ((propertyName == nameof(Settings.TargetPeriod)) || (propertyName == nameof(Settings.TargetDates))
				|| (propertyName == nameof(Settings.HandlesJpegFileOnly)))
			{
				_targetConditionChanged?.Invoke();
			}
		}

		#endregion

		#region Operation

		private PropertyChangedEventListener _operationPropertyChangedListener;

		private void ReactOperationPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine($"Operation property changed (MainWindowViewModel): {sender} {e.PropertyName}");

			var propertyName = e.PropertyName;

			if (propertyName == nameof(Operator.IsChecking))
			{
				ManageOperationState();
				ManageBrowserOpen(Op.IsChecking);
			}
			else if (propertyName == nameof(Operator.IsCopying))
			{
				ManageOperationState();
				ManageBrowserOpen(Op.IsCopying);
			}
			else if (propertyName == nameof(Operator.IsAutoRunning))
			{
				ManageOperationState();
				ManageBrowserOpen(Op.IsAutoRunning);
			}
			else if ((propertyName == nameof(Operator.IsSavingDesktop)) || (propertyName == nameof(Operator.IsSendingClipboard)))
			{
				ManageOperationState();
			}
		}

		#endregion

		#endregion
	}
}
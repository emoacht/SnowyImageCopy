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
	public class MainWindowViewModel : NotificationDisposableObject
	{
		#region Interaction

		public string OperationStatus
		{
			get => _operationStatus;
			set => SetPropertyValue(ref _operationStatus, value);
		}
		private string _operationStatus;

		public Settings SettingsCurrent => Settings.Current;

		public event EventHandler ActivateRequested;

		#endregion

		#region Operation

		public Operator Op { get; }

		public ItemObservableCollection<FileItemViewModel> FileListCore { get; } = new ItemObservableCollection<FileItemViewModel>();

		public ListCollectionView FileListCoreView =>
			_fileListCoreView ??= new ListCollectionView(FileListCore) { Filter = item => ((FileItemViewModel)item).IsTarget };
		private ListCollectionView _fileListCoreView;

		public int FileListCoreViewIndex
		{
			get => _fileListCoreViewIndex;
			set => SetPropertyValue(ref _fileListCoreViewIndex, value);
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

		public DelegateCommand CheckAndCopyCommand =>
			_checkAndCopyCommand ??= new DelegateCommand(CheckAndCopyExecute, CanCheckAndCopyExecute);
		private DelegateCommand _checkAndCopyCommand;

		private async void CheckAndCopyExecute() => await Op.CheckCopyFileAsync();
		private bool CanCheckAndCopyExecute() => IsNoOngoing;

		#endregion

		#region Check & Copy Auto Command

		public DelegateCommand CheckAndCopyAutoCommand =>
			_checkAndCopyAutoCommand ??= new DelegateCommand(CheckAndCopyAutoExecute, CanCheckAndCopyAutoExecute);
		private DelegateCommand _checkAndCopyAutoCommand;

		private void CheckAndCopyAutoExecute() => Op.StartAutoTimer();
		private bool CanCheckAndCopyAutoExecute() => IsNoOngoing;

		#endregion

		#region Check Command

		public DelegateCommand CheckCommand =>
			_checkFileCommand ??= new DelegateCommand(CheckExecute, CanCheckExecute);
		private DelegateCommand _checkFileCommand;

		private async void CheckExecute() => await Op.CheckFileAsync();
		private bool CanCheckExecute() => IsNoOngoing;

		#endregion

		#region Copy Command

		public DelegateCommand CopyCommand =>
			_copyCommand ??= new DelegateCommand(CopyExecute, CanCopyExecute);
		private DelegateCommand _copyCommand;

		private async void CopyExecute() => await Op.CopyFileAsync();
		private bool CanCopyExecute() => IsNoOngoing;

		#endregion

		#region Stop Command

		public DelegateCommand StopCommand =>
			_stopCommand ??= new DelegateCommand(StopExecute, CanStopExecute);
		private DelegateCommand _stopCommand;

		private void StopExecute() => Op.Stop();
		private bool CanStopExecute() => IsAnyOngoing;

		#endregion

		#region Save Desktop Command

		public DelegateCommand SaveDesktopCommand =>
			_saveDesktopCommand ??= new DelegateCommand(SaveDesktopExecute, CanSaveDesktopExecute);
		private DelegateCommand _saveDesktopCommand;

		private async void SaveDesktopExecute() => await Op.SaveDesktopAsync();
		private bool CanSaveDesktopExecute() => IsCurrentImageDataGiven && !Op.IsSavingDesktop;

		#endregion

		#region Send Clipboard Command

		public DelegateCommand SendClipboardCommand =>
			_sendClipboardCommand ??= new DelegateCommand(SendClipboardExecute, CanSendClipboardExecute);
		private DelegateCommand _sendClipboardCommand;

		private async void SendClipboardExecute() => await Op.SendClipboardAsync();
		private bool CanSendClipboardExecute() => IsCurrentImageDataGiven && !Op.IsSendingClipboard;

		#endregion

		private void RaiseCanExecuteChanged() => DelegateCommand.RaiseCanExecuteChanged();

		#endregion

		#region Current image

		public FileItemViewModel CurrentItem { get; set; }

		private readonly ReaderWriterLockSlim _dataLocker = new ReaderWriterLockSlim();

		public byte[] CurrentImageData
		{
			get
			{
				try
				{
					_dataLocker.EnterReadLock();

					return _currentImageData;
				}
				finally
				{
					_dataLocker.ExitReadLock();
				}
			}
			set
			{
				try
				{
					_dataLocker.EnterWriteLock();

					_currentImageData = value;
				}
				finally
				{
					_dataLocker.ExitWriteLock();
				}

				SetCurrentImage(value);
				IsCurrentImageDataGiven = (value?.Length > 0);
			}
		}
		private byte[] _currentImageData;

		private bool IsCurrentImageDataGiven
		{
			get => _isCurrentImageDataGiven;
			set
			{
				if (_isCurrentImageDataGiven == value)
					return;

				_isCurrentImageDataGiven = value;
				RaiseCanExecuteChanged();
			}
		}
		private bool _isCurrentImageDataGiven;

		public BitmapSource CurrentImage
		{
			get => _currentImage ??= GetDefaultCurrentImage();
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
		private async void SetCurrentImage(byte[] data)
		{
			if (Designer.IsInDesignMode) // To avoid NullReferenceException in Design mode.
				return;

			if (!IsCurrentImageVisible)
			{
				CurrentImage = null;
				return;
			}

			BitmapSource image = null;

			if ((data?.Any() == true) && (CurrentItem != null))
			{
				try
				{
					image = !CurrentFrameSize.IsEmpty
						? await ImageManager.ConvertBytesToBitmapSourceUniformAsync(data, CurrentFrameSize, CurrentItem.CanReadExif, DestinationColorProfile)
						: await ImageManager.ConvertBytesToBitmapSourceAsync(data, CurrentImageWidth, CurrentItem.CanReadExif, DestinationColorProfile);
				}
				catch (ImageNotSupportedException)
				{
					CurrentItem.CanLoadDataLocal = false;
				}
			}

			CurrentImage = image ?? GetDefaultCurrentImage();
		}

		private BitmapImage GetDefaultCurrentImage()
		{
			return !CurrentFrameSize.IsEmpty
				? ImageManager.ConvertFrameworkElementToBitmapImage(new ThumbnailBox(), CurrentFrameSize)
				: ImageManager.ConvertFrameworkElementToBitmapImage(new ThumbnailBox(), CurrentImageWidth);
		}

		public bool IsCurrentImageVisible
		{
			get => Settings.Current.IsCurrentImageVisible;
			set
			{
				Settings.Current.IsCurrentImageVisible = value;
				RaisePropertyChanged();
			}
		}

		public double CurrentImageWidth
		{
			get => Settings.Current.CurrentImageWidth;
			set
			{
				Settings.Current.CurrentImageWidth = value;
				RaisePropertyChanged();
			}
		}

		public Size CurrentFrameSize
		{
			get => _currentFrameSize;
			set
			{
				if (_currentFrameSize == value) // This check is necessary to prevent resizing loop.
					return;

				_currentFrameSize = value;
				RaisePropertyChanged();
			}
		}
		private Size _currentFrameSize = Size.Empty;

		public ColorContext DestinationColorProfile
		{
			get => _destinationColorProfile ?? new ColorContext(PixelFormats.Bgra32);
			set
			{
				_destinationColorProfile = value;
				RaisePropertyChanged();
			}
		}
		private ColorContext _destinationColorProfile;

		#endregion

		#region Browser

		public bool IsBrowserOpen
		{
			get => _isBrowserOpen;
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
			Subscription.Add(Op);

			// Add event listeners.
			if (!Designer.IsInDesignMode) // To avoid NullReferenceException in Design mode.
			{
				_fileListPropertyChangedListener = new PropertyChangedEventListener(FileListPropertyChanged);
				PropertyChangedEventManager.AddListener(FileListCore, _fileListPropertyChangedListener, string.Empty);

				_settingsPropertyChangedListener = new PropertyChangedEventListener(ReactSettingsPropertyChanged);
				PropertyChangedEventManager.AddListener(Settings.Current, _settingsPropertyChangedListener, string.Empty);

				_operatorPropertyChangedListener = new PropertyChangedEventListener(ReactOperatorPropertyChanged);
				PropertyChangedEventManager.AddListener(Op, _operatorPropertyChangedListener, string.Empty);
			}

			// Subscribe event handlers.
			Subscription.Add(Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>
				(
					handler => (sender, e) => handler(e),
					handler => this.PropertyChanged += handler,
					handler => this.PropertyChanged -= handler
				)
				.Where(args =>
				{
					var name = args.PropertyName;
					return (name == nameof(IsCurrentImageVisible))
						|| (name == nameof(CurrentFrameSize))
						|| (name == nameof(DestinationColorProfile));
				})
				.Throttle(TimeSpan.FromMilliseconds(50))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => SetCurrentImage(CurrentImageData)));

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

		#region Event listener

		#region FileItem

		private readonly PropertyChangedEventListener _fileListPropertyChangedListener;

		private async void FileListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(ItemObservableCollection<FileItemViewModel>.ItemPropertyChangedSender))
				return;

			var item = ((ItemObservableCollection<FileItemViewModel>)sender).ItemPropertyChangedSender;
			var propertyName = ((ItemObservableCollection<FileItemViewModel>)sender).ItemPropertyChangedEventArgs.PropertyName;

			switch (propertyName)
			{
				case nameof(FileItemViewModel.IsSelected):
					switch (item.Status)
					{
						case FileStatus.NotCopied:
						case FileStatus.OnceCopied:
							// Make remote file as to be copied.
							if (!item.IsAliveRemote)
								break;

							item.Status = FileStatus.ToBeCopied;
							break;

						case FileStatus.ToBeCopied:
							// Make remote file as not to be copied.
							item.Status = item.IsAliveLocal
								? FileStatus.Copied
								: (item.IsOnceCopied == true)
									? FileStatus.OnceCopied
									: FileStatus.NotCopied;
							break;

						case FileStatus.Copied:
							// Load image data from local file.
							if (!IsCurrentImageVisible || Op.IsCopying)
								break;

							await Op.LoadSetAsync(item);
							break;
					}
					break;

				case nameof(FileItemViewModel.Status):
					switch (item.Status)
					{
						case FileStatus.ToBeCopied:
							// Trigger instant copy.
							if (!Settings.Current.InstantCopy || Op.IsChecking || Op.IsCopying)
								break;

							await Op.CopyFileAsync();
							break;
					}
					break;
			}
		}

		#endregion

		#region Settings

		private readonly PropertyChangedEventListener _settingsPropertyChangedListener;

		private event Action _autoCheckIntervalChanged;
		private event Action _targetConditionChanged;

		private void ReactSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(Settings.AutoCheckInterval):
					_autoCheckIntervalChanged?.Invoke();
					break;

				case nameof(Settings.TargetPeriod):
				case nameof(Settings.TargetDates):
				case nameof(Settings.HandlesJpegFileOnly):
					_targetConditionChanged?.Invoke();
					break;

				case nameof(Settings.OrdersFromNewer):
					FileListCore.Clear();
					break;
			}
		}

		#endregion

		#region Operator

		private readonly PropertyChangedEventListener _operatorPropertyChangedListener;

		private void ReactOperatorPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(Operator.IsChecking):
					ManageOperationState();
					ManageBrowserOpen(Op.IsChecking);
					break;

				case nameof(Operator.IsCopying):
					ManageOperationState();
					ManageBrowserOpen(Op.IsCopying);
					break;

				case nameof(Operator.IsAutoRunning):
					ManageOperationState();
					ManageBrowserOpen(Op.IsAutoRunning);
					break;

				case nameof(Operator.IsSavingDesktop):
				case nameof(Operator.IsSendingClipboard):
					ManageOperationState();
					break;
			}
		}

		#endregion

		#endregion
	}
}
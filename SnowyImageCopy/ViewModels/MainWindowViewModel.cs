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
			set
			{
				_operationStatus = value;
				RaisePropertyChanged();
			}
		}
		private string _operationStatus;

		public Settings SettingsCurrent
		{
			get { return Settings.Current; }
		}

		public bool IsWindowActivateRequested
		{
			get { return _isWindowActivateRequested; }
			set
			{
				_isWindowActivateRequested = value;
				RaisePropertyChanged();
			}
		}
		private bool _isWindowActivateRequested;

		#endregion


		#region Operation

		public Operation Op
		{
			get { return _op ?? (_op = new Operation(this)); }
		}
		private Operation _op;

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
			set
			{
				_fileListCoreViewIndex = value;
				RaisePropertyChanged();
			}
		}
		private int _fileListCoreViewIndex = -1; // No selection

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

				var handler = _currentFrameSizeChanged;
				if (handler != null)
					handler();
			}
		}
		private Size _currentFrameSize = Size.Empty;

		private event Action _currentFrameSizeChanged = null;

		public FileItemViewModel CurrentItem { get; set; }

		private ReaderWriterLockSlim _dataLocker = new ReaderWriterLockSlim();
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
		/// Set current image.
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


		#region Command

		#region Check & Copy Command

		public DelegateCommand CheckCopyCommand
		{
			get { return _checkCopyCommand ?? (_checkCopyCommand = new DelegateCommand(CheckCopyExecute, CanCheckCopyExecute)); }
		}
		private DelegateCommand _checkCopyCommand;

		private async void CheckCopyExecute()
		{
			await Op.CheckCopyFileAsync();
		}

		private bool CanCheckCopyExecute()
		{
			IsCheckCopyRunning = Op.IsChecking && Op.IsCopying;

			return !Op.IsChecking && !Op.IsCopying && !Op.IsAutoRunning;
		}

		public bool IsCheckCopyRunning
		{
			get { return _isCheckCopyRunning; }
			set
			{
				_isCheckCopyRunning = value;
				RaisePropertyChanged();
			}
		}
		private bool _isCheckCopyRunning;

		#endregion


		#region Check & Copy Auto Command

		public DelegateCommand CheckCopyAutoCommand
		{
			get { return _checkCopyAutoCommand ?? (_checkCopyAutoCommand = new DelegateCommand(CheckCopyAutoExecute, CanCheckCopyAutoExecute)); }
		}
		private DelegateCommand _checkCopyAutoCommand;

		private void CheckCopyAutoExecute()
		{
			Op.StartAutoTimer();
		}

		private bool CanCheckCopyAutoExecute()
		{
			IsCheckCopyAutoRunning = Op.IsAutoRunning;

			return !Op.IsChecking && !Op.IsCopying && !Op.IsAutoRunning;
		}

		public bool IsCheckCopyAutoRunning
		{
			get { return _isCheckCopyAutoRunning; }
			set
			{
				_isCheckCopyAutoRunning = value;
				RaisePropertyChanged();
			}
		}
		private bool _isCheckCopyAutoRunning;

		#endregion


		#region Check Command

		public DelegateCommand CheckCommand
		{
			get { return _checkFileCommand ?? (_checkFileCommand = new DelegateCommand(CheckExecute, CanCheckExecute)); }
		}
		private DelegateCommand _checkFileCommand;

		private async void CheckExecute()
		{
			await Op.CheckFileAsync();
		}

		private bool CanCheckExecute()
		{
			IsCheckRunning = Op.IsChecking && !Op.IsCopying;

			return !Op.IsChecking && !Op.IsCopying && !Op.IsAutoRunning;
		}

		public bool IsCheckRunning
		{
			get { return _isCheckRunning; }
			set
			{
				_isCheckRunning = value;
				RaisePropertyChanged();
			}
		}
		private bool _isCheckRunning;

		#endregion


		#region Copy Command

		public DelegateCommand CopyCommand
		{
			get { return _copyCommand ?? (_copyCommand = new DelegateCommand(CopyExecute, CanCopyExecute)); }
		}
		private DelegateCommand _copyCommand;

		private async void CopyExecute()
		{
			await Op.CopyFileAsync();
		}

		private bool CanCopyExecute()
		{
			IsCopyRunning = !Op.IsChecking && Op.IsCopying;

			return !Op.IsChecking && !Op.IsCopying && !Op.IsAutoRunning;
		}

		public bool IsCopyRunning
		{
			get { return _isCopyRunning; }
			set
			{
				_isCopyRunning = value;
				RaisePropertyChanged();
			}
		}
		private bool _isCopyRunning;

		#endregion


		#region Stop Command

		public DelegateCommand StopCommand
		{
			get { return _stopCommand ?? (_stopCommand = new DelegateCommand(StopExecute, CanStopExecute)); }
		}
		private DelegateCommand _stopCommand;

		private void StopExecute()
		{
			Op.Stop();
		}

		private bool CanStopExecute()
		{
			return Op.IsChecking || Op.IsCopying || Op.IsAutoRunning;
		}

		#endregion


		#region Save Desktop Command

		public DelegateCommand SaveDesktopCommand
		{
			get { return _saveDesktopCommand ?? (_saveDesktopCommand = new DelegateCommand(SaveDesktopExecute, CanSaveDesktopExecute)); }
		}
		private DelegateCommand _saveDesktopCommand;

		private async void SaveDesktopExecute()
		{
			await Op.SaveDesktopAsync();
		}

		private bool CanSaveDesktopExecute()
		{
			return (CurrentImageData != null) && !Op.IsSavingDesktop;
		}

		#endregion


		#region Send Clipboard Command

		public DelegateCommand SendClipboardCommand
		{
			get { return _sendClipboardCommand ?? (_sendClipboardCommand = new DelegateCommand(SendClipboardExecute, CanSendClipboardExecute)); }
		}
		private DelegateCommand _sendClipboardCommand;

		private async void SendClipboardExecute()
		{
			await Op.SendClipboardAsync();
		}

		private bool CanSendClipboardExecute()
		{
			return (CurrentImageData != null) && !Op.IsSendingClipboard;
		}

		#endregion


		private void RaiseCanExecuteChanged()
		{
			// This method is static.
			DelegateCommand.RaiseCanExecuteChanged();
		}

		#endregion


		#region Browser

		public bool IsBrowserOpen
		{
			get { return _isBrowserOpen; }
			set
			{
				_isBrowserOpen = value;
				RaisePropertyChanged();

				if (value)
					Op.Stop();
			}
		}
		private bool _isBrowserOpen;

		private void ManageBrowserOpen(bool isRunning)
		{
			if (isRunning)
				IsBrowserOpen = false;
		}

		#endregion


		#region Constructor

		public MainWindowViewModel()
		{
			SetSample(1);

			// Add event listeners.
			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_fileListPropertyChangedListener = new PropertyChangedEventListener(FileListPropertyChanged);
				PropertyChangedEventManager.AddListener(FileListCore, _fileListPropertyChangedListener, String.Empty);

				_settingsPropertyChangedListener = new PropertyChangedEventListener(ReactSettingsPropertyChanged);
				PropertyChangedEventManager.AddListener(Settings.Current, _settingsPropertyChangedListener, String.Empty);

				_operationPropertyChangedListener = new PropertyChangedEventListener(ReactOperationPropertyChanged);
				PropertyChangedEventManager.AddListener(Op, _operationPropertyChangedListener, String.Empty);
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
					handler => _targetDateChanged += handler,
					handler => _targetDateChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(200))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ =>
				{
					FileListCoreView.Refresh();
					Op.UpdateProgress();
				}));
		}

		private void SetSample(int number = 1)
		{
			Enumerable.Range(0, number)
				.Select(x =>
				{
					var source = String.Format("/DCIM,SAMPLE{0}.JPG,0,0,0,0", ((0 < x) ? x.ToString(CultureInfo.InvariantCulture) : String.Empty));
					return new FileItemViewModel(source, "/DCIM");
				})
				.ToList()
				.ForEach(x => FileListCore.Insert(x));
		}

		#endregion


		#region Event Listener

		#region FileItem

		private PropertyChangedEventListener _fileListPropertyChangedListener;

		private string CaseItemPropertyChangedSender
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(ItemObservableCollection<FileItemViewModel>)).ItemPropertyChangedSender); }
		}

		private string CaseIsSelected
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(FileItemViewModel)).IsSelected); }
		}

		private string CaseStatus
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(FileItemViewModel)).Status); }
		}

		private async void FileListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("File List property changed: {0} {1}", sender, e.PropertyName);

			if (e.PropertyName != CaseItemPropertyChangedSender)
				return;

			var item = ((ItemObservableCollection<FileItemViewModel>)sender).ItemPropertyChangedSender;
			var propertyName = ((ItemObservableCollection<FileItemViewModel>)sender).ItemPropertyChangedEventArgs.PropertyName;

			//Debug.WriteLine(String.Format("ItemPropertyChanged: {0} {1}", item.FileName, propertyName));

			if (propertyName == CaseIsSelected)
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
			else if (propertyName == CaseStatus)
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

		private string CaseAutoCheckInterval
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(Settings)).AutoCheckInterval); }
		}

		private string CaseTargetPeriod
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(Settings)).TargetPeriod); }
		}

		private string CaseTargetDates
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(Settings)).TargetDates); }
		}

		private event Action _autoCheckIntervalChanged = null;
		private event Action _targetDateChanged = null;

		private void ReactSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Settings property changed: {0} {1}", sender, e.PropertyName);

			var propertyName = e.PropertyName;

			if (propertyName == CaseAutoCheckInterval)
			{
				var handler = _autoCheckIntervalChanged;
				if (handler != null)
					handler();
			}
			else if ((propertyName == CaseTargetPeriod) || (propertyName == CaseTargetDates))
			{
				var handler = _targetDateChanged;
				if (handler != null)
					handler();
			}
		}

		#endregion


		#region Operation

		private PropertyChangedEventListener _operationPropertyChangedListener;

		private string CaseIsChecking
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(Operation)).IsChecking); }
		}

		private string CaseIsCopying
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(Operation)).IsCopying); }
		}

		private string CaseIsAutoRunning
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(Operation)).IsAutoRunning); }
		}

		private string CaseIsSavingDesktop
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(Operation)).IsSavingDesktop); }
		}

		private string CaseIsSendingClipboard
		{
			get { return GetPropertyName() ?? GetPropertyName(() => (default(Operation)).IsSendingClipboard); }
		}

		private void ReactOperationPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Operation property changed (MainWindowViewModel): {0} {1}", sender, e.PropertyName);

			var propertyName = e.PropertyName;

			if (propertyName == CaseIsChecking)
			{
				RaiseCanExecuteChanged();
				ManageBrowserOpen(Op.IsChecking);
			}
			else if (propertyName == CaseIsCopying)
			{
				RaiseCanExecuteChanged();
				ManageBrowserOpen(Op.IsCopying);
			}
			else if (propertyName == CaseIsAutoRunning)
			{
				RaiseCanExecuteChanged();
				ManageBrowserOpen(Op.IsAutoRunning);
			}
			else if ((propertyName == CaseIsSavingDesktop) || (propertyName == CaseIsSendingClipboard))
			{
				RaiseCanExecuteChanged();
			}
		}

		#endregion

		#endregion
	}
}
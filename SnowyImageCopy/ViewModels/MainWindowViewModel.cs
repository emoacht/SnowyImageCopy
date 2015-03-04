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

		public FileItemViewModelCollection FileListCore
		{
			get { return _fileListCore ?? (_fileListCore = new FileItemViewModelCollection()); }
		}
		private FileItemViewModelCollection _fileListCore;

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


		#region Operation progress

		/// <summary>
		/// Percentage of total size of local files of items that are in target and copied so far
		/// </summary>
		/// <remarks>This includes local files that are copied during current operation.</remarks>
		public double ProgressCopiedAll
		{
			get { return _progressCopiedAll; }
			set
			{
				_progressCopiedAll = value;
				RaisePropertyChanged();
			}
		}
		private double _progressCopiedAll = 40; // Sample percentage

		/// <summary>
		/// Percentage of total size of local files of items that are in target and copied during current operation
		/// </summary>
		public double ProgressCopiedCurrent
		{
			get { return _progressCopiedCurrent; }
			set
			{
				_progressCopiedCurrent = value;
				RaisePropertyChanged();
			}
		}
		private double _progressCopiedCurrent = 60; // Sample percentage

		/// <summary>
		/// Remaining time for current operation that is calculated by current transfer rate.
		/// </summary>
		public TimeSpan RemainingTime
		{
			get { return _remainingTime; }
			set
			{
				_remainingTime = value;
				RaisePropertyChanged();
			}
		}
		private TimeSpan _remainingTime;

		/// <summary>
		/// Whether progress has been updated
		/// </summary>
		public bool IsUpdated
		{
			get { return _isUpdated; }
			set
			{
				if (_isUpdated == value)
					return;

				_isUpdated = value;
				RaisePropertyChanged();
			}
		}
		private bool _isUpdated;

		private void UpdateProgress(ProgressInfo info)
		{
			IsUpdated = true;

			int sizeCopiedLatest = 0;
			var elapsedTimeLatest = TimeSpan.Zero;

			if (info != null)
			{
				sizeCopiedLatest = info.CurrentValue;
				elapsedTimeLatest = info.ElapsedTime;
			}

			var fileListBuff = FileListCoreView.Cast<FileItemViewModel>().ToArray();

			var sizeTotal = fileListBuff
				.Where(x => (x.Status != FileStatus.Recycled))
				.Sum(x => (long)x.Size);

			var sizeCopied = fileListBuff
				.Where(x => (x.Status == FileStatus.Copied))
				.Sum(x => (long)x.Size);

			if (sizeTotal == 0)
			{
				ProgressCopiedAll = 0D;
			}
			else
			{
				ProgressCopiedAll = (double)(sizeCopied + sizeCopiedLatest) * 100D / (double)sizeTotal;

				//Debug.WriteLine("ProgressCopiedAll: {0}", ProgressCopiedAll);
			}

			var sizeCopiedCurrent = fileListBuff
				.Where(x => (x.Status == FileStatus.Copied) && (Op.CopyStartTime < x.CopiedTime))
				.Sum(x => (long)x.Size);

			var sizeToBeCopied = fileListBuff
				.Where(x => (x.Status == FileStatus.ToBeCopied) || (x.Status == FileStatus.Copying))
				.Sum(x => (long)x.Size);

			if (sizeToBeCopied == 0)
			{
				ProgressCopiedCurrent = 0D;
				RemainingTime = TimeSpan.Zero;
			}
			else if (sizeCopiedLatest > 0)
			{
				ProgressCopiedCurrent = (double)(sizeCopiedCurrent + sizeCopiedLatest) * 100D / (double)(sizeCopiedCurrent + sizeToBeCopied);
				RemainingTime = TimeSpan.FromSeconds((double)(sizeToBeCopied - sizeCopiedLatest) * elapsedTimeLatest.TotalSeconds / (double)sizeCopiedLatest);

				//Debug.WriteLine("ProgressCopiedCurrent: {0} RemainingTime: {1}", ProgressCopiedCurrent, RemainingTime);
			}
		}

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

		private ReaderWriterLockSlim _dataLock = new ReaderWriterLockSlim();
		private bool _isCurrentImageDataGiven;

		public byte[] CurrentImageData
		{
			get
			{
				_dataLock.EnterReadLock();
				try
				{
					return _currentImageData;
				}
				finally
				{
					_dataLock.ExitReadLock();
				}
			}
			set
			{
				_dataLock.EnterWriteLock();
				try
				{
					_currentImageData = value;
				}
				finally
				{
					_dataLock.ExitWriteLock();
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
			// Set samples.
			FileListCore.Insert(CreateSampleFileItem(0));

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
			Disposer.Add(Observable.FromEvent
				(
					handler => _currentFrameSizeChanged += handler,
					handler => _currentFrameSizeChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(50))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => SetCurrentImage()));

			Disposer.Add(Observable.FromEvent
				(
					handler => _autoCheckChanged += handler,
					handler => _autoCheckChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(200))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => Op.ResetAutoTimer()));

			Disposer.Add(Observable.FromEvent
				(
					handler => _targetDateChanged += handler,
					handler => _targetDateChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(200))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => FileListCoreView.Refresh()));
		}

		private FileItemViewModel CreateSampleFileItem(int index)
		{
			var source = String.Format("/DCIM,SAMPLE{0}.JPG,0,0,0,0", ((0 < index) ? index.ToString(CultureInfo.InvariantCulture) : String.Empty));

			return new FileItemViewModel(source, "/DCIM");
		}

		#endregion


		#region Event Listener

		#region FileItem

		private PropertyChangedEventListener _fileListPropertyChangedListener;

		private string CaseItemProperty
		{
			get
			{
				return _caseItemProperty ?? (_caseItemProperty =
					PropertySupport.GetPropertyName(() => (default(FileItemViewModelCollection)).ItemPropertyChangedSender));
			}
		}
		private string _caseItemProperty;

		private string CaseFileStatus
		{
			get
			{
				return _caseFileStatus ?? (_caseFileStatus =
					PropertySupport.GetPropertyName(() => (default(FileItemViewModel)).IsSelected));
			}
		}
		private string _caseFileStatus;

		private string CaseInstantCopy
		{
			get
			{
				return _caseInstantCopy ?? (_caseInstantCopy =
					PropertySupport.GetPropertyName(() => (default(FileItemViewModel)).Status));
			}
		}
		private string _caseInstantCopy;

		private async void FileListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("File List property changed: {0} {1}", sender, e.PropertyName);

			if (e.PropertyName != CaseItemProperty)
				return;

			var item = ((FileItemViewModelCollection)sender).ItemPropertyChangedSender;
			var propertyName = ((FileItemViewModelCollection)sender).ItemPropertyChangedEventArgs.PropertyName;

			//Debug.WriteLine(String.Format("ItemPropartyChanegd: {0} {1}", item.FileName, propertyName));

			if (CaseFileStatus == propertyName)
			{
				switch (item.Status)
				{
					case FileStatus.NotCopied:
						// Make remote file as to be copied.
						if (!item.IsAliveRemote)
							return;

						item.Status = FileStatus.ToBeCopied;
						break;

					case FileStatus.ToBeCopied:
						// Make remote file as not to be copied.
						item.Status = item.IsAliveLocal ? FileStatus.Copied : FileStatus.NotCopied;
						break;

					case FileStatus.Copied:
						// Load image data from local file.
						if (!IsCurrentImageVisible || Op.IsCopying)
							return;

						await Op.LoadSetAsync(item);
						break;
				}
			}
			else if (CaseInstantCopy == propertyName)
			{
				if ((item.Status != FileStatus.ToBeCopied) || Op.IsChecking || Op.IsCopying || !Settings.Current.InstantCopy)
					return;

				await Op.CopyFileAsync();
			}
		}

		#endregion


		#region Settings

		private PropertyChangedEventListener _settingsPropertyChangedListener;

		private string CaseAutoCheck
		{
			get
			{
				return _caseAutoCheck ?? (_caseAutoCheck =
					PropertySupport.GetPropertyName(() => (default(Settings)).AutoCheckInterval));
			}
		}
		private string _caseAutoCheck;

		private string[] CaseTargetDate
		{
			get
			{
				if (_caseTargetDate == null)
				{
					var settings = default(Settings);

					_caseTargetDate = new[]
					{
						PropertySupport.GetPropertyName(() => settings.TargetPeriod),
						PropertySupport.GetPropertyName(() => settings.TargetDates),
					};
				}

				return _caseTargetDate;
			}
		}
		private string[] _caseTargetDate;

		private event Action _autoCheckChanged = null;
		private event Action _targetDateChanged = null;

		private void ReactSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Settings property changed: {0} {1}", sender, e.PropertyName);

			var propertyName = e.PropertyName;

			if (CaseAutoCheck == propertyName)
			{
				var handler = _autoCheckChanged;
				if (handler != null)
					handler();
			}
			else if (CaseTargetDate.Contains(propertyName))
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
			get
			{
				return _caseIsChecking ?? (_caseIsChecking =
					PropertySupport.GetPropertyName(() => (default(Operation)).IsChecking));
			}
		}
		private string _caseIsChecking;

		private string CaseIsCopying
		{
			get
			{
				return _caseIsCopying ?? (_caseIsCopying =
					PropertySupport.GetPropertyName(() => (default(Operation)).IsCopying));
			}
		}
		private string _caseIsCopying;

		private string CaseIsAutoRunning
		{
			get
			{
				return _caseIsAutoRunning ?? (_caseIsAutoRunning =
					PropertySupport.GetPropertyName(() => (default(Operation)).IsAutoRunning));
			}
		}
		private string _caseIsAutoRunning;

		private string CaseOperationProgress
		{
			get
			{
				return _caseOperationProgress ?? (_caseOperationProgress =
					PropertySupport.GetPropertyName(() => (default(Operation)).OperationProgress));
			}
		}
		private string _caseOperationProgress;

		private string CaseIsSavingDesktop
		{
			get
			{
				return _caseIsSavingDesktop ?? (_caseIsSavingDesktop =
					PropertySupport.GetPropertyName(() => (default(Operation)).IsSavingDesktop));
			}
		}
		private string _caseIsSavingDesktop;

		private string CaseIsSendingClipboard
		{
			get
			{
				return _caseIsSendingClipboard ?? (_caseIsSendingClipboard =
					PropertySupport.GetPropertyName(() => (default(Operation)).IsSendingClipboard));
			}
		}
		private string _caseIsSendingClipboard;

		private void ReactOperationPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Operation property changed (MainWindowViewModel): {0} {1}", sender, e.PropertyName);

			var propertyName = e.PropertyName;

			if (CaseIsChecking == propertyName)
			{
				RaiseCanExecuteChanged();
				ManageBrowserOpen(Op.IsChecking);
			}
			else if (CaseIsCopying == propertyName)
			{
				RaiseCanExecuteChanged();
				ManageBrowserOpen(Op.IsCopying);
			}
			else if (CaseIsAutoRunning == propertyName)
			{
				RaiseCanExecuteChanged();
				ManageBrowserOpen(Op.IsAutoRunning);
			}
			else if (CaseOperationProgress == propertyName)
			{
				UpdateProgress(Op.OperationProgress);
			}
			else if ((CaseIsSavingDesktop == propertyName) || (CaseIsSendingClipboard == propertyName))
			{
				RaiseCanExecuteChanged();
			}
		}

		#endregion

		#endregion
	}
}
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
using System.Windows.Media.Imaging;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Models;
using SnowyImageCopy.Views.Controls;
using SnowyImageCopy.Models.Exceptions;

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
		public double ProgressCopyFileAll
		{
			get { return _progressCopyFileAll; }
			set
			{
				_progressCopyFileAll = value;
				RaisePropertyChanged();
			}
		}
		private double _progressCopyFileAll = 40; // Sample percentage

		/// <summary>
		/// Percentage of total size of local files of items that are in target and copied during current operation
		/// </summary>
		public double ProgressCopyFileCurrent
		{
			get { return _progressCopyFileCurrent; }
			set
			{
				_progressCopyFileCurrent = value;
				RaisePropertyChanged();
			}
		}
		private double _progressCopyFileCurrent = 60; // Sample percentage

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

		private void UpdateProgress(ProgressInfo info)
		{
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
				.Sum(x => x.Size);

			var sizeCopied = fileListBuff
				.Where(x => (x.Status == FileStatus.Copied))
				.Sum(x => x.Size);

			if (sizeTotal == 0)
			{
				ProgressCopyFileAll = 0D;
			}
			else
			{
				ProgressCopyFileAll = (double)(sizeCopied + sizeCopiedLatest) * 100D / (double)sizeTotal;

				//Debug.WriteLine("ProgressCopyFileAll: {0}", ProgressCopyFileAll);
			}

			var sizeCopiedCurrent = fileListBuff
				.Where(x => (x.Status == FileStatus.Copied) && (Op.CopyStartTime < x.CopiedTime))
				.Sum(x => x.Size);

			var sizeToBeCopied = fileListBuff
				.Where(x => (x.Status == FileStatus.ToBeCopied) || (x.Status == FileStatus.Copying))
				.Sum(x => x.Size);

			if (sizeToBeCopied == 0)
			{
				ProgressCopyFileCurrent = 0D;
				RemainingTime = TimeSpan.Zero;
			}
			else if (sizeCopiedLatest > 0)
			{
				ProgressCopyFileCurrent = (double)(sizeCopiedCurrent + sizeCopiedLatest) * 100D / (double)(sizeCopiedCurrent + sizeToBeCopied);
				RemainingTime = TimeSpan.FromSeconds((double)(sizeToBeCopied - sizeCopiedLatest) * elapsedTimeLatest.TotalSeconds / (double)sizeCopiedLatest);

				//Debug.WriteLine("ProgressCopyFileCurrent: {0} RemainingTime: {1}", ProgressCopyFileCurrent, RemainingTime);
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

				var handler = CurrentFrameSizeChanged;
				if (handler != null)
				{
					handler();
				}
			}
		}
		private Size _currentFrameSize = Size.Empty;

		private event Action CurrentFrameSizeChanged = null;

		public FileItemViewModel CurrentItem { get; set; }

		public byte[] CurrentImageData
		{
			get { return _currentImageData; }
			set
			{
				_currentImageData = value;

				if (!Designer.IsInDesignMode)
					SetCurrentImage();
			}
		}
		private byte[] _currentImageData;

		public BitmapImage CurrentImage
		{
			get { return _currentImage ?? (_currentImage = GetDefaultCurrentImage()); }
			set
			{
				_currentImage = value;

				if (_currentImage != null)
					CurrentImageWidth = _currentImage.PixelWidth; // Not ordinary Width

				RaisePropertyChanged();
			}
		}
		private BitmapImage _currentImage;

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

			BitmapImage image = null;

			if ((CurrentImageData != null) && (CurrentItem != null))
			{
				try
				{
					if (!CurrentFrameSize.IsEmpty)
						image = await ImageManager.ConvertBytesToBitmapImageUniformAsync(CurrentImageData, CurrentFrameSize, CurrentItem.CanReadExif);
					else
						image = await ImageManager.ConvertBytesToBitmapImageAsync(CurrentImageData, CurrentImageWidth, CurrentItem.CanReadExif);
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


		private void RaiseCanExecuteChanged()
		{
			// This method is static.
			DelegateCommand.RaiseCanExecuteChanged();
		}

		#endregion


		#region Constructor

		public MainWindowViewModel()
		{
			// Set samples.
			FileListCore.Insert(GetSampleFileData(0));

			// Add event listeners.
			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				fileListPropertyChangedListener = new PropertyChangedEventListener(FileListPropertyChanged);
				PropertyChangedEventManager.AddListener(FileListCore, fileListPropertyChangedListener, String.Empty);

				settingsPropertyChangedListener = new PropertyChangedEventListener(ReactSettingsPropertyChanged);
				PropertyChangedEventManager.AddListener(Settings.Current, settingsPropertyChangedListener, String.Empty);

				operationPropertyChangedListener = new PropertyChangedEventListener(ReactOperationPropertyChanged);
				PropertyChangedEventManager.AddListener(Op, operationPropertyChangedListener, String.Empty);
			}

			// Subscribe event handlers.
			var currentFrameSizeChangedSubscriber =
				Observable.FromEvent
				(
					handler => CurrentFrameSizeChanged += handler,
					handler => CurrentFrameSizeChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(50))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => SetCurrentImage());

			var autoCheckIntervalChangedSubscriber =
				Observable.FromEvent
				(
					handler => AutoCheckChanged += handler,
					handler => AutoCheckChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(200))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => Op.ResetAutoTimer());

			var targetPeriodDatesChangedSubscriber =
				Observable.FromEvent
				(
					handler => TargetDateChanged += handler,
					handler => TargetDateChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(200))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(_ => FileListCoreView.Refresh());
		}

		private FileItemViewModel GetSampleFileData(int fileNumber)
		{
			var source = String.Format("/DCIM,SAMPLE{0}.JPG,0,0,0,0", ((0 < fileNumber) ? fileNumber.ToString(CultureInfo.InvariantCulture) : String.Empty));

			return new FileItemViewModel(source, "/DCIM");
		}

		#endregion


		#region Event listener

		#region FileItem

		private PropertyChangedEventListener fileListPropertyChangedListener;

		private string CaseItemPropertyChanged
		{
			get
			{
				return _caseItemPropertyChanged ?? (_caseItemPropertyChanged =
					PropertySupport.GetPropertyName(() => new FileItemViewModelCollection().ItemPropertyChangedSender));
			}
		}
		private string _caseItemPropertyChanged;

		private string CaseFileStatusChanged
		{
			get
			{
				return _caseFileStatusChanged ?? (_caseFileStatusChanged =
					PropertySupport.GetPropertyName(() => new FileItemViewModel().IsSelected));
			}
		}
		private string _caseFileStatusChanged;

		private string CaseInstantCopy
		{
			get
			{
				return _caseInstantCopy ?? (_caseInstantCopy =
					PropertySupport.GetPropertyName(() => new FileItemViewModel().Status));
			}
		}
		private string _caseInstantCopy;

		private async void FileListPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("File List property changed: {0} {1}", sender, e.PropertyName);

			if (e.PropertyName != CaseItemPropertyChanged)
				return;

			var item = ((FileItemViewModelCollection)sender).ItemPropertyChangedSender;
			var propertyName = ((FileItemViewModelCollection)sender).ItemPropertyChangedEventArgs.PropertyName;

			//Debug.WriteLine(String.Format("ItemPropartyChanegd: {0} {1}", item.FileName, propertyName));

			if (CaseFileStatusChanged == propertyName)
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

						await Op.LoadSetFileAsync(item);
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

		private PropertyChangedEventListener settingsPropertyChangedListener;

		private string CaseAutoCheckChanged
		{
			get
			{
				return _caseAutoCheckChanged ?? (_caseAutoCheckChanged =
					PropertySupport.GetPropertyName(() => new Settings().AutoCheckInterval));
			}
		}
		private string _caseAutoCheckChanged;

		private string[] CaseTargetDateChanged
		{
			get
			{
				if (_caseTargetDateChanged == null)
				{
					var instance = new Settings();

					_caseTargetDateChanged = new string[]
					{
						PropertySupport.GetPropertyName(() => instance.TargetPeriod),
						PropertySupport.GetPropertyName(() => instance.TargetDates),
					};
				}

				return _caseTargetDateChanged;
			}
		}
		private string[] _caseTargetDateChanged;

		private event Action AutoCheckChanged = null;
		private event Action TargetDateChanged = null;

		private void ReactSettingsPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Settings property changed: {0} {1}", sender, e.PropertyName);

			var propertyName = e.PropertyName;

			if (CaseAutoCheckChanged == propertyName)
			{
				var handler = AutoCheckChanged;
				if (handler != null)
				{
					handler();
				}
			}
			else if (CaseTargetDateChanged.Contains(propertyName))
			{
				var handler = TargetDateChanged;
				if (handler != null)
				{
					handler();
				}
			}
		}

		#endregion


		#region Operation

		private PropertyChangedEventListener operationPropertyChangedListener;

		private string[] CaseOperationStateChanged
		{
			get
			{
				if (_caseOperationStateChanged == null)
				{
					var instance = new Operation(null);

					_caseOperationStateChanged = new string[]
					{
						PropertySupport.GetPropertyName(() => instance.IsChecking),
						PropertySupport.GetPropertyName(() => instance.IsCopying),
						PropertySupport.GetPropertyName(() => instance.IsAutoRunning),
					};
				}

				return _caseOperationStateChanged;
			}
		}
		private string[] _caseOperationStateChanged;

		private string CaseOperationProgressChanged
		{
			get
			{
				return _caseOperationProgressChanged ?? (_caseOperationProgressChanged =
					PropertySupport.GetPropertyName(() => new Operation(null).OperationProgress));
			}
		}
		private string _caseOperationProgressChanged;

		private void ReactOperationPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Operation property changed (MainWindowViewModel): {0} {1}", sender, e.PropertyName);

			var propertyName = e.PropertyName;

			if (CaseOperationStateChanged.Contains(propertyName))
			{
				RaiseCanExecuteChanged();
			}
			else if (CaseOperationProgressChanged == propertyName)
			{
				UpdateProgress(Op.OperationProgress);
			}
		}

		#endregion

		#endregion
	}
}
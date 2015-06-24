using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Media;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Shell;
using System.Windows.Threading;

using DesktopToast;
using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Models.Exceptions;
using SnowyImageCopy.Properties;
using SnowyImageCopy.ViewModels;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Run operations.
	/// </summary>
	public class Operation : NotificationObject
	{
		/// <summary>
		/// Instance of MainWindowViewModel
		/// </summary>
		private MainWindowViewModel MainWindowViewModelInstance
		{
			get { return _mainWindowViewModelInstance; }
			set
			{
				if ((_mainWindowViewModelInstance != null) || (value == null))
					return;

				_mainWindowViewModelInstance = value;

				if (!Designer.IsInDesignMode) // ListCollectionView may be null in Design mode.
				{
					FileListCoreView.CurrentChanged += (sender, e) => CheckFileListCoreViewThumbnail();
				}
			}
		}
		private MainWindowViewModel _mainWindowViewModelInstance;

		public Operation(MainWindowViewModel mainWindowViewModelInstance)
		{
			MainWindowViewModelInstance = mainWindowViewModelInstance;
		}


		#region Access to MainWindowViewModel member

		private string OperationStatus
		{
			set { MainWindowViewModelInstance.OperationStatus = value; }
		}

		private ItemObservableCollection<FileItemViewModel> FileListCore
		{
			get { return MainWindowViewModelInstance.FileListCore; }
		}

		private ListCollectionView FileListCoreView
		{
			get { return MainWindowViewModelInstance.FileListCoreView; }
		}

		private int FileListCoreViewIndex
		{
			set { MainWindowViewModelInstance.FileListCoreViewIndex = value; }
		}

		private FileItemViewModel CurrentItem
		{
			get { return MainWindowViewModelInstance.CurrentItem; }
			set { MainWindowViewModelInstance.CurrentItem = value; }
		}

		private byte[] CurrentImageData
		{
			get { return MainWindowViewModelInstance.CurrentImageData; }
			set { MainWindowViewModelInstance.CurrentImageData = value; }
		}

		private bool IsWindowActivateRequested
		{
			set { MainWindowViewModelInstance.IsWindowActivateRequested = value; }
		}

		#endregion


		#region Constant

		/// <summary>
		/// Waiting time length for showing status in case of failure during auto check
		/// </summary>
		private readonly TimeSpan _autoWaitingLength = TimeSpan.FromSeconds(5);

		/// <summary>
		/// Threshold time length of intervals to actually check files during auto check
		/// </summary>
		private readonly TimeSpan _autoThresholdLength = TimeSpan.FromMinutes(10);

		/// <summary>
		/// Waiting time length for showing completion of copying
		/// </summary>
		private readonly TimeSpan _copyWaitingLength = TimeSpan.FromSeconds(0.2);

		/// <summary>
		/// Threshold time length of copying to determine whether to send a toast notification after copy
		/// </summary>
		private readonly TimeSpan _toastThresholdLength = TimeSpan.FromSeconds(30);

		#endregion


		#region Operation state

		/// <summary>
		/// Checking files in FlashAir card
		/// </summary>
		public bool IsChecking
		{
			get { return _isChecking; }
			set
			{
				_isChecking = value;
				RaisePropertyChanged();
			}
		}
		private bool _isChecking;

		/// <summary>
		/// Copying files from FlashAir card
		/// </summary>
		public bool IsCopying
		{
			get { return _isCopying; }
			set
			{
				_isCopying = value;
				RaisePropertyChanged();
			}
		}
		private bool _isCopying;

		/// <summary>
		/// Running timer for auto check
		/// </summary>
		internal bool IsAutoRunning
		{
			get { return _isAutoRunning; }
			set
			{
				_isAutoRunning = value;
				RaisePropertyChanged();
			}
		}
		private bool _isAutoRunning;

		/// <summary>
		/// Saving current image data on desktop
		/// </summary>
		internal bool IsSavingDesktop
		{
			get { return _isSavingDesktop; }
			set
			{
				_isSavingDesktop = value;
				RaisePropertyChanged();
			}
		}
		private bool _isSavingDesktop;

		/// <summary>
		/// Sending current image data to clipboard
		/// </summary>
		internal bool IsSendingClipboard
		{
			get { return _isSendingClipboard; }
			set
			{
				_isSendingClipboard = value;
				RaisePropertyChanged();
			}
		}
		private bool _isSendingClipboard;

		#endregion


		#region Progress

		/// <summary>
		/// Percentage of total size of items in target and copied so far including current operation
		/// </summary>
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
		/// Percentage of total size of items in target and copied during current operation
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
		/// Remaining time for current operation calculated by current transfer rate
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
		/// Progress state of current operation
		/// </summary>
		public TaskbarItemProgressState ProgressState
		{
			get { return _progressState; }
			set
			{
				if (_progressState == value)
					return;

				_progressState = value;
				RaisePropertyChanged();
			}
		}
		private TaskbarItemProgressState _progressState;

		private readonly object _updateLocker = new object();
		private long _sizeOverall;           // Total size of items
		private long _sizeCopiedAll;         // Total size of items copied so far including current operation
		private long _sizeCopiedCurrent;     // Total size of items copied during current operation
		private long _sizeToBeCopiedCurrent; // Total size of items to be copied during current operation

		internal void UpdateProgress(ProgressInfo info = null)
		{
			lock (_updateLocker)
			{
				UpdateSize(info);
				UpdateProgressAll(info);
				UpdateProgressCurrent(info);
			}
		}

		private void UpdateSize(ProgressInfo info)
		{
			if ((info != null) && !info.IsFirst)
				return;

			var checksCopiedCurrent = (info != null);

			_sizeOverall = 0L;
			_sizeCopiedAll = 0L;
			_sizeCopiedCurrent = 0L;
			_sizeToBeCopiedCurrent = 0L;

			foreach (var item in FileListCoreView.Cast<FileItemViewModel>())
			{
				switch (item.Status)
				{
					case FileStatus.Recycled:
						break;
					default:
						_sizeOverall += item.Size;

						switch (item.Status)
						{
							case FileStatus.Copied:
								_sizeCopiedAll += item.Size;

								if (checksCopiedCurrent && (CopyStartTime < item.CopiedTime))
									_sizeCopiedCurrent += item.Size;
								break;

							case FileStatus.ToBeCopied:
							case FileStatus.Copying:
								_sizeToBeCopiedCurrent += item.Size;
								break;
						}
						break;
				}
			}
		}

		private void UpdateProgressAll(ProgressInfo info)
		{
			if (_sizeOverall == 0)
			{
				ProgressCopiedAll = 0D;
			}
			else
			{
				var sizeCopiedLatest = (info != null) ? (long)info.CurrentValue : 0L;

				ProgressCopiedAll = (double)(_sizeCopiedAll + sizeCopiedLatest) * 100D / (double)_sizeOverall;

				//Debug.WriteLine("ProgressCopiedAll: {0}", ProgressCopiedAll);
			}
		}

		private void UpdateProgressCurrent(ProgressInfo info)
		{
			if (info == null)
			{
				ProgressCopiedCurrent = 0D;
				RemainingTime = TimeSpan.Zero;
				ProgressState = TaskbarItemProgressState.None;
			}
			else if (info.IsError)
			{
				ProgressState = TaskbarItemProgressState.Error;
			}
			else
			{
				var sizeCopiedLatest = (long)info.CurrentValue;
				var elapsedTimeLatest = info.ElapsedTime;
				var sizeCopiedToBeCopiedCurrent = _sizeCopiedCurrent + _sizeToBeCopiedCurrent;

				if ((sizeCopiedLatest <= 0) || (sizeCopiedToBeCopiedCurrent <= 0))
					return; // For just in case

				ProgressCopiedCurrent = (double)(_sizeCopiedCurrent + sizeCopiedLatest) * 100D / (double)sizeCopiedToBeCopiedCurrent;
				RemainingTime = TimeSpan.FromSeconds((double)(_sizeToBeCopiedCurrent - sizeCopiedLatest) * elapsedTimeLatest.TotalSeconds / (double)sizeCopiedLatest);
				ProgressState = TaskbarItemProgressState.Normal;

				//Debug.WriteLine("ProgressCopiedCurrent: {0} RemainingTime: {1}", ProgressCopiedCurrent, RemainingTime);
			}
		}

		#endregion


		#region Auto check

		private DispatcherTimer _autoTimer;

		private bool _isFileListCoreViewThumbnailFilled;

		private void CheckFileListCoreViewThumbnail()
		{
			if (IsChecking)
				return;

			_isFileListCoreViewThumbnailFilled = FileListCore
				.Where(x => x.IsTarget && (x.Size != 0))
				.Where(x => (x.IsAliveRemote && x.CanGetThumbnailRemote) || (x.IsAliveLocal && x.IsAvailableLocal && x.CanLoadDataLocal))
				.All(x => x.HasThumbnail);
		}

		internal void StartAutoTimer()
		{
			CheckFileListCoreViewThumbnail();

			IsAutoRunning = true;
			ResetAutoTimer();
		}

		private void StopAutoTimer()
		{
			IsAutoRunning = false;
			ResetAutoTimer();
		}

		internal void ResetAutoTimer()
		{
			if (IsAutoRunning)
			{
				if (_autoTimer == null)
				{
					_autoTimer = new DispatcherTimer();
					_autoTimer.Tick += OnAutoTimerTick;
				}

				_autoTimer.Stop();
				_autoTimer.Interval = TimeSpan.FromSeconds(Settings.Current.AutoCheckInterval);
				_autoTimer.Start();
				OperationStatus = Resources.OperationStatus_WaitingAutoCheck;
			}
			else
			{
				if (_autoTimer == null)
					return;

				_autoTimer.Stop();
				_autoTimer = null;
				SystemSounds.Asterisk.Play();
				OperationStatus = Resources.OperationStatus_Stopped;
			}
		}

		private async void OnAutoTimerTick(object sender, EventArgs e)
		{
			if (IsChecking || IsCopying)
				return;

			_autoTimer.Stop();

			if (!await ExecuteAutoCheckAsync())
				await Task.Delay(_autoWaitingLength);

			if (IsAutoRunning)
			{
				_autoTimer.Start();
				OperationStatus = Resources.OperationStatus_WaitingAutoCheck;
			}
		}

		/// <summary>
		/// Execute auto check by timer.
		/// </summary>
		/// <returns>False if failed</returns>
		private async Task<bool> ExecuteAutoCheckAsync()
		{
			if (_isFileListCoreViewThumbnailFilled &&
				(DateTime.Now < LastCheckCopyTime.Add(_autoThresholdLength)))
			{
				var isUpdated = await CheckUpdateAsync();
				if (!isUpdated.HasValue)
					return false;
				if (!isUpdated.Value)
					return true; // This is true case.
			}

			var hasCompleted = await CheckCopyFileAsync();
			if (hasCompleted)
				_isFileListCoreViewThumbnailFilled = true;

			return hasCompleted;
		}

		#endregion


		#region Check & Copy

		private readonly CardInfo _card = new CardInfo();

		private CancellationTokenSource _tokenSourceWorking;
		private bool _isTokenSourceWorkingDisposed;

		private DateTime LastCheckCopyTime { get; set; }

		internal DateTime CopyStartTime { get; private set; }
		private int _copyFileCount;


		#region 1st tier

		/// <summary>
		/// Check if content of FlashAir card is updated.
		/// </summary>
		/// <returns>
		/// True:  If completed and content found updated.
		/// False: If completed and content found not updated.
		/// Null:  If failed.
		/// </returns>
		private async Task<bool?> CheckUpdateAsync()
		{
			try
			{
				if (NetworkChecker.IsNetworkConnected(_card))
				{
					OperationStatus = Resources.OperationStatus_Checking;

					var isUpdated = _card.CanGetWriteTimeStamp
						? (await FileManager.GetWriteTimeStampAsync() != _card.WriteTimeStamp)
						: await FileManager.CheckUpdateStatusAsync();

					OperationStatus = Resources.OperationStatus_Completed;
					return isUpdated;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
					return false;
				}
			}
			catch (Exception ex)
			{
				if (ex.GetType() == typeof(RemoteConnectionUnableException))
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
				}
				else if (ex.GetType() == typeof(RemoteConnectionLostException))
				{
					OperationStatus = Resources.OperationStatus_ConnectionLost;
				}
				else if (ex.GetType() == typeof(TimeoutException))
				{
					OperationStatus = Resources.OperationStatus_TimedOut;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_Error;
					Debug.WriteLine("Failed to check if content is updated. {0}", ex);
					throw new UnexpectedException("Failed to check if content is updated.", ex);
				}
			}

			return null;
		}

		/// <summary>
		/// Check & Copy files in FlashAir card.
		/// </summary>
		/// <returns>
		/// True:  If completed.
		/// False: If interrupted or failed.
		/// </returns>
		internal async Task<bool> CheckCopyFileAsync()
		{
			if (!IsReady())
				return true; // This is true case.

			try
			{
				IsChecking = true;
				IsCopying = true;
				UpdateProgress();

				await CheckFileBaseAsync();

				UpdateProgress();

				LastCheckCopyTime = CheckFileToBeCopied(true)
					? DateTime.MinValue
					: DateTime.Now;

				await CopyFileBaseAsync(new Progress<ProgressInfo>(UpdateProgress));

				LastCheckCopyTime = DateTime.Now;

				await Task.Delay(_copyWaitingLength);
				UpdateProgress();
				IsChecking = false;
				IsCopying = false;

				await ShowToastAsync();

				return true;
			}
			catch (OperationCanceledException)
			{
				SystemSounds.Asterisk.Play();
				OperationStatus = Resources.OperationStatus_Stopped;
			}
			catch (Exception ex)
			{
				UpdateProgress(new ProgressInfo(isError: true));

				if (!IsAutoRunning)
					SystemSounds.Hand.Play();

				if (ex.GetType() == typeof(RemoteConnectionUnableException))
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
				}
				else if (ex.GetType() == typeof(RemoteConnectionLostException))
				{
					OperationStatus = Resources.OperationStatus_ConnectionLost;
				}
				else if (ex.GetType() == typeof(RemoteFileNotFoundException))
				{
					OperationStatus = Resources.OperationStatus_NotFolderFound;
				}
				else if (ex.GetType() == typeof(TimeoutException))
				{
					OperationStatus = Resources.OperationStatus_TimedOut;
				}
				else if (ex.GetType() == typeof(UnauthorizedAccessException))
				{
					OperationStatus = Resources.OperationStatus_UnauthorizedAccess;
				}
				else if (ex.GetType() == typeof(CardChangedException))
				{
					OperationStatus = Resources.OperationStatus_NotSameFlashAir;
				}
				else if (ex.GetType() == typeof(CardUploadDisabledException))
				{
					OperationStatus = Resources.OperationStatus_DeleteDisabled;
				}
				else if (ex.GetType() == typeof(RemoteFileDeletionFailedException))
				{
					OperationStatus = Resources.OperationStatus_DeleteFailed;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_Error;
					Debug.WriteLine("Failed to check & copy files. {0}", ex);
					throw new UnexpectedException("Failed to check & copy files.", ex);
				}
			}
			finally
			{
				// In case any exception happened.
				IsChecking = false;
				IsCopying = false;
			}

			return false;
		}

		/// <summary>
		/// Check files in FlashAir card.
		/// </summary>
		internal async Task CheckFileAsync()
		{
			if (!IsReady())
				return;

			try
			{
				IsChecking = true;
				UpdateProgress();

				await CheckFileBaseAsync();

				UpdateProgress();

				LastCheckCopyTime = CheckFileToBeCopied(false)
					? DateTime.MinValue
					: DateTime.Now;
			}
			catch (OperationCanceledException)
			{
				SystemSounds.Asterisk.Play();
				OperationStatus = Resources.OperationStatus_Stopped;
			}
			catch (Exception ex)
			{
				UpdateProgress(new ProgressInfo(isError: true));
				SystemSounds.Hand.Play();

				if (ex.GetType() == typeof(RemoteConnectionUnableException))
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
				}
				else if (ex.GetType() == typeof(RemoteConnectionLostException))
				{
					OperationStatus = Resources.OperationStatus_ConnectionLost;
				}
				else if (ex.GetType() == typeof(RemoteFileNotFoundException))
				{
					OperationStatus = Resources.OperationStatus_NotFolderFound;
				}
				else if (ex.GetType() == typeof(TimeoutException))
				{
					OperationStatus = Resources.OperationStatus_TimedOut;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_Error;
					Debug.WriteLine("Failed to check files. {0}", ex);
					throw new UnexpectedException("Failed to check files.", ex);
				}
			}
			finally
			{
				IsChecking = false;
			}
		}

		/// <summary>
		/// Copy files from FlashAir card.
		/// </summary>
		internal async Task CopyFileAsync()
		{
			if (!IsReady())
				return;

			try
			{
				IsCopying = true;

				await CopyFileBaseAsync(new Progress<ProgressInfo>(UpdateProgress));

				await Task.Delay(_copyWaitingLength);
				UpdateProgress();
				IsCopying = false;

				await ShowToastAsync();
			}
			catch (OperationCanceledException)
			{
				SystemSounds.Asterisk.Play();
				OperationStatus = Resources.OperationStatus_Stopped;
			}
			catch (Exception ex)
			{
				UpdateProgress(new ProgressInfo(isError: true));
				SystemSounds.Hand.Play();

				if (ex.GetType() == typeof(RemoteConnectionUnableException))
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
				}
				else if (ex.GetType() == typeof(RemoteConnectionLostException))
				{
					OperationStatus = Resources.OperationStatus_ConnectionLost;
				}
				else if (ex.GetType() == typeof(TimeoutException))
				{
					OperationStatus = Resources.OperationStatus_TimedOut;
				}
				else if (ex.GetType() == typeof(UnauthorizedAccessException))
				{
					OperationStatus = Resources.OperationStatus_UnauthorizedAccess;
				}
				else if (ex.GetType() == typeof(CardChangedException))
				{
					OperationStatus = Resources.OperationStatus_NotSameFlashAir;
				}
				else if (ex.GetType() == typeof(CardUploadDisabledException))
				{
					OperationStatus = Resources.OperationStatus_DeleteDisabled;
				}
				else if (ex.GetType() == typeof(RemoteFileDeletionFailedException))
				{
					OperationStatus = Resources.OperationStatus_DeleteFailed;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_Error;
					Debug.WriteLine("Failed to copy files. {0}", ex);
					throw new UnexpectedException("Failed to copy files.", ex);
				}
			}
			finally
			{
				// In case any exception happened.
				IsCopying = false;
			}
		}

		/// <summary>
		/// Stop operation.
		/// </summary>
		internal void Stop()
		{
			StopAutoTimer();

			if (_isTokenSourceWorkingDisposed || (_tokenSourceWorking == null) || _tokenSourceWorking.IsCancellationRequested)
				return;

			try
			{
				_tokenSourceWorking.Cancel();
			}
			catch (ObjectDisposedException ode)
			{
				Debug.WriteLine("CancellationTokenSource has been disposed when tried to cancel operation. {0}", ode);
			}
		}

		#endregion


		#region 2nd tier

		/// <summary>
		/// Check if ready for operation.
		/// </summary>
		/// <returns>True if ready</returns>
		private bool IsReady()
		{
			if (!NetworkChecker.IsNetworkConnected())
			{
				OperationStatus = Resources.OperationStatus_NoNetwork;
				return false;
			}

			if ((Settings.Current.TargetPeriod == FilePeriod.Select) &&
				!Settings.Current.TargetDates.Any())
			{
				OperationStatus = Resources.OperationStatus_NoDateCalender;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Check files in FlashAir card.
		/// </summary>
		private async Task CheckFileBaseAsync()
		{
			OperationStatus = Resources.OperationStatus_Checking;

			try
			{
				_tokenSourceWorking = new CancellationTokenSource();
				_isTokenSourceWorkingDisposed = false;

				// Check firmware version.
				_card.FirmwareVersion = await FileManager.GetFirmwareVersionAsync(_tokenSourceWorking.Token);

				// Check CID.
				if (_card.CanGetCid)
					_card.Cid = await FileManager.GetCidAsync(_tokenSourceWorking.Token);

				// Check SSID and check if PC is connected to FlashAir card by wireless network.
				_card.Ssid = await FileManager.GetSsidAsync(_tokenSourceWorking.Token);
				_card.IsWirelessConnected = NetworkChecker.IsWirelessNetworkConnected(_card.Ssid);

				// Get all items.
				var fileListNew = await FileManager.GetFileListRootAsync(_card, _tokenSourceWorking.Token);
				fileListNew.Sort();

				// Record time stamp of write event.
				if (_card.CanGetWriteTimeStamp)
					_card.WriteTimeStamp = await FileManager.GetWriteTimeStampAsync(_tokenSourceWorking.Token);

				// Check if any sample is in old items.
				var isSample = FileListCore.Any(x => x.Size == 0);

				// Check if FlashAir card is changed.
				bool isChanged;
				if (_card.IsChanged.HasValue)
				{
					isChanged = _card.IsChanged.Value;
				}
				else
				{
					var signatures = FileListCore.Select(x => x.Signature).ToArray();
					isChanged = !fileListNew.Select(x => x.Signature).Any(x => signatures.Contains(x));
				}

				if (isSample || isChanged)
					FileListCore.Clear();

				// Check old items.
				foreach (var itemOld in FileListCore)
				{
					var itemSame = fileListNew.FirstOrDefault(x =>
						x.FilePath.Equals(itemOld.FilePath, StringComparison.OrdinalIgnoreCase) &&
						(x.Size == itemOld.Size));

					if (itemSame != null)
					{
						fileListNew.Remove(itemSame);

						itemOld.IsAliveRemote = true;
						itemOld.IsReadOnly = itemSame.IsReadOnly;
						continue;
					}

					itemOld.IsAliveRemote = false;
				}

				// Add new items.
				var isLeadOff = true;
				foreach (var itemNew in fileListNew)
				{
					if (isLeadOff)
					{
						FileListCoreViewIndex = FileListCoreView.IndexOf(itemNew);
						isLeadOff = false;
					}

					itemNew.IsAliveRemote = true;

					FileListCore.Insert(itemNew); // Customized Insert method
				}

				// Check all local files.
				foreach (var item in FileListCore)
				{
					var info = GetFileInfoLocal(item);
					item.IsAliveLocal = IsAliveLocal(info, item.Size);
					item.IsAvailableLocal = IsAvailableLocal(info);
					item.Status = item.IsAliveLocal ? FileStatus.Copied : FileStatus.NotCopied;
				}

				// Manage lost items.
				var itemsLost = FileListCore.Where(x => !x.IsAliveRemote).ToArray();
				if (itemsLost.Any())
				{
					if (Settings.Current.MovesFileToRecycle)
					{
						foreach (var item in itemsLost)
						{
							if (!item.IsDescendant || (item.Status != FileStatus.Copied))
								continue;

							try
							{
								Recycle.MoveToRecycle(ComposeLocalPath(item));
							}
							catch (Exception ex)
							{
								Debug.WriteLine("Failed to move a file to Recycle. {0}", ex);
								item.Status = FileStatus.NotCopied;
								continue;
							}

							item.Status = FileStatus.Recycled;
						}
					}

					for (int i = itemsLost.Length - 1; i >= 0; i--)
					{
						var item = itemsLost[i];
						if (item.IsDescendant &&
							((item.Status == FileStatus.Copied) || (item.Status == FileStatus.Recycled)))
							continue;

						FileListCore.Remove(item);
					}
				}

				// Get thumbnails (from local).
				foreach (var item in FileListCore.Where(x => x.IsTarget && !x.HasThumbnail))
				{
					if (!item.IsAliveLocal || !item.IsAvailableLocal || !item.CanLoadDataLocal)
						continue;

					_tokenSourceWorking.Token.ThrowIfCancellationRequested();

					try
					{
						if (item.CanReadExif)
							item.Thumbnail = await ImageManager.ReadThumbnailAsync(ComposeLocalPath(item));
						else if (item.CanLoadDataLocal)
							item.Thumbnail = await ImageManager.CreateThumbnailAsync(ComposeLocalPath(item));
					}
					catch (FileNotFoundException)
					{
						item.Status = FileStatus.NotCopied;
						item.IsAliveLocal = false;
					}
					catch (IOException)
					{
						item.IsAvailableLocal = false;
					}
					catch (ImageNotSupportedException)
					{
						item.CanLoadDataLocal = false;
					}
				}

				// Get thumbnails (from remote).
				foreach (var item in FileListCore.Where(x => x.IsTarget && !x.HasThumbnail))
				{
					if (!item.IsAliveRemote || !item.CanGetThumbnailRemote)
						continue;

					if (!_card.CanGetThumbnail)
						continue;

					_tokenSourceWorking.Token.ThrowIfCancellationRequested();

					try
					{
						item.Thumbnail = await FileManager.GetThumbnailAsync(item.FilePath, _card, _tokenSourceWorking.Token);
					}
					catch (RemoteFileThumbnailFailedException)
					{
						item.CanGetThumbnailRemote = false;
						_card.ThumbnailFailedPath = item.FilePath;
					}
				}

				OperationStatus = Resources.OperationStatus_CheckCompleted;
			}
			finally
			{
				FileListCoreViewIndex = -1; // No selection

				if (_tokenSourceWorking != null)
				{
					_isTokenSourceWorkingDisposed = true;
					_tokenSourceWorking.Dispose();
				}
			}
		}

		/// <summary>
		/// Check files to be copied from FlashAir card.
		/// </summary>
		/// <param name="changesToBeCopied">Whether to change file status if a file meets conditions to be copied</param>
		/// <returns>True if any file to be copied is contained</returns>
		private bool CheckFileToBeCopied(bool changesToBeCopied)
		{
			int countToBeCopied = 0;

			foreach (var item in FileListCore)
			{
				if (item.IsTarget && item.IsAliveRemote && (item.Status == FileStatus.NotCopied) &&
					(!Settings.Current.SelectsReadOnlyFile || item.IsReadOnly))
				{
					countToBeCopied++;

					if (changesToBeCopied)
						item.Status = FileStatus.ToBeCopied;
				}
			}

			return (0 < countToBeCopied);
		}

		/// <summary>
		/// Copy files from FlashAir card.
		/// </summary>
		/// <param name="progress">Progress</param>
		private async Task CopyFileBaseAsync(IProgress<ProgressInfo> progress)
		{
			CopyStartTime = DateTime.Now;
			_copyFileCount = 0;

			if (!FileListCore.Any(x => x.IsTarget && (x.Status == FileStatus.ToBeCopied)))
			{
				OperationStatus = Resources.OperationStatus_NoFileToBeCopied;
				return;
			}

			OperationStatus = Resources.OperationStatus_Copying;

			try
			{
				_tokenSourceWorking = new CancellationTokenSource();
				_isTokenSourceWorkingDisposed = false;

				// Check CID.
				if (_card.CanGetCid)
				{
					if (await FileManager.GetCidAsync(_tokenSourceWorking.Token) != _card.Cid)
						throw new CardChangedException();
				}

				// Check if upload.cgi is disabled.
				if (Settings.Current.DeleteOnCopy && _card.CanGetUpload)
				{
					_card.Upload = await FileManager.GetUploadAsync(_tokenSourceWorking.Token);
					if (_card.IsUploadDisabled)
						throw new CardUploadDisabledException();
				}

				while (true)
				{
					var item = FileListCore.FirstOrDefault(x => x.IsTarget && (x.Status == FileStatus.ToBeCopied));
					if (item == null)
						break; // Copy completed.

					_tokenSourceWorking.Token.ThrowIfCancellationRequested();

					try
					{
						item.Status = FileStatus.Copying;

						FileListCoreViewIndex = FileListCoreView.IndexOf(item);

						var localPath = ComposeLocalPath(item);

						var localDirectory = Path.GetDirectoryName(localPath);
						if (!String.IsNullOrEmpty(localDirectory) && !Directory.Exists(localDirectory))
							Directory.CreateDirectory(localDirectory);

						var data = await FileManager.GetSaveFileAsync(item.FilePath, localPath, item.Size, item.Date, item.CanReadExif, progress, _card, _tokenSourceWorking.Token);

						CurrentItem = item;
						CurrentImageData = data;

						if (!item.HasThumbnail)
						{
							try
							{
								if (item.CanReadExif)
									item.Thumbnail = await ImageManager.ReadThumbnailAsync(CurrentImageData);
								else if (item.CanLoadDataLocal)
									item.Thumbnail = await ImageManager.CreateThumbnailAsync(CurrentImageData);
							}
							catch (ImageNotSupportedException)
							{
								item.CanLoadDataLocal = false;
							}
						}

						item.CopiedTime = DateTime.Now;
						item.IsAliveLocal = true;
						item.IsAvailableLocal = true;
						item.Status = FileStatus.Copied;

						_copyFileCount++;
					}
					catch (RemoteFileNotFoundException)
					{
						item.IsAliveRemote = false;
					}
					catch (RemoteFileInvalidException)
					{
						item.Status = FileStatus.Weird;
					}
					catch
					{
						item.Status = FileStatus.ToBeCopied; // Revert to status before copying.
						throw;
					}

					if (Settings.Current.DeleteOnCopy &&
						!item.IsReadOnly && IsAliveLocal(item))
					{
						await FileManager.DeleteFileAsync(item.FilePath, _tokenSourceWorking.Token);
					}
				}

				OperationStatus = String.Format(Resources.OperationStatus_CopyCompleted, _copyFileCount, (int)(DateTime.Now - CopyStartTime).TotalSeconds);
			}
			finally
			{
				FileListCoreViewIndex = -1; // No selection

				if (_tokenSourceWorking != null)
				{
					_isTokenSourceWorkingDisposed = true;
					_tokenSourceWorking.Dispose();
				}
			}
		}

		/// <summary>
		/// Show a toast to notify copy completed.
		/// </summary>
		private async Task ShowToastAsync()
		{
			if (!OsVersion.IsEightOrNewer || (_copyFileCount <= 0) || (DateTime.Now - CopyStartTime < _toastThresholdLength))
				return;

			var request = new ToastRequest
			{
				ToastHeadline = Resources.ToastHeadline_CopyCompleted,
				ToastBody = Resources.ToastBody_CopyCompleted,
				ToastBodyExtra = String.Format(Resources.ToastBodyExtra_CopyCompleted, _copyFileCount, (int)(DateTime.Now - CopyStartTime).TotalSeconds),
				ShortcutFileName = Properties.Settings.Default.ShortcutFileName,
				ShortcutTargetFilePath = Assembly.GetExecutingAssembly().Location,
				AppId = Properties.Settings.Default.AppId
			};

			var result = await ToastManager.ShowAsync(request);

			if (result == ToastResult.Activated)
				IsWindowActivateRequested = true; // Activating Window is requested.
		}

		#endregion

		#endregion


		#region Load & Save & Send

		private CancellationTokenSource _tokenSourceLoading;
		private bool _isTokenSourceLoadingDisposed;

		/// <summary>
		/// Load image data from a local file and set it to current image data.
		/// </summary>
		/// <param name="item">Target item</param>
		internal async Task LoadSetAsync(FileItemViewModel item)
		{
			if (!_isTokenSourceLoadingDisposed && (_tokenSourceLoading != null) && !_tokenSourceLoading.IsCancellationRequested)
			{
				try
				{
					_tokenSourceLoading.Cancel();
				}
				catch (ObjectDisposedException ode)
				{
					Debug.WriteLine("CancellationTokenSource has been disposed when tried to cancel operation. {0}", ode);
				}
			}

			try
			{
				_tokenSourceLoading = new CancellationTokenSource();
				_isTokenSourceLoadingDisposed = false;

				byte[] data = null;
				if (item.IsAvailableLocal && item.CanLoadDataLocal)
					data = await FileAddition.ReadAllBytesAsync(ComposeLocalPath(item), _tokenSourceLoading.Token);

				CurrentItem = item;
				CurrentImageData = data;
			}
			catch (OperationCanceledException)
			{
				// None.
			}
			catch (FileNotFoundException)
			{
				item.Status = FileStatus.NotCopied;
				item.IsAliveLocal = false;
			}
			catch (IOException)
			{
				item.IsAvailableLocal = false;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to load image data from local file. {0}", ex);
				throw new UnexpectedException("Failed to load image data from local file.", ex);
			}
			finally
			{
				if (_tokenSourceLoading != null)
				{
					_isTokenSourceLoadingDisposed = true;
					_tokenSourceLoading.Dispose();
				}
			}
		}

		/// <summary>
		/// Save current image data on desktop.
		/// </summary>
		internal async Task SaveDesktopAsync()
		{
			if ((CurrentImageData == null) || (CurrentItem == null))
				return;

			try
			{
				IsSavingDesktop = true;

				var filePath = ComposeDesktopPath(CurrentItem);

				using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite))
				{
					await fs.WriteAsync(CurrentImageData, 0, CurrentImageData.Length);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to save image data on desktop. {0}", ex);
			}
			finally
			{
				IsSavingDesktop = false;
			}
		}

		/// <summary>
		/// Send current image data to clipboard.
		/// </summary>
		internal async Task SendClipboardAsync()
		{
			if (CurrentImageData == null)
				return;

			try
			{
				IsSendingClipboard = true;

				var image = await ImageManager.ConvertBytesToBitmapSourceAsync(CurrentImageData);

				var tcs = new TaskCompletionSource<bool>();
				var thread = new Thread(() =>
				{
					try
					{
						Clipboard.SetImage(image);
						tcs.SetResult(true);
					}
					catch (Exception ex)
					{
						tcs.SetException(ex);
					}
				});
				thread.SetApartmentState(ApartmentState.STA); // Clipboard class must run in STA thread.
				thread.Start();

				await tcs.Task;
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to send image data to clipboard. {0}", ex);
			}
			finally
			{
				IsSendingClipboard = false;
			}
		}

		#endregion


		#region Helper

		private const string _unknownFolderName = "Unknown"; // Folder name for an item whose date or time is invalid

		/// <summary>
		/// Compose local file path to a specified local folder.
		/// </summary>
		/// <param name="item">Target item</param>
		/// <returns>Local file path</returns>
		private static string ComposeLocalPath(FileItemViewModel item)
		{
			var folderName = (item.Date != default(DateTime))
				? item.Date.ToString("yyyyMMdd")
				: _unknownFolderName;

			return Path.Combine(Settings.Current.LocalFolder, folderName, item.FileNameWithCaseExtension);
		}

		/// <summary>
		/// Compose local file path to desktop.
		/// </summary>
		/// <param name="item">Target item</param>
		/// <returns>Local file path</returns>
		private static string ComposeDesktopPath(FileItemViewModel item)
		{
			return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), item.FileNameWithCaseExtension);
		}

		/// <summary>
		/// Get FileInfo of a local file.
		/// </summary>
		/// <param name="item">Target item</param>
		/// <returns>FileInfo</returns>
		private static FileInfo GetFileInfoLocal(FileItemViewModel item)
		{
			var localPath = ComposeLocalPath(item);

			return File.Exists(localPath) ? new FileInfo(localPath) : null;
		}

		/// <summary>
		/// Check if a local file exists
		/// </summary>
		/// <param name="item">Target item</param>
		/// <returns>True if exists</returns>
		private static bool IsAliveLocal(FileItemViewModel item)
		{
			return IsAliveLocal(GetFileInfoLocal(item), item.Size);
		}

		/// <summary>
		/// Check if a local file exists.
		/// </summary>
		/// <param name="info">FileInfo of a local file</param>
		/// <param name="size">File size of a local file</param>
		/// <returns>True if exists</returns>
		private static bool IsAliveLocal(FileInfo info, int size)
		{
			return (info != null) && (info.Length == size);
		}

		/// <summary>
		/// Check if a local file is not offline in terms of OneDrive.
		/// </summary>
		/// <param name="info">FileInfo of a local file</param>
		/// <returns>True if not offline</returns>
		private static bool IsAvailableLocal(FileInfo info)
		{
			return (info != null) && !info.Attributes.HasFlag(FileAttributes.Offline);
		}

		#endregion
	}
}
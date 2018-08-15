using System;
using System.Collections.Concurrent;
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
	/// Operates.
	/// </summary>
	public class Operator : NotificationDisposableObject
	{
		private readonly MainWindowViewModel _mainWindowViewModel;
		private readonly FileManager _manager;

		public Operator(MainWindowViewModel mainWindowViewModel)
		{
			this._mainWindowViewModel = mainWindowViewModel;

			this._manager = new FileManager();
			Subscription.Add(_manager);

			if (!Designer.IsInDesignMode) // ListCollectionView may be null in Design mode.
			{
				FileListCoreView.CurrentChanged += (sender, e) => CheckFileListCoreViewThumbnail();
			}
		}

		#region Access to MainWindowViewModel

		private string OperationStatus
		{
			set => _mainWindowViewModel.OperationStatus = value;
		}

		private ItemObservableCollection<FileItemViewModel> FileListCore
		{
			get => _mainWindowViewModel.FileListCore;
		}

		private ListCollectionView FileListCoreView
		{
			get => _mainWindowViewModel.FileListCoreView;
		}

		private int FileListCoreViewIndex
		{
			set => _mainWindowViewModel.FileListCoreViewIndex = value;
		}

		private FileItemViewModel CurrentItem
		{
			get => _mainWindowViewModel.CurrentItem;
			set => _mainWindowViewModel.CurrentItem = value;
		}

		private byte[] CurrentImageData
		{
			get => _mainWindowViewModel.CurrentImageData;
			set => _mainWindowViewModel.CurrentImageData = value;
		}

		public event EventHandler ActivateRequested;

		private void InvokeSafely(Action action)
		{
			if (!Application.Current.Dispatcher.CheckAccess())
			{
				Application.Current.Dispatcher.Invoke(action);
			}
			else
			{
				action.Invoke();
			}
		}

		#endregion

		#region Constant

		/// <summary>
		/// Duration to show status in case of failure during auto check
		/// </summary>
		private readonly TimeSpan _autoCheckFailureDuration = TimeSpan.FromSeconds(5);

		/// <summary>
		/// The longest interval of actual checking during auto check
		/// </summary>
		private readonly TimeSpan _autoCheckLongestInterval = TimeSpan.FromMinutes(10);

		/// <summary>
		/// Waiting duration before showing completion of copying
		/// </summary>
		private readonly TimeSpan _copyWaitingDuration = TimeSpan.FromSeconds(0.2);

		/// <summary>
		/// The shortest duration of copying to show a toast notification after copying
		/// </summary>
		private readonly TimeSpan _copyToastShortestDuration = TimeSpan.FromSeconds(30);

		#endregion

		#region Operation state

		/// <summary>
		/// Whether checking files in FlashAir card
		/// </summary>
		public bool IsChecking
		{
			get => _isChecking;
			set => SetPropertyValue(ref _isChecking, value);
		}
		private bool _isChecking;

		/// <summary>
		/// Whether copying files from FlashAir card
		/// </summary>
		public bool IsCopying
		{
			get => _isCopying;
			set => SetPropertyValue(ref _isCopying, value);
		}
		private bool _isCopying;

		/// <summary>
		/// Whether running timer for auto check
		/// </summary>
		internal bool IsAutoRunning
		{
			get => _isAutoRunning;
			set => SetPropertyValue(ref _isAutoRunning, value);
		}
		private bool _isAutoRunning;

		/// <summary>
		/// Whether saving current image data on desktop
		/// </summary>
		internal bool IsSavingDesktop
		{
			get => _isSavingDesktop;
			set => SetPropertyValue(ref _isSavingDesktop, value);
		}
		private bool _isSavingDesktop;

		/// <summary>
		/// Whether sending current image data to clipboard
		/// </summary>
		internal bool IsSendingClipboard
		{
			get => _isSendingClipboard;
			set => SetPropertyValue(ref _isSendingClipboard, value);
		}
		private bool _isSendingClipboard;

		#endregion

		#region Progress

		/// <summary>
		/// Percentage of total size of items in target and copied so far including current operation
		/// </summary>
		public double ProgressCopiedAll
		{
			get => _progressCopiedAll;
			set => SetPropertyValue(ref _progressCopiedAll, value);
		}
		private double _progressCopiedAll = 40; // Sample percentage

		/// <summary>
		/// Percentage of total size of items in target and copied during current operation
		/// </summary>
		public double ProgressCopiedCurrent
		{
			get => _progressCopiedCurrent;
			set => SetPropertyValue(ref _progressCopiedCurrent, value);
		}
		private double _progressCopiedCurrent = 60; // Sample percentage

		/// <summary>
		/// Remaining time for current operation calculated by current transfer rate
		/// </summary>
		public TimeSpan RemainingTime
		{
			get => _remainingTime;
			set => SetPropertyValue(ref _remainingTime, value);
		}
		private TimeSpan _remainingTime;

		/// <summary>
		/// Progress state of current operation
		/// </summary>
		public TaskbarItemProgressState ProgressState
		{
			get => _progressState;
			set => SetPropertyValue(ref _progressState, value);
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
				var sizeCopiedLatest = (long)(info?.CurrentValue).GetValueOrDefault();

				ProgressCopiedAll = (double)(_sizeCopiedAll + sizeCopiedLatest) * 100D / (double)_sizeOverall;

				//Debug.WriteLine($"ProgressCopiedAll: {ProgressCopiedAll}");
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

				//Debug.WriteLine($"ProgressCopiedCurrent: {ProgressCopiedCurrent} RemainingTime: {RemainingTime}");
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
				await Task.Delay(_autoCheckFailureDuration);

			if (IsAutoRunning)
			{
				_autoTimer.Start();
				OperationStatus = Resources.OperationStatus_WaitingAutoCheck;
			}
		}

		/// <summary>
		/// Executes auto check by timer.
		/// </summary>
		/// <returns>False if failed</returns>
		private async Task<bool> ExecuteAutoCheckAsync()
		{
			if (_isFileListCoreViewThumbnailFilled &&
				(DateTime.Now < LastCheckCopyTime.Add(_autoCheckLongestInterval)))
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

		private CancellationTokenSourcePlus _tokenSourceWorking;

		private DateTime LastCheckCopyTime { get; set; }

		internal DateTime CopyStartTime { get; private set; }
		private int _copyFileCount;

		#region 1st layer

		/// <summary>
		/// Checks if content of FlashAir card is updated.
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
						? (await _manager.GetWriteTimeStampAsync(CancellationToken.None) != _card.WriteTimeStamp)
						: await _manager.CheckUpdateStatusAsync();

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
				if (ex is RemoteConnectionUnableException)
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
				}
				else if (ex is RemoteConnectionLostException)
				{
					OperationStatus = Resources.OperationStatus_ConnectionLost;
				}
				else if (ex is TimeoutException)
				{
					OperationStatus = Resources.OperationStatus_TimedOut;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_Error;
					Debug.WriteLine($"Failed to check if content is updated.\r\n{ex}");
					throw new UnexpectedException("Failed to check if content is updated.", ex);
				}
			}

			return null;
		}

		/// <summary>
		/// Checks & Copies files in FlashAir card.
		/// </summary>
		/// <returns>
		/// True:  If completed.
		/// False: If interrupted or failed.
		/// </returns>
		internal async Task<bool> CheckCopyFileAsync()
		{
			if (!(await CheckReadyAsync()))
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

				await Task.Delay(_copyWaitingDuration);
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

				if (ex is RemoteConnectionUnableException)
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
				}
				else if (ex is RemoteConnectionLostException)
				{
					OperationStatus = Resources.OperationStatus_ConnectionLost;
				}
				else if (ex is RemoteFileNotFoundException)
				{
					OperationStatus = Resources.OperationStatus_NotFolderFound;
				}
				else if (ex is TimeoutException)
				{
					OperationStatus = Resources.OperationStatus_TimedOut;
				}
				else if (ex is UnauthorizedAccessException)
				{
					OperationStatus = Resources.OperationStatus_LocalFolderUnauthorized;
				}
				else if (ex is CardChangedException)
				{
					OperationStatus = Resources.OperationStatus_NotSameFlashAir;
				}
				else if (ex is CardUploadDisabledException)
				{
					OperationStatus = Resources.OperationStatus_DeleteDisabled;
				}
				else if (ex is RemoteFileDeletionFailedException)
				{
					OperationStatus = Resources.OperationStatus_DeleteFailed;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_Error;
					Debug.WriteLine($"Failed to check & copy files.\r\n{ex}");
					throw new UnexpectedException("Failed to check & copy files.", ex);
				}
			}
			finally
			{
				// In case any exception is thrown.
				IsChecking = false;
				IsCopying = false;
			}

			return false;
		}

		/// <summary>
		/// Checks files in FlashAir card.
		/// </summary>
		internal async Task CheckFileAsync()
		{
			if (!(await CheckReadyAsync()))
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

				if (ex is RemoteConnectionUnableException)
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
				}
				else if (ex is RemoteConnectionLostException)
				{
					OperationStatus = Resources.OperationStatus_ConnectionLost;
				}
				else if (ex is RemoteFileNotFoundException)
				{
					OperationStatus = Resources.OperationStatus_NotFolderFound;
				}
				else if (ex is TimeoutException)
				{
					OperationStatus = Resources.OperationStatus_TimedOut;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_Error;
					Debug.WriteLine($"Failed to check files.\r\n{ex}");
					throw new UnexpectedException("Failed to check files.", ex);
				}
			}
			finally
			{
				IsChecking = false;
			}
		}

		/// <summary>
		/// Copies files from FlashAir card.
		/// </summary>
		internal async Task CopyFileAsync()
		{
			if (!(await CheckReadyAsync()))
				return;

			try
			{
				IsCopying = true;

				await CopyFileBaseAsync(new Progress<ProgressInfo>(UpdateProgress));

				await Task.Delay(_copyWaitingDuration);
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

				if (ex is RemoteConnectionUnableException)
				{
					OperationStatus = Resources.OperationStatus_ConnectionUnable;
				}
				else if (ex is RemoteConnectionLostException)
				{
					OperationStatus = Resources.OperationStatus_ConnectionLost;
				}
				else if (ex is TimeoutException)
				{
					OperationStatus = Resources.OperationStatus_TimedOut;
				}
				else if (ex is UnauthorizedAccessException)
				{
					OperationStatus = Resources.OperationStatus_LocalFolderUnauthorized;
				}
				else if (ex is CardChangedException)
				{
					OperationStatus = Resources.OperationStatus_NotSameFlashAir;
				}
				else if (ex is CardUploadDisabledException)
				{
					OperationStatus = Resources.OperationStatus_DeleteDisabled;
				}
				else if (ex is RemoteFileDeletionFailedException)
				{
					OperationStatus = Resources.OperationStatus_DeleteFailed;
				}
				else
				{
					OperationStatus = Resources.OperationStatus_Error;
					Debug.WriteLine($"Failed to copy files.\r\n{ex}");
					throw new UnexpectedException("Failed to copy files.", ex);
				}
			}
			finally
			{
				// In case any exception is thrown.
				IsCopying = false;
			}
		}

		/// <summary>
		/// Stops operation.
		/// </summary>
		internal void Stop()
		{
			StopAutoTimer();
			_tokenSourceWorking?.TryCancel(); // Canceling already canceled source will be simply ignored.
		}

		#endregion

		#region 2nd layer

		/// <summary>
		/// Checks if ready for operation.
		/// </summary>
		/// <returns>True if ready</returns>
		private async Task<bool> CheckReadyAsync()
		{
			if ((Settings.Current.TargetPeriod == FilePeriod.Select) &&
				!Settings.Current.TargetDates.Any())
			{
				OperationStatus = Resources.OperationStatus_NoDateCalender;
				return false;
			}

			if (!NetworkChecker.IsNetworkConnected())
			{
				OperationStatus = Resources.OperationStatus_NoNetwork;
				return false;
			}

			if (!(await Task.Run(() => Settings.Current.CheckLocalFolderValid())))
			{
				OperationStatus = Resources.OperationStatus_LocalFolderInvalid;
				return false;
			}

			return true;
		}

		/// <summary>
		/// Checks files in FlashAir card.
		/// </summary>
		private async Task CheckFileBaseAsync()
		{
			OperationStatus = Resources.OperationStatus_Checking;

			try
			{
				_tokenSourceWorking = new CancellationTokenSourcePlus();

				var prepareTask = Settings.Current.SkipsOnceCopiedFile
					? Signatures.PrepareAsync()
					: Task.FromResult(false); // Completed task

				var checkTask = Task.Run(async () =>
				{
					// Check firmware version.
					_card.FirmwareVersion = await _manager.GetFirmwareVersionAsync(_tokenSourceWorking.Token);

					// Check CID.
					if (_card.CanGetCid)
						_card.Cid = await _manager.GetCidAsync(_tokenSourceWorking.Token);

					// Check SSID and check if PC is connected to FlashAir card by a wireless connection.
					_card.Ssid = await _manager.GetSsidAsync(_tokenSourceWorking.Token);
					_card.IsWirelessConnected = NetworkChecker.IsWirelessNetworkConnected(_card.Ssid);

					// Get all items.
					var fileListNew = (await _manager.GetFileListRootAsync(_card, _tokenSourceWorking.Token))
						.Select(fileItem => new FileItemViewModel(fileItem))
						.ToList();
					fileListNew.Sort();

					// Record time stamp of write event.
					if (_card.CanGetWriteTimeStamp)
						_card.WriteTimeStamp = await _manager.GetWriteTimeStampAsync(_tokenSourceWorking.Token);

					// Check if any sample is in old items.
					var isSample = FileListCore.Any(x => x.Size == 0);

					// Check if FlashAir card is changed.
					var isChanged = _card.IsChanged ?? !(new HashSet<FileItemViewModel>(FileListCore).Overlaps(fileListNew));

					if (isSample || isChanged)
						FileListCore.Clear();

					// Check old items.
					foreach (var itemOld in FileListCore)
					{
						var itemSameIndex = fileListNew.IndexOf(x => x.Equals(itemOld));
						if (itemSameIndex >= 0)
						{
							itemOld.IsAliveRemote = true;
							itemOld.FileItem = fileListNew[itemSameIndex].FileItem;

							fileListNew.RemoveAt(itemSameIndex);
						}
						else
						{
							itemOld.IsAliveRemote = false;
						}
					}

					// Add new items (This operation may be heavy).
					var isLeadOff = true;
					foreach (var itemNew in fileListNew)
					{
						if (isLeadOff)
						{
							InvokeSafely(() => FileListCoreViewIndex = FileListCoreView.IndexOf(itemNew));
							isLeadOff = false;
						}

						itemNew.IsAliveRemote = true;

						FileListCore.Insert(itemNew); // Customized Insert method
					}
				}, _tokenSourceWorking.Token);

				await Task.WhenAll(prepareTask, checkTask);

				await Task.Run(() =>
				{
					// Check all local files.
					foreach (var item in FileListCore)
					{
						var info = GetFileInfoLocal(item);
						item.IsAliveLocal = IsAliveLocal(info, item.Size);
						item.IsAvailableLocal = IsAvailableLocal(info);

						if (Settings.Current.SkipsOnceCopiedFile)
						{
							if (item.IsOnceCopied == null)
							{
								if (item.IsAliveLocal)
								{
									item.IsOnceCopied = true;
									Signatures.Append(item.Signature);
								}
								else
								{
									item.IsOnceCopied = Signatures.Contains(item.Signature);
								}
							}
							// Maintain current value.
						}
						else
						{
							item.IsOnceCopied = null; // Reset value.
						}

						item.Status = item.IsAliveLocal
							? FileStatus.Copied
							: (item.IsOnceCopied == true)
								? FileStatus.OnceCopied
								: FileStatus.NotCopied;
					}

					// Manage deleted items.
					var itemDeletedPairs = FileListCore
						.Select((x, Index) => !x.IsAliveRemote ? new { Item = x, Index } : null)
						.Where(x => x != null)
						.OrderByDescending(x => x.Index)
						.ToArray();

					foreach (var itemDeletedPair in itemDeletedPairs)
					{
						var item = itemDeletedPair.Item;

						if (Settings.Current.MovesFileToRecycle &&
							item.IsDescendant &&
							(item.Status == FileStatus.Copied))
						{
							try
							{
								Recycle.MoveToRecycle(ComposeLocalPath(item));
								item.Status = FileStatus.Recycled;
							}
							catch (Exception ex)
							{
								Debug.WriteLine($"Failed to move a file to Recycle.\r\n{ex}");
								item.Status = (item.IsOnceCopied == true)
									? FileStatus.OnceCopied
									: FileStatus.NotCopied;
							}
						}

						if (!item.IsDescendant ||
							((item.Status != FileStatus.Copied) && (item.Status != FileStatus.Recycled)))
						{
							FileListCore.RemoveAt(itemDeletedPair.Index);
						}
					}
				});

				// Fill thumbnails.
				var itemsDueLocal = new List<FileItemViewModel>(FileListCore.Count);
				using (var itemsDueRemote = new BlockingCollection<FileItemViewModel>(FileListCore.Count))
				{
					foreach (var item in FileListCore.Where(x => x.IsTarget && !x.HasThumbnail))
					{
						if (item.IsAliveLocal && item.IsAvailableLocal && item.CanLoadDataLocal)
						{
							itemsDueLocal.Add(item);
						}
						else if (item.IsAliveRemote && item.CanGetThumbnailRemote)
						{
							itemsDueRemote.Add(item);
						}
					}

					// Get thumbnails from local.
					var getLocalTask = Task.Run(async () =>
					{
						foreach (var item in itemsDueLocal)
						{
							_tokenSourceWorking.Token.ThrowIfCancellationRequested();

							try
							{
								if (item.CanReadExif)
									item.Thumbnail = await ImageManager.ReadThumbnailAsync(ComposeLocalPath(item));
								else if (item.CanLoadDataLocal)
									item.Thumbnail = await ImageManager.CreateThumbnailAsync(ComposeLocalPath(item));

							}
							catch (Exception ex)
							{
								if (ex is FileNotFoundException)
								{
									item.IsAliveLocal = false;
									item.Status = (item.IsOnceCopied == true)
										? FileStatus.OnceCopied
										: FileStatus.NotCopied;
								}
								else if (ex is IOException)
								{
									item.IsAvailableLocal = false;
								}
								else if (ex is ImageNotSupportedException)
								{
									item.CanLoadDataLocal = false;
								}

								if (item.IsAliveRemote && item.CanGetThumbnailRemote)
								{
									itemsDueRemote.Add(item);
								}
							}
						}
						itemsDueRemote.CompleteAdding();
					}, _tokenSourceWorking.Token);

					// Get thumbnails from remote.
					var getRemoteTask = Task.Run(async () =>
					{
						foreach (var item in itemsDueRemote.GetConsumingEnumerable(_tokenSourceWorking.Token))
						{
							_tokenSourceWorking.Token.ThrowIfCancellationRequested();

							try
							{
								item.Thumbnail = await _manager.GetThumbnailAsync(item.FilePath, _card, _tokenSourceWorking.Token);
							}
							catch (RemoteFileThumbnailFailedException)
							{
								item.CanGetThumbnailRemote = false;
							}
						}
					}, _tokenSourceWorking.Token);

					await Task.WhenAll(getLocalTask, getRemoteTask);
				}

				OperationStatus = Resources.OperationStatus_CheckCompleted;
			}
			finally
			{
				FileListCoreViewIndex = -1; // No selection
				_tokenSourceWorking?.Dispose();

				if (Settings.Current.SkipsOnceCopiedFile)
					await Signatures.FlushAsync();
			}
		}

		/// <summary>
		/// Checks files to be copied from FlashAir card.
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
		/// Copies files from FlashAir card.
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
				_tokenSourceWorking = new CancellationTokenSourcePlus();

				// Check CID.
				if (_card.CanGetCid)
				{
					if (await _manager.GetCidAsync(_tokenSourceWorking.Token) != _card.Cid)
						throw new CardChangedException();
				}

				// Check if upload.cgi is disabled.
				if (Settings.Current.DeleteOnCopy && _card.CanGetUpload)
				{
					_card.Upload = await _manager.GetUploadAsync(_tokenSourceWorking.Token);
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
						if (!string.IsNullOrEmpty(localDirectory) && !Directory.Exists(localDirectory))
							Directory.CreateDirectory(localDirectory);

						var data = await _manager.GetSaveFileAsync(item.FilePath, localPath, item.Size, item.Date, item.CanReadExif, progress, _card, _tokenSourceWorking.Token);

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
						await _manager.DeleteFileAsync(item.FilePath, _tokenSourceWorking.Token);
					}

					if (Settings.Current.SkipsOnceCopiedFile)
						Signatures.Append(item.Signature);
				}

				OperationStatus = string.Format(Resources.OperationStatus_CopyCompleted, _copyFileCount, (int)(DateTime.Now - CopyStartTime).TotalSeconds);
			}
			finally
			{
				FileListCoreViewIndex = -1; // No selection
				_tokenSourceWorking?.Dispose();

				if (Settings.Current.SkipsOnceCopiedFile)
					await Signatures.FlushAsync();
			}
		}

		/// <summary>
		/// Shows a toast to notify completion of copying.
		/// </summary>
		private async Task ShowToastAsync()
		{
			if (!OsVersion.IsEightOrNewer || (_copyFileCount <= 0) || (DateTime.Now - CopyStartTime < _copyToastShortestDuration))
				return;

			var request = new ToastRequest
			{
				ToastHeadline = Resources.ToastHeadline_CopyCompleted,
				ToastBody = Resources.ToastBody_CopyCompleted,
				ToastBodyExtra = string.Format(Resources.ToastBodyExtra_CopyCompleted, _copyFileCount, (int)(DateTime.Now - CopyStartTime).TotalSeconds),
				ShortcutFileName = Properties.Settings.Default.ShortcutFileName,
				ShortcutTargetFilePath = Assembly.GetExecutingAssembly().Location,
				AppId = Properties.Settings.Default.AppId
			};

			var result = await ToastManager.ShowAsync(request);

			if (result == ToastResult.Activated)
				ActivateRequested?.Invoke(this, EventArgs.Empty);
		}

		#endregion

		#endregion

		#region Load & Save & Send

		private CancellationTokenSourcePlus _tokenSourceLoading;

		/// <summary>
		/// Loads image data from a local file and set it to current image data.
		/// </summary>
		/// <param name="item">Target item</param>
		internal async Task LoadSetAsync(FileItemViewModel item)
		{
			_tokenSourceLoading?.TryCancel(); // Canceling already canceled source will be simply ignored.

			try
			{
				_tokenSourceLoading = new CancellationTokenSourcePlus();

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
				item.IsAliveLocal = false;
				item.Status = (item.IsOnceCopied == true)
					? FileStatus.OnceCopied
					: FileStatus.NotCopied;
			}
			catch (IOException)
			{
				item.IsAvailableLocal = false;
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load image data from local file.\r\n{ex}");
				throw new UnexpectedException("Failed to load image data from local file.", ex);
			}
			finally
			{
				_tokenSourceLoading?.Dispose();
			}
		}

		/// <summary>
		/// Saves current image data on desktop.
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
				Debug.WriteLine($"Failed to save image data on desktop.\r\n{ex}");
			}
			finally
			{
				IsSavingDesktop = false;
			}
		}

		/// <summary>
		/// Sends current image data to clipboard.
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
				Debug.WriteLine($"Failed to send image data to clipboard.\r\n{ex}");
			}
			finally
			{
				IsSendingClipboard = false;
			}
		}

		#endregion

		#region Helper

		private const string UnknownFolderName = "Unknown"; // Folder name for an item whose date or time is invalid

		/// <summary>
		/// Composes local file path to a specified local folder.
		/// </summary>
		/// <param name="item">Target item</param>
		/// <returns>Local file path</returns>
		private static string ComposeLocalPath(FileItemViewModel item)
		{
			var folderName = Settings.Current.CreatesDatedFolder
				? (item.Date != default(DateTime))
					? item.Date.ToString("yyyyMMdd")
					: UnknownFolderName
				: string.Empty;

			return Path.Combine(Settings.Current.LocalFolder, folderName, item.FileNameWithCaseExtension);
		}

		/// <summary>
		/// Composes local file path to desktop.
		/// </summary>
		/// <param name="item">Target item</param>
		/// <returns>Local file path</returns>
		private static string ComposeDesktopPath(FileItemViewModel item) =>
			Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), item.FileNameWithCaseExtension);

		/// <summary>
		/// Gets FileInfo of a local file.
		/// </summary>
		/// <param name="item">Target item</param>
		/// <returns>FileInfo</returns>
		private static FileInfo GetFileInfoLocal(FileItemViewModel item)
		{
			var localPath = ComposeLocalPath(item);

			return File.Exists(localPath) // File.Exists method is more robust than FileInfo constructor.
				? new FileInfo(localPath)
				: null;
		}

		/// <summary>
		/// Checks if a local file exists
		/// </summary>
		/// <param name="item">Target item</param>
		/// <returns>True if exists</returns>
		private static bool IsAliveLocal(FileItemViewModel item) =>
			IsAliveLocal(GetFileInfoLocal(item), item.Size);

		/// <summary>
		/// Checks if a local file exists.
		/// </summary>
		/// <param name="info">FileInfo of a local file</param>
		/// <param name="size">File size of a local file</param>
		/// <returns>True if exists</returns>
		private static bool IsAliveLocal(FileInfo info, int size) =>
			(info != null) && (info.Length == size);

		/// <summary>
		/// Checks if a local file is available (not offline) in relation to OneDrive.
		/// </summary>
		/// <param name="info">FileInfo of a local file</param>
		/// <returns>True if available</returns>
		private static bool IsAvailableLocal(FileInfo info) =>
			(info != null) && !info.Attributes.HasFlag(FileAttributes.Offline);

		#endregion
	}
}
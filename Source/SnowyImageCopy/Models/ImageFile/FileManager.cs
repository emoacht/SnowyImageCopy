using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

using SnowyImageCopy.Helper;
using SnowyImageCopy.Models.Card;
using SnowyImageCopy.Models.Exceptions;
using SnowyImageCopy.Models.Network;

namespace SnowyImageCopy.Models.ImageFile
{
	/// <summary>
	/// Manages FlashAir card and its content files.
	/// </summary>
	internal class FileManager : IDisposable
	{
		#region Constant

		/// <summary>
		/// Interval to monitor network connection during operation
		/// </summary>
		private static readonly TimeSpan _monitorInterval = TimeSpan.FromSeconds(2);

		/// <summary>
		/// Interval of retry
		/// </summary>
		private static readonly TimeSpan _retryInterval = TimeSpan.FromMilliseconds(500);

		/// <summary>
		/// The maximum count of retry
		/// </summary>
		private const int MaxRetryCount = 3;

		#endregion

		private readonly HttpClient _client;

		internal FileManager()
		{
			_client = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
		}

		private string _remoteRoot;
		private string _remoteDescendant;
		private TimeSpan _timeoutDuration;

		internal void Ensure((string remoteRoot, string remoteDescendant) remoteRootDescendant, int timeoutDuration)
		{
			if (string.IsNullOrEmpty(remoteRootDescendant.remoteRoot))
				throw new ArgumentNullException(nameof(remoteRootDescendant.remoteRoot));

			if (timeoutDuration <= 0)
				throw new ArgumentOutOfRangeException(nameof(timeoutDuration), timeoutDuration, "The value must be positive.");

			(this._remoteRoot, this._remoteDescendant) = remoteRootDescendant;
			this._timeoutDuration = TimeSpan.FromSeconds(timeoutDuration);
		}

		#region IDisposable

		private bool _disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				_client?.Dispose();
			}

			_disposed = true;
		}

		#endregion

		#region Method (Internal)

		/// <summary>
		/// Gets a list of all files recursively from root folder of FlashAir card.
		/// </summary>
		/// <param name="card">State of FlashAir card</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>File list</returns>
		internal async Task<IEnumerable<IFileItem>> GetFileListRootAsync(ICardState card, CancellationToken cancellationToken)
		{
			try
			{
				return await GetFileListAllAsync(_remoteDescendant, card, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				Debug.WriteLine("Failed to get all file list.");
				throw;
			}
		}

		/// <summary>
		/// Gets a list of all files recursively in a specified directory in FlashAir card.
		/// </summary>
		/// <param name="remoteDirectoryPath">Remote directory path</param>
		/// <param name="card">State of FlashAir card</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>File list</returns>
		/// <remarks>This method is part of parent method.</remarks>
		private async Task<List<IFileItem>> GetFileListAllAsync(string remoteDirectoryPath, ICardState card, CancellationToken cancellationToken)
		{
			var itemList = await GetFileListEachAsync(remoteDirectoryPath, card, cancellationToken).ConfigureAwait(false);

			for (int i = itemList.Count - 1; 0 <= i; i--)
			{
				var item = itemList[i];

				if (item.IsHidden || item.IsSystem || item.IsVolume ||
					item.IsFlashAirSystem)
				{
					itemList.RemoveAt(i);
					continue;
				}

				if (!item.IsDirectory)
				{
					if (!item.IsImageFile)
					{
						itemList.RemoveAt(i);
					}
					continue;
				}

				var path = item.FilePath;
				itemList.RemoveAt(i);
				itemList.AddRange(await GetFileListAllAsync(path, card, cancellationToken).ConfigureAwait(false));
			}
			return itemList;
		}

		/// <summary>
		/// Gets a list of files in a specified directory in FlashAir card.
		/// </summary>
		/// <param name="remoteDirectoryPath">Remote directory path</param>
		/// <param name="card">State of FlashAir card</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>File list</returns>
		/// <remarks>This method is part of parent method.</remarks>
		private async Task<List<IFileItem>> GetFileListEachAsync(string remoteDirectoryPath, ICardState card, CancellationToken cancellationToken)
		{
			var remotePath = ComposeRemotePath(FileManagerCommand.GetFileList, remoteDirectoryPath);

			var fileEntries = await DownloadStringAsync(remotePath, card, cancellationToken).ConfigureAwait(false);

			return fileEntries.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select<string, IFileItem>(fileEntry => new FileItem(fileEntry, remoteDirectoryPath))
				.Where(x => x.IsImported)
				.ToList();
		}

		/// <summary>
		/// Gets a list of files in a specified directory in FlashAir card.
		/// </summary>
		/// <param name="remoteDirectoryPath">Remote directory path</param>
		/// <param name="card">State of FlashAir card</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>File list</returns>
		/// <remarks>This method is not actually used.</remarks>
		internal async Task<IEnumerable<IFileItem>> GetFileListAsync(string remoteDirectoryPath, ICardState card, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(remoteDirectoryPath))
				throw new ArgumentNullException(nameof(remoteDirectoryPath));

			var remotePath = ComposeRemotePath(FileManagerCommand.GetFileList, remoteDirectoryPath);

			try
			{
				var fileEntries = await DownloadStringAsync(remotePath, card, cancellationToken).ConfigureAwait(false);

				return fileEntries.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
					.Select<string, IFileItem>(fileEntry => new FileItem(fileEntry, remoteDirectoryPath))
					.Where(x => x.IsImported)
					.ToList();
			}
			catch
			{
				Debug.WriteLine("Failed to get file list.");
				throw;
			}
		}

		/// <summary>
		/// Gets the number of files in a specified directory in FlashAir card.
		/// </summary>
		/// <param name="remoteDirectoryPath">Remote directory path</param>
		/// <param name="card">State of FlashAir card</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>The number of files</returns>
		/// <remarks>This method is not actually used.</remarks>
		internal async Task<int> GetFileNumAsync(string remoteDirectoryPath, ICardState card, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(remoteDirectoryPath))
				throw new ArgumentNullException(nameof(remoteDirectoryPath));

			var remotePath = ComposeRemotePath(FileManagerCommand.GetFileNum, remoteDirectoryPath);

			try
			{
				var fileNum = await DownloadStringAsync(remotePath, card, cancellationToken).ConfigureAwait(false);

				return int.TryParse(fileNum, out var num) ? num : 0;
			}
			catch
			{
				Debug.WriteLine("Failed to get the number of files.");
				throw;
			}
		}

		/// <summary>
		/// Gets a thumbnail of a specified image file in FlashAir card.
		/// </summary>
		/// <param name="remoteFilePath">Remote file path</param>
		/// <param name="card">State of FlashAir card</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Thumbnail of image file</returns>
		internal async Task<BitmapSource> GetThumbnailAsync(string remoteFilePath, ICardState card, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(remoteFilePath))
				throw new ArgumentNullException(nameof(remoteFilePath));

			var remotePath = ComposeRemotePath(FileManagerCommand.GetThumbnail, remoteFilePath);

			try
			{
				var bytes = await DownloadBytesAsync(remotePath, card, cancellationToken).ConfigureAwait(false);

				return await ImageManager.ConvertBytesToBitmapSourceAsync(bytes).ConfigureAwait(false);
			}
			catch (ImageNotSupportedException)
			{
				// This exception should not be thrown because thumbnail data is directly provided by FlashAir card.
				return null;
			}
			catch (RemoteFileNotFoundException)
			{
				// If the format of image file is not JPEG or if there is no Exif standardized thumbnail stored,
				// StatusCode will be HttpStatusCode.NotFound.
				throw new RemoteFileThumbnailFailedException("Image file is not JPEG format or does not contain standardized thumbnail.", remotePath);
			}
			catch (RemoteConnectionUnableException rcue) when (rcue.Code == HttpStatusCode.InternalServerError)
			{
				// If image file is non-standard JPEG format, StatusCode may be HttpStatusCode.InternalServerError.
				throw new RemoteFileThumbnailFailedException("Image file is non-standard JPEG format.", remotePath);
			}
			catch
			{
				Debug.WriteLine("Failed to get a thumbnail.");
				throw;
			}
		}

		/// <summary>
		/// Gets file data of a specified remote file in FlashAir card and save it in local folder.
		/// </summary>
		/// <param name="remoteFilePath">Remote file path</param>
		/// <param name="localFilePath">Local file path</param> 
		/// <param name="size">File size provided by FlashAir card</param>
		/// <param name="itemDate">Date provided by FlashAir card</param>
		/// <param name="overwrite">Whether to overwrite existing file</param>
		/// <param name="readExif">Whether to read Exif metadata from file</param>
		/// <param name="progress">Progress</param>
		/// <param name="card">State of FlashAir card</param>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>Byte array of file</returns>
		internal async Task<byte[]> GetSaveFileAsync(string remoteFilePath, string localFilePath, int size, DateTime itemDate, bool overwrite, bool readExif, IProgress<ProgressInfo> progress, ICardState card, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(remoteFilePath))
				throw new ArgumentNullException(nameof(remoteFilePath));

			if (string.IsNullOrWhiteSpace(localFilePath))
				throw new ArgumentNullException(nameof(localFilePath));

			var remotePath = ComposeRemotePath(FileManagerCommand.None, remoteFilePath);
			byte[] bytes;

			try
			{
				bytes = await DownloadBytesAsync(remotePath, size, progress, card, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				Debug.WriteLine("Failed to get a file.");
				throw;
			}

			var mode = overwrite ? FileMode.Create : FileMode.CreateNew; // FileMode.CreateNew will cause IOException if a same name file exists.
			int retryCount = 0;

			while (true)
			{
				try
				{
					using (var fs = new FileStream(localFilePath, mode, FileAccess.Write, FileShare.ReadWrite))
					{
						var creationTime = itemDate;
						var lastWriteTime = itemDate;

						// Overwrite creation time by date of image taken from Exif metadata.
						if (readExif)
						{
							var exifDateTaken = await ImageManager.GetExifDateTakenAsync(bytes, DateTimeKind.Local).ConfigureAwait(false);
							if (exifDateTaken != default)
								creationTime = exifDateTaken;
						}

						FileTime.SetFileTime(fs.SafeFileHandle, creationTime: creationTime, lastWriteTime: lastWriteTime);

						await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
					}
					return bytes;
				}
				catch (IOException) when (overwrite && (++retryCount < MaxRetryCount))
				{
					// Wait interval before retry.
					if (TimeSpan.Zero < _retryInterval)
						await Task.Delay(_retryInterval, cancellationToken);
				}
				catch
				{
					Debug.WriteLine("Failed to save a file.");
					throw;
				}
			}
		}

		/// <summary>
		/// Deletes a specified remote file in FlashAir card.
		/// </summary>
		/// <param name="remoteFilePath">Remote file path</param>
		/// <param name="cancellationToken">Cancellation token</param>
		internal async Task DeleteFileAsync(string remoteFilePath, CancellationToken cancellationToken)
		{
			if (string.IsNullOrWhiteSpace(remoteFilePath))
				throw new ArgumentNullException(nameof(remoteFilePath));

			var remotePath = ComposeRemotePath(FileManagerCommand.DeleteFile, remoteFilePath);

			try
			{
				var result = await DownloadStringAsync(remotePath, null, cancellationToken).ConfigureAwait(false);

				// "SUCCESS": If succeeded.
				// "ERROR":   If failed.
				if (!result.Equals("SUCCESS", StringComparison.Ordinal))
					throw new RemoteFileDeletionFailedException(result, remotePath);
			}
			catch (RemoteFileNotFoundException)
			{
				// If upload.cgi is disabled, StatusCode will be HttpStatusCode.NotFound.
				throw new RemoteFileDeletionFailedException(null, remotePath);
			}
			catch
			{
				Debug.WriteLine("Failed to delete a remote file.");
				throw;
			}
		}

		/// <summary>
		/// Gets firmware version of FlashAir card.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <remarks>Firmware version</remarks>
		internal async Task<string> GetFirmwareVersionAsync(CancellationToken cancellationToken)
		{
			var remotePath = ComposeRemotePath(FileManagerCommand.GetFirmwareVersion, string.Empty);

			try
			{
				return await DownloadStringAsync(remotePath, null, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				Debug.WriteLine("Failed to get firmware version.");
				throw;
			}
		}

		/// <summary>
		/// Gets CID of FlashAir card.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>If succeeded, CID. If failed, empty string.</returns>
		internal async Task<string> GetCidAsync(CancellationToken cancellationToken)
		{
			var remotePath = ComposeRemotePath(FileManagerCommand.GetCid, string.Empty);

			try
			{
				return await DownloadStringAsync(remotePath, null, cancellationToken).ConfigureAwait(false);
			}
			catch (RemoteConnectionUnableException)
			{
				return string.Empty;
			}
			catch
			{
				Debug.WriteLine("Failed to get CID.");
				throw;
			}
		}

		/// <summary>
		/// Gets SSID of FlashAir card.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>SSID</returns>
		internal async Task<string> GetSsidAsync(CancellationToken cancellationToken)
		{
			var remotePath = ComposeRemotePath(FileManagerCommand.GetSsid, string.Empty);

			try
			{
				return await DownloadStringAsync(remotePath, null, cancellationToken).ConfigureAwait(false);
			}
			catch
			{
				Debug.WriteLine("Failed to get SSID.");
				throw;
			}
		}

		/// <summary>
		/// Gets free/total capacities of FlashAir card.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>
		/// <para>Tuple.Item1: Free capacity (bytes)</para>
		/// <para>Tuple.Item2: Total capacity (bytes)</para>
		/// </returns>
		internal async Task<Tuple<ulong, ulong>> GetCapacityAsync(CancellationToken cancellationToken)
		{
			var remotePath = ComposeRemotePath(FileManagerCommand.GetCapacity, string.Empty);

			try
			{
				var result = await DownloadStringAsync(remotePath, null, cancellationToken).ConfigureAwait(false);

				// [number of free sectors]/[number of total sectors],[sector size (bytes)]
				var num = result.Split(new[] { '/', ',' }, 3).Select(x => ulong.Parse(x)).ToArray();
				var freeSectors = num[0];
				var totalSectors = num[1];
				var sectorSize = num[2]; // 512

				return Tuple.Create(freeSectors * sectorSize, totalSectors * sectorSize);
			}
			catch
			{
				Debug.WriteLine("Failed to get capacities.");
				throw;
			}
		}

		/// <summary>
		/// Checks update status of FlashAir card.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>True if update status is set</returns>
		internal async Task<bool> CheckUpdateStatusAsync(CancellationToken cancellationToken)
		{
			var remotePath = ComposeRemotePath(FileManagerCommand.GetUpdateStatus, string.Empty);

			try
			{
				var status = await DownloadStringAsync(remotePath, null, cancellationToken).ConfigureAwait(false);

				// 1: If memory has been updated.
				// 0: If not.
				return (status == "1");
			}
			catch
			{
				Debug.WriteLine("Failed to check update status.");
				throw;
			}
		}

		/// <summary>
		/// Gets time stamp of write event in FlashAir card.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>If succeeded, time stamp (msec). If failed, -1.</returns>
		/// <remarks>If no write event occurred since FlashAir card started running, this value will be 0.</remarks>
		internal async Task<int> GetWriteTimeStampAsync(CancellationToken cancellationToken)
		{
			var remotePath = ComposeRemotePath(FileManagerCommand.GetWriteTimeStamp, string.Empty);

			try
			{
				var timeStamp = await DownloadStringAsync(remotePath, null, cancellationToken).ConfigureAwait(false);

				return int.TryParse(timeStamp, out var num) ? num : -1;
			}
			catch (RemoteConnectionUnableException)
			{
				// If request for time stamp of write event is not supported, StatusCode will be HttpStatusCode.BadRequest.
				return -1;
			}
			catch
			{
				Debug.WriteLine("Failed to get time stamp of write event.");
				throw;
			}
		}

		/// <summary>
		/// Gets Upload parameter of FlashAir card.
		/// </summary>
		/// <param name="cancellationToken">Cancellation token</param>
		/// <returns>If succeeded, Upload parameter. If failed, -1.</returns>
		internal async Task<int> GetUploadAsync(CancellationToken cancellationToken)
		{
			var remotePath = ComposeRemotePath(FileManagerCommand.GetUpload, string.Empty);

			try
			{
				var upload = await DownloadStringAsync(remotePath, null, cancellationToken).ConfigureAwait(false);

				return int.TryParse(upload, out var num) ? num : -1;
			}
			catch (RemoteConnectionUnableException)
			{
				// If request for Upload parameter is not supported, StatusCode will be HttpStatusCode.BadRequest.
				return -1;
			}
			catch
			{
				Debug.WriteLine("Failed to get Upload parameter.");
				throw;
			}
		}

		#endregion

		#region Method (Private)

		internal event EventHandler<(string request, string response)> Downloaded;

		private async Task<string> DownloadStringAsync(string path, ICardState card, CancellationToken cancellationToken)
		{
			var bytes = await DownloadBytesAsync(path, 0, null, card, cancellationToken).ConfigureAwait(false);

			// Response from FlashAir card seems to be encoded by ASCII.
			string buffer = null;
			Downloaded?.Invoke(this, (path, buffer = Encoding.ASCII.GetString(bytes)));

			return buffer ?? Encoding.ASCII.GetString(bytes);
		}

		private Task<byte[]> DownloadBytesAsync(string path, ICardState card, CancellationToken cancellationToken)
		{
			return DownloadBytesAsync(path, 0, null, card, cancellationToken);
		}

		private Task<byte[]> DownloadBytesAsync(string path, int size, ICardState card, CancellationToken cancellationToken)
		{
			return DownloadBytesAsync(path, size, null, card, cancellationToken);
		}

		private async Task<byte[]> DownloadBytesAsync(string path, int size, IProgress<ProgressInfo> progress, ICardState card, CancellationToken cancellationToken)
		{
			int retryCount = 0;

			using var monitorTokenSource = new CancellationTokenSource();

			// Start monitoring network connection.
			using var monitor = new NetworkMonitor(() =>
			{
				try
				{
					monitorTokenSource.Cancel();
				}
				catch (ObjectDisposedException)
				{ }
			}, card, _monitorInterval);

			bool isContentTimeout = false;

			async Task ReadAsync(Task readTask, Action abort)
			{
				using var contentTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
					cancellationToken,
					monitorTokenSource.Token);

				using var timeoutTask = Task.Delay(_timeoutDuration, contentTokenSource.Token);

				if (await Task.WhenAny(readTask, timeoutTask) == timeoutTask)
				{
					isContentTimeout = timeoutTask.IsCompleted;

					// Abort read task so that the task can be awaited in faulted status.
					abort.Invoke();
				}
				else
				{
					// Cancel timeout task so that the task can be disposed.
					contentTokenSource.Cancel();
				}
			}

			while (true)
			{
				try
				{
					try
					{
						using var headerTimeoutTokenSource = new CancellationTokenSource(_timeoutDuration);
						using var headerTokenSource = CancellationTokenSource.CreateLinkedTokenSource(
							cancellationToken,
							monitorTokenSource.Token,
							headerTimeoutTokenSource.Token);

						using var response = await _client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead, headerTokenSource.Token).ConfigureAwait(false);

						// If HttpResponseMessage.EnsureSuccessStatusCode is set, an exception by this setting
						// will be thrown in the scope of HttpClient and so cannot be caught in this method.
						switch (response.StatusCode)
						{
							case HttpStatusCode.OK:
								// None.
								break;
							case HttpStatusCode.Unauthorized:
							case HttpStatusCode.InternalServerError:
							case HttpStatusCode.BadRequest:
							case HttpStatusCode.ServiceUnavailable:
								throw new RemoteConnectionUnableException(response.StatusCode);
							case HttpStatusCode.NotFound:
								// This status code does not always mean that the specified file is missing.
								throw new RemoteFileNotFoundException("File is missing or request cannot be handled!", path);
							default:
								throw new HttpRequestException($"StatusCode: {response.StatusCode}");
						}

						if ((0 < size) &&
							(response.Content.Headers.ContentLength != size))
							throw new RemoteFileInvalidException("Data length does not match!", path);

						// Because of HttpCompletionOption.ResponseHeadersRead option, neither CancellationToken
						// nor HttpClient.Timeout setting works for response content.

						// Register delegate to CancellationToken because CancellationToken can no longer
						// directly affect HttpClient. Disposing the HttpResponseMessage will make ReadAsStreamAsync
						// method throw an ObjectDisposedException and so exit this operation.
						using var ctr = cancellationToken.Register(() => response.Dispose());

						if ((size == 0) || (progress is null))
						{
							// Route without progress reporting
							var readTask = response.Content.ReadAsByteArrayAsync();

							await ReadAsync(readTask, () => response.Dispose());

							// Await read task to get the result or rethrow exceptions on this thread.
							var bytes = await readTask;

							if ((0 < size) && (bytes.Length != size))
								throw new RemoteFileInvalidException("Data length does not match!", path);

							return bytes;
						}
						else
						{
							// Route with progress reporting
							int readLength;
							int readLengthTotal = 0;

							var buffer = new byte[65536]; // 64KiB
							var bufferTotal = new byte[size];

							const double stepUint = 524288D; // 512KiB
							double stepTotal = Math.Ceiling(size / stepUint); // The number of steps to report during downloading
							if (stepTotal < 6)
								stepTotal = 6; // The minimum number of steps

							double stepCurrent = 1D;
							var startTime = DateTime.Now;

							using var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

							while (readLengthTotal != size)
							{
								// CancellationToken in overload of ReadAsync method will not work for response content.
								var readTask = stream.ReadAsync(buffer, 0, buffer.Length);

								await ReadAsync(readTask, () => response.Dispose());

								// Await read task to get the result or rethrow exceptions on this thread.
								readLength = await readTask;

								if ((readLength == 0) || (readLengthTotal + readLength > size))
									throw new RemoteFileInvalidException("Data length does not match!", path);

								Buffer.BlockCopy(buffer, 0, bufferTotal, readLengthTotal, readLength);

								readLengthTotal += readLength;

								// Restart monitoring as the connection is confirmed by successful reading.
								monitor.Restart(_monitorInterval);

								// Report if read length in total exceeds stepped length.
								if (stepCurrent / stepTotal * size <= readLengthTotal)
								{
									progress.Report(new ProgressInfo(
										currentValue: readLengthTotal,
										totalValue: size,
										elapsedTime: DateTime.Now - startTime,
										isFirst: stepCurrent == 1D));

									stepCurrent++;
								}
							}
							return bufferTotal;
						}
					}
					catch (Exception ex) when (cancellationToken.IsCancellationRequested)
					{
						throw new OperationCanceledException("Reading canceled!", ex, cancellationToken);
					}
					catch (Exception ex) when (monitorTokenSource.IsCancellationRequested)
					{
						throw new RemoteConnectionLostException("Connection lost!", ex);
					}
					catch (Exception) when (isContentTimeout)
					{
						throw new TimeoutException("Reading response content timed out!");
					}
					catch (OperationCanceledException)
					{
						// OperationCanceledException includes the case of TaskCanceledException.
						// If this exception is not thrown by any other reason, the reason may be timeout
						// while reading response header.
						throw new TimeoutException("Reading response header timed out!");
					}
					catch (ObjectDisposedException ode)
					{
						// ObjectDisposedException can be thrown when essential objects, such as HttpClient,
						// CancellationTokenSource, NetworkStream, is unexpectedly disposed.
						// If this exception is not thrown by any other reason, the most possible reason
						// will be a lost of the connection to FlashAir card.
						throw new RemoteConnectionLostException("Connection lost!", ode);
					}
					catch (IOException ie) when (ie.InnerException is SocketException)
					{
						// If the connection to FlashAir card is changed, this exception may be thrown.
						// Message: A socket operation was attempted to an unreachable network.
						throw new RemoteConnectionLostException("Connection lost!", ie);
					}
					catch (HttpRequestException hre) when (hre.InnerException is ObjectDisposedException ode)
					{
						// If the connection to FlashAir card is lost, this exception may be thrown.
						// Message: Error while copying content to a stream.
						throw new RemoteConnectionLostException("Connection lost!", hre);
					}
					catch (HttpRequestException hre) when (hre.InnerException is WebException we)
					{
						// If unable to connect to FlashAir card, this exception will be thrown.
						// The status of response may vary, such as WebExceptionStatus.NameResolutionFailure,
						// WebExceptionStatus.ConnectFailure.
						throw new RemoteConnectionUnableException(we.Status);
					}
				}
				catch (RemoteConnectionUnableException) when (++retryCount < MaxRetryCount)
				{
					// Wait interval before retry.
					if (TimeSpan.Zero < _retryInterval)
						await Task.Delay(_retryInterval, cancellationToken);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to download byte array.\r\n{ex}");
					throw;
				}
			}
		}

		#endregion

		#region Remote path

		private static readonly IReadOnlyDictionary<FileManagerCommand, string> _commandMap =
			new Dictionary<FileManagerCommand, string>
			{
				{FileManagerCommand.None, string.Empty},
				{FileManagerCommand.GetFileList, @"command.cgi?op=100&DIR=/"},
				{FileManagerCommand.GetFileNum, @"command.cgi?op=101&DIR=/"},
				{FileManagerCommand.GetThumbnail, @"thumbnail.cgi?/"},
				{FileManagerCommand.GetFirmwareVersion, @"command.cgi?op=108"},
				{FileManagerCommand.GetCid, @"command.cgi?op=120"},
				{FileManagerCommand.GetSsid, @"command.cgi?op=104"},
				{FileManagerCommand.GetCapacity, @"command.cgi?op=140"},
				{FileManagerCommand.GetUpdateStatus, @"command.cgi?op=102"},
				{FileManagerCommand.GetWriteTimeStamp, @"command.cgi?op=121"},
				{FileManagerCommand.GetUpload, @"command.cgi?op=118"},
				{FileManagerCommand.DeleteFile, @"upload.cgi?DEL=/"},
			};

		/// <summary>
		/// Composes remote path in FlashAir card inserting CGI command string.
		/// </summary>
		/// <param name="command">CGI command type</param>
		/// <param name="remotePath">Source remote path</param>
		/// <returns>Outcome remote path</returns>
		private string ComposeRemotePath(FileManagerCommand command, string remotePath)
		{
			return string.Concat(_remoteRoot, _commandMap[command], remotePath.TrimStart('/'));
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Windows.Media.Editing;
using Windows.Storage;
using Windows.Storage.FileProperties;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Models
{
	public static class VideoManager
	{
		/// <summary>
		/// Gets the snapshot image at a specified time from start of playback.
		/// </summary>
		/// <param name="filePath">File path of video file</param>
		/// <param name="timeFromStart">Time from start of playback</param>
		/// <returns>BitmapImage</returns>
		public static ValueTask<BitmapImage> GetSnapshotImageAsync(string filePath, TimeSpan timeFromStart)
		{
			return GetSnapshotImageAsync(filePath, timeFromStart, new Size(0, 0));
		}

		/// <summary>
		/// Gets the snapshot image at a specified time from start of playback.
		/// </summary>
		/// <param name="filePath">File path of video file</param>
		/// <param name="timeFromStart">Time from start of playback</param>
		/// <param name="size">Size of snapshot image</param>
		/// <returns>BitmapImage</returns>
		public static async ValueTask<BitmapImage> GetSnapshotImageAsync(string filePath, TimeSpan timeFromStart, Size size)
		{
			if (!OsVersion.Is10OrGreater)
				return null;

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			if (timeFromStart < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(timeFromStart));

			var (stream, actualWidth, actualHeight) = await GetSnapshotStreamAsync(filePath, timeFromStart).ConfigureAwait(false);

			var ratio = Math.Min(size.Width / actualWidth, size.Height / actualHeight);
			if (ratio > 0)
			{
				(actualWidth, actualHeight) = ((int)(actualWidth * ratio), (int)(actualHeight * ratio));
			}

			return GetBitmapImageFromStream(stream, actualWidth, actualHeight);
		}

		/// <summary>
		/// Gets the byte array of snapshot image at a specified time from start of playback.
		/// </summary>
		/// <param name="filePath">File path of video file</param>
		/// <param name="timeFromStart">Time from start of playback</param>
		/// <param name="qualityLevel">Quality level of snapshot image in JPEG format</param>
		/// <returns>Byte array</returns>		
		public static async ValueTask<byte[]> GetSnapshotBytesAsync(string filePath, TimeSpan timeFromStart, int qualityLevel = 0)
		{
			if (!OsVersion.Is10OrGreater)
				return null;

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			if (timeFromStart < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(timeFromStart));

			var (stream, _, _) = await GetSnapshotStreamAsync(filePath, timeFromStart).ConfigureAwait(false);

			return GetJpegBytesFromStream(stream, qualityLevel);
		}

		private static async Task<(Stream stream, int width, int height)> GetThumbnailStreamAsync(string filePath)
		{
			var videoFile = await StorageFile.GetFileFromPathAsync(filePath);

			const string frameWidthName = "System.Video.FrameWidth";
			const string frameHeightName = "System.Video.FrameHeight";

			// Get video resolution.
			var frameProperties = await videoFile.Properties.RetrievePropertiesAsync(new[] { frameWidthName, frameHeightName });
			uint frameWidth = (uint)frameProperties[frameWidthName];
			uint frameHeight = (uint)frameProperties[frameHeightName];

			// Get the thumbnail. The time from start of playback varies depending on each video file.
			var thumbnail = await videoFile.GetThumbnailAsync(ThumbnailMode.VideosView);

			return (thumbnail.AsStream(), (int)frameWidth, (int)frameHeight);
		}

		private static async Task<(Stream stream, int width, int height)> GetSnapshotStreamAsync(string filePath, TimeSpan timeFromStart)
		{
			var videoFile = await StorageFile.GetFileFromPathAsync(filePath);

			const string frameWidthName = "System.Video.FrameWidth";
			const string frameHeightName = "System.Video.FrameHeight";

			// Get video resolution.
			var frameProperties = await videoFile.Properties.RetrievePropertiesAsync(new[] { frameWidthName, frameHeightName });
			uint frameWidth = (uint)frameProperties[frameWidthName];
			uint frameHeight = (uint)frameProperties[frameHeightName];

			// Use Windows.Media.Editing to get ImageStream.
			var clip = await MediaClip.CreateFromFileAsync(videoFile);
			var composition = new MediaComposition();
			composition.Clips.Add(clip);

			// Prevent time from passing end of playback.
			var timeEnd = composition.Duration - TimeSpan.FromMilliseconds(1);
			if (timeFromStart > timeEnd)
				timeFromStart = timeEnd;

			var imageStream = await composition.GetThumbnailAsync(timeFromStart, (int)frameWidth, (int)frameHeight, VideoFramePrecision.NearestFrame);

			return (imageStream.AsStream(), (int)frameWidth, (int)frameHeight);
		}

		private static BitmapImage GetBitmapImageFromStream(Stream stream, int width, int height)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var image = new BitmapImage();
			image.BeginInit();
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.StreamSource = stream;
			image.DecodePixelWidth = width;
			image.DecodePixelHeight = height;
			image.EndInit();
			image.Freeze();

			return image;
		}

		private static byte[] GetJpegBytesFromStream(Stream stream, int qualityLevel = 0)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var encoder = new JpegBitmapEncoder();

			if (qualityLevel > 0)
				encoder.QualityLevel = qualityLevel;

			encoder.Frames.Add(BitmapFrame.Create(stream));

			using var ms = new MemoryStream();
			encoder.Save(ms);
			return ms.ToArray();
		}

		private static void SaveJpegFileFromStream(Stream stream, string filePath, int qualityLevel = 0)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var encoder = new JpegBitmapEncoder();

			if (qualityLevel > 0)
				encoder.QualityLevel = qualityLevel;

			encoder.Frames.Add(BitmapFrame.Create(stream));

			using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
			encoder.Save(fileStream);
		}
	}
}
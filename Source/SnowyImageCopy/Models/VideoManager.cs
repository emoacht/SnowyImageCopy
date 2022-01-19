using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
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
		/// <param name="targetSize">Target size of image</param>
		/// <returns>BitmapImage</returns>
		public static async ValueTask<BitmapImage> GetSnapshotImageAsync(string filePath, TimeSpan timeFromStart, Size targetSize)
		{
			if (!OsVersion.Is10OrGreater)
				return null;

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			if (timeFromStart < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(timeFromStart));

			var (stream, actualWidth, actualHeight) = await GetSnapshotStreamAsync(filePath, timeFromStart).ConfigureAwait(false);

			var ratio = Math.Min(targetSize.Width / actualWidth, targetSize.Height / actualHeight);
			if (ratio > 0)
			{
				(actualWidth, actualHeight) = ((int)(actualWidth * ratio), (int)(actualHeight * ratio));
			}

			return ImageManager.ConvertStreamToBitmapImage(stream, new Size(actualWidth, actualHeight));
		}

		/// <summary>
		/// Creates the thumbnail iamge at a specified time from start of playback.
		/// </summary>
		/// <param name="filePath">File path of video file</param>
		/// <param name="timeFromStart">Time from start of playback</param>
		/// <returns>BitmapImage</returns>
		public static async ValueTask<BitmapImage> CreateThumbnailImageAsync(string filePath, TimeSpan timeFromStart)
		{
			if (!OsVersion.Is10OrGreater)
				return null;

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			if (timeFromStart < TimeSpan.Zero)
				throw new ArgumentOutOfRangeException(nameof(timeFromStart));

			var (stream, actualWidth, actualHeight) = await GetSnapshotStreamAsync(filePath, timeFromStart).ConfigureAwait(false);

			var thumbnailSize = ImageManager.ThumbnailSize;

			var ratio = Math.Min(thumbnailSize.Width / actualWidth, thumbnailSize.Height / actualHeight);
			if (ratio > 0)
			{
				(actualWidth, actualHeight) = ((int)(actualWidth * ratio), (int)(actualHeight * ratio));
			}

			var image = ImageManager.ConvertStreamToBitmapImage(stream, new Size(actualWidth, actualHeight));

			return ImageManager.CreateThumbnailFromImageUniform(image, thumbnailSize, StripeBrush.Value);
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

			return ImageManager.ConvertStreamToBytes(stream, qualityLevel);
		}

		private static async Task<(Stream stream, int width, int height)> GetThumbnailStreamAsync(string filePath)
		{
			var videoFile = await StorageFile.GetFileFromPathAsync(filePath);

			var (width, height) = await GetResolutionAsync(videoFile);

			// Get the thumbnail. The time from start of playback varies depending on each video file.
			var thumbnail = await videoFile.GetThumbnailAsync(ThumbnailMode.VideosView);

			return (thumbnail.AsStream(), width, height);
		}

		private static async Task<(Stream stream, int width, int height)> GetSnapshotStreamAsync(string filePath, TimeSpan timeFromStart)
		{
			var videoFile = await StorageFile.GetFileFromPathAsync(filePath);

			var (width, height) = await GetResolutionAsync(videoFile);

			// Use Windows.Media.Editing to get ImageStream.
			var clip = await MediaClip.CreateFromFileAsync(videoFile);
			var composition = new MediaComposition();
			composition.Clips.Add(clip);

			// Prevent time from passing end of playback.
			var timeEnd = composition.Duration - TimeSpan.FromMilliseconds(1);
			if (timeFromStart > timeEnd)
				timeFromStart = timeEnd;

			var imageStream = await composition.GetThumbnailAsync(timeFromStart, width, height, VideoFramePrecision.NearestFrame);

			return (imageStream.AsStream(), width, height);
		}

		private static async Task<(int width, int height)> GetResolutionAsync(StorageFile videoFile)
		{
			const string frameWidthName = "System.Video.FrameWidth";
			const string frameHeightName = "System.Video.FrameHeight";

			// Get video resolution.
			var frameProperties = await videoFile.Properties.RetrievePropertiesAsync(new[] { frameWidthName, frameHeightName });
			uint frameWidth = (uint)frameProperties[frameWidthName];
			uint frameHeight = (uint)frameProperties[frameHeightName];

			return ((int)frameWidth, (int)frameHeight);
		}

		#region Brush

		private static Lazy<Brush> StripeBrush => new(() => CreateStripeBrush(Brushes.Gray, Brushes.Black));

		/// <summary>
		/// Creates stripe Brush for rectangle { Width: 160, Height: 120 }.
		/// </summary>
		/// <param name="backgroundStripe">Brush for background stripe</param>
		/// <param name="foregroundStripe">Brush for foreground stripe</param>
		/// <returns>DrawingBrush</returns>
		/// <remarks>
		/// This stripe fits only to rectangle whose width is 160 and height is 120 in DPI.
		/// </remarks>
		private static Brush CreateStripeBrush(Brush backgroundStripe, Brush foregroundStripe)
		{
			var backgroundDrawing = new GeometryDrawing(backgroundStripe, default,
				new RectangleGeometry(new Rect(0, 0, 10, 12)));

			var foregroundDrawing = new GeometryDrawing(foregroundStripe, default,
				new PathGeometry(new[]
				{
					new PathFigure(
						new Point(0, 0),
						new[]
						{
							new LineSegment(new Point(0, 6), true),
							new LineSegment(new Point(5, 12), true),
							new LineSegment(new Point(10, 12), true),
						},
						true),
					new PathFigure(
						new Point(5, 0),
						new[]
						{
							new LineSegment(new Point(10, 6), true),
							new LineSegment(new Point(10, 0), true),
						},
						true),
				}));

			var drawingGroup = new DrawingGroup();
			drawingGroup.Children.Add(backgroundDrawing);
			drawingGroup.Children.Add(foregroundDrawing);

			var drawingBrush = new DrawingBrush(drawingGroup)
			{
				Stretch = Stretch.UniformToFill,
				TileMode = TileMode.Tile,
				Viewport = new Rect(0, 0, 40, 48),
				ViewportUnits = BrushMappingMode.Absolute,
			};
			drawingBrush.Freeze(); // To be considered.
			return drawingBrush;
		}

		#endregion
	}
}
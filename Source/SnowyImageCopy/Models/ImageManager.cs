using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using SnowyImageCopy.Models.Exceptions;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Reads/Creates/Converts images.
	/// </summary>
	internal static class ImageManager
	{
		#region Constant

		/// <summary>
		/// Size of Exif thumbnail
		/// </summary>
		public static readonly Size ThumbnailSize = new Size(160D, 120D);

		#endregion

		#region Read/Create Thumbnail (Internal)

		/// <summary>
		/// Reads a thumbnail in metadata from a specified file.
		/// </summary>
		/// <param name="localPath">Local file path</param>
		/// <returns>BitmapImage of thumbnail</returns>
		internal static async Task<BitmapImage> ReadThumbnailAsync(string localPath)
		{
			if (string.IsNullOrWhiteSpace(localPath))
				throw new ArgumentNullException(nameof(localPath));

			if (!File.Exists(localPath))
				throw new FileNotFoundException("File seems missing.", localPath);

			try
			{
				using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					return await Task.Run(() => ReadThumbnailFromExifByImaging(fs));
				}
			}
			catch (Exception ex) when (IsImageNotSupported(ex))
			{
				throw new ImageNotSupportedException();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to read a thumbnail.\r\n{ex}");
				throw;
			}
		}

		/// <summary>
		/// Reads a thumbnail in metadata from byte array.
		/// </summary>
		/// <param name="bytes">Byte array</param>
		/// <returns>BitmapImage of thumbnail</returns>
		internal static async Task<BitmapImage> ReadThumbnailAsync(byte[] bytes)
		{
			ThrowIfNullOrEmpty(bytes, nameof(bytes));

			try
			{
				using (var ms = new MemoryStream())
				{
					await ms.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

					// If continued by ContinueWith, an exception will not be caught by try-catch block in debug mode.
					return ReadThumbnailFromExifByImaging(ms);
				}
			}
			catch (Exception ex) when (IsImageNotSupported(ex))
			{
				throw new ImageNotSupportedException();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to read a thumbnail.\r\n{ex}");
				throw;
			}
		}

		/// <summary>
		/// Creates a thumbnail from a specified file.
		/// </summary>
		/// <param name="localPath">Local file path</param>
		/// <returns>BitmapImage of thumbnail</returns>
		internal static async Task<BitmapImage> CreateThumbnailAsync(string localPath)
		{
			if (string.IsNullOrWhiteSpace(localPath))
				throw new ArgumentNullException(nameof(localPath));

			if (!File.Exists(localPath))
				throw new FileNotFoundException("File seems missing.", localPath);

			try
			{
				using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var ms = new MemoryStream())
				{
					await fs.CopyToAsync(ms).ConfigureAwait(false);

					// If continued by ContinueWith, an exception will not be caught by try-catch block in debug mode.
					return CreateThumbnailFromImageUniform(ms);
				}
			}
			catch (Exception ex) when (IsImageNotSupported(ex))
			{
				throw new ImageNotSupportedException();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to create a thumbnail.\r\n{ex}");
				throw;
			}
		}

		/// <summary>
		/// Creates a thumbnail from byte array.
		/// </summary>
		/// <param name="bytes">Byte array</param>
		/// <returns>BitmapImage of thumbnail</returns>
		internal static async Task<BitmapImage> CreateThumbnailAsync(byte[] bytes)
		{
			ThrowIfNullOrEmpty(bytes, nameof(bytes));

			try
			{
				using (var ms = new MemoryStream())
				{
					await ms.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

					// If continued by ContinueWith, an exception will not be caught by try-catch block in debug mode.
					return CreateThumbnailFromImageUniform(ms);
				}
			}
			catch (Exception ex) when (IsImageNotSupported(ex))
			{
				throw new ImageNotSupportedException();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to create a thumbnail.\r\n{ex}");
				throw;
			}
		}

		#endregion

		#region Read/Create Thumbnail (Private)

		/// <summary>
		/// Reads a thumbnail in metadata by System.Drawing.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>BitmapImage of thumbnail</returns>
		private static BitmapImage ReadThumbnailFromExifByDrawing(Stream stream)
		{
			const int thumbnailDataId = 0x501B; // Property ID for PropertyTagThumbnailData

			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			using (var drawingImage = System.Drawing.Image.FromStream(stream, false, false))
			{
				if (!drawingImage.PropertyIdList.Any(propertyId => propertyId == thumbnailDataId))
					return null;

				var propItem = drawingImage.GetPropertyItem(thumbnailDataId);

				using (var ms = new MemoryStream())
				{
					ms.Write(propItem.Value, 0, propItem.Value.Length);
					return ConvertStreamToBitmapImage(ms);
				}
			}
		}

		/// <summary>
		/// Reads a thumbnail in metadata by System.Windows.Media.Imaging.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>BitmapImage of thumbnail</returns>
		private static BitmapImage ReadThumbnailFromExifByImaging(Stream stream)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var bitmapFrame = BitmapFrame.Create(stream, BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnDemand);
			var bitmapSource = bitmapFrame.Thumbnail;

			if (bitmapSource == null)
				return null;

			using (var ms = new MemoryStream())
			{
				var encoder = new JpegBitmapEncoder(); // Codec is to be considered.
				encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
				encoder.Save(ms);

				return ConvertStreamToBitmapImage(ms);
			}
		}

		/// <summary>
		/// Creates a thumbnail from image applying Uniform transformation.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>BitmapImage of thumbnail</returns>
		private static BitmapImage CreateThumbnailFromImageUniform(Stream stream)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var image = ConvertStreamToBitmapImageUniform(stream, ThumbnailSize);

			var dv = new DrawingVisual();
			using (var dc = dv.RenderOpen())
			{
				dc.DrawRectangle(Brushes.Black, null, new Rect(ThumbnailSize));
				dc.DrawImage(
					image,
					new Rect(
						(ThumbnailSize.Width - image.Width) / 2,
						(ThumbnailSize.Height - image.Height) / 2,
						image.Width,
						image.Height));
			}

			var rtb = new RenderTargetBitmap(
				(int)ThumbnailSize.Width, (int)ThumbnailSize.Height,
				96, 96,
				PixelFormats.Pbgra32);

			rtb.Render(dv);

			using (var ms = new MemoryStream())
			{
				var encoder = new JpegBitmapEncoder(); // Codec is to be considered.
				encoder.Frames.Add(BitmapFrame.Create(rtb));
				encoder.Save(ms);

				return ConvertStreamToBitmapImage(ms);
			}
		}

		/// <summary>
		/// Creates a thumbnail from image applying UniformToFill transformation.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>BitmapImage of thumbnail</returns>
		private static BitmapImage CreateThumbnailFromImageUniformToFill(Stream stream)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var bitmapSource = (BitmapSource)BitmapFrame.Create(stream);
			CropBitmapSource(ref bitmapSource, ThumbnailSize);

			using (var ms = new MemoryStream())
			{
				var encoder = new JpegBitmapEncoder(); // Codec is to be considered.
				encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
				encoder.Save(ms);

				return ConvertStreamToBitmapImage(ms, ThumbnailSize);
			}
		}

		#endregion

		#region Convert FrameworkElement to BitmapImage

		/// <summary>
		/// Converts a FrameworkElement to BitmapImage.
		/// </summary>
		/// <param name="element">FrameworkElement</param>
		/// <returns>BitmapImage of FrameworkElement</returns>
		internal static BitmapImage ConvertFrameworkElementToBitmapImage(FrameworkElement element)
		{
			return ConvertFrameworkElementToBitmapImage(element, Size.Empty);
		}

		/// <summary>
		/// Converts a FrameworkElement to BitmapImage.
		/// </summary>
		/// <param name="element">FrameworkElement</param>
		/// <param name="targetWidth">Target width</param>
		/// <returns>BitmapImage of FrameworkElement</returns>
		internal static BitmapImage ConvertFrameworkElementToBitmapImage(FrameworkElement element, double targetWidth)
		{
			return ConvertFrameworkElementToBitmapImage(element, new Size(targetWidth, double.MaxValue));
		}

		/// <summary>
		/// Converts a FrameworkElement to BitmapImage.
		/// </summary>
		/// <param name="element">FrameworkElement</param>
		/// <param name="outerSize">Target outer size</param>
		/// <returns>BitmapImage of FrameworkElement</returns>
		/// <remarks>
		/// A FrameworkElement instantiated by UI thread cannot be accessed by sub thread.
		/// So, there is not much merit in making asynchronous version.
		/// </remarks>
		internal static BitmapImage ConvertFrameworkElementToBitmapImage(FrameworkElement element, Size outerSize)
		{
			if (double.IsNaN(element.Width) || (element.Width <= 0) ||
				double.IsNaN(element.Height) || (element.Height <= 0))
				throw new ArgumentException("The element is invalid.", nameof(element));

			try
			{
				element.Arrange(new Rect(new Size(element.Width, element.Height))); // This is necessary to render a image.

				var target = (Visual)element;
				var targetSize = new Size(element.Width, element.Height);

				if (outerSize != Size.Empty)
				{
					targetSize = GetSizeUniform(element.Width, element.Height, outerSize);

					var dv = new DrawingVisual()
					{
						Transform = new ScaleTransform(
							targetSize.Width / element.Width,
							targetSize.Height / element.Height),
					};
					using (var dc = dv.RenderOpen())
					{
						dc.DrawRectangle(new VisualBrush(element), null, new Rect(new Size(element.Width, element.Height)));
					}

					target = dv;
				}

				//source.Arrange(new Rect(targetSize)); // This is necessary to render a image.

				var rtb = new RenderTargetBitmap(
					(int)targetSize.Width, (int)targetSize.Height,
					96, 96,
					PixelFormats.Pbgra32);

				rtb.Render(target);

				using (var ms = new MemoryStream())
				{
					var encoder = new PngBitmapEncoder(); // PNG format is for transparent color.
					encoder.Frames.Add(BitmapFrame.Create(rtb));
					encoder.Save(ms);

					return ConvertStreamToBitmapImage(ms);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to convert a FrameworkElement to BitmapImage.\r\n{ex}");
				throw;
			}
		}

		#endregion

		#region Convert byte array to BitmapSource

		/// <summary>
		/// Converts byte array to BitmapSource.
		/// </summary>
		/// <param name="bytes">Byte array</param>
		/// <returns>BitmapSource</returns>
		internal static Task<BitmapSource> ConvertBytesToBitmapSourceAsync(byte[] bytes)
		{
			return ConvertBytesToBitmapSourceAsync(bytes, 0D, 0D, false);
		}

		/// <summary>
		/// Converts byte array to BitmapSource.
		/// </summary>
		/// <param name="bytes">Byte array</param>
		/// <param name="targetWidth">Target width</param>
		/// <param name="willReadExif">Whether Exif metadata will be read from byte array</param>
		/// <param name="destinationProfile">Destination color profile for color management</param>
		/// <returns>BitmapSource</returns>
		internal static Task<BitmapSource> ConvertBytesToBitmapSourceAsync(byte[] bytes, double targetWidth, bool willReadExif, ColorContext destinationProfile = null)
		{
			return ConvertBytesToBitmapSourceAsync(bytes, targetWidth, 0D, willReadExif, destinationProfile);
		}

		/// <summary>
		/// Converts byte array to BitmapSource.
		/// </summary>
		/// <param name="bytes">Byte array</param>
		/// <param name="targetWidth">Target width</param>
		/// <param name="targetHeight">Target height</param>
		/// <param name="willReadExif">Whether Exif metadata will be read from byte array</param>
		/// <param name="destinationProfile">Destination color profile for color management</param>
		/// <returns>BitmapSource</returns>
		internal static async Task<BitmapSource> ConvertBytesToBitmapSourceAsync(byte[] bytes, double targetWidth, double targetHeight, bool willReadExif, ColorContext destinationProfile = null)
		{
			ThrowIfNullOrEmpty(bytes, nameof(bytes));

			var targetSize = new Size(targetWidth, targetHeight);

			try
			{
				using (var ms = new MemoryStream())
				{
					await ms.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

					return await Task.Run(() =>
					{
						if (willReadExif || (destinationProfile != null))
							return ConvertStreamToBitmapSource(ms, targetSize, willReadExif, destinationProfile);
						else
							return ConvertStreamToBitmapImage(ms, targetSize);
					});
				}
			}
			catch (Exception ex) when (IsImageNotSupported(ex))
			{
				throw new ImageNotSupportedException();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to convert byte array to BitmapSource.\r\n{ex}");
				throw;
			}
		}

		/// <summary>
		/// Converts byte array to BitmapSource applying Uniform transformation.
		/// </summary>
		/// <param name="bytes">Byte array</param>
		/// <param name="outerSize">Target outer size</param>
		/// <param name="willReadExif">Whether Exif metadata will be read from byte array</param>
		/// <param name="destinationProfile">Destination color profile for color management</param>
		/// <returns>BitmapSource</returns>
		internal static async Task<BitmapSource> ConvertBytesToBitmapSourceUniformAsync(byte[] bytes, Size outerSize, bool willReadExif, ColorContext destinationProfile = null)
		{
			ThrowIfNullOrEmpty(bytes, nameof(bytes));

			try
			{
				using (var ms = new MemoryStream())
				{
					await ms.WriteAsync(bytes, 0, bytes.Length).ConfigureAwait(false);

					return await Task.Run(() =>
					{
						if (willReadExif || (destinationProfile != null))
							return ConvertStreamToBitmapSource(ms, outerSize, willReadExif, destinationProfile);
						else
							return ConvertStreamToBitmapImageUniform(ms, outerSize);
					});
				}
			}
			catch (Exception ex) when (IsImageNotSupported(ex))
			{
				throw new ImageNotSupportedException();
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to convert byte array to BitmapSource.\r\n{ex}");
				throw;
			}
		}

		/// <summary>
		/// Converts Stream to BitmapSource.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="outerSize">Target outer size</param>
		/// <param name="willReadExif">Whether Exif metadata will be read from Stream</param>
		/// <param name="destinationProfile">Destination color profile for color management</param>
		/// <returns>BitmapSource</returns>
		private static BitmapSource ConvertStreamToBitmapSource(Stream stream, Size outerSize, bool willReadExif, ColorContext destinationProfile)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var bitmapFrame = BitmapFrame.Create(
				stream,
				BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.PreservePixelFormat, // For color management
				BitmapCacheOption.OnLoad);

			var orientation = willReadExif ? GetExifOrientation(bitmapFrame) : 0; // 0 means invalid.

			var bitmapSource = ResizeAndReflectExifOrientation(bitmapFrame, outerSize, orientation);

			if (destinationProfile != null)
			{
				var sourceProfile = GetColorProfile(bitmapFrame);
				bitmapSource = ConvertColorProfile(bitmapSource, sourceProfile, destinationProfile);
			}

			bitmapSource.Freeze();

			return bitmapSource;
		}

		/// <summary>
		/// Resizes BitmapSource and reflect Exif orientation to BitmapSource.
		/// </summary>
		/// <param name="bitmapSource">Source BitmapSource</param>
		/// <param name="outerSize">Target outer size</param>
		/// <param name="orientation">Exif orientation</param>
		/// <returns>Outcome BitmapSource</returns>
		private static BitmapSource ResizeAndReflectExifOrientation(BitmapSource bitmapSource, Size outerSize, int orientation)
		{
			var transform = new TransformGroup();
			var centerX = bitmapSource.Width / 2D;
			var centerY = bitmapSource.Height / 2D;
			bool isRotatedRightAngle = false;

			// Reflect Exif orientation.
			switch (orientation)
			{
				case 0: // Invalid
				case 1: // Horizontal (normal)
					break;

				case 2: // Mirror horizontal
					transform.Children.Add(new ScaleTransform(-1, 1, centerX, centerY));
					break;
				case 3: // Rotate 180 clockwise
					transform.Children.Add(new RotateTransform(180D, centerX, centerY));
					break;
				case 4: // Mirror vertical
					transform.Children.Add(new ScaleTransform(1, -1, centerX, centerY));
					break;
				case 5: // Mirror horizontal and rotate 270 clockwise
					transform.Children.Add(new ScaleTransform(-1, 1, centerX, centerY));
					transform.Children.Add(new RotateTransform(270D, centerX, centerY));
					isRotatedRightAngle = true;
					break;
				case 6: // Rotate 90 clockwise
					transform.Children.Add(new RotateTransform(90D, centerX, centerY));
					isRotatedRightAngle = true;
					break;
				case 7: // Mirror horizontal and rotate 90 clockwise
					transform.Children.Add(new ScaleTransform(-1, 1, centerX, centerY));
					transform.Children.Add(new RotateTransform(90D, centerX, centerY));
					isRotatedRightAngle = true;
					break;
				case 8: // Rotate 270 clockwise
					transform.Children.Add(new RotateTransform(270D, centerX, centerY));
					isRotatedRightAngle = true;
					break;
			}

			// Resize.
			if ((0 < bitmapSource.Width) && (0 < bitmapSource.Height)) // For just in case
			{
				var factor = new[]
					{
						(outerSize.Width / (isRotatedRightAngle ? bitmapSource.Height : bitmapSource.Width)), // Scale factor of X
						(outerSize.Height / (isRotatedRightAngle ? bitmapSource.Width : bitmapSource.Height)) // Scale factor of Y
					}
					.Where(x => 0 < x)
					.DefaultIfEmpty(1D)
					.Min();

				transform.Children.Add(new ScaleTransform(factor, factor, centerX, centerY));
			}

			var bitmapTransformed = new TransformedBitmap();
			bitmapTransformed.BeginInit();
			bitmapTransformed.Transform = transform;
			bitmapTransformed.Source = bitmapSource;
			bitmapTransformed.EndInit();

			return bitmapTransformed;
		}

		/// <summary>
		/// Gets color profile from BitmapFrame.
		/// </summary>
		/// <param name="bitmapFrame">BitmapFrame</param>
		/// <returns>Color profile</returns>
		private static ColorContext GetColorProfile(BitmapFrame bitmapFrame)
		{
			return (bitmapFrame.ColorContexts?.Count > 0)
				? bitmapFrame.ColorContexts.First()
				: new ColorContext(PixelFormats.Bgra32);
		}

		/// <summary>
		/// Converts color profile of BitmapSource for color management.
		/// </summary>
		/// <param name="bitmapSource">Source BitmapSource</param>
		/// <param name="sourceProfile">Source color profile</param>
		/// <param name="destinationProfile">Destination color profile</param>
		/// <returns>Outcome BitmapSource</returns>
		/// <remarks>
		/// Source color profile is color profile embedded in image file and destination color profile is
		/// color profile used by the monitor to which the Window belongs.
		/// </remarks>
		public static BitmapSource ConvertColorProfile(BitmapSource bitmapSource, ColorContext sourceProfile, ColorContext destinationProfile)
		{
			var bitmapConverted = new ColorConvertedBitmap();
			bitmapConverted.BeginInit();
			bitmapConverted.Source = bitmapSource;
			bitmapConverted.SourceColorContext = sourceProfile;
			bitmapConverted.DestinationColorContext = destinationProfile;
			bitmapConverted.DestinationFormat = PixelFormats.Bgra32;
			bitmapConverted.EndInit();

			return bitmapConverted;
		}

		#endregion

		#region Exif (Internal)

		/// <summary>
		/// Gets date of image taken in Exif metadata from byte array.
		/// </summary>
		/// <param name="bytes">Byte array</param>
		/// <returns>Date of image taken</returns>
		internal static async Task<DateTime> GetExifDateTakenAsync(byte[] bytes)
		{
			ThrowIfNullOrEmpty(bytes, nameof(bytes));

			using (var ms = new MemoryStream(bytes)) // This MemoryStream will not be changed.
			{
				try
				{
					return await Task.Run(() => GetExifDateTaken(BitmapFrame.Create(ms)));
				}
				catch (Exception ex) when (IsImageNotSupported(ex))
				{
					return default(DateTime);
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to get DateTaken from metadata.\r\n{ex}");
					throw;
				}
			}
		}

		/// <summary>
		/// Gets orientation in Exif metadata from byte array.
		/// </summary>
		/// <param name="bytes">Byte array</param>
		/// <returns>Orientation</returns>
		internal static async Task<int> GetExifOrientationAsync(byte[] bytes)
		{
			ThrowIfNullOrEmpty(bytes, nameof(bytes));

			using (var ms = new MemoryStream(bytes)) // This MemoryStream will not be changed.
			{
				try
				{
					return await Task.Run(() => GetExifOrientation(BitmapFrame.Create(ms)));
				}
				catch (Exception ex) when (IsImageNotSupported(ex))
				{
					return 0;
				}
				catch (Exception ex)
				{
					Debug.WriteLine($"Failed to get Orientation from metadata.\r\n{ex}");
					throw;
				}
			}
		}

		#endregion

		#region Exif (Private)

		/// <summary>
		/// Gets date of image taken in Exif metadata from BitmapFrame.
		/// </summary>
		/// <param name="bitmapFrame">BitmapFrame</param>
		/// <returns>Date of image taken</returns>
		private static DateTime GetExifDateTaken(BitmapFrame bitmapFrame)
		{
			//const string queryDateTaken = "System.Photo.DateTaken";

			return (bitmapFrame.Metadata is BitmapMetadata metadata)
				&& DateTime.TryParse(metadata.DateTaken, out DateTime dateTaken)
				? dateTaken
				: default(DateTime);
		}

		/// <summary>
		/// Gets orientation in Exif metadata from BitmapFrame.
		/// </summary>
		/// <param name="bitmapFrame">BitmapFrame</param>
		/// <returns>Orientation</returns>
		private static int GetExifOrientation(BitmapFrame bitmapFrame)
		{
			const string queryOrientation = "System.Photo.Orientation";

			return (bitmapFrame.Metadata is BitmapMetadata metadata)
				&& metadata.ContainsQuery(queryOrientation)
				&& (metadata.GetQuery(queryOrientation) is ushort orientation)
				? orientation // Orientation is defined as 16-bit unsigned integer in the specification.
				: 0;
		}

		#endregion

		#region General

		/// <summary>
		/// Converts stream to BitmapImage.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <returns>BitmapImage</returns>
		private static BitmapImage ConvertStreamToBitmapImage(Stream stream)
		{
			return ConvertStreamToBitmapImage(stream, Size.Empty);
		}

		/// <summary>
		/// Converts stream to BitmapImage.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="targetSize">Target size</param>
		/// <returns>BitmapImage</returns>
		private static BitmapImage ConvertStreamToBitmapImage(Stream stream, Size targetSize)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var image = new BitmapImage();
			image.BeginInit();
			image.CacheOption = BitmapCacheOption.OnLoad;
			image.StreamSource = stream;

			// When either width or height is not specified, the original aspect ratio will be preserved.
			if (0 < targetSize.Width)
				image.DecodePixelWidth = (int)targetSize.Width;
			if (0 < targetSize.Height)
				image.DecodePixelHeight = (int)targetSize.Height;

			image.EndInit();
			image.Freeze(); // This is necessary for other thread to use the image.

			return image;
		}

		/// <summary>
		/// Converts stream to BitmapImage applying Uniform transformation.
		/// </summary>
		/// <param name="stream">Stream</param>
		/// <param name="outerSize">Target outer size</param>
		/// <returns>BitmapImage</returns>
		private static BitmapImage ConvertStreamToBitmapImageUniform(Stream stream, Size outerSize)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var targetSize = Size.Empty;
			if (outerSize != Size.Empty)
			{
				var bitmapFrame = BitmapFrame.Create(stream);
				targetSize = GetSizeUniform(bitmapFrame.Width, bitmapFrame.Height, outerSize);
			}

			return ConvertStreamToBitmapImage(stream, targetSize);
		}

		/// <summary>
		/// Gets size of rectangle that inscribes outer rectangle for Uniform transformation.
		/// </summary>
		/// <param name="originWidth">Original image width</param>
		/// <param name="originHeight">Original image height</param>
		/// <param name="outerSize">Outer size</param>
		/// <returns>Size</returns>
		/// <remarks>Unit of original image width and height does not matter. Only the ratio matters.</remarks>
		private static Size GetSizeUniform(double originWidth, double originHeight, Size outerSize)
		{
			if ((originWidth <= 0) || (originHeight <= 0) || (outerSize == Size.Empty))
				return Size.Empty;

			var originRatio = originWidth / originHeight;
			var outerRatio = outerSize.Width / outerSize.Height;

			if (originRatio == outerRatio) // This comparison virtually will never hit because of precision loss.
			{
				return outerSize;
			}
			if (originRatio > outerRatio) // Origin is horizontally longer.
			{
				return new Size(outerSize.Width,
								outerSize.Width / originRatio);
			}
			else // Origin is vertically longer.
			{
				return new Size(outerSize.Height * originRatio,
								outerSize.Height);
			}
		}

		private static double GetScaleFactorUniform(double originWidth, double originHeight, Size outerSize)
		{
			var targetSize = GetSizeUniform(originWidth, originHeight, outerSize);

			if (targetSize == Size.Empty)
				return 1D;

			return targetSize.Width / originWidth;
		}

		/// <summary>
		/// Crops BitmapSource for UniformToFill transformation.
		/// </summary>
		/// <param name="bitmapSource">BitmapSource</param>
		/// <param name="innerSize">Inner size</param>
		private static void CropBitmapSource(ref BitmapSource bitmapSource, Size innerSize)
		{
			var sourceRect = GetRectCropped(bitmapSource.Width, bitmapSource.Height, innerSize);
			var bitmapCropped = new CroppedBitmap(bitmapSource, sourceRect);

			bitmapSource = bitmapCropped;
		}

		private static Int32Rect GetRectCropped(double originWidth, double originHeight, Size innerSize)
		{
			if ((originWidth <= 0) || (originHeight <= 0) || (innerSize == Size.Empty))
				return Int32Rect.Empty;

			var originRatio = originWidth / originHeight;
			var innerRatio = innerSize.Width / innerSize.Height;

			if (originRatio == innerRatio) // This comparison virtually will never hit because of precision loss.
			{
				return Int32Rect.Empty;
			}
			if (originRatio > innerRatio) // Origin is horizontally longer.
			{
				// Cut off left and right.
				var croppedWidth = originHeight * innerRatio;
				var croppedX = (originWidth - croppedWidth) / 2;

				return new Int32Rect((int)croppedX, 0, (int)croppedWidth, (int)originHeight);
			}
			else // Origin is vertically longer.
			{
				// Cut off top and bottom.
				var croppedHeight = originWidth / innerRatio;
				var croppedY = (originHeight - croppedHeight) / 2;

				return new Int32Rect(0, (int)croppedY, (int)originWidth, (int)croppedHeight);
			}
		}

		#endregion

		#region Helper

		private static void ThrowIfNullOrEmpty<T>(ICollection<T> source, string name)
		{
			if (!(source?.Count > 0))
				throw new ArgumentNullException(name);
		}

		/// <summary>
		/// Checks if an exception is thrown because image format is not supported by PC.
		/// </summary>
		/// <param name="ex">Exception</param>
		private static bool IsImageNotSupported(Exception ex)
		{
			if (ex is FileFormatException)
				return true;

			// Windows Imaging Component (WIC) defined error code
			// This description is "No imaging component suitable to complete this operation was found."
			const uint WINCODEC_ERR_COMPONENTNOTFOUND = 0x88982F50;

			if ((ex is NotSupportedException) &&
				(ex.InnerException is COMException) &&
				((uint)ex.InnerException.HResult == WINCODEC_ERR_COMPONENTNOTFOUND))
				return true;

			return false;
		}

		#endregion
	}
}
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

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Read/Create/Convert images.
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
		/// Read a thumbnail in metadata from a specified file.
		/// </summary>
		/// <param name="localPath">Local file path</param>
		internal static async Task<BitmapImage> ReadThumbnailAsync(string localPath)
		{
			if (String.IsNullOrEmpty(localPath))
				throw new ArgumentNullException("localPath");

			if (!File.Exists(localPath))
				throw new FileNotFoundException(localPath);

			try
			{
				using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					return await Task.Run(() => ReadThumbnailFromExifByImaging(fs));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to read a thumbnail. {0}", ex);

				if (IsImageFormatException(ex))
					return null;

				throw;
			}
		}

		/// <summary>
		/// Read a thumbnail in metadata from byte array.
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		internal static async Task<BitmapImage> ReadThumbnailAsync(byte[] bytes)
		{
			if ((bytes == null) || !bytes.Any())
				throw new ArgumentNullException("bytes");

			try
			{
				using (var ms = new MemoryStream())
				using (var writer = new BinaryWriter(ms))
				{
					writer.Write(bytes);

					return await Task.Run(() => ReadThumbnailFromExifByImaging(ms));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to read a thumbnail. {0}", ex);

				if (IsImageFormatException(ex))
					return null;

				throw;
			}
		}

		/// <summary>
		/// Create a thumbnail from a specified file.
		/// </summary>
		/// <param name="localPath">Local file path</param>
		internal static async Task<BitmapImage> CreateThumbnailAsync(string localPath)
		{
			if (String.IsNullOrEmpty(localPath))
				throw new ArgumentNullException("localPath");

			if (!File.Exists(localPath))
				throw new FileNotFoundException(localPath);

			try
			{
				using (var fs = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				using (var ms = new MemoryStream())
				{
					return await fs.CopyToAsync(ms)
						.ContinueWith(_ => CreateThumbnailFromImageUniform(ms))
						.ConfigureAwait(false);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to create a thumbnail. {0}", ex);

				if (IsImageFormatException(ex))
					return null;

				throw;
			}
		}

		/// <summary>
		/// Create a thumbnail from byte array.
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		internal static async Task<BitmapImage> CreateThumbnailAsync(byte[] bytes)
		{
			if ((bytes == null) || !bytes.Any())
				throw new ArgumentNullException("bytes");

			try
			{
				using (var ms = new MemoryStream())
				using (var writer = new BinaryWriter(ms))
				{
					writer.Write(bytes);

					return await Task.Run(() => CreateThumbnailFromImageUniform(ms));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to create a thumbnail. {0}", ex);

				if (IsImageFormatException(ex))
					return null;

				throw;
			}
		}

		#endregion


		#region Read/Create Thumbnail (Private)

		/// <summary>
		/// Read a thumbnail in metadata by System.Drawing.
		/// </summary>
		/// <param name="stream">Source stream</param>
		/// <returns>BitmapImage</returns>
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
				using (var writer = new BinaryWriter(ms))
				{
					writer.Write(propItem.Value);

					return ConvertStreamToBitmapImage(ms);
				}
			}
		}

		/// <summary>
		/// Read a thumbnail in metadata by System.Windows.Media.Imaging.
		/// </summary>
		/// <param name="stream">Source stream</param>
		/// <returns>BitmapImage</returns>
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
				var encoder = new JpegBitmapEncoder(); // Codec?
				encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
				encoder.Save(ms);

				return ConvertStreamToBitmapImage(ms);
			}
		}

		/// <summary>
		/// Create a thumbnail from image applying Uniform transformation.
		/// </summary>
		/// <param name="stream">Source stream</param>
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
				var encoder = new JpegBitmapEncoder(); // Codec?
				encoder.Frames.Add(BitmapFrame.Create(rtb));
				encoder.Save(ms);

				return ConvertStreamToBitmapImage(ms);
			}
		}

		/// <summary>
		/// Create a thumbnail from image applying UniformToFill transformation.
		/// </summary>
		/// <param name="stream">Source stream</param>
		private static BitmapImage CreateThumbnailFromImageUniformToFill(Stream stream)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var bitmapSource = (BitmapSource)BitmapFrame.Create(stream);
			CropBitmapSource(ref bitmapSource, ThumbnailSize);

			using (var ms = new MemoryStream())
			{
				var encoder = new JpegBitmapEncoder(); // Codec?
				encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
				encoder.Save(ms);

				return ConvertStreamToBitmapImage(ms, ThumbnailSize);
			}
		}

		#endregion


		#region Convert FrameworkElement to BitmapImage

		/// <summary>
		/// Convert a FrameworkElement to BitmapImage.
		/// </summary>
		/// <param name="element">Source FrameworkElement</param>
		internal static BitmapImage ConvertFrameworkElementToBitmapImage(FrameworkElement element)
		{
			return ConvertFrameworkElementToBitmapImage(element, Size.Empty);
		}

		/// <summary>
		/// Convert a FrameworkElement to BitmapImage.
		/// </summary>
		/// <param name="element">Source FrameworkElement</param>
		/// <param name="targetWidth">Target width</param>
		internal static BitmapImage ConvertFrameworkElementToBitmapImage(FrameworkElement element, double targetWidth)
		{
			return ConvertFrameworkElementToBitmapImage(element, new Size(targetWidth, double.MaxValue));
		}

		/// <summary>
		/// Convert a FrameworkElement to BitmapImage.
		/// </summary>
		/// <param name="element">Source FrameworkElement</param>
		/// <param name="outerSize">Target outer size</param>
		/// <remarks>A FrameworkElement instantiated by UI thread cannot be accessed by sub thread.
		/// So, there is not much merit in making asynchronous version.</remarks>
		internal static BitmapImage ConvertFrameworkElementToBitmapImage(FrameworkElement element, Size outerSize)
		{
			if (Double.IsNaN(element.Width) || (element.Width <= 0) ||
				Double.IsNaN(element.Height) || (element.Height <= 0))
				throw new ArgumentException("source");

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
				Debug.WriteLine("Failed to convert a FrameworkElement to BitmapImage. {0}", ex);
				throw;
			}
		}

		#endregion


		#region Convert byte array to BitmapImage

		/// <summary>
		/// Convert byte array to BitmapImage.
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		internal static async Task<BitmapImage> ConvertBytesToBitmapImageAsync(byte[] bytes)
		{
			return await ConvertBytesToBitmapImageAsync(bytes, 0D, 0D, false).ConfigureAwait(false);
		}

		/// <summary>
		/// Convert byte array to BitmapImage.
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		/// <param name="targetWidth">Target width</param>
		internal static async Task<BitmapImage> ConvertBytesToBitmapImageAsync(byte[] bytes, double targetWidth)
		{
			return await ConvertBytesToBitmapImageAsync(bytes, targetWidth, 0D, false).ConfigureAwait(false);
		}

		/// <summary>
		/// Convert byte array to BitmapImage.
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		/// <param name="targetWidth">Target width</param>
		/// <param name="willReadExif">Whether Exif metadata will be read from byte array</param>
		internal static async Task<BitmapImage> ConvertBytesToBitmapImageAsync(byte[] bytes, double targetWidth, bool willReadExif)
		{
			return await ConvertBytesToBitmapImageAsync(bytes, targetWidth, 0D, willReadExif).ConfigureAwait(false);
		}

		/// <summary>
		/// Convert byte array to BitmapImage.
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		/// <param name="targetWidth">Target width</param>
		/// <param name="targetHeight">Target Height</param>
		/// <param name="willReadExif">Whether Exif metadata will be read from byte array</param>
		internal static async Task<BitmapImage> ConvertBytesToBitmapImageAsync(byte[] bytes, double targetWidth, double targetHeight, bool willReadExif)
		{
			if ((bytes == null) || !bytes.Any())
				throw new ArgumentNullException("bytes");

			try
			{
				using (var ms = new MemoryStream())
				using (var writer = new BinaryWriter(ms))
				{
					writer.Write(bytes);

					if (willReadExif)
						await Task.Run(() => ReflectOrientationToStream(ms));

					return await Task.Run(() => ConvertStreamToBitmapImage(ms, new Size(targetWidth, targetHeight)));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to convert byte array to BitmapImage. {0}", ex);

				if (IsImageFormatException(ex))
					return null;

				throw;
			}
		}

		/// <summary>
		/// Convert byte array to BitmapImage applying Uniform transformation
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		/// <param name="outerSize">Target outer size</param>
		/// <param name="willReadExif">Whether Exif metadata will be read from byte array</param>
		internal static async Task<BitmapImage> ConvertBytesToBitmapImageUniformAsync(byte[] bytes, Size outerSize, bool willReadExif)
		{
			if ((bytes == null) || !bytes.Any())
				throw new ArgumentNullException("bytes");

			try
			{
				using (var ms = new MemoryStream())
				using (var writer = new BinaryWriter(ms))
				{
					writer.Write(bytes);

					if (willReadExif)
						await Task.Run(() => ReflectOrientationToStream(ms));

					return await Task.Run(() => ConvertStreamToBitmapImageUniform(ms, outerSize));
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to convert byte array to BitmapImage. {0}", ex);

				if (IsImageFormatException(ex))
					return null;

				throw;
			}
		}

		/// <summary>
		/// Reflect orientation in Exif metadata to image.
		/// </summary>
		/// <param name="stream">Target stream</param>
		private static void ReflectOrientationToStream(MemoryStream stream)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			var orientation = GetExifOrientation(stream);
			stream.Seek(0, SeekOrigin.Begin);

			var bitmapSource = (BitmapSource)BitmapFrame.Create(stream);
			ReflectOrientationToBitmapSource(ref bitmapSource, orientation);

			using (var ms = new MemoryStream())
			{
				var encoder = new JpegBitmapEncoder(); // Codec?
				encoder.Frames.Add(BitmapFrame.Create(bitmapSource));
				encoder.Save(ms);
				ms.Seek(0, SeekOrigin.Begin);

				// Copy to the original stream.
				stream.SetLength(0);
				ms.CopyTo(stream);
			}
		}

		#endregion


		#region Exif (Internal)

		/// <summary>
		/// Get Date of image taken from Exif metadata.
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		internal static async Task<DateTime> GetExifDateTakenAsync(byte[] bytes)
		{
			if ((bytes == null) || !bytes.Any())
				throw new ArgumentNullException("bytes");

			using (var ms = new MemoryStream(bytes)) // This MemoryStream will not be changed.
			{
				try
				{
					return await Task.Run(() => GetExifDateTaken(ms));
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Failed to get DateTaken from metadata. {0}", ex);

					if (IsImageFormatException(ex))
						return DateTime.MinValue;

					throw;
				}
			}
		}

		/// <summary>
		/// Get orientation in Exif metadata.
		/// </summary>
		/// <param name="bytes">Source byte array</param>
		internal static async Task<int> GetExifOrientationAsync(byte[] bytes)
		{
			if ((bytes == null) || !bytes.Any())
				throw new ArgumentNullException("bytes");

			using (var ms = new MemoryStream(bytes)) // This MemoryStream will not be changed.
			{
				try
				{
					return await Task.Run(() => GetExifOrientation(ms));
				}
				catch (Exception ex)
				{
					Debug.WriteLine("Failed to get Orientation from metadata. {0}", ex);

					if (IsImageFormatException(ex))
						return 0;

					throw;
				}
			}
		}

		#endregion


		#region Exif (Private)

		/// <summary>
		/// Get date of image taken in Exif metadata.
		/// </summary>
		/// <param name="stream">Source stream</param>
		private static DateTime GetExifDateTaken(Stream stream)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			//const string query_DateTaken = "System.Photo.DateTaken";

			var bitmapFrame = BitmapFrame.Create(stream);
			var bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;
			if (bitmapMetadata != null)
			{
				DateTime dateTaken;
				if (DateTime.TryParse(bitmapMetadata.DateTaken, out dateTaken))
				{
					//Debug.WriteLine("Exif DateTaken: {0}", dateTaken);
					return dateTaken;
				}
			}

			return DateTime.MinValue;
		}

		/// <summary>
		/// Get orientation in Exif metadata.
		/// </summary>
		/// <param name="stream">Source stream</param>
		private static int GetExifOrientation(Stream stream)
		{
			if (0 < stream.Position)
				stream.Seek(0, SeekOrigin.Begin);

			const string query_Orientation = "System.Photo.Orientation";

			var bitmapFrame = BitmapFrame.Create(stream);
			var bitmapMetadata = bitmapFrame.Metadata as BitmapMetadata;
			if (bitmapMetadata != null)
			{
				if (bitmapMetadata.ContainsQuery(query_Orientation))
				{
					var value = bitmapMetadata.GetQuery(query_Orientation);
					if (value != null)
					{
						int orientation;
						if (int.TryParse(value.ToString(), out orientation))
						{
							//Debug.WriteLine("Exif Orientation: {0}", orientation);
							return orientation;
						}
					}
				}
			}

			return 0;
		}

		/// <summary>
		/// Reflect orientation in Exif metadata to BitmapSource.
		/// </summary>
		/// <param name="bitmapSource">Target BitmapSource</param>
		/// <param name="orientation">Orientation in Exif metadata</param>
		private static void ReflectOrientationToBitmapSource(ref BitmapSource bitmapSource, int orientation)
		{
			// To reflect orientation:
			// 1 -> Horizontal (normal)
			// 2 -> Mirror horizontal
			// 3 -> Rotate 180 clockwise
			// 4 -> Mirror vertical
			// 5 -> Mirror horizontal and rotate 270 clockwise
			// 6 -> Rotate 90 clockwise
			// 7 -> Mirror horizontal and rotate 90 clockwise
			// 8 -> Rotate 270 clockwise

			switch (orientation)
			{
				case 0: // Invalid
				case 1: // Horizontal (normal)
					break;
				default:
					var transform = new TransformGroup();
					switch (orientation)
					{
						case 2: // Mirror horizontal
							transform.Children.Add(new ScaleTransform(-1, 1, bitmapSource.Width / 2, bitmapSource.Height / 2));
							break;
						case 3: // Rotate 180 clockwise
							transform.Children.Add(new RotateTransform(180D, bitmapSource.Width / 2, bitmapSource.Height / 2));
							break;
						case 4: // Mirror vertical
							transform.Children.Add(new ScaleTransform(1, -1, bitmapSource.Width / 2, bitmapSource.Height / 2));
							break;
						case 5: // Mirror horizontal and rotate 270 clockwise
							transform.Children.Add(new ScaleTransform(-1, 1, bitmapSource.Width / 2, bitmapSource.Height / 2));
							transform.Children.Add(new RotateTransform(270D, bitmapSource.Width / 2, bitmapSource.Height / 2));
							break;
						case 6: // Rotate 90 clockwise
							transform.Children.Add(new RotateTransform(90D, bitmapSource.Width / 2, bitmapSource.Height / 2));
							break;
						case 7: // Mirror horizontal and rotate 90 clockwise
							transform.Children.Add(new ScaleTransform(-1, 1, bitmapSource.Width / 2, bitmapSource.Height / 2));
							transform.Children.Add(new RotateTransform(90D, bitmapSource.Width / 2, bitmapSource.Height / 2));
							break;
						case 8: // Rotate 270 clockwise
							transform.Children.Add(new RotateTransform(270D, bitmapSource.Width / 2, bitmapSource.Height / 2));
							break;
					}

					var bitmapTransformed = new TransformedBitmap();
					bitmapTransformed.BeginInit();
					bitmapTransformed.Transform = transform;
					bitmapTransformed.Source = bitmapSource;
					bitmapTransformed.EndInit();

					bitmapSource = bitmapTransformed;
					break;
			}
		}

		#endregion


		#region General

		/// <summary>
		/// Convert stream to BitmapImage.
		/// </summary>
		/// <param name="stream">Source stream</param>
		private static BitmapImage ConvertStreamToBitmapImage(Stream stream)
		{
			return ConvertStreamToBitmapImage(stream, Size.Empty);
		}
				
		/// <summary>
		/// Convert stream to BitmapImage.
		/// </summary>
		/// <param name="stream">Source stream</param>
		/// <param name="targetSize">Target size</param>
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
		/// Convert stream to BitmapImage applying Uniform transformation.
		/// </summary>
		/// <param name="stream">Source stream</param>
		/// <param name="outerSize">Target outer size</param>
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
		/// Get size of rectangle that inscribes outer rectangle for Uniform transformation.
		/// </summary>
		/// <param name="originWidth">Original image width</param>
		/// <param name="originHeight">Original image height</param>
		/// <param name="outerSize">Target outer size</param>
		/// <remarks>Unit of original image width and height will not matter. Only the ratio matters.</remarks>
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
		/// Crop BitmapSource for UniformToFill transformation.
		/// </summary>
		/// <param name="bitmapSource">Target BitmapSource</param>
		/// <param name="innerSize">Target inner size</param>
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

		/// <summary>
		/// Check if an exception is thrown because image format is not compatible.
		/// </summary>
		/// <param name="ex">Target exception</param>
		private static bool IsImageFormatException(Exception ex)
		{
			if (ex.GetType() == typeof(FileFormatException))
				return true;

			// Windows Imaging Component (WIC) defined error code
			// The description is: No imaging component suitable to complete this operation was found.
			const uint WINCODEC_ERR_COMPONENTNOTFOUND = 0x88982F50;

			if (ex.GetType() == typeof(NotSupportedException))
			{
				var innerException = ex.InnerException;
				if ((innerException != null) &&
					(innerException.GetType() == typeof(COMException)) &&
					((uint)innerException.HResult == WINCODEC_ERR_COMPONENTNOTFOUND))
					return true;
			}

			return false;
		}

		#endregion
	}
}

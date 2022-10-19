using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Extension methods for <see cref="System.Windows.Media.Imaging.BitmapImage"/>
	/// </summary>
	public static class BitmapImageExtension
	{
		/// <summary>
		/// Converts a BitmapImage to a Bitmap.
		/// </summary>
		/// <param name="source">Source System.Windows.Media.Imaging.BitmapImage</param>
		/// <returns>Outcome System.Drawing.Bitmap</returns>
		/// <remarks>System.Drawing.Bitmap's format will be PixelFormat.Format32bppPArgb.</remarks>
		public static System.Drawing.Bitmap ToBitmap(this BitmapImage source)
		{
			return ToBitmap(source, PixelFormats.Bgra32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		}

		private static System.Drawing.Bitmap ToBitmap(BitmapSource source, PixelFormat format1, System.Drawing.Imaging.PixelFormat format2)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			BitmapSource input = source;
			if (source.Format != format1)
			{
				input = new FormatConvertedBitmap(
					source,
					format1,
					null,
					0);
				input.Freeze();
			}

			int width = input.PixelWidth;
			int height = input.PixelHeight;
			int stride = width * 4;
			var buffer = new byte[stride * height];
			input.CopyPixels(new Int32Rect(0, 0, width, height), buffer, stride, 0);

			var output = new System.Drawing.Bitmap(
				width,
				height,
				System.Drawing.Imaging.PixelFormat.Format32bppPArgb);

			try
			{
				System.Drawing.Imaging.BitmapData outputData = output.LockBits(
					new System.Drawing.Rectangle(0, 0, width, height),
					System.Drawing.Imaging.ImageLockMode.WriteOnly,
					format2);

				try
				{
					Marshal.Copy(buffer, 0, outputData.Scan0, buffer.Length);
					return output;
				}
				finally
				{
					output.UnlockBits(outputData);
				}
			}
			catch
			{
				output.Dispose();
				throw;
			}
		}

		/// <summary>
		/// Converts a BitmapImage to a Bitmap.
		/// </summary>
		/// <typeparam name="TEncoder">BitmapEncoder</typeparam>
		/// <param name="source">Source System.Windows.Media.Imaging.BitmapImage</param>
		/// <returns>Outcome System.Drawing.Bitmap</returns>
		public static System.Drawing.Bitmap ToBitmap<TEncoder>(this BitmapImage source) where TEncoder : BitmapEncoder, new()
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			using var ms = new MemoryStream();

			var encoder = new TEncoder();
			encoder.Frames.Add(BitmapFrame.Create(source));
			encoder.Save(ms);
			ms.Seek(0, SeekOrigin.Begin);

			return new System.Drawing.Bitmap(ms);
		}

		/// <summary>
		/// Saves a BitmapImage to a specified file.
		/// </summary>
		/// <typeparam name="TEncoder">BitmapEncoder</typeparam>
		/// <param name="source">BitmapImage</param>
		/// <param name="filePath">File path</param>
		public static void Save<TEncoder>(this BitmapImage source, string filePath) where TEncoder : BitmapEncoder, new()
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			if (string.IsNullOrEmpty(filePath))
				throw new ArgumentNullException(nameof(filePath));

			using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write);

			var encoder = new TEncoder();
			encoder.Frames.Add(BitmapFrame.Create(source));
			encoder.Save(fs);
		}

		/// <summary>
		/// Gets codec information from a BitmapImage.
		/// </summary>
		/// <typeparam name="TEncoder">BitmapEncoder</typeparam>
		/// <param name="source">BitmapImage</param>
		/// <returns>Codec information</returns>
		public static BitmapCodecInfo GetCodecInfo<TEncoder>(this BitmapImage source) where TEncoder : BitmapEncoder, new()
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			using var ms = new MemoryStream();

			var encoder = new TEncoder();
			encoder.Frames.Add(BitmapFrame.Create(source));
			encoder.Save(ms);
			ms.Seek(0, SeekOrigin.Begin);

			var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
			return decoder.CodecInfo;
		}
	}
}
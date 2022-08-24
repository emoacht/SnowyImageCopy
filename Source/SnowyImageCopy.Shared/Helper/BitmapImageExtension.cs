using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Extension method for <see cref="System.Windows.Media.Imaging.BitmapImage"/>
	/// </summary>
	public static class BitmapImageExtension
	{
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
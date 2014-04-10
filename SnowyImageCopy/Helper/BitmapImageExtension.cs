using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SnowyImageCopy.Helper
{
	public static class BitmapImageExtension
	{
		/// <summary>
		/// Convert a BitmapImage to a Bitmap 
		/// </summary>
		/// <typeparam name="T">BitmapEncoder</typeparam>
		/// <param name="source">Source System.Windows.Media.Imaging.BitmapImage</param>
		/// <returns>Outcome System.Drawing.Bitmap</returns>
		public static System.Drawing.Bitmap ToBitmap<T>(this BitmapImage source) where T : BitmapEncoder, new()
		{
			if (source == null)
				throw new ArgumentNullException("source");

			using (var ms = new MemoryStream())
			{
				var encoder = new T();
				encoder.Frames.Add(BitmapFrame.Create(source));
				encoder.Save(ms);
				ms.Seek(0, SeekOrigin.Begin);

				return new System.Drawing.Bitmap(System.Drawing.Bitmap.FromStream(ms));
			}
		}

		/// <summary>
		/// Save a BitmapImage to a specified file.
		/// </summary>
		/// <typeparam name="T">BitmapEncoder</typeparam>
		/// <param name="source">Source BitmapImage</param>
		/// <param name="filePath">Target file path</param>
		public static void Save<T>(this BitmapImage source, string filePath) where T : BitmapEncoder, new()
		{
			if (source == null)
				throw new ArgumentNullException("source");
			if (String.IsNullOrEmpty(filePath))
				throw new ArgumentNullException("filePath");

			using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write))
			{
				var encoder = new T();
				encoder.Frames.Add(BitmapFrame.Create(source));
				encoder.Save(fs);
			}
		}

		/// <summary>
		/// Get codec info from a BitmapImage.
		/// </summary>
		/// <typeparam name="T">BitmapEncoder</typeparam>
		/// <param name="source">Source BitmapImage</param>
		/// <returns>Codec info</returns>
		public static BitmapCodecInfo GetCodecInfo<T>(this BitmapImage source) where T : BitmapEncoder, new()
		{
			if (source == null)
				throw new ArgumentNullException("source");

			using (var ms = new MemoryStream())
			{
				var encoder = new T();
				encoder.Frames.Add(BitmapFrame.Create(source));
				encoder.Save(ms);
				ms.Seek(0, SeekOrigin.Begin);

				var decoder = BitmapDecoder.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.Default);
				return decoder.CodecInfo;
			}
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SnowyImageCopy.Helper
{
	public static class BitmapExtension
	{
		/// <summary>
		/// Convert a Bitmap to a BitmapImage.
		/// </summary>
		/// <param name="source">Source System.Drawing.Bitmap</param>
		/// <returns>Outcome System.Windows.Media.Imaging.BitmapImage</returns>
		public static BitmapImage ToBitmapImage(this System.Drawing.Bitmap source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			using (var ms = new MemoryStream())
			{
				source.Save(ms, source.RawFormat); // Bitmap.RawFormat is for formats other than default (PNG).
				ms.Seek(0, SeekOrigin.Begin);

				var image = new BitmapImage();
				image.BeginInit();
				image.CacheOption = BitmapCacheOption.OnLoad;
				image.StreamSource = ms;
				image.EndInit();
				image.Freeze();

				return image;
			}
		}
	}
}

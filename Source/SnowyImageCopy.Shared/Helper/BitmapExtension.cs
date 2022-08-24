using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Extension method for <see cref="System.Drawing.Bitmap"/>
	/// </summary>
	public static class BitmapExtension
	{
		#region Win32

		[DllImport("Gdi32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool DeleteObject(IntPtr hObject);

		#endregion

		/// <summary>
		/// Converts a Bitmap to a BitmapImage.
		/// </summary>
		/// <param name="source">Source System.Drawing.Bitmap</param>
		/// <returns>Outcome System.Windows.Media.Imaging.BitmapImage</returns>
		public static BitmapImage ToBitmapImage(this System.Drawing.Bitmap source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			using var ms = new MemoryStream();

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

		/// <summary>
		/// Converts a Bitmap to a BitmapSource.
		/// </summary>
		/// <param name="source">Source System.Drawing.Bitmap</param>
		/// <returns>Outcome System.Windows.Media.Imaging.BitmapSource</returns>
		public static BitmapSource ToBitmapSource(this System.Drawing.Bitmap source)
		{
			if (source is null)
				throw new ArgumentNullException(nameof(source));

			var handleBitmap = IntPtr.Zero;
			try
			{
				handleBitmap = source.GetHbitmap();

				return Imaging.CreateBitmapSourceFromHBitmap(
					handleBitmap,
					IntPtr.Zero,
					Int32Rect.Empty,
					BitmapSizeOptions.FromEmptyOptions());
			}
			finally
			{
				if (handleBitmap != IntPtr.Zero)
					DeleteObject(handleBitmap);
			}
		}
	}
}
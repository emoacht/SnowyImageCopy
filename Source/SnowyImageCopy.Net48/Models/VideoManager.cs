using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace SnowyImageCopy.Models
{
	internal static class VideoManager
	{
		public static Task<BitmapImage> GetSnapshotImageAsync(string filePath, TimeSpan timeFromStart, Size size)
		{
			return Task.FromResult((BitmapImage)null);
		}

		public static Task<byte[]> GetSnapshotBytesAsync(string filePath, TimeSpan timeFromStart, int qualityLevel = 0)
		{
			return Task.FromResult((byte[])null);
		}
	}
}

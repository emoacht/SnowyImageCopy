using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using SnowyImageCopy.Models;

namespace SnowyImageCopy.Views.Converters
{
	/// <summary>
	/// Convert FileStatus to Brush.
	/// </summary>
	[ValueConversion(typeof(FileStatus), typeof(Brush))]
	public class FileStatusToBrushConverter : IValueConverter
	{
		public static Dictionary<FileStatus, Color> StatusColorMap
		{
			get
			{
				if (_statusColorMap == null)
				{
					_statusColorMap = new Dictionary<FileStatus, Color>
					{
						{FileStatus.Unknown, (Color)App.Current.Resources["FileItemStatus.UnknownColor"]},
						{FileStatus.NotCopied, (Color)App.Current.Resources["FileItemStatus.NotCopiedColor"]},
						{FileStatus.ToBeCopied,(Color)App.Current.Resources["FileItemStatus.ToBeCopiedColor"]},
						{FileStatus.Copying, (Color)App.Current.Resources["FileItemStatus.CopyingColor"]},
						{FileStatus.Copied, (Color)App.Current.Resources["FileItemStatus.CopiedColor"]},
						{FileStatus.Weird, (Color)App.Current.Resources["FileItemStatus.WeirdColor"]},
						{FileStatus.Recycled, (Color)App.Current.Resources["FileItemStatus.RecycledColor"]},
					};
				}

				return _statusColorMap;
			}
		}
		private static Dictionary<FileStatus, Color> _statusColorMap;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is FileStatus))
				return DependencyProperty.UnsetValue;

			var status = (FileStatus)value;

			return StatusColorMap.Keys.Contains(status)
				? new SolidColorBrush(StatusColorMap[status])
				: Brushes.LightGray; // Fallback
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
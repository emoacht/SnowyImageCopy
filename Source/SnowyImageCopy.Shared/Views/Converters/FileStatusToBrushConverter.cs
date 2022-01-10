using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

using SnowyImageCopy.Models.ImageFile;

namespace SnowyImageCopy.Views.Converters
{
	/// <summary>
	/// Converts FileStatus to corresponding Brush.
	/// </summary>
	[ValueConversion(typeof(FileStatus), typeof(Brush))]
	public class FileStatusToBrushConverter : IValueConverter
	{
		public static IReadOnlyDictionary<FileStatus, Color> StatusColorMap
		{
			get => _statusColorMap ??= new Dictionary<FileStatus, Color>
			{
				{FileStatus.Unknown, (Color)Application.Current.Resources["FileItemStatus.UnknownColor"]},
				{FileStatus.NotCopied, (Color)Application.Current.Resources["FileItemStatus.NotCopiedColor"]},
				{FileStatus.ToBeCopied,(Color)Application.Current.Resources["FileItemStatus.ToBeCopiedColor"]},
				{FileStatus.Copying, (Color)Application.Current.Resources["FileItemStatus.CopyingColor"]},
				{FileStatus.Copied, (Color)Application.Current.Resources["FileItemStatus.CopiedColor"]},
				{FileStatus.OnceCopied, (Color)Application.Current.Resources["FileItemStatus.OnceCopiedColor"]},
				{FileStatus.Weird, (Color)Application.Current.Resources["FileItemStatus.WeirdColor"]},
				{FileStatus.Recycled, (Color)Application.Current.Resources["FileItemStatus.RecycledColor"]}
			};
		}
		private static Dictionary<FileStatus, Color> _statusColorMap;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not FileStatus status)
				return DependencyProperty.UnsetValue;

			return StatusColorMap.TryGetValue(status, out Color statusColor)
				? new SolidColorBrush(statusColor)
				: Brushes.LightGray; // Fallback
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
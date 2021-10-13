using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SnowyImageCopy.Views.Converters
{
	/// <summary>
	/// Converts double representing percent to double divided by 100.
	/// </summary>
	[ValueConversion(typeof(double), typeof(double))]
	public class PercentUnitConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not double sourceValue)
				return DependencyProperty.UnsetValue;

			var targetValue = sourceValue / 100D;
			return targetValue switch
			{
				< 0D => 0D,
				< 1D => targetValue,
				_ => 1D
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not double targetValue)
				return DependencyProperty.UnsetValue;

			var sourceValue = targetValue * 100D;
			return sourceValue switch
			{
				< 0D => 0D,
				< 100D => sourceValue,
				_ => 100D
			};
		}
	}
}
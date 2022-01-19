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
	/// Converts ulong representing the number of bytes to double divided by the power of 1024.
	/// </summary>
	[ValueConversion(typeof(ulong), typeof(double))]
	public class BytesUnitConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((value is not ulong sourceValue) || double.TryParse(parameter as string, out double factor))
				return DependencyProperty.UnsetValue;

			return sourceValue / Math.Pow(1024, factor);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
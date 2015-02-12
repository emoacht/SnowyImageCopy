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
	/// Convert double of percentage to centesimal double.
	/// </summary>
	[ValueConversion(typeof(double), typeof(double))]
	public class DoubleCentesimalConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double))
				return DependencyProperty.UnsetValue;

			var buff = (double)value;

			if (buff < 0D)
				return 0D;
			if (buff < 100D)
				return buff / 100D;
			else
				return 1D;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is double))
				return DependencyProperty.UnsetValue;

			var buff = (double)value;

			if (buff < 0D)
				return 0D;
			if (buff < 100D)
				return buff * 100D;
			else
				return 100D;
		}
	}
}
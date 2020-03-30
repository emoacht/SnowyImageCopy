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
	/// Converts ulong to double divided by the power of 1024.
	/// </summary>
	[ValueConversion(typeof(ulong), typeof(double))]
	public class TripleDigitsConverter : IValueConverter
	{
		private const double TripleDigitsFactor = 1024;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is ulong sourceValue))
				return DependencyProperty.UnsetValue;

			var factor = TripleDigitsFactor;
			if (double.TryParse(parameter as string, out double buffer))
				factor = Math.Pow(TripleDigitsFactor, buffer);

			return sourceValue / factor;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
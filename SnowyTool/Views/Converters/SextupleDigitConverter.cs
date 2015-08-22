using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SnowyTool.Views.Converters
{
	/// <summary>
	/// Convert between ULong and ULong divided by 1048576.
	/// </summary>
	[ValueConversion(typeof(ulong), typeof(ulong))]
	public class SextupleDigitConverter : IValueConverter
	{
		private const ulong _sextupleDigitFactor = 1048576UL;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ulong sourceValue;
			if ((value == null) || (!ulong.TryParse(value.ToString(), out sourceValue)))
				return DependencyProperty.UnsetValue;

			return sourceValue / _sextupleDigitFactor;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ulong sourceValue;
			if ((value == null) || (!ulong.TryParse(value.ToString(), out sourceValue)))
				return DependencyProperty.UnsetValue;

			return sourceValue * _sextupleDigitFactor;
		}
	}
}
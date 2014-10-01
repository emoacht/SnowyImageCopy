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
	/// Convert ULong to ULong divided by 1048576.
	/// </summary>
	[ValueConversion(typeof(ulong), typeof(ulong))]
	public class SextupleDigitConverter : IValueConverter
	{
		private const ulong sextupleDigitFactor = 1048576UL;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ulong buff;
			if ((value == null) || (!ulong.TryParse(value.ToString(), out buff)))
				return DependencyProperty.UnsetValue;

			return buff / sextupleDigitFactor;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ulong buff;
			if ((value == null) || (!ulong.TryParse(value.ToString(), out buff)))
				return DependencyProperty.UnsetValue;

			return buff * sextupleDigitFactor;
		}
	}
}
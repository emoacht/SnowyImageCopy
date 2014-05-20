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
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ulong num;
			if (!ulong.TryParse(value.ToString(), out num))
				return DependencyProperty.UnsetValue;

			return num / 1048576UL;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			ulong num;
			if (!ulong.TryParse(value.ToString(), out num))
				return DependencyProperty.UnsetValue;

			return num * 1048576UL;
		}
	}
}
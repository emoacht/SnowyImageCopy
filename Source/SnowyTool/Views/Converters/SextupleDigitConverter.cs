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
	/// Converts ulong to ulong divided by 1048576.
	/// </summary>
	[ValueConversion(typeof(ulong), typeof(ulong))]
	public class SextupleDigitConverter : IValueConverter
	{
		private const ulong SextupleDigitFactor = 1048576UL; // 1024 * 1024

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is ulong))
				return DependencyProperty.UnsetValue;

			return (ulong)value / SextupleDigitFactor;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
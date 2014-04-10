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
	/// Convert Enum to Boolean.
	/// </summary>
	[ValueConversion(typeof(Enum), typeof(bool))]
	public class EnumToBooleanConverter : IValueConverter
	{
		/// <summary>
		/// Return True when source Enum value matches target Enum value string.
		/// </summary>
		/// <param name="value">Source Enum value</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Target Enum value string</param>
		/// <param name="culture"></param>
		/// <returns>Boolean</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Enum))
				return DependencyProperty.UnsetValue;

			return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

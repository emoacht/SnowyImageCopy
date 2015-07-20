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
	/// Convert between Enum and Boolean.
	/// </summary>
	[ValueConversion(typeof(Enum), typeof(bool))]
	public class EnumToBooleanConverter : IValueConverter
	{
		/// <summary>
		/// Convert Enum value to Boolean.
		/// </summary>
		/// <param name="value">Enum value</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Target Enum name string</param>
		/// <param name="culture"></param>
		/// <returns>True if Enum name matches target Enum name string. False if not.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Enum) || (parameter == null))
				return DependencyProperty.UnsetValue;

			return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Convert Boolean to Enum value.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Target Enum name string</param>
		/// <param name="culture"></param>
		/// <returns>Target Enum value if Boolean is true.</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool) || !(bool)value || (parameter == null))
				return DependencyProperty.UnsetValue;

			var enumValue = Enum.GetValues(targetType)
				.Cast<Enum>()
				.FirstOrDefault(x => x.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase));

			return enumValue ?? DependencyProperty.UnsetValue;
		}
	}
}
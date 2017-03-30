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
	/// Converts between Enum and Boolean.
	/// </summary>
	[ValueConversion(typeof(Enum), typeof(bool))]
	public class EnumToBooleanConverter : IValueConverter
	{
		/// <summary>
		/// Converts Enum value to Boolean.
		/// </summary>
		/// <param name="value">Enum value</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Condition Enum name string (case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>True if Enum value matches condition Enum value. False if not.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Enum) || !(parameter is string))
				return DependencyProperty.UnsetValue;

			var condition = GetEnumValue(value.GetType(), (string)parameter);
			if (condition == null)
				return DependencyProperty.UnsetValue;

			return ((Enum)value).Equals(condition);
		}

		/// <summary>
		/// Converts Boolean to Enum value.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Condition Enum name string (case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>Condition Enum value if Boolean is true</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool) || !(bool)value || !targetType.IsEnum || !(parameter is string))
				return DependencyProperty.UnsetValue;

			var condition = GetEnumValue(targetType, (string)parameter);
			if (condition == null)
				return DependencyProperty.UnsetValue;

			return condition;
		}

		private static Enum GetEnumValue(Type enumType, string source)
		{
			return Enum.GetValues(enumType)
				.Cast<Enum>()
				.FirstOrDefault(x => x.ToString().Equals(source, StringComparison.OrdinalIgnoreCase));
		}
	}
}
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
			if ((value is not Enum sourceValue) || (parameter is not string conditionString))
				return DependencyProperty.UnsetValue;

			if (!TryParse(value.GetType(), conditionString, out Enum conditionValue))
				return DependencyProperty.UnsetValue;

			return sourceValue.Equals(conditionValue); // == operator works well for concrete enum but not for System.Enum.
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
			if ((value is not bool targetValue) || !targetValue || (parameter is not string conditionString))
				return DependencyProperty.UnsetValue;

			if (!TryParse(targetType, conditionString, out Enum conditionValue))
				return DependencyProperty.UnsetValue;

			return conditionValue;
		}

		private static bool TryParse(Type enumType, string source, out Enum value)
		{
			// Compare enum values with source string ignoring the case.
			// Enum.IsDefined and Enum.Parse methods are case-sensitive and so not usable for this purpose.
			value = enumType.IsEnum
				? Enum.GetValues(enumType).Cast<Enum>()
					.FirstOrDefault(x => x.ToString().Equals(source.Trim(), StringComparison.OrdinalIgnoreCase))
				: null;

			return (value is not null);
		}
	}
}
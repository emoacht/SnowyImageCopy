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
	/// Converts Enum to Visibility.
	/// </summary>
	[ValueConversion(typeof(Enum), typeof(Visibility))]
	public class EnumToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// Converts Enum to Visibility.
		/// </summary>
		/// <param name="value">Enum value</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Condition Enum name string (case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>Visibility.Visible if Enum value matches condition Enum value. Visibility.Collapsed if not.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Enum sourceValue) || !(parameter is string conditionString))
				return DependencyProperty.UnsetValue;

			var condition = GetEnumValue(value.GetType(), conditionString);
			if (condition == null)
				return DependencyProperty.UnsetValue;

			return ((Enum)value).Equals(condition)
				? Visibility.Visible
				: Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		private static Enum GetEnumValue(Type enumType, string source)
		{
			return Enum.GetValues(enumType)
				.Cast<Enum>()
				.FirstOrDefault(x => x.ToString().Equals(source, StringComparison.OrdinalIgnoreCase));
		}
	}
}
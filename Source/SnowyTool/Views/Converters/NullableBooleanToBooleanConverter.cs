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
	/// Converts Nullable Boolean to Boolean.
	/// </summary>
	[ValueConversion(typeof(bool?), typeof(bool))]
	public class NullableBooleanToBooleanConverter : IValueConverter
	{
		/// <summary>
		/// Converts Nullable Boolean to Boolean.
		/// </summary>
		/// <param name="value">Nullable Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Condition Boolean or Boolean string (case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>Boolean</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!TryParse(value, out bool sourceValue))
				return false;

			if (!TryParse(parameter, out bool conditionValue))
				return DependencyProperty.UnsetValue;

			return (sourceValue == conditionValue);
		}

		/// <summary>
		/// Converts Boolean to Nullable Boolean.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Condition Boolean or Boolean string (case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>Nullable Boolean</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool sourceValue) || !sourceValue)
				return DependencyProperty.UnsetValue;

			if (!TryParse(parameter, out bool conditionValue))
				return DependencyProperty.UnsetValue;

			return conditionValue;
		}

		private static bool TryParse(object source, out bool value)
		{
			if ((source is bool buff) || bool.TryParse(source as string, out buff))
			{
				value = buff;
				return true;
			}
			value = default;
			return false;
		}
	}
}
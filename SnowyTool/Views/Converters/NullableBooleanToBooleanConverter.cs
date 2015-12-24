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
	/// Convert between Boolean and Nullable Boolean.
	/// </summary>
	[ValueConversion(typeof(bool?), typeof(bool))]
	public class NullableBooleanToBooleanConverter : IValueConverter
	{
		/// <summary>
		/// Convert Nullable Boolean to Boolean.
		/// </summary>
		/// <param name="value">Nullable Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Condition Boolean string</param>
		/// <param name="culture"></param>
		/// <returns>Boolean</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var source = FindBoolean(value);
			if (!source.HasValue)
				return false;

			var condition = FindBoolean(parameter);
			if (!condition.HasValue)
				return DependencyProperty.UnsetValue;

			return condition.Value == source.Value;
		}

		/// <summary>
		/// Convert Boolean to Nullable Boolean.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Condition Boolean string</param>
		/// <param name="culture"></param>
		/// <returns>Nullable Boolean</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool) || !(bool)value)
				return DependencyProperty.UnsetValue;

			var condition = FindBoolean(parameter);
			if (!condition.HasValue)
				return DependencyProperty.UnsetValue;

			return condition.Value;
		}

		private static bool? FindBoolean(object source)
		{
			if (source is bool)
				return (bool)source;

			if (bool.TrueString.Equals(source as string, StringComparison.OrdinalIgnoreCase))
				return true;

			if (bool.FalseString.Equals(source as string, StringComparison.OrdinalIgnoreCase))
				return false;

			return null;
		}
	}
}
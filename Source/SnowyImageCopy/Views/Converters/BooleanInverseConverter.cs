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
	/// Inverses Boolean.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(bool))]
	public class BooleanInverseConverter : IValueConverter
	{
		/// <summary>
		/// Inverses Boolean.
		/// </summary>
		/// <param name="value">Source Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Condition Boolean or Boolean string (optional, case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>Inversed Boolean except if condition Boolean is given and does not match source Boolean.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not bool sourceValue)
				return DependencyProperty.UnsetValue;

			if (TryParse(parameter, out bool conditionValue) && (sourceValue != conditionValue))
				return Binding.DoNothing; // DependencyProperty.UnsetValue will not work well.

			return !sourceValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return Convert(value, targetType, parameter, culture);
		}

		private static bool TryParse(object source, out bool value)
		{
			if ((source is bool buffer) || bool.TryParse(source as string, out buffer))
			{
				value = buffer;
				return true;
			}
			value = default;
			return false;
		}
	}
}
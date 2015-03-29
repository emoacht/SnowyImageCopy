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
	/// Convert Enum to Visibility.
	/// </summary>
	[ValueConversion(typeof(Enum), typeof(Visibility))]
	public class EnumToVisibilityConverter : IValueConverter
	{
		/// <summary>
		/// Return Visibility.Visible when source Enum name matches target Enum name string.
		/// </summary>
		/// <param name="value">Enum name</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Target Enum name string</param>
		/// <param name="culture"></param>
		/// <returns>Visibility</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Enum) || (parameter == null))
				return DependencyProperty.UnsetValue;

			return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase)
				? Visibility.Visible
				: Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
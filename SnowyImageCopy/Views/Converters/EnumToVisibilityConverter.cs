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
		/// Convert Enum value to Visibility.
		/// </summary>
		/// <param name="value">Enum value</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Target Enum name string</param>
		/// <param name="culture"></param>
		/// <returns>Visibility.Visible if Enum name matches target Enum name string. Visibility.Collapsed if not.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Enum))
				return DependencyProperty.UnsetValue;

			var enumType = value.GetType();

			if ((parameter == null) || !Enum.IsDefined(enumType, parameter))
				return DependencyProperty.UnsetValue;

			if (!(parameter is Enum))
				parameter = Enum.Parse(enumType, parameter.ToString());

			return (((Enum)value).Equals(parameter))
				? Visibility.Visible
				: Visibility.Collapsed;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
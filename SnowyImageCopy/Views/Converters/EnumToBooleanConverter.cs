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
		/// Return true when source Enum name matches target Enum name string.
		/// </summary>
		/// <param name="value">Source Enum name</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Target Enum name string</param>
		/// <param name="culture"></param>
		/// <returns>Boolean</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is Enum))
				return DependencyProperty.UnsetValue;

			return value.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		/// <summary>
		/// Return Enum name when source Boolean is true.
		/// </summary>
		/// <param name="value">Source Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Target Enum name string</param>
		/// <param name="culture"></param>
		/// <returns>Enum name</returns>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool) || !(bool)value)
				return DependencyProperty.UnsetValue;

			var name = Enum.GetNames(targetType)
				.FirstOrDefault(x => x.ToString().Equals(parameter.ToString(), StringComparison.OrdinalIgnoreCase));

			return name ?? DependencyProperty.UnsetValue;
		}
	}
}

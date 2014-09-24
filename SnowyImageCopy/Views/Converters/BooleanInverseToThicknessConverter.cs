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
	/// Inverse Boolean and then convert it to Thickness.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Thickness))]
	public class BooleanInverseToThicknessConverter : IValueConverter
	{
		/// <summary>
		/// If true, return zero thickness. If false, return thickness given by parameter.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Thickness</param>
		/// <param name="culture"></param>
		/// <returns>Thickness</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool) || !(parameter is Thickness))
				return DependencyProperty.UnsetValue;

			return (bool)value ? new Thickness(0) : (Thickness)parameter;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
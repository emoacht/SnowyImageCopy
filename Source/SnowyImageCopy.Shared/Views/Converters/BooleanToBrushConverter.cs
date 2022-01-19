using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SnowyImageCopy.Views.Converters
{
	/// <summary>
	/// Converts Boolean to <see cref="System.Windows.Media.Brush"/>.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Brush))]
	public class BooleanToBrushConverter : IValueConverter
	{
		/// <summary>
		/// Converts Boolean to Brush.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Array of Brushes</param>
		/// <param name="culture"></param>
		/// <returns>First Brush if Boolean is true. Second Brush if not.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((value is not bool sourceValue) || (parameter is not Brush[] brushes) || (brushes.Length < 2))
				return DependencyProperty.UnsetValue;

			return sourceValue ? brushes[0] : brushes[1];
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace SnowyTool.Views.Converters
{
	/// <summary>
	/// Inverses Boolean and converts it to Brush.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Brush))]
	public class BooleanInverseToBrushConverter : IValueConverter
	{
		private static readonly HashSet<string> _predefinedColorNames =
			new HashSet<string>(typeof(Colors).GetProperties().Select(x => x.Name.ToLower()));

		/// <summary>
		/// Inverses Boolean and converts it to Brush.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Predefined color name string (case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>SolidColorBrush of the predefined color if Boolean is false</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool sourceValue) || sourceValue || !(parameter is string colorString))
				return DependencyProperty.UnsetValue;

			if (!_predefinedColorNames.Contains(colorString.ToLower()))
				return DependencyProperty.UnsetValue;

			return (SolidColorBrush)new BrushConverter().ConvertFromInvariantString(colorString);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
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
	/// Inverse Boolean and convert it to Brush.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Brush))]
	public class BooleanInverseToBrushConverter : IValueConverter
	{
		private readonly string[] _predefinedColorNames = typeof(Colors).GetProperties()
			.Select(x => x.Name)
			.ToArray();

		/// <summary>
		/// Inverse Boolean and convert it to Brush.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Predefined color name</param>
		/// <param name="culture"></param>
		/// <returns>SolidColorBrush of the predefined color if Boolean is false.</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool) || (bool)value || (parameter == null))
				return DependencyProperty.UnsetValue;

			if (!_predefinedColorNames.Contains(parameter.ToString()))
				return DependencyProperty.UnsetValue;

			return (SolidColorBrush)new BrushConverter().ConvertFromInvariantString(parameter.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
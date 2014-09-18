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
		private readonly string[] predefinedColorNames = typeof(Colors).GetProperties()
			.Select(x => x.Name)
			.ToArray();

		/// <summary>
		/// Return a SolidColorBrush when source Boolean is false.
		/// </summary>
		/// <param name="value">Source Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Name of a predefined color</param>
		/// <param name="culture"></param>
		/// <returns>SolidColorBrush of the predefined color</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is bool) || (bool)value || (parameter == null))
				return DependencyProperty.UnsetValue;

			if (!predefinedColorNames.Contains(parameter.ToString()))
				return DependencyProperty.UnsetValue;

			return (SolidColorBrush)new BrushConverter().ConvertFromString(parameter.ToString());
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}

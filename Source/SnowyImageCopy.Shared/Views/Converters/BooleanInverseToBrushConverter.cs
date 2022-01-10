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
	/// Inverses Boolean and converts it to Brush.
	/// </summary>
	[ValueConversion(typeof(bool), typeof(Brush))]
	public class BooleanInverseToBrushConverter : IValueConverter
	{
		private static readonly Dictionary<string, Brush> _brushPairs = new();

		/// <summary>
		/// Inverses Boolean and converts it to Brush.
		/// </summary>
		/// <param name="value">Boolean</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Color name string (case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>SolidColorBrush of the predefined color if Boolean is false</returns>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if ((value is not bool sourceValue) || sourceValue || (parameter is not string colorString))
				return DependencyProperty.UnsetValue;

			if (!_brushPairs.TryGetValue(colorString, out Brush brush))
			{
				brush = _brushPairs[colorString] = GetBrush(colorString);
			}
			return brush ?? DependencyProperty.UnsetValue;

			static Brush GetBrush(string colorString)
			{
				try
				{
					return (SolidColorBrush)new BrushConverter().ConvertFromInvariantString(colorString);
				}
				catch (FormatException)
				{
					return default;
				}
			};
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
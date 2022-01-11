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
	/// Converts TimeSpan to formatted string.
	/// </summary>
	[ValueConversion(typeof(TimeSpan), typeof(string))]
	public class TimeSpanToStringConverter : IValueConverter
	{
		/// <summary>
		/// Converts TimeSpan to formatted string.
		/// </summary>
		/// <param name="value">TimeSpan</param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>Formatted string</returns>
		/// <remarks>The seconds will be rounded up.</remarks>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is not TimeSpan sourceValue)
				return DependencyProperty.UnsetValue;

			var totalSeconds = (int)Math.Ceiling(sourceValue.TotalSeconds);

			return $"{(totalSeconds / 60):D2}:{(totalSeconds % 60):D2}";
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotSupportedException();
		}
	}
}
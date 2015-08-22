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
	/// Convert TimeSpan to formatted string.
	/// </summary>
	[ValueConversion(typeof(TimeSpan), typeof(string))]
	public class TimeSpanToStringConverter : IValueConverter
	{
		/// <summary>
		/// Convert TimeSpan to formatted string.
		/// </summary>
		/// <param name="value">TimeSpan</param>
		/// <param name="targetType"></param>
		/// <param name="parameter"></param>
		/// <param name="culture"></param>
		/// <returns>Formatted string</returns>
		/// <remarks>The Seconds will be rounded up.</remarks>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (!(value is TimeSpan))
				return DependencyProperty.UnsetValue;

			var timeSpan = TimeSpan.FromSeconds(Math.Ceiling(((TimeSpan)value).TotalSeconds));

			return String.Format("{0:D2}:{1:D2}", (int)Math.Floor(timeSpan.TotalMinutes), timeSpan.Seconds);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
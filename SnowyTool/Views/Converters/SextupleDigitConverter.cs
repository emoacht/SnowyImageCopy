using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SnowyTool.Views.Converters
{
	/// <summary>
	/// Convert between ULong and ULong divided by 1048576.
	/// </summary>
	[ValueConversion(typeof(ulong), typeof(ulong))]
	public class SextupleDigitConverter : IValueConverter
	{
		private const ulong _sextupleDigitFactor = 1048576UL;

		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var source = FindULong(value);
			if (!source.HasValue)
				return DependencyProperty.UnsetValue;

			return source.Value / _sextupleDigitFactor;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var source = FindULong(value);
			if (!source.HasValue)
				return DependencyProperty.UnsetValue;

			return source.Value * _sextupleDigitFactor;
		}

		private static ulong? FindULong(object source)
		{
			if (source is ulong)
				return (ulong)source;

			ulong buff;
			if (ulong.TryParse(source as string, out buff))
				return buff;

			return null;
		}
	}
}
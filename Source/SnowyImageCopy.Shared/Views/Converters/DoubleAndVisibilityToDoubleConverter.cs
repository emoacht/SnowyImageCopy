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
	/// Converts pairs of double and Visibility to double.
	/// </summary>
	public class DoubleAndVisibilityToDoubleConverter : IMultiValueConverter
	{
		private enum Order
		{
			Add,
			Subtract,
		}

		/// <summary>
		/// Adds/Subtracts the second and subsequent double to/from the first double. Values have to
		/// be pairs of double and Visibility. If a Visibility is not Visibility.Visible, the paired
		/// double will be ignored.
		/// </summary>
		/// <param name="values">Double or Visibility</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Order string ("Add" or "Subtract", case-insensitive)</param>
		/// <param name="culture"></param>
		/// <returns>Double</returns>
		/// <remarks>If order string is null, it will be regarded as "Add".</remarks>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var sourceLengths = Enumerable.Zip(values.OfType<double>(), values.OfType<Visibility>(), (x, y) => new { Length = x, Visibility = y })
				.Where(x => x.Visibility == Visibility.Visible)
				.Select(x => x.Length)
				.ToArray();

			if (sourceLengths.Length == 0)
				return double.NaN; // DependencyProperty.UnsetValue has the same effect.

			if (sourceLengths.Length == 1)
				return sourceLengths[0];

			var order = Enum.TryParse(parameter as string, true, out Order parsed)
				? parsed
				: Order.Add; // Fallback

			double sum = 0D;
			switch (order)
			{
				case Order.Add:
					sum = sourceLengths.Sum();
					break;
				case Order.Subtract:
					sum = sourceLengths[0] - sourceLengths.Skip(1).Sum();
					break;
			}

			return (0 <= sum) ? sum : double.NaN;
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
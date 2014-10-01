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
	/// Convert pairs of double and Visibility to double.
	/// </summary>
	public class DoubleAndVisibilityToDoubleConverter : IMultiValueConverter
	{
		private enum Order
		{
			Add,
			Subtract,
		}

		/// <summary>
		/// Add/Subtract the second and subsequent double to/from the first double. Values have to
		/// be pairs of double and Visibility. If a Visibility is not Visibility.Visible, the paired
		/// double will be ignored.
		/// </summary>
		/// <param name="values">Double or Visibility</param>
		/// <param name="targetType"></param>
		/// <param name="parameter">Order to add/subtract</param>
		/// <param name="culture"></param>
		/// <returns>Double</returns>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			var lengths = values.OfType<double>().Select((x, i) => new { Index = i, Length = x });
			var visibilities = values.OfType<Visibility>().Select((x, i) => new { Index = i, Visibility = x });

			var sourceLengths = lengths
				.Join(visibilities, x => x.Index, y => y.Index, (x, y) => new { x.Length, y.Visibility })
				.Where(x => x.Visibility == Visibility.Visible)
				.Select(x => x.Length)
				.ToArray();

			if (!sourceLengths.Any())
				return double.NaN; // DependencyProperty.UnsetValue has the same effect.

			if (sourceLengths.Length == 1)
				return sourceLengths[0];

			var order = ((parameter == null) || (parameter.ToString().Equals(Order.Add.ToString(), StringComparison.OrdinalIgnoreCase)))
				? Order.Add
				: Order.Subtract;

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
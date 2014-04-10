using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SnowyImageCopy.Views.Controls
{
	/// <summary>
	/// Interaction logic for NumericUpDown.xaml
	/// </summary>
	public partial class NumericUpDown : UserControl
	{
		public NumericUpDown()
		{
			InitializeComponent();
		}


		#region Dependency Property

		public double Value
		{
			get { return (double)GetValue(ValueProperty); }
			set { SetValue(ValueProperty, value); }
		}
		public static readonly DependencyProperty ValueProperty =
			DependencyProperty.Register(
				"Value",
				typeof(double),
				typeof(NumericUpDown),
				new FrameworkPropertyMetadata(
					0D,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					OnPropertyChanged,
					(d, baseValue) =>
					{
						var numeric = (NumericUpDown)d;
						if ((double)baseValue < numeric.Minimum)
							return numeric.Minimum;
						if (numeric.Maximum < (double)baseValue)
							return numeric.Maximum;

						return (double)baseValue;
					}));

		public double Minimum
		{
			get { return (double)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}
		public static readonly DependencyProperty MinimumProperty =
			RangeBase.MinimumProperty.AddOwner(
				typeof(NumericUpDown),
				new FrameworkPropertyMetadata(0D, OnPropertyChanged));

		public double Maximum
		{
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}
		public static readonly DependencyProperty MaximumProperty =
			RangeBase.MaximumProperty.AddOwner(
				typeof(NumericUpDown),
				new FrameworkPropertyMetadata(10D, OnPropertyChanged));

		public double Frequency
		{
			get { return (double)GetValue(FrequencyProperty); }
			set { SetValue(FrequencyProperty, value); }
		}
		public static readonly DependencyProperty FrequencyProperty =
			DependencyProperty.Register(
				"Frequency",
				typeof(double),
				typeof(NumericUpDown),
				new FrameworkPropertyMetadata(1D));

		#endregion


		private enum Direction
		{
			Down,
			Up,
		}


		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			((NumericUpDown)sender).ChangeCanChangeValue();
		}

		private void OnClick(object sender, RoutedEventArgs e)
		{
			var direction = ((FrameworkElement)e.Source).Equals(DownButton) ? Direction.Down : Direction.Up;
			SetAppearance(direction);
		}


		private void SetAppearance(Direction direction)
		{
			switch (direction)
			{
				case Direction.Down:
					if (Value > Minimum)
					{
						var num = Value - Frequency;
						Value = (num > Minimum) ? num : Minimum;
					}
					break;
				case Direction.Up:
					if (Value < Maximum)
					{
						var num = Value + Frequency;
						Value = (num < Maximum) ? num : Maximum;
					}
					break;
			}

			ChangeCanChangeValue();
		}

		private void ChangeCanChangeValue()
		{
			DownButton.IsEnabled = (Value > Minimum);
			UpButton.IsEnabled = (Value < Maximum);
		}
	}
}

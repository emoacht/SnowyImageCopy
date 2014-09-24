using System;
using System.Collections.Generic;
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
				new FrameworkPropertyMetadata(
					0D,
					OnPropertyChanged));

		public double Maximum
		{
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}
		public static readonly DependencyProperty MaximumProperty =
			RangeBase.MaximumProperty.AddOwner(
				typeof(NumericUpDown),
				new FrameworkPropertyMetadata(
					10D,
					OnPropertyChanged));

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
				new FrameworkPropertyMetadata(
					1D,
					null,
					(d, baseValue) => (0 < (double)baseValue) ? (double)baseValue : DependencyProperty.UnsetValue));

		/// <summary>
		/// Middle level between Minimum and Maximum
		/// </summary>
		/// <remarks>To enable Middle, HigherFrequency and LowerFrequency must be set.</remarks>
		public double Middle
		{
			get { return (double)GetValue(MiddleProperty); }
			set { SetValue(MiddleProperty, value); }
		}
		public static readonly DependencyProperty MiddleProperty =
			DependencyProperty.Register(
				"Middle",
				typeof(double),
				typeof(NumericUpDown),
				new FrameworkPropertyMetadata(0D));

		/// <summary>
		/// Frequency when value is higher than Middle.
		/// </summary>
		/// <remarks>Default means invalid.</remarks>
		public double HigherFrequency
		{
			get { return (double)GetValue(HigherFrequencyProperty); }
			set { SetValue(HigherFrequencyProperty, value); }
		}
		public static readonly DependencyProperty HigherFrequencyProperty =
			DependencyProperty.Register(
				"HigherFrequency",
				typeof(double),
				typeof(NumericUpDown),
				new FrameworkPropertyMetadata(0D));

		/// <summary>
		/// Frequency when value is lower than Middle.
		/// </summary>
		/// <remarks>Default means invalid.</remarks>
		public double LowerFrequency
		{
			get { return (double)GetValue(LowerFrequencyProperty); }
			set { SetValue(LowerFrequencyProperty, value); }
		}
		public static readonly DependencyProperty LowerFrequencyProperty =
			DependencyProperty.Register(
				"LowerFrequency",
				typeof(double),
				typeof(NumericUpDown),
				new FrameworkPropertyMetadata(0D));

		#endregion


		private bool IsMiddleEnabled
		{
			get { return (Minimum < Middle) && (Middle < Maximum) && (0 < LowerFrequency) && (0 < HigherFrequency); }
		}


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
					if (!IsMiddleEnabled)
					{
						var num = Value - Frequency;
						Value = (num > Minimum) ? num : Minimum;
					}
					else
					{
						if (Value > Middle)
						{
							var num = Value - HigherFrequency;
							Value = (num > Middle) ? num : Middle; // Stop at Middle.
						}
						else
						{
							var num = Value - LowerFrequency;
							Value = (num > Minimum) ? num : Minimum;
						}
					}
					break;
				case Direction.Up:
					if (!IsMiddleEnabled)
					{
						var num = Value + Frequency;
						Value = (num < Maximum) ? num : Maximum;
					}
					else
					{
						if (Value < Middle)
						{
							var num = Value + LowerFrequency;
							Value = (num < Middle) ? num : Middle; // Stop at Middle.
						}
						else
						{
							var num = Value + HigherFrequency;
							Value = (num < Maximum) ? num : Maximum;
						}
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
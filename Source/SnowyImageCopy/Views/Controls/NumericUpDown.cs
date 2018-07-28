using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SnowyImageCopy.Views.Controls
{
	[TemplatePart(Name = "PART_UpButton", Type = typeof(RepeatButton))]
	[TemplatePart(Name = "PART_DownButton", Type = typeof(RepeatButton))]
	public class NumericUpDown : Control
	{
		#region Template Part

		private RepeatButton UpButton
		{
			get => _upButton;
			set
			{
				if (_upButton != null)
					_upButton.Click -= new RoutedEventHandler(OnButtonClick);

				_upButton = value;

				if (_upButton != null)
					_upButton.Click += new RoutedEventHandler(OnButtonClick);
			}
		}
		private RepeatButton _upButton;

		private RepeatButton DownButton
		{
			get => _downButton;
			set
			{
				if (_downButton != null)
					_downButton.Click -= new RoutedEventHandler(OnButtonClick);

				_downButton = value;

				if (_downButton != null)
					_downButton.Click += new RoutedEventHandler(OnButtonClick);
			}
		}
		private RepeatButton _downButton;

		#endregion

		#region Property

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
						return Math.Max(numeric.Minimum, Math.Min(numeric.Maximum, (double)baseValue));
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
		/// <remarks>Default (0) means invalid.</remarks>
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
		/// <remarks>Default (0) means invalid.</remarks>
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

		private bool IsMiddleEnabled => 
			(Minimum < Middle) && (Middle < Maximum) &&
			(0 < LowerFrequency) && (0 < HigherFrequency);

		#endregion

		private enum Direction
		{
			Down,
			Up,
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			UpButton = this.GetTemplateChild("PART_UpButton") as RepeatButton;
			DownButton = this.GetTemplateChild("PART_DownButton") as RepeatButton;

			ChangeCanChangeValue(); // For the case where Value is already at Minimum or Maximum
		}

		private static void OnPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
		{
			((NumericUpDown)sender).ChangeCanChangeValue();
		}

		private void OnButtonClick(object sender, RoutedEventArgs e)
		{
			if ((UpButton == null) || (DownButton == null))
				return;

			var direction = e.Source.Equals(DownButton) ? Direction.Down : Direction.Up;
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
			if ((UpButton == null) || (DownButton == null))
				return;

			UpButton.IsEnabled = (Value < Maximum);
			DownButton.IsEnabled = (Value > Minimum);
		}
	}
}
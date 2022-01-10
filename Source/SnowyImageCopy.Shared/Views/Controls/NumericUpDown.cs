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
				if (_upButton is not null)
					_upButton.Click -= new RoutedEventHandler(OnButtonClick);

				_upButton = value;

				if (_upButton is not null)
					_upButton.Click += new RoutedEventHandler(OnButtonClick);
			}
		}
		private RepeatButton _upButton;

		private RepeatButton DownButton
		{
			get => _downButton;
			set
			{
				if (_downButton is not null)
					_downButton.Click -= new RoutedEventHandler(OnButtonClick);

				_downButton = value;

				if (_downButton is not null)
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
				new PropertyMetadata(
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
				new PropertyMetadata(
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
				new PropertyMetadata(
					1D,
					null,
					(d, baseValue) => (0 < (double)baseValue) ? (double)baseValue : DependencyProperty.UnsetValue));

		/// <summary>
		/// Medium level
		/// </summary>
		/// <remarks>To enable Medium, MediumFrequency must be set.</remarks>
		public double Medium
		{
			get { return (double)GetValue(MediumProperty); }
			set { SetValue(MediumProperty, value); }
		}
		public static readonly DependencyProperty MediumProperty =
			DependencyProperty.Register(
				"Medium",
				typeof(double),
				typeof(NumericUpDown),
				new PropertyMetadata(0D));

		private bool IsMediumEnabled =>
			(Minimum < Medium) && (Medium < Maximum) &&
			(0 < MediumFrequency);

		/// <summary>
		/// Small level
		/// </summary>
		/// <remarks>To enable Small, Medium must be enabled and SmallFrequency must be set.</remarks>
		public double Small
		{
			get { return (double)GetValue(SmallProperty); }
			set { SetValue(SmallProperty, value); }
		}
		public static readonly DependencyProperty SmallProperty =
			DependencyProperty.Register(
				"Small",
				typeof(double),
				typeof(NumericUpDown),
				new PropertyMetadata(1D));

		private bool IsSmallEnabled => IsMediumEnabled &&
			(Minimum < Small) && (Small < Medium) &&
			(0 < SmallFrequency);

		/// <summary>
		/// Frequency when value is equal to or lower than Medium
		/// </summary>
		/// <remarks>Default (0) means invalid.</remarks>
		public double MediumFrequency
		{
			get { return (double)GetValue(MediumFrequencyProperty); }
			set { SetValue(MediumFrequencyProperty, value); }
		}
		public static readonly DependencyProperty MediumFrequencyProperty =
			DependencyProperty.Register(
				"MediumFrequency",
				typeof(double),
				typeof(NumericUpDown),
				new PropertyMetadata(0D));

		/// <summary>
		/// Frequency when value is equal to or lower than Small
		/// </summary>
		/// <remarks>Default (0) means invalid.</remarks>
		public double SmallFrequency
		{
			get { return (double)GetValue(SmallFrequencyProperty); }
			set { SetValue(SmallFrequencyProperty, value); }
		}
		public static readonly DependencyProperty SmallFrequencyProperty =
			DependencyProperty.Register(
				"SmallFrequency",
				typeof(double),
				typeof(NumericUpDown),
				new PropertyMetadata(0D));

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
			if ((UpButton is null) || (DownButton is null))
				return;

			var direction = e.Source.Equals(DownButton) ? Direction.Down : Direction.Up;
			SetAppearance(direction);
		}

		private void SetAppearance(Direction direction)
		{
			double Change(double current)
			{
				switch (direction)
				{
					case Direction.Down:
						if (IsSmallEnabled && (current <= Small))
						{
							return Math.Max(current - SmallFrequency, Minimum);
						}
						if (IsMediumEnabled && (current <= Medium))
						{
							return Math.Max(current - MediumFrequency, (IsSmallEnabled ? Small /* Stop at Small. */ : Minimum));
						}
						{
							return Math.Max(current - Frequency, (IsMediumEnabled ? Medium /* Stop at Medium. */ : Minimum));
						}
					case Direction.Up:
						if (IsSmallEnabled && (current < Small))
						{
							return Math.Min(current + SmallFrequency, Small /* Stop at Small. */);
						}
						if (IsMediumEnabled && (current < Medium))
						{
							return Math.Min(current + MediumFrequency, Medium /* Stop at Medium. */);
						}
						{
							return Math.Min(current + Frequency, Maximum);
						}
					default:
						throw new InvalidOperationException();
				}
			}

			Value = Change(Value);

			ChangeCanChangeValue();
		}

		private void ChangeCanChangeValue()
		{
			if ((UpButton is null) || (DownButton is null))
				return;

			UpButton.IsEnabled = (Value < Maximum);
			DownButton.IsEnabled = (Value > Minimum);
		}
	}
}
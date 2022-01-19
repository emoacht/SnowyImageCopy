﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace SnowyImageCopy.Views.Controls
{
	public class NumericTextBox : TextBox
	{
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
				typeof(NumericTextBox),
				new FrameworkPropertyMetadata(
					0D,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					null,
					(d, baseValue) =>
					{
						var numeric = (NumericTextBox)d;
						return Math.Max(numeric.Minimum, Math.Min(numeric.Maximum, (double)baseValue));
					}));

		public double Minimum
		{
			get { return (double)GetValue(MinimumProperty); }
			set { SetValue(MinimumProperty, value); }
		}
		public static readonly DependencyProperty MinimumProperty =
			RangeBase.MinimumProperty.AddOwner(
				typeof(NumericTextBox),
				new PropertyMetadata(0D));

		public double Maximum
		{
			get { return (double)GetValue(MaximumProperty); }
			set { SetValue(MaximumProperty, value); }
		}
		public static readonly DependencyProperty MaximumProperty =
			RangeBase.MaximumProperty.AddOwner(
				typeof(NumericTextBox),
				new PropertyMetadata(10D));

		public double Frequency
		{
			get { return (double)GetValue(FrequencyProperty); }
			set { SetValue(FrequencyProperty, value); }
		}
		public static readonly DependencyProperty FrequencyProperty =
			DependencyProperty.Register(
				"Frequency",
				typeof(double),
				typeof(NumericTextBox),
				new PropertyMetadata(1D));

		#endregion

		protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
		{
			base.OnPreviewMouseUp(e);

			var buffer = (Math.Floor(Value / Frequency) + 1) * Frequency;

			Value = ((Minimum <= buffer) && (buffer <= Maximum)) ? buffer : Minimum;
		}
	}
}
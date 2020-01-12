using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace SnowyTool.Views.Controls
{
	public class FadingTextBox : TextBox
	{
		public FadingTextBox()
		{ }

		static FadingTextBox()
		{
			TextBox.TextProperty.OverrideMetadata(
				typeof(FadingTextBox),
				new FrameworkPropertyMetadata(
					string.Empty,
					null,
					(d, baseValue) =>
					{
						// Using coerceValueCallback instead of propertyChangedCallback is for the case that 
						// source value is the same.
						if (!string.IsNullOrEmpty((string)baseValue))
							((FadingTextBox)d).ManageText();

						return (string)baseValue;
					}));
		}

		#region Property

		public double FadeOutTime // sec
		{
			get { return (double)GetValue(FadeOutTimeProperty); }
			set { SetValue(FadeOutTimeProperty, value); }
		}
		public static readonly DependencyProperty FadeOutTimeProperty =
			DependencyProperty.Register(
				"FadeOutTime",
				typeof(double),
				typeof(FadingTextBox),
				new FrameworkPropertyMetadata(
					0D,
					null,
					(d, baseValue) => Math.Max((double)baseValue, _fadingTime)));

		#endregion

		private DispatcherTimer _fadingTimer;

		private const double _fadingInterval = 0.1; // sec
		private const double _fadingTime = 1.5; // sec
		private const double _fadingStep = _fadingInterval / _fadingTime;
		private double _remainingTime; // sec

		private void ManageText()
		{
			if (FadeOutTime <= 0D)
				return;

			if (_fadingTimer is null)
			{
				_fadingTimer = new DispatcherTimer();
				_fadingTimer.Tick += OnFadingTimerTick;
			}

			_fadingTimer.Stop();

			var buff = this.Foreground.Clone();
			buff.Opacity = 1D;
			this.Foreground = buff;

			_remainingTime = FadeOutTime;
			_fadingTimer.Interval = TimeSpan.FromSeconds(_fadingInterval);
			_fadingTimer.Start();
		}

		private void OnFadingTimerTick(object sender, EventArgs e)
		{
			_remainingTime -= _fadingInterval;

			if (_remainingTime <= 0D)
			{
				this.Text = string.Empty;
				_fadingTimer.Stop();
			}
			else if (_remainingTime <= _fadingTime)
			{
				var buff = this.Foreground.Clone();
				buff.Opacity -= _fadingStep;
				this.Foreground = buff;
			}
		}
	}
}
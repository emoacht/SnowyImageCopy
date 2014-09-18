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
		#region Dependency Property

		public int FadeOutTime // sec
		{
			get { return (int)GetValue(FadeOutTimeProperty); }
			set { SetValue(FadeOutTimeProperty, value); }
		}
		public static readonly DependencyProperty FadeOutTimeProperty =
			DependencyProperty.Register(
				"FadeOutTime",
				typeof(int),
				typeof(FadingTextBox),
				new FrameworkPropertyMetadata(
					0,
					null,
					(d, baseValue) => (int)Math.Max((double)(int)baseValue, fadingTime)));

		#endregion


		public FadingTextBox() : base() { }

		static FadingTextBox()
		{
			TextBox.TextProperty.OverrideMetadata(
				typeof(FadingTextBox),
				new FrameworkPropertyMetadata(
					String.Empty,
					null,
					(d, baseValue) =>
					{
						// Using coerceValueCallback instead of propertyChangedCallback is for the case that 
						// source value is the same.
						if (!String.IsNullOrEmpty((String)baseValue))
							((FadingTextBox)d).ManageText();

						return (String)baseValue;
					}));
		}

		private DispatcherTimer fadingTimer;

		private const double fadingInterval = 0.1; // sec
		private const double fadingTime = 1.5; // sec
		private const double fadingStep = fadingInterval / fadingTime;
		private double remainingTime; // sec

		private void ManageText()
		{
			if (FadeOutTime == 0)
				return;

			if (fadingTimer == null)
			{
				fadingTimer = new DispatcherTimer();
				fadingTimer.Tick += OnFadingTimerTick;
			}

			fadingTimer.Stop();

			var buff = this.Foreground.Clone();
			buff.Opacity = 1D;
			this.Foreground = buff;

			remainingTime = (double)FadeOutTime;
			fadingTimer.Interval = TimeSpan.FromSeconds(fadingInterval);
			fadingTimer.Start();
		}

		private void OnFadingTimerTick(object sender, EventArgs e)
		{
			remainingTime -= fadingInterval;

			if (remainingTime <= 0D)
			{
				this.Text = String.Empty;
				fadingTimer.Stop();
			}
			else if (remainingTime <= fadingTime)
			{
				var buff = this.Foreground.Clone();
				buff.Opacity -= fadingStep;
				this.Foreground = buff;
			}
		}
	}
}

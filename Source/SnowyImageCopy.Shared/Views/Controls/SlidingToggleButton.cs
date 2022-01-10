using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace SnowyImageCopy.Views.Controls
{
	[TemplatePart(Name = "PART_BackgroundTextBox", Type = typeof(TextBox))]
	[TemplatePart(Name = "PART_ForegroundButtonLeft", Type = typeof(Button))]
	[TemplatePart(Name = "PART_ForegroundButtonRight", Type = typeof(Button))]
	[TemplatePart(Name = "PART_ForegroundTextBlock", Type = typeof(TextBlock))]
	public class SlidingToggleButton : Control
	{
		#region Template Part

		private TextBox _backgroundTextBox;
		private Button _foregroundButtonLeft;
		private Button _foregroundButtonRight;
		private TextBlock _foregroundTextBlock;

		#endregion

		#region Property

		public double InnerButtonWidth
		{
			get { return (double)GetValue(InnerButtonWidthProperty); }
			set { SetValue(InnerButtonWidthProperty, value); }
		}
		public static readonly DependencyProperty InnerButtonWidthProperty =
			DependencyProperty.Register(
				"InnerButtonWidth",
				typeof(double),
				typeof(SlidingToggleButton),
				new PropertyMetadata(
					18D,
					OnWidthChanged));

		/// <summary>
		/// Button text when checked
		/// </summary>
		public string TextChecked
		{
			get { return (string)GetValue(TextCheckedProperty); }
			set { SetValue(TextCheckedProperty, value); }
		}
		public static readonly DependencyProperty TextCheckedProperty =
			DependencyProperty.Register(
				"TextChecked",
				typeof(string),
				typeof(SlidingToggleButton),
				new PropertyMetadata(
					"On",
					OnAppearanceChanged));

		/// <summary>
		/// Button text when unchecked
		/// </summary>
		public string TextUnchecked
		{
			get { return (string)GetValue(TextUncheckedProperty); }
			set { SetValue(TextUncheckedProperty, value); }
		}
		public static readonly DependencyProperty TextUncheckedProperty =
			DependencyProperty.Register(
				"TextUnchecked",
				typeof(string),
				typeof(SlidingToggleButton),
				new PropertyMetadata(
					"Off",
					OnAppearanceChanged));

		/// <summary>
		/// Foreground Brush when checked
		/// </summary>
		public Brush ForegroundChecked
		{
			get { return (Brush)GetValue(ForegroundCheckedProperty); }
			set { SetValue(ForegroundCheckedProperty, value); }
		}
		public static readonly DependencyProperty ForegroundCheckedProperty =
			DependencyProperty.Register(
				"ForegroundChecked",
				typeof(Brush),
				typeof(SlidingToggleButton),
				new PropertyMetadata(
					Brushes.Black,
					OnAppearanceChanged));

		/// <summary>
		/// Foreground Brush when unchecked
		/// </summary>
		public Brush ForegroundUnchecked
		{
			get { return (Brush)GetValue(ForegroundUncheckedProperty); }
			set { SetValue(ForegroundUncheckedProperty, value); }
		}
		public static readonly DependencyProperty ForegroundUncheckedProperty =
			DependencyProperty.Register(
				"ForegroundUnchecked",
				typeof(Brush),
				typeof(SlidingToggleButton),
				new PropertyMetadata(
					Brushes.Black,
					OnAppearanceChanged));

		/// <summary>
		/// Background Brush when checked
		/// </summary>
		public Brush BackgroundChecked
		{
			get { return (Brush)GetValue(BackgroundCheckedProperty); }
			set { SetValue(BackgroundCheckedProperty, value); }
		}
		public static readonly DependencyProperty BackgroundCheckedProperty =
			DependencyProperty.Register(
				"BackgroundChecked",
				typeof(Brush),
				typeof(SlidingToggleButton),
				new PropertyMetadata(
					Brushes.SkyBlue,
					OnAppearanceChanged));

		/// <summary>
		/// Background Brush when unchecked
		/// </summary>
		public Brush BackgroundUnchecked
		{
			get { return (Brush)GetValue(BackgroundUncheckedProperty); }
			set { SetValue(BackgroundUncheckedProperty, value); }
		}
		public static readonly DependencyProperty BackgroundUncheckedProperty =
			DependencyProperty.Register(
				"BackgroundUnchecked",
				typeof(Brush),
				typeof(SlidingToggleButton),
				new PropertyMetadata(
					Brushes.Gray,
					OnAppearanceChanged));

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}
		public static readonly DependencyProperty IsCheckedProperty =
			ToggleButton.IsCheckedProperty.AddOwner(
				typeof(SlidingToggleButton),
				new FrameworkPropertyMetadata(
					false,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					(d, e) =>
					{
						if ((bool)e.NewValue)
							((SlidingToggleButton)d).Checked?.Invoke(d, new RoutedEventArgs(CheckedEvent));
						else
							((SlidingToggleButton)d).Unchecked?.Invoke(d, new RoutedEventArgs(UncheckedEvent));

						OnAppearanceChanged(d, e);
					}));

		public event RoutedEventHandler Checked;
		public event RoutedEventHandler Unchecked;

		public static readonly RoutedEvent CheckedEvent = EventManager.RegisterRoutedEvent("Checked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SlidingToggleButton));
		public static readonly RoutedEvent UncheckedEvent = EventManager.RegisterRoutedEvent("Unchecked", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(SlidingToggleButton));

		#endregion

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			_backgroundTextBox = this.GetTemplateChild("PART_BackgroundTextBox") as TextBox;
			if (_backgroundTextBox is not null)
			{
				_backgroundTextBox.PreviewMouseUp += new MouseButtonEventHandler(OnClick);
				_backgroundTextBox.PreviewKeyUp += new KeyEventHandler(OnClick);
			}

			_foregroundButtonLeft = this.GetTemplateChild("PART_ForegroundButtonLeft") as Button;
			if (_foregroundButtonLeft is not null)
				_foregroundButtonLeft.Click += new RoutedEventHandler(OnClick);

			_foregroundButtonRight = this.GetTemplateChild("PART_ForegroundButtonRight") as Button;
			if (_foregroundButtonRight is not null)
				_foregroundButtonRight.Click += new RoutedEventHandler(OnClick);

			_foregroundTextBlock = this.GetTemplateChild("PART_ForegroundTextBlock") as TextBlock;

			SetWidth();
			SetAppearance();
		}

		private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((SlidingToggleButton)d).SetWidth();
		}

		private void SetWidth()
		{
			if ((_foregroundButtonLeft is null) || (_foregroundButtonRight is null))
				return;

			_foregroundButtonLeft.Width = InnerButtonWidth;
			_foregroundButtonRight.Width = InnerButtonWidth;
		}

		private static void OnAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			((SlidingToggleButton)d).SetAppearance();
		}

		private void OnClick(object sender, RoutedEventArgs args)
		{
			IsChecked = !IsChecked;
			SetAppearance();
		}

		private void SetAppearance()
		{
			if ((_backgroundTextBox is null) || (_foregroundButtonLeft is null) || (_foregroundButtonRight is null) || (_foregroundTextBlock is null))
				return;

			if (IsChecked)
			{
				_backgroundTextBox.Background = BackgroundChecked;
				_foregroundButtonLeft.Visibility = Visibility.Collapsed;
				_foregroundButtonRight.Visibility = Visibility.Visible;
				_foregroundTextBlock.Text = TextChecked;
				_foregroundTextBlock.Foreground = ForegroundChecked;
			}
			else
			{
				_backgroundTextBox.Background = BackgroundUnchecked;
				_foregroundButtonLeft.Visibility = Visibility.Visible;
				_foregroundButtonRight.Visibility = Visibility.Collapsed;
				_foregroundTextBlock.Text = TextUnchecked;
				_foregroundTextBlock.Foreground = ForegroundUnchecked;
			}
		}
	}
}
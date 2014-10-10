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
	/// Interaction logic for SlidingToggleButton.xaml
	/// </summary>
	public partial class SlidingToggleButton : UserControl
	{
		#region Dependency Property

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
				new FrameworkPropertyMetadata(
					18D,
					OnWidthChanged));

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
						((SlidingToggleButton)d).IsCheckedCopy = (bool)e.NewValue;
						OnAppearanceChanged(d, e);
					}));

		/// <summary>
		/// Copy of IsChecked property for binding to another source
		/// </summary>
		public bool IsCheckedCopy
		{
			get { return (bool)GetValue(IsCheckedCopyProperty); }
			set { SetValue(IsCheckedCopyProperty, value); }
		}
		public static readonly DependencyProperty IsCheckedCopyProperty =
			DependencyProperty.Register(
				"IsCheckedCopy",
				typeof(bool),
				typeof(SlidingToggleButton),
				new FrameworkPropertyMetadata(false));

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
				new FrameworkPropertyMetadata(
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
				new FrameworkPropertyMetadata(
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
				new FrameworkPropertyMetadata(
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
				new FrameworkPropertyMetadata(
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
				new FrameworkPropertyMetadata(
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
				new FrameworkPropertyMetadata(
					Brushes.Gray,
					OnAppearanceChanged));

		#endregion


		public SlidingToggleButton()
		{
			InitializeComponent();
		}

		static SlidingToggleButton()
		{
			FrameworkElement.WidthProperty.OverrideMetadata(
				typeof(SlidingToggleButton),
				new FrameworkPropertyMetadata(
					60D,
					OnWidthChanged));
		}


		private static void OnWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var button = (SlidingToggleButton)d;

			button.ForegroundButtonLeft.Width = button.InnerButtonWidth;
			button.ForegroundBox.Width = button.Width - button.InnerButtonWidth;
			button.ForegroundButtonRight.Width = button.InnerButtonWidth;
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
			if (IsChecked)
			{
				ForegroundButtonLeft.Visibility = Visibility.Collapsed;
				ForegroundButtonRight.Visibility = Visibility.Visible;
				ForegroundText.Text = TextChecked;
				ForegroundText.Foreground = ForegroundChecked;
				BackgroundBox.Background = BackgroundChecked;
			}
			else
			{
				ForegroundButtonLeft.Visibility = Visibility.Visible;
				ForegroundButtonRight.Visibility = Visibility.Collapsed;
				ForegroundText.Text = TextUnchecked;
				ForegroundText.Foreground = ForegroundUnchecked;
				BackgroundBox.Background = BackgroundUnchecked;
			}
		}
	}
}
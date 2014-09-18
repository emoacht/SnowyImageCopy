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
		public SlidingToggleButton()
		{
			InitializeComponent();
		}


		#region Dependency Property

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
					OnPropertyChanged));

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
				new FrameworkPropertyMetadata("On", OnPropertyChanged));

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
				new FrameworkPropertyMetadata("Off", OnPropertyChanged));
		
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
				new FrameworkPropertyMetadata(Brushes.Black, OnPropertyChanged));

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
				new FrameworkPropertyMetadata(Brushes.Black, OnPropertyChanged));

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
				new FrameworkPropertyMetadata(Brushes.SkyBlue, OnPropertyChanged));

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
				new FrameworkPropertyMetadata(Brushes.Gray, OnPropertyChanged));

		#endregion


		private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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

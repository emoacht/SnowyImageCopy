using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace SnowyImageCopy.Views.Controls
{
	public class CheckTextBox : TextBox
	{
		public CheckTextBox()
		{ }

		static CheckTextBox()
		{
			TextBox.TextProperty.OverrideMetadata(
				typeof(CheckTextBox),
				new FrameworkPropertyMetadata(
					String.Empty,
					(d, e) =>
					{
						var textBox = (CheckTextBox)d;

						textBox.CompareText(textBox.CheckText, (String)e.NewValue);
					}));

			TextBox.VisibilityProperty.OverrideMetadata(
				typeof(CheckTextBox),
				new FrameworkPropertyMetadata(
					Visibility.Visible,
					(d, e) =>
					{
						if ((Visibility)e.NewValue != Visibility.Visible)
							((CheckTextBox)d).IsChecked = false;
					}));
		}


		#region Property

		public String MessageText
		{
			get { return (String)GetValue(MessageTextProperty); }
			set { SetValue(MessageTextProperty, value); }
		}
		public static readonly DependencyProperty MessageTextProperty =
			DependencyProperty.Register(
				"MessageText",
				typeof(String),
				typeof(CheckTextBox),
				new FrameworkPropertyMetadata(String.Empty));

		public string CheckText
		{
			get { return (string)GetValue(CheckTextProperty); }
			set { SetValue(CheckTextProperty, value); }
		}
		public static readonly DependencyProperty CheckTextProperty =
			DependencyProperty.Register(
				"CheckText",
				typeof(string),
				typeof(CheckTextBox),
				new FrameworkPropertyMetadata(
					String.Empty,
					(d, e) =>
					{
						var textBox = (CheckTextBox)d;

						textBox.CompareText((String)e.NewValue, textBox.Text);
					}));

		public bool IsChecked
		{
			get { return (bool)GetValue(IsCheckedProperty); }
			set { SetValue(IsCheckedProperty, value); }
		}
		public static readonly DependencyProperty IsCheckedProperty =
			ToggleButton.IsCheckedProperty.AddOwner(
				typeof(CheckTextBox),
				new FrameworkPropertyMetadata(
					false,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
					(d, e) => ((CheckTextBox)d).ReflectChecked((bool)e.NewValue)));

		#endregion


		#region Message

		private bool _isMessage;

		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			if ((e.Property != VisibilityProperty) || String.IsNullOrEmpty(MessageText))
				return;

			if ((Visibility)e.NewValue == Visibility.Visible)
			{
				if (String.IsNullOrWhiteSpace(this.Text))
				{
					_isMessage = true;
					this.Text = MessageText;
				}
			}
			else
			{
				if (_isMessage)
				{
					_isMessage = false;
					this.Text = String.Empty;
				}
			}
		}

		protected override void OnGotFocus(RoutedEventArgs e)
		{
			base.OnGotFocus(e);

			if (String.IsNullOrEmpty(MessageText))
				return;

			if ((this.Visibility == Visibility.Visible) && _isMessage)
			{
				_isMessage = false;
				this.Text = String.Empty;
			}
		}

		protected override void OnLostFocus(RoutedEventArgs e)
		{
			base.OnLostFocus(e);

			if (String.IsNullOrEmpty(MessageText))
				return;

			if ((this.Visibility == Visibility.Visible) && String.IsNullOrWhiteSpace(this.Text))
			{
				_isMessage = true;
				this.Text = MessageText;
			}
		}

		#endregion


		#region Check

		private bool _isChanged;

		private void CompareText(string baseText, string inputText)
		{
			if (_isChanged)
				return;

			try
			{
				_isChanged = true;

				IsChecked = baseText.Equals(inputText, StringComparison.Ordinal);
			}
			finally
			{
				_isChanged = false;
			}
		}

		private void ReflectChecked(bool isChecked)
		{
			if (_isChanged)
				return;

			try
			{
				_isChanged = true;

				if (isChecked)
				{
					this.Text = CheckText;
					this.Visibility = Visibility.Visible;
				}
				else
				{
					this.Text = String.Empty;
				}
			}
			finally
			{
				_isChanged = false;
			}
		}

		#endregion
	}
}
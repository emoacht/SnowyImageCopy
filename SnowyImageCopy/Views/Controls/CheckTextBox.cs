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
	public class CheckTextBox : TextBox
	{
		#region Dependency Property

		public string BaseText
		{
			get { return (string)GetValue(BaseTextProperty); }
			set { SetValue(BaseTextProperty, value); }
		}
		public static readonly DependencyProperty BaseTextProperty =
			DependencyProperty.Register(
				"BaseText",
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


		public CheckTextBox() : base() { }

		static CheckTextBox()
		{
			TextBox.TextProperty.OverrideMetadata(
				typeof(CheckTextBox),
				new FrameworkPropertyMetadata(
					String.Empty,
					(d, e) =>
					{
						var textBox = (CheckTextBox)d;

						textBox.CompareText(textBox.BaseText, (String)e.NewValue);
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

		private bool isChanged = false;

		private void CompareText(string baseText, string inputText)
		{
			if (isChanged)
				return;

			try
			{
				isChanged = true;

				this.IsChecked = baseText.Equals(inputText, StringComparison.Ordinal);
			}
			finally
			{
				isChanged = false;
			}
		}

		private void ReflectChecked(bool isChecked)
		{
			if (isChanged)
				return;

			try
			{
				isChanged = true;

				if (isChecked)
				{
					this.Text = BaseText;
					this.Visibility = Visibility.Visible;
				}
				else
				{
					this.Text = String.Empty;
				}
			}
			finally
			{
				isChanged = false;
			}
		}
	}
}

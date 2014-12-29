using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SnowyImageCopy.Views.Controls
{
	[TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
	[TemplateVisualState(Name = "MouseOver", GroupName = "CommonStates")]
	[TemplateVisualState(Name = "Pressed", GroupName = "CommonStates")]
	[TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
	[TemplateVisualState(Name = "Busy", GroupName = "CommonStates")]
	//[TemplateVisualState(Name = "Focused", GroupName = "FocusedStates")]
	//[TemplateVisualState(Name = "Unfocused", GroupName = "FocusedStates")]
	public class BusyButton : Button
	{
		#region Property

		public bool IsBusy
		{
			get { return (bool)GetValue(IsBusyProperty); }
			set { SetValue(IsBusyProperty, value); }
		}
		public static readonly DependencyProperty IsBusyProperty =
			DependencyProperty.Register(
				"IsBusy",
				typeof(bool),
				typeof(BusyButton),
				new FrameworkPropertyMetadata(false));

		#endregion


		protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
		{
			base.OnPropertyChanged(e);

			//Debug.WriteLine(String.Format("Property changed: {0}", e.Property.Name));

			if ((e.Property == Button.IsMouseOverProperty) ||
				(e.Property == Button.IsPressedProperty) ||
				(e.Property == Button.IsKeyboardFocusedProperty) || // This seems to be necessary to catch visual state change by Command.
				(e.Property == Button.IsEnabledProperty) ||
				(e.Property == BusyButton.IsBusyProperty))
				UpdateStates(true);
		}

		private void UpdateStates(bool useTransitions)
		{
			// CommonStates
			if (IsBusy)
			{
				VisualStateManager.GoToState(this, "Busy", useTransitions);
			}
			else if (!IsEnabled)
			{
				VisualStateManager.GoToState(this, "Disabled", useTransitions);
			}
			else if (IsPressed)
			{
				VisualStateManager.GoToState(this, "Pressed", useTransitions);
			}
			else if (IsMouseOver)
			{
				VisualStateManager.GoToState(this, "MouseOver", useTransitions);
			}
			else
			{
				VisualStateManager.GoToState(this, "Normal", useTransitions);
			}

			//// FocusStates
			//if (IsKeyboardFocused)
			//{
			//	VisualStateManager.GoToState(this, "Focused", useTransitions);
			//}
			//else
			//{
			//	VisualStateManager.GoToState(this, "Unfocused", useTransitions);
			//}
		}
	}
}
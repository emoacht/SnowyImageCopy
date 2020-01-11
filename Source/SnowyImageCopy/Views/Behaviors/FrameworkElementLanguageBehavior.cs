using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Xaml.Behaviors;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Sets FrameworkElement language.
	/// </summary>
	[TypeConstraint(typeof(FrameworkElement))]
	public class FrameworkElementLanguageBehavior : Behavior<FrameworkElement>
	{
		#region Property

		public string IetfLanguageTag
		{
			get { return (string)GetValue(IetfLanguageTagProperty); }
			set { SetValue(IetfLanguageTagProperty, value); }
		}
		public static readonly DependencyProperty IetfLanguageTagProperty =
			DependencyProperty.Register(
				"IetfLanguageTag",
				typeof(string),
				typeof(FrameworkElementLanguageBehavior),
				new FrameworkPropertyMetadata(
					string.Empty,
					(d, e) => ((FrameworkElementLanguageBehavior)d).SetLanguage((string)e.NewValue)));

		#endregion

		private void SetLanguage(string ietfLanguageTag)
		{
			if (string.IsNullOrEmpty(ietfLanguageTag))
				ietfLanguageTag = CultureInfo.CurrentUICulture.ToString();

			try
			{
				this.AssociatedObject.Language = XmlLanguage.GetLanguage(ietfLanguageTag);
			}
			catch
			{
				Debug.WriteLine("Failed to set FrameworkElement language.");
				throw;
			}
		}
	}
}
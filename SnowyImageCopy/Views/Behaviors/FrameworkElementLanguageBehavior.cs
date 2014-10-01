using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Markup;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Set Language property of FrameworkElement.
	/// </summary>
	public class FrameworkElementLanguageBehavior : Behavior<FrameworkElement>
	{
		#region Dependency Property

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
					String.Empty,
					(d, e) => ((FrameworkElementLanguageBehavior)d).SetLanguage((string)e.NewValue)));

		#endregion


		private void SetLanguage(string ietfCultureTag)
		{
			if (String.IsNullOrEmpty(ietfCultureTag))
				return;

			this.AssociatedObject.Language = XmlLanguage.GetLanguage(ietfCultureTag);
		}
	}
}
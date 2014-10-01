using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;
using System.Windows.Navigation;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Manage WebBrowser.
	/// </summary>
	public class BrowserBehavior : Behavior<WebBrowser>
	{
		#region Dependency Property

		/// <summary>
		/// Target file
		/// </summary>
		public string TargetFile
		{
			get { return (string)GetValue(TargetFileProperty); }
			set { SetValue(TargetFileProperty, value); }
		}
		public static readonly DependencyProperty TargetFileProperty =
			DependencyProperty.Register(
				"TargetFile",
				typeof(string),
				typeof(BrowserBehavior),
				new FrameworkPropertyMetadata(String.Empty));

		/// <summary>
		/// Alternate text when target file does not exist
		/// </summary>
		public string AlternateText
		{
			get { return (string)GetValue(AlternateTextProperty); }
			set { SetValue(AlternateTextProperty, value); }
		}
		public static readonly DependencyProperty AlternateTextProperty =
			DependencyProperty.Register(
				"AlternateText",
				typeof(string),
				typeof(BrowserBehavior),
				new FrameworkPropertyMetadata(String.Empty));

		#endregion


		protected override void OnAttached()
		{
			base.OnAttached();

			this.AssociatedObject.IsVisibleChanged += OnVisibleChanged;
			this.AssociatedObject.Navigating += OnNavigating;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (this.AssociatedObject == null)
				return;

			this.AssociatedObject.IsVisibleChanged -= OnVisibleChanged;
			this.AssociatedObject.Navigating -= OnNavigating;
		}


		private bool isApplying;

		private void OnVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			isApplying = true;

			if (!(bool)e.NewValue)
			{
				this.AssociatedObject.Navigate("about:blank"); // Or (Uri)null
				return;
			}

			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TargetFile);
			if (File.Exists(RemoveAnchor(path)))
				this.AssociatedObject.Navigate(path);
			else
				this.AssociatedObject.NavigateToString(AlternateText);

			// OnNavigating event may be fired after exiting this method.
		}

		private void OnNavigating(object sender, NavigatingCancelEventArgs e)
		{
			if (isApplying)
			{
				isApplying = false;
				return;
			}

			// Cancel navigating and open external browser instead.
			e.Cancel = true;
			Process.Start(e.Uri.OriginalString);
		}


		#region Helper

		private static readonly Regex anchorPattern = new Regex(@"\.(htm|html)#.*", RegexOptions.Compiled);

		/// <summary>
		/// Remove anchor at the end of htm/html file path.
		/// </summary>
		/// <param name="source">Source file path</param>
		/// <returns>File path without anchor</returns>
		private static string RemoveAnchor(string source)
		{
			if (String.IsNullOrWhiteSpace(source))
				return source;

			var match = anchorPattern.Match(source);
			if (!match.Success)
				return source;

			return source.Substring(0, match.Index + match.Value.IndexOf('#'));
		}

		#endregion
	}
}

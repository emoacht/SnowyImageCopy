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
	/// Manage <see cref="WebBrowser"/>.
	/// </summary>
	[TypeConstraint(typeof(WebBrowser))]
	public class BrowserBehavior : Behavior<WebBrowser>
	{
		#region Property

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

			this.AssociatedObject.IsVisibleChanged += OnIsVisibleChanged;
			this.AssociatedObject.Navigating += OnNavigating;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			if (this.AssociatedObject == null)
				return;

			this.AssociatedObject.IsVisibleChanged -= OnIsVisibleChanged;
			this.AssociatedObject.Navigating -= OnNavigating;
		}


		private bool _isApplying;

		private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			_isApplying = true;

			if (!(bool)e.NewValue)
			{
				this.AssociatedObject.Navigate("about:blank"); // Or (Uri)null
				return;
			}

			var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TargetFile);
			if (File.Exists(RemoveFragment(path)))
				this.AssociatedObject.Navigate(path);
			else
				this.AssociatedObject.NavigateToString(AlternateText);

			// OnNavigating event may be fired after exiting this method.
		}

		private void OnNavigating(object sender, NavigatingCancelEventArgs e)
		{
			if (_isApplying)
			{
				_isApplying = false;
				return;
			}

			// Cancel navigating and open external browser instead.
			e.Cancel = true;
			Process.Start(e.Uri.OriginalString);
		}


		#region Helper

		private static readonly Regex _fragmentPattern = new Regex(@"(?<path>.+\.(?:htm|html))#.*");

		/// <summary>
		/// Remove fragment at the end of htm/html file path.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <returns>File path without fragment</returns>
		/// <remarks>Uri.GetLeftPart(UriPartial.Path) method does not work for file path.</remarks>
		private static string RemoveFragment(string filePath)
		{
			if (String.IsNullOrWhiteSpace(filePath))
				return filePath;

			var match = _fragmentPattern.Match(filePath);
			if (!match.Success)
				return filePath;

			return match.Groups["path"].Value;
		}

		#endregion
	}
}
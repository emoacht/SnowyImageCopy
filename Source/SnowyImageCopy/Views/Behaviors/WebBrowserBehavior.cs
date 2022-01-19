using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Xaml.Behaviors;

using MonitorAware.Models;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Manages <see cref="System.Windows.Controls.WebBrowser"/>.
	/// </summary>
	[TypeConstraint(typeof(WebBrowser))]
	public class WebBrowserBehavior : Behavior<WebBrowser>
	{
		#region Property

		/// <summary>
		/// Source file path
		/// </summary>
		public string SourcePath
		{
			get { return (string)GetValue(SourcePathProperty); }
			set { SetValue(SourcePathProperty, value); }
		}
		public static readonly DependencyProperty SourcePathProperty =
			DependencyProperty.Register(
				"SourcePath",
				typeof(string),
				typeof(WebBrowserBehavior),
				new PropertyMetadata(default(string)));

		/// <summary>
		/// Source text
		/// </summary>
		public string SourceText
		{
			get { return (string)GetValue(SourceTextProperty); }
			set { SetValue(SourceTextProperty, value); }
		}
		public static readonly DependencyProperty SourceTextProperty =
			DependencyProperty.Register(
				"SourceText",
				typeof(string),
				typeof(WebBrowserBehavior),
				new PropertyMetadata(default(string)));

		public DpiScale WindowDpi
		{
			get { return (DpiScale)GetValue(WindowDpiProperty); }
			set { SetValue(WindowDpiProperty, value); }
		}
		public static readonly DependencyProperty WindowDpiProperty =
			DependencyProperty.Register(
				"WindowDpi",
				typeof(DpiScale),
				typeof(WebBrowserBehavior),
				new PropertyMetadata(DpiHelper.Identity));

		public int ZoomPercentage { get; private set; } = 0;

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

			this.AssociatedObject.IsVisibleChanged -= OnIsVisibleChanged;
			this.AssociatedObject.Navigating -= OnNavigating;
		}

		private bool _isLoading;

		private async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (!(bool)e.NewValue)
				return;

			// WebBrowser's default zoom seems to depend on System DPI and so must be set correctly
			// when it is shown first time.
			var percentage = (int)(WindowDpi.DpiScaleX * 100);
			if (ZoomPercentage != percentage)
			{
				// Load dummy text (neither null nor empty) and set zoom before loading actual file or text.
				// This is because setting zoom will cause unintended scrolling.
				var tcs = new TaskCompletionSource<bool>();

				void OnLoadCompleted(object sender, NavigationEventArgs e)
				{
					this.AssociatedObject.LoadCompleted -= OnLoadCompleted;
					tcs.SetResult(true);
				}

				_isLoading = true;
				this.AssociatedObject.LoadCompleted += OnLoadCompleted;
				this.AssociatedObject.NavigateToString("LOADING" /* dummy text */);
				await tcs.Task;

				if (TrySetZoom(percentage))
					ZoomPercentage = percentage;
			}

			if (!string.IsNullOrEmpty(SourcePath) && File.Exists(RemoveFragment(SourcePath)))
			{
				_isLoading = true;
				this.AssociatedObject.Navigate(SourcePath);
			}
			else if (!string.IsNullOrEmpty(SourceText))
			{
				_isLoading = true;
				this.AssociatedObject.NavigateToString(SourceText);
			}

			// Navigating event will be fired after exiting this method.

		}

		private void OnNavigating(object sender, NavigatingCancelEventArgs e)
		{
			if (_isLoading)
			{
				_isLoading = false;
				return;
			}

			// Cancel navigating and open external browser instead.
			e.Cancel = true;
			Process.Start(e.Uri.OriginalString);
		}

		private dynamic _browser;

		private bool TrySetZoom(int percentage)
		{
			// Parameters derived from DocObj.h
			const int OLECMDID_OPTICAL_ZOOM = 63;
			const int OLECMDEXECOPT_DONTPROMPTUSER = 2;

			_browser ??= typeof(WebBrowser).GetProperty("AxIWebBrowser2", BindingFlags.Instance | BindingFlags.NonPublic)
				?.GetValue(this.AssociatedObject);
			if (_browser is null)
				return false;

			try
			{
				_browser.ExecWB(OLECMDID_OPTICAL_ZOOM, OLECMDEXECOPT_DONTPROMPTUSER, percentage, IntPtr.Zero);
				return true;
			}
			catch (COMException ce)
			{
				Debug.WriteLine($"Failed to set zoom of WebBrowser.\r\n{ce}");
				return false;
			}
		}

		#region Helper

		/// <summary>
		/// Removes fragment at the end of htm/html file path.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <returns>File path without fragment</returns>
		/// <remarks>Uri.GetLeftPart(UriPartial.Path) method does not work for file path.</remarks>
		private static string RemoveFragment(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				return filePath;

			var match = new Regex(@"(?<path>.+\.(?:htm|html))#.*").Match(filePath);
			if (!match.Success)
				return filePath;

			return match.Groups["path"].Value;
		}

		#endregion
	}
}
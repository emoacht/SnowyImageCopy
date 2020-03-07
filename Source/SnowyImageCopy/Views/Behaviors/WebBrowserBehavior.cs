﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Xaml.Behaviors;

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
				typeof(WebBrowserBehavior),
				new FrameworkPropertyMetadata(string.Empty));

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
				typeof(WebBrowserBehavior),
				new FrameworkPropertyMetadata(string.Empty));

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

			if (this.AssociatedObject is null)
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

		/// <summary>
		/// Removes fragment at the end of htm/html file path.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <returns>File path without fragment</returns>
		/// <remarks>Uri.GetLeftPart(UriPartial.Path) method does not work for file path.</remarks>
		private static string RemoveFragment(string filePath)
		{
			const string fragmentPattern = @"(?<path>.+\.(?:htm|html))#.*";

			if (string.IsNullOrWhiteSpace(filePath))
				return filePath;

			var match = new Regex(fragmentPattern).Match(filePath);
			if (!match.Success)
				return filePath;

			return match.Groups["path"].Value;
		}

		#endregion
	}
}
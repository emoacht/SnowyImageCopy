using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Xaml.Behaviors;

using MonitorAware.Models;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Adjusts minimum size of attached Window depending on DPI.
	/// </summary>
	[TypeConstraint(typeof(Window))]
	public class WindowSizeBehavior : Behavior<Window>
	{
		#region Property

		public DpiScale WindowDpi
		{
			get { return (DpiScale)GetValue(WindowDpiProperty); }
			set { SetValue(WindowDpiProperty, value); }
		}
		public static readonly DependencyProperty WindowDpiProperty =
			DependencyProperty.Register(
				"WindowDpi",
				typeof(DpiScale),
				typeof(WindowSizeBehavior),
				new PropertyMetadata(
					DpiHelper.Identity,
					(d, e) => ((WindowSizeBehavior)d).AdjustMinSize((DpiScale)e.NewValue)));

		public DpiScale SystemDpi
		{
			get { return (DpiScale)GetValue(SystemDpiProperty); }
			set { SetValue(SystemDpiProperty, value); }
		}
		public static readonly DependencyProperty SystemDpiProperty =
			DependencyProperty.Register(
				"SystemDpi",
				typeof(DpiScale),
				typeof(WindowSizeBehavior),
				new PropertyMetadata(DpiHelper.Identity));

		#endregion

		private double _defaultMinWidth;
		private double _defaultMinHeight;

		protected override void OnAttached()
		{
			base.OnAttached();

			_defaultMinWidth = this.AssociatedObject.MinWidth;
			_defaultMinHeight = this.AssociatedObject.MinHeight;

			this.AssociatedObject.Activated += OnActivated;
		}

		private void OnActivated(object sender, EventArgs e)
		{
			this.AssociatedObject.Activated -= OnActivated;

			AdjustMinSize(WindowDpi);
		}

		private void AdjustMinSize(DpiScale windowDpi)
		{
			if (this.AssociatedObject is null)
				return;

			// This calculation is incorrect because it doesn't take into account DWM window bounds.
			// However, it would be enough for settings MinWidth and MinHeight.
			this.AssociatedObject.MinWidth = _defaultMinWidth * windowDpi.DpiScaleX / SystemDpi.DpiScaleX;
			this.AssociatedObject.MinHeight = _defaultMinHeight * windowDpi.DpiScaleY / SystemDpi.DpiScaleY;
		}
	}
}
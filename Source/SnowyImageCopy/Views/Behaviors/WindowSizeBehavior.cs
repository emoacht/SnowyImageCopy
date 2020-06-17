using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using Microsoft.Xaml.Behaviors;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Adjusts minimum size of attached Window depending on DPI.
	/// </summary>
	[TypeConstraint(typeof(Window))]
	public class WindowSizeBehavior : Behavior<Window>
	{
		#region Property

		public Point ScaleFactor
		{
			get { return (Point)GetValue(ScaleFactorProperty); }
			set { SetValue(ScaleFactorProperty, value); }
		}
		public static readonly DependencyProperty ScaleFactorProperty =
			DependencyProperty.Register(
				"ScaleFactor",
				typeof(Point),
				typeof(WindowSizeBehavior),
				new PropertyMetadata(
					new Point(1D, 1D),
					(d, e) => ((WindowSizeBehavior)d).AdjustMinSize((Point)e.NewValue)));

		#endregion

		private double _defaultMinWidth;
		private double _defaultMinHeight;

		protected override void OnAttached()
		{
			base.OnAttached();

			_defaultMinWidth = this.AssociatedObject.MinWidth;
			_defaultMinHeight = this.AssociatedObject.MinHeight;
		}

		private void AdjustMinSize(Point scaleFactor)
		{
			if (this.AssociatedObject is null)
				return;

			this.AssociatedObject.MinWidth = _defaultMinWidth * scaleFactor.X;
			this.AssociatedObject.MinHeight = _defaultMinHeight * scaleFactor.Y;
		}
	}
}
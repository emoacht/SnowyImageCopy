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
	/// Checks intersection of multiple FrameworkElements.
	/// </summary>
	[TypeConstraint(typeof(FrameworkElement))]
	public class FrameworkElementIntersectionBehavior : Behavior<FrameworkElement>
	{
		#region Property

		/// <summary>
		/// Associated FrameworkElement
		/// </summary>
		private FrameworkElement AssociatedElement => this.AssociatedObject;

		/// <summary>
		/// Target FrameworkElement for checking
		/// </summary>
		public FrameworkElement TargetElement
		{
			get { return (FrameworkElement)GetValue(TargetElementProperty); }
			set { SetValue(TargetElementProperty, value); }
		}
		public static readonly DependencyProperty TargetElementProperty =
			DependencyProperty.Register(
				"TargetElement",
				typeof(FrameworkElement),
				typeof(FrameworkElementIntersectionBehavior),
				new PropertyMetadata(default(FrameworkElement)));

		/// <summary>
		/// Expanded margin of associated FrameworkElement for checking
		/// </summary>
		public Thickness ExpandedMargin
		{
			get { return (Thickness)GetValue(ExpandedMarginProperty); }
			set { SetValue(ExpandedMarginProperty, value); }
		}
		public static readonly DependencyProperty ExpandedMarginProperty =
			DependencyProperty.Register(
				"ExpandedMargin",
				typeof(Thickness),
				typeof(FrameworkElementIntersectionBehavior),
				new PropertyMetadata(default(Thickness)));

		public DpiScale WindowDpi
		{
			get { return (DpiScale)GetValue(WindowDpiProperty); }
			set { SetValue(WindowDpiProperty, value); }
		}
		public static readonly DependencyProperty WindowDpiProperty =
			DependencyProperty.Register(
				"WindowDpi",
				typeof(DpiScale),
				typeof(FrameworkElementIntersectionBehavior),
				new PropertyMetadata(DpiHelper.Identity));

		/// <summary>
		/// Whether associated FrameworkElement is intersected with target FrameworkElement
		/// </summary>
		public bool IsIntersected
		{
			get { return (bool)GetValue(IsIntersectedProperty); }
			set { SetValue(IsIntersectedProperty, value); }
		}
		public static readonly DependencyProperty IsIntersectedProperty =
			DependencyProperty.Register(
				"IsIntersected",
				typeof(bool),
				typeof(FrameworkElementIntersectionBehavior),
				new PropertyMetadata(false));

		/// <summary>
		/// Object to trigger checking
		/// </summary>
		/// <remarks>Any kinds of object will be accepted.</remarks>
		public object TriggerObject
		{
			get { return (object)GetValue(TriggerObjectProperty); }
			set { SetValue(TriggerObjectProperty, value); }
		}
		public static readonly DependencyProperty TriggerObjectProperty =
			DependencyProperty.Register(
				"TriggerObject",
				typeof(object),
				typeof(FrameworkElementIntersectionBehavior),
				new PropertyMetadata(
					null,
					(d, e) => ((FrameworkElementIntersectionBehavior)d).CheckElements()));

		#endregion

		private void CheckElements(bool retry = true)
		{
			// Check if AssociatedElement and TargetElement are assigned and Visibility properties
			// are set to Visibility.Visible (It does not necessarily mean they are visible).
			if ((AssociatedElement is null) ||
				(AssociatedElement.Visibility == Visibility.Collapsed) ||
				(TargetElement is null) ||
				(TargetElement.Visibility == Visibility.Collapsed))
				return;

			// Check if AssociatedElement and TargetElement have been already visible.
			// If not, an InvalidOperationException ("This Visual is not connected to a PresentationSource")
			// will be thrown when calling PointToScreen method.
			if (AssociatedElement.IsVisible &&
				TargetElement.IsVisible)
			{
				var isIntersected = IsElementIntersected();
				if (this.IsIntersected != isIntersected)
				{
					this.IsIntersected = isIntersected;
					return;
				}
			}

			if (retry)
				AssociatedElement.LayoutUpdated += OnLayoutUpdated;
		}

		private void OnLayoutUpdated(object sender, EventArgs e)
		{
			AssociatedElement.LayoutUpdated -= OnLayoutUpdated;

			CheckElements(false);
		}

		private bool IsElementIntersected()
		{
			// Compute factor from default DPI to Window DPI.
			var factor = new { X = WindowDpi.DpiScaleX, Y = WindowDpi.DpiScaleY };

			var associatedLocation = AssociatedElement.PointToScreen(default);
			var expandedRect = new Rect(
				associatedLocation.X - ExpandedMargin.Left * factor.X,
				associatedLocation.Y - ExpandedMargin.Top * factor.Y,
				(AssociatedElement.ActualWidth + ExpandedMargin.Left + ExpandedMargin.Right) * factor.X,
				(AssociatedElement.ActualHeight + ExpandedMargin.Top + ExpandedMargin.Bottom) * factor.Y);

			var targetLocation = TargetElement.PointToScreen(default);
			var targetRect = new Rect(
				targetLocation.X,
				targetLocation.Y,
				TargetElement.ActualWidth * factor.X,
				TargetElement.ActualHeight * factor.Y);

			return IsRectIntersected(new[] { expandedRect, targetRect });
		}

		private static bool IsRectIntersected(Rect[] rects)
		{
			for (int i = 0; i < rects.Length; i++)
			{
				for (int j = i + 1; j < rects.Length; j++)
				{
					if (rects[i].IntersectsWith(rects[j]))
						return true;
				}
			}
			return false;
		}
	}
}
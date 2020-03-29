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
		/// Target FrameworkElement for checking
		/// </summary>
		public FrameworkElement TargetFrameworkElement
		{
			get { return (FrameworkElement)GetValue(TargetFrameworkElementProperty); }
			set { SetValue(TargetFrameworkElementProperty, value); }
		}
		public static readonly DependencyProperty TargetFrameworkElementProperty =
			DependencyProperty.Register(
				"TargetFrameworkElement",
				typeof(FrameworkElement),
				typeof(FrameworkElementIntersectionBehavior),
				new PropertyMetadata(default(FrameworkElement)));

		/// <summary>
		/// Object to trigger checking
		/// </summary>
		/// <remarks>Any kind of object will be accepted.</remarks>
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
					(d, e) => ((FrameworkElementIntersectionBehavior)d).CheckIntersection()));

		/// <summary>
		/// Expanded margin of this FrameworkElement for checking
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

		/// <summary>
		/// Whether this FrameworkElement is intersected with target FrameworkElement
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

		public Dpi WindowDpi
		{
			get { return (Dpi)GetValue(WindowDpiProperty); }
			set { SetValue(WindowDpiProperty, value); }
		}
		public static readonly DependencyProperty WindowDpiProperty =
			DependencyProperty.Register(
				"WindowDpi",
				typeof(Dpi),
				typeof(FrameworkElementIntersectionBehavior),
				new PropertyMetadata(Dpi.Default));

		#endregion

		private void CheckIntersection()
		{
			if ((this.AssociatedObject is null) || (TargetFrameworkElement is null))
				return;

			IsIntersected = IsFrameworkElementIntersected(this.AssociatedObject, new[] { TargetFrameworkElement });
		}

		private bool IsFrameworkElementIntersected(FrameworkElement baseElement, IEnumerable<FrameworkElement> targetElements)
		{
			if (!baseElement.IsVisible) // If not visible, PointToScreen method will fail.
				return false;

			// Compute factor from default DPI to Window DPI.
			var factor = new { X = (double)WindowDpi.X / Dpi.Default.X, Y = (double)WindowDpi.Y / Dpi.Default.Y };

			var baseElementLocation = baseElement.PointToScreen(default);

			var expandedRect = new Rect(
				baseElementLocation.X - ExpandedMargin.Left * factor.X,
				baseElementLocation.Y - ExpandedMargin.Top * factor.Y,
				(baseElement.ActualWidth + ExpandedMargin.Left + ExpandedMargin.Right) * factor.X,
				(baseElement.ActualHeight + ExpandedMargin.Top + ExpandedMargin.Bottom) * factor.Y);

			var rects = new[] { expandedRect }
				.Concat(targetElements
					.Where(x => x.IsVisible) // If not visible, PointToScreen method will fail.
					.Select(x => new Rect(x.PointToScreen(default), new Size(x.ActualWidth * factor.X, x.ActualHeight * factor.Y))))
				.ToArray();

			return IsRectIntersected(rects);
		}

		private bool IsFrameworkElementIntersected(IEnumerable<FrameworkElement> elements)
		{
			var rects = elements
				.Where(x => x.IsVisible) // If not visible, PointToScreen method will fail.
				.Select(x => new Rect(x.PointToScreen(default), new Size(x.ActualWidth, x.ActualHeight)))
				.ToArray();

			return IsRectIntersected(rects);
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
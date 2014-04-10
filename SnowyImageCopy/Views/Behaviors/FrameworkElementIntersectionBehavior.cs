using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Check intersection of multiple FrameworkElements.
	/// </summary>
	public class FrameworkElementIntersectionBehavior : Behavior<FrameworkElement>
	{
		#region Dependency Property

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
				new FrameworkPropertyMetadata(null));

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
				new FrameworkPropertyMetadata(null, (d, e) =>
					((FrameworkElementIntersectionBehavior)d).CheckIntersection()));

		/// <summary>
		/// Expanded margin of this FrameworkElement for checking.
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
				new FrameworkPropertyMetadata(new Thickness(0)));

		/// <summary>
		/// Whether this FrameworkElement is intersected with target FrameworkElement.
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
				new FrameworkPropertyMetadata(false));

		#endregion


		private void CheckIntersection()
		{
			if ((this.AssociatedObject == null) || (TargetFrameworkElement == null))
				return;

			IsIntersected = IsFrameworkElementIntersected(this.AssociatedObject, new FrameworkElement[] { TargetFrameworkElement });
		}

		private bool IsFrameworkElementIntersected(FrameworkElement baseElement, IEnumerable<FrameworkElement> targetElements)
		{
			if (!baseElement.IsVisible) // If not visible, PointToScreen method will fail.
				return false;

			var baseElementLocation = baseElement.PointToScreen(new Point(0D, 0D));

			var expandedRect = new Rect(
					baseElementLocation.X - ExpandedMargin.Left,
					baseElementLocation.Y - ExpandedMargin.Top,
					baseElement.ActualWidth + ExpandedMargin.Left + ExpandedMargin.Right,
					baseElement.ActualHeight + ExpandedMargin.Top + ExpandedMargin.Bottom);

			var rects = new Rect[] { expandedRect }.Concat(targetElements
				.Where(x => x.IsVisible) // If not visible, PointToScreen method will fail.
				.Select(x => new Rect(x.PointToScreen(new Point(0D, 0D)), new Size(x.ActualWidth, x.ActualHeight))))
				.ToArray();

			return IsRectIntersected(rects);
		}

		private bool IsFrameworkElementIntersected(IEnumerable<FrameworkElement> elements)
		{
			var rects = elements
				.Where(x => x.IsVisible) // If not visible, PointToScreen method will fail.
				.Select(x => new Rect(x.PointToScreen(new Point(0D, 0D)), new Size(x.ActualWidth, x.ActualHeight)))
				.ToArray();

			return IsRectIntersected(rects);
		}

		private bool IsRectIntersected(Rect[] rects)
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

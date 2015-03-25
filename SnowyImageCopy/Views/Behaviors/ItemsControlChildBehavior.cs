using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interactivity;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Check child items inside the viewport of <see cref="ScrollViewer"/> in <see cref="ItemsControl"/>.
	/// </summary>
	[TypeConstraint(typeof(ItemsControl))]
	public class ItemsControlChildBehavior : Behavior<ItemsControl>
	{
		private ScrollViewer _viewer;
		private CompositeDisposable _subscription;

		protected override void OnAttached()
		{
			base.OnAttached();

			_subscription = new CompositeDisposable();
			this.AssociatedObject.Loaded += OnLoaded;
		}

		protected override void OnDetaching()
		{
			base.OnDetaching();

			_subscription.Dispose();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.AssociatedObject.Loaded -= OnLoaded;

			_viewer = VisualTreeHelperAddition.GetDescendants(this.AssociatedObject)
				.OfType<ScrollViewer>()
				.FirstOrDefault();
			if (_viewer == null)
				return;

			CheckChild(); // Initial check

			_subscription.Add(Observable.FromEventPattern<ScrollChangedEventHandler, ScrollChangedEventArgs>(
				handler => handler.Invoke,
				handler => _viewer.ScrollChanged += handler,
				handler => _viewer.ScrollChanged -= handler)
				//.Do(_ => Debug.WriteLine("ScrollChanged!"))
				.Subscribe(x => CheckChild()));

			var source = this.AssociatedObject.Items as INotifyCollectionChanged; // ItemsSource
			if (source == null)
				return;

			_subscription.Add(Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
				handler => handler.Invoke,
				handler => source.CollectionChanged += handler,
				handler => source.CollectionChanged -= handler)
				//.Do(_ => Debug.WriteLine("CollectionChanged!"))
				.Where(x => x.EventArgs.Action != NotifyCollectionChangedAction.Remove)
				.Where(x => x.EventArgs.NewItems != null)
				.Delay(TimeSpan.FromMilliseconds(10)) // Waiting time for child to perform Measure method
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(x => CheckChild(x.EventArgs.NewItems.Cast<object>())));
		}

		private const double _margin = 100D; // Vertical margin of viewport for checking
		private int _startIndex = 0; // Start index for checking

		private void CheckChild()
		{
			if (this.AssociatedObject.Items.Count == 0)
				return;

			var viewportRect = new Rect(0, -_margin, _viewer.RenderSize.Width, _viewer.RenderSize.Height + _margin);

			int firstIndex = -1;
			int lastIndex = -1;

			int upwardIndex = _startIndex;
			int downwardIndex = _startIndex + 1;

			var isUpwardChecking = true;
			var isDownwardChecking = true;

			while (isUpwardChecking || isDownwardChecking)
			{
				// Check upward.
				if (isUpwardChecking)
				{
					if (0 <= upwardIndex)
					{
						var child = this.AssociatedObject.ItemContainerGenerator.ContainerFromIndex(upwardIndex) as ContentControl;
						if (child != null)
						{
							if (IsIntersected(viewportRect, child))
							{
								firstIndex = upwardIndex;

								if (lastIndex == -1) // If not found yet
									lastIndex = upwardIndex;
							}
							else
							{
								if (firstIndex != -1) // If found already
									isUpwardChecking = false;
							}
						}

						upwardIndex--;
					}
					else
					{
						isUpwardChecking = false;
					}
				}

				// Check downward.
				if (isDownwardChecking)
				{
					if (downwardIndex < this.AssociatedObject.Items.Count)
					{
						var child = this.AssociatedObject.ItemContainerGenerator.ContainerFromIndex(downwardIndex) as ContentControl;
						if (child == null)
							continue;

						if (IsIntersected(viewportRect, child))
						{
							if (firstIndex == -1) // If not found yet
								firstIndex = downwardIndex;

							lastIndex = downwardIndex;
						}
						else
						{
							if (lastIndex != -1) // If found already
								isDownwardChecking = false;
						}

						downwardIndex++;
					}
					else
					{
						isDownwardChecking = false;
					}
				}
			}

			if (firstIndex == -1)
				firstIndex = 0; // Fallback
			if (lastIndex == -1)
				lastIndex = this.AssociatedObject.Items.Count - 1; // Fallback

			_startIndex = firstIndex;

			for (int i = 0; i < this.AssociatedObject.Items.Count; i++)
			{
				var child = this.AssociatedObject.ItemContainerGenerator.ContainerFromIndex(i) as ContentControl;
				if (child == null)
					continue;

				var isIntersected = (firstIndex <= i) && (i <= lastIndex);

				StoreIntersected(isIntersected, child);
			}
		}

		private void CheckChild(IEnumerable<object> sourceItems)
		{
			if (sourceItems == null)
				return;

			var viewportRect = new Rect(0, -_margin, _viewer.RenderSize.Width, _viewer.RenderSize.Height + _margin);

			foreach (var sourceItem in sourceItems)
			{
				var child = this.AssociatedObject.ItemContainerGenerator.ContainerFromItem(sourceItem) as ContentControl;
				if (child == null)
					continue;

				var isIntersected = IsIntersected(viewportRect, child);

				StoreIntersected(isIntersected, child);
			}
		}

		/// <summary>
		/// Check if a specified child item is intersected with the viewport.
		/// </summary>
		/// <param name="viewportRect">Viewport Rect</param>
		/// <param name="child">Child item</param>
		/// <returns>True if intersected</returns>
		private bool IsIntersected(Rect viewportRect, Control child)
		{
			// If called after CollectionChanged event, child may have not performed Measure method yet
			// and so RenderSize may be 0 and 0. A waiting time after the event will mostly prevent it
			// but still it may happen. In such case, call UpdateLayout method to force child perform
			// Measure method.
			if (child.RenderSize.Width == 0) // Checking width only is enough.
				child.UpdateLayout();

			var childRect = child.TransformToAncestor(this.AssociatedObject)
				.TransformBounds(new Rect(new Point(0, 0), child.RenderSize));

			return viewportRect.IntersectsWith(childRect);
		}

		/// <summary>
		/// Store information on intersection to a specified child item's Tag property.
		/// </summary>
		/// <param name="isIntersected">Information on intersection</param>
		/// <param name="child">Child item</param>
		private void StoreIntersected(bool isIntersected, Control child)
		{
			var isTrue = (child.Tag is bool) && (bool)child.Tag;

			if ((isIntersected && !isTrue) || (!isIntersected && isTrue))
				child.Tag = isIntersected;
		}
	}
}
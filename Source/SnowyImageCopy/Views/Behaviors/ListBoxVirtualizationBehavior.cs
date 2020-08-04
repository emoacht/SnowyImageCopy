using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Xaml.Behaviors;
using static System.Math;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Checks which child items are inside the viewport of ScrollViewer in ListBox.
	/// </summary>
	[TypeConstraint(typeof(ListBox))]
	public class ListBoxVirtualizationBehavior : Behavior<ListBox>
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

			_viewer = VisualTreeHelperAddition.GetDescendants<ScrollViewer>(this.AssociatedObject).FirstOrDefault();
			if (_viewer is null)
				return;

			CheckItems(); // Initial check

			_subscription.Add(Observable.FromEventPattern<ScrollChangedEventHandler, ScrollChangedEventArgs>(
				handler => handler.Invoke,
				handler => _viewer.ScrollChanged += handler,
				handler => _viewer.ScrollChanged -= handler)
				.Subscribe(x => CheckItems(x.EventArgs)));

			if (!(this.AssociatedObject.Items is INotifyCollectionChanged source)) // ItemsSource
				return;

			_subscription.Add(Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
				handler => handler.Invoke,
				handler => source.CollectionChanged += handler,
				handler => source.CollectionChanged -= handler)
				.Do(x => ReflectItemsFormer(x.EventArgs))
				.Delay(TimeSpan.FromMilliseconds(10)) // Waiting time for child items to perform Measure method
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(x => ReflectItemsLatter(x.EventArgs)));
		}

		/// <summary>
		/// Vertical margin (top and bottom) added in the viewport
		/// </summary>
		protected double ViewportMargin { get; set; } = 100D;

		private int _firstOldIndex;
		private int _lastOldIndex;

		private int _firstOldDevience;
		private int _lastOldDevience;

		private readonly object _locker = new object();

		private void CheckItems(ScrollChangedEventArgs args = null)
		{
			if (this.AssociatedObject.Items.Count == 0)
				return;

			lock (_locker)
			{
				var viewportRect = new Rect(0, -ViewportMargin, _viewer.ViewportWidth, _viewer.ViewportHeight + ViewportMargin * 2);

				int? firstNewIndex = null;
				int? lastNewIndex = null;

				int upwardIndex = Max(_firstOldIndex, 0);
				int downwardIndex = Max(upwardIndex + 1, _lastOldIndex);

				var oldIndices = IndicesCreate(_firstOldIndex - _firstOldDevience, _lastOldIndex + _lastOldDevience, this.AssociatedObject.Items.Count - 1);

				var firstNewIndexMayExist = true;
				var lastNewIndexMayExist = true;
				var firstOldIndexMayExist = true;
				var lastOldIndexMayExist = true;

				var isLeadoff = true;

				while (firstNewIndexMayExist || lastNewIndexMayExist)
				{
					// Check upward.
					if (firstNewIndexMayExist)
					{
						if (0 <= upwardIndex)
						{
							if (this.AssociatedObject.ItemContainerGenerator.ContainerFromIndex(upwardIndex) is ContentControl item)
							{
								var isIntersected = IsIntersected(viewportRect, item);
								if (isIntersected)
								{
									firstNewIndex = upwardIndex;

									if (!isLeadoff && !lastNewIndex.HasValue) // If not found yet									
										lastNewIndex = upwardIndex;
								}
								else
								{
									if (firstNewIndex.HasValue) // If found already
										firstNewIndexMayExist = false;
								}

								SetViewable(isIntersected, item, upwardIndex);
							}

							if (firstOldIndexMayExist)
								firstOldIndexMayExist = IndicesRemove(oldIndices, upwardIndex);

							upwardIndex--;
						}
						else
						{
							firstNewIndexMayExist = false;
						}
					}

					// Check downward.
					if (lastNewIndexMayExist)
					{
						if (downwardIndex < this.AssociatedObject.Items.Count)
						{
							if (this.AssociatedObject.ItemContainerGenerator.ContainerFromIndex(downwardIndex) is ContentControl item)
							{
								var isIntersected = IsIntersected(viewportRect, item);
								if (isIntersected)
								{
									if (!isLeadoff && !firstNewIndex.HasValue) // If not found yet									
										firstNewIndex = downwardIndex;

									lastNewIndex = downwardIndex;
								}
								else
								{
									if (lastNewIndex.HasValue) // If found already
										lastNewIndexMayExist = false;
								}

								SetViewable(isIntersected, item, downwardIndex);
							}

							if (lastOldIndexMayExist)
								lastOldIndexMayExist = IndicesRemove(oldIndices, downwardIndex);

							downwardIndex++;
						}
						else
						{
							lastNewIndexMayExist = false;
						}
					}

					if (isLeadoff)
					{
						isLeadoff = false;

						if (firstNewIndex.HasValue && lastNewIndex.HasValue)
						{
							// Continue both upward and downward. Ignore the indices between.
							for (int i = firstNewIndex.Value + 1; i <= lastNewIndex.Value - 1; i++)
								IndicesRemove(oldIndices, i);
						}
						else
						{
							var restartDownward = true;

							if (firstNewIndex.HasValue)
							{
								// Continue upward. Restart downward.
								lastNewIndex = firstNewIndex.Value; // upwardIndex + 1
							}
							else if (lastNewIndex.HasValue)
							{
								// Restart upward. Continue downward.
								firstNewIndex = lastNewIndex.Value; // downwardIndex - 1
								restartDownward = false;
							}
							else
							{
								// Restart downward or upward depending on scroll direction.
								restartDownward = (args is null) || (args.VerticalChange + args.HorizontalChange < 0);
							}

							if (restartDownward)
							{
								downwardIndex = upwardIndex + 2;
								lastNewIndexMayExist = true;
								lastOldIndexMayExist = true;
							}
							else
							{
								upwardIndex = downwardIndex - 2;
								firstNewIndexMayExist = true;
								firstOldIndexMayExist = true;
							}
						}
					}
				}

				foreach (int oldIndex in oldIndices)
				{
					// ItemContainerGenerator.ContainerFromIndex method will not throw an exception
					// even if the index is out of range.
					if (this.AssociatedObject.ItemContainerGenerator.ContainerFromIndex(oldIndex) is ContentControl item)
					{
						SetViewable(false, item, oldIndex);
					}
				}

				_firstOldIndex = firstNewIndex ?? 0; // Fallback
				_lastOldIndex = lastNewIndex ?? Max(_firstOldIndex, this.AssociatedObject.Items.Count - 1); // Fallback

				_firstOldDevience = 0;
				_lastOldDevience = 0;
			}

			static List<int> IndicesCreate(int firstIndex, int lastIndex, int maxIndex)
			{
				firstIndex = Max(firstIndex, 0);
				lastIndex = Min(lastIndex, maxIndex);

				return Enumerable.Range(firstIndex, lastIndex - firstIndex + 1).ToList();
			}

			static bool IndicesRemove(List<int> indices, int item)
			{
				var index = indices.BinarySearch(item);
				if (index < 0)
					return false;

				indices.RemoveAt(index);
				return true;
			}
		}

		private void ReflectItemsFormer(NotifyCollectionChangedEventArgs args)
		{
			lock (_locker)
			{
				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
					case NotifyCollectionChangedAction.Move:
					case NotifyCollectionChangedAction.Remove:
						if (args.OldItems?.Count > 0)
						{
							_firstOldDevience += args.OldItems.Count;
						}
						if (args.NewItems?.Count > 0)
						{
							_lastOldDevience += args.NewItems.Count;
						}
						break;
					case NotifyCollectionChangedAction.Reset:
						_firstOldIndex = 0;
						_lastOldIndex = 0;
						break;
				}
			}
		}

		private void ReflectItemsLatter(NotifyCollectionChangedEventArgs args)
		{
			lock (_locker)
			{
				switch (args.Action)
				{
					case NotifyCollectionChangedAction.Add:
					case NotifyCollectionChangedAction.Move:
					case NotifyCollectionChangedAction.Replace:
						if (args.NewItems?.Count > 0)
						{
							var viewportRect = new Rect(0, -ViewportMargin, _viewer.ViewportWidth, _viewer.ViewportHeight + ViewportMargin * 2);

							//_viewer.UpdateLayout();

							foreach (var newItem in args.NewItems)
							{
								if (this.AssociatedObject.ItemContainerGenerator.ContainerFromItem(newItem) is ContentControl item)
								{
									var isIntersected = IsIntersected(viewportRect, item);
									SetViewable(isIntersected, item);
								}
							}
						}
						break;
					case NotifyCollectionChangedAction.Reset:
						// This indicates that ListCollectionView.Refresh method is called.
						CheckItems();
						break;
				}
			}
		}

		/// <summary>
		/// Determines whether a specified child item is intersected with the viewport.
		/// </summary>
		/// <param name="viewportRect">Viewport Rect</param>
		/// <param name="item">Child item</param>
		/// <returns>True if intersected</returns>
		private bool IsIntersected(Rect viewportRect, Control item)
		{
			// If called after CollectionChanged event, child item may not have performed Measure
			// method yet and so RenderSize may be 0 and 0. A waiting time after the event will
			// mostly prevent it but still it may happen. In such case, call UpdateLayout method
			// to force child item perform Measure method. System.Windows.Size.IsEmpty property is
			// not useful for this purpose because it checks if its width is below 0.
			if (item.RenderSize.Width == 0)
				item.UpdateLayout();

			var itemRect = item.TransformToAncestor(this.AssociatedObject)
				.TransformBounds(new Rect(default, item.RenderSize));

			return viewportRect.IntersectsWith(itemRect);
		}

		/// <summary>
		/// Sets information on whether a specified child item is viewable.
		/// </summary>
		/// <param name="isViewable">Information on whether the child item is viewable</param>
		/// <param name="item">Child item</param>
		/// <param name="index">Child item's (Source item's) index (optional)</param>
		/// <remarks>
		/// If child item's index is -1, the index is not given. If the index is necessary,
		/// call ItemContainerGenerator.IndexFromContainer method with the given child item.
		/// </remarks>
		protected virtual void SetViewable(bool isViewable, Control item, int index = -1)
		{
			StoreViewable(isViewable, item);
		}

		/// <summary>
		/// Stores information on whether a specified child item is viewable in its Tag property.
		/// </summary>
		/// <param name="isViewable">Information on whether the child item is viewable</param>
		/// <param name="item">Child item</param>
		private static void StoreViewable(bool isViewable, Control item)
		{
			var isTrue = (item.Tag is bool stored) && stored;
			if (isTrue ^ isViewable)
				item.Tag = isViewable;
		}
	}
}
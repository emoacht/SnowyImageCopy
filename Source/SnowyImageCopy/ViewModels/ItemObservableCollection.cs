using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SnowyImageCopy.ViewModels
{
	public class ItemObservableCollection<T> : ObservableCollection<T>
		where T : class, INotifyPropertyChanged, IComparable<T>
	{
		private readonly object _locker = new();

		public ItemObservableCollection()
		{
			// If BindingOperations.EnableCollectionSynchronization method is used, locking is required
			// when collection members are changed (not when a property value of a member is changed)
			// according to http://10rem.net/blog/2012/01/20/wpf-45-cross-thread-collection-synchronization-redux
			BindingOperations.EnableCollectionSynchronization(this, _locker);
		}

		#region Method

		/// <summary>
		/// Inserts new item to correct position in order to minimize the necessity of sorting.
		/// </summary>
		/// <param name="item">New item to collection</param>
		public void Insert(T item)
		{
			lock (_locker)
			{
				int index = 0;

				for (int i = Count - 1; i >= 0; i--)
				{
					if (Items[i].CompareTo(item) < 0)
					{
						index = i + 1;
						break;
					}
				}

				base.InsertItem(index, item);
			}
		}

		protected override void InsertItem(int index, T item)
		{
			lock (_locker)
			{
				base.InsertItem(index, item);
			}
		}

		protected override void RemoveItem(int index)
		{
			lock (_locker)
			{
				base.RemoveItem(index);
			}
		}

		protected override void MoveItem(int oldIndex, int newIndex)
		{
			lock (_locker)
			{
				base.MoveItem(oldIndex, newIndex);
			}
		}

		protected override void SetItem(int index, T item)
		{
			lock (_locker)
			{
				base.SetItem(index, item);
			}
		}

		protected override void ClearItems()
		{
			// Remove event handlers for PropertyChanged event of items.
			foreach (T item in Items)
				item.PropertyChanged -= OnItemPropertyChanged;

			lock (_locker)
			{
				base.ClearItems();
			}
		}

		#endregion

		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			// Add/Remove event handlers for PropertyChanged event of items.
			if (e.OldItems?.Count > 0)
				foreach (T item in e.OldItems)
					item.PropertyChanged -= OnItemPropertyChanged;

			if (e.NewItems?.Count > 0)
				foreach (T item in e.NewItems)
					item.PropertyChanged += OnItemPropertyChanged;

			lock (_locker)
			{
				base.OnCollectionChanged(e);
			}
		}

		#region PropertyChanged event of item

		private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ItemPropertyChangedSender = sender as T;
			ItemPropertyChangedEventArgs = e;

			base.OnPropertyChanged(new PropertyChangedEventArgs(nameof(ItemPropertyChangedSender)));
		}

		/// <summary>
		/// Item which fired a PropertyChanged event
		/// </summary>
		public T ItemPropertyChangedSender { get; private set; }

		/// <summary>
		/// PropertyChangedEventArgs of the PropertyChanged event
		/// </summary>
		public PropertyChangedEventArgs ItemPropertyChangedEventArgs { get; private set; }

		#endregion
	}
}
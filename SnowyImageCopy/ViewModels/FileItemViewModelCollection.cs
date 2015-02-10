using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;

namespace SnowyImageCopy.ViewModels
{
	public class FileItemViewModelCollection : ObservableCollection<FileItemViewModel>
	{
		private readonly object _locker = new object();

		/// <summary>
		/// Insert new item to correct position in order to minimize the necessity of sorting.
		/// </summary>
		/// <param name="item">New item to collection</param>
		public void Insert(FileItemViewModel item)
		{
			lock (_locker)
			{
				int index = 0;

				for (int i = this.Count - 1; i >= 0; i--)
				{
					if (this[i].CompareTo(item) < 0)
					{
						index = i + 1;
						break;
					}
				}

				base.Insert(index, item);
			}
		}


		#region PropertyChanged event of item

		/// <summary>
		/// Add or remove an event handler for PropertyChanged event of each item.
		/// </summary>
		protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
		{
			if (e.OldItems != null)
				foreach (FileItemViewModel item in e.OldItems)
					item.PropertyChanged -= OnItemPropertyChanged;

			if (e.NewItems != null) // e.NewItems seems not to become null.
				foreach (FileItemViewModel item in e.NewItems)
					item.PropertyChanged += OnItemPropertyChanged;

			base.OnCollectionChanged(e);
		}

		/// <summary>
		/// Remove all event handlers for PropertyChanged event of all items.
		/// </summary>
		protected override void ClearItems()
		{
			if (this.Items != null)
				this.Items.ToList().ForEach(x => x.PropertyChanged -= OnItemPropertyChanged);

			base.ClearItems();
		}


		private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			ItemPropertyChangedSender = sender as FileItemViewModel;
			ItemPropertyChangedEventArgs = e;

			base.OnPropertyChanged(new PropertyChangedEventArgs(NameItemPropertyChangedSender));
		}

		private string NameItemPropertyChangedSender
		{
			get
			{
				return _nameItemPropertyChangedSender ?? (_nameItemPropertyChangedSender =
					PropertySupport.GetPropertyName(() => ItemPropertyChangedSender));
			}
		}
		private string _nameItemPropertyChangedSender;

		/// <summary>
		/// Item which fired a PropertyChanged event
		/// </summary>
		public FileItemViewModel ItemPropertyChangedSender { get; private set; }

		/// <summary>
		/// PropertyChangedEventArgs of the PropertyChanged event
		/// </summary>
		public PropertyChangedEventArgs ItemPropertyChangedEventArgs { get; private set; }

		#endregion
	}
}
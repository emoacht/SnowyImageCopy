using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Common
{
	public abstract class NotificationObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected bool SetProperty<T>(ref T storage, in T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(storage, value))
				return false;

			storage = value;
			OnPropertyChanged(propertyName);
			return true;
		}

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		protected static bool SetProperty<T>(ref T storage, in T value, EventHandler<PropertyChangedEventArgs> handler, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(storage, value))
				return false;

			storage = value;
			OnPropertyChanged(handler, propertyName);
			return true;
		}

		protected static void OnPropertyChanged(EventHandler<PropertyChangedEventArgs> handler, [CallerMemberName] string propertyName = null) =>
			handler?.Invoke(null, new PropertyChangedEventArgs(propertyName));
	}
}
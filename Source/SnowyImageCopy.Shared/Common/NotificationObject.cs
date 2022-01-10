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

		protected bool SetPropertyValue<T>(ref T storage, in T value, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(storage, value))
				return false;

			storage = value;
			RaisePropertyChanged(propertyName);
			return true;
		}

		protected void RaisePropertyChanged([CallerMemberName] string propertyName = null) =>
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

		protected static bool SetPropertyValue<T>(ref T storage, in T value, EventHandler<PropertyChangedEventArgs> handler, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(storage, value))
				return false;

			storage = value;
			RaisePropertyChanged(handler, propertyName);
			return true;
		}

		protected static void RaisePropertyChanged(EventHandler<PropertyChangedEventArgs> handler, [CallerMemberName] string propertyName = null) =>
			handler?.Invoke(null, new PropertyChangedEventArgs(propertyName));
	}
}
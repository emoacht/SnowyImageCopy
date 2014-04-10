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
		protected NotificationObject()
		{ }
		
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void RaisePropertyChanged([CallerMemberName]string propertyName = null)
		{
			var handler = this.PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
	}
}

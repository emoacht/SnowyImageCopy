using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SnowyImageCopy.Common
{
	public class PropertyChangedEventListener : IWeakEventListener
	{
		private readonly Action<object, PropertyChangedEventArgs> _propertyChangedAction;

		public PropertyChangedEventListener(Action<object, PropertyChangedEventArgs> propertyChangedAction)
		{
			this._propertyChangedAction = propertyChangedAction ?? throw new ArgumentNullException(nameof(propertyChangedAction));
		}

		public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType != typeof(PropertyChangedEventManager))
				return false;

			if (e is not PropertyChangedEventArgs buffer)
				return false;

			_propertyChangedAction(sender, buffer);
			return true;
		}
	}
}
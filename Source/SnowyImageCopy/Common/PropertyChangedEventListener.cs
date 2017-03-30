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
			this._propertyChangedAction = propertyChangedAction;
		}

		public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType != typeof(PropertyChangedEventManager))
				return false;

			var pce = e as PropertyChangedEventArgs;
			if (pce == null)
				return false;

			this._propertyChangedAction(sender, pce);
			return true;
		}
	}
}
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
		public PropertyChangedEventListener(Action<object, PropertyChangedEventArgs> propertyChangedAction)
		{
			this.propertyChangedAction = propertyChangedAction;
		}

		private readonly Action<object, PropertyChangedEventArgs> propertyChangedAction;

		public bool ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
		{
			if (managerType != typeof(PropertyChangedEventManager))
				return false;
			
			var pce = e as PropertyChangedEventArgs;
			if (pce == null)
				return false;

			this.propertyChangedAction(sender, pce);
			return true;
		}
	}
}
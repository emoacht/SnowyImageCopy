using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;

namespace SnowyImageCopy.ViewModels
{
	public abstract class ViewModel : NotificationObject, IDisposable
	{
		public ViewModel()
		{ }

		#region Dispose

		protected CompositeDisposable Subscription { get { return _subscription.Value; } }
		private Lazy<CompositeDisposable> _subscription = new Lazy<CompositeDisposable>(() => new CompositeDisposable());

		private bool _disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (_subscription.IsValueCreated)
					_subscription.Value.Dispose();
			}

			_disposed = true;
		}

		#endregion
	}
}
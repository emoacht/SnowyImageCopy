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

		public CompositeDisposable Disposer
		{
			get { return _disposer ?? (_disposer = new CompositeDisposable()); }
		}
		private CompositeDisposable _disposer;

		bool _disposed = false;

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
				if (_disposer != null)
					_disposer.Dispose();
			}

			_disposed = true;
		}

		#endregion
	}
}
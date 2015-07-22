using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Disposables;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;

namespace SnowyImageCopy.ViewModels
{
	public abstract class ViewModel : NotificationObject, IDisposable
	{
		public ViewModel()
		{ }


		#region Property name

		private Lazy<Dictionary<string, string>> _propertyNameMap = new Lazy<Dictionary<string, string>>(() => new Dictionary<string, string>());

		protected string GetPropertyName([CallerMemberName] string callerPropertyName = null)
		{
			if (String.IsNullOrEmpty(callerPropertyName))
				return null;

			return _propertyNameMap.Value.ContainsKey(callerPropertyName)
				? _propertyNameMap.Value[callerPropertyName]
				: null;
		}

		protected string GetPropertyName<T>(Expression<Func<T>> propertyExpression, [CallerMemberName] string callerPropertyName = null)
		{
			if (String.IsNullOrEmpty(callerPropertyName))
				return null;

			var calledPropertyName = PropertySupport.GetPropertyName(propertyExpression);			
			_propertyNameMap.Value.Add(callerPropertyName, calledPropertyName);

			return calledPropertyName;
		}

		#endregion


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
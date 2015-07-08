using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// <see cref="System.Threading.CancellationTokenSource"/> which can cancel without exception
	/// </summary>
	public sealed class CancellationTokenSourcePlus : CancellationTokenSource
	{
		public CancellationTokenSourcePlus() : base() { }
		public CancellationTokenSourcePlus(int millisecondsDelay) : base(millisecondsDelay) { }
		public CancellationTokenSourcePlus(TimeSpan delay) : base(delay) { }

		private readonly object _locker = new object();


		#region Dispose

		public bool IsDisposed { get { return _isDisposed; } }
		private bool _isDisposed = false;

		protected override void Dispose(bool disposing)
		{
			lock (_locker)
			{
				if (_isDisposed)
					return;

				base.Dispose(disposing);
				_isDisposed = true;
			}
		}

		#endregion


		#region Cancel

		/// <summary>
		/// Communicates a Cancel request on this CancellationTokenSource if it has not been disposed.
		/// </summary>
		/// <returns>True if communicated.</returns>
		public bool TryCancel()
		{
			lock (_locker)
			{
				if (_isDisposed)
					return false;

				base.Cancel();
				return true;
			}
		}

		/// <summary>
		/// Communicates a Cancel request on this CancellationTokenSource if it has not been disposed.
		/// </summary>
		/// <param name="throwOnFirstException">Whether exceptions should immediately propagate</param>
		/// <returns>True if communicated.</returns>
		public bool TryCancel(bool throwOnFirstException)
		{
			lock (_locker)
			{
				if (_isDisposed)
					return false;

				base.Cancel(throwOnFirstException);
				return true;
			}
		}

		/// <summary>
		/// Schedules a Cancel operation on this CancellationTokenSource if it has not been disposed.
		/// </summary>
		/// <param name="millisecondsDelay">Waiting duration before canceling this CancellationTokenSource</param>
		/// <returns>True if scheduled.</returns>
		public bool TryCancelAfter(int millisecondsDelay)
		{
			lock (_locker)
			{
				if (_isDisposed)
					return false;

				base.CancelAfter(millisecondsDelay);
				return true;
			}
		}

		/// <summary>
		/// Schedules a Cancel operation on this CancellationTokenSource if it has not been disposed.
		/// </summary>
		/// <param name="delay">Waiting duration before canceling this CancellationTokenSource</param>
		/// <returns>True if scheduled.</returns>
		public bool TryCancelAfter(TimeSpan delay)
		{
			lock (_locker)
			{
				if (_isDisposed)
					return false;

				base.CancelAfter(delay);
				return true;
			}
		}

		#endregion
	}
}
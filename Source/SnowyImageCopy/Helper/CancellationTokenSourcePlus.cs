using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Derived class of <see cref="System.Threading.CancellationTokenSource"/> which can cancel without exception
	/// </summary>
	public sealed class CancellationTokenSourcePlus : CancellationTokenSource
	{
		public CancellationTokenSourcePlus() : base() { }
		public CancellationTokenSourcePlus(int millisecondsDelay) : base(millisecondsDelay) { }
		public CancellationTokenSourcePlus(TimeSpan delay) : base(delay) { }

		private readonly object _locker = new();

		#region Dispose

		public bool IsDisposed => _isDisposed;
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
		/// Communicates a cancel request if this CancellationTokenSource has not been disposed.
		/// </summary>
		/// <returns>True if communicated.</returns>
		public bool TryCancel()
		{
			lock (_locker)
			{
				if (_isDisposed)
					return false;

				Cancel();
				return true;
			}
		}

		/// <summary>
		/// Communicates a cancel request if this CancellationTokenSource has not been disposed.
		/// </summary>
		/// <param name="throwOnFirstException">Whether exceptions should immediately propagate</param>
		/// <returns>True if communicated.</returns>
		public bool TryCancel(bool throwOnFirstException)
		{
			lock (_locker)
			{
				if (_isDisposed)
					return false;

				Cancel(throwOnFirstException);
				return true;
			}
		}

		/// <summary>
		/// Schedules a cancel operation if this CancellationTokenSource has not been disposed.
		/// </summary>
		/// <param name="millisecondsDelay">Waiting duration before canceling this CancellationTokenSource</param>
		/// <returns>True if scheduled.</returns>
		public bool TryCancelAfter(int millisecondsDelay)
		{
			lock (_locker)
			{
				if (_isDisposed)
					return false;

				CancelAfter(millisecondsDelay);
				return true;
			}
		}

		/// <summary>
		/// Schedules a cancel operation if this CancellationTokenSource has not been disposed.
		/// </summary>
		/// <param name="delay">Waiting duration before canceling this CancellationTokenSource</param>
		/// <returns>True if scheduled.</returns>
		public bool TryCancelAfter(TimeSpan delay)
		{
			lock (_locker)
			{
				if (_isDisposed)
					return false;

				CancelAfter(delay);
				return true;
			}
		}

		#endregion
	}
}
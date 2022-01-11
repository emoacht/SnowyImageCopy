using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Decorator class of <see cref="System.Threading.CancellationTokenSource"/> which can release reference
	/// </summary>
	/// <remarks>
	/// Be sure to call Release method at the end of each cycle.
	/// </remarks>
	public class CancellationTokenContainer
	{
		private CancellationTokenSource _cancellationTokenSource;

		private readonly object _locker = new();

		public CancellationToken Token
		{
			get
			{
				lock (_locker)
				{
					_cancellationTokenSource ??= new CancellationTokenSource();
					return _cancellationTokenSource.Token;
				}
			}
		}

		public bool IsCancellationRequested => (_cancellationTokenSource is { IsCancellationRequested: true });
		public bool IsReleased => (_cancellationTokenSource is null);

		public bool TryCancel(bool release = false)
		{
			lock (_locker)
			{
				if (IsReleased || IsCancellationRequested)
					return false;

				_cancellationTokenSource.Cancel();

				if (release)
				{
					_cancellationTokenSource.Dispose();
					_cancellationTokenSource = null;
				}
				return true;
			}
		}

		public void Release()
		{
			lock (_locker)
			{
				if (IsReleased)
					return;

				_cancellationTokenSource.Dispose();
				_cancellationTokenSource = null;
			}
		}
	}
}
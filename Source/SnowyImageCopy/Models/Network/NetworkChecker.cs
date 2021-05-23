using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SnowyImageCopy.Models.Card;

namespace SnowyImageCopy.Models.Network
{
	/// <summary>
	/// Checks network connection.
	/// </summary>
	internal static class NetworkChecker
	{
		/// <summary>
		/// Determines whether PC is connected to a network.
		/// </summary>
		internal static bool IsNetworkConnected() => NetworkInterface.GetIsNetworkAvailable();

		/// <summary>
		/// Determines whether PC is connected to a network and if applicable, a specified wireless LAN.
		/// </summary>
		/// <param name="card">State of FlashAir card</param>
		/// <returns>True if connected</returns>
		internal static bool IsNetworkConnected(ICardState card)
		{
			if ((card is null) || !card.IsWirelessConnected)
				return IsNetworkConnected();

			return IsWirelessNetworkConnected(card.Ssid);
		}

		/// <summary>
		/// Determines whether PC is connected to a specified wireless LAN.
		/// </summary>
		/// <param name="ssid">SSID of wireless LAN</param>
		/// <returns>True if connected</returns>
		internal static bool IsWirelessNetworkConnected(string ssid)
		{
			if (string.IsNullOrWhiteSpace(ssid))
				return false;

			//if (NetworkInterface.GetAllNetworkInterfaces()
			//	.Where(x => x.OperationalStatus == OperationalStatus.Up)
			//	.All(x => x.NetworkInterfaceType != NetworkInterfaceType.Wireless80211))
			//	return false;

			try
			{
				return Workspace.Wifi.EnumerateConnectedNetworkSsids()
					.Any(x => string.Equals(x, ssid, StringComparison.Ordinal));
			}
			catch (Win32Exception)
			{
				return false;
			}
		}
	}

	internal class NetworkMonitor : IDisposable
	{
		private readonly Action _disconnected;
		private readonly ICardState _card;

		public NetworkMonitor(Action disconnected, ICardState card, TimeSpan interval)
		{
			this._disconnected = disconnected ?? throw new ArgumentNullException(nameof(disconnected));
			this._card = card; // Null will be allowed.

			Subscribe(interval);
		}

		private Timer _timer;
		private readonly object _locker = new object();
		private IDisposable _isSubscription;

		private void Subscribe(TimeSpan interval)
		{
			if ((_card is null) || !_card.IsWirelessConnected)
			{
				// Timer mode
				lock (_locker)
				{
					_timer = new Timer(_ => CheckIsConnected(), null, interval, interval);
				}
			}
			else
			{
				// Event mode
				_isSubscription = Observable.FromEventPattern<ConnectionEventArgs>(
					h => Workspace.Wifi.Disconnected += h,
					h => Workspace.Wifi.Disconnected -= h)
					.Subscribe(e => CheckIsConnected(e.EventArgs.Ssid));
			}
		}

		private void Unsubscribe()
		{
			lock (_locker)
			{
				_timer?.Dispose();
				_timer = null;
			}

			_isSubscription?.Dispose();
		}

		public void Restart(TimeSpan interval)
		{
			lock (_locker)
			{
				_timer?.Change(interval, interval);
			}
		}

		private bool CheckIsConnected(string ssid = null)
		{
			bool IsConnected()
			{
				if (!string.IsNullOrEmpty(ssid) && string.Equals(ssid, _card.Ssid, StringComparison.Ordinal))
					return false;

				return NetworkChecker.IsNetworkConnected(_card);
			}

			if (IsConnected())
				return true;

			_disconnected.Invoke();
			Unsubscribe();
			return false;
		}

		#region IDisposable

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
				Unsubscribe();
			}

			_disposed = true;
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
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
			if (!NetworkInterface.GetIsNetworkAvailable())
				return false;

			if ((card is null) || string.IsNullOrWhiteSpace(card.Ssid) || !card.IsWirelessConnected)
				return true;

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

			if (NetworkInterface.GetAllNetworkInterfaces()
				.Where(x => x.OperationalStatus == OperationalStatus.Up)
				.All(x => x.NetworkInterfaceType != NetworkInterfaceType.Wireless80211))
				return false;

			try
			{
				var ssids = NativeWifi.EnumerateConnectedNetworkSsids();

				return ssids.Any(x => string.Equals(x, ssid, StringComparison.Ordinal));
			}
			catch (Win32Exception)
			{
				return false;
			}
		}
	}
}
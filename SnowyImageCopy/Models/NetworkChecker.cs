using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Check network connection.
	/// </summary>
	internal static class NetworkChecker
	{
		/// <summary>
		/// Check if PC is connected to a network.
		/// </summary>
		internal static bool IsNetworkConnected()
		{
			return NetworkInterface.GetIsNetworkAvailable();
		}

		/// <summary>
		/// Check if PC is connected to a network and if applicable, a specified wireless network.
		/// </summary>
		/// <param name="card">FlashAir card information</param>
		internal static bool IsNetworkConnected(CardInfo card)
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
				return false;

			if ((card == null) || String.IsNullOrWhiteSpace(card.Ssid) || !card.IsWirelessConnected)
				return true;

			return IsWirelessNetworkConnected(card.Ssid);
		}

		/// <summary>
		/// Check if PC is connected to a specified wireless network.
		/// </summary>
		/// <param name="ssid">SSID of wireless network</param>
		internal static bool IsWirelessNetworkConnected(string ssid)
		{
			if (String.IsNullOrWhiteSpace(ssid))
				return false;

			if (NetworkInterface.GetAllNetworkInterfaces()
				.Where(x => x.OperationalStatus == OperationalStatus.Up)
				.All(x => x.NetworkInterfaceType != NetworkInterfaceType.Wireless80211))
				return false;

			var ssids = NativeWifi.GetConnectedNetworkSsid();

			return ssids.Any(x => x.Equals(ssid, StringComparison.Ordinal));
		}
	}
}
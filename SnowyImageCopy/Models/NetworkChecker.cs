using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Checks network connection.
	/// </summary>
	internal static class NetworkChecker
	{
		/// <summary>
		/// Checks if PC is connected to a network.
		/// </summary>
		internal static bool IsNetworkConnected()
		{
			return NetworkInterface.GetIsNetworkAvailable();
		}

		/// <summary>
		/// Checks if PC is connected to a network and if applicable, a specified wireless LAN.
		/// </summary>
		/// <param name="card">FlashAir card information</param>
		/// <returns>True if connected</returns>
		internal static bool IsNetworkConnected(CardInfo card)
		{
			if (!NetworkInterface.GetIsNetworkAvailable())
				return false;

			if ((card == null) || String.IsNullOrWhiteSpace(card.Ssid) || !card.IsWirelessConnected)
				return true;

			return IsWirelessNetworkConnected(card.Ssid);
		}

		/// <summary>
		/// Checks if PC is connected to a specified wireless LAN.
		/// </summary>
		/// <param name="ssid">SSID of wireless LAN</param>
		/// <returns>True if connected</returns>
		internal static bool IsWirelessNetworkConnected(string ssid)
		{
			if (String.IsNullOrWhiteSpace(ssid))
				return false;

			if (NetworkInterface.GetAllNetworkInterfaces()
				.Where(x => x.OperationalStatus == OperationalStatus.Up)
				.All(x => x.NetworkInterfaceType != NetworkInterfaceType.Wireless80211))
				return false;

			try
			{
				var ssids = NativeWifi.EnumerateConnectedNetworkSsids();

				return ssids.Any(x => x.Equals(ssid, StringComparison.Ordinal));
			}
			catch (Win32Exception)
			{
				return false;
			}
		}
	}
}
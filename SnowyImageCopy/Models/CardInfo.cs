using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// FlashAir card information
	/// </summary>
	internal class CardInfo
	{
		/// <summary>
		/// SSID
		/// </summary>
		public string Ssid { get; set; }

		/// <summary>
		/// Whether PC is connected to FlashAir card by a wireless connection.  
		/// </summary>
		/// <remarks>True: Access Point mode, Internet Pass-Thru mode. False: Station mode.</remarks>
		public bool IsWirelessConnected { get; set; }
	}
}

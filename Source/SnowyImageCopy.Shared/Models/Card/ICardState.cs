using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Card
{
	internal interface ICardState
	{
		string Ssid { get; }

		bool IsWirelessConnected { get; }
	}
}
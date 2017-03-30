using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when PC lost connection to FlashAir card
	/// </summary>
	[Serializable]
	internal class RemoteConnectionLostException : HttpRequestException
	{
		public RemoteConnectionLostException() { }
		public RemoteConnectionLostException(string message) : base(message) { }
		public RemoteConnectionLostException(string message, Exception inner) : base(message, inner) { }
	}
}
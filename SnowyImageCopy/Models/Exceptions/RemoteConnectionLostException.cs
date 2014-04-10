using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when lost connection to FlashAir card.
	/// </summary>
	[Serializable]
	internal class RemoteConnectionLostException : Exception
	{
		public RemoteConnectionLostException() { }
		public RemoteConnectionLostException(string message) : base(message) { }
		public RemoteConnectionLostException(string message, Exception inner) : base(message, inner) { }
		protected RemoteConnectionLostException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

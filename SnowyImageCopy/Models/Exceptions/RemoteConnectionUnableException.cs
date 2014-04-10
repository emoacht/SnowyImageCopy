using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when unable to connect to FlashAir card.
	/// </summary>
	[Serializable]
	internal class RemoteConnectionUnableException : Exception
	{
		public HttpStatusCode Code { get; private set; }
		public WebExceptionStatus Status { get; private set; }


		#region Constructor

		public RemoteConnectionUnableException() { }
		public RemoteConnectionUnableException(string message) : base(message) { }
		public RemoteConnectionUnableException(string message, Exception inner) : base(message, inner) { }
		protected RemoteConnectionUnableException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public RemoteConnectionUnableException(HttpStatusCode code)
		{
			this.Code = code;
		}

		public RemoteConnectionUnableException(WebExceptionStatus status)
		{
			this.Status = status;
		}

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when PC is unable to connect to FlashAir card
	/// </summary>
	[Serializable]
	internal class RemoteConnectionUnableException : HttpRequestException
	{
		public HttpStatusCode Code { get; private set; }
		public WebExceptionStatus Status { get; private set; }


		#region Constructor

		public RemoteConnectionUnableException() { }
		public RemoteConnectionUnableException(string message) : base(message) { }
		public RemoteConnectionUnableException(string message, Exception inner) : base(message, inner) { }

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

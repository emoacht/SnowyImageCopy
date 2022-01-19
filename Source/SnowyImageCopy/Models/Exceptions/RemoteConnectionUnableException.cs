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
	/// Exception when PC is unable to connect to FlashAir card
	/// </summary>
	[Serializable]
	internal class RemoteConnectionUnableException : Exception
	{
		public HttpStatusCode Code { get; private set; }
		public WebExceptionStatus Status { get; private set; }

		public RemoteConnectionUnableException() { }
		public RemoteConnectionUnableException(string message) : base(message) { }
		public RemoteConnectionUnableException(HttpStatusCode code) => this.Code = code;
		public RemoteConnectionUnableException(WebExceptionStatus status) => this.Status = status;

		protected RemoteConnectionUnableException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.Code = (HttpStatusCode)info.GetValue(nameof(Code), typeof(HttpStatusCode));
			this.Status = (WebExceptionStatus)info.GetValue(nameof(Status), typeof(WebExceptionStatus));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(nameof(Code), this.Code, typeof(HttpStatusCode));
			info.AddValue(nameof(Status), this.Status, typeof(WebExceptionStatus));
		}
	}
}
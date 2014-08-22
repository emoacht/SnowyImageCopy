using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when remote file in FlashAir card is not found
	/// </summary>
	[Serializable]
	internal class RemoteFileNotFoundException : Exception
	{
		public new string Message
		{
			get { return !String.IsNullOrEmpty(base.Message) ? base.Message : this._message; }
			private set { _message = value; }
		}
		private string _message;

		public string FileName { get; private set; }


		#region Constructor

		public RemoteFileNotFoundException() { }
		public RemoteFileNotFoundException(string message) : base(message) { }
		public RemoteFileNotFoundException(string message, Exception inner) : base(message, inner) { }
		protected RemoteFileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public RemoteFileNotFoundException(string message, string fileName)
		{
			this.Message = message;
			this.FileName = fileName;
		}

		#endregion
	}
}

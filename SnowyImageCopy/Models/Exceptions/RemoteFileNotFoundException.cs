using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when a remote file in FlashAir card is not found
	/// </summary>
	[Serializable]
	internal class RemoteFileNotFoundException : Exception
	{
		public string FilePath { get; private set; }


		#region Constructor

		public RemoteFileNotFoundException() { }
		public RemoteFileNotFoundException(string message) : base(message) { }
		public RemoteFileNotFoundException(string message, Exception inner) : base(message, inner) { }
		protected RemoteFileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public RemoteFileNotFoundException(string message, string filePath)
			: base(message)
		{
			this.FilePath = filePath;
		}

		#endregion
	}
}

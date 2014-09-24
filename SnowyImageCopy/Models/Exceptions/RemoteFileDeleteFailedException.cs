using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when failed to delete a remote file in FlashAir card.
	/// </summary>
	[Serializable]
	internal class RemoteFileDeleteFailedException : Exception
	{
		public string FilePath { get; private set; }


		#region Constructor

		public RemoteFileDeleteFailedException() { }
		public RemoteFileDeleteFailedException(string message) : base(message) { }
		public RemoteFileDeleteFailedException(string message, Exception inner) : base(message, inner) { }
		protected RemoteFileDeleteFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public RemoteFileDeleteFailedException(string message, string filePath)
			: base(message)
		{
			this.FilePath = filePath;
		}

		#endregion
	}
}
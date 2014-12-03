using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when failed to delete a remote file in FlashAir card
	/// </summary>
	[Serializable]
	internal class RemoteFileDeletionFailedException : Exception
	{
		public string FilePath { get; private set; }


		#region Constructor

		public RemoteFileDeletionFailedException() { }
		public RemoteFileDeletionFailedException(string message) : base(message) { }
		public RemoteFileDeletionFailedException(string message, Exception inner) : base(message, inner) { }
		protected RemoteFileDeletionFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public RemoteFileDeletionFailedException(string message, string filePath)
			: base(message)
		{
			this.FilePath = filePath;
		}

		#endregion
	}
}
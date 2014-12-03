using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when failed to get a thumbnail of a remote file in FlashAir card
	/// </summary>
	[Serializable]
	public class RemoteFileThumbnailFailedException : Exception
	{
		public string FilePath { get; private set; }


		#region Constructor

		public RemoteFileThumbnailFailedException() { }
		public RemoteFileThumbnailFailedException(string message) : base(message) { }
		public RemoteFileThumbnailFailedException(string message, Exception inner) : base(message, inner) { }
		protected RemoteFileThumbnailFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public RemoteFileThumbnailFailedException(string message, string filePath)
			: base(message)
		{
			this.FilePath = filePath;
		}

		#endregion
	}
}
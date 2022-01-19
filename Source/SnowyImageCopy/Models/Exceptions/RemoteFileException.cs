using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	[Serializable]
	internal abstract class RemoteFileException : Exception
	{
		public string FilePath { get; private set; }

		public RemoteFileException() { }
		public RemoteFileException(string message, string filePath) : base(message) => this.FilePath = filePath;

		protected RemoteFileException(SerializationInfo info, StreamingContext context) : base(info, context)
		{
			this.FilePath = info.GetString(nameof(FilePath));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(nameof(FilePath), this.FilePath, typeof(string));
		}
	}

	/// <summary>
	/// Exception when a remote file in FlashAir card is found invalid
	/// </summary>
	[Serializable]
	internal class RemoteFileInvalidException : RemoteFileException
	{
		public RemoteFileInvalidException() { }
		public RemoteFileInvalidException(string message, string filePath) : base(message, filePath) { }
		protected RemoteFileInvalidException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Exception when failed to delete a remote file in FlashAir card
	/// </summary>
	[Serializable]
	internal class RemoteFileDeletionFailedException : RemoteFileException
	{
		public RemoteFileDeletionFailedException() { }
		public RemoteFileDeletionFailedException(string message, string filePath) : base(message, filePath) { }
		protected RemoteFileDeletionFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Exception when a remote file in FlashAir card is not found
	/// </summary>
	[Serializable]
	internal class RemoteFileNotFoundException : RemoteFileException
	{
		public RemoteFileNotFoundException() { }
		public RemoteFileNotFoundException(string message, string filePath) : base(message, filePath) { }
		protected RemoteFileNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}

	/// <summary>
	/// Exception when failed to get a thumbnail of a remote file in FlashAir card
	/// </summary>
	[Serializable]
	internal class RemoteFileThumbnailFailedException : RemoteFileException
	{
		public RemoteFileThumbnailFailedException() { }
		public RemoteFileThumbnailFailedException(string message, string filePath) : base(message, filePath) { }
		protected RemoteFileThumbnailFailedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
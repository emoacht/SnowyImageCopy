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
	internal class RemoteFileThumbnailFailedException : Exception
	{
		public string FilePath { get; private set; }

		#region Constructor

		public RemoteFileThumbnailFailedException() { }
		public RemoteFileThumbnailFailedException(string message) : base(message) { }
		public RemoteFileThumbnailFailedException(string message, Exception inner) : base(message, inner) { }

		public RemoteFileThumbnailFailedException(string message, string filePath)
			: base(message)
		{
			this.FilePath = filePath;
		}

		protected RemoteFileThumbnailFailedException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			this.FilePath = info.GetString(nameof(FilePath));
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			info.AddValue(nameof(FilePath), this.FilePath);
		}

		#endregion
	}
}
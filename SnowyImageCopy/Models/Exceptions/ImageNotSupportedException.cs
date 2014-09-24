using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when image format is not supported by PC
	/// </summary>
	[Serializable]
	public class ImageNotSupportedException : Exception
	{
		public ImageNotSupportedException() { }
		public ImageNotSupportedException(string message) : base(message) { }
		public ImageNotSupportedException(string message, Exception inner) : base(message, inner) { }
		protected ImageNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
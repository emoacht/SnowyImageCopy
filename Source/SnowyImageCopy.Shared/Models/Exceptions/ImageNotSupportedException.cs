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
	internal class ImageNotSupportedException : Exception
	{
		public ImageNotSupportedException() { }
		public ImageNotSupportedException(string message) : base(message) { }
		protected ImageNotSupportedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
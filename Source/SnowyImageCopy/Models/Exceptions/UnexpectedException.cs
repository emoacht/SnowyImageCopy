using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when an exception which has not been expected so far happened
	/// </summary>
	/// <remarks>This exception is for debugging purpose.</remarks>
	[Serializable]
	internal class UnexpectedException : Exception
	{
		public UnexpectedException() { }
		public UnexpectedException(string message) : base(message) { }
		public UnexpectedException(string message, Exception inner) : base(message, inner) { }
		protected UnexpectedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
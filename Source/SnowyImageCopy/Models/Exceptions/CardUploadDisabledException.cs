using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when upload.cgi of FlashAir card is disabled
	/// </summary>
	[Serializable]
	internal class CardUploadDisabledException : Exception
	{
		public CardUploadDisabledException() { }
		public CardUploadDisabledException(string message) : base(message) { }
		protected CardUploadDisabledException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
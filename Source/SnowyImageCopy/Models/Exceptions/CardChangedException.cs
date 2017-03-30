using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when FlashAir card is changed
	/// </summary>
	[Serializable]
	internal class CardChangedException : Exception
	{
		public CardChangedException() { }
		public CardChangedException(string message) : base(message) { }
		public CardChangedException(string message, Exception inner) : base(message, inner) { }
		protected CardChangedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
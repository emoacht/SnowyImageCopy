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
		protected CardChangedException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Exceptions
{
	/// <summary>
	/// Exception when an exception which has not been encountered happened
	/// </summary>
	/// <remarks>This exception is for debugging purpose.</remarks>
	[Serializable]
	public class NotEncounteredException : Exception
	{
		public NotEncounteredException() { }
		public NotEncounteredException(string message) : base(message) { }
		public NotEncounteredException(string message, Exception inner) : base(message, inner) { }
		protected NotEncounteredException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}

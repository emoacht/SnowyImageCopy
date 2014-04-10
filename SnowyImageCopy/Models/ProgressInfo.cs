using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Progress information for reporting
	/// </summary>
	/// <remarks>This class must be instantiated each time for reporting.</remarks>
	public class ProgressInfo
	{
		/// <summary>
		/// Current percentage
		/// </summary>
		public double CurrentPercentage { get; private set; }

		/// <summary>
		/// Current value
		/// </summary>
		public int CurrentValue { get; private set; }

		/// <summary>
		/// Total value
		/// </summary>
		public int TotalValue { get; private set; }

		/// <summary>
		/// Elapsed time since operation started
		/// </summary>
		public TimeSpan ElapsedTime { get; private set; }


		#region Constructor

		public ProgressInfo(double currentPercentage)
		{
			this.CurrentPercentage = currentPercentage;
		}

		public ProgressInfo(int currentValue, int totalValue, TimeSpan elapsedTime)
		{
			this.CurrentValue = currentValue;
			this.TotalValue = totalValue;
			this.ElapsedTime = elapsedTime;
		}

		#endregion
	}
}

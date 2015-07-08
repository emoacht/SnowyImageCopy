﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Progress information for <see cref="System.IProgress"/> reporting
	/// </summary>
	/// <remarks>
	/// <para>This class must be instantiated each time for reporting.</para>
	/// <para>This class should be immutable.</para>
	/// </remarks>
	internal class ProgressInfo
	{
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

		/// <summary>
		/// Whether this report is the first among a group of reports
		/// </summary>
		public bool IsFirst { get; private set; }

		/// <summary>
		/// Whether this report is for an error
		/// </summary>
		public bool IsError { get; private set; }


		#region Constructor

		public ProgressInfo(int currentValue, int totalValue, TimeSpan elapsedTime, bool isFirst)
		{
			this.CurrentValue = currentValue;
			this.TotalValue = totalValue;
			this.ElapsedTime = elapsedTime;
			this.IsFirst = isFirst;
		}

		public ProgressInfo(bool isError)
		{
			this.IsError = isError;
		}

		#endregion
	}
}
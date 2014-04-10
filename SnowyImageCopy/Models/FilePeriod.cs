using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// File period types to filter files in FlashAir card
	/// </summary>
	public enum FilePeriod
	{
		/// <summary>
		/// All files
		/// </summary>
		All,

		/// <summary>
		/// Files dated today.
		/// </summary>
		Today,

		/// <summary>
		/// Files dated on selected dates
		/// </summary>
		Select,
	}
}

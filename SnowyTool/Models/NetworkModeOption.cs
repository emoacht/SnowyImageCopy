using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyTool.Models
{
	public enum NetworkModeOption
	{
		None = 0,

		/// <summary>
		/// AP (Access Point) mode
		/// </summary>
		AccessPoint,

		/// <summary>
		/// STA (Station) mode
		/// </summary>
		Station,

		/// <summary>
		/// Internet pass-thru mode
		/// </summary>
		InternetPassThru,
	}
}

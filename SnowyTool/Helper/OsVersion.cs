using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyTool.Helper
{
	/// <summary>
	/// OS version information
	/// </summary>
	public static class OsVersion
	{
		private static readonly Version _ver = Environment.OSVersion.Version;

		/// <summary>
		/// Whether OS is Windows 8 or newer
		/// </summary>
		/// <remarks>Windows 8 = version 6.2</remarks>
		public static bool IsEightOrNewer
		{
			get { return ((6 == _ver.Major) && (2 <= _ver.Minor)) || (7 <= _ver.Major); }
		}
	}
}
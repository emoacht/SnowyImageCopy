using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	public static class OsVersion
	{
		private static readonly OperatingSystem os = Environment.OSVersion;

		/// <summary>
		/// Check if OS is Windows 8 or newer.
		/// </summary>
		/// <remarks>Windows 8 = version 6.2</remarks>
		public static bool IsEightOrNewer
		{
			get { return (6 <= os.Version.Major) && (2 <= os.Version.Minor); }
		}

		/// <summary>
		/// Check if OS is Windows 8.1 or newer.
		/// </summary>
		/// <remarks>Windows 8.1 = version 6.3</remarks>
		public static bool IsEightOneOrNewer
		{
			get { return (6 <= os.Version.Major) && (3 <= os.Version.Minor); }
		}
	}
}

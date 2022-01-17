using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// OS version information
	/// </summary>
	public static class OsVersion
	{
		/// <summary>
		/// Whether OS is Windows 8 or greater
		/// </summary>
		/// <remarks>Windows 8 = version 6.2</remarks>
		public static bool Is8OrGreater => _is8OrGreater ??= (new Version(6, 2) <= Environment.OSVersion.Version);
		private static bool? _is8OrGreater;

		/// <summary>
		/// Whether OS is Windows 10 or greater
		/// </summary>
		/// <remarks>Windows 10 = version 10.0.10240.0</remarks>
		public static bool Is10OrGreater => _is10OrGreater ??= (new Version(10, 0, 10240, 0) <= Environment.OSVersion.Version);
		private static bool? _is10OrGreater;
	}
}
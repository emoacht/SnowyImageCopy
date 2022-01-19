﻿using System;
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
		/// Whether OS is Windows 8 or newer
		/// </summary>
		/// <remarks>Windows 8 = version 6.2</remarks>
		public static bool IsEightOrNewer => _isEightOrNewer ??= (new Version(6, 2) <= Environment.OSVersion.Version);
		private static bool? _isEightOrNewer;
	}
}
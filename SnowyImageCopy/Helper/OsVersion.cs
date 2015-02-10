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
        private static readonly Version ver = Environment.OSVersion.Version;

        /// <summary>
        /// Whether OS is Windows 8 or newer
        /// </summary>
        /// <remarks>Windows 8 = version 6.2</remarks>
        public static bool IsEightOrNewer
        {
            get { return ((6 == ver.Major) && (2 <= ver.Minor)) || (7 <= ver.Major); }
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32.SafeHandles;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Manages File Times.
	/// </summary>
	/// <remarks>
	/// https://docs.microsoft.com/en-us/windows/desktop/sysinfo/file-times
	/// </remarks>
	public static class FileTime
	{
		#region Win32

		[DllImport("Kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool SetFileTime(
			SafeFileHandle hFile,
			[In] ref FILETIME creationTime,
			[In] ref FILETIME lastAccessTime,
			[In] ref FILETIME lastWriteTime);

		[StructLayout(LayoutKind.Sequential)]
		private struct FILETIME
		{
			public uint ftTimeLow;
			public uint ftTimeHigh;

			public FILETIME(long fileTime)
			{
				ftTimeLow = (uint)fileTime;
				ftTimeHigh = (uint)(fileTime >> 32);
			}

			public FILETIME(DateTime dateTime) : this(dateTime.ToFileTimeUtc())
			{ }
		}

		#endregion

		/// <summary>
		/// Sets creation time and last write time of a specified file.
		/// </summary>
		/// <param name="handle">File handle</param>
		/// <param name="creationTime">Creation time</param>
		/// <param name="lastWriteTime">Last write time</param>
		/// <returns>True if succeeded. False if failed.</returns>
		/// <remarks>DateTimeKind of time must be properly set. Otherwise, it will be regarded as UTC time.</remarks>
		public static bool SetFileTime(SafeFileHandle handle, DateTime creationTime, DateTime lastWriteTime)
		{
			return SetFileTime(handle, creationTime, default(DateTime), lastWriteTime);
		}

		/// <summary>
		/// Sets creation time, last access time and last write time of a specified file.
		/// </summary>
		/// <param name="handle">File handle</param>
		/// <param name="creationTime">Creation time</param>
		/// <param name="lastAccessTime">Last access time</param>
		/// <param name="lastWriteTime">Last write time</param>
		/// <returns>True if succeeded. False if failed.</returns>
		/// <remarks>DateTimeKind of time must be properly set. Otherwise, it will be regarded as UTC time.</remarks>
		public static bool SetFileTime(SafeFileHandle handle, DateTime creationTime, DateTime lastAccessTime, DateTime lastWriteTime)
		{
			var creationFileTime = GetFileTime(creationTime);
			var lastAccessFileTime = GetFileTime(lastAccessTime);
			var lastWriteFileTime = GetFileTime(lastWriteTime);

			return SetFileTime(handle, ref creationFileTime, ref lastAccessFileTime, ref lastWriteFileTime);

			FILETIME GetFileTime(DateTime dateTime) =>
				dateTime.Equals(default(DateTime)) ? default(FILETIME) : new FILETIME(dateTime);
		}
	}
}
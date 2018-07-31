using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Manages Recycle.
	/// </summary>
	public static class Recycle
	{
		#region Win32

		/// <summary>
		/// SHFileOperation function
		/// </summary>
		/// <param name="lpFileOp"></param>
		/// <returns>Error code (0 if successful)</returns>
		/// <remarks>MSDN says don't use GetLastError to examine the return values of this function.
		/// Several of return values are based on pre-Win32 error codes, which in some cases overlap
		/// common Win32 error codes without matching their meaning.</remarks>
		[DllImport("Shell32.dll", EntryPoint = "SHFileOperationW")]
		private static extern int SHFileOperation(ref SHFILEOPSTRUCT lpFileOp);

		[StructLayout(LayoutKind.Sequential)]
		private struct SHFILEOPSTRUCT
		{
			public IntPtr hwnd;
			public FO wFunc;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pFrom;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string pTo;

			public FOF fFlags;

			[MarshalAs(UnmanagedType.Bool)]
			public bool fAnyOperationsAborted;

			public IntPtr hNameMappings;

			[MarshalAs(UnmanagedType.LPWStr)]
			public string lpszProgressTitle;
		}

		/// <summary>
		/// Shell File Operations
		/// </summary>
		/// <remarks>Derived from shellapi.h</remarks>
		private enum FO : uint
		{
			FO_MOVE = 0x0001,
			FO_COPY = 0x0002,
			FO_DELETE = 0x0003,
			FO_RENAME = 0x0004,
		}

		/// <summary>
		/// Shell File Operation Flags
		/// </summary>
		/// <remarks>Derived from shellapi.h</remarks>
		[Flags]
		private enum FOF : ushort
		{
			FOF_MULTIDESTFILES = 0x0001,
			FOF_CONFIRMMOUSE = 0x0002,
			FOF_SILENT = 0x0004,                // Don't display progress UI (confirm prompts may be displayed still).
			FOF_RENAMEONCOLLISION = 0x0008,     // Automatically rename the source files to avoid the collisions.
			FOF_NOCONFIRMATION = 0x0010,        // Don't display confirmation UI, assume "yes" for cases that can be bypassed, "no" for those that can not.
			FOF_WANTMAPPINGHANDLE = 0x0020,     // Fill in SHFILEOPSTRUCT.hNameMappings. Must be freed using SHFreeNameMappings.
			FOF_ALLOWUNDO = 0x0040,             // Enable undo including Recycle behavior for IFileOperation::Delete().
			FOF_FILESONLY = 0x0080,             // Only operate on the files (non folders), both files and folders are assumed without this.
			FOF_SIMPLEPROGRESS = 0x0100,        // Don't show names of files.
			FOF_NOCONFIRMMKDIR = 0x0200,        // Don't display confirmation UI before making any needed directories, assume "Yes" in these cases.
			FOF_NOERRORUI = 0x0400,             // Don't put up error UI, other UI may be displayed, progress, confirmations.
			FOF_NOCOPYSECURITYATTRIBS = 0x0800, // Don't copy file security attributes (ACLs).
			FOF_NORECURSION = 0x1000,           // Don't recurse into directories for operations that would recurse.
			FOF_NO_CONNECTED_ELEMENTS = 0x2000, // Don't operate on connected elements ("xxx_files" folders that go with .htm files).
			FOF_WANTNUKEWARNING = 0x4000,       // During delete operation, warn if object is being permanently destroyed instead of recycling (partially overrides FOF_NOCONFIRMATION).
			FOF_NORECURSEREPARSE = 0x8000,      // Deprecated; the operations engine always does the right thing on FolderLink objects (symlinks, reparse points, folder shortcuts).
		}

		#endregion

		/// <summary>
		/// Moves a specified file to Recycle.
		/// </summary>
		/// <param name="filePath">File path</param>
		public static void MoveToRecycle(string filePath)
		{
			if (string.IsNullOrWhiteSpace(filePath))
				return;

			MoveToRecycle(filePath);
		}

		/// <summary>
		/// Moves specified files to Recycle.
		/// </summary>
		/// <param name="filePaths">File paths</param>
		public static void MoveToRecycle(params string[] filePaths)
		{
			if (!(filePaths?.Length > 0))
				return;

			var filePathCombined = string.Join("\0", filePaths) + '\0' + '\0';

			var sh = new SHFILEOPSTRUCT
			{
				hwnd = IntPtr.Zero,
				wFunc = FO.FO_DELETE,
				pFrom = filePathCombined,
				pTo = null,
				// FOF_SILENT is necessary to prevent this application's window from losing focus.
				fFlags = FOF.FOF_ALLOWUNDO | FOF.FOF_NOCONFIRMATION | FOF.FOF_SILENT,
				fAnyOperationsAborted = false,
				hNameMappings = IntPtr.Zero,
				lpszProgressTitle = null,
			};

			var result = SHFileOperation(ref sh);

			if (result != 0) // 0 means success or user canceled operation (it will never happen in this application).
				throw new Win32Exception($"Failed to move files to Recycle. Error code: {result} File paths: {string.Join(",", filePaths)}");
		}
	}
}
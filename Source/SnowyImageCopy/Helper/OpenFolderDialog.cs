using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// A folder dialog similar to <see cref="Microsoft.Win32.OpenFileDialog"/>
	/// </summary>
	public class OpenFolderDialog
	{
		#region Win32/COM

		[DllImport("Shell32.dll")]
		private static extern int SHILCreateFromPath(
			[MarshalAs(UnmanagedType.LPWStr)] string pszPath,
			out IntPtr ppidl,
			ref uint rgfInOut);

		[DllImport("Shell32.dll")]
		private static extern int SHCreateShellItem(
			IntPtr pidlParent,
			IntPtr psfParent,
			IntPtr pidl,
			out IShellItem ppsi);

		[ComImport]
		[Guid("DC1C5A9C-E88A-4dde-A5A1-60F82A20AEF7")]
		private class FileOpenDialog
		{ }

		// Based on
		// https://referencesource.microsoft.com/#PresentationFramework/src/Framework/MS/Internal/AppModel/ShellProvider.cs
		[ComImport]
		[Guid("D57C7288-D4AD-4768-BE02-9D969532D960")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IFileOpenDialog
		{
			[PreserveSig]
			int Show(IntPtr parent);

			void SetFileTypes(); // Not for use
			void SetFileTypeIndex(); // Not for use
			void GetFileTypeIndex(); // Not for use
			void Advise(); // Not for use
			void Unadvise(); // Not for use

			void SetOptions(FOS fos);
			FOS GetOptions();
			void SetDefaultFolder(IShellItem psi);
			void SetFolder(IShellItem psi);
			IShellItem GetFolder();
			IShellItem GetCurrentSelection();
			void SetFileName([MarshalAs(UnmanagedType.LPWStr)] string pszName);

			[return: MarshalAs(UnmanagedType.LPWStr)]
			void GetFileName();

			void SetTitle([MarshalAs(UnmanagedType.LPWStr)] string pszTitle);
			void SetOkButtonLabel([MarshalAs(UnmanagedType.LPWStr)] string pszText);
			void SetFileNameLabel([MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

			IShellItem GetResult();

			void AddPlace(); // Not for use
			void SetDefaultExtension(); // Not for use
			void Close(); // Not for use
			void SetClientGuid(); // Not for use
			void ClearClientData();
			void SetFilter(); // Not for use
			void GetResults(); // Not for use
			void GetSelectedItems(); // Not for use
		}

		// Based on
		// https://referencesource.microsoft.com/#PresentationFramework/src/Framework/MS/Internal/AppModel/ShellProvider.cs
		[ComImport]
		[Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		private interface IShellItem
		{
			void BindToHandler(); // Not for use
			IShellItem GetParent();

			[return: MarshalAs(UnmanagedType.LPWStr)]
			string GetDisplayName(SIGDN sigdnName);

			void GetAttributes(); // Not for use
			void Compare(); // Not for use
		}

		[Flags]
		private enum FOS
		{
			// ...
			FOS_PICKFOLDERS = 0x20,
			FOS_FORCEFILESYSTEM = 0x40,
			// ...
		}

		private enum SIGDN : uint
		{
			// ...
			SIGDN_FILESYSPATH = 0x80058000,
			// ...
		}

		private const int S_OK = 0;

		#endregion

		public string Title { get; set; }
		public string InitialPath { get; set; }
		public string SelectedPath { get; private set; }

		public bool ShowDialog()
		{
			return ShowDialog(IntPtr.Zero);
		}

		public bool ShowDialog(Window owner)
		{
			if (owner is null)
				return ShowDialog();

			return ShowDialog(new WindowInteropHelper(owner).Handle);
		}

		private bool ShowDialog(IntPtr owner)
		{
			IFileOpenDialog fod = null;
			try
			{
				fod = new FileOpenDialog() as IFileOpenDialog;
				fod.SetOptions(FOS.FOS_PICKFOLDERS | FOS.FOS_FORCEFILESYSTEM);

				if (!string.IsNullOrEmpty(InitialPath))
				{
					uint attribute = 0U;
					if ((SHILCreateFromPath(InitialPath, out IntPtr idl, ref attribute) == S_OK) &&
						(SHCreateShellItem(IntPtr.Zero, IntPtr.Zero, idl, out IShellItem item) == S_OK))
					{
						fod.SetFolder(item);
					}
				}

				if (!string.IsNullOrEmpty(Title))
					fod.SetTitle(Title);

				if (fod.Show(owner) != S_OK)
					return false;

				SelectedPath = fod.GetResult().GetDisplayName(SIGDN.SIGDN_FILESYSPATH);
				return true;
			}
			catch (COMException ce)
			{
				Debug.WriteLine($"Failed to manage open folder dialog.\r\n{ce}");
				return false;
			}
			finally
			{
				if (fod is not null)
					Marshal.FinalReleaseComObject(fod);
			}
		}
	}
}
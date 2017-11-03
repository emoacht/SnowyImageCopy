using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Xml;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// This application's window size and position
	/// </summary>
	internal class WindowPlacement
	{
		#region Win32

		[DllImport("User32.dll", SetLastError = true)]
		private static extern bool SetWindowPlacement(
			IntPtr hWnd,
			[In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("User32.dll", SetLastError = true)]
		private static extern bool GetWindowPlacement(
			IntPtr hWnd,
			out WINDOWPLACEMENT lpwndpl);

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		private struct WINDOWPLACEMENT
		{
			public int length;
			public int flags;
			public SW showCmd;
			public POINT ptMinPosition;
			public POINT ptMaxPosition;
			public RECT rcNormalPosition;
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int x;
			public int y;
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;
		}

		private enum SW
		{
			SW_HIDE = 0,
			SW_SHOWNORMAL = 1,
			SW_SHOWMINIMIZED = 2,
			SW_SHOWMAXIMIZED = 3,
			SW_SHOWNOACTIVATE = 4,
			SW_SHOW = 5,
			SW_MINIMIZE = 6,
			SW_SHOWMINNOACTIVE = 7,
			SW_SHOWNA = 8,
			SW_RESTORE = 9,
			SW_SHOWDEFAULT = 10,
		}

		#endregion

		#region Load/Save

		public static void Load(Window window, bool isNormal = true)
		{
			WINDOWPLACEMENT placement;
			if (!TryLoad(out placement))
				return;

			var handle = new WindowInteropHelper(window).Handle;

			placement.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
			placement.flags = 0; // No flag set
			placement.showCmd = isNormal ? SW.SW_SHOWNORMAL : SW.SW_SHOWMINNOACTIVE; // Make window state normal by default.

			SetWindowPlacement(handle, ref placement);
		}

		public static void Save(Window window)
		{
			var handle = new WindowInteropHelper(window).Handle;

			WINDOWPLACEMENT placement;
			GetWindowPlacement(handle, out placement);

			Save(placement);
		}

		private const string _placementFileName = "placement.xml";
		private static readonly string _placementFilePath = Path.Combine(FolderService.FolderAppDataPath, _placementFileName);

		private static bool TryLoad<T>(out T placement)
		{
			var fileInfo = new FileInfo(_placementFilePath);
			if (!fileInfo.Exists || (fileInfo.Length == 0))
			{
				placement = default(T);
				return false;
			}

			try
			{
				using (var sr = new StreamReader(_placementFilePath, Encoding.UTF8))
				using (var xr = XmlReader.Create(sr))
				{
					var serializer = new DataContractSerializer(typeof(T));
					placement = (T)serializer.ReadObject(xr);
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load window placement.\r\n{ex}");
				placement = default(T);
				return false;
			}
		}

		private static void Save<T>(T placement)
		{
			try
			{
				FolderService.AssureFolderAppData();

				using (var sw = new StreamWriter(_placementFilePath, false, Encoding.UTF8)) // BOM will be emitted.
				using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true }))
				{
					var serializer = new DataContractSerializer(typeof(T));
					serializer.WriteObject(xw, placement);
					xw.Flush();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to save window placement.\r\n{ex}");
			}
		}

		#endregion
	}
}
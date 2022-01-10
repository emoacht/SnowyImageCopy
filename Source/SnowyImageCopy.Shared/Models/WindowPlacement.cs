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

using MonitorAware.Models;

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

			public POINT(int x, int y)
			{
				this.x = x;
				this.y = y;
			}
		}

		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		private struct RECT
		{
			public int left;
			public int top;
			public int right;
			public int bottom;

			public int Width => right - left;
			public int Height => bottom - top;

			public RECT(int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			public static implicit operator RECT(System.Windows.Rect rect) => new RECT((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
			public static implicit operator System.Windows.Rect(RECT rect) => new System.Windows.Rect(rect.left, rect.top, rect.Width, rect.Height);
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

		#region Type

		[DataContract]
		private class Container
		{
			[DataMember]
			public WINDOWPLACEMENT Placement { get; private set; }

			public DpiScale Dpi => new DpiScale(_dpi.X, _dpi.Y);
			[DataMember(Name = "Dpi")]
			private readonly Point _dpi;

			public Container(WINDOWPLACEMENT placement, DpiScale dpi)
			{
				this.Placement = placement;
				this._dpi = new Point(dpi.DpiScaleX, dpi.DpiScaleY);
			}
		}

		#endregion

		#region Load/Save/Delete

		private static string GetPlacementFileName(in string value) => $"placement{value}.xml";
		private static string GetPlacementFilePath(in string value) => FolderService.GetAppDataFilePath(GetPlacementFileName(value));

		internal static void Load(in string indexString, Window window)
		{
			if (!TryLoad(GetPlacementFilePath(indexString), out Container container))
				return;

			var placement = container.Placement;

			var dpi = DpiHelper.GetDpiFromRect(placement.rcNormalPosition);
			if (!dpi.Equals(container.Dpi))
				return;

			var handle = new WindowInteropHelper(window).Handle;

			placement.length = Marshal.SizeOf<WINDOWPLACEMENT>();
			placement.flags = 0; // No flag set
			placement.showCmd = (window.WindowState == WindowState.Minimized)
				? SW.SW_SHOWMINNOACTIVE // If WindowState property is WindowState.Minimized, make window state minimized.
				: SW.SW_SHOWNORMAL;

			SetWindowPlacement(handle, ref placement);
		}

		internal static void Save(in string indexString, Window window)
		{
			var handle = new WindowInteropHelper(window).Handle;

			GetWindowPlacement(handle, out WINDOWPLACEMENT placement);

			var dpi = DpiHelper.GetDpiFromVisual(window);
			if (!dpi.IsIdentity())
			{
				placement.rcNormalPosition.right = placement.rcNormalPosition.left + (int)(placement.rcNormalPosition.Width / dpi.DpiScaleX);
				placement.rcNormalPosition.bottom = placement.rcNormalPosition.top + (int)(placement.rcNormalPosition.Height / dpi.DpiScaleY);
			}

			Save(GetPlacementFilePath(indexString), new Container(placement, dpi));
		}

		private static bool TryLoad<T>(in string filePath, out T instance)
		{
			var fileInfo = new FileInfo(filePath);
			if (fileInfo is { Exists: false } or { Length: 0 })
			{
				instance = default;
				return false;
			}

			try
			{
				using (var sr = new StreamReader(filePath, Encoding.UTF8))
				using (var xr = XmlReader.Create(sr))
				{
					var serializer = new DataContractSerializer(typeof(T));
					instance = (T)serializer.ReadObject(xr);
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to load window placement.\r\n{ex}");
				instance = default;
				return false;
			}
		}

		private static void Save<T>(in string filePath, T instance)
		{
			try
			{
				FolderService.AssureAppDataFolder();

				using (var sw = new StreamWriter(filePath, false, Encoding.UTF8)) // BOM will be emitted.
				using (var xw = XmlWriter.Create(sw, new XmlWriterSettings { Indent = true }))
				{
					var serializer = new DataContractSerializer(typeof(T));
					serializer.WriteObject(xw, instance);
					xw.Flush();
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to save window placement.\r\n{ex}");
			}
		}

		internal static void Delete(in string indexString) => FolderService.Delete(GetPlacementFilePath(indexString));

		#endregion
	}
}
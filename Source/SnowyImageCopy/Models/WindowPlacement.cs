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

			public RECT(int left, int top, int right, int bottom)
			{
				this.left = left;
				this.top = top;
				this.right = right;
				this.bottom = bottom;
			}

			public int Width => right - left;
			public int Height => bottom - top;
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

			[DataMember]
			public Point Scale { get; private set; }

			public Container(WINDOWPLACEMENT placement, Point scale)
			{
				this.Placement = placement;
				this.Scale = scale;
			}
		}

		#endregion

		#region Load/Save

		public static void Load(Window window, bool isNormal = true)
		{
			if (!TryLoad(out Container container))
				return;

			var placement = container.Placement;

			var scale = GetScale(placement.rcNormalPosition);
			if (scale != container.Scale)
				return;

			var handle = new WindowInteropHelper(window).Handle;

			placement.length = Marshal.SizeOf<WINDOWPLACEMENT>();
			placement.flags = 0; // No flag set
			placement.showCmd = isNormal ? SW.SW_SHOWNORMAL : SW.SW_SHOWMINNOACTIVE; // Make window state normal by default.

			SetWindowPlacement(handle, ref placement);
		}

		public static void Save(Window window)
		{
			var handle = new WindowInteropHelper(window).Handle;

			GetWindowPlacement(handle, out WINDOWPLACEMENT placement);

			var scale = GetScale(window);
			if ((scale.X != 1) || (scale.Y != 1))
			{
				placement.rcNormalPosition.right = placement.rcNormalPosition.left + (int)(placement.rcNormalPosition.Width / scale.X);
				placement.rcNormalPosition.bottom = placement.rcNormalPosition.top + (int)(placement.rcNormalPosition.Height / scale.Y);
			}

			Save(new Container(placement, scale));
		}

		private const string PlacementFileName = "placement.xml";
		private static readonly string _placementFilePath = Path.Combine(FolderService.AppDataFolderPath, PlacementFileName);

		private static bool TryLoad<T>(out T placement)
		{
			var fileInfo = new FileInfo(_placementFilePath);
			if (!fileInfo.Exists || (fileInfo.Length == 0))
			{
				placement = default;
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
				placement = default;
				return false;
			}
		}

		private static void Save<T>(T placement)
		{
			try
			{
				FolderService.AssureAppDataFolder();

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

		#region Scale

		private static Point GetScale(Window window)
		{
			var windowDpi = DpiChecker.GetDpiFromVisual(window);

			return GetScaleBase(windowDpi);
		}

		private static Point GetScale(RECT rect)
		{
			var monitorDpi = DpiChecker.GetDpiFromRect(new Rect(rect.left, rect.top, rect.right - rect.left, rect.bottom - rect.top));

			return GetScaleBase(monitorDpi);
		}

		private static Point GetScaleBase(Dpi monitorDpi)
		{
			var systemDpi = DpiChecker.GetSystemDpi();

			// Use Point structure as container of a combination of doubles.
			return new Point(
				(double)monitorDpi.X / systemDpi.X,
				(double)monitorDpi.X / systemDpi.Y);
		}

		#endregion
	}
}
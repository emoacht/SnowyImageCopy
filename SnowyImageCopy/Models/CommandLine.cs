using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// Command line options
	/// </summary>
	public static class CommandLine
	{
		#region Win32

		[DllImport("Kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AttachConsole(uint dwProcessId);

		private const uint ATTACH_PARENT_PROCESS = uint.MaxValue;

		[DllImport("Kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FreeConsole();

		#endregion


		/// <summary>
		/// Whether to show command line usage
		/// </summary>
		public static bool ShowsUsage
		{
			get { return CheckArgs("/?", "-?"); }
		}

		/// <summary>
		/// Whether to start auto check at startup of this application
		/// </summary>
		public static bool StartsAutoCheck
		{
			get { return CheckArgs(StartsAutoCheckOptions); }
		}
		private static string[] StartsAutoCheckOptions
		{
			get { return new[] { "/autocheck", "-autocheck", "/a", "-a" }; }
		}

		/// <summary>
		/// Whether to make window state minimized at startup of this application
		/// </summary>
		public static bool MakesWindowStateMinimized
		{
			get { return CheckArgs(MakesWindowStateMinimizedOptions); }
		}
		private static string[] MakesWindowStateMinimizedOptions
		{
			get { return new[] { "/minimized", "-minimized", "/m", "-m" }; }
		}

		/// <summary>
		/// Whether to record download log
		/// </summary>
		public static bool RecordsDownloadLog
		{
			get { return CheckArgs(RecordsDownloadLogOptions); }
		}
		private static string[] RecordsDownloadLogOptions
		{
			get { return new[] { "/recordlog", "-recordlog", "/r", "-r" }; }
		}


		public static void ShowUsage()
		{
			if (!AttachConsole(ATTACH_PARENT_PROCESS))
				return;

			Console.WriteLine(
				"\n" +
				"Usage: SnowyImageCopy [{0}] [{1}] [{2}]\n" +
				"{0}: Start auto check at startup\n" +
				"{1}: Make window state minimized at startup\n" +
				"{2}: Record download log",
				StartsAutoCheckOptions[0],
				MakesWindowStateMinimizedOptions[0],
				RecordsDownloadLogOptions[0]);

			FreeConsole();
		}


		#region Helper

		private static string[] _args;

		private static bool CheckArgs(params string[] options)
		{
			if (_args == null)
			{
				_args = Environment.GetCommandLineArgs()
					.Skip(1) // First arg is always executable file path.
					.Select(x => x.ToLower())
					.ToArray();
			}

			return _args.Any() && options.Any() && _args.Intersect(options).Any();
		}

		#endregion
	}
}
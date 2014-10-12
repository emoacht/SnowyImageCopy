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
			get { return CheckArgs(new string[] { "/?", "-?" }); }
		}

		/// <summary>
		/// Whether to start auto check at startup of this application
		/// </summary>
		public static bool StartsAutoCheck
		{
			get { return CheckArgs(startsAutoCheckOptions); }
		}
		private static readonly string[] startsAutoCheckOptions = { "/autocheck", "-autocheck", "/a", "-a" };

		/// <summary>
		/// Whether to make window state minimized at startup of this application
		/// </summary>
		public static bool MakesWindowStateMinimized
		{
			get { return CheckArgs(makesWindowStateMinimizedOptions); }
		}
		private static readonly string[] makesWindowStateMinimizedOptions = { "/minimized", "-minimized", "/m", "-m" };


		public static void ShowUsage()
		{
			if (!AttachConsole(ATTACH_PARENT_PROCESS))
				return;

			Console.WriteLine(
				"\n" +
				"Usage: SnowyImageCopy [{0}] [{1}]\n" +
				"{0}: Start auto check at startup\n" +
				"{1}: Make window state minimized at startup",
				startsAutoCheckOptions[0],
				makesWindowStateMinimizedOptions[0]);

			FreeConsole();
		}


		#region Helper

		private static string[] args;

		private static bool CheckArgs(IEnumerable<string> options)
		{
			if (args == null)
			{
				args = Environment.GetCommandLineArgs()
					.Skip(1) // First arg is executable file path.
					.Select(x => x.ToLower())
					.ToArray();
			}

			return args.Any() && args.Intersect(options).Any();
		}

		#endregion
	}
}
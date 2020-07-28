using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SnowyImageCopy.Models
{
	public class Workspace : IDisposable
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

		public Workspace()
		{
			if (!Debugger.IsAttached)
				SubscribeExceptions();
		}

		public virtual bool Initialize()
		{
			if (ShowsUsage)
			{
				WriteConsole(GetUsage());
				return false;
			}

			return TryCreateSemaphore(Properties.Settings.Default.AppId);
		}

		#region IDisposable member

		private bool _disposed = false;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (_disposed)
				return;

			if (disposing)
			{
				if (!Debugger.IsAttached)
					UnsubscribeExceptions();

				CloseSemaphore();
			}

			_disposed = true;
		}

		#endregion

		#region Semaphore

		protected Semaphore _semaphore;

		protected virtual bool TryCreateSemaphore(string name)
		{
			if (string.IsNullOrEmpty(name))
				throw new ArgumentNullException(nameof(name));

			_semaphore = new Semaphore(1, 1, name, out bool createdNew);
			Debug.WriteLine($"Semaphore -> {(createdNew ? "Go" : "No-Go")}");
			return createdNew;
		}

		protected virtual void CloseSemaphore() => _semaphore?.Dispose();

		#endregion

		#region Exception

		private void SubscribeExceptions()
		{
			Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
			TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		}

		private void UnsubscribeExceptions()
		{
			Application.Current.DispatcherUnhandledException -= OnDispatcherUnhandledException;
			TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;
			AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
		}

		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			OnException(sender, e.Exception);
		}

		private void OnUnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
		{
			OnException(sender, e.Exception);
		}

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			OnException(sender, e.ExceptionObject as Exception);
		}

		protected virtual void OnException(object sender, Exception exception, [CallerMemberName] string methodName = null)
		{
			LogService.RecordException(sender, exception);
		}

		#endregion

		#region Command-line

		/// <summary>
		/// Writes a specified value to console.
		/// </summary>
		/// <param name="value"></param>
		public static void WriteConsole(string value)
		{
			if (AttachConsole(ATTACH_PARENT_PROCESS))
			{
				Console.WriteLine(value);

				FreeConsole();
			}
		}

		/// <summary>
		/// Whether to show command line usage
		/// </summary>
		public static bool ShowsUsage => CheckArgs("/?", "-?");

		/// <summary>
		/// Whether to start auto check at startup of this application
		/// </summary>
		public static bool StartsAutoCheckAtStart => CheckArgs(StartsAutoCheckAtStartOptions);
		private static string[] StartsAutoCheckAtStartOptions => Propagate("autocheck");

		/// <summary>
		/// Whether to minimize window at startup of this application
		/// </summary>
		public static bool MinimizesWindowAtStart => CheckArgs(MinimizesWindowAtStartOptions);
		private static string[] MinimizesWindowAtStartOptions => Propagate("minimize");

		/// <summary>
		/// Whether to record download log
		/// </summary>
		public static bool RecordsDownloadLog => CheckArgs(RecordsDownloadLogOptions);
		private static string[] RecordsDownloadLogOptions => Propagate("recordlog");

		protected virtual string GetUsage()
		{
			return string.Format(
				"\n" +
				"Usage: SnowyImageCopy [{0}] [{1}] [{2}]\n" +
				"{0}: Start auto check at startup\n" +
				"{1}: Minimize window at startup\n" +
				"{2}: Record download log",
				StartsAutoCheckAtStartOptions[0],
				MinimizesWindowAtStartOptions[0],
				RecordsDownloadLogOptions[0]);
		}

		public static WindowState WindowStateAtStart => MinimizesWindowAtStart ? WindowState.Minimized : WindowState.Normal;

		private static string[] _args;

		protected static bool CheckArgs(params string[] options)
		{
			_args ??= Environment.GetCommandLineArgs()
				.Skip(1) // The first arg is always executable file path.
				.Select(x => x.ToLower())
				.ToArray();

			return (options != null) && _args.Intersect(options).Any();
		}

		protected static string[] Propagate(string source) =>
			Enumerable.Range(0, 4)
				.Select(x => x switch
				{
					0 => $"/{source}",
					1 => $"-{source}",
					2 => $"/{source[0]}",
					3 => $"-{source[0]}",
					_ => throw new InvalidOperationException() // SwitchExpressionException would be appropriate.
				})
				.ToArray();

		#endregion

		public virtual void Clean(Settings settings)
		{
			Settings.Delete(settings);
			WindowPlacement.Delete(settings.IndexString);
			Signatures.Delete(settings.IndexString);
		}
	}
}
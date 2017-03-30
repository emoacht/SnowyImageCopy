using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace SnowyTool
{
	public partial class App : Application
	{
		public App()
		{
			if (!Debugger.IsAttached)
				AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			if (!Debugger.IsAttached)
				this.DispatcherUnhandledException += OnDispatcherUnhandledException;
		}

		#region Exception

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			RecordException(sender, e.ExceptionObject as Exception);
		}

		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			RecordException(sender, e.Exception);
		}

		private void RecordException(object sender, Exception exception)
		{
			const string fileName = "exception.log";

			var content = $"[Date: {DateTime.Now} Sender: {sender}]" + Environment.NewLine
				+ exception + Environment.NewLine + Environment.NewLine;

			Trace.WriteLine(content);

			var filePathAppData = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				Assembly.GetExecutingAssembly().GetName().Name,
				fileName);

			try
			{
				var folderPathAppData = Path.GetDirectoryName(filePathAppData);
				if (!Directory.Exists(folderPathAppData))
					Directory.CreateDirectory(folderPathAppData);

				File.AppendAllText(filePathAppData, content);
			}
			catch (Exception ex)
			{
				Trace.WriteLine($"Failed to record exception to AppData.\r\n{ex}");
			}
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

using SnowyImageCopy.Models;
using SnowyImageCopy.Views;

namespace SnowyImageCopy
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

			if (CommandLine.ShowsUsage)
			{
				CommandLine.ShowUsage();
				this.Shutdown();
				return;
			}

			Settings.Load();

			ResourceService.Current.ChangeCulture(Settings.Current.CultureName);

			this.MainWindow = new MainWindow();
			this.MainWindow.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			base.OnExit(e);

			Settings.Save();
		}


		#region Exception

		private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			LogService.RecordException(sender, e.ExceptionObject as Exception);
		}

		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			LogService.RecordException(sender, e.Exception);

			e.Handled = true;
			Application.Current.Shutdown();
		}

		#endregion
	}
}
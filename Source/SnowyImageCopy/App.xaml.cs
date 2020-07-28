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
		private readonly Workspace _workspace;

		public App()
		{
			_workspace = new Workspace();
		}

		private Settings _settings;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			if (!_workspace.Initialize())
			{
				this.Shutdown(0); // This shutdown is expected behavior.
				return;
			}

			_settings = Settings.Load().DefaultIfEmpty(new Settings()).First();
			Settings.CommonCultureName = _settings.CultureName;
			_settings.Start();
			this.MainWindow = new MainWindow(_settings) { WindowState = Workspace.WindowStateAtStart };
			this.MainWindow.Show();
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_settings?.Stop();
			_workspace.Dispose();

			base.OnExit(e);
		}
	}
}
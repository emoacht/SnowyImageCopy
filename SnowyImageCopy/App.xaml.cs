using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

using SnowyImageCopy.Models;
using SnowyImageCopy.Views;

namespace SnowyImageCopy
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		public App()
		{
#if (!DEBUG)
			AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
#endif
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

#if (!DEBUG)
			this.DispatcherUnhandledException += OnDispatcherUnhandledException;
#endif

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
			RecordException(sender, e.ExceptionObject as Exception);
		}

		private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
		{
			RecordException(sender, e.Exception);

			e.Handled = true;
			Application.Current.Shutdown();
		}

		private void RecordException(object sender, Exception exception)
		{
			const string fileName = "exception.log";

			var content = String.Format(@"[Date: {0} Sender: {1}]", DateTime.Now, sender) + Environment.NewLine
				+ exception + Environment.NewLine + Environment.NewLine;

			Trace.WriteLine(content); // For the case when surrounded by #if (!DEBUG)

			var filePathAppData = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				Assembly.GetExecutingAssembly().GetName().Name,
				fileName);

			try
			{
				var folderPathAppData = Path.GetDirectoryName(filePathAppData);
				if (!String.IsNullOrEmpty(folderPathAppData) && !Directory.Exists(folderPathAppData))
					Directory.CreateDirectory(folderPathAppData);

				File.AppendAllText(filePathAppData, content);
			}
			catch (Exception ex)
			{
				Trace.WriteLine(String.Format("Failed to record exception to AppData. {0}", ex));
			}

			var result = MessageBox.Show(SnowyImageCopy.Properties.Resources.RecordException, ProductInfo.Product, MessageBoxButton.YesNo, MessageBoxImage.Error, MessageBoxResult.Yes);
			if (result != MessageBoxResult.Yes)
				return;

			var filePathDesktop = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
				fileName);

			try
			{
				File.AppendAllText(filePathDesktop, content);
			}
			catch (Exception ex)
			{
				Trace.WriteLine(String.Format("Failed to record exception to Desktop. {0}", ex));
			}
		}

		#endregion
	}
}
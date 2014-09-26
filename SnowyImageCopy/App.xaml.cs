using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;

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
			AppDomain.CurrentDomain.UnhandledException += (sender, args) => ReportException(sender, args.ExceptionObject as Exception);
#endif
		}

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

#if (!DEBUG)
			this.DispatcherUnhandledException += (sender, args) => ReportException(sender, args.Exception);
#endif

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


		#region Exception handling

		private static void ReportException(object sender, Exception exception)
		{
			var filePath = Path.Combine(
				Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
				Assembly.GetExecutingAssembly().GetName().Name,
				"exception.log");

			try
			{
				var content = String.Format(@"[Date: {0} Sender: {1}]", DateTime.Now, sender) + Environment.NewLine
					+ exception + Environment.NewLine + Environment.NewLine;

				Debug.WriteLine(content);

				var folderPath = Path.GetDirectoryName(filePath);
				if (!Directory.Exists(folderPath))
					Directory.CreateDirectory(folderPath);

				File.AppendAllText(filePath, content);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to report exception. {0}", ex);
			}
		}

		#endregion
	}
}
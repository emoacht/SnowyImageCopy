using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// This application's AppData folder
	/// </summary>
	internal static class FolderService
	{
		public static string GetAppDataFilePath(in string fileName) =>
			Path.Combine(AppDataFolderPath, fileName);

		public static string AppDataFolderPath => _appDataFolderPath ??= GetAppDataFolderPath();
		private static string _appDataFolderPath;

		private static string GetAppDataFolderPath()
		{
			var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			if (string.IsNullOrEmpty(appDataPath)) // This should not happen.
				throw new DirectoryNotFoundException();

			return Path.Combine(appDataPath, Assembly.GetExecutingAssembly().GetName().Name);
		}

		public static void AssureAppDataFolder()
		{
			if (!Directory.Exists(AppDataFolderPath))
				Directory.CreateDirectory(AppDataFolderPath);
		}
	}
}
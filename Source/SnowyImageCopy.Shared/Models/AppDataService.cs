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
	internal static class AppDataService
	{
		public static string FolderPath => _folderPath ??= GetFolderPath();
		private static string _folderPath;

		private static string GetFolderPath()
		{
			var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
			if (string.IsNullOrEmpty(appDataPath)) // This should not happen.
				throw new DirectoryNotFoundException();

			return Path.Combine(appDataPath, Assembly.GetExecutingAssembly().GetName().Name);
		}

		public static string GetFilePath(in string fileName) =>
			Path.Combine(FolderPath, fileName);

		public static string EnsureFolderPath()
		{
			if (!Directory.Exists(FolderPath))
				Directory.CreateDirectory(FolderPath);

			return FolderPath;
		}

		public static void Delete(in string filePath)
		{
			try
			{
				File.Delete(filePath);
			}
			catch
			{ }
		}
	}
}
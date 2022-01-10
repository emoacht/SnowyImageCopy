using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Lexicon.Properties;

namespace SnowyImageCopy.ViewModels
{
	public class DocumentViewModel : NotificationObject
	{
		public DocumentViewModel()
		{ }

		#region Source

		public string SourcePath
		{
			get => _sourcePath;
			private set => SetPropertyValue(ref _sourcePath, value);
		}
		private string _sourcePath;

		public string SourceText
		{
			get => _sourceText;
			private set => SetPropertyValue(ref _sourceText, value);
		}
		private string _sourceText;

		#endregion

		public bool IsOpen
		{
			get => _isOpen;
			private set => SetPropertyValue(ref _isOpen, value);
		}
		private bool _isOpen;

		public void OpenReadme()
		{
			IsOpen = false;
			SourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Resources.ReadmeFile);
			SourceText = SnowyImageCopy.Lexicon.Invariant.Readme;
			IsOpen = true;
		}

		public void OpenReadmeDelete()
		{
			IsOpen = false;
			SourcePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Resources.ReadmeFileDelete);
			SourceText = SnowyImageCopy.Lexicon.Invariant.Readme;
			IsOpen = true;
		}

		public void OpenLicense()
		{
			IsOpen = false;
			SourcePath = null;
			SourceText = GetResourceContent(SnowyImageCopy.Lexicon.Invariant.LicenseFile);
			IsOpen = true;
		}

		public void Close() => IsOpen = false;

		#region Helper

		private static string GetResourceContent(string resourceName)
		{
			var assembly = Assembly.GetExecutingAssembly();
			var resourcePath = assembly.GetManifestResourceNames().FirstOrDefault(x => x.EndsWith(resourceName));
			if (resourcePath is null)
				return null;

			using var s = assembly.GetManifestResourceStream(resourcePath);
			using var sr = new StreamReader(s);
			return sr.ReadToEnd();
		}

		#endregion
	}
}
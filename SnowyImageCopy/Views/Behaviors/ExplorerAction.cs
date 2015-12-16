using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Open folder by Explorer.
	/// </summary>
	public class ExplorerAction : TriggerAction<DependencyObject>
	{
		#region Property

		/// <summary>
		/// Folder path to be opened.
		/// </summary>
		public string FolderPath
		{
			get { return (string)GetValue(FolderPathProperty); }
			set { SetValue(FolderPathProperty, value); }
		}
		public static readonly DependencyProperty FolderPathProperty =
			DependencyProperty.Register(
				"FolderPath",
				typeof(string),
				typeof(ExplorerAction),
				new FrameworkPropertyMetadata(String.Empty));

		#endregion

		protected override void Invoke(object parameter)
		{
			var initialPath = FolderPath;

			if (!String.IsNullOrEmpty(initialPath) && !Directory.Exists(initialPath))
			{
				var parent = Path.GetDirectoryName(initialPath);
				if (!String.IsNullOrEmpty(parent))
					initialPath = parent;
			}

			Process.Start("Explorer.exe", String.Format(@"/e, {0}", initialPath));
		}
	}
}
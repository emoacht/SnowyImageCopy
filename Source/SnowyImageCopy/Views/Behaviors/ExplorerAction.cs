using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using Shell32;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Opens folder by Explorer.
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
				new FrameworkPropertyMetadata(string.Empty));

		#endregion

		protected override void Invoke(object parameter)
		{
			var initialPath = FolderPath;

			if (!string.IsNullOrEmpty(initialPath) && !Directory.Exists(initialPath))
			{
				try
				{
					var parent = Path.GetDirectoryName(initialPath);
					if (!string.IsNullOrEmpty(parent))
						initialPath = parent;
				}
				catch (ArgumentException)
				{ }
			}

			new Shell().Explore(initialPath);
		}
	}
}
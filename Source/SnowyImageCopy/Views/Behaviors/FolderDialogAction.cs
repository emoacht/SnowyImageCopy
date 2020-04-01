using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Opens folder dialog.
	/// </summary>
	public class FolderDialogAction : TriggerAction<DependencyObject>
	{
		#region Property

		/// <summary>
		/// Title shown in the dialog
		/// </summary>
		public string Title
		{
			get { return (string)GetValue(TitleProperty); }
			set { SetValue(TitleProperty, value); }
		}
		public static readonly DependencyProperty TitleProperty =
			DependencyProperty.Register(
				"Title",
				typeof(string),
				typeof(FolderDialogAction),
				new PropertyMetadata(default(string)));

		/// <summary>
		/// Folder path specified in the dialog.
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
			   typeof(FolderDialogAction),
			   new FrameworkPropertyMetadata(
				   default(string),
				   FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		#endregion

		protected override void Invoke(object parameter)
		{
			static string GetInitialPath(string source)
			{
				if (string.IsNullOrWhiteSpace(source))
					return null;

				if (Directory.Exists(source))
					return source;

				var parent = Path.GetDirectoryName(source);
				if (!string.IsNullOrEmpty(parent))
					return parent;

				return null;
			}

			var ofd = new OpenFolderDialog
			{
				Title = this.Title,
				InitialPath = GetInitialPath(FolderPath),
			};
			if (ofd.ShowDialog(Window.GetWindow(this.AssociatedObject)))
			{
				FolderPath = ofd.SelectedPath;
			}
		}
	}
}
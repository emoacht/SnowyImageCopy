using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Opens file dialog.
	/// </summary>
	public class FileDialogAction : TriggerAction<DependencyObject>
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
				typeof(FileDialogAction),
				new PropertyMetadata(default(string)));

		/// <summary>
		/// Filter for file extension in the dialog
		/// </summary>
		public string Filter
		{
			get { return (string)GetValue(FilterProperty); }
			set { SetValue(FilterProperty, value); }
		}
		public static readonly DependencyProperty FilterProperty =
			DependencyProperty.Register(
				"Filter",
				typeof(string),
				typeof(FileDialogAction),
				new PropertyMetadata(default(string)));

		/// <summary>
		/// File path specified in the dialog
		/// </summary>
		public string FilePath
		{
			get { return (string)GetValue(FilePathProperty); }
			set { SetValue(FilePathProperty, value); }
		}
		public static readonly DependencyProperty FilePathProperty =
			DependencyProperty.Register(
				"FilePath",
				typeof(string),
				typeof(FileDialogAction),
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

				var parent = Path.GetDirectoryName(source);
				if (!string.IsNullOrEmpty(parent))
					return parent;

				return null;
			}

			var ofd = new OpenFileDialog
			{
				Title = this.Title,
				Filter = this.Filter,
				InitialDirectory = GetInitialPath(FilePath)
			};
			if (ofd.ShowDialog(Window.GetWindow(this.AssociatedObject)) == true)
			{
				FilePath = ofd.FileName;
			}
		}
	}
}
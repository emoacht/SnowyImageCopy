using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Xaml.Behaviors;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Opens folder browser dialog.
	/// </summary>
	public class FolderDialogAction : TriggerAction<DependencyObject>
	{
		#region Property

		/// <summary>
		/// Explanation shown in the dialog.
		/// </summary>
		public string Explanation
		{
			get { return (string)GetValue(ExplanationProperty); }
			set { SetValue(ExplanationProperty, value); }
		}
		public static readonly DependencyProperty ExplanationProperty =
			DependencyProperty.Register(
				"Explanation",
				typeof(string),
				typeof(FolderDialogAction),
				new FrameworkPropertyMetadata(default));

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
				   default,
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

			using var fbd = new System.Windows.Forms.FolderBrowserDialog
			{
				Description = Explanation,
				SelectedPath = GetInitialPath(FolderPath)
			};
			if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				FolderPath = fbd.SelectedPath;
			}
		}
	}
}
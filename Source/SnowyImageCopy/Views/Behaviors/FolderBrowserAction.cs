using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;

namespace SnowyImageCopy.Views.Behaviors
{
	/// <summary>
	/// Opens folder browser dialog.
	/// </summary>
	public class FolderBrowserAction : TriggerAction<DependencyObject>
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
				typeof(FolderBrowserAction),
				new FrameworkPropertyMetadata(string.Empty));

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
				typeof(FolderBrowserAction),
				new FrameworkPropertyMetadata(
					string.Empty,
					FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

		#endregion

		protected override void Invoke(object parameter)
		{
			var initialPath = FolderPath;

			if (!string.IsNullOrEmpty(initialPath) && !Directory.Exists(initialPath))
			{
				var parent = Path.GetDirectoryName(initialPath);
				if (!string.IsNullOrEmpty(parent))
					initialPath = parent;
			}

			using (var fbd = new System.Windows.Forms.FolderBrowserDialog
			{
				Description = Explanation,
				SelectedPath = initialPath,
			})
			{
				if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					FolderPath = fbd.SelectedPath;
				}
			}
		}
	}
}
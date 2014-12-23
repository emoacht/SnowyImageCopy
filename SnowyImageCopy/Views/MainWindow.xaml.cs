using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using PerMonitorDpi.Views;
using SnowyImageCopy.Models;
using SnowyImageCopy.ViewModels;

namespace SnowyImageCopy.Views
{
	public partial class MainWindow : PerMonitorDpiWindow
	{
		public MainWindow()
		{
			this.InitializeComponent();
		}


		#region Property

		public bool IsWindowPlacementReliable
		{
			get { return (bool)GetValue(IsWindowPlacementReliableProperty); }
			set { SetValue(IsWindowPlacementReliableProperty, value); }
		}
		public static readonly DependencyProperty IsWindowPlacementReliableProperty =
			DependencyProperty.Register(
				"IsWindowPlacementReliable",
				typeof(bool),
				typeof(MainWindow),
				new FrameworkPropertyMetadata(false));

		#endregion


		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			IsWindowPlacementReliable = true; // This must be set before loading WindowPlacement.

			new WindowPlacement().Load(this, !CommandLine.MakesWindowStateMinimized);

			if (CommandLine.StartsAutoCheck)
			{
				var mainWindowViewModelInstance = this.DataContext as MainWindowViewModel;
				if (mainWindowViewModelInstance != null)
				{
					if (mainWindowViewModelInstance.CheckCopyAutoCommand.CanExecute())
						mainWindowViewModelInstance.CheckCopyAutoCommand.Execute();
				}
			}
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (e.Cancel)
				return;

			new WindowPlacement().Save(this);
		}
	}
}
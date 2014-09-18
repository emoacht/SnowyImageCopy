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

namespace SnowyImageCopy.Views
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : PerMonitorDpiWindow
	{
		public MainWindow()
		{
			this.InitializeComponent();
		}


		#region Dependency Property

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

			new WindowPlacement().Load(this);
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
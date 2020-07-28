using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using MonitorAware;

using SlateElement;

using SnowyImageCopy.Models;
using SnowyImageCopy.ViewModels;

namespace SnowyImageCopy.Views
{
	public partial class MainWindow : SlateWindow
	{
		private readonly MainWindowViewModel _mainWindowViewModel;

		public MainWindow(Settings settings) : base(SlateWindow.PrototypeResourceUriString)
		{
			InitializeComponent();

			this.DataContext = _mainWindowViewModel = new MainWindowViewModel(settings);
		}

		public override void OnApplyTemplate()
		{
			base.OnApplyTemplate();

			this.OptionsBorder.Child = new Options(this);
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
				new PropertyMetadata(false));

		#endregion

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			IsWindowPlacementReliable = true; // This must be set before loading WindowPlacement.

			WindowPlacement.Load(_mainWindowViewModel.Settings.IndexString, this);

			var monitorProperty = MonitorAwareProperty.GetInstance(this);
			if (monitorProperty != null)
			{
				SetDestinationColorProfile(monitorProperty.WindowHandler.ColorProfilePath);
				monitorProperty.WindowHandler.ColorProfileChanged += (_, e) => SetDestinationColorProfile(e.NewPath);
			}
		}

		private void SetDestinationColorProfile(string colorProfilePath)
		{
			_mainWindowViewModel.DestinationColorProfile = File.Exists(colorProfilePath)
				? new ColorContext(new Uri(colorProfilePath))
				: null;
		}

		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			if (e.Cancel)
				return;

			WindowPlacement.Save(_mainWindowViewModel.Settings.IndexString, this);

			_mainWindowViewModel.Dispose();
		}
	}
}
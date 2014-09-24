using System;
using System.Collections.Generic;
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

using SnowyImageCopy.ViewModels;

namespace SnowyImageCopy.Views
{
	/// <summary>
	/// Interaction logic for Options.xaml
	/// </summary>
	public partial class Options : UserControl
	{
		public Options()
		{
			InitializeComponent();

			this.DataContext = new OptionsViewModel();
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			var window = Window.GetWindow(this);
			if (window == null)
				return;

			var mainWindowViewModelInstance = window.DataContext as MainWindowViewModel;
			if (mainWindowViewModelInstance == null)
				return;

			((OptionsViewModel)this.DataContext).MainWindowViewModelInstance = mainWindowViewModelInstance;
		}
	}
}
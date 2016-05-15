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
using SnowyImageCopy.Views.Controls;

namespace SnowyImageCopy.Views
{
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

			// Binding in xaml will not work because MainWindowViewModel may be null when Options is instantiated.
			ChooseDeleteOnCopyButton.SetBinding(
				SlidingToggleButton.IsCheckedCopyProperty,
				new Binding(nameof(MainWindowViewModel.IsBrowserOpen))
				{
					Source = mainWindowViewModelInstance,
					Mode = BindingMode.OneWayToSource,
				});
		}
	}
}
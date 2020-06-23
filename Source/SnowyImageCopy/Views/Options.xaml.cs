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
using SnowyImageCopy.Views.Converters;

namespace SnowyImageCopy.Views
{
	public partial class Options : UserControl
	{
		private readonly MainWindowViewModel _mainWindowViewModel;

		public Options(MainWindow mainWindow)
		{
			InitializeComponent();

			_mainWindowViewModel = mainWindow?.DataContext as MainWindowViewModel ?? throw new InvalidOperationException();
			this.DataContext = new OptionsViewModel(_mainWindowViewModel.Settings);

			// If Options is defined in MainWindow's xaml, it will be instantiated before MainWindowViewModel
			// and so binding to MainWindowViewModel will not work.
			var booleanInverseConverter = new BooleanInverseConverter();

			PathGroupBox.SetBinding(
				UIElement.IsEnabledProperty,
				new Binding(nameof(MainWindowViewModel.IsCheckOrCopyOngoing))
				{
					Source = _mainWindowViewModel,
					Mode = BindingMode.OneWay,
					Converter = booleanInverseConverter
				});

			FileGroupBox.SetBinding(
				UIElement.IsEnabledProperty,
				new Binding(nameof(MainWindowViewModel.IsCheckOrCopyOngoing))
				{
					Source = _mainWindowViewModel,
					Mode = BindingMode.OneWay,
					Converter = booleanInverseConverter
				});

			LanguageComboBox.SetBinding(
				UIElement.IsEnabledProperty,
				new Binding(nameof(MainWindowViewModel.IsCheckOrCopyOngoing))
				{
					Source = _mainWindowViewModel,
					Mode = BindingMode.OneWay,
					Converter = booleanInverseConverter
				});
		}
	}
}
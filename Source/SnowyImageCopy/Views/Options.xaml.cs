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
using SnowyImageCopy.Views.Converters;

namespace SnowyImageCopy.Views
{
	public partial class Options : UserControl
	{
		public Options()
		{
			InitializeComponent();

			this.DataContext = new OptionsViewModel();
			this.Loaded += OnLoaded;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= OnLoaded;

			var mainWindowViewModel = Window.GetWindow(this)?.DataContext as MainWindowViewModel;
			if (mainWindowViewModel is null)
				return;

			// Binding in xaml will not work because MainWindowViewModel may be null when Options is instantiated.
			var booleanInverseConverter = new BooleanInverseConverter();

			PathGroupBox.SetBinding(
				UIElement.IsEnabledProperty,
				new Binding(nameof(MainWindowViewModel.IsCheckOrCopyOngoing))
				{
					Source = mainWindowViewModel,
					Mode = BindingMode.OneWay,
					Converter = booleanInverseConverter
				});

			FileGroupBox.SetBinding(
				UIElement.IsEnabledProperty,
				new Binding(nameof(MainWindowViewModel.IsCheckOrCopyOngoing))
				{
					Source = mainWindowViewModel,
					Mode = BindingMode.OneWay,
					Converter = booleanInverseConverter
				});

			ChooseDeleteOnCopyButton.SetBinding(
				SlidingToggleButton.IsCheckedCopyProperty,
				new Binding(nameof(MainWindowViewModel.IsBrowserOpen))
				{
					Source = mainWindowViewModel,
					Mode = BindingMode.OneWayToSource,
				});

			LanguageComboBox.SetBinding(
				UIElement.IsEnabledProperty,
				new Binding(nameof(MainWindowViewModel.IsCheckOrCopyOngoing))
				{
					Source = mainWindowViewModel,
					Mode = BindingMode.OneWay,
					Converter = booleanInverseConverter
				});
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
	public partial class Card : UserControl
	{
		private readonly CardViewModel _cardViewModel;

		public Card()
		{
			InitializeComponent();

			this.DataContext = _cardViewModel = new CardViewModel();
			this.Loaded += OnLoaded;
			this.IsVisibleChanged += OnIsVisibleChanged;
		}

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			this.Loaded -= OnLoaded;

			var mainWindow = Window.GetWindow(this);
			var mainWindowViewModel = mainWindow?.DataContext as MainWindowViewModel;
			if (mainWindowViewModel is null)
				return;

			_cardViewModel.Initialize(mainWindowViewModel);

			mainWindow.Closing += OnClosing;
		}

		private async void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			this.IsVisibleChanged -= OnIsVisibleChanged;

			await _cardViewModel.SearchFirstAsync();
		}

		private void OnClosing(object sender, CancelEventArgs e)
		{
			_cardViewModel.Dispose();
		}
	}
}
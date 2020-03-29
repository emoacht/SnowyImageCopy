using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Models;
using SnowyImageCopy.Models.Card;

namespace SnowyImageCopy.ViewModels
{
	public class CardViewModel : NotificationDisposableObject
	{
		public CardViewModel()
		{ }

		private MainWindowViewModel _mainWindowViewModel;

		internal void Initialize(MainWindowViewModel mainWindowViewModel)
		{
			this._mainWindowViewModel ??= mainWindowViewModel ?? throw new ArgumentNullException(nameof(mainWindowViewModel));
		}

		#region Local

		public CardConfigViewModel Local
		{
			get => _local;
			private set
			{
				if (_local != null)
					_local.PropertyChanged -= OnLocalPropertyChanged;

				_local = value;

				if (_local != null)
					_local.PropertyChanged += OnLocalPropertyChanged;

				RaisePropertyChanged();
				RaisePropertyChanged(nameof(LocalIsAvailable));
			}
		}
		private CardConfigViewModel _local;

		public bool LocalIsAvailable => (Local != null);

		private void OnLocalPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(CardConfigViewModel.IsChanged))
				DelegateCommand.RaiseCanExecuteChanged();
		}

		#endregion

		#region Command

		#region Search Command

		public DelegateCommand SearchCommand =>
			_searchCommand ??= new DelegateCommand(SearchExecute, CanSearchExecute);
		private DelegateCommand _searchCommand;

		private async void SearchExecute() => await SearchConfigAsync();
		private bool CanSearchExecute() => !_isSearching;

		#endregion

		#region Apply Command

		public DelegateCommand ApplyCommand =>
			_applyCommand ??= new DelegateCommand(ApplyExecute, CanApplyExecute);
		private DelegateCommand _applyCommand;

		private async void ApplyExecute() => await ApplyConfigAsync();
		private bool CanApplyExecute() => !_isApplying && (Local != null) && Local.IsChanged;

		#endregion

		#endregion

		#region Search/Apply

		private bool _isSearching;
		private bool _isApplying;

		private async Task SearchConfigAsync()
		{
			try
			{
				_isSearching = true;

				var disks = await Task.Run(() => DiskSearcher.Search());

				foreach (var disk in disks.Where(x => x.CanBeSD).OrderBy(x => x.PhysicalDrive))
				{
					var configNew = new CardConfigViewModel();

					if (!await configNew.ReadAsync(disk))
						continue;

					Local = configNew;
					return;
				}

				Local = null;
			}
			catch
			{
				SoundManager.PlayError();
			}
			finally
			{
				_isSearching = false;
				DelegateCommand.RaiseCanExecuteChanged();
			}
		}

		private async Task ApplyConfigAsync()
		{
			try
			{
				_isApplying = true;

				var configNew = new CardConfigViewModel();

				if (!await configNew.ReadAsync(Local.AssociatedDisk) ||
					(configNew.CID != Local.CID))
				{
					SoundManager.PlayError();
					return;
				}

				await Local.WriteAsync();
			}
			catch
			{
				SoundManager.PlayError();
			}
			finally
			{
				_isApplying = false;
				DelegateCommand.RaiseCanExecuteChanged();
			}
		}

		#endregion
	}
}
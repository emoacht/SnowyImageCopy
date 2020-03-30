using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Models;
using SnowyImageCopy.Models.Card;
using SnowyImageCopy.Properties;

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

			// Subscribe event handlers.
			Subscription.Add(Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>
				(
					handler => (sender, e) => handler(e),
					handler => mainWindowViewModel.Op.Card.PropertyChanged += handler,
					handler => mainWindowViewModel.Op.Card.PropertyChanged -= handler
				)
				.Throttle(TimeSpan.FromMilliseconds(100))
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(e =>
				{
					switch (e.PropertyName)
					{
						case nameof(CardState.FirmwareVersion):
						case nameof(CardState.Ssid):
						case nameof(CardState.Cid):
							Remote = new CardStateViewModel(this._mainWindowViewModel.Op.Card);
							break;
					}
				}));
		}

		#region Remote

		public CardStateViewModel Remote
		{
			get => _remote;
			private set
			{
				if (_remote != null)
					Subscription.Remove(_remote);

				_remote = value;

				if (_remote != null)
					Subscription.Add(_remote);

				RaisePropertyChanged();
				RaisePropertyChanged(nameof(RemoteIsAvailable));
			}
		}
		private CardStateViewModel _remote;

		public bool RemoteIsAvailable => (Remote != null);

		#endregion

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
			if (Designer.IsInDesignMode) // To avoid NullReferenceException in Design mode.
				return;

			try
			{
				_isSearching = true;
				_mainWindowViewModel.OperationStatus = Resources.OperationStatus_Card_Searching;

				var disks = await Task.Run(() => DiskSearcher.Search());

				foreach (var disk in disks.Where(x => x.CanBeSD).OrderBy(x => x.PhysicalDrive))
				{
					var configNew = new CardConfigViewModel();

					if (!await configNew.ReadAsync(disk))
						continue;

					Local = configNew;
					_mainWindowViewModel.OperationStatus = Resources.OperationStatus_Card_OneFound;
					return;
				}

				Local = null;
				_mainWindowViewModel.OperationStatus = Resources.OperationStatus_Card_NoFound;
			}
			catch
			{
				SoundManager.PlayError();
				_mainWindowViewModel.OperationStatus = Resources.OperationStatus_Failed;
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
				_mainWindowViewModel.OperationStatus = Resources.OperationStatus_Card_Applying;

				var configNew = new CardConfigViewModel();

				if (!await configNew.ReadAsync(Local.AssociatedDisk) ||
					(configNew.CID != Local.CID))
				{
					SoundManager.PlayError();
					_mainWindowViewModel.OperationStatus = Resources.OperationStatus_Card_Replaced;
					return;
				}

				await Local.WriteAsync();

				_mainWindowViewModel.OperationStatus = Resources.OperationStatus_Card_Applied;
			}
			catch
			{
				SoundManager.PlayError();
				_mainWindowViewModel.OperationStatus = Resources.OperationStatus_Failed;
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
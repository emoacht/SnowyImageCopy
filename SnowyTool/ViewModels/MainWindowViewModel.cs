using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

using SnowyTool.Common;
using SnowyTool.Models;

namespace SnowyTool.ViewModels
{
	public class MainWindowViewModel : ViewModel
	{
		#region Type

		/// <summary>
		/// Set of corresponding modes for APPMODE
		/// </summary>
		private class ModeSet
		{
			public int AppMode { get; private set; }
			public NetworkModeOption NetworkMode { get; private set; }
			public StartupModeOption StartupMode { get; private set; }

			public ModeSet(int appMode, NetworkModeOption networkMode, StartupModeOption startupMode)
			{
				this.AppMode = appMode;
				this.NetworkMode = networkMode;
				this.StartupMode = startupMode;
			}
		}

		#endregion


		#region Property

		public ConfigViewModel CurrentConfig
		{
			get { return _currentConfig; }
			set
			{
				_currentConfig = value;

				ModeSet currentMode = null;

				if (value != null)
				{
					currentMode = modeMap
						.Where(x => x.AppMode == value.APPMODE)
						.SingleOrDefault();
				}

				if (currentMode != null)
				{
					CurrentNetworkMode = currentMode.NetworkMode;
					CurrentStartupMode = currentMode.StartupMode;
				}
				else
				{
					CurrentNetworkMode = NetworkModeOption.None;
					CurrentStartupMode = StartupModeOption.None;
				}

				RaisePropertyChanged();
			}
		}
		private ConfigViewModel _currentConfig;


		public NetworkModeOption CurrentNetworkMode
		{
			get { return _currentNetworkMode; }
			set
			{
				if (_currentNetworkMode == value)
					return;

				_currentNetworkMode = value;

				RaisePropertyChanged();

				var currentMode = modeMap
					.Where(x => x.NetworkMode == value)
					.Where(x => x.StartupMode == CurrentStartupMode)
					.SingleOrDefault();

				if (currentMode != null)
					CurrentConfig.APPMODE = currentMode.AppMode;
			}
		}
		private NetworkModeOption _currentNetworkMode;

		public StartupModeOption CurrentStartupMode
		{
			get { return _currentStartupMode; }
			set
			{
				if (_currentStartupMode == value)
					return;

				_currentStartupMode = value;

				RaisePropertyChanged();

				var currentMode = modeMap
					.Where(x => x.StartupMode == value)
					.Where(x => x.NetworkMode == CurrentNetworkMode)
					.SingleOrDefault();

				if (currentMode != null)
					CurrentConfig.APPMODE = currentMode.AppMode;
			}
		}
		private StartupModeOption _currentStartupMode;

		private readonly ModeSet[] modeMap = new ModeSet[]
		{
			new ModeSet(0, NetworkModeOption.AccessPoint, StartupModeOption.Manual),
			new ModeSet(2, NetworkModeOption.Station, StartupModeOption.Manual),
			new ModeSet(3, NetworkModeOption.InternetPassThru, StartupModeOption.Manual),
			new ModeSet(4, NetworkModeOption.AccessPoint, StartupModeOption.Automatic),
			new ModeSet(5, NetworkModeOption.Station, StartupModeOption.Automatic),
			new ModeSet(6, NetworkModeOption.InternetPassThru, StartupModeOption.Automatic),
		};

		#endregion


		#region Command

		#region Search Command

		public DelegateCommand SearchCommand
		{
			get { return _searchCommand ?? (_searchCommand = new DelegateCommand(SearchExecute, CanSearchExecute)); }
		}
		private DelegateCommand _searchCommand;

		private async void SearchExecute()
		{
			await SearchConfigAsync();
		}

		private bool CanSearchExecute()
		{
			return !isSearching;
		}

		#endregion


		#region Apply Command

		public DelegateCommand ApplyCommand
		{
			get { return _applyCommand ?? (_applyCommand = new DelegateCommand(ApplyExecute, CanApplyExecute)); }
		}
		private DelegateCommand _applyCommand;

		private async void ApplyExecute()
		{
			await ApplyConfigAsync();
		}

		private bool CanApplyExecute()
		{
			return !isApplying;
		}

		#endregion

		#endregion


		#region Constructor

		public MainWindowViewModel()
		{
			InitializeTask = InitializeAsync();
		}

		public Task InitializeTask { get; private set; }

		private async Task InitializeAsync()
		{
			await SearchConfigAsync();
		}

		#endregion


		#region Search/Apply

		private bool isSearching;

		private async Task SearchConfigAsync()
		{
			isSearching = true;

			try
			{
				var drives = await Task.Run(() => DiskFinder.Search());

				var configList = new List<ConfigViewModel>();

				foreach (var drive in drives)
				{
					var configNew = new ConfigViewModel(drive);

					if (!await configNew.ReadAsync())
						continue;

					configList.Add(configNew);
				}

				CurrentConfig = configList
					.OrderBy(x => x.AssociatedDisk.PhysicalDrive)
					.FirstOrDefault();
			}
			finally
			{
				isSearching = false;
				DelegateCommand.RaiseCanExecuteChanged();
			}
		}

		private bool isApplying;

		private async Task ApplyConfigAsync()
		{
			isApplying = true;

			var configNew = new ConfigViewModel(CurrentConfig.AssociatedDisk);

			if (!await configNew.ReadAsync() ||
				(configNew.CID != CurrentConfig.CID))
			{
				SystemSounds.Hand.Play();
				MessageBox.Show("FlashAir seems to be changed.");
				return;
			}

			try
			{
				await CurrentConfig.WriteAsync();

				SystemSounds.Exclamation.Play();
				MessageBox.Show("Applied new config of FlashAir.");
			}
			catch (Exception ex)
			{
				SystemSounds.Hand.Play();
				MessageBox.Show("Failed to apply new config of FlashAir." + Environment.NewLine + ex.ToString());
			}
			finally
			{
				isApplying = false;
				DelegateCommand.RaiseCanExecuteChanged();
			}
		}

		#endregion
	}
}

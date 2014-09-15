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
		#region Interaction

		public string OperationStatus
		{
			get { return _operationStatus; }
			set
			{
				_operationStatus = value;
				RaisePropertyChanged();
			}
		}
		private string _operationStatus;

		#endregion


		#region Config

		/// <summary>
		/// Set of corresponding modes for APPMODE
		/// </summary>
		private class ModeSet
		{
			public int AppMode { get; private set; }
			public LanModeOption LanMode { get; private set; }
			public LanStartupModeOption LanStartupMode { get; private set; }

			public ModeSet(int appMode, LanModeOption lanMode, LanStartupModeOption lanStartupMode)
			{
				this.AppMode = appMode;
				this.LanMode = lanMode;
				this.LanStartupMode = lanStartupMode;
			}
		}

		public ConfigViewModel CurrentConfig
		{
			get { return _currentConfig; }
			set
			{
				_currentConfig = value;

				ModeSet currentMode = null;

				if (value != null)
				{
					currentMode = modeSetMap
						.Where(x => x.AppMode == value.APPMODE)
						.SingleOrDefault();
				}

				if (currentMode != null)
				{
					CurrentLanMode = currentMode.LanMode;
					CurrentLanStartupMode = currentMode.LanStartupMode;
				}
				else
				{
					CurrentLanMode = LanModeOption.None;
					CurrentLanStartupMode = LanStartupModeOption.None;
				}

				RaisePropertyChanged(() => IsUploadEnabled);

				RaisePropertyChanged();
			}
		}
		private ConfigViewModel _currentConfig;
		
		public LanModeOption CurrentLanMode
		{
			get { return _currentLanMode; }
			set
			{
				if (_currentLanMode == value)
					return;

				_currentLanMode = value;

				RaisePropertyChanged();

				var currentMode = modeSetMap
					.Where(x => x.LanMode == value)
					.Where(x => x.LanStartupMode == CurrentLanStartupMode)
					.SingleOrDefault();

				if ((CurrentConfig != null) && (currentMode != null))
					CurrentConfig.APPMODE = currentMode.AppMode;
			}
		}
		private LanModeOption _currentLanMode;

		public LanStartupModeOption CurrentLanStartupMode
		{
			get { return _currentLanStartupMode; }
			set
			{
				if (_currentLanStartupMode == value)
					return;

				_currentLanStartupMode = value;

				RaisePropertyChanged();

				var currentMode = modeSetMap
					.Where(x => x.LanStartupMode == value)
					.Where(x => x.LanMode == CurrentLanMode)
					.SingleOrDefault();

				if ((CurrentConfig != null) && (currentMode != null))
					CurrentConfig.APPMODE = currentMode.AppMode;
			}
		}
		private LanStartupModeOption _currentLanStartupMode;

		private readonly ModeSet[] modeSetMap = new ModeSet[]
		{
			new ModeSet(0, LanModeOption.AccessPoint, LanStartupModeOption.Manual),
			new ModeSet(2, LanModeOption.Station, LanStartupModeOption.Manual),
			new ModeSet(3, LanModeOption.InternetPassThru, LanStartupModeOption.Manual),
			new ModeSet(4, LanModeOption.AccessPoint, LanStartupModeOption.Automatic),
			new ModeSet(5, LanModeOption.Station, LanStartupModeOption.Automatic),
			new ModeSet(6, LanModeOption.InternetPassThru, LanStartupModeOption.Automatic),
		};

		public bool? IsUploadEnabled
		{
			get
			{
				if (CurrentConfig == null)
					return null;

				return (CurrentConfig.UPLOAD == 1);
			}
			set
			{
				if ((CurrentConfig != null) && value.HasValue)
					// If false, set any number other than 1.
					CurrentConfig.UPLOAD = value.Value ? 1 : 0;

				RaisePropertyChanged();
			}
		}

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
			return !isApplying && (CurrentConfig != null) && CurrentConfig.IsChanged;
		}

		#endregion

		#endregion


		#region Constructor

		public MainWindowViewModel()
		{
			InitializeTask = InitializeAsync();
		}

		private Task InitializeTask { get; set; }

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
				var drives = await Task.Run(() => DiskSearcher.Search());

				foreach (var drive in drives.Where(x => x.CanBeSD).OrderBy(x => x.PhysicalDrive))
				{
					var configNew = new ConfigViewModel();

					if (!await configNew.ReadAsync(drive))
						continue;

					CurrentConfig = configNew;
					OperationStatus = "Found FlashAir.";
					return;
				}

				CurrentConfig = null;
				OperationStatus = "No FlashAir";
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

			var configNew = new ConfigViewModel();

			if (!await configNew.ReadAsync(CurrentConfig.AssociatedDisk) ||
				(configNew.CID != CurrentConfig.CID))
			{
				SystemSounds.Hand.Play();
				OperationStatus = "FlashAir seems changed.";
				return;
			}

			try
			{
				await CurrentConfig.WriteAsync();

				SystemSounds.Exclamation.Play();
				OperationStatus = "Applied new config.";
			}
			catch (Exception ex)
			{
				SystemSounds.Hand.Play();
				OperationStatus = "Failed to apply new config.";
				Debug.WriteLine("Failed to apply new config. {0}", ex);
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

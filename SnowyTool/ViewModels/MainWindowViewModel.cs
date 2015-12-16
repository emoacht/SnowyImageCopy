using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;

using SnowyTool.Common;
using SnowyTool.Models;
using SnowyTool.Properties;

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

		private static readonly ModeSet[] _modeSetMap =
		{
			new ModeSet(0, LanModeOption.AccessPoint, LanStartupModeOption.Manual),
			new ModeSet(2, LanModeOption.Station, LanStartupModeOption.Manual),
			new ModeSet(3, LanModeOption.InternetPassThru, LanStartupModeOption.Manual),
			new ModeSet(4, LanModeOption.AccessPoint, LanStartupModeOption.Automatic),
			new ModeSet(5, LanModeOption.Station, LanStartupModeOption.Automatic),
			new ModeSet(6, LanModeOption.InternetPassThru, LanStartupModeOption.Automatic),
		};

		public ConfigViewModel CurrentConfig
		{
			get { return _currentConfig; }
			set
			{
				if (_currentConfig != null)
					_currentConfig.PropertyChanged -= OnCurrentConfigPropertyChanged;

				_currentConfig = value;

				if (_currentConfig != null)
					_currentConfig.PropertyChanged += OnCurrentConfigPropertyChanged;

				var currentMode = (value != null)
					? _modeSetMap.SingleOrDefault(x => x.AppMode == value.APPMODE)
					: null;

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

				RaisePropertyChanged();
				RaisePropertyChanged(() => IsUploadEnabled);
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

				if (CurrentConfig != null)
				{
					var currentMode = _modeSetMap
						.Where(x => x.LanMode == value)
						.SingleOrDefault(x => x.LanStartupMode == CurrentLanStartupMode);

					if (currentMode != null)
						CurrentConfig.APPMODE = currentMode.AppMode;
				}
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

				if (CurrentConfig != null)
				{
					var currentMode = _modeSetMap
						.Where(x => x.LanStartupMode == value)
						.SingleOrDefault(x => x.LanMode == CurrentLanMode);

					if (currentMode != null)
						CurrentConfig.APPMODE = currentMode.AppMode;
				}
			}
		}
		private LanStartupModeOption _currentLanStartupMode;

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

		private void OnCurrentConfigPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "IsChanged")
				DelegateCommand.RaiseCanExecuteChanged();
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
			return !_isSearching;
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
			return !_isApplying && (CurrentConfig != null) && CurrentConfig.IsChanged;
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

		private bool _isSearching;
		private bool _isApplying;

		private async Task SearchConfigAsync()
		{
			try
			{
				_isSearching = true;

				var drives = await Task.Run(() => DiskSearcher.Search());

				foreach (var drive in drives.Where(x => x.CanBeSD).OrderBy(x => x.PhysicalDrive))
				{
					var configNew = new ConfigViewModel();

					if (!await configNew.ReadAsync(drive))
						continue;

					CurrentConfig = configNew;
					OperationStatus = Resources.OperationStatus_Found;
					return;
				}

				CurrentConfig = null;
				OperationStatus = Resources.OperationStatus_No;
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

				var configNew = new ConfigViewModel();

				if (!await configNew.ReadAsync(CurrentConfig.AssociatedDisk) ||
					(configNew.CID != CurrentConfig.CID))
				{
					SystemSounds.Hand.Play();
					OperationStatus = Resources.OperationStatus_Changed;
					return;
				}

				try
				{
					await CurrentConfig.WriteAsync();

					SystemSounds.Asterisk.Play();
					OperationStatus = Resources.OperationStatus_Applied;
				}
				catch (Exception ex)
				{
					SystemSounds.Hand.Play();
					OperationStatus = Resources.OperationStatus_Failed;
					Debug.WriteLine("Failed to apply new config. {0}", ex);
				}
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
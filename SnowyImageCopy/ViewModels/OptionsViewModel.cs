using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Models;

namespace SnowyImageCopy.ViewModels
{
	public class OptionsViewModel : ViewModel
	{
		/// <summary>
		/// Instance of MainWindowViewModel
		/// </summary>
		public MainWindowViewModel MainWindowViewModelInstance
		{
			get { return _mainWindowViewModelInstance; }
			set
			{
				if (_mainWindowViewModelInstance != null)
					return;

				_mainWindowViewModelInstance = value;

				operationPropertyChangedListener = new PropertyChangedEventListener(ReactOperationPropertyChanged);
				PropertyChangedEventManager.AddListener(MainWindowViewModelInstance.Op, operationPropertyChangedListener, String.Empty);
			}
		}
		private MainWindowViewModel _mainWindowViewModelInstance;

		public OptionsViewModel()
		{
			// MainWindow may be null when Options is instantiated.
		}


		#region Event Listener

		private PropertyChangedEventListener operationPropertyChangedListener;

		private string[] CaseIsCheckingOrCopying
		{
			get
			{
				if (_caseIsCheckingOrCopying == null)
				{
					var operation = default(Operation);

					_caseIsCheckingOrCopying = new[]
					{
						PropertySupport.GetPropertyName(() => operation.IsChecking),
						PropertySupport.GetPropertyName(() => operation.IsCopying),
					};
				}

				return _caseIsCheckingOrCopying;
			}
		}
		private string[] _caseIsCheckingOrCopying;

		private void ReactOperationPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Operation property changed (OptionsViewModel): {0} {1}", sender, e.PropertyName);

			if (CaseIsCheckingOrCopying.Contains(e.PropertyName))
			{
				IsCheckingOrCopying = MainWindowViewModelInstance.Op.IsChecking || MainWindowViewModelInstance.Op.IsCopying;
			}
		}

		#endregion


		#region Operation

		public bool IsCheckingOrCopying
		{
			get { return _isCheckingOrCopying; }
			set
			{
				_isCheckingOrCopying = value;
				RaisePropertyChanged();
			}
		}
		private bool _isCheckingOrCopying;

		#endregion


		public Settings SettingsCurrent
		{
			get { return Settings.Current; }
		}


		#region Path

		// Left to View.

		#endregion


		#region Date

		public List<FilePeriodViewModel> FilePeriodList
		{
			get
			{
				if (_filePeriodList == null)
				{
					_filePeriodList = Enum.GetValues(typeof(FilePeriod)).OfType<FilePeriod>()
						.Select(x => new FilePeriodViewModel { Period = x })
						.ToList();
				}

				return _filePeriodList;
			}
			set
			{
				_filePeriodList = value;
				RaisePropertyChanged();
			}
		}
		private List<FilePeriodViewModel> _filePeriodList;

		public FilePeriodViewModel FilePeriodSelected
		{
			get { return FilePeriodList.FirstOrDefault(x => x.Period == Settings.Current.TargetPeriod); }
			set
			{
				Settings.Current.TargetPeriod = value.Period;
				RaisePropertyChanged();
			}
		}

		#endregion


		#region Auto Check

		// Left to View.

		#endregion


		#region File

		// Left to View.

		#endregion


		#region Language

		private Dictionary<string, string> cultureMap;
		private const string cultureNameAuto = "(auto)";

		public string[] Cultures
		{
			get
			{
				if (cultureMap == null)
				{
					cultureMap = new[] { new KeyValuePair<string, string>(String.Empty, cultureNameAuto) }
						.Concat(ResourceService.Current.SupportedCultures
							.Select(x => new KeyValuePair<string, string>(x.Name, x.EnglishName)) // Or NativeName
							.OrderBy(x => x.Value))
						.ToDictionary(x => x.Key, x => x.Value);
				}

				return cultureMap.Select(x => x.Value).ToArray();
			}
		}

		public int CultureSeletedIndex
		{
			get
			{
				var index = cultureMap.Select(x => x.Key).ToList().FindIndex(x => x == Settings.Current.CultureName);

				return (0 <= index) ? index : 0;
			}
			set
			{
				var cultureName = cultureMap.Select(x => x.Key).ToList()[value];

				// If cultureName is empty, Culture of this application's Resources will be automatically selected.
				ResourceService.Current.ChangeCulture(cultureName);

				Settings.Current.CultureName = cultureName;
				RaisePropertyChanged();
			}
		}

		#endregion


		#region Info

		// Left to View.

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Models;

namespace SnowyImageCopy.ViewModels
{
	public class OptionsViewModel : NotificationObject
	{
		public OptionsViewModel()
		{ }

		public Settings SettingsCurrent => Settings.Current;

		#region Date

		public IReadOnlyCollection<FilePeriodViewModel> FilePeriodList
		{
			get => _filePeriodList ??= Enum.GetValues(typeof(FilePeriod))
				.Cast<FilePeriod>()
				.Select(x => new FilePeriodViewModel { Period = x })
				.ToArray();
		}
		private FilePeriodViewModel[] _filePeriodList;

		public FilePeriodViewModel FilePeriodSelected
		{
			get => FilePeriodList.Single(x => x.Period == Settings.Current.TargetPeriod);
			set
			{
				Settings.Current.TargetPeriod = value.Period;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region Language

		private Dictionary<string, string> _cultureMap;
		private const string CultureNameAuto = "(auto)";

		public string[] Cultures
		{
			get
			{
				_cultureMap ??= new[] { new KeyValuePair<string, string>(string.Empty, CultureNameAuto) }
					.Concat(ResourceService.Current.SupportedCultures
						.Select(x => new KeyValuePair<string, string>(x.Name, x.EnglishName)) // Or NativeName
						.OrderBy(x => x.Value))
					.ToDictionary(x => x.Key, x => x.Value);

				return _cultureMap.Values.ToArray();
			}
		}

		public int CultureSeletedIndex
		{
			get
			{
				var index = _cultureMap.Keys.ToList().FindIndex(x => x == Settings.Current.CultureName);
				return Math.Max(0, index);
			}
			set
			{
				var cultureName = _cultureMap.Keys.ToList()[value];

				// If cultureName is empty, Culture of this application's Resources will be automatically selected.
				ResourceService.Current.ChangeCulture(cultureName);

				Settings.Current.CultureName = cultureName;
				RaisePropertyChanged();
			}
		}

		#endregion
	}
}
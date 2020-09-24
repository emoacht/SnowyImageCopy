using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Models;
using SnowyImageCopy.Models.ImageFile;
using SnowyImageCopy.Views;

namespace SnowyImageCopy.ViewModels
{
	public class OptionsViewModel : NotificationObject
	{
		public Settings Settings { get; }

		public OptionsViewModel(Settings settings)
		{
			this.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
		}

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
			get => FilePeriodList.Single(x => x.Period == Settings.TargetPeriod);
			set
			{
				Settings.TargetPeriod = value.Period;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region Language

		private const string CultureNameAuto = "(auto)";

		private static IReadOnlyDictionary<string, string> CultureMap
		{
			get => _culureMap ??= ResourceService.Current.SupportedCultures
				.Select(x => (cultureName: x.Name, friendlyName: x.EnglishName /* Or NativeName */))
				.OrderBy(x => x.friendlyName)
				.Prepend((cultureName: string.Empty, friendlyName: CultureNameAuto))
				.ToDictionary(x => x.cultureName, x => x.friendlyName);
		}
		private static Dictionary<string, string> _culureMap;

		public static string[] Cultures => CultureMap.Values.ToArray();

		public static CultureViewModel Culture { get; } = GetCultureViewModel();

		// This method and delegate are to access private constructor without exposing static instance. 
		private static CultureViewModel GetCultureViewModel()
		{
			RuntimeHelpers.RunClassConstructor(typeof(CultureViewModel).TypeHandle);
			return CreateCultureViewModel.Invoke();
		}

		private static Func<CultureViewModel> CreateCultureViewModel;

		public class CultureViewModel : NotificationObject
		{
			static CultureViewModel() => CreateCultureViewModel = () => new CultureViewModel();

			private CultureViewModel()
			{ }

			public int SeletedIndex
			{
				get => _selectedIndex ??= Math.Max(0, CultureMap.Keys.ToList().FindIndex(x => x == Settings.CommonCultureName));
				set
				{
					_selectedIndex = value;
					Settings.CommonCultureName = CultureMap.Keys.ToList()[value];
					RaisePropertyChanged();
				}
			}
			private int? _selectedIndex;
		}

		#endregion

		#region Info

		public string Title
		{
			get => _title ?? ProductInfo.Title;
			set => SetPropertyValue(ref _title, value);
		}
		public string _title;

		public Version Version
		{
			get => _version ?? ProductInfo.Version;
			set => SetPropertyValue(ref _version, value);
		}
		public Version _version;

		#endregion
	}
}
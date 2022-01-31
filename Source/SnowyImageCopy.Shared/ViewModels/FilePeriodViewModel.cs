using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Lexicon;
using SnowyImageCopy.Lexicon.Properties;
using SnowyImageCopy.Models.ImageFile;

namespace SnowyImageCopy.ViewModels
{
	public class FilePeriodViewModel : NotificationObject
	{
		public FilePeriodViewModel()
		{
			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_resourcesPropertyChangedListener = new PropertyChangedEventListener(OnResourcesPropertyChanged);
				PropertyChangedEventManager.AddListener(ResourceService.Current, _resourcesPropertyChangedListener, nameof(ResourceService.Resources));
			}
		}

		#region Event listener

		private readonly PropertyChangedEventListener _resourcesPropertyChangedListener;

		private void OnResourcesPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			SetDescription(Period);
		}

		#endregion

		public FilePeriod Period
		{
			get => _period;
			set
			{
				_period = value;
				SetDescription(value);
			}
		}
		private FilePeriod _period;

		public string Description
		{
			get => _description;
			set => SetPropertyValue(ref _description, value);
		}
		private string _description;

		private void SetDescription(FilePeriod period)
		{
			Description = period switch
			{
				FilePeriod.All => Resources.Options_DateAll,
				FilePeriod.Today => Resources.Options_DateToday,
				FilePeriod.Recent => Resources.Options_DateRecent,
				FilePeriod.Select => Resources.Options_DateSelect,
				_ => throw new InvalidOperationException()
			};
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Models;
using SnowyImageCopy.Properties;

namespace SnowyImageCopy.ViewModels
{
	public class FilePeriodViewModel : NotificationObject
	{
		public FilePeriodViewModel()
		{
			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_resourcesPropertyChangedListener = new PropertyChangedEventListener(ReactResourcesPropertyChanged);
				PropertyChangedEventManager.AddListener(ResourceService.Current, _resourcesPropertyChangedListener, nameof(ResourceService.Resources));
			}
		}

		#region Event listener

		private PropertyChangedEventListener _resourcesPropertyChangedListener;

		private void ReactResourcesPropertyChanged(object sender, PropertyChangedEventArgs e)
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
			Description = GetDescription();

			string GetDescription()
			{
				switch (period)
				{
					case FilePeriod.Today:
						return Resources.Options_DateToday;
					case FilePeriod.Select:
						return Resources.Options_DateSelect;
					default:
						return Resources.Options_DateAll;
				}
			}
		}
	}
}
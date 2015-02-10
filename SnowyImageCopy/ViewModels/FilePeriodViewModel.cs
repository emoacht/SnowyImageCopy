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
	public class FilePeriodViewModel : ViewModel
	{
		public FilePeriodViewModel()
		{
			if (!Designer.IsInDesignMode) // AddListener source may be null in Design mode.
			{
				_resourcesPropertyChangedListener = new PropertyChangedEventListener(ReactResourcesPropertyChanged);
				PropertyChangedEventManager.AddListener(ResourceService.Current, _resourcesPropertyChangedListener, "Resources");
			}
		}


		#region Event Listener

		private PropertyChangedEventListener _resourcesPropertyChangedListener;

		private void ReactResourcesPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Debug.WriteLine("Resources property changed: {0} {1}", sender, e.PropertyName);

			SetDescription(Period);
		}

		#endregion


		public FilePeriod Period
		{
			get { return _period; }
			set
			{
				_period = value;
				SetDescription(value);
			}
		}
		private FilePeriod _period;

		public string Description
		{
			get { return _description; }
			set
			{
				_description = value;
				RaisePropertyChanged();
			}
		}
		private string _description;


		private void SetDescription(FilePeriod period)
		{
			switch (period)
			{
				case FilePeriod.Today:
					Description = Resources.Options_DateToday;
					break;
				case FilePeriod.Select:
					Description = Resources.Options_DateSelect;
					break;
				default:
					Description = Resources.Options_DateAll;
					break;
			}
		}
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Models.Card;

namespace SnowyImageCopy.ViewModels
{
	public class CardStateViewModel : NotificationSubscriptionObject
	{
		private readonly CardState _card;

		internal CardStateViewModel(CardState card)
		{
			this._card = card ?? throw new ArgumentNullException(nameof(card));
			Cid = card.Cid;

			// Subscribe event handlers.
			Subscription.Add(Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>
				(
					handler => (sender, e) => handler(e),
					handler => card.PropertyChanged += handler,
					handler => card.PropertyChanged -= handler
				)
				.ObserveOn(SynchronizationContext.Current)
				.Subscribe(e =>
				{
					switch (e.PropertyName)
					{
						case nameof(CardState.Capacity):
							RaisePropertyChanged(nameof(FreeCapacity));
							RaisePropertyChanged(nameof(TotalCapacity));
							RaisePropertyChanged(nameof(UsedPercentage));
							break;
					}
				}));
		}

		public string FirmwareVersion => _card.FirmwareVersion;

		#region CID & SSID

		public string Cid
		{
			get => _cid.Source;
			private set => _cid.Import(value);
		}
		private readonly CidInfo _cid = new();

		public string Ssid => _card.Ssid;

		#endregion

		#region Content of CID

		public string ProductName => _cid.ProductName;
		public string ProductRevision => _cid.ProductRevision;
		public uint ProductSerialNumber => _cid.ProductSerialNumber;
		public DateTime ManufacturingDate => _cid.ManufacturingDate;

		#endregion

		public ulong FreeCapacity => _card.FreeCapacity;
		public ulong TotalCapacity => _card.TotalCapacity;
		public float UsedPercentage => (TotalCapacity > 0) ? ((TotalCapacity - FreeCapacity) * 100F / TotalCapacity) : 0F;
	}
}
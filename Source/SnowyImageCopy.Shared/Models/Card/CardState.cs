using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Models.Card
{
	/// <summary>
	/// State of FlashAir card
	/// </summary>
	internal class CardState : NotificationObject, ICardState
	{
		/// <summary>
		/// Whether FlashAir card is changed
		/// </summary>
		/// <returns>
		/// True:  If changed.
		/// False: If not changed.
		/// Null:  If cannot detect change.
		/// </returns>
		/// <remarks>
		/// Checking not only CID but also firmware version and SSID is for the case that firmware version
		/// of a card is too old to support request for CID. However, if cards have the same firmware version
		/// and if the firmware version is too old and if cards have the same SSID, change of cards cannot
		/// be detected.
		/// Prior to draw value out of this property, getting the three parameters in sequence is required.
		/// </remarks>
		public bool? IsChanged
		{
			get
			{
				if (_isFirmwareVersionChanged || _isSsidChanged)
					return true;

				return CanGetCid ? _isCidChanged : (bool?)null;
			}
		}

		#region Firmware version

		/// <summary>
		/// Firmware version
		/// </summary>
		public string FirmwareVersion
		{
			get => _firmwareVersion;
			set
			{
				_isFirmwareVersionChanged = (_firmwareVersion != value);
				if (!_isFirmwareVersionChanged)
					return;

				_firmwareVersion = value;
				OnPropertyChanged();

				if (!VersionAddition.TryFind(value, out Version version))
					return;

				_isFirmwareVersion103OrNewer = (version >= new Version(1, 0, 3));
				_isFirmwareVersion202OrNewer = (version >= new Version(2, 0, 2));
				_isFirmwareVersion300OeNewer = (version >= new Version(3, 0, 0));
			}
		}
		private string _firmwareVersion;
		private bool _isFirmwareVersionChanged;

		private bool _isFirmwareVersion103OrNewer; // Equal to or newer than 1.00.03
		private bool _isFirmwareVersion202OrNewer; // Equal to or newer than 2.00.02
		private bool _isFirmwareVersion300OeNewer; // Equal to or newer than 3.00.00

		#endregion

		#region CID & SSID

		public bool CanGetCid => _isFirmwareVersion103OrNewer;

		/// <summary>
		/// CID
		/// </summary>
		public string Cid
		{
			get => _cid;
			set
			{
				_isCidChanged = (_cid != value);
				if (!_isCidChanged)
					return;

				_cid = value;
				OnPropertyChanged();
			}
		}
		private string _cid;
		private bool _isCidChanged;

		/// <summary>
		/// SSID
		/// </summary>
		public string Ssid
		{
			get => _ssid;
			set
			{
				_isSsidChanged = (_ssid != value);
				if (!_isSsidChanged)
					return;

				_ssid = value;
				OnPropertyChanged();
			}
		}
		private string _ssid;
		private bool _isSsidChanged;

		/// <summary>
		/// Whether PC is connected to FlashAir card by a wireless connection
		/// </summary>
		/// <remarks>
		/// True:  Access Point mode, Internet pass-thru mode
		/// False: Station mode
		/// </remarks>
		public bool IsWirelessConnected { get; set; }

		#endregion

		#region Capacity

		public bool CanGetCapacity => _isFirmwareVersion103OrNewer;

		/// <summary>
		/// Free/Total capacities of FlashAir card
		/// </summary>
		public Tuple<ulong, ulong> Capacity
		{
			get => _capacity;
			set
			{
				if (value is null)
				{
					if (_capacity is null)
						return;
				}
				else if (value.Equals(_capacity))
				{
					return;
				}

				_capacity = value;
				OnPropertyChanged();
			}
		}
		private Tuple<ulong, ulong> _capacity;

		public ulong FreeCapacity => _capacity?.Item1 ?? 0UL;
		public ulong TotalCapacity => _capacity?.Item2 ?? 0UL;

		#endregion

		#region Time stamp of write event

		public bool CanGetWriteTimeStamp => _isFirmwareVersion202OrNewer;

		/// <summary>
		/// Time stamp of write event
		/// </summary>
		public int WriteTimeStamp
		{
			get => _writeTimeStamp;
			set => SetProperty(ref _writeTimeStamp, value);
		}
		private int _writeTimeStamp = -1;

		#endregion

		#region Upload

		public bool CanGetUpload => _isFirmwareVersion202OrNewer;

		/// <summary>
		/// Upload parameter
		/// </summary>
		public int Upload
		{
			get => _upload;
			set => SetProperty(ref _upload, value);
		}
		private int _upload = -1;

		/// <summary>
		/// Whether upload.cgi is disabled
		/// </summary>
		/// <remarks>
		/// False will not always mean upload.cgi is enabled. Because request for Upload parameter is 
		/// supported only by newer firmware version, there is no straightforward way to confirm it.
		/// </remarks>
		public bool IsUploadDisabled
		{
			get
			{
				if (!CanGetUpload)
					return false;

				// 1:     Uploading is enabled.
				// Other: Uploading is disabled.
				return (Upload != 1);
			}
		}

		#endregion
	}
}
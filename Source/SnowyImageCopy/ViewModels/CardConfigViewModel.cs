using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Common;
using SnowyImageCopy.Helper;
using SnowyImageCopy.Models.Card;

namespace SnowyImageCopy.ViewModels
{
	public class CardConfigViewModel : NotificationObject
	{
		#region Type

		/// <summary>
		/// Attribute to indicate a persistent member of CONFIG file
		/// </summary>
		[AttributeUsage(AttributeTargets.Property)]
		private class PersistentMemberAttribute : Attribute
		{
			public bool IsMonitored { get; private set; }

			public PersistentMemberAttribute(bool isMonitored = false) => this.IsMonitored = isMonitored;
		}

		#endregion

		#region Content of CONFIG file

		/// <summary>
		/// Wireless LAN mode
		/// </summary>
		/// <remarks>
		/// 0: Removes the write protection from the "Wireless LAN Boot Screen" image. Sets the Wireless LAN to AP mode.
		/// 2: Removes the write protection from the "Wireless LAN Boot Screen" image. Sets the Wireless LAN to STA mode.
		/// 3: Removes the write protection from the "Wireless LAN Boot Screen" image. Sets the Wireless LAN to Internet pass-thru mode. (FW 2.00.02+)
		/// 4: Wireless LAN functionality will be set when the card is turned on. Sets the Wireless LAN to AP mode.
		/// 5: Wireless LAN functionality will be set when the card is turned on. Sets the Wireless LAN to STA mode.
		/// 6: Wireless LAN functionality will be set when the card is turned on. Sets the Wireless LAN to Internet pass-thru mode. (FW 2.00.02+)
		/// Other: Undefined behavior.
		/// </remarks>
		[PersistentMember(true)]
		public int APPMODE
		{
			get => _APPMODE;
			set
			{
				_APPMODE = value;
				RaisePropertyChanged(nameof(IsChanged));
			}
		}
		private int _APPMODE = 4; // Default

		/// <summary>
		/// NETBIOS/Bonjour name (Address of FlashAir)
		/// </summary>
		/// <remarks>
		/// Format: 15 characters
		/// If this parameter does not exist or empty, default name "flashair" will be used.
		/// </remarks>
		[PersistentMember(true)]
		public string APPNAME
		{
			get => _APPNAME;
			set
			{
				_APPNAME = GetNullOrLimited(value, 15);
				RaisePropertyChanged(nameof(IsChanged));
			}
		}
		private string _APPNAME;

		/// <summary>
		/// SSID
		/// </summary>
		/// <remarks>
		/// Format: 1-32 characters
		/// For AP mode and Internet pass-thru mode, SSID of built-in Access Point of FlashAir.
		/// For STA mode, SSID of router Access Point.
		/// If this parameter is not set, default string "flashair_" will be used.
		/// </remarks>
		[PersistentMember(true)]
		public string APPSSID
		{
			get => _APPSSID;
			set
			{
				_APPSSID = GetNullOrLimited(value, 32);
				RaisePropertyChanged(nameof(IsChanged));
			}
		}
		private string _APPSSID;

		/// <summary>
		/// Network security key corresponding to APPSSID
		/// </summary>
		/// <remarks>
		/// Format: 0-64 characters (at least 8 characters are required to enable security functionality)
		/// Once rebooted, this value will be saved in hidden area in FlashAir and this value will be masked.
		/// </remarks>
		[PersistentMember(true)]
		public string APPNETWORKKEY
		{
			get => _APPNETWORKKEY;
			set
			{
				_APPNETWORKKEY = GetNullOrLimited(value, 64);
				RaisePropertyChanged(nameof(IsChanged));
			}
		}
		private string _APPNETWORKKEY;

		/// <summary>
		/// SSID of router Access Point for Internet pass-thru mode
		/// </summary>
		/// <remarks>
		/// Format: 1-32 characters
		/// </remarks>
		[PersistentMember(true)]
		public string BRGSSID
		{
			get => _BRGSSID;
			set
			{
				_BRGSSID = GetNullOrLimited(value, 32);
				RaisePropertyChanged(nameof(IsChanged));
			}
		}
		private string _BRGSSID;

		/// <summary>
		/// Network security key corresponding to BRGSSID
		/// </summary>
		/// <remarks>
		/// Format: 0-64 characters
		/// </remarks>
		[PersistentMember(true)]
		public string BRGNETWORKKEY
		{
			get => _BRGNETWORKKEY;
			set
			{
				_BRGNETWORKKEY = GetNullOrLimited(value, 64);
				RaisePropertyChanged(nameof(IsChanged));
			}
		}
		private string _BRGNETWORKKEY;

		/// <summary>
		/// Automatic timeout period (msec) for Wireless LAN
		/// </summary>
		/// <remarks>
		/// Range: from 60000 to 4294967294 (UInt32.MaxValue - 1)
		/// 300000: Default
		/// 0:      Disable automatic timeout
		/// </remarks>
		[PersistentMember(true)]
		public uint APPAUTOTIME
		{
			get => _APPAUTOTIME;
			set
			{
				if ((value != 0) && ((value < 60_000) || (uint.MaxValue <= value)))
					return;

				_APPAUTOTIME = value;
				RaisePropertyChanged(nameof(IsChanged));
			}
		}
		private uint _APPAUTOTIME = 300_000; // Default

		/// <summary>
		/// DNS operation mode
		/// </summary>
		/// <remarks>
		/// 0: Returns the FlashAir's IP Address only if the DNS request is done with the APPNAME or "flashair".
		/// 1: Returns the FlashAir's IP Address to any DNS requests.
		/// </remarks>
		[PersistentMember]
		public int DNSMODE { get; set; } = 1; // Default

		/// <summary>
		/// Upload operation enabled flag
		/// </summary>
		/// <remarks>
		/// 1:     Upload operation enabled.
		/// Other: Upload operation disabled.
		/// If this parameter does not exist, upload operation will be regarded as disabled.
		/// </remarks>
		[PersistentMember(true)]
		public int UPLOAD
		{
			get => _UPLOAD;
			set
			{
				_UPLOAD = value;
				RaisePropertyChanged(nameof(IsChanged));
			}
		}
		private int _UPLOAD = 0; // Disabled

		/// <summary>
		/// Upload destination directory
		/// </summary>
		[PersistentMember]
		public string UPDIR { get; set; }

		/// <summary>
		/// Wireless LAN Boot Screen
		/// </summary>
		/// <remarks>
		/// Full-path of the image to use as the wireless boot screen.
		/// This image is used when Wireless LAN mode (APPMODE) is 0, 1, or 2.
		/// </remarks>
		[PersistentMember]
		public string CIPATH { get; set; } = "/DCIM/100__TSB/FA000001.JPG"; // Default

		/// <summary>
		/// Master Code to set SSID and network security key
		/// </summary>
		/// <remarks>
		/// Format: 12-digit hexadecimal number
		/// </remarks>
		[PersistentMember]
		public string MASTERCODE { get; set; }

		/// <summary>
		/// CID (Card Identification number register)
		/// </summary>
		/// <remarks>
		/// Format: 32-digit hexadecimal number
		/// </remarks>
		[PersistentMember]
		public string CID
		{
			get => _CID.Source;
			set => _CID.Import(value);
		}
		private readonly CidInfo _CID = new CidInfo();

		/// <summary>
		/// Product code
		/// </summary>
		/// <remarks>This is always "FlashAir".</remarks>
		[PersistentMember]
		public string PRODUCT { get; set; }

		/// <summary>
		/// Vendor code
		/// </summary>
		/// <remarks>This is always "TOSHIBA".</remarks>
		[PersistentMember]
		public string VENDOR { get; set; }

		/// <summary>
		/// Firmware version
		/// </summary>
		[PersistentMember]
		public string VERSION { get; set; }

		#endregion

		#region Content of CID parameter

		/// <summary>
		/// Manufacturer ID
		/// </summary>
		public int ManufacturerID => _CID.ManufacturerID;

		/// <summary>
		/// OEM/Application ID
		/// </summary>
		public string OemApplicationID => _CID.OemApplicationID;

		/// <summary>
		/// Product Name
		/// </summary>
		public string ProductName => _CID.ProductName;

		/// <summary>
		/// Product Revision
		/// </summary>
		public string ProductRevision => _CID.ProductRevision;

		/// <summary>
		/// Product Serial Number
		/// </summary>
		public uint ProductSerialNumber => _CID.ProductSerialNumber;

		/// <summary>
		/// Manufacturing Date
		/// </summary>
		public DateTime ManufacturingDate => _CID.ManufacturingDate;

		#endregion

		#region Modes of APPMODE parameter

		/// <summary>
		/// Set of corresponding modes for APPMODE parameter
		/// </summary>
		private class ModeSet
		{
			public int AppMode { get; }
			public LanModeOption LanMode { get; }
			public LanStartupModeOption LanStartupMode { get; }

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

		public LanModeOption LanMode
		{
			get => _lanMode;
			set
			{
				if (_lanMode == value)
					return;

				_lanMode = value;
				RaisePropertyChanged();

				var modeSet = _modeSetMap.SingleOrDefault(x => (x.LanMode == value) && (x.LanStartupMode == this.LanStartupMode));
				if (modeSet != null)
					APPMODE = modeSet.AppMode;
			}
		}
		private LanModeOption _lanMode;

		public LanStartupModeOption LanStartupMode
		{
			get => _lanStartupMode;
			set
			{
				if (_lanStartupMode == value)
					return;

				_lanStartupMode = value;
				RaisePropertyChanged();

				var modeSet = _modeSetMap.SingleOrDefault(x => (x.LanStartupMode == value) && (x.LanMode == this.LanMode));
				if (modeSet != null)
					APPMODE = modeSet.AppMode;
			}
		}
		private LanStartupModeOption _lanStartupMode;

		#endregion

		#region Supplementary

		/// <summary>
		/// Disk information of associated disk
		/// </summary>
		internal DiskInfo AssociatedDisk { get; private set; }

		public string DriveLetter => AssociatedDisk?.DriveLetter;

		public ulong TotalCapacity => AssociatedDisk?.Size ?? 0UL;
		public ulong FreeCapacity => AssociatedDisk?.FreeSpace ?? 0UL;
		public float UsedPercentage => (TotalCapacity > 0) ? ((TotalCapacity - FreeCapacity) * 100F / TotalCapacity) : 0F;

		/// <summary>
		/// Remaining parameters in CONFIG file
		/// </summary>
		/// <remarks>This is to hold unused parameters (LOCK, APPINFO).</remarks>
		private readonly Dictionary<string, string> _remaining = new Dictionary<string, string>();

		public bool IsChanged => !_isImporting && !GetMonitoredValues().SequenceEqual(_monitoredValues);

		public bool IsInternetPassThruReady =>
			VersionAddition.TryFind(VERSION, out Version version) && (version >= new Version(2, 0, 2)); // Equal to or newer than 2.00.02

		public bool IsUploadEnabled
		{
			get => (UPLOAD == 1);
			set
			{
				// If true, set 1. If false, set any number other than 1.
				UPLOAD = value ? 1 : 0;
				RaisePropertyChanged();
			}
		}

		#endregion

		#region Read/Write

		/// <summary>
		/// Reads CONFIG file in FlashAir card.
		/// </summary>
		/// <param name="disk">Disk information</param>
		/// <returns>True if successfully read.</returns>
		internal async Task<bool> ReadAsync(DiskInfo disk)
		{
			AssociatedDisk = disk ?? throw new ArgumentNullException(nameof(disk));

			var configPath = ComposeConfigPath();
			if (!File.Exists(configPath))
				return false;

			try
			{
				using (var sr = new StreamReader(configPath, Encoding.ASCII))
				{
					Import(await sr.ReadToEndAsync());
					return true;
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to read CONFIG file.\r\n{ex}");
				return false;
			}
		}

		/// <summary>
		/// Writes CONFIG file in FlashAir card.
		/// </summary>
		internal async Task WriteAsync()
		{
			try
			{
				var content = Export();

				var configPath = ComposeConfigPath();

				// Remove hidden attribute from CONFIG file.
				var fileInfo = new FileInfo(configPath);
				fileInfo.Attributes &= ~FileAttributes.Hidden;

				using (var sw = new StreamWriter(configPath, false, Encoding.ASCII))
				{
					await sw.WriteAsync(content);
				}

				// Add hidden attribute to CONFIG file.
				fileInfo.Attributes |= FileAttributes.Hidden;

				Import(content);
			}
			catch (Exception ex)
			{
				Debug.WriteLine($"Failed to write CONFIG file.\r\n{ex}");
				throw;
			}
		}

		private string ComposeConfigPath() =>
			(AssociatedDisk != null) ? Path.Combine(AssociatedDisk.DriveLetter, "SD_WLAN", "CONFIG") : null;

		#endregion

		#region Import/Export

		private static readonly PropertyInfo[] _persistentProperties = typeof(CardConfigViewModel)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(x => x.IsDefined(typeof(PersistentMemberAttribute)))
			.ToArray();

		private static readonly PropertyInfo[] _monitoredProperties = _persistentProperties
			.Where(x => ((PersistentMemberAttribute)x.GetCustomAttribute(typeof(PersistentMemberAttribute))).IsMonitored)
			.ToArray();

		private string[] _monitoredValues;

		private IEnumerable<string> GetMonitoredValues() => _monitoredProperties
			.Select(x => x.GetValue(this))
			.Select(x => x?.ToString() ?? string.Empty);

		private const char Separator = '=';
		private bool _isImporting;

		internal void Import(string configContent)
		{
			static Dictionary<string, string> Parse(string source, char separator)
			{
				return source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
					.Select(x => x.Split(new[] { separator }, 2))
					.Where(x => x.Length == 2)
					.Select(x => new { Key = x[0].Trim(), Value = x[1].Trim() })
					.Where(x => !string.IsNullOrEmpty(x.Key))
					.GroupBy(x => x.Key)
					.ToDictionary(x => x.Key, x => x.Last().Value);
			}

			var contents = Parse(configContent, Separator);

			try
			{
				_isImporting = true;

				_remaining.Clear();

				foreach (var c in contents)
				{
					var p = _persistentProperties.FirstOrDefault(x => c.Key == x.Name);
					if (p is null)
					{
						if (!_remaining.Keys.Contains(c.Key))
							_remaining.Add(c.Key, c.Value);

						continue;
					}

					try
					{
						p.SetValue(this, Convert.ChangeType(c.Value, p.PropertyType));

						Trace.WriteLine($"Imported {p.Name}: {p.GetValue(this)}");
					}
					catch (Exception ex)
					{
						throw new Exception($"Failed to import value ({p.Name}).", ex);
					}
				}

				_monitoredValues = GetMonitoredValues().ToArray();

				var modeSet = _modeSetMap.SingleOrDefault(x => x.AppMode == this.APPMODE);
				this.LanMode = modeSet?.LanMode ?? default;
				this.LanStartupMode = modeSet?.LanStartupMode ?? default;
			}
			finally
			{
				_isImporting = false;
			}
		}

		private string Export()
		{
			// Turn empty string value to null.
			foreach (var p in _persistentProperties)
			{
				if (!(p.GetValue(this) is string value))
					continue;

				if (string.IsNullOrWhiteSpace(value))
					p.SetValue(this, null);
			}

			// Conform empty string value or null of corresponding SSID and network security key.
			ConformNullOrEmpty(ref _APPSSID, ref _APPNETWORKKEY);
			ConformNullOrEmpty(ref _BRGSSID, ref _BRGNETWORKKEY);

			// Compose outcome.
			var outcome = new List<string>(_persistentProperties.Length);

			foreach (var p in _persistentProperties)
			{
				var value = p.GetValue(this);
				if (value is null)
					continue;

				outcome.Add($"{p.Name}{Separator}{value}");
			}

			if (_remaining.Any())
				outcome.AddRange(_remaining.Select(x => $"{x.Key}{Separator}{x.Value}"));

			outcome.Sort();

			outcome.Insert(0, "[Vendor]");
			outcome.Insert(1, string.Empty);

			return string.Join(Environment.NewLine, outcome);
		}

		#endregion

		#region Helper

		private static string GetNullOrLimited(string source, int maxLength)
		{
			if (maxLength < 0)
				throw new ArgumentOutOfRangeException(nameof(maxLength));

			if (string.IsNullOrWhiteSpace(source))
				return null;

			return (source.Length <= maxLength)
				? source
				: source.Substring(0, maxLength);
		}

		private static void ConformNullOrEmpty(ref string a, ref string b)
		{
			if (string.IsNullOrEmpty(a) && string.IsNullOrEmpty(b))
			{
				a = null;
				b = null;
			}
			else
			{
				a ??= string.Empty;
				b ??= string.Empty;
			}
		}

		#endregion
	}
}
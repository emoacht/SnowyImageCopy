using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SnowyTool.Helper;
using SnowyTool.Models;

namespace SnowyTool.ViewModels
{
	public class ConfigViewModel : ViewModel
	{
		#region Type

		[AttributeUsage(AttributeTargets.Property)]
		private class PersistentMemberAttribute : Attribute
		{
			public bool IsMonitored { get; private set; }

			public PersistentMemberAttribute(bool isMonitored = false)
			{
				this.IsMonitored = isMonitored;
			}
		}

		#endregion

		#region Property (Raw)

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
			get { return _APPMODE; }
			set
			{
				_APPMODE = value;
				RaisePropertyChanged(() => IsChanged);
				RaisePropertyChanged(() => IsInternetPassThruEnabled);
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
			get { return _APPNAME; }
			set
			{
				_APPNAME = GetNullOrLimited(value, 15);
				RaisePropertyChanged(() => IsChanged);
			}
		}
		private string _APPNAME;

		/// <summary>
		/// SSID
		/// </summary>
		/// <remarks>
		/// Format: 1-32 characters
		/// For AP mode and Internet pass-thru mode, SSID of built-in Access Point of FlashAir.
		/// For STA mode, SSID of external Access Point.
		/// If this parameter is not set, default string "flashair_" will be used.
		/// </remarks>
		[PersistentMember(true)]
		public string APPSSID
		{
			get { return _APPSSID; }
			set
			{
				_APPSSID = GetNullOrLimited(value, 32);
				RaisePropertyChanged(() => IsChanged);
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
			get { return _APPNETWORKKEY; }
			set
			{
				_APPNETWORKKEY = GetNullOrLimited(value, 64);
				RaisePropertyChanged(() => IsChanged);
			}
		}
		private string _APPNETWORKKEY;

		/// <summary>
		/// SSID of external Access Point for Internet pass-thru mode
		/// </summary>
		/// <remarks>
		/// Format: 1-32 characters
		/// </remarks>
		[PersistentMember(true)]
		public string BRGSSID
		{
			get { return _BRGSSID; }
			set
			{
				_BRGSSID = GetNullOrLimited(value, 32);
				RaisePropertyChanged(() => IsChanged);
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
			get { return _BRGNETWORKKEY; }
			set
			{
				_BRGNETWORKKEY = GetNullOrLimited(value, 64);
				RaisePropertyChanged(() => IsChanged);
			}
		}
		private string _BRGNETWORKKEY;

		/// <summary>
		/// Automatic timeout period (msec) for Wireless LAN
		/// </summary>
		/// <remarks>
		/// Range: from 60000 to 4294967294
		/// 300000: Default
		/// 0:      Disable automatic timeout
		/// </remarks>
		[PersistentMember]
		public uint APPAUTOTIME
		{
			get { return _APPAUTOTIME; }
			set
			{
				if ((value != 0) && ((value < 60000) || (4294967294 < value)))
					return;

				_APPAUTOTIME = value;
			}
		}
		private uint _APPAUTOTIME = 300000; // Default

		/// <summary>
		/// DNS operation mode
		/// </summary>
		/// <remarks>
		/// 0: Returns the FlashAir's IP Address only if the DNS request is done with the APPNAME or "flashair".
		/// 1: Returns the FlashAir's IP Address to any DNS requests.
		/// </remarks>
		[PersistentMember]
		public int DNSMODE
		{
			get { return _DNSMODE; }
			set { _DNSMODE = value; }
		}
		private int _DNSMODE = 1; // Default

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
			get { return _UPLOAD; }
			set
			{
				_UPLOAD = value;
				RaisePropertyChanged(() => IsChanged);
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
		public string CIPATH
		{
			get { return _CIPATH; }
			set { _CIPATH = value; }
		}
		private string _CIPATH = "/DCIM/100__TSB/FA000001.JPG"; // Default

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
			get { return _CID.Source; }
			set { _CID.Import(value); }
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

		#region Property (Content of CID)

		/// <summary>
		/// Manufacturer ID
		/// </summary>
		public int ManufacturerID { get { return _CID.ManufacturerID; } }

		/// <summary>
		/// OEM/Application ID
		/// </summary>
		public string OemApplicationID { get { return _CID.OemApplicationID; } }

		/// <summary>
		/// Product Name
		/// </summary>
		public string ProductName { get { return _CID.ProductName; } }

		/// <summary>
		/// Product Revision
		/// </summary>
		public string ProductRevision { get { return _CID.ProductRevision; } }

		/// <summary>
		/// Product Serial Number
		/// </summary>
		public uint ProductSerialNumber { get { return _CID.ProductSerialNumber; } }

		/// <summary>
		/// Manufacturing Date
		/// </summary>
		public DateTime ManufacturingDate { get { return _CID.ManufacturingDate; } }

		#endregion

		#region Supplementary

		/// <summary>
		/// Disk information of associated disk
		/// </summary>
		public DiskInfo AssociatedDisk { get; private set; }

		/// <summary>
		/// Remaining parameters in the config file
		/// </summary>
		/// <remarks>This is to hold unusable parameters (LOCK, APPINFO) and unknown ones.</remarks>
		private readonly Dictionary<string, string> _remaining = new Dictionary<string, string>();

		public bool IsChanged
		{
			get { return !_isImporting && !GetMonitoredValues().SequenceEqual(_monitoredValues); }
		}

		private static readonly Regex _versionPattern = new Regex(@"[1-9]\.\d{2}\.\d{2}$", RegexOptions.Compiled); // Pattern for firmware version (number part)

		public bool IsInternetPassThruReady
		{
			get
			{
				var match = _versionPattern.Match(VERSION);
				if (!match.Success)
					return false;

				return (new Version(match.Value) >= new Version(2, 0, 2)); // Equal to or newer than 2.00.02
			}
		}

		public bool IsInternetPassThruEnabled
		{
			get { return IsInternetPassThruReady && ((APPMODE == 3) || (APPMODE == 6)); }
		}

		#endregion

		#region Read/Write

		/// <summary>
		/// Reads Config file.
		/// </summary>
		/// <remarks>True if completed</remarks>
		internal async Task<bool> ReadAsync(DiskInfo info)
		{
			if (info == null)
				throw new ArgumentNullException("info");

			AssociatedDisk = info;

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
				Debug.WriteLine("Failed to read config file. {0}", ex);
				return false;
			}
		}

		/// <summary>
		/// Writes Config file.
		/// </summary>
		internal async Task WriteAsync()
		{
			try
			{
				var content = Export();

				var configPath = ComposeConfigPath();

				// Remove hidden attribute from the config file.
				var fileInfo = new FileInfo(configPath);
				fileInfo.Attributes &= ~FileAttributes.Hidden;

				using (var sw = new StreamWriter(configPath, false, Encoding.ASCII))
				{
					await sw.WriteAsync(content);
				}

				// Add hidden attribute to the config file.
				fileInfo.Attributes |= FileAttributes.Hidden;

				Import(content);
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to write config file. {0}", ex);
				throw;
			}
		}

		private string ComposeConfigPath()
		{
			return (AssociatedDisk != null)
				? Path.Combine(AssociatedDisk.DriveLetters.First(), "SD_WLAN", "CONFIG")
				: null;
		}

		#endregion

		#region Import/Export

		private static readonly PropertyInfo[] _persistentProperties = typeof(ConfigViewModel)
			.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
			.Where(x => x.IsDefined(typeof(PersistentMemberAttribute)))
			.ToArray();

		private static readonly PropertyInfo[] _monitoredProperties = _persistentProperties
			.Where(x => ((PersistentMemberAttribute)x.GetCustomAttribute(typeof(PersistentMemberAttribute))).IsMonitored)
			.ToArray();

		private string[] _monitoredValues;

		private IEnumerable<string> GetMonitoredValues()
		{
			return _monitoredProperties
				.Select(x => x.GetValue(this))
				.Select(x => (x != null) ? x.ToString() : String.Empty);
		}

		private const char _separator = '=';
		private bool _isImporting;

		internal void Import(string configContent)
		{
			var contents = StringDictionary.Parse(configContent, _separator);

			_isImporting = true;

			try
			{
				_remaining.Clear();

				foreach (var c in contents)
				{
					var p = _persistentProperties.FirstOrDefault(x => c.Key == x.Name);
					if (p == null)
					{
						if (!_remaining.Keys.Contains(c.Key))
							_remaining.Add(c.Key, c.Value);

						continue;
					}

					try
					{
						p.SetValue(this, Convert.ChangeType(c.Value, p.PropertyType));

						Debug.WriteLine("Imported {0}: {1}", p.Name, p.GetValue(this));
					}
					catch (Exception ex)
					{
						throw new Exception(String.Format("Failed to import value ({0}). ", p.Name), ex);
					}
				}

				_monitoredValues = GetMonitoredValues().ToArray();

				RemoveTemporaryMembers(_remaining); // Remove temporary members added by bug.
			}
			finally
			{
				_isImporting = false;
			}
		}

		internal string Export()
		{
			// Turn empty string value to null.
			foreach (var p in _persistentProperties)
			{
				var value = p.GetValue(this) as string;
				if (value == null)
					continue;

				if (String.IsNullOrWhiteSpace(value))
					p.SetValue(this, null);
			}

			// Conform empty string value or null of corresponding SSID and network security key.
			ConformNullOrEmpty(ref _APPSSID, ref _APPNETWORKKEY);
			ConformNullOrEmpty(ref _BRGSSID, ref _BRGNETWORKKEY);

			// Compose outcome.
			var outcome = new List<string>();

			foreach (var p in _persistentProperties)
			{
				var value = p.GetValue(this);
				if (value == null)
					continue;

				outcome.Add(String.Format("{0}{1}{2}", p.Name, _separator, value));
			}

			if (_remaining.Any())
				outcome.AddRange(_remaining.Select(x => String.Format("{0}{1}{2}", x.Key, _separator, x.Value)));

			outcome.Sort();

			outcome.Insert(0, "[Vendor]");
			outcome.Insert(1, String.Empty);

			return String.Join(Environment.NewLine, outcome);
		}

		private static readonly string[] _temporaryMemberNames =
		{
			"ManufacturerID",
			"OemApplicationID",
			"ProductName",
			"ProductRevision",
			"ProductSerialNumber",
			"ManufacturingDate",
			"AssociatedDisk"
		};

		private static void RemoveTemporaryMembers(Dictionary<string, string> source)
		{
			var sourceNames = source.Select(x => x.Key).ToArray();

			if (!_temporaryMemberNames.All(x => sourceNames.Contains(x)))
				return;

			foreach (var name in _temporaryMemberNames)
				source.Remove(name);
		}

		#endregion

		#region Helper

		private static string GetNullOrLimited(string source, int maxLength)
		{
			if (maxLength < 0)
				throw new ArgumentOutOfRangeException("maxLength");

			if (String.IsNullOrWhiteSpace(source))
				return null;

			return (source.Length <= maxLength)
				? source
				: source.Substring(0, maxLength);
		}

		private static void ConformNullOrEmpty(ref string a, ref string b)
		{
			if (String.IsNullOrEmpty(a) && String.IsNullOrEmpty(b))
			{
				a = null;
				b = null;
			}
			else
			{
				if (a == null)
					a = String.Empty;

				if (b == null)
					b = String.Empty;
			}
		}

		#endregion
	}
}
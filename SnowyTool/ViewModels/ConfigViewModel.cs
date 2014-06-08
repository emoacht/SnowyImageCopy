using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using SnowyTool.Helper;
using SnowyTool.Models;

namespace SnowyTool.ViewModels
{
	public class ConfigViewModel : ViewModel
	{
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
		/// Other: Undefined behaviour.
		/// </remarks>
		public int APPMODE
		{
			get { return _APPMODE; }
			set
			{
				_APPMODE = value;

				if (isImporting)
					APPMODE_IMPORT = _APPMODE;
				else
					RaisePropertyChanged(() => IsChanged);
			}
		}
		private int _APPMODE = 4; // Default
		private int APPMODE_IMPORT = 4;

		/// <summary>
		/// NETBIOS/Bonjour name (Address of FlashAir)
		/// </summary>
		/// <remarks>
		/// Format: 15 characters
		/// If this parameter does not exist or empty, default name "flashair" will be used.
		/// </remarks>
		public string APPNAME
		{
			get { return _APPNAME; }
			set { _APPNAME = (value.Length <= 15) ? value : value.Substring(0, 15); }
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
		public string APPSSID
		{
			get { return _APPSSID; }
			set
			{
				_APPSSID = (value.Length <= 32) ? value : value.Substring(0, 32);

				if (isImporting)
					APPSSID_IMPORT = _APPSSID;
				else
					RaisePropertyChanged(() => IsChanged);
			}
		}
		private string _APPSSID;
		private string APPSSID_IMPORT;

		/// <summary>
		/// Network security key corresponding to APPSSID
		/// </summary> 
		/// <remarks>
		/// Format: 0-64 characters (at least 8 characters are required to enable security functionality) 
		/// Once rebooted, this value will be saved in hidden area and this string will be masked.
		/// </remarks>
		public string APPNETWORKKEY
		{
			get { return _APPNETWORKKEY; }
			set
			{
				_APPNETWORKKEY = (value.Length <= 64) ? value : value.Substring(0, 64);

				if (isImporting)
					APPNETWORKKEY_IMPORT = _APPNETWORKKEY;
				else
					RaisePropertyChanged(() => IsChanged);
			}
		}
		private string _APPNETWORKKEY;
		private string APPNETWORKKEY_IMPORT;

		/// <summary>
		/// SSID of external Access Point for Internet pass-thru mode
		/// </summary>
		/// <remarks>
		/// Format: 1-32 characters
		/// </remarks>
		public string BRGSSID
		{
			get { return _BRGSSID; }
			set
			{
				_BRGSSID = (value.Length <= 32) ? value : value.Substring(0, 32);

				if (isImporting)
					BRGSSID_IMPORT = _BRGSSID;
				else
					RaisePropertyChanged(() => IsChanged);
			}
		}
		private string _BRGSSID;
		private string BRGSSID_IMPORT;

		/// <summary>
		/// Network security key corresponding to BRGSSID
		/// </summary>
		/// <remarks>
		/// Format: 0-64 characters
		/// </remarks>
		public string BRGNETWORKKEY
		{
			get { return _BRGNETWORKKEY; }
			set
			{
				_BRGNETWORKKEY = (value.Length <= 64) ? value : value.Substring(0, 64);

				if (isImporting)
					BRGNETWORKKEY_IMPORT = _BRGNETWORKKEY;
				else
					RaisePropertyChanged(() => IsChanged);
			}
		}
		private string _BRGNETWORKKEY;
		private string BRGNETWORKKEY_IMPORT;

		/// <summary>
		/// Automatic timeout period (msec) for Wireless LAN
		/// </summary>
		/// <remarks>
		/// Range: from 60000 to 4294967294
		/// 300000: Default
		/// 0: Disable automatic timeout
		/// </remarks>
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
		/// Redirect function flag
		/// </summary>
		/// <remarks>
		/// 0: Off
		/// 1: On 
		/// </remarks>
		public int DNSMODE
		{
			get { return _DNSMODE; }
			set { _DNSMODE = value; }
		}
		private int _DNSMODE = 0;

		/// <summary>
		/// Upload operation enabled flag
		/// </summary>
		/// <remarks>
		/// 1: Upload operation enabled
		/// Other: Upload operation disabled
		/// </remarks>
		public int UPLOAD
		{
			get { return _UPLOAD; }
			set { _UPLOAD = value; }
		}
		private int _UPLOAD = 1;

		/// <summary>
		/// Wireless LAN Boot Screen
		/// </summary>
		/// <remarks>
		/// Full-path of the image to use as the wireless boot screen.
		/// This image is used when Wireless LAN mode (APPMODE) is 0, 1, or 2.
		/// </remarks>
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
		public string MASTERCODE { get; set; }

		/// <summary>
		/// Application's unique information
		/// </summary>
		public string APPINFO { get; set; }

		/// <summary>
		/// CID (Card Identification number register)
		/// </summary>
		/// <remarks>
		/// Format: 32-digit hexadecimal number
		/// </remarks>
		public string CID
		{
			get { return _CID; }
			set
			{
				_CID = value;
				ParseCID(value);
			}
		}
		private string _CID;

		/// <summary>
		/// Product code
		/// </summary>
		/// <remarks>This is always "FlashAir".</remarks>
		public string PRODUCT { get; set; }

		/// <summary>
		/// Vendor code
		/// </summary>
		/// <remarks>This is always "TOSHIBA".</remarks>
		public string VENDOR { get; set; }

		/// <summary>
		/// Firmware version
		/// </summary>
		public string VERSION { get; set; }
		
		#endregion


		#region Property (Content of CID)

		/// <summary>
		/// Manufacturer ID (MID)
		/// </summary> 
		/// <remarks>
		/// 8 bits [127:120] -> bytes 0
		/// 8-bit binary number
		/// 1: Panasonic
		/// 2: Toshiba
		/// 3: SanDisk
		/// </remarks>
		public int ManufacturerID
		{
			get { return _manufacturerID; }
		}
		public int _manufacturerID;
	
		/// <summary>
		/// OEM/Application ID (OID)
		/// </summary>
		/// <remarks>
		/// 16 bits [119:104] -> bytes 1-2 
		/// 2-character ASCII string
		/// </remarks>
		public string OemApplicationID
		{
			get { return _oemApplicationID; }
		}
		private string _oemApplicationID;

		/// <summary>
		/// Product Name (PNM)
		/// </summary>
		/// <remarks>
		/// 40 bits [103:64] -> bytes 3-7
		/// 5-character ASCII string
		/// </remarks>
		public string ProductName
		{
			get { return _productName; }
		}
		private string _productName;

		/// <summary>
		/// Product Revision (PRV)
		/// </summary>
		/// <remarks>
		/// 8 bits [63:56] -> bytes 8
		/// "n.m" revision number: First 4 bits [63:60] for major revision ("n") and second 4 bits [59:56] for minor revision ("m")
		/// </remarks>
		public string ProductRevision
		{
			get { return _productRevision; }
		}
		private string _productRevision;

		/// <summary>
		/// Product Serial Number (PSN)
		/// </summary>
		/// <remarks>
		/// 32 bits [55:24] -> bytes 9-12
		/// 32-bit binary number
		/// </remarks>
		public uint ProductSerialNumber
		{
			get { return _productSerialNumber; }
		}
		private uint _productSerialNumber;

		/// <summary>
		/// Manufacturing Date (MDT)
		/// </summary>
		/// <remarks>
		/// 12 bits [19:8] -> bytes 13 (second half) and 14
		/// 8 bits [19:12] at the head for the year (0 means 2000) and 4 bits [11:8] at the tail for the month
		/// </remarks>
		public DateTime ManufacturingDate
		{
			get { return _manufacturingDate; }
		}
		private DateTime _manufacturingDate;

		// CID register has 128 bits = 16 bytes in total.
		// 4 bits [23:20] are reserved.
		// 7 bits [7:1] are for CRC7 checksum.
		// 1 bit [0:0] is not used and always 1.

		#endregion


		#region Supplement

		public DiskInfo AssociatedDisk
		{
			get { return _associatedDisk; }
		}
		private DiskInfo _associatedDisk;

		public string ConfigPath
		{
			get { return (AssociatedDisk != null) ? Path.Combine(AssociatedDisk.DriveLetters.First(), "SD_WLAN", "CONFIG") : null; }
		}

		/// <summary>
		/// Remaining items in config file
		/// </summary>
		/// <remarks>This is to hold unusable items (LOCK) and other unknown items.</remarks>
		private Dictionary<string, string> Remaining = new Dictionary<string, string>();
		
		private readonly Regex patternVersion = new Regex(@"[1-9]\.\d{2}\.\d{2}", RegexOptions.Compiled);

		public bool IsReadyInternetPassThru
		{
			get
			{
				var matchVersion = patternVersion.Match(VERSION);
				if (!matchVersion.Success)
					return false;

				var version = new Version(matchVersion.Value);
				if (version.Major < 2)
					return false;

				return true;
			}
		}

		public bool IsChanged
		{
			get
			{
				return ((APPMODE_IMPORT != APPMODE) ||
					(APPSSID_IMPORT != APPSSID) || (APPNETWORKKEY_IMPORT != APPNETWORKKEY) ||
					(BRGSSID_IMPORT != BRGSSID) || (BRGNETWORKKEY_IMPORT != BRGNETWORKKEY));
			}
		}

		#endregion


		#region Constructor

		public ConfigViewModel(DiskInfo info)
		{
			_associatedDisk = info;
		}

		#endregion


		#region Import/Export

		private bool isImporting = false;
		private const Char separator = '=';
		
		internal void Import(string configContent)
		{
			var contents = TextParse.GetContent(configContent, separator);

			var properties = typeof(ConfigViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

			isImporting = true;

			try
			{
				foreach (var c in contents)
				{
					bool isFound = false;

					foreach (var p in properties)
					{
						if ((c.Key != p.Name) || string.IsNullOrEmpty(c.Value) || !p.CanWrite) // Not CanWrite means ReadOnly property.
							continue;

						try
						{
							p.SetValue(this, Convert.ChangeType(c.Value, p.PropertyType), null);
						}
						catch (Exception ex)
						{
							throw new Exception(String.Format("Failed to import value ({0}). ", p.Name), ex);
						}

						Debug.WriteLine("{0}: {1}", p.Name, p.GetValue(this, null));

						isFound = true;
						break;
					}

					if (!isFound && !Remaining.Keys.Contains(c.Key))
						Remaining.Add(c.Key, c.Value);
				}
			}
			finally
			{
				isImporting = false;
				RaisePropertyChanged(() => IsChanged);
			}
		}

		internal string Export()
		{
			var outcome = new List<string>();

			var properties = typeof(ConfigViewModel).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly);

			foreach (var p in properties)
			{
				if (!p.CanWrite) // Not CanWrite means ReadOnly property.
					continue;

				var value = p.GetValue(this, null);

				if ((value == null) || ((p.GetType() == typeof(String)) && String.IsNullOrWhiteSpace(value.ToString())))
					continue;

				outcome.Add(String.Format("{0}{1}{2}", p.Name, separator, value));
			}

			if (Remaining.Any())
				outcome.AddRange(Remaining.Select(x => String.Format("{0}{1}{2}", x.Key, separator, x.Value)));

			outcome.Sort();

			outcome.Insert(0, "[Vendor]");
			outcome.Insert(1, String.Empty);

			return String.Join(Environment.NewLine, outcome);
		}
		
		private readonly Regex patternAscii = new Regex("^[\x20-\x7F]{32}$", RegexOptions.Compiled); // Pattern for string in ASCII code (alphanumeric symbols)

		private void ParseCID(string cid)
		{
			if (!patternAscii.IsMatch(cid))
				return;

			var bytes = SoapHexBinary.Parse(cid).Value;

			_manufacturerID = bytes[0]; // Bytes 0
			_oemApplicationID = Encoding.ASCII.GetString(bytes.Skip(1).Take(2).ToArray()); // Bytes 1-2
			_productName = Encoding.ASCII.GetString(bytes.Skip(3).Take(5).ToArray()); // Bytes 3-7

			var productRevisionBits = new BitArray(new Byte[] { bytes[8] }).Cast<bool>().Reverse().ToArray(); // Bytes 8
			var major = ConvertFromBitsToInt(productRevisionBits.Take(4).Reverse());
			var minor = ConvertFromBitsToInt(productRevisionBits.Skip(4).Take(4).Reverse());
			_productRevision = String.Format("{0}.{1}", major, minor);

			_productSerialNumber = BitConverter.ToUInt32(bytes, 9); // Bytes 9-12

			var manufacturingDateBits = bytes.Skip(13).Take(2) // Bytes 13-14
				.SelectMany(x => new BitArray(new Byte[] { x }).Cast<bool>().Reverse())
				.Skip(4) // Skip reserved field.
				.ToArray();

			var year = ConvertFromBitsToInt(manufacturingDateBits.Take(8).Reverse());
			var month = ConvertFromBitsToInt(manufacturingDateBits.Skip(8).Take(4).Reverse());
			if ((year <= 1000) && (month <= 12))
			{
				_manufacturingDate = new DateTime(year + 2000, month, 1);
			}
		}

		private int ConvertFromBitsToInt(IEnumerable<bool> source)
		{
			var target = new int[1];
			new BitArray(source.ToArray()).CopyTo(target, 0);
			return target[0];
		}

		#endregion


		#region Read/Apply

		/// <summary>
		/// Read Config file.
		/// </summary>
		internal async Task<bool> ReadAsync()
		{
			if ((AssociatedDisk == null) || !AssociatedDisk.CanBeSD)
				return false;

			if (!File.Exists(ConfigPath))
				return false;

			try
			{
				using (var sr = new StreamReader(ConfigPath, Encoding.ASCII))
				{
					Import(await sr.ReadToEndAsync());
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Failed to read config file. {0}", ex);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Write Config file.
		/// </summary>
		internal async Task WriteAsync()
		{
			try
			{
				var content = Export();

				// Remove hidden attribute from the config file.
				var fileInfo = new FileInfo(ConfigPath);
				fileInfo.Attributes &= ~FileAttributes.Hidden;

				using (var sw = new StreamWriter(ConfigPath, false, Encoding.ASCII))
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

		#endregion
	}
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// A managed implementation of Native Wifi API
	/// </summary>
	public class NativeWifi
	{
		#region Win32

		[DllImport("Wlanapi.dll")]
		private static extern uint WlanOpenHandle(
			uint dwClientVersion,
			IntPtr pReserved,
			out uint pdwNegotiatedVersion,
			out IntPtr phClientHandle);

		[DllImport("Wlanapi.dll")]
		private static extern uint WlanCloseHandle(
			IntPtr hClientHandle,
			IntPtr pReserved);

		[DllImport("Wlanapi.dll")]
		private static extern void WlanFreeMemory(IntPtr pMemory);

		[DllImport("Wlanapi.dll")]
		private static extern uint WlanEnumInterfaces(
			IntPtr hClientHandle,
			IntPtr pReserved,
			out IntPtr ppInterfaceList);

		[DllImport("Wlanapi.dll")]
		private static extern uint WlanQueryInterface(
			IntPtr hClientHandle,
			[MarshalAs(UnmanagedType.LPStruct)] Guid pInterfaceGuid,
			WLAN_INTF_OPCODE OpCode,
			IntPtr pReserved,
			out uint pdwDataSize,
			ref IntPtr ppData,
			IntPtr pWlanOpcodeValueType);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_INTERFACE_INFO
		{
			public Guid InterfaceGuid;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strInterfaceDescription;

			public WLAN_INTERFACE_STATE isState;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WLAN_INTERFACE_INFO_LIST
		{
			public uint dwNumberOfItems;
			public uint dwIndex;
			public WLAN_INTERFACE_INFO[] InterfaceInfo;

			public WLAN_INTERFACE_INFO_LIST(IntPtr ppInterfaceList)
			{
				dwNumberOfItems = (uint)Marshal.ReadInt32(ppInterfaceList, 0);
				dwIndex = (uint)Marshal.ReadInt32(ppInterfaceList, 4 /* Offset for dwNumberOfItems */);
				InterfaceInfo = new WLAN_INTERFACE_INFO[dwNumberOfItems];

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var interfaceInfo = new IntPtr(ppInterfaceList.ToInt64()
						+ 8 /* Offset for dwNumberOfItems and dwIndex */
						+ (Marshal.SizeOf(typeof(WLAN_INTERFACE_INFO)) * i) /* Offset for preceding items */);

					InterfaceInfo[i] = Marshal.PtrToStructure<WLAN_INTERFACE_INFO>(interfaceInfo);
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DOT11_SSID
		{
			public uint uSSIDLength;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
			public byte[] ucSSID;

			public byte[] ToBytes()
			{
				return (ucSSID != null)
					? ucSSID.Take((int)uSSIDLength).ToArray()
					: null;
			}

			private static Encoding _encoding =
				Encoding.GetEncoding(65001, // UTF-8 code page
					EncoderFallback.ReplacementFallback,
					DecoderFallback.ExceptionFallback);

			public override string ToString()
			{
				if (ucSSID == null)
					return null;

				try
				{
					return _encoding.GetString(ToBytes());
				}
				catch (DecoderFallbackException)
				{
					return null;
				}
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DOT11_MAC_ADDRESS
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] ucDot11MacAddress;

			public byte[] ToBytes()
			{
				return (ucDot11MacAddress != null)
					? ucDot11MacAddress.ToArray()
					: null;
			}

			public override string ToString()
			{
				return (ucDot11MacAddress != null)
					? BitConverter.ToString(ucDot11MacAddress).Replace('-', ':')
					: null;
			}
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_CONNECTION_ATTRIBUTES
		{
			public WLAN_INTERFACE_STATE isState;
			public WLAN_CONNECTION_MODE wlanConnectionMode;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strProfileName;

			public WLAN_ASSOCIATION_ATTRIBUTES wlanAssociationAttributes;
			public WLAN_SECURITY_ATTRIBUTES wlanSecurityAttributes;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WLAN_ASSOCIATION_ATTRIBUTES
		{
			public DOT11_SSID dot11Ssid;
			public DOT11_BSS_TYPE dot11BssType;
			public DOT11_MAC_ADDRESS dot11Bssid;
			public DOT11_PHY_TYPE dot11PhyType;
			public uint uDot11PhyIndex;
			public uint wlanSignalQuality;
			public uint ulRxRate;
			public uint ulTxRate;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct WLAN_SECURITY_ATTRIBUTES
		{
			[MarshalAs(UnmanagedType.Bool)]
			public bool bSecurityEnabled;

			[MarshalAs(UnmanagedType.Bool)]
			public bool bOneXEnabled;

			public DOT11_AUTH_ALGORITHM dot11AuthAlgorithm;
			public DOT11_CIPHER_ALGORITHM dot11CipherAlgorithm;
		}

		private enum WLAN_INTERFACE_STATE
		{
			wlan_interface_state_not_ready = 0,
			wlan_interface_state_connected = 1,
			wlan_interface_state_ad_hoc_network_formed = 2,
			wlan_interface_state_disconnecting = 3,
			wlan_interface_state_disconnected = 4,
			wlan_interface_state_associating = 5,
			wlan_interface_state_discovering = 6,
			wlan_interface_state_authenticating = 7
		}

		private enum WLAN_CONNECTION_MODE
		{
			wlan_connection_mode_profile,
			wlan_connection_mode_temporary_profile,
			wlan_connection_mode_discovery_secure,
			wlan_connection_mode_discovery_unsecure,
			wlan_connection_mode_auto,
			wlan_connection_mode_invalid
		}

		private enum DOT11_BSS_TYPE
		{
			/// <summary>
			/// Infrastructure BSS network
			/// </summary>
			dot11_BSS_type_infrastructure = 1,

			/// <summary>
			/// Independent BSS (IBSS) network
			/// </summary>
			dot11_BSS_type_independent = 2,

			/// <summary>
			/// Either infrastructure or IBSS network
			/// </summary>
			dot11_BSS_type_any = 3,
		}

		private enum DOT11_PHY_TYPE : uint
		{
			dot11_phy_type_unknown = 0,
			dot11_phy_type_any = 0,
			dot11_phy_type_fhss = 1,
			dot11_phy_type_dsss = 2,
			dot11_phy_type_irbaseband = 3,
			dot11_phy_type_ofdm = 4,
			dot11_phy_type_hrdsss = 5,
			dot11_phy_type_erp = 6,
			dot11_phy_type_ht = 7,
			dot11_phy_type_vht = 8,
			dot11_phy_type_IHV_start = 0x80000000,
			dot11_phy_type_IHV_end = 0xffffffff
		}

		private enum DOT11_AUTH_ALGORITHM : uint
		{
			DOT11_AUTH_ALGO_80211_OPEN = 1,
			DOT11_AUTH_ALGO_80211_SHARED_KEY = 2,
			DOT11_AUTH_ALGO_WPA = 3,
			DOT11_AUTH_ALGO_WPA_PSK = 4,
			DOT11_AUTH_ALGO_WPA_NONE = 5,
			DOT11_AUTH_ALGO_RSNA = 6,
			DOT11_AUTH_ALGO_RSNA_PSK = 7,
			DOT11_AUTH_ALGO_IHV_START = 0x80000000,
			DOT11_AUTH_ALGO_IHV_END = 0xffffffff
		}

		private enum DOT11_CIPHER_ALGORITHM : uint
		{
			DOT11_CIPHER_ALGO_NONE = 0x00,
			DOT11_CIPHER_ALGO_WEP40 = 0x01,
			DOT11_CIPHER_ALGO_TKIP = 0x02,
			DOT11_CIPHER_ALGO_CCMP = 0x04,
			DOT11_CIPHER_ALGO_WEP104 = 0x05,
			DOT11_CIPHER_ALGO_WPA_USE_GROUP = 0x100,
			DOT11_CIPHER_ALGO_RSN_USE_GROUP = 0x100,
			DOT11_CIPHER_ALGO_WEP = 0x101,
			DOT11_CIPHER_ALGO_IHV_START = 0x80000000,
			DOT11_CIPHER_ALGO_IHV_END = 0xffffffff
		}

		private enum WLAN_INTF_OPCODE : uint
		{
			wlan_intf_opcode_autoconf_start = 0x000000000,
			wlan_intf_opcode_autoconf_enabled,
			wlan_intf_opcode_background_scan_enabled,
			wlan_intf_opcode_media_streaming_mode,
			wlan_intf_opcode_radio_state,
			wlan_intf_opcode_bss_type,
			wlan_intf_opcode_interface_state,
			wlan_intf_opcode_current_connection,
			wlan_intf_opcode_channel_number,
			wlan_intf_opcode_supported_infrastructure_auth_cipher_pairs,
			wlan_intf_opcode_supported_adhoc_auth_cipher_pairs,
			wlan_intf_opcode_supported_country_or_region_string_list,
			wlan_intf_opcode_current_operation_mode,
			wlan_intf_opcode_supported_safe_mode,
			wlan_intf_opcode_certified_safe_mode,
			wlan_intf_opcode_hosted_network_capable,
			wlan_intf_opcode_management_frame_protection_capable,
			wlan_intf_opcode_autoconf_end = 0x0fffffff,
			wlan_intf_opcode_msm_start = 0x10000100,
			wlan_intf_opcode_statistics,
			wlan_intf_opcode_rssi,
			wlan_intf_opcode_msm_end = 0x1fffffff,
			wlan_intf_opcode_security_start = 0x20010000,
			wlan_intf_opcode_security_end = 0x2fffffff,
			wlan_intf_opcode_ihv_start = 0x30000000,
			wlan_intf_opcode_ihv_end = 0x3fffffff
		}

		private const uint ERROR_SUCCESS = 0;

		#endregion

		/// <summary>
		/// Enumerates SSIDs of connected wireless LANs.
		/// </summary>
		/// <returns>SSIDs</returns>
		public static IEnumerable<string> EnumerateConnectedNetworkSsids()
		{
			using (var client = new WlanClient())
			{
				var interfaceInfoList = GetInterfaceInfoList(client.Handle);

				foreach (var interfaceInfo in interfaceInfoList)
				{
					var connection = GetConnectionAttributes(client.Handle, interfaceInfo.InterfaceGuid);
					if (connection.isState != WLAN_INTERFACE_STATE.wlan_interface_state_connected)
						continue;

					var association = connection.wlanAssociationAttributes;
					if (string.IsNullOrEmpty(association.dot11Ssid.ToString()))
						continue;

					Debug.WriteLine("Interface: {0}, SSID: {1}, BSSID: {2}, Signal: {3}",
						interfaceInfo.strInterfaceDescription,
						association.dot11Ssid,
						association.dot11Bssid,
						association.wlanSignalQuality);

					yield return association.dot11Ssid.ToString();
				}
			}
		}

		#region Base

		private class WlanClient : IDisposable
		{
			private IntPtr _clientHandle = IntPtr.Zero;

			public IntPtr Handle { get { return _clientHandle; } }

			public WlanClient()
			{
				uint negotiatedVersion;
				var result = WlanOpenHandle(
					2, // Client version for Windows Vista and Windows Server 2008
					IntPtr.Zero,
					out negotiatedVersion,
					out _clientHandle);

				if (result != ERROR_SUCCESS)
					throw new Win32Exception((int)result);
			}

			#region Dispose

			private bool _disposed = false;

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				if (_disposed)
					return;

				if (_clientHandle != IntPtr.Zero)
					WlanCloseHandle(_clientHandle, IntPtr.Zero);

				_disposed = true;
			}

			~WlanClient()
			{
				Dispose(false);
			}

			#endregion
		}

		private static WLAN_INTERFACE_INFO[] GetInterfaceInfoList(IntPtr clientHandle)
		{
			var interfaceList = IntPtr.Zero;
			try
			{
				var result = WlanEnumInterfaces(
					clientHandle,
					IntPtr.Zero,
					out interfaceList);

				return (result == ERROR_SUCCESS)
					? new WLAN_INTERFACE_INFO_LIST(interfaceList).InterfaceInfo
					: new WLAN_INTERFACE_INFO[0];
			}
			finally
			{
				if (interfaceList != IntPtr.Zero)
					WlanFreeMemory(interfaceList);
			}
		}

		private static WLAN_CONNECTION_ATTRIBUTES GetConnectionAttributes(IntPtr clientHandle, Guid interfaceGuid)
		{
			var queryData = IntPtr.Zero;
			try
			{
				uint dataSize;
				var result = WlanQueryInterface(
					clientHandle,
					interfaceGuid,
					WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection,
					IntPtr.Zero,
					out dataSize,
					ref queryData,
					IntPtr.Zero);

				return (result == ERROR_SUCCESS)
					? Marshal.PtrToStructure<WLAN_CONNECTION_ATTRIBUTES>(queryData)
					: default(WLAN_CONNECTION_ATTRIBUTES);
			}
			finally
			{
				if (queryData != IntPtr.Zero)
					WlanFreeMemory(queryData);
			}
		}

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models.Network
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
			out IntPtr ppData,
			IntPtr pWlanOpcodeValueType);

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_INTERFACE_INFO
		{
			public Guid InterfaceGuid;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strInterfaceDescription;

			public WLAN_INTERFACE_STATE isState;
		}

		private struct WLAN_INTERFACE_INFO_LIST
		{
			public uint dwNumberOfItems;
			public uint dwIndex;
			public WLAN_INTERFACE_INFO[] InterfaceInfo;

			public WLAN_INTERFACE_INFO_LIST(IntPtr ppInterfaceList)
			{
				var uintSize = Marshal.SizeOf<uint>(); // 4

				dwNumberOfItems = (uint)Marshal.ReadInt32(ppInterfaceList, 0);
				dwIndex = (uint)Marshal.ReadInt32(ppInterfaceList, uintSize /* Offset for dwNumberOfItems */);
				InterfaceInfo = new WLAN_INTERFACE_INFO[dwNumberOfItems];

				for (int i = 0; i < dwNumberOfItems; i++)
				{
					var interfaceInfo = new IntPtr(ppInterfaceList.ToInt64()
						+ (uintSize * 2) /* Offset for dwNumberOfItems and dwIndex */
						+ (Marshal.SizeOf<WLAN_INTERFACE_INFO>() * i) /* Offset for preceding items */);

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

			public byte[] ToBytes() => ucSSID?.Take((int)uSSIDLength).ToArray();

			private static Lazy<Encoding> _encoding = new Lazy<Encoding>(() =>
				Encoding.GetEncoding(65001, // UTF-8 code page
					EncoderFallback.ReplacementFallback,
					DecoderFallback.ExceptionFallback));

			public override string ToString()
			{
				if (ucSSID != null)
				{
					try
					{
						return _encoding.Value.GetString(ToBytes());
					}
					catch (DecoderFallbackException)
					{ }
				}
				return null;
			}
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct DOT11_MAC_ADDRESS
		{
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
			public byte[] ucDot11MacAddress;

			public byte[] ToBytes() => ucDot11MacAddress?.ToArray();

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

		[DllImport("Wlanapi.dll")]
		public static extern uint WlanRegisterNotification(
			IntPtr hClientHandle,
			uint dwNotifSource,
			[MarshalAs(UnmanagedType.Bool)] bool bIgnoreDuplicate,
			WLAN_NOTIFICATION_CALLBACK funcCallback,
			IntPtr pCallbackContext,
			IntPtr pReserved,
			uint pdwPrevNotifSource);

		public delegate void WLAN_NOTIFICATION_CALLBACK(
			IntPtr data, // Pointer to WLAN_NOTIFICATION_DATA
			IntPtr context);

		[StructLayout(LayoutKind.Sequential)]
		public struct WLAN_NOTIFICATION_DATA
		{
			public uint NotificationSource;
			public WLAN_NOTIFICATION_ACM NotificationCode;
			public Guid InterfaceGuid;
			public uint dwDataSize;
			public IntPtr pData;
		}

		public enum WLAN_NOTIFICATION_ACM : uint
		{
			wlan_notification_acm_start = 0,
			wlan_notification_acm_autoconf_enabled,
			wlan_notification_acm_autoconf_disabled,
			wlan_notification_acm_background_scan_enabled,
			wlan_notification_acm_background_scan_disabled,
			wlan_notification_acm_bss_type_change,
			wlan_notification_acm_power_setting_change,
			wlan_notification_acm_scan_complete,
			wlan_notification_acm_scan_fail,
			wlan_notification_acm_connection_start,
			wlan_notification_acm_connection_complete,
			wlan_notification_acm_connection_attempt_fail,
			wlan_notification_acm_filter_list_change,
			wlan_notification_acm_interface_arrival,
			wlan_notification_acm_interface_removal,
			wlan_notification_acm_profile_change,
			wlan_notification_acm_profile_name_change,
			wlan_notification_acm_profiles_exhausted,
			wlan_notification_acm_network_not_available,
			wlan_notification_acm_network_available,
			wlan_notification_acm_disconnecting,
			wlan_notification_acm_disconnected,
			wlan_notification_acm_adhoc_network_state_change,
			wlan_notification_acm_profile_unblocked,
			wlan_notification_acm_screen_power_change,
			wlan_notification_acm_profile_blocked,
			wlan_notification_acm_scan_list_refresh,
			wlan_notification_acm_end
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
		private struct WLAN_CONNECTION_NOTIFICATION_DATA
		{
			public WLAN_CONNECTION_MODE wlanConnectionMode;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string strProfileName;

			public DOT11_SSID dot11Ssid;
			public DOT11_BSS_TYPE dot11BssType;

			[MarshalAs(UnmanagedType.Bool)]
			public bool bSecurityEnabled;

			public uint wlanReasonCode; // WLAN_REASON_CODE
			public uint dwFlags;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1)]
			public string strProfileXml;
		}

		public const uint WLAN_NOTIFICATION_SOURCE_NONE = 0;
		public const uint WLAN_NOTIFICATION_SOURCE_ALL = 0x0000FFFF;
		public const uint WLAN_NOTIFICATION_SOURCE_ACM = 0x00000008;
		public const uint WLAN_NOTIFICATION_SOURCE_HNWK = 0x00000080;
		public const uint WLAN_NOTIFICATION_SOURCE_ONEX = 0x00000004;
		public const uint WLAN_NOTIFICATION_SOURCE_MSM = 0x00000010;
		public const uint WLAN_NOTIFICATION_SOURCE_SECURITY = 0x00000020;
		public const uint WLAN_NOTIFICATION_SOURCE_IHV = 0x00000040;

		private const uint ERROR_SUCCESS = 0;

		#endregion

		private readonly WlanClient _client;

		public NativeWifi()
		{
			_client = new WlanClient();

			RegisterNotification(_client.Handle, OnNotificationReceived);
		}

		#region IDisposable

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

			if (disposing)
			{
				UnregisterNotification(_client.Handle);

				_client.Dispose();
			}

			_disposed = true;
		}

		#endregion

		/// <summary>
		/// Occurs when a wireless interface is connected to a wireless LAN.
		/// </summary>
		public event EventHandler<ConnectionEventArgs> Connected;

		/// <summary>
		/// Occurs when a wireless interface is disconnected from a wireless LAN.
		/// </summary>
		public event EventHandler<ConnectionEventArgs> Disconnected;

		private void OnNotificationReceived(WLAN_NOTIFICATION_DATA e)
		{
			switch (e.NotificationCode)
			{
				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_connection_complete:
					{
						var data = Marshal.PtrToStructure<WLAN_CONNECTION_NOTIFICATION_DATA>(e.pData);
						Connected?.Invoke(this, new ConnectionEventArgs(data.dot11Ssid.ToString()));
					}
					break;

				case WLAN_NOTIFICATION_ACM.wlan_notification_acm_disconnected:
					{
						var data = Marshal.PtrToStructure<WLAN_CONNECTION_NOTIFICATION_DATA>(e.pData);
						Disconnected?.Invoke(this, new ConnectionEventArgs(data.dot11Ssid.ToString()));
					}
					break;
			}
		}

		/// <summary>
		/// Enumerates SSIDs of connected wireless LANs.
		/// </summary>
		/// <returns>SSIDs</returns>
		public IEnumerable<string> EnumerateConnectedNetworkSsids()
		{
			foreach (var interfaceInfo in GetInterfaceInfoList(_client.Handle))
			{
				var connection = GetConnectionAttributes(_client.Handle, interfaceInfo.InterfaceGuid);
				if (connection.isState != WLAN_INTERFACE_STATE.wlan_interface_state_connected)
					continue;

				var association = connection.wlanAssociationAttributes;

				yield return association.dot11Ssid.ToString();
			}
		}

		#region Base

		private class WlanClient : IDisposable
		{
			private readonly IntPtr _clientHandle = IntPtr.Zero;

			public IntPtr Handle => _clientHandle;

			public WlanClient()
			{
				var result = WlanOpenHandle(
					2, // Client version for Windows Vista and Windows Server 2008
					IntPtr.Zero,
					out _,
					out _clientHandle);

				if (result != ERROR_SUCCESS)
					throw new Win32Exception((int)result);
			}

			#region IDisposable

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

		private WLAN_NOTIFICATION_CALLBACK _notificationCallback;

		private void RegisterNotification(IntPtr clientHandle, Action<WLAN_NOTIFICATION_DATA> notificationReceived)
		{
			// Storing a delegate in class field is necessary to prevent garbage collector from collecting
			// the delegate before it is called. Otherwise, CallbackOnCollectedDelegate may occur.
			_notificationCallback = new WLAN_NOTIFICATION_CALLBACK((data, context) =>
			{
				var notificationData = Marshal.PtrToStructure<WLAN_NOTIFICATION_DATA>(data);
				if (notificationData.NotificationSource != WLAN_NOTIFICATION_SOURCE_ACM)
					return;

				notificationReceived?.Invoke(notificationData);
			});

			var result = WlanRegisterNotification(
				clientHandle,
				WLAN_NOTIFICATION_SOURCE_ACM,
				false,
				_notificationCallback,
				IntPtr.Zero,
				IntPtr.Zero,
				0);

			if (result != ERROR_SUCCESS)
				throw new Win32Exception((int)result);
		}

		private void UnregisterNotification(IntPtr clientHandle)
		{
			_notificationCallback = new WLAN_NOTIFICATION_CALLBACK((data, context) => { });

			var result = WlanRegisterNotification(
				clientHandle,
				WLAN_NOTIFICATION_SOURCE_NONE,
				false,
				_notificationCallback,
				IntPtr.Zero,
				IntPtr.Zero,
				0);
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
				var result = WlanQueryInterface(
					clientHandle,
					interfaceGuid,
					WLAN_INTF_OPCODE.wlan_intf_opcode_current_connection,
					IntPtr.Zero,
					out _,
					out queryData,
					IntPtr.Zero);

				return (result == ERROR_SUCCESS)
					? Marshal.PtrToStructure<WLAN_CONNECTION_ATTRIBUTES>(queryData)
					: default;
			}
			finally
			{
				if (queryData != IntPtr.Zero)
					WlanFreeMemory(queryData);
			}
		}

		#endregion
	}

	public class ConnectionEventArgs : EventArgs
	{
		public string Ssid { get; }

		public ConnectionEventArgs(string ssid) : base() => this.Ssid = ssid;
	}
}
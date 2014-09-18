using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyTool.Models
{
	/// <summary>
	/// Disk information
	/// </summary>
	public class DiskInfo
	{
		/// <summary>
		/// Index number of Physical drive
		/// </summary>
		/// <remarks>
		/// "Index" in Wi32_DiskDrive
		/// "DeviceId" in MSFT_PhysicalDisk
		/// </remarks>
		public int PhysicalDrive { get; set; }

		/// <summary>
		/// Size (Bytes) by WMI (Win32_DiskDrive)
		/// </summary>
		public ulong Size { get; set; }

		/// <summary>
		/// Media type by WMI (Win32_DiskDrive)
		/// </summary>
		/// <remarks>
		/// Possible values (Actual values may be shorter):
		/// External hard disk media
		/// Removable media other than floppy
		/// Fixed hard disk media
		/// Format is unknown
		/// </remarks>
		public string MediaType { get; set; }

		/// <summary>
		/// Drive type by WMI (Win32_LogicalDisk)
		/// </summary>
		/// <remarks>
		/// Possible values:
		/// 0 = Unknown
		/// 1 = No Root Directory
		/// 2 = Removable Disk
		/// 3 = Local Disk
		/// 4 = Network Drive
		/// 5 = Compact Disc
		/// 6 = RAM Disk
		/// </remarks>
		public uint DriveType { get; set; }

		/// <summary>
		/// Bus type by WMI (MSFT_PhysicalDisk)
		/// </summary>
		/// <remarks>
		/// Possible values:
		/// 0 = Unknown
		/// 1 = SCSI
		/// 2 = ATAPI
		/// 3 = ATA
		/// 4 = IEEE 1394
		/// 5 = SSA
		/// 6 = Fibre Channel
		/// 7 = USB
		/// 8 = RAID
		/// 9 = iSCSI
		/// 10 = Serial Attached SCSI (SAS)
		/// 11 = Serial ATA (SATA)
		/// 12 = Secure Digital (SD)
		/// 13 = Multimedia Card (MMC)
		/// 14 = Reserved
		/// 15 = File-Backed Virtual
		/// 16 = Storage spaces
		/// 17 = Reserved
		/// </remarks>
		public ushort BusType { get; set; }

		/// <summary>
		/// Drive letters by WMI (Win32_LogicalDisk)
		/// </summary>
		public string[] DriveLetters { get; set; }
		
		/// <summary>
		/// Whether this disk can be SD
		/// </summary>
		public bool CanBeSD
		{
			get
			{
				return ((BusType == 12) || (DriveType == 2) || MediaType.ToLower().Contains("removable"));
			}
		}
	}
}

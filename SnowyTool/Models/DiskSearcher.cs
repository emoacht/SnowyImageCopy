using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

using SnowyTool.Helper;

namespace SnowyTool.Models
{
	/// <summary>
	/// Search disks.
	/// </summary>
	internal static class DiskSearcher
	{
		/// <summary>
		/// Search disks by WMI.
		/// </summary>
		internal static DiskInfo[] Search()
		{
			var diskGroup = new List<DiskInfo>();

			SearchDiskDrive(ref diskGroup);
			SearchPhysicalDisk(ref diskGroup);

			return diskGroup.ToArray();
		}

		/// <summary>
		/// Search drives by WMI (Win32_DiskDrive, Win32_DiskPartition, Win32_LogicalDisk).
		/// </summary>
		private static void SearchDiskDrive(ref List<DiskInfo> diskGroup)
		{
			var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

			foreach (var drive in searcher.Get())
			{
				if (drive["Index"] == null) // Index number of physical drive
					continue;

				int index;
				if (!int.TryParse(drive["Index"].ToString(), out index))
					continue;

				var info = new DiskInfo();
				info.PhysicalDrive = index;

				if (drive["MediaType"] != null)
				{
					info.MediaType = drive["MediaType"].ToString();
				}

				if (drive["Size"] != null)
				{
					ulong numSize;
					if (ulong.TryParse(drive["Size"].ToString(), out numSize))
						info.Size = numSize;
				}

				var driveLetters = new List<string>();

				foreach (var diskPartition in ((ManagementObject)drive).GetRelated("Win32_DiskPartition"))
				{
					if ((diskPartition["DiskIndex"] == null) || // Index number of physical drive
						(diskPartition["DiskIndex"].ToString() != info.PhysicalDrive.ToString(CultureInfo.InvariantCulture)))
						continue;

					foreach (var logicalDisk in ((ManagementObject)diskPartition).GetRelated("Win32_LogicalDisk"))
					{
						if (logicalDisk["DeviceID"] == null) // Drive letter
							continue;

						var driveLetter = logicalDisk["DeviceID"].ToString().Trim();

						if (String.IsNullOrEmpty(driveLetter) || !driveLetter.EndsWith(':'.ToString(CultureInfo.InvariantCulture)))
							continue;

						driveLetters.Add(driveLetter);

						if (logicalDisk["DriveType"] != null)
						{
							uint numType;
							if (!uint.TryParse(logicalDisk["DriveType"].ToString(), out numType))
								continue;

							info.DriveType = numType;
						}
					}
				}

				if (!driveLetters.Any())
					continue;

				info.DriveLetters = driveLetters.ToArray();

				diskGroup.Add(info);
			}
		}

		/// <summary>
		/// Search drives and supplement information by WMI (MSFT_PhysicalDisk).
		/// </summary>
		/// <remarks>Windows Storage Management API is only available for Windows 8 or newer.</remarks>
		private static void SearchPhysicalDisk(ref List<DiskInfo> diskGroup)
		{
			if (!OsVersion.IsEightOrNewer)
				return;

			var searcher = new ManagementObjectSearcher(@"\\.\Root\Microsoft\Windows\Storage", "SELECT * FROM MSFT_PhysicalDisk");

			foreach (var drive in searcher.Get())
			{
				if (drive["DeviceId"] == null) // Index number of physical drive
					continue;

				int numId;
				if (!int.TryParse(drive["DeviceId"].ToString(), out numId))
					continue;

				var info = diskGroup.FirstOrDefault(x => x.PhysicalDrive == numId);
				if (info == null)
					continue;

				if (drive["BusType"] != null)
				{
					ushort numType;
					if (ushort.TryParse(drive["BusType"].ToString(), out numType))
						info.BusType = numType;
				}
			}
		}
	}
}
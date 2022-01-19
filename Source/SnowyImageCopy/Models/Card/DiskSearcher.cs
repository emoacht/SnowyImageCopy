using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Models.Card
{
	/// <summary>
	/// Searches disks.
	/// </summary>
	internal static class DiskSearcher
	{
		/// <summary>
		/// Searches disks by WMI.
		/// </summary>
		/// <returns>Array of disk information</returns>
		internal static DiskInfo[] Search()
		{
			return SearchByDiskDrive().SupplementByPhysicalDisk().ToArray();
		}

		/// <summary>
		/// Searches disk information by WMI (Win32_DiskDrive, Win32_DiskPartition, Win32_LogicalDisk).
		/// </summary>
		/// <returns>Enumeration of disk information</returns>
		private static IEnumerable<DiskInfo> SearchByDiskDrive()
		{
			using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");

			foreach (ManagementObject diskDrive in searcher.Get()) // Casting to ManagementObject is for GetRelated method.
			{
				if (diskDrive["Index"] is not uint index) // Index represents index number of physical drive.
					continue;

				var mediaType = diskDrive["MediaType"] as string;

				foreach (ManagementObject diskPartition in diskDrive.GetRelated("Win32_DiskPartition")) // Casting to ManagementObject is for GetRelated method.
				{
					if ((diskPartition["DiskIndex"] is not uint diskIndex) || // DiskIndex represents index number of physical drive.
						(diskIndex != index))
						continue;

					foreach (var logicalDisk in diskPartition.GetRelated("Win32_LogicalDisk"))
					{
						var driveLetter = (logicalDisk["DeviceID"] as string)?.Trim(); // Drive letter							
						if (string.IsNullOrEmpty(driveLetter) ||
							!driveLetter.EndsWith(':'.ToString(CultureInfo.InvariantCulture)))
							continue;

						yield return new DiskInfo
						{
							PhysicalDrive = index,
							MediaType = mediaType,
							DriveType = (logicalDisk["DriveType"] is uint driveType) ? driveType : 0U,
							Size = (logicalDisk["Size"] is ulong size) ? size : 0UL,
							FreeSpace = (logicalDisk["FreeSpace"] is ulong freeSpace) ? freeSpace : 0UL,
							DriveLetter = driveLetter,
							VolumeLabel = (logicalDisk["VolumeName"] as string)?.Trim()
						};
					}
				}
			}
		}

		/// <summary>
		/// Supplements disk information by WMI (MSFT_PhysicalDisk).
		/// </summary>
		/// <param name="source">Enumeration of disk information</param>
		/// <returns>Enumeration of disk information</returns>
		/// <remarks>Windows Storage Management API is only available for Windows 8 or newer.</remarks>
		private static IEnumerable<DiskInfo> SupplementByPhysicalDisk(this IEnumerable<DiskInfo> source)
		{
			return OsVersion.IsEightOrNewer ? Enumerate() : source;

			IEnumerable<DiskInfo> Enumerate()
			{
				foreach (var disk in source)
				{
					using (var searcher = new ManagementObjectSearcher(
						@"\\.\Root\Microsoft\Windows\Storage",
						$"SELECT * FROM MSFT_PhysicalDisk WHERE DeviceId = {disk.PhysicalDrive}")) // DeviceId represents index number of physical drive.
					using (var physicalDisk = searcher.Get().Cast<ManagementObject>().FirstOrDefault())
					{
						if (physicalDisk?["BusType"] is ushort busType)
							disk.BusType = busType;
					}

					yield return disk;
				}
			}
		}
	}
}
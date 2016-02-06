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
			using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive"))
			{
				foreach (ManagementObject disk in searcher.Get()) // Casting to ManagementObject is for GetRelated method.
				{
					if (!(disk["Index"] is uint)) // Index represents index number of physical drive.
						continue;

					var info = new DiskInfo();
					info.PhysicalDrive = (uint)disk["Index"];
					info.MediaType = disk["MediaType"] as string;
					info.Size = (disk["Size"] is ulong) ? (ulong)disk["Size"] : 0L;

					var driveLetters = new List<string>();

					foreach (ManagementObject diskPartition in disk.GetRelated("Win32_DiskPartition")) // Casting to ManagementObject is for GetRelated method.
					{
						if (!(diskPartition["DiskIndex"] is uint) || // DiskIndex represents index number of physical drive.
							((uint)diskPartition["DiskIndex"] != info.PhysicalDrive))
							continue;

						foreach (var logicalDisk in diskPartition.GetRelated("Win32_LogicalDisk"))
						{
							var driveLetter = logicalDisk["DeviceID"] as string; // Drive letter
							if (!String.IsNullOrWhiteSpace(driveLetter) &&
								driveLetter.Trim().EndsWith(':'.ToString(CultureInfo.InvariantCulture)))
							{
								driveLetters.Add(driveLetter.Trim());
							}

							if ((info.DriveType == 0) && (logicalDisk["DriveType"] is uint))
							{
								info.DriveType = (uint)logicalDisk["DriveType"];
							}
						}
					}

					if (!driveLetters.Any())
						continue;

					info.DriveLetters = driveLetters.ToArray();

					yield return info;
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
			var canUseStorageManagement = OsVersion.IsEightOrNewer;

			foreach (var info in source)
			{
				if (canUseStorageManagement)
				{
					using (var searcher = new ManagementObjectSearcher(
						@"\\.\Root\Microsoft\Windows\Storage",
						String.Format("SELECT * FROM MSFT_PhysicalDisk WHERE DeviceId = {0}", info.PhysicalDrive))) // DeviceId represents index number of physical drive.
					{
						var disk = searcher.Get().Cast<ManagementObject>().FirstOrDefault();
						if ((disk != null) && (disk["BusType"] is ushort))
						{
							info.BusType = (ushort)disk["BusType"];
						}
					}
				}

				yield return info;
			}
		}
	}
}
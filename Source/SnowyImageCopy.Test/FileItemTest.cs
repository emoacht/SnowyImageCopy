using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SnowyImageCopy.Helper;
using SnowyImageCopy.Models;

namespace SnowyImageCopy.Test
{
	[TestClass]
	public class FileItemTest
	{
		#region Import

		/// <summary>
		/// Directory 1
		/// </summary>
		[TestMethod]
		public void TestImportDirectory1()
		{
			TestImportBase(
				fileEntry: "/DCIM,100__TSB,0,16,17181,18432",
				directoryPath: "/DCIM",
				directory: "/DCIM",
				fileName: "100__TSB",
				size: 0,
				date: new DateTime(2013, 8, 29, 9, 0, 0),
				isReadOnly: false,
				isHidden: false,
				isSystem: false,
				isVolume: false,
				isDirectory: true,
				isArchive: false,
				isFlashAirSystem: false,
				isJpeg: false);
		}

		/// <summary>
		/// Directory 2
		/// </summary>
		[TestMethod]
		public void TestImportDirectory2()
		{
			TestImportBase(
				directoryPath: "",
				fileName: "DCIM",
				size: 0,
				isFile: false,
				date: DateTime.Today);
		}

		/// <summary>
		/// File 1
		/// </summary>
		[TestMethod]
		public void TestImportFile1()
		{
			TestImportBase(
				fileEntry: "/DCIM/160___01,IMG_6221.JPG,2299469,32,17953,20181",
				directoryPath: "/DCIM/160___01",
				directory: "/DCIM/160___01",
				fileName: "IMG_6221.JPG",
				size: 2299469,
				date: new DateTime(2015, 1, 1, 9, 54, 42),
				isReadOnly: false,
				isHidden: false,
				isSystem: false,
				isVolume: false,
				isDirectory: false,
				isArchive: true,
				isFlashAirSystem: false,
				isJpeg: true);
		}

		/// <summary>
		/// File 2
		/// </summary>
		[TestMethod]
		public void TestImportFile2()
		{
			TestImportBase(
				fileEntry: "/DCIM/170___02,IMG_0058.CR2,25235506,32,18607,35600",
				directoryPath: "/DCIM/170___02",
				directory: "/DCIM/170___02",
				fileName: "IMG_0058.CR2",
				size: 25235506,
				date: new DateTime(2016, 5, 15, 17, 24, 32),
				isReadOnly: false,
				isHidden: false,
				isSystem: false,
				isVolume: false,
				isDirectory: false,
				isArchive: true,
				isFlashAirSystem: false,
				isJpeg: false);
		}

		/// <summary>
		/// File 3
		/// </summary>
		[TestMethod]
		public void TestImportFile3()
		{
			TestImportBase(
				fileEntry: "/DCIM/150___03,IMG_6837.JPG,2747563,32,17513,39513",
				directoryPath: "/DCIM/150___03",
				directory: "/DCIM/150___03",
				fileName: "IMG_6837.JPG",
				size: 2747563,
				date: new DateTime(2014, 3, 9, 19, 18, 50));
		}

		/// <summary>
		/// File 4
		/// </summary>
		[TestMethod]
		public void TestImportFile4()
		{
			TestImportBase(
				directoryPath: "/DCIM/100___012345678901234567890123456789",
				fileName: "IMG_012345678901234567890123456789.JPG",
				size: 1922840,
				isFile: true,
				date: DateTime.Today);
		}

		/// <summary>
		/// FlashAir system file
		/// </summary>
		[TestMethod]
		public void TestImportFlashAirSystemFile()
		{
			TestImportBase(
				fileEntry: $"/DCIM/100__TSB,FA000001.JPG,128751,32,17181,18432",
				directoryPath: "/DCIM/100__TSB",
				directory: "/DCIM/100__TSB",
				fileName: "FA000001.JPG",
				size: 128751,
				date: new DateTime(2013, 8, 29, 9, 0, 0),
				isReadOnly: false,
				isHidden: false,
				isSystem: false,
				isVolume: false,
				isDirectory: false,
				isArchive: true,
				isFlashAirSystem: true,
				isJpeg: true);
		}

		/// <summary>
		/// Valid date
		/// </summary>
		[TestMethod]
		public void TestImportValidDate()
		{
			TestImportBase(
				fileEntry: ",DCIM,0,16,17852,17",
				directoryPath: "",
				directory: "",
				fileName: "DCIM",
				size: 0,
				date: default);
		}

		/// <summary>
		/// Invalid size
		/// </summary>
		[TestMethod]
		public void TestImportInvalidSize()
		{
			var size = (long)Int32.MaxValue * 2;

			TestImportBase(
				fileEntry: $"/DCIM/150___03,IMG_3862.CR2,{size},32,17519,31985",
				directoryPath: "/DCIM/150___03",
				directory: "/DCIM/150___03",
				fileName: "IMG_3862.CR2",
				size: 0,
				date: default,
				isImported: false);
		}

		#region Base

		private void TestImportBase(
			string directoryPath,
			string fileName,
			long size,
			bool isFile,
			DateTime date,
			bool isImported = true)
		{
			var fileEntry = string.Format("{0},{1},{2},{3},{4},{5}",
				directoryPath,
				fileName,
				size,
				(isFile ? 32 : 16),
				FatDateTime.ConvertFromDateTimeToDateInt(date),
				FatDateTime.ConvertFromDateTimeToTimeInt(date));

			TestImportBase(fileEntry, directoryPath, directoryPath, fileName, size, date, isImported);
		}

		private void TestImportBase(
			string fileEntry,
			string directoryPath,
			string directory,
			string fileName,
			long size,
			DateTime date,
			bool isImported = true)
		{
			var instance = new FileItem(fileEntry, directoryPath);

			Assert.AreEqual(directory, instance.Directory, nameof(directory));
			Assert.AreEqual(fileName, instance.FileName, nameof(fileName));
			Assert.AreEqual(size, instance.Size, nameof(size));
			Assert.AreEqual(date, instance.Date, nameof(date));

			Assert.AreEqual(isImported, instance.IsImported, nameof(isImported));
		}

		private void TestImportBase(
			string fileEntry,
			string directoryPath,
			string directory,
			string fileName,
			int size,
			DateTime date,
			bool isReadOnly,
			bool isHidden,
			bool isSystem,
			bool isVolume,
			bool isDirectory,
			bool isArchive,
			bool isFlashAirSystem,
			bool isJpeg,
			bool isImported = true)
		{
			var instance = new FileItem(fileEntry, directoryPath);

			Assert.AreEqual(directory, instance.Directory, nameof(directory));
			Assert.AreEqual(fileName, instance.FileName, nameof(fileName));
			Assert.AreEqual(size, instance.Size, nameof(size));
			Assert.AreEqual(date, instance.Date, nameof(date));

			Assert.AreEqual(isReadOnly, instance.IsReadOnly, nameof(isReadOnly));
			Assert.AreEqual(isHidden, instance.IsHidden, nameof(isHidden));
			Assert.AreEqual(isSystem, instance.IsSystem, nameof(isSystem));
			Assert.AreEqual(isVolume, instance.IsVolume, nameof(isVolume));
			Assert.AreEqual(isDirectory, instance.IsDirectory, nameof(isDirectory));
			Assert.AreEqual(isArchive, instance.IsArchive, nameof(isArchive));

			Assert.AreEqual(isFlashAirSystem, instance.IsFlashAirSystem, nameof(isFlashAirSystem));
			Assert.AreEqual(isJpeg, instance.IsJpeg, nameof(isJpeg));

			Assert.AreEqual(isImported, instance.IsImported, nameof(isImported));
		}

		#endregion

		#endregion

		#region CompareTo

		[TestMethod]
		public void TestCompareTo()
		{
			var baseTime = DateTime.Now;

			var instances = new List<FileItem>
			{
				CreateFileItem("/DCIM/141___01", "IMG_5982.JPG", 2209469, 32, baseTime.AddMonths(1)), // 5
				CreateFileItem("/DCIM/100___01", "IMG_6256.JPG", 2209461, 32, baseTime), // 0
				CreateFileItem("/DCIM/100___01", "IMG_1358.JPG", 2209461, 32, baseTime.AddHours(1)), // 2
				CreateFileItem("/DCIM/140___01", "IMG_1340.JPG", 2209461, 32, baseTime.AddDays(1)), // 3
				CreateFileItem("/DCIM/100___01", "IMG_1356.JPG", 2209461, 32, baseTime.AddHours(1)), // 1
				CreateFileItem("/DCIM/141___01", "IMG_5982.JPG", 1256912, 32, baseTime.AddMonths(1)), // 4
			};
			instances.Sort();

			Assert.AreEqual("IMG_6256.JPG", instances[0].FileName);
			Assert.AreEqual("IMG_1356.JPG", instances[1].FileName);
			Assert.AreEqual("IMG_1358.JPG", instances[2].FileName);
			Assert.AreEqual("IMG_1340.JPG", instances[3].FileName);
			Assert.AreEqual(1256912, instances[4].Size);
			Assert.AreEqual(2209469, instances[5].Size);
		}

		private FileItem CreateFileItem(string directoryPath, string fileName, int size, int attributes, DateTime date)
		{
			var fileEntry = string.Format("{0},{1},{2},{3},{4},{5}",
				directoryPath,
				fileName,
				size,
				attributes,
				FatDateTime.ConvertFromDateTimeToDateInt(date),
				FatDateTime.ConvertFromDateTimeToTimeInt(date));

			return new FileItem(fileEntry, directoryPath);
		}

		#endregion
	}
}
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
		/// Directory case
		/// </summary>
		[TestMethod]
		public void TestImportDirectory()
		{
			TestImportBase("/DCIM,100__TSB,0,16,17181,18432", "/DCIM", "/DCIM", "100__TSB", 0, false, false, false, false, true, false, new DateTime(2013, 8, 29, 9, 0, 0));
		}

		/// <summary>
		/// File case
		/// </summary>
		[TestMethod]
		public void TestImportFile()
		{
			TestImportBase("/DCIM/160___01,IMG_6221.JPG,2299469,32,17953,20181", "/DCIM/160___01", "/DCIM/160___01", "IMG_6221.JPG", 2299469, false, false, false, false, false, true, new DateTime(2015, 1, 1, 9, 54, 42));
		}

		/// <summary>
		/// File case without attributes
		/// </summary>
		[TestMethod]
		public void TestImportFileWithoutAttributes()
		{
			TestImportBase("/DCIM/150___03,IMG_6837.JPG,2747563,32,17513,39513", "/DCIM/150___03", "/DCIM/150___03", "IMG_6837.JPG", 2747563, new DateTime(2014, 3, 9, 19, 18, 50));
		}

		/// <summary>
		/// Invalid date case
		/// </summary>
		[TestMethod]
		public void TestImportInvalidDate()
		{
			TestImportBase(",DCIM,0,16,17852,17", "", "", "DCIM", 0, default(DateTime));
		}

		/// <summary>
		/// Invalid size case
		/// </summary>
		[TestMethod]
		public void TestImportInvalidSize()
		{
			var size = (long)Int32.MaxValue * 2;
			var fileEntry = String.Format("/DCIM/150___03,IMG_3862.CR2,{0},32,17519,31985", size);

			TestImportBase(fileEntry, "/DCIM/150___03", "/DCIM/150___03", "IMG_3862.CR2", 0, default(DateTime), false);
		}

		/// <summary>
		/// Other cases
		/// </summary>
		[TestMethod]
		public void TestImportOther()
		{
			TestImportBase("", "DCIM", 0, false, DateTime.Today);
			TestImportBase("/DCIM/100___012345678901234567890123456789", "IMG_012345678901234567890123456789.JPG", 1922840, true, DateTime.Today);
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
			var fileEntry = String.Format("{0},{1},{2},{3},{4},{5}",
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

			Assert.AreEqual(directory, instance.Directory);
			Assert.AreEqual(fileName, instance.FileName);
			Assert.AreEqual(size, instance.Size);
			Assert.AreEqual(date, instance.Date);
			Assert.AreEqual(isImported, instance.IsImported);
		}

		private void TestImportBase(
			string fileEntry,
			string directoryPath,
			string directory,
			string fileName,
			int size,
			bool isReadOnly,
			bool isHidden,
			bool isSystemFile,
			bool isVolume,
			bool isDirectory,
			bool isArchive,
			DateTime date,
			bool isImported = true)
		{
			var instance = new FileItem(fileEntry, directoryPath);

			Assert.AreEqual(directory, instance.Directory);
			Assert.AreEqual(fileName, instance.FileName);
			Assert.AreEqual(size, instance.Size);

			Assert.AreEqual(isReadOnly, instance.IsReadOnly);
			Assert.AreEqual(isHidden, instance.IsHidden);
			Assert.AreEqual(isSystemFile, instance.IsSystemFile);
			Assert.AreEqual(isVolume, instance.IsVolume);
			Assert.AreEqual(isDirectory, instance.IsDirectory);
			Assert.AreEqual(isArchive, instance.IsArchive);

			Assert.AreEqual(date, instance.Date);
			Assert.AreEqual(isImported, instance.IsImported);
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
			var fileEntry = String.Format("{0},{1},{2},{3},{4},{5}",
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
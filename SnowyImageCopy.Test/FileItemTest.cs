using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SnowyImageCopy.Helper;
using SnowyImageCopy.Models;

namespace SnowyImageCopy.Test
{
	[TestClass]
	public class FileItemTest
	{
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
	}
}
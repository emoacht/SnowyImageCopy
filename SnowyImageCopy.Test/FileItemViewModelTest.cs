using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SnowyImageCopy.Helper;
using SnowyImageCopy.ViewModels;

namespace SnowyImageCopy.Test
{
	[TestClass]
	public class FileItemViewModelTest
	{
		/// <summary>
		/// Basic directory case
		/// </summary>
		[TestMethod]
		public void ImportTest1()
		{
			ImportTestBase("/DCIM,100__TSB,0,16,17181,18432", "/DCIM", "/DCIM", "100__TSB", 0, false, false, false, false, true, false, new DateTime(2013, 8, 29, 9, 0, 0));
		}

		/// <summary>
		/// Basic file case
		/// </summary>
		[TestMethod]
		public void ImportTest2()
		{
			ImportTestBase("/DCIM/160___01,IMG_6221.JPG,2299469,32,17953,20181", "/DCIM/160___01", "/DCIM/160___01", "IMG_6221.JPG", 2299469, false, false, false, false, false, true, new DateTime(2015, 1, 1, 9, 54, 42));
		}

		/// <summary>
		/// Basic file case without attributes
		/// </summary>
		[TestMethod]
		public void ImportTest3()
		{
			ImportTestBase("/DCIM/150___03,IMG_6837.JPG,2747563,32,17513,39513", "/DCIM/150___03", "/DCIM/150___03", "IMG_6837.JPG", 2747563, new DateTime(2014, 3, 9, 19, 18, 50));
		}

		/// <summary>
		/// Invalid date case
		/// </summary>
		[TestMethod]
		public void ImportTest4()
		{
			ImportTestBase(",DCIM,0,16,17852,17", "", "", "DCIM", 0, default(DateTime));
		}

		/// <summary>
		/// Invalid size case
		/// </summary>
		[TestMethod]
		public void ImportTest5()
		{
			var size = (long)Int32.MaxValue * 2;
			var source = String.Format("/DCIM/150___03,IMG_3862.CR2,{0},32,17519,31985", size);

			ImportTestBase(source, "/DCIM/150___03", "/DCIM/150___03", "IMG_3862.CR2", 0, default(DateTime), false);
		}

		/// <summary>
		/// Other cases
		/// </summary>
		[TestMethod]
		public void ImportTest6()
		{
			ImportTestBase("", "DCIM", 0, false, DateTime.Today);
			ImportTestBase("/DCIM/100___012345678901234567890123456789", "IMG_012345678901234567890123456789.JPG", 1922840, true, DateTime.Today);
		}


		#region Base

		private void ImportTestBase(
			string directoryPath,
			string fileName,
			long size,
			bool isFile,
			DateTime time,
			bool isImported = true)
		{
			var source = String.Format("{0},{1},{2},{3},{4},{5}",
				directoryPath,
				fileName,
				size,
				(isFile ? 32 : 16),
				FatDateTime.ConvertFromDateTimeToDateInt(time),
				FatDateTime.ConvertFromDateTimeToTimeInt(time));

			ImportTestBase(source, directoryPath, directoryPath, fileName, size, time, isImported);
		}

		private void ImportTestBase(
			string source,
			string directoryPath,
			string directory,
			string fileName,
			long size,
			DateTime date,
			bool isImported = true)
		{
			var instance = new FileItemViewModel(source, directoryPath);

			Assert.AreEqual(instance.Directory, directory);
			Assert.AreEqual(instance.FileName, fileName);
			Assert.AreEqual(instance.Size, size);
			Assert.AreEqual(instance.Date, date);
			Assert.AreEqual(instance.IsImported, isImported);
		}

		private void ImportTestBase(
			string source,
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
			var instance = new FileItemViewModel(source, directoryPath);

			Assert.AreEqual(instance.Directory, directory);
			Assert.AreEqual(instance.FileName, fileName);
			Assert.AreEqual(instance.Size, size);

			Assert.AreEqual(instance.IsReadOnly, isReadOnly);
			Assert.AreEqual(instance.IsHidden, isHidden);
			Assert.AreEqual(instance.IsSystemFile, isSystemFile);
			Assert.AreEqual(instance.IsVolume, isVolume);
			Assert.AreEqual(instance.IsDirectory, isDirectory);
			Assert.AreEqual(instance.IsArchive, isArchive);

			Assert.AreEqual(instance.Date, date);
			Assert.AreEqual(instance.IsImported, isImported);
		}

		#endregion
	}
}
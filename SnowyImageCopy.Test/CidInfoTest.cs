using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SnowyTool.Models;

namespace SnowyImageCopy.Test
{
	[TestClass]
	public class CidInfoTest
	{
		[TestMethod]
		public void TestImportCid1()
		{
			var cid = new CidInfo();
			cid.Import("02544d5357303847075000a01e00c701"); // FlashAir 8GB
			Assert.AreEqual(2, cid.ManufacturerID);
			Assert.AreEqual("TM", cid.OemApplicationID);
			Assert.AreEqual("SW08G", cid.ProductName);
			Assert.AreEqual("0.7", cid.ProductRevision);
			Assert.AreEqual(1342218270U, cid.ProductSerialNumber);
			Assert.AreEqual(new DateTime(2012, 7, 1), cid.ManufacturingDate);
		}

		[TestMethod]
		public void TestImportCid2()
		{
			var cid = new CidInfo();
			cid.Import("02544d535731364708d5c5617800e101"); // FlashAir W-02 16GB
			Assert.AreEqual(2, cid.ManufacturerID);
			Assert.AreEqual("TM", cid.OemApplicationID);
			Assert.AreEqual("SW16G", cid.ProductName);
			Assert.AreEqual("0.8", cid.ProductRevision);
			Assert.AreEqual(3586482552U, cid.ProductSerialNumber);
			Assert.AreEqual(new DateTime(2014, 1, 1), cid.ManufacturingDate);
		}

		[TestMethod]
		public void TestImportCid3()
		{
			var cid = new CidInfo();
			cid.Import("02544d535731364731d3468a7900f101"); // FlashAir W-03 16GB
			Assert.AreEqual(2, cid.ManufacturerID);
			Assert.AreEqual("TM", cid.OemApplicationID);
			Assert.AreEqual("SW16G", cid.ProductName);
			Assert.AreEqual("3.1", cid.ProductRevision);
			Assert.AreEqual(3544615545U, cid.ProductSerialNumber);
			Assert.AreEqual(new DateTime(2015, 1, 1), cid.ManufacturingDate);
		}

		[TestMethod]
		public void TestImportCid4()
		{
			var cid = new CidInfo();
			cid.Import("0353445355303847801BB8C2F600C82F");
			Assert.AreEqual(3, cid.ManufacturerID);
			Assert.AreEqual("SD", cid.OemApplicationID);
			Assert.AreEqual("SU08G", cid.ProductName);
			Assert.AreEqual("8.0", cid.ProductRevision);
			Assert.AreEqual(465093366U, cid.ProductSerialNumber);
			Assert.AreEqual(new DateTime(2012, 8, 1), cid.ManufacturingDate);
		}

		[TestMethod]
		public void TestImportCid5()
		{
			var cid = new CidInfo();
			cid.Import("02544d5341303247049c62cae60099dd");
			Assert.AreEqual(2, cid.ManufacturerID);
			Assert.AreEqual("TM", cid.OemApplicationID);
			Assert.AreEqual("SA02G", cid.ProductName);
			Assert.AreEqual("0.4", cid.ProductRevision);
			Assert.AreEqual(2623720166U, cid.ProductSerialNumber);
			Assert.AreEqual(new DateTime(2009, 9, 1), cid.ManufacturingDate);
		}
	}
}
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SnowyImageCopy.Models;

namespace SnowyImageCopy.Test
{
	[TestClass]
	public class SettingsTest
	{
		[TestMethod]
		public void TryParseRemoteAddressTest1()
		{
			TryParseRemoteAddressTestBase(@"http://flashair/", @"http://flashair/", String.Empty);
		}

		[TestMethod]
		public void TryParseRemoteAddressTest2()
		{
			TryParseRemoteAddressTestBase(@"http://flashair_012345/", @"http://flashair_012345/", String.Empty);
		}

		[TestMethod]
		public void TryParseRemoteAddressTest3()
		{
			TryParseRemoteAddressTestBase(@"http://flashair/dcim", @"http://flashair/", @"dcim");
		}

		[TestMethod]
		public void TryParseRemoteAddressTest4()
		{
			TryParseRemoteAddressTestBase(@"https://flashair//dcim/", @"https://flashair/", @"dcim/");
		}

		[TestMethod]
		public void TryParseRemoteAddressTest5()
		{
			TryParseRemoteAddressTestBase(@"http://flashair/dcim//161___01", @"http://flashair/", @"dcim/161___01");
		}

		[TestMethod]
		public void TryParseRemoteAddressTest6()
		{
			TryParseRemoteAddressTestBase(@"http:///");
		}

		[TestMethod]
		public void TryParseRemoteAddressTest7()
		{
			TryParseRemoteAddressTestBase(@"http://flashair_0123456/");
		}


		#region Base

		private void TryParseRemoteAddressTestBase(string source, string root, string descendant)
		{
			var settingsObject = new PrivateObject(new Settings());
			var args = new object[] { source, null, null };

			Assert.IsTrue((bool)settingsObject.Invoke("TryParseRemoteAddress", args));
			Assert.IsTrue(((string)args[1]).Equals(root, StringComparison.Ordinal));
			Assert.IsTrue(((string)args[2]).Equals(descendant, StringComparison.Ordinal));
		}

		private void TryParseRemoteAddressTestBase(string source)
		{
			var settingsObject = new PrivateObject(new Settings());
			var args = new object[] { source, null, null };

			Assert.IsFalse((bool)settingsObject.Invoke("TryParseRemoteAddress", args));
		}

		#endregion
	}
}
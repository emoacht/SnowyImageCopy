using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SnowyImageCopy.Models;

namespace SnowyImageCopy.Test
{
	[TestClass]
	public class SettingsTest
	{
		#region TryParseRemoteAddress

		[TestMethod]
		public void TryParseRemoteAddressTest1()
		{
			TryParseRemoteAddressTestBase(@"http://flashair/", @"http://flashair/", String.Empty);
			TryParseRemoteAddressTestBase(@"http://flashair_012345/", @"http://flashair_012345/", String.Empty);
			TryParseRemoteAddressTestBase(@"http://flashair/dcim", @"http://flashair/", @"dcim");
			TryParseRemoteAddressTestBase(@"https://flashair//dcim/", @"https://flashair/", @"dcim/");
			TryParseRemoteAddressTestBase(@"http://flashair/dcim//161___01", @"http://flashair/", @"dcim/161___01");
		}

		[TestMethod]
		public void TryParseRemoteAddressTest2()
		{
			TryParseRemoteAddressTestBase(@"http:///");
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

		#endregion
	}
}
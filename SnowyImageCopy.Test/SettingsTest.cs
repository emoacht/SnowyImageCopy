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
		public void TestTryParseRemoteAddressValid()
		{
			TestTryParseRemoteAddressBase(@"http://flashair/", @"http://flashair/", String.Empty);
			TestTryParseRemoteAddressBase(@"http://flashair_012345/", @"http://flashair_012345/", String.Empty);
			TestTryParseRemoteAddressBase(@"http://flashair/dcim", @"http://flashair/", @"dcim");
			TestTryParseRemoteAddressBase(@"https://flashair//dcim/", @"https://flashair/", @"dcim/");
			TestTryParseRemoteAddressBase(@"http://flashair/dcim//161___01", @"http://flashair/", @"dcim/161___01");
			TestTryParseRemoteAddressBase(@"http://flashair:8080/dcim//161___01", @"http://flashair:8080/", @"dcim/161___01");
		}

		[TestMethod]
		public void TestTryParseRemoteAddressInvalid()
		{
			TestTryParseRemoteAddressBase(@"http:///"); // Wrong path
			TestTryParseRemoteAddressBase(@"http://flashair"); // No slash at the end
			TestTryParseRemoteAddressBase(@"http://flashair_0123456/"); // Too long host name
		}


		#region Base

		private void TestTryParseRemoteAddressBase(string source, string root, string descendant)
		{
			var settingsObject = new PrivateObject(new Settings());
			var args = new object[] { source, null, null };

			Assert.IsTrue((bool)settingsObject.Invoke("TryParseRemoteAddress", args));
			Assert.IsTrue(((string)args[1]).Equals(root, StringComparison.Ordinal));
			Assert.IsTrue(((string)args[2]).Equals(descendant, StringComparison.Ordinal));
		}

		private void TestTryParseRemoteAddressBase(string source)
		{
			var settingsObject = new PrivateObject(new Settings());
			var args = new object[] { source, null, null };

			Assert.IsFalse((bool)settingsObject.Invoke("TryParseRemoteAddress", args));
		}

		#endregion

		#endregion
	}
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
			TestTryParseRemoteAddressBase(@"http://flashair/", @"http://flashair/", string.Empty);
			TestTryParseRemoteAddressBase(@"http://flashair_012345/", @"http://flashair_012345/", string.Empty);
			TestTryParseRemoteAddressBase(@"http://flashair/dcim", @"http://flashair/", @"dcim");
			TestTryParseRemoteAddressBase(@"https://flashair//dcim/", @"https://flashair/", @"dcim/");
			TestTryParseRemoteAddressBase(@"http://flashair/dcim//161___01", @"http://flashair/", @"dcim/161___01");
			TestTryParseRemoteAddressBase(@"http://flashair:8080/dcim//161___01", @"http://flashair:8080/", @"dcim/161___01");
		}

		[TestMethod]
		public void TestTryParseRemoteAddressInvalid()
		{
			TestTryParseRemoteAddressBase(null); // Null
			TestTryParseRemoteAddressBase(@"http:///"); // Wrong path
			TestTryParseRemoteAddressBase(@"http://flashair"); // No slash at the end
			TestTryParseRemoteAddressBase(@"http://flashair_0123456/"); // Too long host name
		}

		#region Base

		private void TestTryParseRemoteAddressBase(string source, string root, string descendant)
		{
			var settingsObject = new PrivateObject(Activator.CreateInstance(typeof(Settings), true));
			var args = new object[] { source, null, null };

			Assert.IsTrue((bool)settingsObject.Invoke("TryParseRemoteAddress", args));
			Assert.IsTrue(((string)args[1]).Equals(root, StringComparison.Ordinal));
			Assert.IsTrue(((string)args[2]).Equals(descendant, StringComparison.Ordinal));
		}

		private void TestTryParseRemoteAddressBase(string source)
		{
			var settingsObject = new PrivateObject(Activator.CreateInstance(typeof(Settings), true));
			var args = new object[] { source, null, null };

			Assert.IsFalse((bool)settingsObject.Invoke("TryParseRemoteAddress", args));
		}

		#endregion

		#endregion

		#region IsDatedFolderValid

		[TestMethod]
		public void TestIsDatedFolderValidTrue()
		{
			TestTrue("yyyyMMdd");
			TestTrue("yyyy-MM-dd");
			TestTrue("yyyy_MM_dd");
			TestTrue("ddMMyyyy");
			TestTrue("dd-MM-yyyy");
			TestTrue("dd_MM_yyyy");
			TestTrue("yyMd");
			TestTrue("ddMy");
			TestTrue("yyyyMM");
			TestTrue("yyy-MM");
			TestTrue("yy_MM");
			TestTrue("yM");
			TestTrue("MMyyyy");
			TestTrue("MMMMyyy");
			TestTrue("MM-yy");
			TestTrue("M_y");
			TestTrue("My");

			void TestTrue(string source) => TestIsDatedFolderValidBase(source, true);
		}

		[TestMethod]
		public void TestIsDatedFolderValidFalse()
		{
			TestFalse(null);
			TestFalse(string.Empty);
			TestFalse(" ");
			TestFalse("yyyymmdd");
			TestFalse("yyyyMMDD");
			TestFalse("YYYYMMdd");
			TestFalse("yyyy/MM/dd");
			TestFalse(@"yyyy\MM\dd");
			TestFalse("yyyy.MM.dd");
			TestFalse("yyyy MM dd");
			TestFalse("ddmmyyyy");
			TestFalse("MMdd");
			TestFalse("ddMM");

			void TestFalse(string source) => TestIsDatedFolderValidBase(source, false);
		}

		#region Base

		private void TestIsDatedFolderValidBase(string source, bool isValid)
		{
			var privateType = new PrivateType(typeof(Settings));
			Assert.AreEqual((bool)privateType.InvokeStatic("IsDatedFolderValid", source), isValid);
		}

		#endregion

		#endregion
	}
}
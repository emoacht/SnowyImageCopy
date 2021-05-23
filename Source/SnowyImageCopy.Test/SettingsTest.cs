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
			TestTryParseRemoteAddressBase(@"http://flashair/", @"http://flashair/", string.Empty, "flashair");
			TestTryParseRemoteAddressBase(@"http://flashair_012345/", @"http://flashair_012345/", string.Empty, "flashair_012345");
			TestTryParseRemoteAddressBase(@"http://0flash/dcim", @"http://0flash/", "dcim", "0flash");
			TestTryParseRemoteAddressBase(@"https://f//dcim/", @"https://f/", @"dcim/", "f");
			TestTryParseRemoteAddressBase(@"http://flash_air/dcim//161___01", @"http://flash_air/", @"dcim/161___01", "flash_air");
			TestTryParseRemoteAddressBase(@"http://flashair:8080/dcim//161___01", @"http://flashair:8080/", @"dcim/161___01", "flashair:8080");
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

		private void TestTryParseRemoteAddressBase(string source, string root, string descendant, string name)
		{
			var settingsObject = new PrivateObject(Activator.CreateInstance(typeof(Settings), true));
			var args = new object[] { source, null, null, null };

			Assert.IsTrue((bool)settingsObject.Invoke("TryParseRemoteAddress", args));
			Assert.IsTrue(((string)args[1]).Equals(root, StringComparison.Ordinal));
			Assert.IsTrue(((string)args[2]).Equals(descendant, StringComparison.Ordinal));
			Assert.IsTrue(((string)args[3]).Equals(name, StringComparison.Ordinal));
		}

		private void TestTryParseRemoteAddressBase(string source)
		{
			var settingsObject = new PrivateObject(Activator.CreateInstance(typeof(Settings), true));
			var args = new object[] { source, null, null, null };

			Assert.IsFalse((bool)settingsObject.Invoke("TryParseRemoteAddress", args));
		}

		#endregion

		#endregion

		#region IsValidDatedFolder

		[TestMethod]
		public void TestIsValidDatedFolderTrue()
		{
			// YMD
			TestTrue("yyyyMMdd");
			TestTrue("YYYYmmDD");
			TestTrue("yyyy-MM-dd");
			TestTrue("yyyy_MM_dd");
			TestTrue("yyMd");

			// YM
			TestTrue("yyyyMM");
			TestTrue("yyy-MM");
			TestTrue("yy_MM");
			TestTrue("yM");

			// DMY
			TestTrue("ddMMyyyy");
			TestTrue("DDMMyyyy");
			TestTrue("dd-MM-yyyy");
			TestTrue("dd_MM_yyyy");
			TestTrue("ddMy");

			// MY
			TestTrue("MMyyyy");
			TestTrue("MMMMyyy");
			TestTrue("MM-yy");
			TestTrue("M_y");
			TestTrue("My");

			// MDY
			TestTrue("MMddyyyy");
			TestTrue("MMDDyyy");
			TestTrue("M-dd-y");
			TestTrue("MM_dd_yyy");
			TestTrue("Mdy");

			void TestTrue(string source) => TestIsValidDatedFolderBase(source, true);
		}

		[TestMethod]
		public void TestIsValidDatedFolderFalse()
		{
			// Empty
			TestFalse(null);
			TestFalse(string.Empty);
			TestFalse(" ");

			// Essessive length
			TestFalse("yyyyyMMdd");
			TestFalse("yyMMMMMdd");
			TestFalse("yMMddd");

			// Invalid delimiter
			TestFalse("yyyy/MM/dd");
			TestFalse(@"yyyy\MM\dd");
			TestFalse("yyyy.MM.dd");
			TestFalse("yyyy MM dd");

			// Wrong delimiter place
			TestFalse("yy-yyMMdd");
			TestFalse("yyyy-M-Mdd");
			TestFalse("yyyy_MMd_d");

			// MD, DM, YD, DY
			TestFalse("MMdd");
			TestFalse("ddMM");
			TestFalse("yyyydd");
			TestFalse("ddYY");

			void TestFalse(string source) => TestIsValidDatedFolderBase(source, false);
		}

		#region Base

		private void TestIsValidDatedFolderBase(string source, bool isValid)
		{
			var privateType = new PrivateType(typeof(Settings));
			Assert.AreEqual((bool)privateType.InvokeStatic("IsValidDatedFolder", source), isValid);
		}

		#endregion

		#endregion
	}
}
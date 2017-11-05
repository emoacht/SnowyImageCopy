using System;
using System.IO;
using System.Linq;
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

		#region TryNormalizeLocalFolder

		[TestMethod]
		public void TestTryNormalizeLocalFolderValid()
		{
			var drive = Environment.GetFolderPath(Environment.SpecialFolder.Windows).Substring(0, 1); // Only drive letter matters.

			TestTryNormalizeLocalFolderBase($@"{drive}:\Windows", $@"{drive}:\Windows");
			TestTryNormalizeLocalFolderBase($@"{drive.ToLower()}:\Windows\abc", $@"{drive.ToLower()}:\Windows\abc");
			TestTryNormalizeLocalFolderBase($@" ' {drive}:\Windows\abc\'", $@"{drive}:\Windows\abc\");
			TestTryNormalizeLocalFolderBase($@"""{drive}:\\Windows\\abc\\ "" ", $@"{drive}:\Windows\abc\");
			TestTryNormalizeLocalFolderBase($@" "" '{drive}:\\\Windows!\\abc\def ghi '{Environment.NewLine}""", $@"{drive}:\Windows!\abc\def ghi");
		}

		[TestMethod]
		public void TestTryNormalizeLocalFolderInvalid()
		{
			var drive = Environment.GetFolderPath(Environment.SpecialFolder.Windows).Substring(0, 1);

			TestTryNormalizeLocalFolderBase($@"{drive}:\Windows|");
			TestTryNormalizeLocalFolderBase($@"{drive}A:\Windows\");

			drive = Enumerable.Range('A', 26).Select(x => Convert.ToChar(x).ToString()).FirstOrDefault(x => !Directory.Exists($@"{x}:\"));
			if (drive != null)
			{
				TestTryNormalizeLocalFolderBase($@"{drive}:\Windows\");
			}

			TestTryNormalizeLocalFolderBase($@"#:\Windows\");
			TestTryNormalizeLocalFolderBase($@":\Windows\");
			TestTryNormalizeLocalFolderBase($@"\Windows");
			TestTryNormalizeLocalFolderBase($@"Windows");
			TestTryNormalizeLocalFolderBase(@"'""'");
			TestTryNormalizeLocalFolderBase(@"\\\");
		}

		#region base

		private void TestTryNormalizeLocalFolderBase(string source, string normalized)
		{
			var settingsObject = new PrivateObject(Activator.CreateInstance(typeof(Settings), true));
			var args = new object[] { source, null };

			Assert.IsTrue((bool)settingsObject.Invoke("TryNormalizeLocalFolder", args));
			Assert.IsTrue(((string)args[1]).Equals(normalized, StringComparison.Ordinal));
		}

		private void TestTryNormalizeLocalFolderBase(string source)
		{
			var settingsObject = new PrivateObject(Activator.CreateInstance(typeof(Settings), true));
			var args = new object[] { source, null };

			Assert.IsFalse((bool)settingsObject.Invoke("TryNormalizeLocalFolder", args));
		}

		#endregion

		#endregion
	}
}
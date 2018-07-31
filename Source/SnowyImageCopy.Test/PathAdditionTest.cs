using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using SnowyImageCopy.Helper;

namespace SnowyImageCopy.Test
{
	[TestClass]
	public class PathAdditionTest
	{
		#region TryNormalizePath

		[TestMethod]
		public void TestTryNormalizePathValidDrive()
		{
			TestTryNormalizePathBase(@"C:\Users\USERNAME\Pictures\FlashAirImages", @"C:\Users\USERNAME\Pictures\FlashAirImages");

			var drives = Enumerable.Range('A', 26).Select(x => Convert.ToChar(x).ToString());
			foreach (var drive in drives)
			{
				TestTryNormalizePathBase($@"{drive}:\Windows", $@"{drive}:\Windows");
				TestTryNormalizePathBase($@"{drive.ToLower()}:\Windows\abc.jpg", $@"{drive.ToLower()}:\Windows\abc.jpg");
			}

			TestTryNormalizePathBase(@" ' C:\Windows\abc\'", @"C:\Windows\abc\");
			TestTryNormalizePathBase(@"""D:\\Windows\\abc\\ "" ", @"D:\Windows\abc\");
			TestTryNormalizePathBase(@" "" 'E:\\\Windows!\\abc\def ghi '" + Environment.NewLine + @"""", @"E:\Windows!\abc\def ghi");
		}

		[TestMethod]
		public void TestTryNormalizePathValidUnc()
		{
			TestTryNormalizePathBase(@"\\host-name\share-name\path\file", @"\\host-name\share-name\path\file");
			TestTryNormalizePathBase(@"\\Archive\Photos\FlashAir", @"\\Archive\Photos\FlashAir");
			TestTryNormalizePathBase(@"\\Archive\Photos\FlashAir\xyz.jpg", @"\\Archive\Photos\FlashAir\xyz.jpg");
			TestTryNormalizePathBase(@"\\files\personal\photos\flashair1", @"\\files\personal\photos\flashair1");
			TestTryNormalizePathBase(@"\\\Files\\Pictures\", @"\\Files\Pictures\");
			TestTryNormalizePathBase(@"\\", @"\\");
		}

		[TestMethod]
		public void TestTryNormalizePathInvalid()
		{
			TestTryNormalizePathBase(@"C:\Windows|");
			TestTryNormalizePathBase(@"AA:\Windows\");
			TestTryNormalizePathBase(@"#:\Windows\");
			TestTryNormalizePathBase(@":\Windows\");
			TestTryNormalizePathBase(@"\Windows");
			TestTryNormalizePathBase(@"Windows");
			TestTryNormalizePathBase(@"'""'");
			TestTryNormalizePathBase(@"\");
		}

		#region base

		private void TestTryNormalizePathBase(string source, string expected)
		{
			Assert.IsTrue(PathAddition.TryNormalizePath(source, out var normalized));
			Assert.IsTrue(string.Equals(normalized, expected, StringComparison.Ordinal));
		}

		private void TestTryNormalizePathBase(string source)
		{
			Assert.IsFalse(PathAddition.TryNormalizePath(source, out _));
		}

		#endregion

		#endregion
	}
}
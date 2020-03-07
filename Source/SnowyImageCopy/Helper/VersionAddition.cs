using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Additional methods for <see cref="System.Version"/> 
	/// </summary>
	public static class VersionAddition
	{
		private static readonly Regex _versionPattern = new Regex(@"[1-9]\.\d{1,2}\.\d{1,2}", RegexOptions.Compiled);

		/// <summary>
		/// Attempts to find version number.
		/// </summary>
		/// <param name="source">Source string</param>
		/// <param name="version">Version number</param>
		/// <returns>True if successfully found.</returns>
		public static bool TryFind(string source, out Version version)
		{
			version = null;

			if (string.IsNullOrWhiteSpace(source))
				return false;

			var match = _versionPattern.Match(source);
			if (!match.Success)
				return false;

			version = new Version(match.Value);
			return true;
		}
	}
}
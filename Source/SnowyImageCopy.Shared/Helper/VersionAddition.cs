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
		private static readonly Regex _shortVersionPattern = new Regex(@"[1-9]\.\d{1,2}\.\d{1,2}", RegexOptions.Compiled);

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

			var match = _shortVersionPattern.Match(source);
			if (!match.Success)
				return false;

			version = new Version(match.Value);
			return true;
		}

		private static readonly Regex _longVersionPattern = new Regex(@"\d{1,2}\.\d{1,2}(\.\d{1,4}(\.\d{1,4}|)|)$");

		/// <summary>
		/// Replace part of version number at the end.
		/// </summary>
		/// <param name="source">Source string</param>
		/// <param name="fieldCount">Version field count</param>
		/// <returns>If version number exists, Replaced string.</returns>
		public static string Replace(string source, int fieldCount = 2)
		{
			if (fieldCount is < 0 or > 4)
				throw new ArgumentOutOfRangeException(nameof(fieldCount));

			if (string.IsNullOrWhiteSpace(source))
				return source;

			var match = _longVersionPattern.Match(source);
			if (!match.Success)
				return source;

			var version = new Version(match.Value);

			fieldCount = (fieldCount, version) switch
			{
				( >= 3, { Build: < 0 }) => 2,
				(4, { Revision: < 0 }) => 3,
				_ => fieldCount
			};

			return source.Replace(match.Value, version.ToString(fieldCount));
		}
	}
}
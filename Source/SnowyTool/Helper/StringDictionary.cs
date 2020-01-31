using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyTool.Helper
{
	/// <summary>
	/// Parses string to Dictionary.
	/// </summary>
	public static class StringDictionary
	{
		/// <summary>
		/// Parses string divided by new lines and separator in each line to Dictionary.
		/// </summary>
		/// <param name="source">Source string</param>
		/// <param name="separator">Separator char</param>
		/// <returns>Dictionary of Key string and value string</returns>
		public static Dictionary<string, string> Parse(string source, char separator)
		{
			if (string.IsNullOrWhiteSpace(source))
				throw new ArgumentNullException(nameof(source));

			if (char.IsWhiteSpace(separator))
				throw new ArgumentException("The separator must not be white space.", nameof(separator));

			return source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
				.Select(x => x.Split(new[] { separator }, 2))
				.Where(x => x.Length == 2)
				.Select(x => new { Key = x[0].Trim(), Value = x[1].Trim() })
				.Where(x => !string.IsNullOrEmpty(x.Key))
				.GroupBy(x => x.Key)
				.ToDictionary(x => x.Key, x => x.Last().Value);
		}
	}
}
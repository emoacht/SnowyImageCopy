using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyTool.Helper
{
	/// <summary>
	/// Parse string to Dictionary.
	/// </summary>
	public static class StringDictionary
	{
		/// <summary>
		/// Parse string divided by new lines and separator in each line to Dictionary.
		/// </summary>
		/// <param name="source">Source string</param>
		/// <param name="separator">Separator char</param>
		/// <returns>Dictionary of Key string and value string</returns>
		public static Dictionary<string, string> Parse(string source, char separator)
		{
			if (String.IsNullOrWhiteSpace(source))
				throw new ArgumentNullException("source");

			if (Char.IsWhiteSpace(separator))
				throw new ArgumentException("The separator is invalid.", "separator");

			var content = new Dictionary<string, string>();

			var lines = source.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

			foreach (var line in lines)
			{
				// Find key and value.
				// (String.Split method may mistakenly separate value which includes same char as separator)
				var indexSeparator = line.IndexOf(separator);

				// Check if separator is found and key has at least one letter.
				if (indexSeparator < 1)
					continue;

				content.Add(line.Substring(0, indexSeparator), line.Substring(indexSeparator + 1));
			}

			return content;
		}
	}
}
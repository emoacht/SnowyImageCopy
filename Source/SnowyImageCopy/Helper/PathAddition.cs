using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Additional method for <see cref="System.IO.Path"/> 
	/// </summary>
	public static class PathAddition
	{
		public static bool TryNormalizePath(string source, out string normalized)
		{
			normalized = null;

			if (string.IsNullOrEmpty(source))
				return false;

			int endIndex = 0;
			for (int i = source.Length - 1; i >= 0; i--)
			{
				if (IsWhiteSpaceOrQuotationChar(source[i]))
				{
					if (i == 0)
						return false;

					continue;
				}

				endIndex = i;
				break;
			}

			bool IsDriveRoot(int i) => (i + 2 <= endIndex)
				&& IsDriveChar(source[i]) // A to Z
				&& (source[i + 1] == Path.VolumeSeparatorChar) // :
				&& (source[i + 2] == Path.DirectorySeparatorChar); // \

			bool IsUnc(int i) => (i + 1 <= endIndex)
				&& (source[i] == Path.DirectorySeparatorChar) // \
				&& (source[i + 1] == Path.DirectorySeparatorChar); // \

			var buff = new StringBuilder(endIndex + 1);

			int startIndex = 0;
			for (int i = 0; i <= endIndex; i++)
			{
				if (IsWhiteSpaceOrQuotationChar(source[i]))
					continue;

				int length = 0;
				if (IsDriveRoot(i))
				{
					length = 3;
				}
				else if (IsUnc(i))
				{
					length = 2;
				}
				else
				{
					return false;
				}

				buff.Append(source, i, length);
				startIndex = i + length;
				break;
			}

			for (int i = startIndex; i <= endIndex; i++)
			{
				if ((source[i] == Path.DirectorySeparatorChar) &&
					(buff[buff.Length - 1] == Path.DirectorySeparatorChar))
					continue;

				if (Path.GetInvalidPathChars().Contains(source[i])) // Invalid path char includes double quotation mark.
					return false;

				buff.Append(source[i]);
			}

			normalized = buff.ToString();
			return true;
		}

		private static bool IsWhiteSpaceOrQuotationChar(char value) =>
			char.IsWhiteSpace(value) || (value == '\'') || (value == '"');

		private static bool IsDriveChar(char value) =>
			('A' <= value && value <= 'Z') || ('a' <= value && value <= 'z');

		/// <summary>
		/// Enumerates directory paths traversing up to a drive root.
		/// </summary>
		/// <param name="source">Source path</param>
		/// <returns>Directory paths</returns>
		public static IEnumerable<string> EnumerateDirectoryPaths(string source)
		{
			if (source == null)
				yield break;

			var buff = source.Trim().Trim('\'', '"', Path.DirectorySeparatorChar);
			if (buff.Length == 0)
				yield break;

			yield return buff;

			int index = 1;
			while (index > 0)
			{
				index = buff.LastIndexOf(Path.DirectorySeparatorChar);
				if (index > 0)
					yield return buff = buff.Substring(0, index);
			}
		}
	}
}
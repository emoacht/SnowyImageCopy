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

			var buffer = new StringBuilder(endIndex + 1);

			int startIndex = 0;
			for (int i = 0; i <= endIndex; i++)
			{
				if (IsWhiteSpaceOrQuotationChar(source[i]))
					continue;

				int length;
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

				buffer.Append(source, i, length);
				startIndex = i + length;
				break;
			}

			for (int i = startIndex; i <= endIndex; i++)
			{
				if ((source[i] == Path.DirectorySeparatorChar) &&
					(buffer[buffer.Length - 1] == Path.DirectorySeparatorChar))
					continue;

				if (Path.GetInvalidPathChars().Contains(source[i])) // Invalid path char includes double quotation mark.
					return false;

				buffer.Append(source[i]);
			}

			normalized = buffer.ToString();
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
			if (source is null)
				yield break;

			var buffer = source.Trim().Trim('\'', '"', Path.DirectorySeparatorChar);
			if (buffer.Length == 0)
				yield break;

			yield return buffer;

			int index = 1;
			while (index > 0)
			{
				index = buffer.LastIndexOf(Path.DirectorySeparatorChar);
				if (index > 0)
					yield return buffer = buffer.Substring(0, index);
			}
		}
	}
}
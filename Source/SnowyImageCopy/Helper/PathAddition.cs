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
				if (char.IsWhiteSpace(source[i]) || IsQuotationChar(source[i]))
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
				if (char.IsWhiteSpace(source[i]) || IsQuotationChar(source[i]))
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

			static bool IsQuotationChar(char value) => value is '\'' or '"';
			static bool IsDriveChar(char value) => value is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z');
		}

		/// <summary>
		/// Enumerates directory paths traversing up to a drive root.
		/// </summary>
		/// <param name="source">String representing directory path</param>
		/// <returns>Enumerable collection of directory paths</returns>
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

		public static bool TryNormalizeExtensions(string source, out string normalized, out string[] extensions)
		{
			var buffer = EnumerateExtensionsWithoutDot(source).ToArray();

			if (buffer.Length > 0)
			{
				normalized = buffer.Length switch
				{
					1 => buffer[0].extension,
					> 1 => string.Join(buffer[0].separator, buffer.Select(x => x.extension)),
					_ => null
				};
				extensions = buffer.Select(x => $".{x.extension}").ToArray();
				return true;
			}
			normalized = default;
			extensions = default;
			return false;
		}

		/// <summary>
		/// Enumerates file extensions from a specified string representing file extensions.
		/// </summary>
		/// <param name="source">String representing file extensions delimited by ' ' or ',' or ';'</param>
		/// <returns>Enumerable collection of file extensions</returns>
		public static IEnumerable<string> EnumerateExtensions(string source)
		{
			return EnumerateExtensionsWithoutDot(source).Select(x => $".{x.extension}");
		}

		private static IEnumerable<(string extension, string separator)> EnumerateExtensionsWithoutDot(string source)
		{
			if (source is null)
				yield break;

			var buffer = new StringBuilder();

			foreach (char c in source.ToLower().Select(x => char.IsWhiteSpace(x) ? ' ' : x))
			{
				if (IsSeparator(c))
				{
					if (buffer.Length > 0)
					{
						yield return (extension: buffer.ToString(), separator: c.ToString());
						buffer.Clear();
					}
				}
				else if (IsAlphanumeric(c))
					buffer.Append(c);
			}

			if (buffer.Length > 0)
				yield return (extension: buffer.ToString(), separator: string.Empty);

			static bool IsSeparator(char c) => c is (' ' or ',' or ';');
			static bool IsAlphanumeric(char c) => c is (>= 'a' and <= 'z') or (>= '0' and <= '9');
		}
	}
}
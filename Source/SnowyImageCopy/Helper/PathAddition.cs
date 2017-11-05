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
		public static bool TryNormalizeDirectoryPath(string source, out string normalized)
		{
			normalized = null;

			if (source == null)
				return false;

			int endIndex = -1;
			for (int i = source.Length - 1; i >= 0; i--)
			{
				if (IsWhiteSpaceOrQuotationChar(source[i]))
					continue;

				endIndex = i;
				break;
			}

			var buff = new StringBuilder(source.Length);

			for (int i = 0; i <= endIndex; i++)
			{
				if (buff.Length == 0)
				{
					if (IsWhiteSpaceOrQuotationChar(source[i]))
						continue;
				}
				else
				{
					if ((source[i] == Path.DirectorySeparatorChar) &&
						(buff[buff.Length - 1] == Path.DirectorySeparatorChar))
						continue;
				}

				if (Path.GetInvalidPathChars().Contains(source[i])) // Invalid path char includes double quotation mark.
					return false;

				buff.Append(source[i]);
			}

			normalized = buff.ToString();

			if (!IsValidDriveRoot(normalized))
				return false;

			return Directory.Exists(Path.GetPathRoot(normalized));
		}

		private static bool IsWhiteSpaceOrQuotationChar(char value) =>
			char.IsWhiteSpace(value) || (value == '\'') || (value == '"');

		/// <summary>
		/// Checks if a specified path starts with a drive root.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <remarks>This is alternate method to Path.IsPathRooted method. It is specialized for
		/// a drive root which includes a drive letter.</remarks>
		private static bool IsValidDriveRoot(string value)
		{
			if (value.Length < 3)
				return false;

			return IsValidDriveChar(value[0])
				&& (value[1] == Path.VolumeSeparatorChar) // :
				&& (value[2] == Path.DirectorySeparatorChar); // \
		}

		private static bool IsValidDriveChar(char value) =>
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
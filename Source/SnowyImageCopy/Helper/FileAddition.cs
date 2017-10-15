using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SnowyImageCopy.Helper
{
	/// <summary>
	/// Additional method for <see cref="System.IO.File"/>
	/// </summary>
	public static class FileAddition
	{
		private const int _defaultCopyBufferSize = 81920; // 80KiB is actual default buffer size in System.IO.File class.

		/// <summary>
		/// Reads all bytes from a specified file asynchronously.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <returns>Byte array of file</returns>
		public static Task<byte[]> ReadAllBytesAsync(string filePath)
		{
			return ReadAllBytesAsync(filePath, _defaultCopyBufferSize, CancellationToken.None);
		}

		/// <summary>
		/// Reads all bytes from a specified file asynchronously.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Byte array of file</returns>
		public static Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken)
		{
			return ReadAllBytesAsync(filePath, _defaultCopyBufferSize, cancellationToken);
		}

		/// <summary>
		/// Reads all bytes from a specified file asynchronously.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <param name="bufferSize">Buffer size</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Byte array of file</returns>
		public static async Task<byte[]> ReadAllBytesAsync(string filePath, int bufferSize, CancellationToken cancellationToken)
		{
			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			using (var ms = new MemoryStream())
			{
				await fs.CopyToAsync(ms, bufferSize, cancellationToken).ConfigureAwait(false);
				return ms.ToArray();
			}
		}
	}
}
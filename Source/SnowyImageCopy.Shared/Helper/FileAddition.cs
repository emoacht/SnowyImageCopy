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
	/// Additional methods for <see cref="System.IO.File"/>
	/// </summary>
	public static class FileAddition
	{
		public const int DefaultBufferSize = 4096;
		private const int DefaultCopyBufferSize = 81920;

		/// <summary>
		/// Asynchronously opens a specified file, reads the contents of the file into a byte array.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <returns>Byte array</returns>
		public static async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken cancellationToken = default)
		{
#if NETCOREAPP3_0_OR_GREATER
			return await File.ReadAllBytesAsync(filePath, cancellationToken).ConfigureAwait(false);
#else
			using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
			using var ms = new MemoryStream();
			await fs.CopyToAsync(ms, DefaultCopyBufferSize, cancellationToken).ConfigureAwait(false);
			return ms.ToArray();
#endif
		}

		/// <summary>
		/// Asynchronously creates a specified file, writes a specified byte array to the file.
		/// </summary>
		/// <param name="filePath">File path</param>
		/// <param name="bytes">Byte array</param>
		/// <param name="cancellationToken">CancellationToken</param>
		/// <remarks>If the file already exists, it will be overwritten.</remarks>
		public static async Task WriteAllBytesAsync(string filePath, byte[] bytes, CancellationToken cancellationToken = default)
		{
#if NETCOREAPP3_0_OR_GREATER
			await File.WriteAllBytesAsync(filePath, bytes, cancellationToken).ConfigureAwait(false);
#else
			using var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.Read);
			await fs.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
#endif
		}
	}
}
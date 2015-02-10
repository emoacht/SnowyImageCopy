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
        private const int defaultCopyBufferSize = 81920; // 80KiB is actual default buffer size in System.IO.File class.

        /// <summary>
        /// Read all bytes from a specified file asynchronously.
        /// </summary>
        /// <param name="filePath">Source file path</param>
        public static async Task<byte[]> ReadAllBytesAsync(string filePath)
        {
            return await ReadAllBytesAsync(filePath, defaultCopyBufferSize, CancellationToken.None);
        }

        /// <summary>
        /// Read all bytes from a specified file asynchronously.
        /// </summary>
        /// <param name="filePath">Source file path</param>
        /// <param name="token">CancellationToken</param>
        public static async Task<byte[]> ReadAllBytesAsync(string filePath, CancellationToken token)
        {
            return await ReadAllBytesAsync(filePath, defaultCopyBufferSize, token);
        }

        /// <summary>
        /// Read all bytes from a specified file asynchronously.
        /// </summary>
        /// <param name="filePath">Source file path</param>
        /// <param name="bufferSize">Buffer size</param>
        /// <param name="token">CancellationToken</param>
        public static async Task<byte[]> ReadAllBytesAsync(string filePath, int bufferSize, CancellationToken token)
        {
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var ms = new MemoryStream())
            {
                await fs.CopyToAsync(ms, bufferSize, token);

                return ms.ToArray();
            }
        }
    }
}
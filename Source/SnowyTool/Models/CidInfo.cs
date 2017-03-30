using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Metadata.W3cXsd2001;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SnowyTool.Models
{
	/// <summary>
	/// CID information
	/// </summary>
	internal class CidInfo
	{
		#region Property (Content of CID)

		/// <summary>
		/// Manufacturer ID (MID)
		/// </summary>
		/// <remarks>
		/// 8 bits [127:120] -> bytes 0
		/// 8-bit binary number
		/// 1: Panasonic
		/// 2: Toshiba
		/// 3: SanDisk
		/// </remarks>
		public int ManufacturerID { get; private set; }

		/// <summary>
		/// OEM/Application ID (OID)
		/// </summary>
		/// <remarks>
		/// 16 bits [119:104] -> bytes 1-2
		/// 2-character ASCII string
		/// </remarks>
		public string OemApplicationID { get; private set; }

		/// <summary>
		/// Product Name (PNM)
		/// </summary>
		/// <remarks>
		/// 40 bits [103:64] -> bytes 3-7
		/// 5-character ASCII string
		/// </remarks>
		public string ProductName { get; private set; }

		/// <summary>
		/// Product Revision (PRV)
		/// </summary>
		/// <remarks>
		/// 8 bits [63:56] -> bytes 8
		/// "n.m" revision number: First 4 bits [63:60] for major revision ("n") and second 4 bits [59:56] for minor revision ("m")
		/// </remarks>
		public string ProductRevision { get; private set; }

		/// <summary>
		/// Product Serial Number (PSN)
		/// </summary>
		/// <remarks>
		/// 32 bits [55:24] -> bytes 9-12
		/// 32-bit binary number
		/// </remarks>
		public uint ProductSerialNumber { get; private set; }

		/// <summary>
		/// Manufacturing Date (MDT)
		/// </summary>
		/// <remarks>
		/// 12 bits [19:8] -> bytes 13 (second half) and 14
		/// 8 bits [19:12] at the head for count of years from 2000 and 4 bits [11:8] at the tail for month
		/// </remarks>
		public DateTime ManufacturingDate { get; private set; }

		// CID register has 128 bits = 16 bytes in total.
		// 4 bits [23:20] are reserved.
		// 7 bits [7:1] are for CRC7 checksum.
		// 1 bit [0:0] is not used and always 1.

		#endregion

		/// <summary>
		/// Source hexadecimal string
		/// </summary>
		public string Source { get; private set; }

		private static readonly Regex _asciiPattern = new Regex("^[\x20-\x7F]{32}$"); // Pattern for string in ASCII code (alphanumeric symbols)

		public void Import(string source)
		{
			this.Source = source;

			if (string.IsNullOrWhiteSpace(source) || !_asciiPattern.IsMatch(source))
				return;

			var bytes = SoapHexBinary.Parse(source).Value;

			ManufacturerID = bytes[0]; // Bytes 0
			OemApplicationID = Encoding.ASCII.GetString(bytes.Skip(1).Take(2).ToArray()); // Bytes 1-2
			ProductName = Encoding.ASCII.GetString(bytes.Skip(3).Take(5).ToArray()); // Bytes 3-7

			var productRevisionBits = new BitArray(new[] { bytes[8] }).Cast<bool>().Reverse().ToArray(); // Bytes 8
			var major = ConvertFromBitsToInt(productRevisionBits.Take(4).Reverse());
			var minor = ConvertFromBitsToInt(productRevisionBits.Skip(4).Take(4).Reverse());
			ProductRevision = $"{major}.{minor}";

			var productSerialNumberBytes = bytes.Skip(9).Take(4); // Bytes 9-12
			if (BitConverter.IsLittleEndian)
			{
				productSerialNumberBytes = productSerialNumberBytes.Reverse();
			}
			ProductSerialNumber = BitConverter.ToUInt32(productSerialNumberBytes.ToArray(), 0);

			var manufacturingDateBits = bytes.Skip(13).Take(2) // Bytes 13-14
				.SelectMany(x => new BitArray(new[] { x }).Cast<bool>().Reverse())
				.Skip(4) // Skip reserved field.
				.ToArray();

			var year = ConvertFromBitsToInt(manufacturingDateBits.Take(8).Reverse());
			var month = ConvertFromBitsToInt(manufacturingDateBits.Skip(8).Take(4).Reverse());
			if ((year <= 1000) && (month <= 12))
			{
				ManufacturingDate = new DateTime(year + 2000, month, 1);
			}
		}

		/// <summary>
		/// Converts from sequence of bits to 32-bit integer.
		/// </summary>
		/// <param name="source">Source sequence of bits</param>
		/// <returns>32-bit integer</returns>
		/// <remarks>The order of bits in source sequence must be the same as that of BitArray.
		/// The least significant bit (bit at the far right) comes at the head and the most significant
		/// bit (bit at the far left) at the tail. This is the way that BitArray handles bits.</remarks>
		private static int ConvertFromBitsToInt(IEnumerable<bool> source)
		{
			var buff = new int[1];
			new BitArray(source.ToArray()).CopyTo(buff, 0);
			return buff[0];
		}
	}
}
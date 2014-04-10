using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnowyImageCopy.Models
{
	/// <summary>
	/// CGI command types to access FlashAir card
	/// </summary>
	internal enum FileManagerCommand
	{
		None = 0,

		/// <summary>
		/// Get file list in a specified directory in FlashAir card.
		/// </summary>
		GetFileList,

		/// <summary>
		/// Get number of files in a specified directory in FlashAir card.
		/// </summary>
		GetFileNum,

		/// <summary>
		/// Get thumbnail of a specified image file in FlashAir card.
		/// </summary>
		GetThumbnail,

		/// <summary>
		/// Get update status of FlashAir card.
		/// </summary>
		GetUpdateStatus,

		/// <summary>
		/// Get SSID of FlashAir card.
		/// </summary>
		GetSsid,
	}
}

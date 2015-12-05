
namespace SnowyImageCopy.Models
{
	/// <summary>
	/// CGI command types to manage FlashAir card
	/// </summary>
	internal enum FileManagerCommand
	{
		None = 0,

		/// <summary>
		/// Get file list in a specified directory in FlashAir card.
		/// </summary>
		GetFileList,

		/// <summary>
		/// Get the number of files in a specified directory in FlashAir card.
		/// </summary>
		GetFileNum,

		/// <summary>
		/// Get thumbnail of a specified image file in FlashAir card.
		/// </summary>
		GetThumbnail,

		/// <summary>
		/// Get firmware version of FlashAir card.
		/// </summary>
		GetFirmwareVersion,

		/// <summary>
		/// Get CID (Card Identification number register) of FlashAir card.
		/// </summary>
		GetCid,

		/// <summary>
		/// Get SSID of FlashAir card.
		/// </summary>
		GetSsid,

		/// <summary>
		/// Get update status of FlashAir card.
		/// </summary>
		GetUpdateStatus,

		/// <summary>
		/// Get time stamp of write event in FlashAir card.
		/// </summary>
		GetWriteTimeStamp,

		/// <summary>
		/// Get Upload parameters of FlashAir card.
		/// </summary>
		GetUpload,

		/// <summary>
		/// Delete a specified file in FlashAir card.
		/// </summary>
		DeleteFile,
	}
}
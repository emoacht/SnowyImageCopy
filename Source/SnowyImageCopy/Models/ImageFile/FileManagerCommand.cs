
namespace SnowyImageCopy.Models.ImageFile
{
	/// <summary>
	/// CGI command types to manage FlashAir card
	/// </summary>
	internal enum FileManagerCommand
	{
		None = 0,

		/// <summary>
		/// Gets file list in a specified directory in FlashAir card.
		/// </summary>
		GetFileList,

		/// <summary>
		/// Gets the number of files in a specified directory in FlashAir card.
		/// </summary>
		GetFileNum,

		/// <summary>
		/// Gets thumbnail of a specified image file in FlashAir card.
		/// </summary>
		GetThumbnail,

		/// <summary>
		/// Gets firmware version of FlashAir card.
		/// </summary>
		GetFirmwareVersion,

		/// <summary>
		/// Gets CID (Card Identification number register) of FlashAir card.
		/// </summary>
		GetCid,

		/// <summary>
		/// Gets SSID of FlashAir card.
		/// </summary>
		GetSsid,

		/// <summary>
		/// Gets free/total capacities of FlashAir card.
		/// </summary>
		GetCapacity,

		/// <summary>
		/// Gets update status of FlashAir card.
		/// </summary>
		GetUpdateStatus,

		/// <summary>
		/// Gets time stamp of write event in FlashAir card.
		/// </summary>
		GetWriteTimeStamp,

		/// <summary>
		/// Gets Upload parameter of FlashAir card.
		/// </summary>
		GetUpload,

		/// <summary>
		/// Deletes a specified file in FlashAir card.
		/// </summary>
		DeleteFile,
	}
}
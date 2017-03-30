
namespace SnowyImageCopy.Models
{
	/// <summary>
	/// File status types of files in FlashAir card
	/// </summary>
	public enum FileStatus
	{
		Unknown = 0,

		/// <summary>
		/// File has not copied to local folder yet.
		/// </summary>
		NotCopied,

		/// <summary>
		/// File is to be copied to local folder.
		/// </summary>
		ToBeCopied,

		/// <summary>
		/// File is being copied now.
		/// </summary>
		/// <remarks>This type is seen during copying only.</remarks>
		Copying,

		/// <summary>
		/// File has been already copied to local folder.
		/// </summary>
		Copied,

		/// <summary>
		/// File looks weird.
		/// </summary>
		/// <remarks>This type is given when downloaded data length doesn't match file size provided
		/// remotely by FlashAir card.</remarks>
		Weird,

		/// <summary>
		/// File has been moved to Recycle.
		/// </summary>
		Recycled,
	}
}
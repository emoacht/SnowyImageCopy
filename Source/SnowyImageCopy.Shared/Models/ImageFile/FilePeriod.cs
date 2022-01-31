
namespace SnowyImageCopy.Models.ImageFile
{
	/// <summary>
	/// File period types to filter files in FlashAir card
	/// </summary>
	public enum FilePeriod
	{
		/// <summary>
		/// All files
		/// </summary>
		All,

		/// <summary>
		/// Files that date is today
		/// </summary>
		Today,

		/// <summary>
		/// Files that dates are recent dates
		/// </summary>
		Recent,

		/// <summary>
		/// Files that dates are selected dates
		/// </summary>
		Select,
	}
}
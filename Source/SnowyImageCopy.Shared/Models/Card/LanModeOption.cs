
namespace SnowyImageCopy.Models.Card
{
	/// <summary>
	/// LAN mode options of FlashAir
	/// </summary>
	public enum LanModeOption
	{
		None = 0,

		/// <summary>
		/// AP (Access Point) mode
		/// </summary>
		AccessPoint,

		/// <summary>
		/// STA (Station) mode
		/// </summary>
		Station,

		/// <summary>
		/// Internet pass-thru mode
		/// </summary>
		InternetPassThru,
	}
}
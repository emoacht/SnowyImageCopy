
namespace SnowyTool.Models
{
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
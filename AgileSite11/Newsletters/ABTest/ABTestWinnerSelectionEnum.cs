using System;
using System.Linq;
using System.Text;

namespace CMS.Newsletters
{
    /// <summary>
    /// Winner option enumeration
    /// </summary>
    public enum ABTestWinnerSelectionEnum : int
    {
        /// <summary>
        /// Winner will be selected according to open rate
        /// </summary>
        OpenRate = 0,

        /// <summary>
        /// Winner will be selected according to total clicks
        /// </summary>
        TotalUniqueClicks = 1,

        /// <summary>
        /// Winner will be selected manually
        /// </summary>
        Manual = 2
    }
}

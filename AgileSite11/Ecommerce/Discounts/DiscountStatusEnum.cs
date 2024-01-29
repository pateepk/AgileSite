using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Discount status enumerator.
    /// </summary>
    public enum DiscountStatusEnum
    {
        /// <summary>
        /// Discount is disabled.
        /// </summary>
        [EnumStringRepresentation("Disabled")]
        Disabled = 0,


        /// <summary>
        /// Discount is active.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("Active")]
        Active = 1,


        /// <summary>
        /// Discount has not started yet.
        /// </summary>
        [EnumStringRepresentation("NotStarted")]
        NotStarted = 2,


        /// <summary>
        /// Discount is finished.
        /// </summary>
        [EnumStringRepresentation("Finished")]
        Finished = 4,


        /// <summary>
        /// Discount is incomplete. (Uses coupons but no coupons created)
        /// </summary>
        [EnumStringRepresentation("Incomplete")]
        Incomplete = 6
    }
}
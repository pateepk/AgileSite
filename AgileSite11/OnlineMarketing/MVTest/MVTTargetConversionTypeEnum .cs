using System;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Target conversion type enumeration.
    /// </summary>
    public enum MVTTargetConversionTypeEnum
    {
        /// <summary>
        /// Actual conversions are compared with sum of all conversions for multivariate test
        /// </summary>
        Total = 0,

        /// <summary>
        /// Actual conversions are compared with each combination conversion value.
        /// </summary>
        AnyCombination = 1
    }
}
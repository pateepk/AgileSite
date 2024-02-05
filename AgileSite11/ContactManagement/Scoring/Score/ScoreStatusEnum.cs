using System;

namespace CMS.ContactManagement
{
    /// <summary>
    /// Types of score status used by ScoreInfo object.
    /// </summary>
    public enum ScoreStatusEnum
    {
        /// <summary>
        /// Unspecified.
        /// </summary>
        Unspecified = -1,


        /// <summary>
        /// Score is up to date.
        /// </summary>
        Ready = 0,


        /// <summary>
        /// Recalculating.
        /// </summary>
        Recalculating = 1,


        /// <summary>
        /// New, recalculation required.
        /// </summary>
        RecalculationRequired = 2,


        /// <summary>
        /// Recalculation failed.
        /// </summary>
        RecalculationFailed = 3
    }
}
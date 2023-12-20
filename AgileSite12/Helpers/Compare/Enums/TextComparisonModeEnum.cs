using System;

namespace CMS.Helpers
{
    /// <summary>
    /// Type of comparison in which should be compared text treated.
    /// </summary>
    public enum TextComparisonModeEnum : int
    {
        /// <summary>
        /// Default mode in which HTML is treated as not compared container.
        /// </summary>
        HTML = 0,

        /// <summary>
        /// Whole text is compared including possible HTML.
        /// </summary>
        PlainText = 1,

        /// <summary>
        /// Whole text is compared and no balacing is done.
        /// </summary>
        PlainTextWithoutFormating = 2
    }
}
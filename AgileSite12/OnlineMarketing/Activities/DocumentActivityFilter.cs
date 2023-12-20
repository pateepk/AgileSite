using CMS.Activities;
using CMS.DocumentEngine;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Filters out logging for pages with disabled logging.
    /// </summary>
    internal class DocumentActivityFilter : IActivityLogFilter
    {
        /// <summary>
        /// Filters out logging for pages with disabled logging
        /// </summary>
        /// <returns>Returns <c>true</c> if logging should be filtered out, because current document has disabled logging. Otherwise returns <c>false</c>.</returns>
        public bool IsDenied()
        {
            var pi = DocumentContext.CurrentPageInfo;
            if (pi != null)
            {
                return !pi.DocumentLogActivity;
            }

            return false;
        }
    }
}
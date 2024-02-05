using CMS.Activities;
using CMS.ContactManagement.Internal;

namespace CMS.OnlineMarketing
{
    /// <summary>
    /// Filters out logging when processing of contacts (and therefore the activities) cannot continue.
    /// </summary>
    internal class ContactProcessingCheckerActivityLogFilter : IActivityLogFilter
    {
        private readonly IContactProcessingChecker mContactProcessingChecker;


        public ContactProcessingCheckerActivityLogFilter(IContactProcessingChecker contactProcessingChecker)
        {
            mContactProcessingChecker = contactProcessingChecker;
        }


        /// <summary>
        /// Filters out logging for crawlers.
        /// </summary>
        /// <returns>Returns <c>true</c> if logging should be filtered out, because processing of contacts cannot continue. Otherwise returns <c>false</c>.</returns>
        public bool IsDenied()
        {
            return !mContactProcessingChecker.CanProcessContactInCurrentContext();
        }
    }
}

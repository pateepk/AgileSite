using System;

using CMS.Helpers;

namespace CMS.OnlineMarketing.Internal
{
    /// <summary>
    /// Holds data related to the current request AB visit logging.
    /// </summary>
    /// <remarks>
    /// This helper class is designed for output cache support in MVC. ABVisit is stored in ASP.NET session. Session creates cookie when something is stored.
    /// Set-Cookie causes that output cache response is not cached. This helper ensures that session is created within a separate request.
    /// </remarks>
    public static class ABVisitRequestHelper
    {
        private const string ABVISIT_TEST_KEY = "CMSABVisitRequestHelperKey";

        /// <summary>
        /// Indicates whether <see cref="ABVisitRequestHelper"/> should be used.
        /// </summary>
        public static bool ABVisitRequestEnabled { get; set; }


        /// <summary>
        /// Gets the test name used for ABVisit hit within current request.
        /// </summary>
        public static string ABVisitRequestTestName
        {
            get
            {
                return Convert.ToString(CMSHttpContext.Current?.Items[ABVISIT_TEST_KEY]);
            }
        }


        /// <summary>
        /// Sets the current ABVisit test name to the request store
        /// </summary>
        /// <param name="testName">A/B test name</param>
        /// <returns><c>true</c> when stored successfully to the request store; otherwise <c>false</c>.</returns>
        internal static bool SetABVisitRequestTestName(string testName)
        {
            if (!ABVisitRequestEnabled)
            {
                return false;
            }

            var items = CMSHttpContext.Current?.Items;
            if (items != null)
            {
                items[ABVISIT_TEST_KEY] = testName;
                return true;
            }

            return false;
        }
    }
}

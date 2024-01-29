using System;
using System.Web;

using CMS.Activities;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.Core.Internal;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides methods for logging pages activities.
    /// </summary>
    public class PagesActivityLogger
    {
        private readonly IActivityLogService mActivityLogService = Service.Resolve<IActivityLogService>();
        private readonly IDateTimeNowService mDateTimeNowService = Service.Resolve<IDateTimeNowService>();

        /// <summary>
        /// Logs page visit.
        /// </summary>
        /// <param name="documentName">Name of document where activity occurred</param>
        /// <param name="currentDocument">Current page to log visit for</param>
        /// <param name="attachmentName">Attachment in the page</param>
        /// <param name="activityUrl">URL where activity occurred</param>
        /// <param name="referrerUrl">URL referrer</param>
        public void LogPageVisit(string documentName, ITreeNode currentDocument = null, string attachmentName = null, string activityUrl = null, string referrerUrl = null)
        {
            var userLoginActivity = new PageVisitActivityInitializer(documentName, currentDocument, attachmentName, activityUrl, referrerUrl);
            mActivityLogService.Log(userLoginActivity, GetCurrentRequest());
        }


        /// <summary>
        /// Logs landing page activity.
        /// </summary>
        /// <param name="documentName">Name of document where activity occurred</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        /// <param name="activityUrl">URL where activity occurred</param>
        /// <param name="referrerUrl">URL referrer</param>
        public void LogLandingPage(string documentName, ITreeNode currentDocument = null, string activityUrl = null, string referrerUrl = null)
        {
            var cookie = CookieHelper.GetExistingCookie(CookieName.LandingPageLoaded);
            if (cookie == null)
            {
                var landingPageActivity = new LandingPageActivityInitializer(documentName, currentDocument, activityUrl, referrerUrl);
                mActivityLogService.Log(landingPageActivity, GetCurrentRequest());
            }

            CookieHelper.SetValue(CookieName.LandingPageLoaded, "true", mDateTimeNowService.GetDateTimeNow().AddMinutes(20));
        }


        /// <summary>
        /// Logs internal search activity.
        /// </summary>
        /// <param name="searchKeyword">Searched keyword</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        public void LogInternalSearch(string searchKeyword, ITreeNode currentDocument = null)
        {
            var internalSearchActivityInitializer = new InternalSearchActivityInitializer(searchKeyword, currentDocument);
            mActivityLogService.Log(internalSearchActivityInitializer, GetCurrentRequest());
        }


        /// <summary>
        /// Logs external search activity.
        /// </summary>
        /// <param name="referer">URL referer for current request.</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        /// <param name="activityUrl">URL where activity occurred</param>
        /// <param name="referrerUrl">URL referrer</param>
        public void LogExternalSearch(Uri referer, ITreeNode currentDocument = null, string activityUrl = null, string referrerUrl = null)
        {
            var externalSearchData = ExternalSearchData.Get(referer);

            if (externalSearchData != null)
            {
                var initializer = new ExternalSearchActivityInitializer(externalSearchData.Keyword, currentDocument, activityUrl, referrerUrl);
                mActivityLogService.Log(initializer, GetCurrentRequest());
            }
        }


        /// <summary>
        /// Returns current request.
        /// </summary>
        /// <returns>Current request.</returns>
        protected virtual HttpRequestBase GetCurrentRequest()
        {
            return CMSHttpContext.Current.Request;
        }
    }
}

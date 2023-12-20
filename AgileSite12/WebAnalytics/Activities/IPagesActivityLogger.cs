using System;

using CMS.Base;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Provides possibility to log pages activities.
    /// </summary>
    public interface IPagesActivityLogger
    {
        /// <summary>
        /// Logs page visit.
        /// </summary>
        /// <param name="documentName">Name of document where activity occurred</param>
        /// <param name="currentDocument">Current page to log visit for</param>
        /// <param name="attachmentName">Attachment in the page</param>
        /// <param name="activityUrl">URL where activity occurred</param>
        /// <param name="referrerUrl">URL referrer</param>
        void LogPageVisit(string documentName, ITreeNode currentDocument = null, string attachmentName = null, string activityUrl = null, string referrerUrl = null);


        /// <summary>
        /// Logs landing page activity.
        /// </summary>
        /// <param name="documentName">Name of document where activity occurred</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        /// <param name="activityUrl">URL where activity occurred</param>
        /// <param name="referrerUrl">URL referrer</param>
        void LogLandingPage(string documentName, ITreeNode currentDocument = null, string activityUrl = null, string referrerUrl = null);


        /// <summary>
        /// Logs internal search activity.
        /// </summary>
        /// <param name="searchKeyword">Searched keyword</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        void LogInternalSearch(string searchKeyword, ITreeNode currentDocument = null);


        /// <summary>
        /// Logs external search activity.
        /// </summary>
        /// <param name="referer">URL referer for current request.</param>
        /// <param name="currentDocument">Specifies the document node the activity is logged for</param>
        /// <param name="activityUrl">URL where activity occurred</param>
        /// <param name="referrerUrl">URL referrer</param>
        void LogExternalSearch(Uri referer, ITreeNode currentDocument = null, string activityUrl = null, string referrerUrl = null);
    }
}
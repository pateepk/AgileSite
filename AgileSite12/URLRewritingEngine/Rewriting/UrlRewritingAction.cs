using System;

using CMS.Helpers;

namespace CMS.URLRewritingEngine
{
    /// <summary>
    /// URL rewriting action storage for caching.
    /// </summary>
    public class UrlRewritingAction
    {
        /// <summary>
        /// Rewriting result.
        /// </summary>
        public RequestStatusEnum Status = RequestStatusEnum.Unknown;


        /// <summary>
        /// Rewriting page path.
        /// </summary>
        public string RewritePath = null;


        /// <summary>
        /// Rewriting page query.
        /// </summary>
        public string RewriteQuery = "";


        /// <summary>
        /// Redirection URL.
        /// </summary>
        public string RedirectURL = null;
    }
}
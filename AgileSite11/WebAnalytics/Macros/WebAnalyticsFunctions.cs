using System.Web;

using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Web analytics marketing functions.
    /// </summary>
    public static class WebAnalyticsFunctions
    {
        /// <summary>
        /// Returns true if current visitor is returning.
        /// </summary>
        public static bool IsReturningVisitor()
        {
            return AnalyticsContext.CurrentVisitStatus == VisitorStatusEnum.MoreVisits;
        }


        /// <summary>
        /// Returns true if current visitor comes to the website for the first time.
        /// </summary>
        public static bool IsFirstTimeVisitor()
        {
            return AnalyticsContext.CurrentVisitStatus == VisitorStatusEnum.FirstVisit;
        }


        /// <summary>
        /// Returns search keywords from search engine visitor came from.
        /// </summary>
        public static string GetSearchEngineKeyword()
        {
            if ((HttpContext.Current == null) || (HttpContext.Current.Request.UrlReferrer == null))
            {
                return null;
            }

            string searchKeyword;

            SearchEngineAnalyzer.GetSearchEngineFromUrl(HttpContext.Current.Request.UrlReferrer.AbsoluteUri, out searchKeyword);

            return searchKeyword;
        }


        /// <summary>
        /// Returns search engine visitor came from.
        /// </summary>
        public static string GetSearchEngine()
        {
            if ((HttpContext.Current == null) || (HttpContext.Current.Request.UrlReferrer == null))
            {
                return null;
            }

            string searchKeyword;

            var sei = SearchEngineAnalyzer.GetSearchEngineFromUrl(HttpContext.Current.Request.UrlReferrer.AbsoluteUri, out searchKeyword);
            return (sei != null) ? sei.SearchEngineDisplayName : "";
        }


        /// <summary>
        /// Returns absolute URI of the URLRefferer from current HTTP context.
        /// </summary>
        public static string GetUrlReferrer()
        {
            if ((HttpContext.Current == null) || (HttpContext.Current.Request.UrlReferrer == null))
            {
                return null;
            }

            return HttpContext.Current.Request.UrlReferrer.AbsoluteUri;
        }


        /// <summary>
        /// Returns value of specified URLReferrer query string parameter.
        /// </summary>
        /// <param name="parameterName">Query string parameter name</param>
        public static string GetUrlReferrerParameter(string parameterName)
        {
            string url = GetUrlReferrer();
            if (!string.IsNullOrEmpty(url))
            {
                return URLHelper.GetQueryValue(url, parameterName);
            }
            return null;
        }
    }
}
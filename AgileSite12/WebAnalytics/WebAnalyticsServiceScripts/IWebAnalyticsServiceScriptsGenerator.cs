using System;

using CMS.DocumentEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Service for generating JavaScript code which is able to call the external web service via AJAX. Before using the generated JS snippet, you need
    /// to manually create global WebServiceCall(url, method, parameters) JavaScript function either by calling ScriptHelper.RegisterWebServiceCallFunction(page) or 
    /// manually.
    /// </summary>
    public interface IWebAnalyticsServiceScriptsGenerator
    {
        /// <summary>
        /// Returns script which logs BannerHit asynchronously.
        /// </summary>
        /// <param name="bannerID">ID of the banner which was viewed</param>
        string GetLogBannerHitScript(int bannerID);


        /// <summary>
        /// Returns script which logs Search statistics asynchronously.
        /// </summary>
        /// <param name="pageInfo">PageInfo representing current document/node</param>
        /// <param name="searchText">Text which was searched for</param>
        string GetLogSearchScript(PageInfo pageInfo, string searchText);


        /// <summary>
        /// Returns script which logs page views and other hits asynchronously. The script does not contain the actual parameters.
        /// There is a placeholder at the location where parameters would be and this placeholder should be replaced by the actual parameters
        /// before sending page output to the client browser. This two-step rendering is required, because of the output cache mechanism.
        /// At first, there is a universal script rendered and saved into cache. This universal script is then updated with values specific
        /// for the current request before sending HTTP response back to the client browser.
        /// </summary>
        /// <param name="substitute">String which will be placed at the location where parameters would normally be</param>
        string GetLogHitsScriptWithSubstitute(string substitute);


        /// <summary>
        /// Returns parameters of the LogHits call in JSON format.
        /// </summary>
        /// <param name="pageInfo">PageInfo representing current document/node. Cannot be null</param>
        /// <param name="urlReferrer">Url of the page where the user came from (http referrer). Can be null</param>
        string GetLogHitsParameters(PageInfo pageInfo, string urlReferrer);
    }
}

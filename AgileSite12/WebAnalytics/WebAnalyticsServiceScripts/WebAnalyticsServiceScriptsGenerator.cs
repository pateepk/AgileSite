using System;
using System.Collections.Generic;

using CMS;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.WebAnalytics;

using Newtonsoft.Json;

[assembly:RegisterImplementation(typeof(IWebAnalyticsServiceScriptsGenerator), typeof(WebAnalyticsServiceScriptsGenerator), Priority = CMS.Core.RegistrationPriority.SystemDefault)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Service for generating JavaScript code which is able to call the external web service via AJAX. Before using the generated JS snippet, you need
    /// to manually create global WebServiceCall(url, method, parameters) JavaScript function either by calling ScriptHelper.RegisterWebServiceCallFunction(page) or
    /// manually.
    /// </summary>
    internal class WebAnalyticsServiceScriptsGenerator : IWebAnalyticsServiceScriptsGenerator
    {
        /// <summary>
        /// Node alias path of the page the visitor requested.
        /// </summary>
        public const string PARAM_PAGE_NODE_ALIAS_PATH = "NodeAliasPath";


        /// <summary>
        /// Culture code of the page the visitor requested.
        /// </summary>
        public const string PARAM_PAGE_CULTURE_CODE = "DocumentCultureCode";


        /// <summary>
        /// Referrer query parameter name.
        /// </summary>
        public const string PARAM_REFERRER = "UrlReferrer";


        /// <summary>
        /// Returns script which logs BannerHit asynchronously.
        /// </summary>
        /// <param name="bannerID">ID of the banner which was viewed</param>
        public string GetLogBannerHitScript(int bannerID)
        {
            return GetCallScript("LogBannerHit", bannerID.ToString());
        }


        /// <summary>
        /// Returns script which logs Search statistics asynchronously.
        /// </summary>
        /// <param name="pageInfo">PageInfo representing current document/node</param>
        /// <param name="searchText">Text which was searched for</param>
        public string GetLogSearchScript(PageInfo pageInfo, string searchText)
        {
            if (pageInfo == null)
            {
                throw new ArgumentNullException("pageInfo");
            }
            if (searchText == null)
            {
                throw new ArgumentNullException("searchText");
            }

            string parametersJson = JsonConvert.SerializeObject(new
            {
                Keyword = searchText,
                NodeAliasPath = pageInfo.NodeAliasPath,
                DocumentCultureCode = pageInfo.DocumentCulture,
            },
            new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });

            return GetCallScript("LogSearchHit", parametersJson);
        }


        /// <summary>
        /// Returns script which logs page views and other hits asynchronously. The script does not contain the actual parameters.
        /// There is a placeholder at the location where parameters would be and this placeholder should be replaced by the actual parameters
        /// before sending page output to the client browser. This two-step rendering is required, because of the output cache mechanism.
        /// At first, there is a universal script rendered and saved into cache. This universal script is then updated with values specific
        /// for the current request before sending HTTP response back to the client browser.
        /// </summary>
        /// <param name="substitute">String which will be placed at the location where parameters would normally be</param>
        public string GetLogHitsScriptWithSubstitute(string substitute)
        {
            string parametersJson = "{~" + substitute + "~}";

            return GetCallScript("LogHit", parametersJson);
        }


        /// <summary>
        /// Returns parameters of the LogHits call in JSON format.
        /// </summary>
        /// <param name="pageInfo">PageInfo representing current document/node. Cannot be null</param>
        /// <param name="urlReferrer">Url of the page where the user came from (http referrer). Can be null</param>
        public string GetLogHitsParameters(PageInfo pageInfo, string urlReferrer)
        {
            if (pageInfo == null)
            {
                throw new ArgumentNullException("pageInfo");
            }

            // Create dictionary of parameters to be sent back to the analytics logging service via Ajax request
            var queryParams = new Dictionary<string, string>();

            using (var h = WebAnalyticsEvents.InsertAnalyticsJS.StartEvent(queryParams))
            {
                if (h.CanContinue())
                {
                    queryParams.Add(PARAM_PAGE_NODE_ALIAS_PATH, pageInfo.NodeAliasPath);
                    queryParams.Add(PARAM_PAGE_CULTURE_CODE, pageInfo.DocumentCulture);

                    // Unescaped quotes may result in error during json deserialization
                    queryParams.Add(PARAM_REFERRER, (EscapeQuotes(urlReferrer) ?? ""));

                    h.FinishEvent();
                }
            }

            return JsonConvert.SerializeObject(queryParams, new JsonSerializerSettings { StringEscapeHandling = StringEscapeHandling.EscapeHtml });
        }


        private string GetCallScript(string method, string parametersJson)
        {
            string url = URLHelper.ResolveUrl("~/cmsapi/webanalytics");

            // Parameters JSON string has to be escaped twice, because it is going to be rendered as JavaScript string enclosed in quotes
            return String.Format("WebServiceCall('{0}', '{1}', '{2}')", url, method, parametersJson);
        }


        private static string EscapeQuotes(string url)
        {
            if (String.IsNullOrEmpty(url))
            {
                return url;
            }

            return url.Replace(@"\""", @"""").Replace(@"""", @"\""");
        }
    }
}

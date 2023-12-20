using System;
using System.Web.UI;

using CMS.Activities;
using CMS.Base;
using CMS.Base.Web.UI;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.OutputFilter;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMS.WebAnalytics.Web.UI
{
    /// <summary>
    /// Contains methods for registering JavaScript code into page which logs web analytics statistics asynchronously via call to WebAnalyticsService.
    /// LogHits call is registered automatically for each page request. Other calls have to be registered manually.
    /// </summary>
    public static class WebAnalyticsServiceScriptsRenderer
    {
        /// <summary>
        /// String to put into HTML to JavaScript code before substitution resolving.
        /// </summary>
        private const string JAVASCRIPT_QUERYPARAMS_SUBSTITUTE = "JAVASCRIPTQUERYPARAMSSUBSTITUTE";


        private static IWebAnalyticsServiceScriptsGenerator Generator => Service.Resolve<IWebAnalyticsServiceScriptsGenerator>();


        /// <summary>
        /// Registers script which logs banner hit asynchronously.
        /// </summary>
        /// <param name="page">Page where script should be registered</param>
        /// <param name="bannerID">Banner which was viewed</param>
        public static void RegisterLogBannerHitCall(Page page, int bannerID)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }

            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            string script = Generator.GetLogBannerHitScript(bannerID);
            
            RegisterScriptCall(page, script);
        }


        /// <summary>
        /// Registers script which logs search hit asynchronously.
        /// </summary>
        /// <param name="page">Page where script should be registered</param>
        /// <param name="pageInfo">PageInfo representing current document/node</param>
        /// <param name="searchText">Text which was searched for</param>
        public static void RegisterLogSearchCall(Page page, PageInfo pageInfo, string searchText)
        {
            if (page == null)
            {
                throw new ArgumentNullException(nameof(page));
            }
            if (pageInfo == null)
            {
                throw new ArgumentNullException(nameof(pageInfo));
            }
            if (searchText == null)
            {
                throw new ArgumentNullException(nameof(searchText));
            }

            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            string script = Generator.GetLogSearchScript(pageInfo, searchText);

            RegisterScriptCall(page, script);
        }


        /// <summary>
        /// Binds to appropriate events, so that LogHits call can be rendered on every page request.
        /// </summary>
        internal static void Init()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                ResponseOutputFilter.OnResolveSubstitution += ResolveSubstitution;

                // Handle the PreRender event to insert JavaScript snippet to the current page
                // Use PageContext's event because the Page isn't available on AcquireRequestState yet
                RequestEvents.PostAcquireRequestState.Execute += (sender, e) =>
                {
                    PageContext.PreRender += RegisterLogHitsCall;
                };
            }
        }


        /// <summary>
        /// Registers script which logs page views and other hits asynchronously. The script is registered without the actual parameters.
        /// There is a placeholder at the location where parameters would be and this placeholder should be replaced by the actual parameters
        /// before sending page output to the client browser. This two-step rendering is required, because of the output cache mechanism.
        /// At first, there is a universal script rendered and 
        /// </summary>
        private static void RegisterLogHitsCall(object sender, EventArgs e)
        {
            if (ShouldInsertAnalyticsJS(SiteContext.CurrentSiteName, RequestContext.CurrentDomain))
            {
                string script = Generator.GetLogHitsScriptWithSubstitute(JAVASCRIPT_QUERYPARAMS_SUBSTITUTE);

                RegisterScriptCall(PageContext.CurrentPage, script);
            }
        }


        /// <summary>
        /// Checks whether the JavaScript logging snippet should be added to the page.
        /// </summary>
        private static bool ShouldInsertAnalyticsJS(string currentSiteName, string domain)
        {
            if (PortalContext.ViewMode != ViewModeEnum.LiveSite)
            {
                return false;
            }

            if (!AnalyticsHelper.JavascriptLoggingEnabled(currentSiteName))
            {
                return false;
            }

            if (!AnalyticsHelper.AnalyticsEnabled(currentSiteName) && !ActivitySettingsHelper.ActivitiesEnabledAndModuleLoaded(currentSiteName))
            {
                return false;
            }

            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return false;
            }

            // Check license
            if (!LicenseHelper.CheckFeature(domain, FeatureEnum.WebAnalytics))
            {
                return false;
            }

            if (!RequestContext.LogPageHit || RequestHelper.IsPostBack() || QueryHelper.GetBoolean(URLHelper.SYSTEM_QUERY_PARAMETER, false))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Resolves JavaScript parameters substitution macro.
        /// Substitution macro is used because of full page caching, JavaScript snippet is unique for each visitor, because of the Url Referrer parameter.
        /// </summary>
        private static void ResolveSubstitution(object sender, SubstitutionEventArgs e)
        {
            if (!e.Match)
            {
                if (e.Expression == JAVASCRIPT_QUERYPARAMS_SUBSTITUTE)
                {
                    var scriptsGenerator = Service.Resolve<IWebAnalyticsServiceScriptsGenerator>();

                    var currentPage = DocumentContext.CurrentPageInfo;
                    var urlReferrer = RequestContext.URLReferrer;

                    if (currentPage == null)
                    {
                        return;
                    }

                    string parametersJson = scriptsGenerator.GetLogHitsParameters(currentPage, urlReferrer);
                    
                    e.Match = true;
                    e.Result = parametersJson;
                }
            }
        }


        private static void RegisterScriptCall(Page page, string callScript)
        {
            ScriptHelper.RegisterWebServiceCallFunction(page);
            ScriptHelper.RegisterStartupScript(page, typeof(string), Guid.NewGuid().ToString(), callScript, true);
        }
    }
}

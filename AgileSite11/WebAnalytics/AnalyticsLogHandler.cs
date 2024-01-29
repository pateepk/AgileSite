using System;
using System.Web;
using System.Web.SessionState;

using CMS.Core;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Routing.Web;
using CMS.SiteProvider;
using CMS.WebAnalytics;

[assembly: RegisterHttpHandler("CMSModules/WebAnalytics/Pages/Content/AnalyticsLog.aspx", typeof(AnalyticsLogHandler), Order=1)]

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Handler for logging browser capabilities analytics retrieved by javasript.
    /// </summary>
    public class AnalyticsLogHandler : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Gets whether this handler can be reused for other request.
        /// </summary>
        /// <value>Always <c>True</c></value>
        public bool IsReusable => true;


        /// <summary>
        /// Processes the incoming HTTP request.
        /// </summary>
        public void ProcessRequest(HttpContext context)
        {
            // Do not log analytics if analytics are not enabled or request is not a POST request (security purpose).
            if (!AnalyticsHelper.IsLoggingEnabled(SiteContext.CurrentSiteName) || !RequestHelper.IsPostBack())
            {
                return;
            }

            if (!Service.Resolve<IAnalyticsConsentProvider>().HasConsentForLogging())
            {
                return;
            }

            var data = QueryHelper.GetString("data", string.Empty);
            var guid = ValidationHelper.GetString(SessionHelper.GetValue("BrowserCapatibilities"), string.Empty);
            var urlGuid = QueryHelper.GetString("guid", string.Empty);

            // Compares GUIDs to prevent false data
            if (!string.Equals(guid, urlGuid, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!string.IsNullOrEmpty(data))
            {
                var siteName = SiteContext.CurrentSiteName;
                var cultureCode = CultureHelper.GetPreferredCulture();
                var values = data.Split(';');
                if (values.Length == 7)
                {
                    var browserHelper = CMSDataContext.Current.BrowserHelper;

                    // Resolution
                    var xRes = ValidationHelper.GetInteger(values[0], 0);
                    var yRes = ValidationHelper.GetInteger(values[1], 0);
                    if (xRes > 0 && yRes > 0)
                    {
                        var res = xRes + "x" + yRes;
                        HitLogProvider.LogHit(HitLogProvider.SCREENRESOLUTION, siteName, cultureCode, res, 0);
                        browserHelper.ScreenResolution = res;
                    }

                    // Color depth
                    var colorDepth = ValidationHelper.GetInteger(values[2], 0);
                    if (colorDepth > 0)
                    {
                        var depth = colorDepth + "-bit";
                        HitLogProvider.LogHit(HitLogProvider.SCREENCOLOR, siteName, cultureCode, depth, 0);
                        browserHelper.ScreenColorDepth = depth;
                    }

                    // OS                
                    if (!string.IsNullOrEmpty(values[3]))
                    {
                        var name = string.Empty;
                        switch (values[3])
                        {
                            case "0":
                                name = "Unknown OS";
                                break;

                            case "1":
                                name = "Windows";
                                break;

                            case "2":
                                name = "Mac OS";
                                break;

                            case "3":
                                name = "UNIX";
                                break;

                            case "4":
                                name = "Linux";
                                break;

                            case "5":
                                name = "Solaris";
                                break;
                        }

                        if (name != string.Empty)
                        {
                            HitLogProvider.LogHit(HitLogProvider.OPERATINGSYSTEM, siteName, cultureCode, name, 0);
                            browserHelper.OperatingSystem = name;
                        }
                    }

                    // Silverlight
                    if (!string.IsNullOrEmpty(values[4]) && ValidationHelper.IsInteger(values[4]))
                    {
                        var hasSilverlight = (values[4] != "0");
                        var value = hasSilverlight ? "hs" : "ns";
                        HitLogProvider.LogHit(HitLogProvider.SILVERLIGHT, siteName, cultureCode, value, 0);
                        browserHelper.IsSilverlightInstalled = hasSilverlight;
                    }

                    // Java
                    if (!string.IsNullOrEmpty(values[5]) && ValidationHelper.IsBoolean(values[5]))
                    {
                        var hasJava = !values[5].Equals("false", StringComparison.OrdinalIgnoreCase);
                        var value = hasJava ? "hj" : "nj";
                        HitLogProvider.LogHit(HitLogProvider.JAVA, siteName, cultureCode, value, 0);
                        browserHelper.IsJavaInstalled = hasJava;
                    }

                    // Flash                
                    if (!string.IsNullOrEmpty(values[6]) && ValidationHelper.IsInteger(values[6]))
                    {
                        var hasFlash = (values[6] != "0");
                        var value = hasFlash ? "hf" : "nf";
                        HitLogProvider.LogHit(HitLogProvider.FLASH, siteName, cultureCode, value, 0);
                        browserHelper.IsFlashInstalled = hasFlash;
                    }
                }
            }
        }
    }
}

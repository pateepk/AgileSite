using System;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.EventLog;
using CMS.Helpers;
using CMS.PortalEngine;
using CMS.SiteProvider;
using CMS.UIControls;

namespace CMS.StrandsRecommender.Web.UI
{
    /// <summary>
    /// Contains handlers of the Strands recommendation Web UI module.
    /// </summary>
    internal class StrandsRecommenderWebUIHandlers
    {
        #region "Public methods"

        /// <summary>
        /// Connects handlers to events.
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                // Render tracking JavaScript when the request handler (Page) is created
                // This event is used because we need direct access to current page
                RequestEvents.AcquireRequestState.Execute += BindStrandsToPagePreRender;
            }
        }

        #endregion


        #region "Handlers"

        /// <summary>
        /// Bind RenderStrandsScripts method to page prerender event.
        /// </summary>
        private static void BindStrandsToPagePreRender(object sender, EventArgs eventArgs)
        {
            string currentSiteName = SiteContext.CurrentSiteName;
            var cookieLevelProvider = Service.Resolve<ICurrentCookieLevelProvider>();

            // Do nothing, if user has decided to forbid cookie usage to under Visitor level, because Strands library uses Cookies under the hood
            if (!StrandsSettings.IsStrandsEnabled(currentSiteName) || 
                !PortalContext.ViewMode.IsLiveSite() || 
                (cookieLevelProvider.GetCurrentCookieLevel() < CookieLevel.Visitor))
            {
                return;
            }

            // Check for ContentPage instead of Page is needed to eliminate rendering in the UI, because 
            // PortalContext.ViewMode does not work when UI is not based on the Portal Engine
            var page = HttpContext.Current.CurrentHandler as ContentPage;

            // Do nothing if page is not ContentPage (can happen if this request is a web service request or page is in the old UI)
            if (page == null)
            {
                return;
            }

            page.PreRender += (o, args) =>
            {
                try
                {
                    var delayedRenderer = new StrandsTrackingScriptsManager(page, StrandsSettings.GetApiID(currentSiteName));
                    delayedRenderer.RenderStrandsScripts();
                }
                catch (Exception e)
                {
                    EventLogProvider.LogException("StrandsRecommenderWebUIHandlers", "Render Strands tracking scripts", e);
                }
            };
        }
    }

    #endregion
}

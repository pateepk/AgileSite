using System;
using System.Web;

using CMS.EventLog;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;

namespace CMS.UIControls
{
    /// <summary>
    /// Base class for the portal page template.
    /// </summary>
    public abstract class PortalPage : ContentPage
    {
        #region "Variables"

        /// <summary>
        /// If true, the document has been already redirected to the safe mode.
        /// </summary>
        protected bool mRedirectedToSafeMode = false;

        #endregion


        #region "Methods"

        


        /// <summary>
        /// Error handler.
        /// </summary>
        /// <param name="e">Event arguments</param>
        protected override void OnError(EventArgs e)
        {
            Exception ex = HttpContext.Current.Server.GetLastError();

            var vm = PortalContext.ViewMode;

            // Check if the error should be processed
            bool process = !RequestHelper.IsCallback() && PortalContext.IsDesignMode(vm) || vm.IsOneOf(ViewModeEnum.DashboardWidgets, ViewModeEnum.Edit, ViewModeEnum.EditLive) && !mRedirectedToSafeMode;

            // Error on new design mode (not allowed to pass)
            if (process)
            {
                // Try to switch to the safe mode first
                if (!PortalHelper.SafeMode)
                {
                    // Log the exception
                    EventLogProvider.LogException("Content", "Design", ex);

                    mRedirectedToSafeMode = true;

                    string url = RequestContext.CurrentURL;
                    url = URLHelper.AddParameterToUrl(url, "safemode", "1");
                    URLHelper.Redirect(url);
                }
                else
                {
                    RequestContext.LogCurrentError = false;

                    // Produce a nicer error message
                    throw new Exception("There was an error processing the page. The error can be caused by the configuration of some component on the master page. Check the master page configuration or see event log for more details. Original message: " + ex.Message);
                }
            }
        }

        #endregion
    }
}
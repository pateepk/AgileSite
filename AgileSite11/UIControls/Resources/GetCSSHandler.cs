using System;
using System.Web;

using CMS.Base;
using CMS.Helpers;
using CMS.OutputFilter;
using CMS.Routing.Web;
using CMS.UIControls;

[assembly: RegisterHttpHandler("CMSPages/GetCSS.aspx", typeof(GetCSSHandler), Order = 1)]

namespace CMS.UIControls
{
    /// <summary>
    /// HttpHandler to process 
    /// </summary>
    internal class GetCSSHandler : GetFileHandler
    {
        #region "Methods"

        /// <summary>
        /// Processes the incoming HTTP request that and returns the specified stylesheets.
        /// </summary>
        /// <param name="context">An HTTPContext object that provides references to the intrinsic server objects used to service HTTP requests</param>
        protected override void ProcessRequestInternal(HttpContext context)
        {
            if (!DebugHelper.DebugResources)
            {
                DisableDebugging();
            }

            // Transfer the execution to the newsletter page if newsletter template
            var newsletterTemplateName = QueryHelper.GetString("newslettertemplatename", String.Empty);
            if (newsletterTemplateName != string.Empty)
            {
                var newsletterCSSHandler = new GetNewsletterCSSHandler();
                newsletterCSSHandler.ProcessRequest(context);
                return;
            }

            // Process all other request with resource handler
            var handler = new GetResourceHandler();

            handler.ProcessRequest(context);

            RequestHelper.EndResponse();
        }


        /// <summary>
        /// Disables all debugging for current page
        /// </summary>
        protected void DisableDebugging()
        {
            // Disable the debugging
            DebugHelper.DisableDebug();

            OutputFilterContext.LogCurrentOutputToFile = false;
        }

        #endregion
    }
}
using System;

using CMS.Base;

namespace CMS.WebServices
{
    /// <summary>
    /// Event handlers for web services
    /// </summary>
    internal class WebServicesHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                RequestEvents.End.Execute += HandleRESTAuthentication;
            }
        }


        /// <summary>
        /// Handles REST authentication within current request
        /// </summary>
        private static void HandleRESTAuthentication(object sender, EventArgs e)
        {
            RESTSecurityInvoker.HandleRESTAuthentication();          
        }
    }
}
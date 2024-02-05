using System;
using System.Linq;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WebDAV
{
    /// <summary>
    /// WebDAV module event handlers
    /// </summary>
    internal class WebDAVHandlers
    {
        /// <summary>
        /// Initializes the handlers
        /// </summary>
        public static void Init()
        {
            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                RequestEvents.Begin.Execute += PropFindEndRequest;
            }
        }


        /// <summary>
        /// Ends the request if the request is a WebDAV PROPFIND request
        /// </summary>
        private static void PropFindEndRequest(object sender, EventArgs e)
        {
            // Check WebDAV PROPFIND request
            if (RequestHelper.IsWebDAVPropfindRequest())
            {
                RequestHelper.EndResponse();
            }
        }
    }
}

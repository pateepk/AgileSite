using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using CMS.PortalEngine.Web.UI;

namespace CMSApp.CMSWebParts.CUWebParts
{
    public partial class PreventAddToBrowserHistory : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // When HttpCacheability is set to NoCache or ServerAndNoCache 
            // the Expires HTTP header is set to -1 by default. This instructs 
            // the client to not cache responses in the History folder. Thus, 
            // each time you use the back/forward buttons, the client requests 
            // a new version of the response. 
            Response.Cache.SetCacheability(HttpCacheability.ServerAndNoCache);

            Response.Cache.SetNoStore();

            // Override the ServerAndNoCache behavior by setting the SetAllowInBrowserHistory 
            // method to true. This directs the client browser to store responses in  
            // its History folder.
            Response.Cache.SetAllowResponseInBrowserHistory(false);
        }
    }
}
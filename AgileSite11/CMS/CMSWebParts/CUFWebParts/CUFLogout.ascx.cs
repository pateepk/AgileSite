using System;
using System.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.PortalEngine.Web.UI;
using CMS.PortalEngine;
using CMS.SiteProvider;

namespace CMSApp.CMSWebParts.CUFWebParts
{
    public partial class CUFLogout : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack && !IsDesign && Visible)
            {
                string ret = "user not authenticated";
                if (AuthenticationHelper.IsAuthenticated())
                {
                    bool loadUserData = false;
                    string username = AuthenticationHelper.GetCurrentUser(out loadUserData).UserName;
                    //AuthenticationHelper.LogoutUser();
                    ret = string.Format("{0} logged out", username);
                }

                //return ret;
            }
        }
    }
}
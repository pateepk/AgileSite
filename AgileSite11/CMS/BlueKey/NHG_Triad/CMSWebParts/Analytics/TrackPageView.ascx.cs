using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.Membership;
using CMS.WebAnalytics;
using CMS.DeviceProfiles;
using CMS.SiteProvider;

//using BlueKey;

namespace NHG_T
{
    public partial class BlueKey_CMSWebParts_TrackPageView : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                //build image 
                imgTrackPageView.ImageUrl = "/bluekey/cmswebparts/analytics/view.aspx?";

                string imgVal = "";

                imgVal += "DateViewed=" + DateTime.Now.ToString();
                imgVal += "&UserHostAddress=" + System.Web.HttpContext.Current.Request.UserHostAddress;
                imgVal += "&UserID=" + CurrentUser.UserID;
                imgVal += "&IsAuthenticated=" + AuthenticationHelper.IsAuthenticated().ToString();
                imgVal += "&IsReturningVisitor=" + AnalyticsContext.IsNewVisitor.ToString(); //reversed intentionally
                imgVal += "&IsNewVisitor=" + AnalyticsContext.IsReturningVisitor.ToString(); //reversed intentionally 

                imgVal += "&Browser=" + BrowserHelper.GetBrowser();
                imgVal += "&UserAgent=" + BrowserHelper.GetUserAgent();
                imgVal += "&IsCrawler=" + BrowserHelper.IsCrawler().ToString();

                imgVal += "&IsMobile=" + DeviceContext.CurrentDevice.IsMobile().ToString();
                imgVal += "&IsTablet=false";// + DeviceContext.CurrentDevice.IsTablet.ToString();

                imgVal += "&DocumentID=" + CurrentDocument.DocumentID;
                imgVal += "&PageViewURL=" + System.Web.HttpContext.Current.Request.RawUrl;

                imgVal += "&SiteID=" + SiteContext.CurrentSiteID;

                imgTrackPageView.ImageUrl += imgVal;

            }

        }

    }
}
using System;
using System.Data;

using CMS.DocumentEngine.Web.UI;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace NHG_T
{
    public partial class CMSWebParts_MoveInSpecial : CMSAbstractBaseFilterControl
    {
        #region "Properties"

        public bool FilterByQuery = true;

        #endregion


        #region "Methods"

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!RequestHelper.IsPostBack())
            {
                // Populate Filter Drop Downs
                PopulateContent();

            }

        }


        protected void PopulateContent()
        {

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            TreeProvider tree = new TreeProvider(ui);

            DataSet ds = null;

            string url = RequestContext.RawURL;
            string listing = URLHelper.GetUrlParameter(url, "listing");

            if (listing != null)
            {

                ds = tree.SelectNodes(SiteContext.CurrentSite.SiteName, "/%", "en-us", true, "custom.Listing", "ListingID = " + listing);

                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    ltlMoveInSpecial.Text = "<h1>" + dr["ListingTitle"] + "</h1>" + dr["ListingMoveInSpecialContent"].ToString();
                }

            }

        }


        #endregion
    }
}
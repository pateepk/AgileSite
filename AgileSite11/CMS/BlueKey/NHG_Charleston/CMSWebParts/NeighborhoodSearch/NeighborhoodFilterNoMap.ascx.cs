using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

using System.Diagnostics;
//using System.Windows.Forms;
using System.ComponentModel;

//using CMS.GlobalHelper;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.DocumentEngine.Web.UI;
using CMS.SiteProvider;
using CMS.Membership;

namespace NHG_C
{
    public partial class CMSWebParts_NeighborhoodFilterNoMap : CMSAbstractBaseFilterControl
    {
        #region "Properties"

        public bool FilterByQuery = true;

        protected int defaultCity = 0;
        protected int defaultPrice = 0;
        protected int defaultBuilder = 0;
        protected int defaultType = 0;
        protected int defaultLifestyle = 0;

        #endregion


        #region "Methods"

        protected void Page_Load(object sender, EventArgs e)
        {

            if (!RequestHelper.IsPostBack())
            {
                // Populate Filter Drop Downs
                PopulateFilter();

                if (this.FilterByQuery)
                {
                    GetFilter();
                }

            }

            SetFilter();

        }

        protected void PopulateFilter()
        {
            PopulateCities();
            PopulatePrices();
            PopulateBuilders();
            PopulateTypes();
            //PopulateNeighborhoods();
            PopulateLifestyles();
        }

        #region Population Methods



        protected void PopulateCities()
        {
            ddCity.Items.Insert(0, new ListItem("Any City", "0"));
            ddCity.Items.Insert(1, new ListItem("Pooler, GA", "1"));
            ddCity.Items.Insert(2, new ListItem("Savannah, GA", "1"));
            ddCity.Items.Insert(3, new ListItem("Moncks Corner, SC", "1"));
            ddCity.Items.Insert(4, new ListItem("N. Charleston, SC", "1"));
            ddCity.Items.Insert(5, new ListItem("Summerville, SC", "1"));
            ddCity.Items.Insert(6, new ListItem("Beaufort, SC", "1"));
            ddCity.Items.Insert(7, new ListItem("Goose Creek, SC", "1"));
            ddCity.Items.Insert(8, new ListItem("Charleston, SC", "1"));
            ddCity.Items.Insert(9, new ListItem("Johns Island, SC", "1"));
            ddCity.Items.Insert(10, new ListItem("Mt. Pleasant, SC", "1"));
            ddCity.Items.Insert(11, new ListItem("Dorchestor, SC", "1"));
            ddCity.Items.Insert(12, new ListItem("Bluffton, SC", "1"));
            ddCity.Items.Insert(13, new ListItem("St. Thomas Island, SC", "1"));
            ddCity.Items.Insert(14, new ListItem("Ladson, SC", "1"));
            ddCity.Items.Insert(15, new ListItem("James Island, SC", "1"));
            ddCity.Items.Insert(16, new ListItem("Hanahan, SC", "1"));
            ddCity.Items.Insert(17, new ListItem("Hollywood, SC", "1"));
            ddCity.Items.Insert(18, new ListItem("Ridgeville, SC", "1"));
        }

        protected void PopulatePrices()
        {
            ddPrice.Items.Insert(0, new ListItem("Any Price", "0"));
            ddPrice.Items.Insert(1, new ListItem("<$100,000", "0|100000"));
            ddPrice.Items.Insert(2, new ListItem("$100,001 - $150,000", "100000|150000"));
            ddPrice.Items.Insert(3, new ListItem("$150,001 - $200,000", "150000|200000"));
            ddPrice.Items.Insert(4, new ListItem("$200,001 - $250,000", "200000|250000"));
            ddPrice.Items.Insert(5, new ListItem("$250,001 - $300,000", "250000|300000"));
            ddPrice.Items.Insert(6, new ListItem("$300,001 - $400,000", "300000|400000"));
            ddPrice.Items.Insert(7, new ListItem("$400,001 - $500,000", "400000|500000"));
            ddPrice.Items.Insert(8, new ListItem("$500,001 - $600,000", "500000|600000"));
            ddPrice.Items.Insert(9, new ListItem("$600,001 - $750,000", "600000|750000"));
            ddPrice.Items.Insert(10, new ListItem("$750,001 - $1,000,000", "750000|1000000"));
            ddPrice.Items.Insert(11, new ListItem("$1,000,000+", "1000000|999999999"));

        }

        protected void PopulateBuilders()
        {
            ddBuilder.Items.Insert(0, new ListItem("Any Developer", "0"));

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;
            int i = 1;

            ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Developers", null, "DeveloperName ASC");

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddBuilder.Items.Insert(i, new ListItem(dr["DeveloperName"].ToString(), dr["DevelopersID"].ToString()));
                i++;
            }


        }

        protected void PopulateTypes()
        {
            ddType.Items.Insert(0, new ListItem("Any Type", "0"));
            ddType.Items.Insert(1, new ListItem("Condos/Lofts/Townhomes", "1"));
            ddType.Items.Insert(2, new ListItem("Single-Family Homes", "2"));
            ddType.Items.Insert(3, new ListItem("Homesites", "3"));
        }

        /**
        protected void PopulateNeighborhoods()
        {
            ddNeighborhood.Items.Insert(0, new ListItem("Neighborhood", "0"));

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.TreeEngine.TreeProvider tree = new CMS.TreeEngine.TreeProvider(ui);

            DataSet ds = null;
            int i = 1;

            ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Neighborhood", null, "NeighborhoodName ASC");

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddNeighborhood.Items.Insert(i, new ListItem(dr["NeighborhoodName"].ToString(), dr["NeighborhoodId"].ToString()));
                i++;
            }

        }
        **/

        protected void PopulateLifestyles()
        {
            ddLifestyle.Items.Insert(0, new ListItem("Any Lifestyle", "0"));
            ddLifestyle.Items.Insert(1, new ListItem("55+ Community", "1"));
            ddLifestyle.Items.Insert(2, new ListItem("Golf Community", "2"));
            ddLifestyle.Items.Insert(3, new ListItem("Planned Community", "3"));
            ddLifestyle.Items.Insert(4, new ListItem("Waterfront Community", "4"));
        }

        #endregion

        #region Filter Methods


        private void SetFilter()
        {
            string where = "1 = 1";
            string url = RequestContext.RawURL;

            // Build where condition according to dropdowns setings 
            if (ddCity.SelectedValue != this.defaultCity.ToString())
            {
                where += " AND NeighborhoodCity = '" + ddCity.SelectedValue + "' ";
            }

            if (ddPrice.SelectedValue != this.defaultPrice.ToString())
            {
                where += " AND NeighborhoodPriceRangeLow <= '" + ddPrice.SelectedValue + "' AND NeighborhoodPriceRangeHigh >= '" + ddPrice.SelectedValue + "' ";
            }

            if (ddBuilder.SelectedValue != this.defaultBuilder.ToString())
            {
                where += " AND NeighborhoodDevelopers = '" + ddBuilder.SelectedValue + "' ";
            }

            if (ddType.SelectedValue != this.defaultType.ToString())
            {
                where += " AND NeighborhoodTypes = '" + ddType.SelectedValue + "' ";
            }

            if (ddLifestyle.SelectedValue != this.defaultLifestyle.ToString())
            {
                where += " AND NeighborhoodLifestyle = '" + ddLifestyle.SelectedValue + "' ";
            }

            if (where != "")
            {
                // Set where condition
                this.WhereCondition = where;
            }


            // Filter changed event
            this.RaiseOnFilterChanged();
        }

        private void GetFilter()
        {

            string city = QueryHelper.GetString("city", "");
            string price = QueryHelper.GetString("price", "");
            string builder = QueryHelper.GetString("builder", "");
            string type = QueryHelper.GetString("type", "");
            string lifestyle = QueryHelper.GetString("lifestyle", "");

            // Set order if in query
            if (city != "")
            {
                try
                {
                    ddCity.SelectedValue = city.ToString();
                }
                catch { }
            }

            if (price != "")
            {
                try
                {
                    ddPrice.SelectedValue = price.ToString();
                }
                catch { }
            }

            if (builder != "")
            {
                try
                {

                    ddBuilder.SelectedValue = builder.ToString();
                }
                catch { }
            }

            if (type != "")
            {
                try
                {

                    ddType.SelectedValue = type.ToString();
                }
                catch { }
            }

            if (lifestyle != "")
            {
                try
                {

                    ddLifestyle.SelectedValue = lifestyle.ToString();
                }
                catch { }
            }


        }

        public void btnFilter_Click(object sender, EventArgs e)
        {
            if (this.FilterByQuery)
            {
                // Handle all query parameters
                string url = RequestContext.RawURL;

                url = URLHelper.RemoveParameterFromUrl(url, "city");
                url = URLHelper.RemoveParameterFromUrl(url, "price");
                url = URLHelper.RemoveParameterFromUrl(url, "builder");
                url = URLHelper.RemoveParameterFromUrl(url, "type");
                url = URLHelper.RemoveParameterFromUrl(url, "lifestyle");

                if (ddCity.SelectedValue != this.defaultCity.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "city", ddCity.SelectedValue);
                }

                if (ddPrice.SelectedValue != this.defaultPrice.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "price", ddPrice.SelectedValue);
                }

                if (ddBuilder.SelectedValue != this.defaultBuilder.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "builder", ddBuilder.SelectedValue);
                }

                if (ddType.SelectedValue != this.defaultType.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "type", ddType.SelectedValue);
                }

                if (ddLifestyle.SelectedValue != this.defaultLifestyle.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "lifestyle", ddLifestyle.SelectedValue);
                }

                // Redirect with new query parameters
                URLHelper.Redirect(url);
            }
            else
            {
                // Set filter settings
                SetFilter();
            }
        }

        #endregion

        #endregion
    }
}
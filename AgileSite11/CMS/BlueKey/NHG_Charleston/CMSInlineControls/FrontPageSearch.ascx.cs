using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.CustomTables;
using CMS.Base.Web.UI;
using CMS.Helpers;
using CMS.Membership;


namespace NHG_C
{
    public partial class CMSInlineControls_FrontPageSearch : InlineUserControl
    {

        protected int defaultCity = 0;
        protected int defaultPrice = 0;
        protected int defaultBuilder = 0;
        protected int defaultType = 0;
        protected int defaultLifestyle = 0;
        protected int defaultNeighborhood = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Populate Form Control
            if (!IsPostBack)
            {
                PopulateLists();
            }
        }

        protected void PopulateLists()
        {
            PopulateCities();
            PopulatePrices();
            PopulateBuilders();
            PopulateTypes();
            PopulateNeighborhoods();
            PopulateLifestyles();
        }


        protected void PopulateCities()
        {
            ddCity.Items.Clear();

            ddCity.Items.Insert(0, new ListItem("Any City", "0"));


            DataSet ds = CustomTableItemProvider.GetItems("customlocations.city", "CityVisible = 1", "CityName ASC", 0, "CityName, MAX(ItemID) as ItemID").GroupBy("CityName");

            int i = 1;

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddCity.Items.Insert(i, new ListItem(dr["CityName"].ToString(), dr["ItemID"].ToString()));
                i++;
            }




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
            ddPrice.Items.Insert(8, new ListItem("$500,001 - $750,000+", "500000|750000"));
            ddPrice.Items.Insert(9, new ListItem("$750,001+", "750000"));

        }

        protected void PopulateBuilders()
        {
            ddBuilder.Items.Insert(0, new ListItem("Any Builder", "0"));

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;
            int i = 1;

            ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Developers", "DeveloperActive = 1", "DeveloperName ASC");

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

        protected void PopulateNeighborhoods()
        {
            ddNeighborhood.Items.Insert(0, new ListItem("Any Neighborhood", "0"));

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;
            int i = 1;

            ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodActive = 1", "NeighborhoodName ASC");

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddNeighborhood.Items.Insert(i, new ListItem(dr["NeighborhoodName"].ToString(), dr["NeighborhoodId"].ToString()));
                i++;
            }

        }

        protected void PopulateLifestyles()
        {
            ddLifestyle.Items.Insert(0, new ListItem("Any Lifestyle", "0"));
            ddLifestyle.Items.Insert(1, new ListItem("55+ Community", "1"));
            ddLifestyle.Items.Insert(2, new ListItem("Golf Community", "2"));
            ddLifestyle.Items.Insert(3, new ListItem("Planned Community", "3"));
            ddLifestyle.Items.Insert(4, new ListItem("Waterfront Community", "4"));
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            string url = "/Builders-Neighborhoods.aspx";

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

            if (ddNeighborhood.SelectedValue != this.defaultNeighborhood.ToString())
            {
                url = URLHelper.AddParameterToUrl(url, "neighborhood", ddNeighborhood.SelectedValue);
            }


            // Redirect with new query parameters
            URLHelper.Redirect(url);

            //lblError.Text = "Search it now!";
        }
    }
}
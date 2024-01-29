using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace NHG_T
{
    public partial class BlueKey_CMSWebParts_MobileAdvancedSearchForm : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {

            if (!IsPostBack)
            {
                BuildForm();
            }

        }

        #region Form Building

        private void BuildForm()
        {

            BuildPriceRange();
            BuildBuilders();
            BuildLifestyles();
            BuildTypes();
            BuildLocations();

        }

        private void BuildLocations()
        {
            GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;
            QueryParameters qp = new QueryParameters("SELECT DISTINCT CityName, Min(ItemID) as ItemID FROM customlocations_city GROUP BY CityName ORDER BY CityName ASC", null, QueryTypeEnum.SQLQuery, false);
            ds = conn.ExecuteQuery(qp);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddLocation.DataSource = ds;
                ddLocation.DataTextField = "CityName";
                ddLocation.DataValueField = "ItemID";
                ddLocation.DataBind();

                ddLocation.Items.Insert(0, new ListItem("Current Location", "-1"));
            }

        }

        private void BuildPriceRange()
        {
            GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;

            QueryParameters qp = new QueryParameters("SELECT PriceFilter, PriceLabel FROM BK_Price", null, QueryTypeEnum.SQLQuery, false);
            ds = conn.ExecuteQuery(qp);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddPriceRange.DataSource = ds;
                ddPriceRange.DataTextField = "PriceLabel";
                ddPriceRange.DataValueField = "PriceFilter";
                ddPriceRange.DataBind();
            }
        }

        private void BuildBuilders()
        {
            GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;
            QueryParameters qp = new QueryParameters("SELECT DevelopersID, DeveloperName from View_custom_Developers_Joined WHERE DeveloperActive = 1", null, QueryTypeEnum.SQLQuery, false);
            ds = conn.ExecuteQuery(qp);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddBuilders.DataSource = ds;
                ddBuilders.DataTextField = "DeveloperName";
                ddBuilders.DataValueField = "DevelopersID";
                ddBuilders.DataBind();

                ddBuilders.Items.Insert(0, new ListItem("Any Builder", ""));

            }
        }

        private void BuildLifestyles()
        {
            GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;

           QueryParameters qp = new QueryParameters("SELECT ItemID, LifestyleName FROM BK_Lifestyle", null, QueryTypeEnum.SQLQuery, false);
            ds = conn.ExecuteQuery(qp);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddLifestyle.DataSource = ds;
                ddLifestyle.DataTextField = "LifestyleName";
                ddLifestyle.DataValueField = "ItemID";
                ddLifestyle.DataBind();

                ddLifestyle.Items.Insert(0, new ListItem("Any Lifestyle", ""));
            }
        }

        private void BuildTypes()
        {
            GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;

            QueryParameters qp = new QueryParameters("SELECT ItemID, TypeName FROM BK_Type", null, QueryTypeEnum.SQLQuery, false);
            ds = conn.ExecuteQuery(qp);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddType.DataSource = ds;
                ddType.DataTextField = "TypeName";
                ddType.DataValueField = "ItemID";
                ddType.DataBind();

                ddType.Items.Insert(0, new ListItem("Any Type", ""));
            }
        }

        #endregion

        #region Event Methods

        protected void btnAdvancedSearchSubmit_Click(object sender, EventArgs e)
        {
            string type = ValidationHelper.GetString(ddType.SelectedValue, "");
            string price_range = ValidationHelper.GetString(ddPriceRange.SelectedValue, "");
            string lifestyle = ValidationHelper.GetString(ddLifestyle.SelectedValue, "");
            string developer = ValidationHelper.GetString(ddBuilders.SelectedValue, "");
            string locationID = ValidationHelper.GetString(ddLocation.SelectedValue, "-1");

            string[] price_bits = price_range.Split('|');

            string location = String.Empty;

            if (locationID == "-1")
            {
                string lat = hdnLat.Value;
                string lon = hdnLon.Value;

                location = "&lat=" + lat + "&lon=" + lon;
            }
            else
            {
                location = "&city=" + locationID;
            }

            string price_low = "";
            string price_high = "";

            if (price_bits.Length > 1)
            {
                price_low = price_bits[0];
                price_high = price_bits[1];
            }

            if (price_low == "0") { price_low = ""; }
            if (price_high == "0") { price_high = ""; }

            string url = "/Mobile/Neighborhoods.aspx?type=" + type + "&lifestyle=" + lifestyle + "&price_low=" + price_low + "&price_high=" + price_high + "&builder=" + developer + location;


            Response.Redirect(url);

        }

        #endregion

    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DocumentEngine.Web.UI;
using CMS.CustomTables;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

//using BlueKey;

namespace NHG_C
{
    public partial class CMSWebParts_NeighborhoodFilter2_Lots : CMSAbstractBaseFilterControl
    {
        #region "Properties"

        public bool FilterByQuery = true;

        protected int defaultCity = 0;
        protected int defaultArea = 0;
        protected int defaultCounty = 0;
        protected int defaultPrice = 0;
        protected int defaultBuilder = 0;
        protected int defaultType = 0;
        protected int defaultLifestyle = 0;
        protected int defaultNeighborhood = 0;

        #endregion


        #region "Methods"

        protected void Page_Load(object sender, EventArgs e)
        {
            var arg = Request.Form["__EVENTTARGET"];

            if (arg != null)
            {
                string divID = (string)arg;
                if (divID == "btnClearFilters")
                {
                    ClearFilters();
                }

                if (divID == "btnFilter")
                {
                    Filter();
                }
            }
            if (!RequestHelper.IsPostBack())
            {
                // Populate Filter Drop Down
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
            //TODO: FIX THIS CONTROL
            PopulateCities();
            //PopulatePrices();
            PopulateBuilders();
            PopulateTypes();
            //PopulateNeighborhoods();
            PopulateLifestyles();
            PopulateAreas();
            PopulateCounties();
        }

        #region Population Methods

        protected void PopulateAreas()
        {
            ddArea.Items.Clear();
            ddArea.Items.Insert(0, new ListItem("Any Area", "0"));

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.area", "SiteName='" + SiteContext.CurrentSiteName + "'", "AreaName ASC");

            int i = 1;

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddArea.Items.Insert(i, new ListItem(dr["AreaName"].ToString(), dr["ItemID"].ToString()));
                i++;
            }
        }

        protected void PopulateCounties()
        {
            ddCounty.Items.Clear();

            ddCounty.Items.Insert(0, new ListItem("Any County", "0"));

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.county", "CountyVisible = 1  AND SiteName='" + SiteContext.CurrentSiteName + "' ", "CountyName ASC", 0, "CountyName, MAX(ItemID) as ItemID").GroupBy("CountyName");

            int i = 1;

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddCounty.Items.Insert(i, new ListItem(dr["CountyName"].ToString(), dr["ItemID"].ToString()));
                i++;
            }


        }

        protected void PopulateCities()
        {
            ddCity.Items.Clear();

            ddCity.Items.Insert(0, new ListItem("Any City", "0"));

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.city", "CityVisible = 1  AND SiteName='" + SiteContext.CurrentSiteName + "' ", "CityName ASC", 0, "CityName, MAX(ItemID) as ItemID").GroupBy("CityName");

            int i = 1;

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddCity.Items.Insert(i, new ListItem(dr["CityName"].ToString(), dr["ItemID"].ToString()));
                i++;
            }




        }

        public void ddArea_filterSetAreas(object obj, EventArgs sender)
        {
            int area = ValidationHelper.GetSafeInteger(ddArea.SelectedValue, 0);
            int county = ValidationHelper.GetSafeInteger(ddCounty.SelectedValue, 0);

            if (area != 0)
            {
                resetCounties(area);
                resetCitiesByArea(area);
            }
            else
            {
                PopulateCounties();
                PopulateCities();
            }
        }

        public void ddCounty_filterSetAreas(object obj, EventArgs sender)
        {
            int county = ValidationHelper.GetSafeInteger(ddCounty.SelectedValue, 0);

            if (county != 0)
            {
                resetCities(county);
            }
            else
            {
                PopulateCities();
            }
        }

        protected void resetCitiesByArea(int area)
        {
            if (area != 0)
            {
                DataSet ds = CustomTableItemProvider.GetItems("customlocations.area", "ItemID = '" + Convert.ToString(area) + "'", "AreaName ASC");

                DataTable dt = ds.Tables[0];

                DataRow row = dt.Rows[0];

                ddCity.Items.Clear();

                ddCity.Items.Insert(0, new ListItem("Any " + row["AreaName"] + " City", "0"));

                int i = 1;

                ds = CustomTableItemProvider.GetItems("customlocations.city", "CityCounty IN (SELECT ItemID FROM customlocations_county WHERE CountyArea = '" + Convert.ToString(area) + "' AND SiteName='" + SiteContext.CurrentSiteName + "' ) AND CityVisible = 1 ", "CityName ASC", 0, "CityName, MAX(ItemID) as ItemID").GroupBy("CityName");


                dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {

                    ddCity.Items.Insert(i, new ListItem(dr["CityName"].ToString(), dr["ItemID"].ToString()));
                    i++;
                }
            }
        }

        protected void resetCounties(int area)
        {
            if (area != 0)
            {
                DataSet ds = CustomTableItemProvider.GetItems("customlocations.area", "ItemID = '" + Convert.ToString(area) + "'", "AreaName ASC");

                DataTable dt = ds.Tables[0];

                DataRow row = dt.Rows[0];

                ds = CustomTableItemProvider.GetItems("customlocations.county", "CountyArea = '" + Convert.ToString(area) + "' AND CountyVisible = 1 AND SiteName='" + SiteContext.CurrentSiteName + "' ", "CountyName ASC");

                ddCounty.Items.Clear();

                ddCounty.Items.Insert(0, new ListItem("Any " + row["AreaName"] + " County", "0"));

                int i = 1;

                dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    ddCounty.Items.Insert(i, new ListItem(dr["CountyName"].ToString(), dr["ItemID"].ToString()));
                    i++;
                }
            }
        }

        protected void resetCities(int county)
        {
            if (county != 0)
            {
                DataSet ds = CustomTableItemProvider.GetItems("customlocations.county", "ItemID = '" + Convert.ToString(county) + "'", "CountyName ASC");

                DataTable dt = ds.Tables[0];

                DataRow row = dt.Rows[0];

                ds = CustomTableItemProvider.GetItems("customlocations.city", "CityCounty in (SELECT ItemID FROM customlocations_county WHERE CountyName = '" + row["CountyName"] + "') AND CityVisible = 1 AND SiteName='" + SiteContext.CurrentSiteName + "' ", "CityName ASC");

                //ddDebugLabel.Text = "CityCounty in (SELECT ItemID FROM customlocations_county WHERE CountyName = '" + row["CountyName"] + "') AND CityVisible = 1";
                ddCity.Items.Clear();

                ddCity.Items.Insert(0, new ListItem("Any " + row["CountyName"] + " City", "0"));

                int i = 1;

                dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    ddCity.Items.Insert(i, new ListItem(dr["CityName"].ToString(), dr["ItemID"].ToString()));
                    i++;
                }

            }
        }

        protected void PopulateBuilders()
        {
            ddBuilder.Items.Insert(0, new ListItem("Any Builder", "0"));

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            TreeProvider tree = new TreeProvider(ui);

            DataSet ds = null;
            int i = 1;

            ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DeveloperActive = 1", "DeveloperName ASC");

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddBuilder.Items.Insert(i, new ListItem(dr["DeveloperName"].ToString(), dr["DevelopersID"].ToString()));
                i++;
            }


        }

        protected void PopulateTypes()
        {
            // ddType.Items.Insert(0, new ListItem("Any Type", "0"));
            // ddType.Items.Insert(1, new ListItem("Condos/Lofts/Townhomes", "1"));
            // ddType.Items.Insert(2, new ListItem("Single-Family Homes", "2"));
            ddType.Items.Insert(0, new ListItem("Homesites", "3"));
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
            ddLifestyle.Items.Insert(1, new ListItem("Active Adult Community", "1"));
            ddLifestyle.Items.Insert(2, new ListItem("Golf Community", "2"));
            ddLifestyle.Items.Insert(3, new ListItem("Planned Community", "3"));
            ddLifestyle.Items.Insert(4, new ListItem("Low Maintenance Community", "4"));
        }

        #endregion

        #region Filter Methods

        private string getLocationFilterQueryByArea(int area)
        {
            string str_while = string.Empty;

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.county", "CountyArea = '" + Convert.ToString(area) + "' AND SiteName='" + SiteContext.CurrentSiteName + "' ", null);

            DataTable dt = ds.Tables[0];

            foreach (DataRow county in dt.Rows)
            {
                DataSet dsCities = CustomTableItemProvider.GetItems("customlocations.city", "CityCounty = '" + county["ItemID"].ToString() + "'", null);
                DataTable dtCities = dsCities.Tables[0];

                foreach (DataRow city in dtCities.Rows)
                {
                    string leadingOr = " OR ";
                    if (string.IsNullOrEmpty(str_while))
                    {
                        str_while += " AND (";
                        leadingOr = "";
                    }
                    str_while += leadingOr + "NeighborhoodCity LIKE '%" + city["CityName"].ToString() + "%'";

                }

            }

            str_while += (string.IsNullOrEmpty(str_while) ? "" : ")");

            return str_while;
        }

        private void ClearFilters()
        {
            string url = RequestContext.RawURL;

            if (url.Contains("?"))
            {
                string path = url.Substring(0, url.IndexOf("?"));

                Response.Redirect(path);

            }
        }

        private string getLocationFilterQueryByCounty(int county)
        {
            string str_while = string.Empty;

            DataSet dsCounty = CustomTableItemProvider.GetItems("customlocations.county", "ItemID = '" + Convert.ToString(county) + "' AND SiteName = '" + SiteContext.CurrentSiteName + "' ", null);
            DataTable dtCounties = dsCounty.Tables[0];

            if (dtCounties.Rows.Count > 0)
            {

                foreach (DataRow countyRow in dtCounties.Rows)
                {
                    string leadingOr = " OR ";
                    if (string.IsNullOrEmpty(str_while))
                    {
                        str_while += " AND (";
                        leadingOr = "";
                    }
                    str_while += leadingOr + "NeighborhoodCounty LIKE '" + countyRow["CountyName"].ToString() + "'";
                }

                str_while += (string.IsNullOrEmpty(str_while) ? "" : ")");
            }

            /**
            if (dtCities.Rows.Count > 0)
            {
                str_while += " AND (";

                foreach (DataRow city in dtCities.Rows)
                {
                    str_while += "NeighborhoodCity LIKE '" + city["CityName"].ToString() + "'";
                    str_while += " OR ";

                }

                str_while = str_while.Substring(0, str_while.Length - 4) + ")";
            }
            **/

            return str_while;
        }

        private string getLocationFilterQueryByCity(int cityID)
        {
            string str_while = string.Empty;

            DataSet dsCities = CustomTableItemProvider.GetItems("customlocations.city", "ItemID = '" + Convert.ToString(cityID) + "' AND SiteName = '" + SiteContext.CurrentSiteName + "' ", null);
            DataTable dtCities = dsCities.Tables[0];

            foreach (DataRow city in dtCities.Rows)
            {
                string leadingOr = " OR ";
                if (string.IsNullOrEmpty(str_while))
                {
                    str_while += " AND (";
                    leadingOr = "";
                }
                str_while += leadingOr + "NeighborhoodCity LIKE '%" + city["CityName"].ToString() + "%'";
            }

            str_while += (string.IsNullOrEmpty(str_while) ? "" : ")");

            return str_while;
        }

        private void SetFilter()
        {
            string where = "1 = 1";
            string url = RequestContext.RawURL;

            // Build where condition according to dropdowns setings 
            if (ddCity.SelectedValue != this.defaultCity.ToString())
            {
                where += this.getLocationFilterQueryByCity(ValidationHelper.GetSafeInteger(ddCity.SelectedValue, 0));
            }
            else if (ddCounty.SelectedValue != this.defaultCounty.ToString())
            {
                where += this.getLocationFilterQueryByCounty(ValidationHelper.GetSafeInteger(ddCounty.SelectedValue, 0));
            }
            else if (ddArea.SelectedValue != this.defaultArea.ToString())
            {
                where += this.getLocationFilterQueryByArea(ValidationHelper.GetSafeInteger(ddArea.SelectedValue, 0));
            }

            /*if (ddPrice.SelectedValue != this.defaultPrice.ToString())
            {
                string[] range = ddPrice.SelectedValue.Split('|');

                if (2 == range.Length)
                {
                    where += " AND (CONVERT(INT, REPLACE(REPLACE(REPLACE(NeighborhoodPriceRangeHigh, ',', ''), 's', ''), '+', '')) >= " + ValidationHelper.GetSafeInteger(range[0], 0) + " AND CONVERT(INT, REPLACE(REPLACE(REPLACE(NeighborhoodPriceRangeLow, ',', ''), 's', ''), '+', '')) <= " + ValidationHelper.GetSafeInteger(range[1], 0) + ") ";
                }
            }*/
            if ((QueryHelper.GetInteger("low", -1) > 0) || QueryHelper.GetInteger("high", -1) > 0)
            {
                string[] range = { ((ValidationHelper.GetInteger(hfLowValue.Value, 0) * 1000)).ToString(), (ValidationHelper.GetInteger(hfHighValue.Value, 0) * 1000).ToString() };
                where += " AND (CASE WHEN NeighborhoodPriceRangeHigh IS NULL THEN 999999999999 ELSE CONVERT(INT, REPLACE(REPLACE(REPLACE(NeighborhoodPriceRangeHigh, ',', ''), 's', ''), '+', '')) END >= " + Convert.ToInt32(range[0]) + " AND CASE WHEN NeighborhoodPriceRangeLow IS NULL THEN 0 ELSE CONVERT(INT, REPLACE(REPLACE(REPLACE(NeighborhoodPriceRangeLow, ',', ''), 's', ''), '+', '')) END <= " + Convert.ToInt32(range[1]) + ") ";
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

            if (chkCustom.Checked == true)
            {
                where += " AND NeighborhoodCustom = 1 ";
            }

            string neighborhood = QueryHelper.GetString("neighborhood", "");

            if (neighborhood != "")
            {
                where += " AND NeighborhoodID = '" + neighborhood + "' ";
            }

            if (where != "")
            {
                // Set where condition
                this.WhereCondition = where;
                //ddDebugLabel.Text = where;
            }

            if (!RequestHelper.IsPostBack() || QueryHelper.GetString("filter", string.Empty) == "1")
            {
                var documents = DocumentHelper.GetDocuments("custom.Neighborhood")
                                          .Path("/Neighborhoods/%")
                                          .Where(where + " AND NeighborhoodActive = 1")
                                          .OnCurrentSite()
                                          .Published(true)
                                          .OrderBy("NeighborhoodName ASC");
                ltlShowingCount.Text = documents.Count.ToString();

                var totalDocs = DocumentHelper.GetDocuments("custom.Neighborhood")
                                              .Path("/Neighborhoods/%")
                                              .Where("NeighborhoodActive = 1")
                                              .Where("NeighborhoodTypes = '3'")
                                              .OnCurrentSite()
                                              .Published(true)
                                              .OrderBy("NeighborhoodName ASC");
                ltlTotalCount.Text = totalDocs.Count.ToString();
            }

            // Filter changed event
            this.RaiseOnFilterChanged();
        }


        private void GetFilter()
        {

            string city = QueryHelper.GetString("city", "");
            string area = QueryHelper.GetString("area", "");
            string county = QueryHelper.GetString("county", "");
            //string price = QueryHelper.GetString("price", "");
            string low = QueryHelper.GetString("low", "");
            string high = QueryHelper.GetString("high", "");
            string builder = QueryHelper.GetString("builder", "");
            string type = QueryHelper.GetString("type", "");
            string lifestyle = QueryHelper.GetString("lifestyle", "");
            string custom = QueryHelper.GetString("custom", "");

            // Set order if in query
            if (area != "")
            {
                try
                {
                    resetCounties(ValidationHelper.GetSafeInteger(area, 0));
                    resetCitiesByArea(ValidationHelper.GetSafeInteger(area, 0));
                    ddArea.SelectedValue = area.ToString();
                }
                catch { }
            }
            if (county != "")
            {

                if (area != "")
                {
                    resetCounties(ValidationHelper.GetSafeInteger(area, 0));
                }

                resetCities(ValidationHelper.GetSafeInteger(county, 0));
                ddCounty.SelectedValue = county.ToString();

            }
            if (city != "")
            {
                try
                {
                    ddCity.SelectedValue = city.ToString();
                }
                catch { }
            }


            if (low != "")
            {
                try
                {
                    hfLowValue.Value = low;
                }
                catch { }
            }

            if (high != "")
            {
                try
                {
                    hfHighValue.Value = high;
                }
                catch { }
            }


            /*if (price != "")
            {
                try
                {
                    ddPrice.SelectedValue = price.ToString();
                }
                catch { }
            }*/

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

            if (custom != "")
            {
                try
                {
                    chkCustom.Checked = true;
                }
                catch { }
            }


        }

        public void btnFilter_Click(object sender, EventArgs e)
        {
            Filter();
        }

        private void Filter()
        {
            if (this.FilterByQuery)
            {
                // Handle all query parameters
                string url = RequestContext.RawURL;

                url = URLHelper.RemoveParameterFromUrl(url, "city");
                url = URLHelper.RemoveParameterFromUrl(url, "county");
                url = URLHelper.RemoveParameterFromUrl(url, "area");
                //url = URLHelper.RemoveParameterFromUrl(url, "price");
                url = URLHelper.RemoveParameterFromUrl(url, "low");
                url = URLHelper.RemoveParameterFromUrl(url, "high");
                url = URLHelper.RemoveParameterFromUrl(url, "builder");
                url = URLHelper.RemoveParameterFromUrl(url, "type");
                url = URLHelper.RemoveParameterFromUrl(url, "lifestyle");
                url = URLHelper.RemoveParameterFromUrl(url, "neighborhood");
                url = URLHelper.RemoveParameterFromUrl(url, "custom");
                url = URLHelper.RemoveParameterFromUrl(url, "filter");

                if (ddCity.SelectedValue != this.defaultCity.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "city", ddCity.SelectedValue);
                }

                if (ddCounty.SelectedValue != this.defaultCounty.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "county", ddCounty.SelectedValue);
                }

                if (ddArea.SelectedValue != this.defaultArea.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "area", ddArea.SelectedValue);
                }

                /*  if (ddPrice.SelectedValue != this.defaultPrice.ToString())
                  {
                      url = URLHelper.AddParameterToUrl(url, "price", ddPrice.SelectedValue);
                  }*/

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

                if (ValidationHelper.GetInteger(hfLowValue.Value, 0) > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "low", ValidationHelper.GetString(hfLowValue.Value, string.Empty));
                }

                if (ValidationHelper.GetInteger(hfHighValue.Value, 0) > 0 && ValidationHelper.GetInteger(hfHighValue.Value, 0) != 100000)
                {
                    url = URLHelper.AddParameterToUrl(url, "high", ValidationHelper.GetString(hfHighValue.Value, string.Empty));
                }

                if (chkCustom.Checked == true)
                {
                    url = URLHelper.AddParameterToUrl(url, "custom", "1");
                }

                url = URLHelper.AddParameterToUrl(url, "filter", "1");
                url = url + "#filter";

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
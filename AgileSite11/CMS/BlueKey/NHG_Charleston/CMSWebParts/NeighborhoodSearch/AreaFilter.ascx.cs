using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

using CMS.DocumentEngine.Web.UI;
using CMS.CustomTables;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace NHG_C
{
    public partial class CMSWebParts_AreaFilter : CMSAbstractBaseFilterControl
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
            PopulateCounties();
            PopulateCities();
            PopulateBuilders();
            PopulateTypes();
            PopulateLifestyles();
        }

        #region Population Methods

        protected void PopulateCounties()
        {
            ddCounty.Items.Clear();

            ddCounty.Items.Insert(0, new ListItem("Any County", "0"));

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.county", "CountyVisible = 1", "CountyName ASC");

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

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.city", "CityVisible = 1", "CityName ASC", 0, "CityName, MAX(ItemID) as ItemID").GroupBy("CityName");

            int i = 1;

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddCity.Items.Insert(i, new ListItem(dr["CityName"].ToString(), dr["ItemID"].ToString()));
                i++;
            }
        }

        public void ddCounty_filterSetAreas(object obj, EventArgs sender)
        {
            int county = Convert.ToInt32(ddCounty.SelectedValue);

            if (county != 0)
            {
                resetCities(county);
            }
            else
            {
                PopulateCities();
            }
        }

        protected void resetCounties(int area)
        {
            if (area != 0)
            {
                DataSet ds = CustomTableItemProvider.GetItems("customlocations.area", "ItemID = '" + Convert.ToString(area) + "'", "AreaName ASC");

                DataTable dt = ds.Tables[0];

                DataRow row = dt.Rows[0];

                ds = CustomTableItemProvider.GetItems("customlocations.county", "CountyArea = '" + Convert.ToString(area) + "' AND CountyVisible = 1", "CountyName ASC");

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

                ds = CustomTableItemProvider.GetItems("customlocations.city", "CityCounty in (SELECT ItemID FROM customlocations_county WHERE CountyName LIKE '" + row["CountyName"] + "') AND CityVisible = 1", "CityName ASC");

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

        private string getLocationFilterQueryByCounty(int county)
        {
            string str_while = "";

            DataSet dsCounty = CustomTableItemProvider.GetItems("customlocations.county", "ItemID = '" + Convert.ToString(county) + "'", null);
            DataTable dtCounties = dsCounty.Tables[0];

            if (dtCounties.Rows.Count > 0)
            {
                str_while += " AND (";

                foreach (DataRow countyRow in dtCounties.Rows)
                {
                    str_while += "NeighborhoodCounty LIKE '" + countyRow["CountyName"].ToString() + "'";
                    str_while += " OR ";
                }

                str_while = str_while.Substring(0, str_while.Length - 4) + ")";
            }

            return str_while;
        }

        private string getLocationFilterQueryByCity(int cityID)
        {
            string str_while = "";

            DataSet dsCities = CustomTableItemProvider.GetItems("customlocations.city", "ItemID = '" + Convert.ToString(cityID) + "'", null);
            DataTable dtCities = dsCities.Tables[0];

            str_while += " AND (";

            foreach (DataRow city in dtCities.Rows)
            {
                str_while += "NeighborhoodCity LIKE '" + city["CityName"].ToString() + "'";
                str_while += " OR ";
            }

            str_while = str_while.Substring(0, str_while.Length - 4) + ")";

            return str_while;
        }

        private void ClearFilters()
        {
            string url = RequestContext.RawURL;
            string path = url.Substring(0, url.IndexOf("?"));
            path = path + "#filter";

            Response.Redirect(path);
        }

        private void SetFilter()
        {
            string where = "1 = 1";
            string url = RequestContext.RawURL;

            // Build where condition according to dropdowns setings 
            if (ddCity.SelectedValue != this.defaultCity.ToString())
            {
                where += this.getLocationFilterQueryByCity(Convert.ToInt32(ddCity.SelectedValue));
            }

            if (ddCounty.SelectedValue != this.defaultCounty.ToString())
            {
                where += this.getLocationFilterQueryByCounty(Convert.ToInt32(ddCounty.SelectedValue));
            }

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
                where += " AND (('|' + NeighborhoodTypes + '|') LIKE ('%|" + ddType.SelectedValue + "|%'))";
            }

            /*List<ListItem> selectedItems = ddType.Items.Cast<ListItem>().Where(item => item.Selected).ToList();
            foreach (ListItem item in selectedItems)
            {
                if (item.Value != this.defaultType.ToString())
                {
                    where += " AND (('|' + NeighborhoodTypes + '|') LIKE ('%|" + item.Value + "|%'))";
                }
            }*/

            if (ddLifestyle.SelectedValue != this.defaultLifestyle.ToString())
            {
                where += " AND NeighborhoodLifestyle = '" + ddLifestyle.SelectedValue + "' ";
            }

            string neighborhood = QueryHelper.GetString("neighborhood", "");

            if (neighborhood != "")
            {
                where += " AND NeighborhoodID = '" + neighborhood + "' ";
            }

            CMS.EventLog.EventLogProvider.LogInformation("BlueKey", "Debug", "Neighborhood Where: " + where);

            if (where != "")
            {
                // Set where condition
                this.WhereCondition = where;
                //ddDebugLabel.Text = where;
            }

            // Filter changed event
            this.RaiseOnFilterChanged();
        }

        private void GetFilter()
        {
            string low = QueryHelper.GetString("low", "");
            string high = QueryHelper.GetString("high", "");
            string county = QueryHelper.GetString("county", "");
            string city = QueryHelper.GetString("city", "");
            string builder = QueryHelper.GetString("builder", "");
            string type = QueryHelper.GetString("type", "");
            string lifestyle = QueryHelper.GetString("lifestyle", "");

            // Set order if in query        
            if (county != "")
            {
                resetCities(Convert.ToInt32(county));
                ddCounty.SelectedValue = county.ToString();
            }

            if ("" == city)
            {
                try
                {
                    city = DocumentContext.CurrentDocument.GetValue("City").ToString();
                    city = ddCity.Items.FindByText(city).Value.ToString();
                }
                catch (Exception ex) { }
            }

            if (city != "")
            {
                try
                {
                    ddCity.SelectedValue = city.ToString();
                }
                catch { }
            }

            if ("" == county)
            {
                try
                {
                    county = DocumentContext.CurrentDocument.GetValue("County").ToString();
                    county = ddCounty.Items.FindByText(county).Value.ToString();
                }
                catch (Exception ex) { }
            }

            if (county != "")
            {
                try
                {
                    ddCounty.SelectedValue = county.ToString();
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
            Filter();

        }

        private void Filter()
        {
            if (this.FilterByQuery)
            {
                // Handle all query parameters
                string url = RequestContext.RawURL;

                url = URLHelper.RemoveParameterFromUrl(url, "low");
                url = URLHelper.RemoveParameterFromUrl(url, "high");
                url = URLHelper.RemoveParameterFromUrl(url, "county");
                url = URLHelper.RemoveParameterFromUrl(url, "city");
                url = URLHelper.RemoveParameterFromUrl(url, "builder");
                url = URLHelper.RemoveParameterFromUrl(url, "type");
                url = URLHelper.RemoveParameterFromUrl(url, "lifestyle");
                url = URLHelper.RemoveParameterFromUrl(url, "filter");
                url = URLHelper.RemoveParameterFromUrl(url, "page");

                if (ddCounty.SelectedValue != this.defaultCounty.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "county", ddCounty.SelectedValue);
                }

                if (ddCity.SelectedValue != this.defaultCity.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "city", ddCity.SelectedValue);
                }

                if (ddBuilder.SelectedValue != this.defaultBuilder.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "builder", ddBuilder.SelectedValue);
                }

                if (ddType.SelectedValue != this.defaultType.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "type", ddType.SelectedValue);
                }

                /*string types = string.Empty;
                List<ListItem> selectedItems = ddType.Items.Cast<ListItem>().Where(item => item.Selected).ToList();
                foreach (ListItem item in selectedItems)
                {
                    if (item.Value != this.defaultType.ToString())
                    {
                        types += item.Value + "|";
                        if (!String.IsNullOrEmpty(types))
                        {
                            url = URLHelper.AddParameterToUrl(url, "type", types);
                        }
                    }
                }*/

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
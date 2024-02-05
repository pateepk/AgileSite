using System;
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
using CMS.EventLog;

//using BlueKey;

namespace NHG_C
{

        /// <summary>
    /// Static configuration class for client/site config information
    /// </summary>
    public static class Configuration
    {
        /// Debug Settings
        public static Boolean DebugMode { get { return true; } }
        public static Boolean LogDebug { get { return true; } }
        public static string DebugEmail { get { return "nshepherd+testing@bluekeyinc.com"; } }

        /// Shipping options
        public static string DefaultShippingCode { get { return "FDX_FEDEX_GROUND"; } }
        public static string DefaultShippingCodeName { get { return "FedEx Ground"; } }
        public static string ShipFromZip { get { return "29418"; } }
        public static string ShipFromCountry { get { return "US"; } }
        public static string SendSupplierEmail { get { return "true"; } }
        public static string AuthLogin { get { return "merrifieldgarden123"; } }
        public static string AuthKey { get { return "32cT5XtkE2d6S4Sd"; } }
        public static Boolean EmailSuppliers { get { return true; } }
    }

    public static class Log
    {
        private const string Info = "I";
        private const string Warn = "W";
        private const string Err = "E";

        private const string BlueKeyEventCode = "BlueKeyEvent";
        private const string BlueKeyEventType = "BlueKey";

        public static void Debug(string message)
        {
            if (Configuration.LogDebug)
            {
                LogEvent(Info, "Debug", message);
            }
        }

        public static void Warning(string message)
        {
            Warning(message, BlueKeyEventCode);
        }

        public static void Warning(string message, string eventCode)
        {
            LogEvent(Warn, eventCode, message);
        }

        public static void Information(string message)
        {
            Information(message, BlueKeyEventCode);
        }

        public static void Information(string message, string eventCode)
        {
            LogEvent(Info, eventCode, message);
        }

        public static void Error(string message)
        {
            Error(message, BlueKeyEventCode);
        }

        public static void Error(string message, string eventCode)
        {
            LogEvent(Err, eventCode, message);
        }

        public static void Error(Exception ex)
        {
            Error(ex, BlueKeyEventCode);
        }

        public static void Error(Exception ex, string eventCode)
        {
            EventLogInfo eli = new EventLogInfo();
            eli.EventCode = eventCode;

            EventLogProvider.LogEvent(eli);
        }

        private static void LogEvent(string eventType, string eventCode, string message)
        {
            EventLogInfo eli = new EventLogInfo();
            eli.EventType = eventType;
            eli.EventCode = eventCode;
            eli.EventDescription = message;

            EventLogProvider.LogEvent(eli);
        }

    }

    public partial class CMSWebParts_ListingsFilter : CMSAbstractBaseFilterControl
    {
        #region "Properties"

        public bool FilterByQuery = true;

        protected int defaultCity = 0;
        protected int defaultNeighborhood = 0;
        protected int defaultBuilder = 0;
        protected int defaultCounty = 0;
        protected int defaultBedBath = 0;
        protected int defaultType = 0;
        protected int defaultSqFt = 0;
        protected int defaultStatus = 0;

        #endregion


        #region "Methods"

        protected void Page_Load(object sender, EventArgs e)
        {
            Log.Debug("Page_Load in ListingsFilter");
            try
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
                    // Populate Filter Drop Downs
                    PopulateFilter();
                    if (DocumentContext.CurrentDocument.ClassName == "custom.Developers")
                    {
                        //divShowingHomesTotal.Visible = false;
                        //ddBuilder.Visible = false;
                        ddBuilderDropDownWrapper.Visible = false;
                    }
                    else
                    {
                        //divShowingHomesTotal.Visible = true;
                        ddBuilderDropDownWrapper.Visible = true;
                    }

                    if (this.FilterByQuery)
                    {
                        GetFilter();
                    }

                    // set city based on Document Type if applicable and the city parameter does not exist
                    if (String.Equals(CurrentDocument.ClassName.ToString(), "BK.CityHomesLandingPage")
                        && QueryHelper.GetString("city", "") == "")
                    {
                        Log.Debug("Setting selected value based upon doc type");
                        ddCity.Items.FindByText(CurrentDocument.GetValue("City").ToString()).Selected = true;
                    }
                }

                SetFilter();
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
        }

        protected void PopulateFilter()
        {
            PopulateCities();
            PopulateNeighborhoods();
            PopulateBuilders();
            PopulateCounties();
            PopulateTypes();
            PopulateSqFt();
            PopulateBedBath();
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

        public void ddCounty_filterSetAreas(object obj, EventArgs sender)
        {
            int county = Convert.ToInt32(ddCounty.SelectedValue);

            if (county != 0)
            {
                resetCities(county);
                resetNeighborhoodByCounty(county);
            }
            else
            {
                PopulateCities();
                PopulateNeighborhoods();
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

        protected void resetNeighborhoodByCounty(int county)
        {
            if (county != 0)
            {
                DataSet dsCounty = CustomTableItemProvider.GetItems("customlocations.county", "ItemID = '" + Convert.ToString(county) + "'", "CountyName ASC");
                string countyName = string.Empty;

                if (!DataHelper.DataSourceIsEmpty(dsCounty))
                {
                    countyName = ValidationHelper.GetString(dsCounty.Tables[0].Rows[0]["CountyName"], string.Empty);
                }

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                DataSet ds = null;
                int i = 1;

                //ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodCounty = '" + countyName + "'", "NeighborhoodName ASC");
                ds = tree.SelectNodes()
                    .OnCurrentSite()
                    .Type("custom.Neighborhood", q => q.Columns("NeighborhoodDevelopers", "NeighborhoodCity", "NeighborhoodName", "NeighborhoodId"))
                    .Where("NeighborhoodActive = 1")
                    .Where("NeighborhoodCounty = '" + countyName + "'");


                ddNeighborhood.Items.Clear();

                ddNeighborhood.Items.Insert(0, new ListItem("Any " + countyName + " Neighborhood", "0"));

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    DataTable dt = ds.Tables[0];

                    foreach (DataRow dr in dt.Rows)
                    {
                        ddNeighborhood.Items.Insert(i, new ListItem(dr["NeighborhoodName"].ToString(), dr["NeighborhoodId"].ToString()));
                        i++;
                    }
                }
            }
        }

        protected void resetNeighborhoodByCity(int city)
        {
            if (city != 0)
            {
                DataSet dsCity = CustomTableItemProvider.GetItems("customlocations.city", "ItemID = '" + Convert.ToString(city) + "'", "CityName ASC");
                string cityName = string.Empty;

                if (!DataHelper.DataSourceIsEmpty(dsCity))
                {
                    cityName = ValidationHelper.GetString(dsCity.Tables[0].Rows[0]["CityName"], string.Empty);
                }

                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                DataSet ds = null;
                int i = 1;

                ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodCity = '" + cityName + "' AND NeighborhoodActive = 1", "NeighborhoodName ASC");

                ddNeighborhood.Items.Clear();

                ddNeighborhood.Items.Insert(0, new ListItem("Any " + cityName + " Neighborhood", "0"));

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    DataTable dt = ds.Tables[0];

                    foreach (DataRow dr in dt.Rows)
                    {
                        ddNeighborhood.Items.Insert(i, new ListItem(dr["NeighborhoodName"].ToString(), dr["NeighborhoodId"].ToString()));
                        i++;
                    }
                }
            }
        }

        public void ddCity_filterSetAreas(object obj, EventArgs sender)
        {
            int city = Convert.ToInt32(ddCity.SelectedValue);

            if (city != 0)
            {
                resetNeighborhoodByCity(city);
            }
            else
            {
                PopulateNeighborhoods();
            }
        }

        protected void resetCities(int county)
        {
            if (county != 0)
            {
                DataSet ds = CustomTableItemProvider.GetItems("customlocations.county", "ItemID = '" + Convert.ToString(county) + "'", "CountyName ASC");

                DataTable dt = ds.Tables[0];

                DataRow row = dt.Rows[0];

                ds = CustomTableItemProvider.GetItems("customlocations.city", "CityCounty = '" + Convert.ToString(county) + "' AND CityVisible = 1", "CityName ASC");

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

        protected void PopulateTypes()
        {
            ddType.Items.Insert(0, new ListItem("Any Type", "0"));
            ddType.Items.Insert(1, new ListItem("Condos/Lofts/Townhomes", "1"));
            ddType.Items.Insert(2, new ListItem("Single-Family Homes", "2"));
            ddType.Items.Insert(3, new ListItem("Homesites", "3"));
        }

        protected void PopulateBuilders()
        {
            ddBuilder.Items.Insert(0, new ListItem("Any Builder", "0"));

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

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

        protected void PopulateNeighborhoods()
        {
            ddNeighborhood.Items.Insert(0, new ListItem("Any Neighborhood", "0"));

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;
            int i = 1;

            ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodActive = 1", "NeighborhoodName ASC");

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddNeighborhood.Items.Insert(i, new ListItem(dr["NeighborhoodName"].ToString(), dr["NeighborhoodID"].ToString()));
                i++;
            }
        }

        protected void PopulateSqFt()
        {
            ddSqFt.Items.Insert(0, new ListItem("Any Sq. Ft.", "0"));
            ddSqFt.Items.Insert(1, new ListItem("< 1000", "0|1000"));
            ddSqFt.Items.Insert(2, new ListItem("1,000 - 1,499", "1000|1499"));
            ddSqFt.Items.Insert(3, new ListItem("1,500 - 1,999", "1500|1999"));
            ddSqFt.Items.Insert(4, new ListItem("2,000 - 2,499", "2000|2499"));
            ddSqFt.Items.Insert(5, new ListItem("2,500 - 2,999", "2500|2999"));
            ddSqFt.Items.Insert(6, new ListItem("3,000 - 3,499", "3000|3499"));
            ddSqFt.Items.Insert(7, new ListItem("3,500 - 3,999", "3500|3999"));
            ddSqFt.Items.Insert(8, new ListItem("4,000 - 4,499", "4000|4499"));
            ddSqFt.Items.Insert(9, new ListItem("4,500 - 4,999", "4500|4999"));
            ddSqFt.Items.Insert(10, new ListItem("5,000+", "5000|1000000"));
        }

        protected void PopulateBedBath()
        {
            ddBedrooms.Items.Insert(0, new ListItem("Any Bedroom", "0"));
            ddBedrooms.Items.Insert(1, new ListItem("1 Bedroom", "1"));
            ddBedrooms.Items.Insert(1, new ListItem("2 Bedroom", "2"));
            ddBedrooms.Items.Insert(1, new ListItem("3 Bedroom", "3"));
            ddBedrooms.Items.Insert(1, new ListItem("4 Bedroom", "4"));
            ddBedrooms.Items.Insert(1, new ListItem("5 Bedroom", "5"));
            ddBedrooms.Items.Insert(1, new ListItem("6+ Bedroom", "6"));

        }

        #endregion

        #region Filter Methods

        private string getLocationFilterQueryByCounty(int county)
        {
            string str_while = "";

            /* DataSet dsCities = CustomTableItemProvider.GetItems("customlocations.city", "CityCounty = '" + Convert.ToString(county) + "'", null);
             DataTable dtCities = dsCities.Tables[0];

             if (dtCities.Rows.Count > 0)
             {
                 str_while += " AND (";

                 foreach (DataRow cityRow in dtCities.Rows)
                 {
                     str_while += "ListingCity LIKE '" + cityRow["CityName"].ToString() + "'";
                     str_while += " OR ";
                 }

                 str_while = str_while.Substring(0, str_while.Length - 4) + ")";
             }

             return str_while;*/
            DataSet dsCounties = CustomTableItemProvider.GetItems("customlocations.county", "ItemID = " + Convert.ToString(county), null);

            if (!DataHelper.DataSourceIsEmpty(dsCounties))
            {
                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

                string countyName = ValidationHelper.GetString(dsCounties.Tables[0].Rows[0]["CountyName"], string.Empty);

                DataSet ds = tree.SelectNodes("TheGreaterCharlestonNewHomesGuide", "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodCounty = '" + countyName + "'", "NeighborhoodName ASC");

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    str_while += " AND NodeParentID IN (";
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        str_while += ValidationHelper.GetInteger(row["NodeID"], -1) + ", ";
                    }
                    str_while = str_while.Substring(0, str_while.Length - 2);
                    str_while += ")";
                }
                else
                {
                    str_while += " AND NodeParentID IN ('')";
                }
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
                str_while += "ListingCity LIKE '" + city["CityName"].ToString() + "'";
                str_while += " OR ";
            }

            str_while = str_while.Substring(0, str_while.Length - 4) + ")";

            return str_while;
        }

        private string getNeighborhoodFilterById(int neighborhoodId)
        {
            string str_while = "";

            UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
            CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

            DataSet ds = null;
            ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodActive = 1 AND DocumentForeignKeyValue = " + ValidationHelper.GetString(neighborhoodId, string.Empty), "NeighborhoodName ASC");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                str_while += " AND NodeParentID = " + ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["NodeID"], -1).ToString();
            }

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
            else if (String.Equals(CurrentDocument.ClassName.ToString(), "BK.CityHomesLandingPage"))
            {
                ddCity.Items.FindByText(CurrentDocument.GetValue("City").ToString()).Selected = true;
            }
            if (ddCounty.SelectedValue != this.defaultCounty.ToString())
            {
                where += this.getLocationFilterQueryByCounty(Convert.ToInt32(ddCounty.SelectedValue));
            }

            if ((QueryHelper.GetInteger("low", -1) > 0) || QueryHelper.GetInteger("high", -1) > 0)
            {
                string[] range = { ((ValidationHelper.GetInteger(hfLowValue.Value, 0) * 1000)).ToString(), (ValidationHelper.GetInteger(hfHighValue.Value, 0) * 1000).ToString() };
                where += " AND (CASE WHEN ListingPrice IS NULL THEN 999999999999 ELSE CONVERT(INT, REPLACE(REPLACE(REPLACE(REPLACE(ListingPrice, ',', ''), '$', ''), 's', ''), '+', '')) END >= " + Convert.ToInt32(range[0]) + " AND CASE WHEN ListingPrice IS NULL THEN 0 ELSE CONVERT(INT, REPLACE(REPLACE(REPLACE(REPLACE(ListingPrice, ',', ''), '$', ''), 's', ''), '+', '')) END <= " + Convert.ToInt32(range[1]) + ") ";
            }

            if (ddNeighborhood.SelectedValue != this.defaultNeighborhood.ToString())
            {
                where += this.getNeighborhoodFilterById(Convert.ToInt32(ddNeighborhood.SelectedValue));
            }

            if (ddBuilder.SelectedValue != this.defaultBuilder.ToString())
            {
                where += " AND ListingDeveloper = '" + ddBuilder.SelectedValue + "'";
            }

            if (ddBedrooms.SelectedValue != this.defaultBedBath.ToString())
            {
                where += " AND ListingBedrooms = '" + ddBedrooms.SelectedValue + "' ";
            }

            if (ddType.SelectedValue != this.defaultType.ToString())
            {
                where += " AND (('|' + ListingType + '|') LIKE ('%|" + ddType.SelectedValue + "|%'))";
            }
            /*List<ListItem> selectedItems = ddType.Items.Cast<ListItem>().Where(item => item.Selected).ToList();
            foreach (ListItem item in selectedItems)
            {
                if (item.Value != this.defaultType.ToString())
                {
                    where += " AND (('|' + ListingType + '|') LIKE ('%|" + item.Value + "|%'))";
                }
            }*/

            if (ddMoveInStatus.SelectedValue != this.defaultStatus.ToString())
            {
                where += " AND ListingMoveInStatus = '" + ddMoveInStatus.SelectedValue + "' ";
            }

            if (ddSqFt.SelectedValue != this.defaultSqFt.ToString())
            {
                string[] range = ddSqFt.SelectedValue.Split('|');
                where += " AND CONVERT(INT, ListingSquareFootage) >= " + Convert.ToInt32(range[0]) + " AND CONVERT(INT, ListingSquareFootage) <= " + Convert.ToInt32(range[1]) + " ";
            }


            if (where != "")
            {
                // Set where condition
                this.WhereCondition = where;
            }

            //ddDebugLabel.Text = where;

            if (!RequestHelper.IsPostBack() || QueryHelper.GetString("filter", string.Empty) == "1")
            {
                var documents = DocumentHelper.GetDocuments("custom.Listing")
                                          .Where(where)
                                          .OnSite(SiteContext.CurrentSiteName)
                                          .Published(true)
                                          .OrderBy("ListingName ASC");
                ltlShowingCount.Text = documents.Count.ToString();
            }

            MultiDocumentQuery totalDocs;

            if (DocumentContext.CurrentDocument.ClassName == "custom.Developers")
            {
                totalDocs = DocumentHelper.GetDocuments()
                                          .Type("custom.Listing", q => q.Where("ListingDeveloper = " + DocumentContext.CurrentDocument["DevelopersID"].ToString()))
                                          .Path("/%")
                                          .OnSite(SiteContext.CurrentSiteName)
                                          .Published(true)
                                          .OrderBy("ListingName ASC");
            }
            else
            {
                totalDocs = DocumentHelper.GetDocuments()
                                          .Type("custom.Listing")
                                          .Path("/%")
                                          .OnSite(SiteContext.CurrentSiteName)
                                          .Published(true)
                                          .OrderBy("ListingName ASC");
            }

            ltlTotalCount.Text = totalDocs.Count.ToString();

            // Filter changed event
            this.RaiseOnFilterChanged();
        }

        private void GetFilter()
        {
            string city = QueryHelper.GetString("city", "");
            string low = QueryHelper.GetString("low", "");
            string high = QueryHelper.GetString("high", "");
            string neighborhood = QueryHelper.GetString("neighborhood", "");
            string builder = QueryHelper.GetString("builder", "");
            string county = QueryHelper.GetString("county", "");
            string type = QueryHelper.GetString("type", "");
            string bedrooms = QueryHelper.GetString("bedrooms", "");
            string sqft = QueryHelper.GetString("sqft", "");
            string status = QueryHelper.GetString("status", "");

            // Set order if in query

            if (county != "")
            {
                resetCities(Convert.ToInt32(county));
                ddCounty.SelectedValue = county.ToString();

            }

            if (city != "")
            {
                try
                {
                    resetNeighborhoodByCity(Convert.ToInt32(city));
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

            /*if (type != "")
            {
                List<string> types = type.Split('|').ToList();


                foreach (ListItem item in ddType.Items)
                {
                    if (types.Contains(item.Value))
                    {
                        item.Selected = true;
                    }
                }
            }*/

            if (sqft != "")
            {
                try
                {
                    ddSqFt.SelectedValue = sqft.ToString();
                }
                catch { }
            }

            if (bedrooms != "")
            {
                try
                {
                    ddBedrooms.SelectedValue = bedrooms.ToString();
                }
                catch { }
            }

            if (status != "")

            {
                try
                {
                    ddMoveInStatus.SelectedValue = status.ToString();
                }
                catch { }
            }

            if (DocumentContext.CurrentDocument.ClassName == "custom.Developers")
            {
                ddBuilder.SelectedValue = DocumentContext.CurrentDocument["DevelopersID"].ToString();
            }

            if (neighborhood != "")
            {
                try
                {
                    ddNeighborhood.SelectedValue = neighborhood.ToString();
                }
                catch { }
            }
        }

        private void Filter()
        {
            if (this.FilterByQuery)
            {
                // Handle all query parameters
                string url = RequestContext.RawURL;

                url = URLHelper.RemoveParameterFromUrl(url, "city");
                url = URLHelper.RemoveParameterFromUrl(url, "low");
                url = URLHelper.RemoveParameterFromUrl(url, "high");
                url = URLHelper.RemoveParameterFromUrl(url, "neighborhood");
                url = URLHelper.RemoveParameterFromUrl(url, "builder");
                url = URLHelper.RemoveParameterFromUrl(url, "county");
                url = URLHelper.RemoveParameterFromUrl(url, "sqft");
                url = URLHelper.RemoveParameterFromUrl(url, "bedrooms");
                url = URLHelper.RemoveParameterFromUrl(url, "type");
                url = URLHelper.RemoveParameterFromUrl(url, "status");
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

                if (ValidationHelper.GetInteger(hfLowValue.Value, 0) > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "low", ValidationHelper.GetString(hfLowValue.Value, string.Empty));
                }

                if (ValidationHelper.GetInteger(hfHighValue.Value, 0) > 0 && ValidationHelper.GetInteger(hfHighValue.Value, 0) != 100000)
                {
                    url = URLHelper.AddParameterToUrl(url, "high", ValidationHelper.GetString(hfHighValue.Value, string.Empty));
                }

                if (ddNeighborhood.SelectedValue != this.defaultNeighborhood.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "neighborhood", ddNeighborhood.SelectedValue);
                }

                if (ddBuilder.SelectedValue != this.defaultBuilder.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "builder", ddBuilder.SelectedValue);
                }

                if (ddType.SelectedValue != this.defaultType.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "type", ddType.SelectedValue);
                }
                /* string types = string.Empty;
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

                if (ddSqFt.SelectedValue != this.defaultSqFt.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "sqft", ddSqFt.SelectedValue.ToString());
                }

                if (ddBedrooms.SelectedValue != this.defaultBedBath.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "bedrooms", ddBedrooms.SelectedValue.ToString());
                }

                if (ddMoveInStatus.SelectedValue != this.defaultStatus.ToString())
                {
                    url = URLHelper.AddParameterToUrl(url, "status", ddMoveInStatus.SelectedValue.ToString());
                }

                if (DocumentContext.CurrentDocument.ClassName == "custom.Developers")
                {
                    url = URLHelper.AddParameterToUrl(url, "builder", DocumentContext.CurrentDocument["DevelopersID"].ToString());
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

        public void btnFilter_Click(object sender, EventArgs e)
        {
            Filter();
        }

        #endregion

        #endregion
    }
}
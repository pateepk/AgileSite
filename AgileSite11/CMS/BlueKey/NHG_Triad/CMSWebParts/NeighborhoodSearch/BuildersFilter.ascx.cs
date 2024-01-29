using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DocumentEngine.Web.UI;
using CMS.CustomTables;
using CMS.Helpers;
using CMS.Membership;

namespace NHG_T
{
    public partial class CMSWebParts_BuildersFilter : CMSAbstractBaseFilterControl
    {
        #region "Properties"

        public bool FilterByQuery = true;

        protected bool buildersCustom = false;

        #endregion


        #region "Methods"

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!RequestHelper.IsPostBack())
            {
                // Populate Filter Drop Downs
                //PopulateFilter();

                if (this.FilterByQuery)
                {
                    GetFilter();
                }

            }

            SetFilter();

        }



        #region Filter Methods

        private string getLocationFilterQueryByCity(int cityID)
        {
            string str_while = string.Empty;
            str_while += " AND DevelopersID IN (";

            DataSet dsCities = CustomTableItemProvider.GetItems("customlocations.city", "ItemID = " + cityID);
            if (!DataHelper.DataSourceIsEmpty(dsCities))
            {
                UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
                CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);
                //DataSet ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodActive = 1 AND NeighborhoodCity LIKE '%" + ValidationHelper.GetString(dsCities.Tables[0].Rows[0]["CityName"], string.Empty) + " %'");
                DataSet ds = tree.SelectNodes()
                    .OnCurrentSite()
                    .Type("custom.Neighborhood", q => q.Columns("NeighborhoodDevelopers", "NeighborhoodCity"))
                    .WhereLike("NeighborhoodCity", "%" + ValidationHelper.GetString(dsCities.Tables[0].Rows[0]["CityName"], string.Empty + "%"))
                    .Where("NeighborhoodActive = 1");

                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        List<string> builders = ValidationHelper.GetString(row["NeighborhoodDevelopers"], string.Empty).Split('|').ToList();
                        foreach (string builder in builders)
                        {
                            if (!String.IsNullOrEmpty(builder))
                            {
                                str_while += builder + ", ";
                            }
                        }
                        CMS.EventLog.EventLogProvider.LogInformation("BlueKey", "Debug", builders.Count.ToString());
                    }
                    str_while = str_while.Substring(0, str_while.Length - 2);
                }
                else
                {
                    str_while += "''";
                }
            }

            str_while += ")";

            return str_while;
        }

        private void SetFilter()
        {
            string where = "1 = 1";
            string url = RequestContext.RawURL;

            if (this.buildersCustom == true)
            {
                where += " AND DeveloperCustom = 1 ";
            }

            if ((QueryHelper.GetInteger("low", -1) > 0) || QueryHelper.GetInteger("high", -1) > 0)
            {
                int low = QueryHelper.GetInteger("low", 0);
                int high = QueryHelper.GetInteger("high", 100000000);

                string[] range = { ((ValidationHelper.GetInteger(low, 0) * 1000)).ToString(), (ValidationHelper.GetInteger(high, 0) * 1000).ToString() };
                //where += " AND (CONVERT(INT, REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(DeveloperPriceRangeHigh, ',', ''), '.', ''), ' million', '000000'), 's', ''), '+', '')) >= " + Convert.ToInt32(range[0]) + " AND CONVERT(INT, REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(DeveloperPriceRangeLow, ',', ''), '.', ''), ' million', 000000), 's', ''), '+', '')) <= " + Convert.ToInt32(range[1]) + ") ";
                where += " AND (CASE WHEN DeveloperPriceRangeHigh IS NULL THEN 999999999999 ELSE CAST (CASE WHEN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(DeveloperPriceRangeHigh, ',', ''), '.', ''), ' million', '000000'), 's', ''), '+', '') NOT LIKE '%[^0-9]%' THEN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(DeveloperPriceRangeHigh, ',', ''), '.', ''), ' million', '000000'), 's', ''), '+', '') END AS INT) END >= " + Convert.ToInt32(range[0]) +
                    " AND CASE WHEN DeveloperPriceRangeLow IS NULL THEN 0 ELSE CAST (CASE WHEN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(DeveloperPriceRangeLow, ',', ''), '.', ''), ' million', '000000'), 's', ''), '+', '') NOT LIKE '%[^0-9]%' THEN REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(DeveloperPriceRangeLow, ',', ''), '.', ''), ' million', '000000'), 's', ''), '+', '') END AS INT) END <= " + Convert.ToInt32(range[1]) + ")";
            }

            if (QueryHelper.GetInteger("city", -1) > 0)
            {
                where += this.getLocationFilterQueryByCity(QueryHelper.GetInteger("city", -1));
            }

            CMS.EventLog.EventLogProvider.LogInformation("BlueKey", "Debug", "Builder Filter - where condition" + where);

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
            string custom = QueryHelper.GetString("custom", "");
            string low = QueryHelper.GetString("low", "");
            string high = QueryHelper.GetString("high", "");
            string city = QueryHelper.GetString("city", "");


            // Set order if in query
            if (custom != "")
            {
                try
                {
                    this.buildersCustom = true;
                    btnFilterCustom.Text = "Show All";
                }
                catch { }
            }
            else
            {
                this.buildersCustom = false;
                btnFilterCustom.Text = "Show Custom Builders";
            }



        }

        public void btnFilter_Click(object sender, EventArgs e)
        {
            if (this.FilterByQuery)
            {
                // Handle all query parameters
                string url = RequestContext.RawURL;
                string custom = QueryHelper.GetString("custom", "");
                string low = QueryHelper.GetString("low", "");
                string high = QueryHelper.GetString("high", "");
                string city = QueryHelper.GetString("city", "");

                url = URLHelper.RemoveParameterFromUrl(url, "custom");
                url = URLHelper.RemoveParameterFromUrl(url, "low");
                url = URLHelper.RemoveParameterFromUrl(url, "high");
                url = URLHelper.RemoveParameterFromUrl(url, "city");

                if (ValidationHelper.GetInteger(low, 0) > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "low", low);
                }

                if (ValidationHelper.GetInteger(high, 0) > 0 && ValidationHelper.GetInteger(high, 0) != 400)
                {
                    url = URLHelper.AddParameterToUrl(url, "high", high);
                }

                if (ValidationHelper.GetInteger(city, 0) > 0)
                {
                    url = URLHelper.AddParameterToUrl(url, "city", city);
                }

                if (custom != "1")
                {
                    url = URLHelper.AddParameterToUrl(url, "custom", "1");
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
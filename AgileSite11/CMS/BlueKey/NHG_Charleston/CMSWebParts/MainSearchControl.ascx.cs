using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.CustomTables;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;

namespace NHG_C
{
    public partial class BlueKey_CMSWebParts_MainSearchControl : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var target = Request.Form["__EVENTTARGET"];

            if (target != null)
            {
                string divID = (string)target;
                if (divID == "btnMainSearchControlSubmit")
                {
                    var low = "";
                    var high = "";
                    var args = Request.Form["__EVENTARGUMENT"];
                    if (args != null)
                    {
                        string[] argsParams = args.Split('|');
                        low = argsParams[0];
                        high = argsParams[1];
                    }

                    PerformSearch(low, high);
                }
            }

            if (!RequestHelper.IsPostBack())
            {
                PopulateCities();
            }

            GetFilter();
        }

        private void GetFilter()
        {
            string searchBy = QueryHelper.GetString("searchBy", string.Empty);
            string city = QueryHelper.GetString("city", string.Empty);

            if (searchBy != "")
            {
                try
                {
                    ddType.SelectedValue = searchBy.ToString();
                }
                catch { }
            }

            if (city != "")
            {
                try
                {
                    ddCity.SelectedValue = city.ToString();
                }
                catch { }
            }
        }

        protected void PopulateCities()
        {
            ddCity.Items.Clear();

            ddCity.Items.Insert(0, new ListItem("Any City", "0"));

            //get site name to filter the list
            string sname = CMS.SiteProvider.SiteContext.CurrentSiteName;

            DataSet ds = CustomTableItemProvider.GetItems("customlocations.city", "CityVisible = 1 AND SiteName = '" + sname + "'", "CityName ASC", 0, "CityName, MAX(ItemID) as ItemID").GroupBy("CityName");

            int i = 1;

            DataTable dt = ds.Tables[0];

            foreach (DataRow dr in dt.Rows)
            {
                ddCity.Items.Insert(i, new ListItem(dr["CityName"].ToString(), dr["ItemID"].ToString()));
                i++;
            }
        }


        private void PerformSearch(string low, string high)
        {
            string selectedType = ddType.SelectedValue;
            string selectedCity = ddCity.SelectedValue;

            string url = RequestContext.RawURL;

            switch (selectedType)
            {
                case "home":
                    url = ResolveUrl("~/Available-Homes.aspx");
                    url = URLHelper.AddParameterToUrl(url, "searchBy", "home");
                    break;
                case "neighborhood":
                    url = ResolveUrl("~/Neighborhoods-Builders.aspx");
                    url = URLHelper.AddParameterToUrl(url, "searchBy", "neighborhood");
                    break;
                case "builder":
                    url = ResolveUrl("~/Neighborhoods-Builders/All-Builders.aspx");
                    url = URLHelper.AddParameterToUrl(url, "searchBy", "builder");
                    break;
                case "homesite":
                    url = ResolveUrl("~/Available-Homes.aspx");
                    url = URLHelper.AddParameterToUrl(url, "searchBy", "homesite");
                    url = URLHelper.AddParameterToUrl(url, "type", "3");
                    break;
                default:

                    break;
            }

            if (ValidationHelper.GetInteger(selectedCity, -1) > 0)
            {
                url = URLHelper.AddParameterToUrl(url, "city", selectedCity);
            }

            if (!String.IsNullOrEmpty(low) && !low.Equals("100"))
            {
                url = URLHelper.AddParameterToUrl(url, "low", low);
            }

            if (!String.IsNullOrEmpty(high) && high != "750")
            {
                url = URLHelper.AddParameterToUrl(url, "high", high);
            }

            url = url + "#filter";

            Response.Redirect(url);
        }
    }
}
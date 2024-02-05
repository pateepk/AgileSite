using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;


namespace NHG_T
{
    public partial class BlueKey_CMSWebParts_MobileCitySearch : CMSAbstractWebPart
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                BuildForm();
                //ltlOutput.Text = "Loading";
            }
            else
            {
                //ltlOutput.Text += " Postback ";
            }

        }

        #region Form Building

        private bool ShowForm()
        {
            return string.IsNullOrEmpty(QueryHelper.GetString("lat", string.Empty)) || ("undefined" == QueryHelper.GetString("lat", string.Empty).ToLower()) || (!string.IsNullOrEmpty(QueryHelper.GetString("cs", string.Empty)));
        }

        private void BuildForm()
        {
            if (ShowForm())
            {
                BuildCities();
            }
            else
            {
                pnlCitySearch.Visible = false;
            }
        }

        private void BuildCities()
        {
            GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;

            QueryParameters qp = new QueryParameters("select lat + ',' + long value, city text from bk_zip where display=1 order by city", null, QueryTypeEnum.SQLQuery, false);
            ds = conn.ExecuteQuery(qp);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                ddlCity.DataSource = ds;
                ddlCity.DataTextField = "text";
                ddlCity.DataValueField = "value";
                ddlCity.DataBind();

                ddlCity.Items.Insert(0, new ListItem("Select a city", "-1"));

                if (!string.IsNullOrEmpty(QueryHelper.GetString("cs", string.Empty)))
                {
                    ddlCity.SelectedIndex = ddlCity.Items.IndexOf(ddlCity.Items.FindByText(QueryHelper.GetString("cs", string.Empty)));
                }
            }

        }

        #endregion

        #region Event Methods

        protected void btnCitySearch_Click(object sender, EventArgs e)
        {
            //ltlOutput.Text += " Clicking ";
            string cityGeo = ValidationHelper.GetString(ddlCity.SelectedValue, "");

            if (cityGeo.Contains(","))
            {
                string[] city_coords = cityGeo.Split(',');

                string lat = city_coords[0];
                string lon = city_coords[1];

                string location = "&lat=" + lat + "&lon=" + lon;

                string url = "/mobile/Find-The-Guide?cs=" + ddlCity.SelectedItem.Text + location;
                string script = "<script type='text/javascript'>window.alert('hello'); jQuery.noConflict(); jQuery.mobile.changePage('" + url.Replace("'", "\'") + "');</script>";
                script = "<script type='text/javascript'>window.alert('hello');</script>";

                ClientScriptManager cs = Page.ClientScript;
                cs.RegisterClientScriptBlock(this.GetType(), "SelectCity", script);

                //Response.Redirect(url);
            }
            else
            {
                //ltlOutput.Text += "Skipping...";
            }

        }

        #endregion

    }
}
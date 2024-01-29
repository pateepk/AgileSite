using CMS.DataEngine;
using CMS.EmailEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.SiteProvider;
using CMS.UIControls;
using System;
using System.Data;
using System.Web.UI;
using System.Web.UI.WebControls;

//using BlueKey;

namespace NHG_C
{
    public partial class BlueKey_CMSModules_AnalyticsReportsDashQuarterly : CMSPage
    {
        protected string analyticsTableName = "BK_NHGAnaltyics";

        protected string analyticsDeveloperQuery = "BK.bluekeyqueries.NHGAnalyticsDeveloperDashboardQuarterly";
        protected string analyticsNeighborhoodQuery = "BK.bluekeyqueries.NHGAnalyticsNeighborhoodDashboardQuarterly";
        protected string analyticsFloorplanQuery = "BK.bluekeyqueries.NHGAnalyticsFloorplanDashboardQuarterly";
        protected string analyticsMonthSelectorQuery = "BK.bluekeyqueries.NHGAnalyticsQuarterSelector";

        protected string analyticsDeveloperEmailTemplate = "BK.AnalyticsDeveloperReport";
        protected string analyticsNeighborhoodEmailTemplate = "BK.AnalyticsNeighborhoodReport";
        protected string analyticsFloorplanEmailTemplate = "BK.AnalyticsFloorplanReport";

        protected string filteredDate = String.Empty;

        protected enum NHGObjectType { Neighborhood = 1, Developer = 2, Floorplan = 3 }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetupQuarterSelector();
            }

            SetupControls();
        }

        protected void SetupControls()
        {
            lblQuarterRecords.Text = "the Current Quarter";
            populateAnalyticsTables();
        }

        private void SetupQuarterSelector()
        {
            //GeneralConnection conn = ConnectionHelper.GetConnection();
            //DataSet ds = conn.ExecuteQuery(analyticsMonthSelectorQuery, null);
            DataSet ds = new DataQuery(analyticsMonthSelectorQuery).Execute();

            if (!DataHelper.IsEmpty(ds))
            {
                ddQuarterSelector.DataSource = ds;
                ddQuarterSelector.DataValueField = "quarter_label";
                ddQuarterSelector.DataTextField = "quarter_label";
                ddQuarterSelector.DataBind();
            }
        }

        #region Data Pop Methods

        protected DataSet getAnalyticsData(string queryName)
        {
            // GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;
            object[,] parameters = new object[3, 3];
            string where = "quarter = CEILING(CAST(MONTH(getdate())  as float)/3) and year =  YEAR(getdate())";

            if (!String.IsNullOrEmpty(this.filteredDate))
            {
                string quarter = this.filteredDate.Split('/')[0];
                string year = this.filteredDate.Split('/')[1];
                where = "quarter = " + quarter + " and year = '" + year + "'";
            }

            //ds = conn.ExecuteQuery(queryName, null, where);
            ds = new DataQuery(queryName).Where(where).Execute();

            return ds;
        }

        protected DataSet getFloorplanAnalyticsData(string queryName, int objectID)
        {
            //GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;
            object[,] parameters = new object[3, 3];

            string where = "quarter = CAST(MONTH(getdate()) % 3 as int) and year =  YEAR(getdate())";

            if (!String.IsNullOrEmpty(this.filteredDate))
            {
                string quarter = this.filteredDate.Split('/')[0];
                string year = this.filteredDate.Split('/')[1];
                where = "quarter = " + quarter + " and year = '" + year + "'";
            }

            where += " AND floorplan_id = " + ValidationHelper.GetString(objectID, "-1");

            //ds = conn.ExecuteQuery(queryName, null, where);
            ds = new DataQuery(queryName).Where(where).Execute();

            return ds;
        }

        protected DataSet getNeighborhoodAnalyticsData(string queryName, int objectID)
        {
            // GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;
            object[,] parameters = new object[3, 3];

            string where = "quarter = CAST(MONTH(getdate()) % 3 as int) and year =  YEAR(getdate())";

            if (!String.IsNullOrEmpty(this.filteredDate))
            {
                string quarter = this.filteredDate.Split('/')[0];
                string year = this.filteredDate.Split('/')[1];
                where = "quarter = " + quarter + " and year = '" + year + "'";
            }

            where += " AND neighborhood_id = " + ValidationHelper.GetString(objectID, "-1");

            //ds = conn.ExecuteQuery(queryName, null, where);
            ds = new DataQuery(queryName).Where(where).Execute();

            return ds;
        }

        protected DataSet getDeveloperAnalyticsData(string queryName, int objectID)
        {
            // GeneralConnection conn = ConnectionHelper.GetConnection();

            DataSet ds = null;
            object[,] parameters = new object[3, 3];
            string where = "quarter = CAST(MONTH(getdate()) % 3 as int) and year =  YEAR(getdate())";

            if (!String.IsNullOrEmpty(this.filteredDate))
            {
                string quarter = this.filteredDate.Split('/')[0];
                string year = this.filteredDate.Split('/')[1];
                where = "quarter = " + quarter + " and year = '" + year + "'";
            }

            where += " AND developer_id = " + ValidationHelper.GetString(objectID, "-1");

            //ds = conn.ExecuteQuery(queryName, null, where);
            ds = new DataQuery(queryName).Where(where).Execute();

            return ds;
        }

        protected void populateAnalyticsTables()
        {
            grdAnalyticsDeveloperSummary.DataSource = getAnalyticsData(analyticsDeveloperQuery);
            grdAnalyticsDeveloperSummary.DataBind();

            grdAnalyticsNeighborhoodSummary.DataSource = getAnalyticsData(analyticsNeighborhoodQuery);
            grdAnalyticsNeighborhoodSummary.DataBind();

            grdAnalyticsFloorplanSummary.DataSource = getAnalyticsData(analyticsFloorplanQuery);
            grdAnalyticsFloorplanSummary.DataBind();
        }

        #endregion Data Pop Methods

        #region Helper Methods

        protected void SendReport(int object_id, NHGObjectType object_type)
        {
            EmailMessage em = new EmailMessage();
            MacroResolver resolver = MacroContext.CurrentResolver.CreateChild();

            string recipients = "nshepherd@bluekeyinc.com";
            string emailTemplateName = String.Empty;
            DataSet entData = null;
            DataRow rowData = null;
            string[,] specialMacros = new String[,] { };
            string entityname = String.Empty;
            string date = String.Empty;
            string statImpression = String.Empty;
            string statClick = String.Empty;

            switch (object_type)
            {
                case NHGObjectType.Developer:
                    emailTemplateName = analyticsDeveloperEmailTemplate;
                    entData = getDeveloperAnalyticsData(analyticsDeveloperQuery, object_id);

                    rowData = entData.Tables[0].Rows[0];
                    entityname = ValidationHelper.GetString(rowData["developer_name"], String.Empty);
                    date = ValidationHelper.GetString(rowData["month"], String.Empty);
                    statImpression = ValidationHelper.GetString(rowData["website_impressions"], String.Empty);
                    statClick = ValidationHelper.GetString(rowData["website_clicks"], String.Empty);
                    break;

                case NHGObjectType.Neighborhood:
                    emailTemplateName = analyticsNeighborhoodEmailTemplate;
                    entData = getNeighborhoodAnalyticsData(analyticsNeighborhoodQuery, object_id);

                    rowData = entData.Tables[0].Rows[0];
                    entityname = ValidationHelper.GetString(rowData["neighborhood_name"], String.Empty);
                    date = ValidationHelper.GetString(rowData["month"], String.Empty);
                    statImpression = ValidationHelper.GetString(rowData["website_impressions"], String.Empty);
                    statClick = ValidationHelper.GetString(rowData["website_clicks"], String.Empty);
                    break;

                case NHGObjectType.Floorplan:
                    emailTemplateName = analyticsFloorplanEmailTemplate;
                    entData = getFloorplanAnalyticsData(analyticsFloorplanQuery, object_id);

                    rowData = entData.Tables[0].Rows[0];
                    entityname = ValidationHelper.GetString(rowData["floorplan_name"], String.Empty);
                    date = ValidationHelper.GetString(rowData["month"], String.Empty);
                    statImpression = ValidationHelper.GetString(rowData["website_impressions"], String.Empty);
                    statClick = ValidationHelper.GetString(rowData["website_clicks"], String.Empty);
                    break;
            }

            specialMacros = new String[,]
                {   { "EntityName", entityname },
                    { "date", date },
                    { "stat.impressions",statImpression },
                    { "stat.clicks", statClick } };

            resolver.SetNamedSourceData("EntityName", entityname);
            resolver.SetNamedSourceData("date", date);
            resolver.SetNamedSourceData("stat.impressions", statImpression);
            resolver.SetNamedSourceData("stat.clicks", statClick);

            EmailReport(recipients, em, emailTemplateName, resolver);
        }

        protected void EmailReport(string emails, EmailMessage em, string emailTemplateName, MacroResolver resolver)
        {
            EmailTemplateInfo eti = EmailTemplateProvider.GetEmailTemplate(emailTemplateName, SiteContext.CurrentSiteID);

            em.EmailFormat = EmailFormatEnum.Html;
            em.From = "info@charlottenewhomelink.com";
            em.Recipients = emails;

            EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, em, eti, resolver, false);
        }

        #region not compatible with V11

        /*
        protected void SendReport(int object_id, NHGObjectType object_type)
        {
            EmailMessage em = new EmailMessage();
            // MacroResolver mcr = new MacroResolver();

            string recipients = "nshepherd@bluekeyinc.com";
            string emailTemplateName = String.Empty;
            DataSet entData = null;
            DataRow rowData = null;
            string[,] specialMacros = new String[,] { };

            switch (object_type)
            {
                case NHGObjectType.Developer:
                    emailTemplateName = analyticsDeveloperEmailTemplate;
                    entData = getDeveloperAnalyticsData(analyticsDeveloperQuery, object_id);

                    rowData = entData.Tables[0].Rows[0];
                    specialMacros = new String[,] { { "EntityName", ValidationHelper.GetString(rowData["developer_name"], String.Empty) },
                                                    { "date", ValidationHelper.GetString(rowData["month"], String.Empty) },
                                                    { "stat.impressions", ValidationHelper.GetString(rowData["website_impressions"], String.Empty) },
                                                    { "stat.clicks", ValidationHelper.GetString(rowData["website_clicks"], String.Empty) } };
                    break;

                case NHGObjectType.Neighborhood:
                    emailTemplateName = analyticsNeighborhoodEmailTemplate;
                    entData = getNeighborhoodAnalyticsData(analyticsNeighborhoodQuery, object_id);

                    rowData = entData.Tables[0].Rows[0];
                    specialMacros = new String[,] { { "EntityName", ValidationHelper.GetString(rowData["neighborhood_name"], String.Empty) },
                                                    { "date", ValidationHelper.GetString(rowData["month"], String.Empty) },
                                                    { "stat.impressions", ValidationHelper.GetString(rowData["website_impressions"], String.Empty) },
                                                    { "stat.clicks", ValidationHelper.GetString(rowData["website_clicks"], String.Empty) } };
                    break;

                case NHGObjectType.Floorplan:
                    emailTemplateName = analyticsFloorplanEmailTemplate;
                    entData = getFloorplanAnalyticsData(analyticsFloorplanQuery, object_id);

                    rowData = entData.Tables[0].Rows[0];
                    specialMacros = new String[,] { { "EntityName", ValidationHelper.GetString(rowData["floorplan_name"], String.Empty) },
                                                    { "date", ValidationHelper.GetString(rowData["month"], String.Empty) },
                                                    { "stat.impressions", ValidationHelper.GetString(rowData["website_impressions"], String.Empty) },
                                                    { "stat.clicks", ValidationHelper.GetString(rowData["website_clicks"], String.Empty) } };
                    break;
            }

            EmailReport(recipients, em, emailTemplateName, specialMacros);
        }

        protected void EmailReport(string emails, EmailMessage em, string emailTemplateName, String[,] specialMacros)
        {
            EmailTemplateInfo eti = EmailTemplateProvider.GetEmailTemplate(emailTemplateName, SiteContext.CurrentSiteID);

            em.EmailFormat = EmailFormatEnum.Html;
            em.From = "info@charlottenewhomelink.com";
            em.Recipients = emails;

            EmailSender.SendEmailWithTemplateText(SiteContext.CurrentSiteName, em, eti, specialMacros);
        }
        */

        #endregion not compatible with V11

        #endregion Helper Methods

        #region Action Methods

        protected void grdAnalyticsDeveloperSummary_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SendReport")
            {
                int developer_id = ValidationHelper.GetInteger(e.CommandArgument, -1);

                if (developer_id > -1)
                {
                    SendReport(developer_id, NHGObjectType.Developer);
                }
            }
        }

        protected void grdAnalyticsDeveloperSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton l = (LinkButton)e.Row.FindControl("btnSendReport");
                l.Attributes.Add("onclick", "javascript:return confirm('Are you sure you want to send this report to " + DataBinder.Eval(e.Row.DataItem, "developer_emails") + "?')");
            }
        }

        protected void grdAnalyticsNeighborhoodSummary_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SendReport")
            {
                int neighborhood_id = ValidationHelper.GetInteger(e.CommandArgument, -1);

                if (neighborhood_id > -1)
                {
                    SendReport(neighborhood_id, NHGObjectType.Neighborhood);
                }
            }
        }

        protected void grdAnalyticsNeighborhoodSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton l = (LinkButton)e.Row.FindControl("btnSendReport");
                l.Attributes.Add("onclick", "javascript:return confirm('Are you sure you want to send this report to " + DataBinder.Eval(e.Row.DataItem, "neighborhood_emails") + "?')");
            }
        }

        protected void grdAnalyticsFloorplanSummary_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "SendReport")
            {
                int floorplan_id = ValidationHelper.GetInteger(e.CommandArgument, -1);

                if (floorplan_id > -1)
                {
                    SendReport(floorplan_id, NHGObjectType.Floorplan);
                }
            }
        }

        protected void grdAnalyticsFloorplanSummary_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton l = (LinkButton)e.Row.FindControl("btnSendReport");
                l.Attributes.Add("onclick", "javascript:return confirm('Are you sure you want to send this report to " + DataBinder.Eval(e.Row.DataItem, "floorplan_emails") + "?')");
            }
        }

        protected void btnChangeToQuarterClick(object sender, EventArgs e)
        {
            this.filteredDate = ValidationHelper.GetString(ddQuarterSelector.SelectedValue, "(RTrim(a.month) + '/' + LTrim(a.year)) = CAST(MONTH(getdate()) as varchar) + '/' + CAST(YEAR(getdate()) as varchar)").Trim();
            this.lblQuarterRecords.Text = this.filteredDate;

            populateAnalyticsTables();
        }

        #endregion Action Methods
    }
}
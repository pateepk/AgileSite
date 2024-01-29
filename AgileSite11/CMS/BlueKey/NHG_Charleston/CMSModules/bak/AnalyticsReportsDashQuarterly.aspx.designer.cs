using System;
using System.Web;
using System.Web.UI.WebControls;
using System.Data;

using CMS.CMSHelper;
using CMS.GlobalHelper;
using CMS.SettingsProvider;
using CMS.DataEngine;
using CMS.UIControls;
using CMS.EmailEngine;
using System.Web.UI;

public partial class BlueKey_CMSModules_AnalyticsReportsDashQuarterly : System.Web.UI.Page
{

    protected string analyticsTableName = "BK_NHGAnaltyics";

    protected string analyticsDeveloperQuery = "BK.bluekeyqueries.NHGAnalyticsDeveloperDashboardQuarterly";
    protected string analyticsNeighborhoodQuery = "BK.bluekeyqueries.NHGAnalyticsNeighborhoodDashboardQuarterly";
    protected string analyticsMonthSelectorQuery = "BK.bluekeyqueries.NHGAnalyticsQuarterSelector";

    protected string analyticsDeveloperEmailTemplate = "BK.AnalyticsDeveloperReport";
    protected string analyticsNeighborhoodEmailTemplate = "BK.AnalyticsNeighborhoodReport";

    protected string filteredDate = String.Empty;

    protected enum NHGObjectType { Neighborhood = 1, Developer = 2 }


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
        GeneralConnection conn = ConnectionHelper.GetConnection();
        DataSet ds = conn.ExecuteQuery(analyticsMonthSelectorQuery, null);

        if (!DataHelper.IsEmpty(ds))
        {
            ddQuarterSelector.DataSource = ds;
            ddQuarterSelector.DataValueField = "quarter_label";
            ddQuarterSelector.DataTextField = "quarter_label";
            ddQuarterSelector.DataBind();
        }
    }



    #region Control Setup


    #endregion

    #region Data Pop Methods

    protected DataSet getAnalyticsData(string queryName)
    {
        GeneralConnection conn = ConnectionHelper.GetConnection();

        DataSet ds = null;
        object[,] parameters = new object[3, 3];
        string where = "quarter = CAST(MONTH(getdate()) % 3 as int) and year =  YEAR(getdate())";

        if (!String.IsNullOrEmpty(this.filteredDate))
        {
            string quarter = this.filteredDate.Split('/')[0];
            string year = this.filteredDate.Split('/')[1];
            where = "quarter = " + quarter + " and year = '" + year + "'";
        }

        ds = conn.ExecuteQuery(queryName, null, where);

        return ds;
    }

    protected DataSet getNeighborhoodAnalyticsData(string queryName, int objectID)
    {
        GeneralConnection conn = ConnectionHelper.GetConnection();

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

        ds = conn.ExecuteQuery(queryName, null, where);

        return ds;
    }


    protected DataSet getDeveloperAnalyticsData(string queryName, int objectID)
    {
        GeneralConnection conn = ConnectionHelper.GetConnection();

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

        ds = conn.ExecuteQuery(queryName, null, where);

        return ds;
    }

    protected void populateAnalyticsTables()
    {
        grdAnalyticsDeveloperSummary.DataSource = getAnalyticsData(analyticsDeveloperQuery);
        grdAnalyticsDeveloperSummary.DataBind();

        grdAnalyticsNeighborhoodSummary.DataSource = getAnalyticsData(analyticsNeighborhoodQuery);
        grdAnalyticsNeighborhoodSummary.DataBind();
    }

    #endregion

    #region Helper Methods

    protected void SendReport(int object_id, NHGObjectType object_type)
    {
        EmailMessage em = new EmailMessage();
        MacroResolver mcr = new MacroResolver();

        string recipients = "nshepherd@bluekeyinc.com";
        string emailTemplateName = String.Empty;
        DataSet entData = null;
        DataRow rowData = null;

        switch (object_type)
        {
            case NHGObjectType.Developer:
                emailTemplateName = analyticsDeveloperEmailTemplate;
                entData = getDeveloperAnalyticsData(analyticsDeveloperQuery, object_id);

                rowData = entData.Tables[0].Rows[0];
                mcr.SpecialMacros = new String[,] { { "#EntityName#", ValidationHelper.GetString(rowData["developer_name"], String.Empty) }, 
                                                    { "#date#", ValidationHelper.GetString(rowData["month"], String.Empty) }, 
                                                    { "#stat.impressions#", ValidationHelper.GetString(rowData["website_impressions"], String.Empty) }, 
                                                    { "#stat.clicks#", ValidationHelper.GetString(rowData["website_clicks"], String.Empty) } };
                break;

            case NHGObjectType.Neighborhood:
                emailTemplateName = analyticsNeighborhoodEmailTemplate;
                entData = getNeighborhoodAnalyticsData(analyticsNeighborhoodQuery, object_id);

                rowData = entData.Tables[0].Rows[0];
                mcr.SpecialMacros = new String[,] { { "#EntityName#", ValidationHelper.GetString(rowData["neighborhood_name"], String.Empty) }, 
                                                    { "#date#", ValidationHelper.GetString(rowData["month"], String.Empty) }, 
                                                    { "#stat.impressions#", ValidationHelper.GetString(rowData["website_impressions"], String.Empty) }, 
                                                    { "#stat.clicks#", ValidationHelper.GetString(rowData["website_clicks"], String.Empty) } };
                break;
        }

        EmailReport(recipients, em, emailTemplateName, mcr);

    }

    protected void EmailReport(string emails, EmailMessage em, string emailTemplateName, MacroResolver mcr)
    {

        EmailTemplateInfo eti = EmailTemplateProvider.GetEmailTemplate(emailTemplateName, CMSContext.CurrentSiteID);

        em.EmailFormat = EmailFormatEnum.Html;
        em.From = "info@newhomesguidecharleston.com";
        em.Recipients = emails;

        EmailSender.SendEmailWithTemplateText(CMSContext.CurrentSiteName, em, eti, mcr, true);

    }

    #endregion

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

    protected void btnChangeToQuarterClick(object sender, EventArgs e)
    {
        this.filteredDate = ValidationHelper.GetString(ddQuarterSelector.SelectedValue, "(RTrim(a.month) + '/' + LTrim(a.year)) = CAST(MONTH(getdate()) as varchar) + '/' + CAST(YEAR(getdate()) as varchar)").Trim();
        this.lblQuarterRecords.Text = this.filteredDate;

        populateAnalyticsTables();
    }

    #endregion

}
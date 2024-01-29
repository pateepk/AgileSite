using System;
using System.Data;
using System.Web.UI.WebControls;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.UIControls;
using CMS.WebAnalytics;
using CMS.DeviceProfiles;

//using BlueKey;

public partial class CMSPages_TrackClick : CMSPage
{
    #region app_code classes

    public class SQL
    {
        public static DataSet ExecuteQuery(string sql, QueryDataParameters p)
        {
            QueryParameters qp = new QueryParameters(sql, p, QueryTypeEnum.SQLQuery);

            return ConnectionHelper.ExecuteQuery(qp);
        }
    }

    #endregion app_code classes

    protected void Page_Load(object sender, EventArgs e)
    {
        string url = URLHelper.GetUrlParameter(RequestContext.CurrentURL, "url");
        string track_type = URLHelper.GetUrlParameter(RequestContext.CurrentURL, "type");
        string id = URLHelper.GetUrlParameter(RequestContext.CurrentURL, "id");

        UserInfo ui = UserInfoProvider.GetUserInfo("administrator");
        CMS.DocumentEngine.TreeProvider tree = new CMS.DocumentEngine.TreeProvider(ui);

        DataSet ds = null;
        CMS.DocumentEngine.TreeNode node = null;

        //validate domain
        if (!ValidDomain(url))
        {
            Response.Redirect("/Home-Builders");
        }

        //WTE Custom log
        LogClickTrack();

        if (track_type == "builder")
        {
            int builderId = ValidationHelper.GetInteger(id, 0);
            TrackBuilderClick(builderId);

            ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Developers", "DevelopersID = " + id.ToString());

            if (ds != null)
            {
                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    node = tree.SelectSingleNode(Convert.ToInt32(dr["NodeID"]));

                    node.SetValue("DeveloperSiteClicks", Convert.ToString(Convert.ToInt32(dr["DeveloperSiteClicks"]) + 1));
                    node.Update();
                }
            }
        }
        else if (track_type == "neighborhood")
        {
            int neighborhoodID = ValidationHelper.GetInteger(id, 0);
            TrackNeighborhoodClick(neighborhoodID);

            ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = " + id.ToString());
            if (ds != null)
            {
                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    node = tree.SelectSingleNode(Convert.ToInt32(dr["NodeID"]));
                    node.SetValue("NeighborhoodSiteClicks", ValidationHelper.GetInteger(dr["NeighborhoodSiteClicks"], 0) + 1);
                    node.Update();
                }
            }
        }
        else if (track_type == "floorplan")
        {
            int floorplanID = ValidationHelper.GetInteger(id, 0);
            TrackFloorplanClick(floorplanID);

            ds = tree.SelectNodes(SiteContext.CurrentSiteName, "/%", "en-us", true, "custom.Neighborhood", "NeighborhoodID = " + id.ToString());

            if (ds != null)
            {
                DataTable dt = ds.Tables[0];

                foreach (DataRow dr in dt.Rows)
                {
                    node = tree.SelectSingleNode(Convert.ToInt32(dr["NodeID"]));

                    node.SetValue("FloorplanSiteClicks", Convert.ToString(Convert.ToInt32(dr["FloorplanSiteClicks"]) + 1));
                    node.Update();
                }
            }
        }
        if (!url.Contains("https"))
        {
            Response.Redirect("http://" + url.Replace("http://", ""));
        }
        else
        {
            Response.Redirect(url);
        }
    }

    protected string getDomain(string url)
    {
        string retDomain = url;

        try
        {
            if (!String.IsNullOrWhiteSpace(retDomain))
            {
                retDomain = retDomain.Replace("https://", "");
                retDomain = retDomain.Replace("http://", "");

                Char delimiter = '/';
                retDomain = retDomain.Split(delimiter)[0];
            }
        }
        catch
        {
            //do nothing
        }

        return retDomain;
    }

    protected bool ValidDomain(string linkTo)
    {
        bool ret = false;

        if (!String.IsNullOrWhiteSpace(linkTo))
        {
            string linkToDomain = getDomain(linkTo);
            string sql = string.Empty;
            DataSet ds = null;
            QueryDataParameters parameters = new QueryDataParameters();


            parameters.Add("@ValidateDomain", linkToDomain);

            sql = "SELECT Count(1) [valid] FROM [AS11_NHGCharlestonV11].[dbo].[customtable_ClickTrackAllowedDomains] WHERE @ValidateDomain LIKE '%' + [AllowedDomain] + '%'";

            ds = SQL.ExecuteQuery(sql, parameters);
            DataRow row = ds.Tables[0].Rows[0];

            if (ValidationHelper.GetInteger(row["valid"], 0) == 1)
            {
                ret = true;
            }
            else
            {
                ret = false;
            }
        }
        return ret;
    }

    protected void LogClickTrack()
    {
        string uSql = string.Empty;
        QueryDataParameters uParameters = new QueryDataParameters();
        string linkType = QueryHelper.GetString("type", "");
        string linkItemID = QueryHelper.GetString("id", "");
        string builderId = QueryHelper.GetString("bid", "");
        string neighborhoodId = QueryHelper.GetString("nid", "");
        string linkNote = QueryHelper.GetString("LinkNote", "Notes: ") + " | type = " + linkType + " | id = " + linkItemID;
        string docID = QueryHelper.GetString("DocumentID", "");
        string linkTo = QueryHelper.GetString("url", "");
        string SiteID = "0";
        string linkToDomain = getDomain(linkTo);
        string viewURL = "";

        if (System.Web.HttpContext.Current.Request.UrlReferrer != null)
        {
            viewURL = System.Web.HttpContext.Current.Request.UrlReferrer.ToString();
        }

        if (SiteContext.CurrentSiteID != null)
        {
            SiteID = SiteContext.CurrentSiteID.ToString();
        }

        uParameters.Add("@ClickTrackDate", DateTime.Now.ToString());
        uParameters.Add("@UserIPAddress", System.Web.HttpContext.Current.Request.UserHostAddress);
        uParameters.Add("@UserID", CurrentUser.UserID);
        uParameters.Add("@IsAuthenticated", AuthenticationHelper.IsAuthenticated().ToString());
        uParameters.Add("@IsReturningVisitor", AnalyticsContext.IsReturningVisitor.ToString());
        uParameters.Add("@IsNewVisitor", AnalyticsContext.IsNewVisitor.ToString());
        uParameters.Add("@Browser", BrowserHelper.GetBrowser());
        uParameters.Add("@UserAgent", BrowserHelper.GetUserAgent());
        uParameters.Add("@IsCrawler", BrowserHelper.IsCrawler().ToString());

        uParameters.Add("@IsMobile", DeviceContext.CurrentDevice.IsMobile().ToString());
        uParameters.Add("@IsTablet", "false");

        uParameters.Add("@DocumentID", docID);
        uParameters.Add("@LinkTo", linkTo);
        uParameters.Add("@LinkNote", linkNote);
        uParameters.Add("@PageViewURL", viewURL);
        uParameters.Add("@LinkType", linkType);
        uParameters.Add("@LinkItemID", linkItemID);
        uParameters.Add("@LinkToDomain", linkToDomain);
        uParameters.Add("@BuilderID", builderId);
        uParameters.Add("@NeighborhoodID", neighborhoodId);
        uParameters.Add("@SiteID", SiteID);

        uSql = "INSERT INTO WTE_ClickTrack (ClickTrackDate,UserIPAddress,UserID,IsAuthenticated,IsReturningVisitor,IsNewVisitor,Browser,UserAgent,IsCrawler,IsMobile,IsTablet,DocumentID,LinkTo,LinkNote,PageViewURL,LinkType,LinkItemID,LinkToDomain,BuilderID,NeighborhoodID,SiteID) ";
        uSql += " VALUES (@ClickTrackDate,@UserIPAddress,@UserID,@IsAuthenticated,@IsReturningVisitor,@IsNewVisitor,@Browser,@UserAgent,@IsCrawler,@IsMobile,@IsTablet,@DocumentID,@LinkTo,@LinkNote,@PageViewURL,@LinkType,@LinkItemID,@LinkToDomain,@BuilderID,@NeighborhoodID,@SiteID)";

        SQL.ExecuteQuery(uSql, uParameters);
    }

    private void TrackFloorplanClick(int floorplanID)
    {
        GeneralConnection conn = ConnectionHelper.GetConnection();
        DateTime now = DateTime.Now;
        int month = now.Month;
        int year = now.Year;

        DataSet ds = null;
        QueryDataParameters parameters = new QueryDataParameters();

        parameters.Add("@id", floorplanID.ToString());
        parameters.Add("@month", month);
        parameters.Add("@year", year);

        string sql = "SELECT * FROM BK_NHGAnalytics WHERE floorplan_id = @id AND month = @month AND year = @year";

        ds = SQL.ExecuteQuery(sql, parameters);

        QueryDataParameters uParameters = new QueryDataParameters();
        string uSql = string.Empty;

        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                int clicks = ValidationHelper.GetInteger(row["clicks"], 0);
                clicks++;

                uParameters.Add("@clicks", clicks);
                uParameters.Add("@id", floorplanID.ToString());
                uParameters.Add("@month", month);
                uParameters.Add("@year", year);

                uSql = "UPDATE BK_NHGAnalytics SET clicks = @clicks where floorplan_id = @id AND month = @month AND year = @year";
            }
        }
        else
        {
            uParameters.Add("@id", floorplanID.ToString());
            uParameters.Add("@month", month);
            uParameters.Add("@year", year);

            uSql = "INSERT INTO BK_NHGAnalytics (clicks, floorplan_id, impressions, month, year) VALUES (1, @id, 0, @month, @year)";
        }

        SQL.ExecuteQuery(uSql, uParameters);
    }

    private void TrackBuilderClick(int builderID)
    {
        GeneralConnection conn = ConnectionHelper.GetConnection();
        DateTime now = DateTime.Now;
        int month = now.Month;
        int year = now.Year;

        DataSet ds = null;
        QueryDataParameters parameters = new QueryDataParameters();

        parameters.Add("@id", builderID.ToString());
        parameters.Add("@month", month);
        parameters.Add("@year", year);

        string sql = "SELECT * FROM BK_NHGAnalytics WHERE developer_id = @id AND month = @month AND year = @year";

        ds = SQL.ExecuteQuery(sql, parameters);

        QueryDataParameters uParameters = new QueryDataParameters();
        string uSql = string.Empty;

        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                int clicks = ValidationHelper.GetInteger(row["clicks"], 0);
                clicks++;

                uParameters.Add("@clicks", clicks);
                uParameters.Add("@id", builderID.ToString());
                uParameters.Add("@month", month);
                uParameters.Add("@year", year);

                uSql = "UPDATE BK_NHGAnalytics SET clicks = @clicks where developer_id = @id AND month = @month AND year = @year";
            }
        }
        else
        {
            uParameters.Add("@id", builderID.ToString());
            uParameters.Add("@month", month);
            uParameters.Add("@year", year);

            uSql = "INSERT INTO BK_NHGAnalytics (clicks, developer_id, impressions, month, year) VALUES (1, @id, 0, @month, @year)";
        }

        SQL.ExecuteQuery(uSql, uParameters);
    }

    private void TrackNeighborhoodClick(int neighborhoodID)
    {
        GeneralConnection conn = ConnectionHelper.GetConnection();
        DateTime now = DateTime.Now;
        int month = now.Month;
        int year = now.Year;

        DataSet ds = null;
        QueryDataParameters parameters = new QueryDataParameters();

        parameters.Add("@id", neighborhoodID.ToString());
        parameters.Add("@month", month);
        parameters.Add("@year", year);

        string sql = "SELECT * FROM BK_NHGAnalytics WHERE neighborhood_id = @id AND month = @month AND year = @year";

        ds = SQL.ExecuteQuery(sql, parameters);

        QueryDataParameters uParameters = new QueryDataParameters();
        string uSql = string.Empty;

        if (!DataHelper.DataSourceIsEmpty(ds))
        {
            foreach (DataRow row in ds.Tables[0].Rows)
            {
                int clicks = ValidationHelper.GetInteger(row["clicks"], 0);
                clicks++;

                uParameters.Add("@clicks", clicks);
                uParameters.Add("@id", neighborhoodID.ToString());
                uParameters.Add("@month", month);
                uParameters.Add("@year", year);

                uSql = "UPDATE BK_NHGAnalytics SET clicks = @clicks where neighborhood_id = @id AND month = @month AND year = @year";
            }
        }
        else
        {
            uParameters.Add("@id", neighborhoodID.ToString());
            uParameters.Add("@month", month);
            uParameters.Add("@year", year);

            uSql = "INSERT INTO BK_NHGAnalytics (clicks, neighborhood_id, impressions, month, year) VALUES (1, @id, 0, @month, @year)";
        }

        SQL.ExecuteQuery(uSql, uParameters);
    }
}
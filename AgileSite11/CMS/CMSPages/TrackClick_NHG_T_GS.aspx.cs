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

public partial class CMSPages_TrackClick_NHG_T_GS : CMSPage
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

        if (!String.IsNullOrWhiteSpace(url))
        {

            //WTE Custom log 
            LogClickTrack();

            if (track_type == "builder")
            {

                int builderId = ValidationHelper.GetInteger(id, 0);
                //TrackBuilderClick(builderId);


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
                //TrackNeighborhoodClick(neighborhoodID);

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
                //TrackFloorplanClick(floorplanID);

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

            if (!url.Contains("https") && url.Substring(0, 1) != "/")
            {
                Response.Redirect("http://" + url.Replace("http://", ""));
            }
            else
            {
                Response.Redirect(url);
            }
        }
        else
        {
            Response.Redirect("/builders");
        }
        
    }

    protected string getDomain(string url)
    {
        string retDomain = url;

        try
        {
            retDomain = retDomain.Replace("https://", "");
            retDomain = retDomain.Replace("http://", "");

            Char delimiter = '/';
            retDomain = retDomain.Split(delimiter)[0];

        }
        catch
        {
            //do nothing
        }

        return retDomain;
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
}
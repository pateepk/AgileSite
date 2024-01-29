using CMS.DataEngine;
using CMS.Helpers;
using CMS.UIControls;
using System;
using System.Data;

//using NHG_C.BlueKey;

namespace NHG_C.CMSApp.utils
{
    public partial class View : CMSPage
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
            //throw new Exception("I hit the page");
            LogPageView();

            //Content to return for image
            Response.ContentType = "image/png";

            System.Net.WebClient wc = new System.Net.WebClient();

            byte[] data = wc.DownloadData("https://www.newhomesguidecharleston.com/BlueKey/CMSWebParts/Analytics/pixel.png");

            Response.OutputStream.Write(data, 0, data.Length);
            Response.OutputStream.Flush();
            Response.End();
        }

        protected void LogPageView()
        {
            string uSql = string.Empty;
            QueryDataParameters uParameters = new QueryDataParameters();

            uParameters.Add("@PageViewDate", QueryHelper.GetString("PageViewDate", DateTime.Now.ToString()));
            uParameters.Add("@UserIPAddress", QueryHelper.GetString("UserHostAddress", ""));
            uParameters.Add("@UserID", QueryHelper.GetString("UserID", ""));
            uParameters.Add("@IsAuthenticated", QueryHelper.GetString("IsAuthenticated", ""));
            uParameters.Add("@IsReturningVisitor", QueryHelper.GetString("IsReturningVisitor", ""));
            uParameters.Add("@IsNewVisitor", QueryHelper.GetString("IsNewVisitor", ""));
            uParameters.Add("@Browser", QueryHelper.GetString("Browser", ""));
            uParameters.Add("@UserAgent", QueryHelper.GetString("UserAgent", ""));
            uParameters.Add("@IsCrawler", QueryHelper.GetString("IsCrawler", ""));
            uParameters.Add("@IsMobile", QueryHelper.GetString("IsMobile", ""));
            uParameters.Add("@IsTablet", QueryHelper.GetString("IsTablet", ""));
            uParameters.Add("@DocumentID", QueryHelper.GetString("DocumentID", ""));
            uParameters.Add("@PageViewURL", QueryHelper.GetString("PageViewURL", ""));
            uParameters.Add("@SiteID", QueryHelper.GetString("SiteID", ""));

            uSql = "INSERT INTO WTE_PageView (PageViewDate,UserIPAddress,UserID,IsAuthenticated,IsReturningVisitor,IsNewVisitor,Browser,UserAgent,IsCrawler,IsMobile,IsTablet,DocumentID,PageViewURL,SiteID) ";
            uSql += " VALUES (@PageViewDate,@UserIPAddress,@UserID,@IsAuthenticated,@IsReturningVisitor,@IsNewVisitor,@Browser,@UserAgent,@IsCrawler,@IsMobile,@IsTablet,@DocumentID,@PageViewURL,@SiteID)";

            SQL.ExecuteQuery(uSql, uParameters);
        }
    }
}
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.PortalEngine.Web.UI;
using System;
using System.Data;

//using BlueKey;

namespace NHG_C
{
    public partial class BlueKey_CMSWebParts_ImpressionTracker : CMSAbstractWebPart
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

        public override void OnContentLoaded()
        {
            base.OnContentLoaded();
            TrackImpression();
        }

        protected void TrackImpression()
        {
            string docType = DocumentContext.CurrentDocument.NodeClassName;

            if (docType == "custom.Neighborhood")
            {
                int nID = ValidationHelper.GetInteger(DocumentContext.CurrentDocument.GetValue("NeighborhoodID"), -1);
                TrackNeighborhoodImpression(nID);
                TrackFloorplanImpression(nID);
            }

            if (docType == "custom.Developers")
            {
                int bID = ValidationHelper.GetInteger(DocumentContext.CurrentDocument.GetValue("DevelopersID"), -1);
                TrackBuilderImpression(bID);
            }
        }

        private void TrackBuilderImpression(int builderID)
        {
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

            string uSql = string.Empty;
            QueryDataParameters uParameters = new QueryDataParameters();

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int impressions = ValidationHelper.GetInteger(row["impressions"], 0);
                    impressions++;

                    uParameters.Add("@impressions", impressions);
                    uParameters.Add("@id", builderID.ToString());
                    uParameters.Add("@month", month);
                    uParameters.Add("@year", year);

                    uSql = " UPDATE BK_NHGAnalytics SET impressions = @impressions where developer_id = @id AND month = @month AND year = @year";
                }
            }
            else
            {
                uParameters.Add("@id", builderID.ToString());
                uParameters.Add("@month", month);
                uParameters.Add("@year", year);

                uSql = "INSERT INTO BK_NHGAnalytics (impressions, developer_id, clicks, month, year) VALUES (1, @id, 0, @month, @year)";
            }

            SQL.ExecuteQuery(uSql, uParameters);
        }

        private void TrackFloorplanImpression(int floorplanID)
        {
            DateTime now = DateTime.Now;
            int month = now.Month;
            int year = now.Year;

            DataSet ds = null;
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@id", floorplanID);
            parameters.Add("@month", month);
            parameters.Add("@year", year);

            string sql = "SELECT * FROM BK_NHGAnalytics WHERE floorplan_id = @id AND month = @month AND year = @year";

            ds = SQL.ExecuteQuery(sql, parameters);

            string uSql = string.Empty;
            QueryDataParameters uParameters = new QueryDataParameters();

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int impressions = ValidationHelper.GetInteger(row["impressions"], 0);
                    impressions++;

                    uParameters.Add("@impressions", impressions);
                    uParameters.Add("@id", floorplanID.ToString());
                    uParameters.Add("@month", month);
                    uParameters.Add("@year", year);

                    uSql = "UPDATE BK_NHGAnalytics SET impressions = @impressions where floorplan_id = @id AND month = @month AND year = @year";
                }
            }
            else
            {
                uParameters.Add("@id", floorplanID.ToString());
                uParameters.Add("@month", month);
                uParameters.Add("@year", year);

                uSql = "INSERT INTO BK_NHGAnalytics (impressions, floorplan_id, clicks, month, year) VALUES (1, @id, 0, @month, @year)";
            }

            SQL.ExecuteQuery(uSql, uParameters);
        }

        private void TrackNeighborhoodImpression(int neighborhoodID)
        {
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

            string uSql = string.Empty;
            QueryDataParameters uParameters = new QueryDataParameters();

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow row in ds.Tables[0].Rows)
                {
                    int impressions = ValidationHelper.GetInteger(row["impressions"], 0);
                    impressions++;

                    uParameters.Add("@impressions", impressions);
                    uParameters.Add("@id", neighborhoodID.ToString());
                    uParameters.Add("@month", month);
                    uParameters.Add("@year", year);

                    uSql = "UPDATE BK_NHGAnalytics SET impressions = @impressions where neighborhood_id = @id AND month = @month AND year = @year";
                }
            }
            else
            {
                uParameters.Add("@id", neighborhoodID.ToString());
                uParameters.Add("@month", month);
                uParameters.Add("@year", year);

                uSql = "INSERT INTO BK_NHGAnalytics (impressions, neighborhood_id, clicks, month, year) VALUES (1, @id, 0, @month, @year)";
            }

            SQL.ExecuteQuery(uSql, uParameters);
        }
    }
}
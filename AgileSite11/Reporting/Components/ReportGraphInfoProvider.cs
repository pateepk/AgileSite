using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;

namespace CMS.Reporting
{
    using TypedDataSet = InfoDataSet<ReportGraphInfo>;

    /// <summary>
    /// Class providing ReportGraphInfo management.
    /// </summary>
    public class ReportGraphInfoProvider : AbstractInfoProvider<ReportGraphInfo, ReportGraphInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns a query for all the <see cref="ReportGraphInfo"/> objects.
        /// </summary>
        public static ObjectQuery<ReportGraphInfo> GetReportGraphs()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        public static ReportGraphInfo GetReportGraphInfoByGUID(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the ReportGraphInfo structure for the specified reportGraph.
        /// </summary>
        /// <param name="reportGraphId">ReportGraph ID</param>
        public static ReportGraphInfo GetReportGraphInfo(int reportGraphId)
        {
            return ProviderObject.GetInfoById(reportGraphId);
        }


        /// <summary>
        /// Returns the ReportGraphInfo structure for the specified name.
        /// </summary>
        /// <param name="graphName">ReportGraph name</param>        
        public static ReportGraphInfo GetReportGraphInfo(string graphName)
        {
            return ProviderObject.GetReportGraphInfoInternal(graphName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified report graph.
        /// </summary>
        /// <param name="reportGraph">ReportGraph to set</param>
        public static void SetReportGraphInfo(ReportGraphInfo reportGraph)
        {
            ProviderObject.SetReportGraphInfoInternal(reportGraph);
        }


        /// <summary>
        /// Deletes specified report graph.
        /// </summary>
        /// <param name="reportGraphObj">ReportGraph object</param>
        public static void DeleteReportGraphInfo(ReportGraphInfo reportGraphObj)
        {
            ProviderObject.DeleteInfo(reportGraphObj);
        }


        /// <summary>
        /// Deletes specified report graph.
        /// </summary>
        /// <param name="reportGraphId">ReportGraph ID</param>
        public static void DeleteReportGraphInfo(int reportGraphId)
        {
            ReportGraphInfo reportGraphObj = GetReportGraphInfo(reportGraphId);
            ProviderObject.DeleteInfo(reportGraphObj);
        }


        /// <summary>
        /// Uses the query to retrieve the graph data from the database and returns the resulting table.
        /// </summary>
        /// <param name="graphObj">ReportGraphInfo object</param>
        /// <param name="parameters">Parameters array</param>
        public static DataSet GetGraphData(ReportGraphInfo graphObj, QueryDataParameters parameters)
        {
            return ProviderObject.GetGraphDataInternal(graphObj, parameters);
        }


        /// <summary>
        /// Returns ReportGraphs satisfying where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        [Obsolete("Use method GetReportGraphs() instead")]
        public static TypedDataSet GetGraphs(string where, string orderBy)
        {
            return GetGraphs(where, orderBy, -1, null);
        }


        /// <summary>
        /// Returns ReportGraphs satisfying where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        /// <param name="topN">TOP N rows</param>
        /// <param name="columns">Returned columns</param>
        [Obsolete("Use method GetReportGraphs() instead")]
        public static TypedDataSet GetGraphs(string where, string orderBy, int topN, string columns)
        {
            return GetReportGraphs().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the ReportGraphInfo structure for the specified parameters.
        /// </summary>
        /// <param name="graphName">Graph name</param>  
        protected ReportGraphInfo GetReportGraphInfoInternal(string graphName)
        {
            ReportGraphInfo reportGraphObj = null;

            string graphReportName = "";

            if (graphName.IndexOfCSafe('.') >= 0)
            {
                graphReportName = graphName.Remove(graphName.LastIndexOfCSafe('.'));
            }

            graphName = graphName.Remove(0, graphName.LastIndexOfCSafe('.') + 1);

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ReportGraphName", graphName);
            parameters.Add("@ReportName", graphReportName);

            DataSet ds = ConnectionHelper.ExecuteQuery("Reporting.ReportGraph.selectByFullName", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                reportGraphObj = new ReportGraphInfo(ds.Tables[0].Rows[0]);
            }

            return reportGraphObj;
        }


        /// <summary>
        /// Sets (updates or inserts) specified reportGraph.
        /// </summary>
        /// <param name="reportGraph">ReportGraph to set</param>
        protected void SetReportGraphInfoInternal(ReportGraphInfo reportGraph)
        {
            if (reportGraph != null)
            {
                // Set custom data
                reportGraph.SetValue("GraphSettings", reportGraph.GraphSettings.GetData());

                // Update the database
                if (reportGraph.GraphID > 0)
                {
                    reportGraph.Generalized.UpdateData();
                }
                else
                {
                    reportGraph.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[ReportGraphInfoProvider.SetReportGraphInfo]: No ReportGraphInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ReportGraphInfo info)
        {
            if (info != null)
            {
                // Delete graph's subscriptions
                ReportSubscriptionInfoProvider.DeleteSubscriptions("ReportSubscriptionGraphID=" + info.GraphID);
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Uses the query to retrieve the graph data from the database and returns the resulting table.
        /// </summary>
        /// <param name="graphObj">ReportGraphInfo object</param>
        /// <param name="parameters">Query parameters</param>
        protected DataSet GetGraphDataInternal(ReportGraphInfo graphObj, QueryDataParameters parameters)
        {
            DataSet ds = ConnectionHelper.ExecuteQuery(graphObj.GraphQuery, parameters, (graphObj.GraphQueryIsStoredProcedure ? QueryTypeEnum.StoredProcedure : QueryTypeEnum.SQLQuery));
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds;
            }

            return null;
        }

        #endregion
    }
}
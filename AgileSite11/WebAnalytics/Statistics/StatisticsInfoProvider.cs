using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing StatisticsInfo management.
    /// </summary>
    public class StatisticsInfoProvider : AbstractInfoProvider<StatisticsInfo, StatisticsInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns the StatisticsInfo structure for the specified statistics.
        /// </summary>
        /// <param name="statisticsId">Statistics id</param>
        public static StatisticsInfo GetStatisticsInfo(int statisticsId)
        {
            return ProviderObject.GetInfoById(statisticsId);
        }


        /// <summary>
        /// Retrieves statistic code names under given condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N rows</param>
        public static DataSet GetCodeNames(string where, string orderBy, int topN)
        {
            return ProviderObject.GetCodeNamesInternal(where, orderBy, topN);
        }


        /// <summary>
        /// Returns a query for all the StatisticsInfo objects.
        /// </summary>
        public static ObjectQuery<StatisticsInfo> GetStatistics()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified statistics.
        /// </summary>
        /// <param name="statistics">Statistics to set</param>
        public static void SetStatisticsInfo(StatisticsInfo statistics)
        {
            ProviderObject.SetInfo(statistics);
        }


        /// <summary>
        /// Deletes specified statistics.
        /// </summary>
        /// <param name="statisticsObj">Statistics object</param>
        public static void DeleteStatisticsInfo(StatisticsInfo statisticsObj)
        {
            ProviderObject.DeleteInfo(statisticsObj);
        }


        /// <summary>
        /// Deletes specified statistics.
        /// </summary>
        /// <param name="statisticsId">Statistics id</param>
        public static void DeleteStatisticsInfo(int statisticsId)
        {
            StatisticsInfo statisticsObj = GetStatisticsInfo(statisticsId);
            DeleteStatisticsInfo(statisticsObj);
        }


        /// <summary>
        /// Deletes all statistics with specified where condition.
        /// </summary>
        /// <param name="where">Where condition to use.</param>
        public static void DeleteStatisticsInfo(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }


        /// <summary>
        /// Removes analytics data for specified parameters
        /// </summary>
        /// <param name="from">From date</param>
        /// <param name="to">To date</param>
        /// <param name="siteId">Site id. If is 0, data from all sites are removed</param>
        /// <param name="where">Where condition, if no all data to be deleted</param>
        public static void RemoveAnalyticsData(DateTime from, DateTime to, int siteId, string where)
        {
            ProviderObject.RemoveAnalyticsDataInternal(from, to, siteId, where);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Retrieves statistic code names.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by</param>
        /// <param name="topN">Top N rows</param>
        protected virtual DataSet GetCodeNamesInternal(string where, string orderBy, int topN)
        {
            return GetObjectQuery().Distinct().TopN(topN).Column("StatisticsCode").Where(where).OrderBy(orderBy);
        }


        /// <summary>
        /// Removes analytics data for specified parameters
        /// </summary>
        /// <param name="from">From date</param>
        /// <param name="to">To date</param>
        /// <param name="siteId">Site id. If is 0, data from all sites are removed</param>
        /// <param name="where">Where condition, if no all data to be deleted</param>
        protected virtual void RemoveAnalyticsDataInternal(DateTime from, DateTime to, int siteId, String where)
        {
            // Set long timeout so that remove of statistics data can finish successfully
            using (var cs = new CMSConnectionScope())
            {
                cs.CommandTimeout = ConnectionHelper.LongRunningCommandTimeout;

                if ((from == DateTimeHelper.ZERO_TIME) && (to == DateTimeHelper.ZERO_TIME))
                {
                    // Fast delete
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("SiteID", siteId);

                    ConnectionHelper.ExecuteQuery("analytics.statistics.removeallsiteanalyticsdata", parameters, where);
                }
                else
                {
                    DateTime maxValue = DataTypeManager.MAX_DATETIME;

                    if (from == DateTimeHelper.ZERO_TIME)
                    {
                        from = DataTypeManager.MIN_DATETIME;

                        // Because of operations with datetime add year to minimum
                        from = from.AddYears(1);
                    }

                    if (to == DateTimeHelper.ZERO_TIME)
                    {
                        to = maxValue;
                    }

                    // Prevent SQL overflow
                    if (to.Date >= maxValue.Date.AddYears(-1))
                    {
                        to = to.Date.AddYears(-1);
                    }
                    else
                    {
                        // Add day to delete last selected day
                        to = to.AddDays(1);
                    }

                    // Prepare the parameters
                    QueryDataParameters parameters = new QueryDataParameters();
                    parameters.Add("From", from);
                    parameters.Add("To", to);
                    parameters.Add("SiteID", siteId);

                    var qi = QueryInfoProvider.GetQueryInfo("analytics.statistics.removeanalyticsdata");
                    if (qi != null)
                    {
                        // Replace macros in query
                        String queryText = MacroResolver.Resolve(qi.QueryText);

                        // Resolve classic query macros (##WHERE##)
                        queryText = new QueryMacros
                            {
                                Where = where
                            }
                            .ResolveMacros(queryText);

                        // Call query
                        ConnectionHelper.ExecuteQuery(queryText, parameters, QueryTypeEnum.SQLQuery);
                    }
                }
            }
        }

        #endregion
    }
}
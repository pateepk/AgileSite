using System;
using System.Data;

using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing HitsInfo management.
    /// </summary>
    public class HitsInfoProvider : AbstractInfoProvider<HitsInfo, HitsInfoProvider>
    {
        /// <summary>
        /// Returns the HitsInfo structure for the specified hits.
        /// </summary>
        /// <param name="hitsId">Hits id</param>
        /// <param name="interval">Hits interval enumerator</param>
        public static HitsInfo GetHitsInfo(int hitsId, HitsIntervalEnum interval)
        {
            HitsInfo hitsObj = null;

            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@Id", hitsId);

            // Get the data
            DataSet ds = ConnectionHelper.ExecuteQuery(HitsInfo.HitsIntervalEnumString(interval) + ".select", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                hitsObj = new HitsInfo(ds.Tables[0].Rows[0], interval);
            }

            return hitsObj;
        }


        /// <summary>
        /// Returns the HitsInfo structure for the specified time.
        /// </summary>
        /// <param name="time">Time</param>
        /// <param name="statisticsId">Statistics ID</param>
        /// <param name="interval">Hits interval enumerator</param>
        public static HitsInfo GetHitsInfo(DateTime time, int statisticsId, HitsIntervalEnum interval)
        {
            HitsInfo hitsObj = null;

            DataSet ds = ConnectionHelper.ExecuteQuery(HitsInfo.HitsIntervalEnumString(interval) + ".selectall", null,
                "(HitsStatisticsID=" + statisticsId + ") AND (HitsStartTime <= '" + time + "') AND ('" + time + "' < HitsEndTime)");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                hitsObj = new HitsInfo(ds.Tables[0].Rows[0], interval);
            }

            return hitsObj;
        }


        /// <summary>
        /// Returns all HitsInfo records for the specified time.
        /// <param name="siteId">Site ID</param>
        /// <param name="interval">Hits interval (hour, day, week, year)</param>
        /// <param name="codeName">Statistics type (pageviews, downloads...)</param>
        /// <param name="time">Time</param>
        /// </summary>
        public static DataSet GetAllHitsInfo(int siteId, HitsIntervalEnum interval, string codeName, DateTime time)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.Add("@CodeName", codeName);
            parameters.Add("@Time", time);

            parameters.AddMacro("##HITSTABLE##", HitsInfo.HitsIntervalEnumTableName(interval));

            DataSet ds = ConnectionHelper.ExecuteQuery("analytics.statistics.selectspec", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds;
            }

            return null;
        }


        /// <summary>
        /// Returns all HitsInfo records for the specified time and categories.
        /// <param name="siteId">Site ID</param>
        /// <param name="interval">Hits interval (hour, day, week, year)</param>
        /// <param name="codeNames">Statistics type (pageviews, downloads...)</param>
        /// <param name="time">Time</param>
        /// </summary>
        public static DataSet GetAllHitsInfo(int siteId, HitsIntervalEnum interval, string[] codeNames, DateTime time)
        {
            DataSet result = null;
            DataSet ds = null;
            foreach (string codename in codeNames)
            {
                ds = GetAllHitsInfo(siteId, interval, codename, time);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    if (result == null)
                    {
                        result = ds;
                    }
                    else
                    {
                        result.Merge(ds);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns all HitsInfo records for the specified time.
        /// <param name="siteId">Site ID</param>
        /// <param name="interval">Hits interval (hour, day, week, year)</param>
        /// <param name="codeName">Statistics type (abvisit%, pageviews, downloads...) which is evaluated by LIKE operator</param>
        /// <param name="startTime">Start date</param>
        /// <param name="endTime">End date</param>
        /// <param name="columns">Columns</param>
        /// <param name="culture">Culture</param>
        /// <param name="where">Additional where condition</param>
        /// </summary>
        public static DataSet GetAllHitsInfoBetween(int siteId, HitsIntervalEnum interval, string codeName, DateTime startTime, DateTime endTime, string columns, string culture = "", string where = null)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.Add("@CodeName", codeName);
            parameters.Add("@StartTime", startTime);
            parameters.Add("@EndTime", endTime);
            parameters.Add("@Culture", culture);

            parameters.AddMacro("##HITSTABLE##", HitsInfo.HitsIntervalEnumTableName(interval));

            DataSet ds = ConnectionHelper.ExecuteQuery("analytics.statistics.selectspecbetween", parameters, where, null, -1, columns);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds;
            }

            return null;
        }


        /// <summary>
        /// Returns all HitsInfo records.
        /// <param name="siteId">Site ID</param>
        /// <param name="interval">Hits interval (hour, day, week, year)</param>
        /// <param name="codeName">Statistics type (abvisit%, pageviews, downloads...) which is evaluated by LIKE operator</param>
        /// <param name="columns">Columns</param>
        /// <param name="culture">Culture</param>
        /// <param name="where">Additional where condition</param>
        /// </summary>
        public static DataSet GetAllHitsInfo(int siteId, HitsIntervalEnum interval, string codeName, string columns, string culture = "", string where = null)
        {
            return GetAllHitsInfoBetween(siteId, interval, codeName, DataTypeManager.MIN_DATETIME, DataTypeManager.MAX_DATETIME, columns, culture, where);
        }


        /// <summary>
        /// Returns all HitsInfo records for the specified time and categories.
        /// <param name="siteId">Site ID</param>
        /// <param name="interval">Hits interval (hour, day, week, year)</param>
        /// <param name="codeNames">Statistics type (abvisit%, pageviews, downloads...) which is evaluated by LIKE operator</param>
        /// <param name="startTime">Start date</param>
        /// <param name="endTime">End date</param>
        /// <param name="columns">Columns</param>
        /// </summary>
        public static DataSet GetAllHitsInfoBetween(int siteId, HitsIntervalEnum interval, string[] codeNames, DateTime startTime, DateTime endTime, string columns)
        {
            DataSet result = null;
            DataSet ds = null;
            foreach (string codename in codeNames)
            {
                ds = GetAllHitsInfoBetween(siteId, interval, codename, startTime, endTime, columns);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    if (result == null)
                    {
                        result = ds;
                    }
                    else
                    {
                        result.Merge(ds);
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns all HitsInfo records for the specified object.
        /// <param name="siteId">Site ID</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="interval">Hits interval (hour, day, week, year)</param>
        /// <param name="codeName">Statistics type (pageviews, downloads...)</param>
        /// <param name="timeBegin">Start of time interval (e.g. 01/01/2008)</param>
        /// <param name="timeEnd">End of time interval (e.q. 12/31/2008)</param>
        /// </summary>
        public static DataSet GetObjectHitsInfo(int siteId, int objectId, HitsIntervalEnum interval, string codeName,
                                                DateTime timeBegin, DateTime timeEnd)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.Add("@CodeName", codeName);
            parameters.Add("@TimeBegin", timeBegin);
            parameters.Add("@TimeEnd", timeEnd);
            parameters.Add("@ObjectID", objectId);

            parameters.AddMacro("##HITSTABLE##", HitsInfo.HitsIntervalEnumTableName(interval));

            DataSet ds = ConnectionHelper.ExecuteQuery("analytics.statistics.selectspecobj", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ds;
            }

            return null;
        }


        /// <summary>
        /// Returns all HitsInfo records for the specified object.
        /// <param name="siteId">Site ID</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="interval">Hits interval (hour, day, week, year)</param>
        /// <param name="codeName">Statistics type (pageviews, downloads...)</param>
        /// <param name="timeBegin">Start of time interval (e.g. 01/01/2008)</param>
        /// <param name="timeEnd">End of time interval (e.q. 12/31/2008)</param>
        /// </summary>
        public static int GetObjectHitCount(int siteId, int objectId, HitsIntervalEnum interval, string codeName,
                                            DateTime timeBegin, DateTime timeEnd)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@SiteID", siteId);
            parameters.Add("@CodeName", codeName);
            parameters.Add("@TimeBegin", timeBegin);
            parameters.Add("@TimeEnd", timeEnd);
            parameters.Add("@ObjectID", objectId);

            parameters.AddMacro("##HITSTABLE##", HitsInfo.HitsIntervalEnumTableName(interval));

            DataSet ds = ConnectionHelper.ExecuteQuery("analytics.statistics.selectspecobjhits", parameters);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ValidationHelper.GetInteger(ds.Tables[0].Rows[0][0], 0);
            }

            return 0;
        }


        /// <summary>
        /// Sets (updates or inserts) specified hits.
        /// </summary>
        /// <param name="hits">Hits to set</param>
        public static void SetHitsInfo(HitsInfo hits)
        {
            if (hits != null)
            {
                if (hits.HitsID > 0)
                {
                    hits.Generalized.UpdateData();
                }
                else
                {
                    hits.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[HitsInfoProvider.SetHitsInfo]: No HitsInfo object set.");
            }
        }


        /// <summary>
        /// Deletes specified hits.
        /// </summary>
        /// <param name="hits">HitsInfo object</param>
        public static void DeleteHitsInfo(HitsInfo hits)
        {
            if (hits != null)
            {
                hits.Generalized.DeleteData();
            }
        } 
    }
}
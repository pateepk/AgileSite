using System;
using System.Data;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    using TypedDataSet = InfoDataSet<HitsDayInfo>;

    /// <summary>
    /// Class providing HitsDayInfo management.
    /// </summary>
    public class HitsDayInfoProvider : AbstractInfoProvider<HitsDayInfo, HitsDayInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the HitsDayInfo objects.
        /// </summary>
        public static ObjectQuery<HitsDayInfo> GetHitsDays()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns HitsDayInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsDayInfo ID.</param>
        public static HitsDayInfo GetHitsDayInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified HitsDayInfo.
        /// </summary>
        /// <param name="infoObj">HitsDayInfo to be set.</param>
        public static void SetHitsDayInfo(HitsDayInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified HitsDayInfo.
        /// </summary>
        /// <param name="infoObj">HitsDayInfo to be deleted.</param>
        public static void DeleteHitsDayInfo(HitsDayInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsDayInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsDayInfo ID.</param>
        public static void DeleteHitsDayInfo(int id)
        {
            HitsDayInfo infoObj = GetHitsDayInfo(id);
            DeleteHitsDayInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsDayInfo with specified where condition.
        /// </summary>
        /// <param name="where">Where condition to use.</param>
        public static void DeleteHitsDayInfo(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns max (DateTo) a min (DateFrom) datetime for given statistics.
        /// </summary>
        /// <param name="where">Where condition (contains statisticscode for query)</param>
        public static DataSet GetStatisticsBoundaries(string where)
        {
            return ProviderObject.GetStatisticsBoundariesInternal(where);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns max (DateTo) a min (DateFrom) datetime for given statistics.
        /// </summary>
        /// <param name="where">Where condition (contains statisticscode for query)</param>
        protected virtual DataSet GetStatisticsBoundariesInternal(string where)
        {
            return GetObjectQuery().Columns(new AggregatedColumn(AggregationType.Min, "HitsStartTime").As("DateFrom"), new AggregatedColumn(AggregationType.Max, "HitsStartTime").As("DateTo")).Where(where);
        }

        #endregion
    }
}
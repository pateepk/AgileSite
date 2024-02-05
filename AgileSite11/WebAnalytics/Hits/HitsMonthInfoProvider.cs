using System;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing HitsMonthInfo management.
    /// </summary>
    public class HitsMonthInfoProvider : AbstractInfoProvider<HitsMonthInfo, HitsMonthInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the HitsMonthInfo objects.
        /// </summary>
        public static ObjectQuery<HitsMonthInfo> GetHitsMonths()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns HitsMonthInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsMonthInfo ID.</param>
        public static HitsMonthInfo GetHitsMonthInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified HitsMonthInfo.
        /// </summary>
        /// <param name="infoObj">HitsMonthInfo to be set.</param>
        public static void SetHitsMonthInfo(HitsMonthInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified HitsMonthInfo.
        /// </summary>
        /// <param name="infoObj">HitsMonthInfo to be deleted.</param>
        public static void DeleteHitsMonthInfo(HitsMonthInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsMonthInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsMonthInfo ID.</param>
        public static void DeleteHitsMonthInfo(int id)
        {
            HitsMonthInfo infoObj = GetHitsMonthInfo(id);
            DeleteHitsMonthInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsMonthInfo with specified where condition.
        /// </summary>
        /// <param name="where">Where condition to use.</param>
        public static void DeleteHitsMonthInfo(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion
    }
}
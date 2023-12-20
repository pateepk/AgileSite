using System;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing HitsWeekInfo management.
    /// </summary>
    public class HitsWeekInfoProvider : AbstractInfoProvider<HitsWeekInfo, HitsWeekInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the HitsWeekInfo objects.
        /// </summary>
        public static ObjectQuery<HitsWeekInfo> GetHitsWeeks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns HitsWeekInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsWeekInfo ID.</param>
        public static HitsWeekInfo GetHitsWeekInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified HitsWeekInfo.
        /// </summary>
        /// <param name="infoObj">HitsWeekInfo to be set.</param>
        public static void SetHitsWeekInfo(HitsWeekInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified HitsWeekInfo.
        /// </summary>
        /// <param name="infoObj">HitsWeekInfo to be deleted.</param>
        public static void DeleteHitsWeekInfo(HitsWeekInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsWeekInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsWeekInfo ID.</param>
        public static void DeleteHitsWeekInfo(int id)
        {
            HitsWeekInfo infoObj = GetHitsWeekInfo(id);
            DeleteHitsWeekInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsWeekInfo with specified where condition.
        /// </summary>
        /// <param name="where">Where condition to use.</param>
        public static void DeleteHitsWeekInfo(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion
    }
}
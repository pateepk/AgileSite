using System;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{

    /// <summary>
    /// Class providing HitsHourInfo management.
    /// </summary>
    public class HitsHourInfoProvider : AbstractInfoProvider<HitsHourInfo, HitsHourInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the HitsHourInfo objects.
        /// </summary>
        public static ObjectQuery<HitsHourInfo> GetHitsHours()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns HitsHourInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsHourInfo ID.</param>
        public static HitsHourInfo GetHitsHourInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified HitsHourInfo.
        /// </summary>
        /// <param name="infoObj">HitsHourInfo to be set.</param>
        public static void SetHitsHourInfo(HitsHourInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified HitsHourInfo.
        /// </summary>
        /// <param name="infoObj">HitsHourInfo to be deleted.</param>
        public static void DeleteHitsHourInfo(HitsHourInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsHourInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsHourInfo ID.</param>
        public static void DeleteHitsHourInfo(int id)
        {
            HitsHourInfo infoObj = GetHitsHourInfo(id);
            DeleteHitsHourInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsHourInfo with specified where condition.
        /// </summary>
        /// <param name="where">Where condition to use.</param>
        public static void DeleteHitsHourInfo(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion
    }
}
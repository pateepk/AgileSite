using System;

using CMS.DataEngine;

namespace CMS.WebAnalytics
{
    /// <summary>
    /// Class providing HitsYearInfo management.
    /// </summary>
    public class HitsYearInfoProvider : AbstractInfoProvider<HitsYearInfo, HitsYearInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the HitsYearInfo objects.
        /// </summary>
        public static ObjectQuery<HitsYearInfo> GetHitsYears()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns HitsYearInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsYearInfo ID.</param>
        public static HitsYearInfo GetHitsYearInfo(int id)
        {
            return ProviderObject.GetInfoById(id);
        }


        /// <summary>
        /// Sets (updates or inserts) specified HitsYearInfo.
        /// </summary>
        /// <param name="infoObj">HitsYearInfo to be set.</param>
        public static void SetHitsYearInfo(HitsYearInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified HitsYearInfo.
        /// </summary>
        /// <param name="infoObj">HitsYearInfo to be deleted.</param>
        public static void DeleteHitsYearInfo(HitsYearInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsYearInfo with specified ID.
        /// </summary>
        /// <param name="id">HitsYearInfo ID.</param>
        public static void DeleteHitsYearInfo(int id)
        {
            HitsYearInfo infoObj = GetHitsYearInfo(id);
            DeleteHitsYearInfo(infoObj);
        }


        /// <summary>
        /// Deletes HitsYearInfo with specified where condition.
        /// </summary>
        /// <param name="where">Where condition to use.</param>
        public static void DeleteHitsYearInfo(IWhereCondition where)
        {
            ProviderObject.BulkDelete(where);
        }

        #endregion
    }
}
using System.Data;

using CMS.DataEngine;


namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class providing ExportHistoryInfo management.
    /// </summary>
    public class ExportHistoryInfoProvider : AbstractInfoProvider<ExportHistoryInfo, ExportHistoryInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns all export histories.
        /// </summary>
        public static ObjectQuery<ExportHistoryInfo> GetExportHistories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the ExportHistoryInfo structure for the specified export history.
        /// </summary>
        /// <param name="exportHistoryId">Export history ID</param>
        public static ExportHistoryInfo GetExportHistoryInfo(int exportHistoryId)
        {
            return ProviderObject.GetInfoById(exportHistoryId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified exportHistory.
        /// </summary>
        /// <param name="exportHistory">Export history info object</param>
        public static void SetExportHistoryInfo(ExportHistoryInfo exportHistory)
        {
            ProviderObject.SetInfo(exportHistory);
        }


        /// <summary>
        /// Deletes specified export history.
        /// </summary>
        /// <param name="exportHistoryObj">Export history info object</param>
        public static void DeleteExportHistoryInfo(ExportHistoryInfo exportHistoryObj)
        {
            ProviderObject.DeleteInfo(exportHistoryObj);
        }


        /// <summary>
        /// Deletes specified export history.
        /// </summary>
        /// <param name="exportHistoryId">Export history ID</param>
        public static void DeleteExportHistoryInfo(int exportHistoryId)
        {
            ProviderObject.DeleteExportHistoryInfoInternal(exportHistoryId);
        }


        /// <summary>
        /// Deletes all export histories for given site. When site is 0, deletes all global histories.
        /// </summary>
        /// <param name="site">Site identifier: ID or site name</param>
        public static void DeleteExportHistories(SiteInfoIdentifier site)
        {
            ProviderObject.DeleteExportHistoriesInternal(site);
        }


        /// <summary>
        /// Returns the export histories for the specified site.
        /// </summary>
        /// <param name="site">Site identifier: ID or site name</param>
        public static DataSet GetExportHistories(SiteInfoIdentifier site)
        {
            return ProviderObject.GetExportHistoriesInternal(site);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Deletes specified export history.
        /// </summary>
        /// <param name="exportHistoryId">Export history ID</param>
        protected virtual void DeleteExportHistoryInfoInternal(int exportHistoryId)
        {
            ExportHistoryInfo exportHistoryObj = GetExportHistoryInfo(exportHistoryId);
            DeleteExportHistoryInfo(exportHistoryObj);
        }


        /// <summary>
        /// Deletes all export histories for given site. When site is 0, deletes all global histories.
        /// </summary>
        /// <param name="site">Site identifier: ID or site name. If not provided, deletes all histories.</param>
        protected virtual void DeleteExportHistoriesInternal(SiteInfoIdentifier site)
        {
            var where = (site != null) ? new WhereCondition().WhereID("ExportSiteID", site.ObjectID) : null;

            BulkDelete(where);
        }


        /// <summary>
        /// Returns the export histories for the specified site.
        /// </summary>
        /// <param name="site">Site identifier: ID or site name</param>
        protected virtual ObjectQuery<ExportHistoryInfo> GetExportHistoriesInternal(SiteInfoIdentifier site)
        {
            return GetExportHistories().Where("ExportSiteID", QueryOperator.Equals, site.ObjectID);
        }

        #endregion
    }
}
using System;

using CMS.DataEngine;

namespace CMS.Reporting
{
    /// <summary>
    /// Class providing SavedReportInfo management.
    /// </summary>
    public class SavedReportInfoProvider : AbstractInfoProvider<SavedReportInfo, SavedReportInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns saved reports.
        /// </summary>
        public static ObjectQuery<SavedReportInfo> GetSavedReports()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the SavedReportInfo structure for the specified savedReport.
        /// </summary>
        /// <param name="savedReportId">SavedReport id</param>
        public static SavedReportInfo GetSavedReportInfo(int savedReportId)
        {
            return ProviderObject.GetInfoById(savedReportId);
        }


        /// <summary>
        /// Returns the SavedReportInfo structure for the specified savedReport.
        /// </summary>
        /// <param name="savedReportGuid">SavedReport GUID</param>
        public static SavedReportInfo GetSavedReportInfo(Guid savedReportGuid)
        {
            return ProviderObject.GetInfoByGuid(savedReportGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified savedReport.
        /// </summary>
        /// <param name="savedReport">SavedReport to set</param>
        public static void SetSavedReportInfo(SavedReportInfo savedReport)
        {
            ProviderObject.SetInfo(savedReport);
        }


        /// <summary>
        /// Deletes specified savedReport.
        /// </summary>
        /// <param name="savedReportObj">SavedReport object</param>
        public static void DeleteSavedReportInfo(SavedReportInfo savedReportObj)
        {
            ProviderObject.DeleteInfo(savedReportObj);
        }


        /// <summary>
        /// Deletes specified savedReport.
        /// </summary>
        /// <param name="savedReportId">SavedReport id</param>
        public static void DeleteSavedReportInfo(int savedReportId)
        {
            SavedReportInfo savedReportObj = GetSavedReportInfo(savedReportId);
            DeleteSavedReportInfo(savedReportObj);
        }

        #endregion
    }
}
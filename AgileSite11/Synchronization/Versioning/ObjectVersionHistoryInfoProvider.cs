using System;

using CMS.DataEngine;
using CMS.LicenseProvider;

namespace CMS.Synchronization
{
    using TypedDataSet = InfoDataSet<ObjectVersionHistoryInfo>;

    /// <summary>
    /// Class providing ObjectVersionHistoryInfo management.
    /// </summary>
    public class ObjectVersionHistoryInfoProvider : AbstractInfoProvider<ObjectVersionHistoryInfo, ObjectVersionHistoryInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns all object version histories.
        /// </summary>
        public static ObjectQuery<ObjectVersionHistoryInfo> GetVersionHistories()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all object version histories matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        [Obsolete("Use method GetVersionHistories() instead.")]
        public static TypedDataSet GetVersionHistories(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetVersionHistoriesInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns object version history with specified ID.
        /// </summary>
        /// <param name="historyId">Object version history ID</param>        
        public static ObjectVersionHistoryInfo GetVersionHistoryInfo(int historyId)
        {
            return ProviderObject.GetInfoById(historyId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified object version history.
        /// </summary>
        /// <param name="historyObj">Object version history to be set</param>
        public static void SetVersionHistoryInfo(ObjectVersionHistoryInfo historyObj)
        {
            ProviderObject.SetInfo(historyObj);
        }


        /// <summary>
        /// Deletes specified object version history.
        /// </summary>
        /// <param name="historyObj">Object version history to be deleted</param>
        public static void DeleteVersionHistoryInfo(ObjectVersionHistoryInfo historyObj)
        {
            ProviderObject.DeleteInfo(historyObj);
        }


        /// <summary>
        /// Deletes object version history with specified ID.
        /// </summary>
        /// <param name="historyId">Object version history ID</param>
        public static void DeleteVersionHistoryInfo(int historyId)
        {
            ObjectVersionHistoryInfo historyObj = GetVersionHistoryInfo(historyId);
            DeleteVersionHistoryInfo(historyObj);
        }


        /// <summary>
        /// Delete version histories matching the specified where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        public static void DeleteVersionHistories(string where)
        {
            ProviderObject.DeleteVersionHistoriesInternal(where);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets recycle bin.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <returns>Recycle bin </returns>
        public static TypedDataSet GetRecycleBin(string where, string orderBy, int topN, string columns)
        {
            return GetRecycleBin(0, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets recycle bin for specified user.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <returns>Recycle bin for specified user</returns>
        public static TypedDataSet GetRecycleBin(int userId, string where, string orderBy, int topN, string columns)
        {
            // Add user condition
            if (userId > 0)
            {
                where = SqlHelper.AddWhereCondition(where, "VersionDeletedByUserID = " + userId);
            }

            where = SqlHelper.AddWhereCondition(where, "VersionDeletedWhen IS NOT NULL");

            return GetVersionHistories().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns dataset of all object version histories matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>        
        /// <param name="columns">Columns to be selected</param>
        [Obsolete("Use method GetVersionHistories() instead.")]
        protected virtual TypedDataSet GetVersionHistoriesInternal(string where, string orderBy, int topN, string columns)
        {
            return GetVersionHistories().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true).TypedResult;
        }


        /// <summary>
        /// Delete version histories matching the specified where condition.
        /// </summary>
        /// <param name="where">Where condition</param>
        protected virtual void DeleteVersionHistoriesInternal(string where)
        {
            // Get histories select query
            var selectHistories = "SELECT VersionID FROM CMS_ObjectVersionHistory WHERE " + where;

            // Update related object settings
            var updateObjectSettings = string.Format("UPDATE CMS_ObjectSettings SET ObjectCheckedOutVersionHistoryID = NULL WHERE ObjectCheckedOutVersionHistoryID IN ({0})", selectHistories);
            ConnectionHelper.ExecuteQuery(updateObjectSettings, null, QueryTypeEnum.SQLQuery);

            // Delete histories
            BulkDelete(new WhereCondition(where));
        }


        /// <summary>
        /// Check license limitation
        /// </summary>
        /// <param name="domainName">Domain name</param>
        public static bool CheckLicense(string domainName)
        {
            LicenseHelper.RequestFeature(domainName, FeatureEnum.ObjectVersioning);
            return true;
        }

        #endregion
    }
}
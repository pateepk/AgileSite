using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing VersionHistoryInfo management.
    /// </summary>
    public class VersionHistoryInfoProvider : AbstractInfoProvider<VersionHistoryInfo, VersionHistoryInfoProvider>
    {
        #region "Constructor"

        /// <summary>
        /// Constructor
        /// </summary>
        public VersionHistoryInfoProvider()
            : base(VersionHistoryInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                UseWeakReferences = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the VersionHistoryInfo structure for the specified versionHistory.
        /// </summary>
        /// <param name="versionHistoryId">VersionHistory ID</param>
        public static VersionHistoryInfo GetVersionHistoryInfo(int versionHistoryId)
        {
            return ProviderObject.GetInfoById(versionHistoryId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified versionHistory.
        /// </summary>
        /// <param name="versionHistory">VersionHistory to set</param>
        public static void SetVersionHistoryInfo(VersionHistoryInfo versionHistory)
        {
            ProviderObject.SetInfo(versionHistory);
        }


        /// <summary>
        /// Deletes specified versionHistory.
        /// </summary>
        /// <param name="versionHistory">VersionHistory object</param>
        public static void DeleteVersionHistoryInfo(VersionHistoryInfo versionHistory)
        {
            ProviderObject.DeleteInfo(versionHistory);
        }


        /// <summary>
        /// Deletes specified versionHistory.
        /// </summary>
        /// <param name="versionHistoryId">VersionHistory ID</param>
        public static void DeleteVersionHistoryInfo(int versionHistoryId)
        {
            var version = GetVersionHistoryInfo(versionHistoryId);
            DeleteVersionHistoryInfo(version);
        }


        /// <summary>
        /// Returns the query for all version histories.
        /// </summary>
        public static ObjectQuery<VersionHistoryInfo> GetVersionHistories()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Gets recycle bin for specified user.
        /// </summary>
        /// <param name="siteId">ID of site</param>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="modifiedFrom">Lower time bound</param>
        /// <param name="modifiedTo">Upper time bound</param>
        /// <returns>Recycle bin for specified user and site</returns>
        public static ObjectQuery<VersionHistoryInfo> GetRecycleBin(int siteId, int userId = 0, string where = null, string orderBy = null, int topN = 0, string columns = null, DateTime? modifiedFrom = null, DateTime? modifiedTo = null)
        {
            modifiedFrom = modifiedFrom ?? DateTimeHelper.ZERO_TIME;
            modifiedTo = modifiedTo ?? DateTimeHelper.ZERO_TIME;

            return ProviderObject.GetRecycleBinInternal(siteId, userId, where, orderBy, topN, columns, modifiedFrom.Value, modifiedTo.Value);
        }


        /// <summary>
        /// Moves histories of given document to new site.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="siteId">New site ID</param>
        public static void MoveHistories(int documentId, int siteId)
        {
            ProviderObject.MoveHistoriesInternal(documentId, siteId);
        }


        /// <summary>
        /// Changes version histories document.
        /// </summary>
        /// <param name="originalDocumentId">Document ID</param>
        /// <param name="newDocumentId">New document ID</param>
        public static void ChangeDocument(int originalDocumentId, int newDocumentId)
        {
            ProviderObject.ChangeDocumentInternal(originalDocumentId, newDocumentId);
        }


        /// <summary>
        /// Returns all version histories based on list of IDs. Uses cached versions from memory and pulls rest of the versions from the database.
        /// </summary>
        /// <param name="ids">List of version history IDs</param>
        internal static SafeDictionary<int, BaseInfo> GetVersionHistories(IEnumerable<int> ids)
        {
            return ProviderHelper.GetInfosByIds(VersionHistoryInfo.OBJECT_TYPE, ids);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Updates data for all records given by where condition
        /// </summary>
        /// <param name="updateExpression">Update expression, e.g. "Value = Value * 2"</param>
        /// <param name="where">Where condition</param>
        /// <param name="parameters">Parameters</param>
        internal static void UpdateData(string updateExpression, string where, QueryDataParameters parameters)
        {
            ProviderObject.UpdateData(updateExpression, parameters, where);
        }


        /// <summary>
        /// Gets recycle bin for specified user.
        /// </summary>
        /// <param name="siteId">ID of site</param>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="modifiedFrom">Lower time bound</param>
        /// <param name="modifiedTo">Upper time bound</param>
        /// <returns>Recycle bin for specified user and site</returns>
        protected virtual ObjectQuery<VersionHistoryInfo> GetRecycleBinInternal(int siteId, int userId, string where, string orderBy, int topN, string columns, DateTime modifiedFrom, DateTime modifiedTo)
        {
            return GetVersionHistories()
                .Where(GetRecycleBinWhereCondition(siteId, userId, modifiedFrom, modifiedTo))
                .Where(where)
                .OrderBy(orderBy)
                .TopN(topN)
                .Columns(columns);
        }


        private static IWhereCondition GetRecycleBinWhereCondition(int siteId, int userId, DateTime modifiedFrom, DateTime modifiedTo)
        {
            var where = new WhereCondition()
                .WhereNotNull("VersionDeletedWhen");

            if (siteId > 0)
            {
                where.WhereEquals("NodeSiteID", siteId);
            }

            if (userId > 0)
            {
                where.WhereContains("VersionDeletedByUserID", userId.ToString());
            }

            if (modifiedFrom != DateTimeHelper.ZERO_TIME)
            {
                where.WhereGreaterOrEquals("ModifiedWhen", modifiedFrom);
            }

            if (modifiedTo != DateTimeHelper.ZERO_TIME)
            {
                where.WhereLessOrEquals("ModifiedWhen", modifiedTo);
            }

            return where;
        }


        /// <summary>
        /// Moves histories of given document to different site.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="siteId">New site ID</param>
        protected virtual void MoveHistoriesInternal(int documentId, int siteId)
        {
            // Move the VersionHistories
            var parameters = new QueryDataParameters();
            parameters.Add("@DocumentID", documentId);
            parameters.Add("@NewSiteID", siteId);

            UpdateData("[NodeSiteID] = @NewSiteID", parameters, "[DocumentID] = @DocumentID");
        }


        /// <summary>
        /// Changes version histories document.
        /// </summary>
        /// <param name="originalDocumentId">Document ID</param>
        /// <param name="newDocumentId">New document ID</param>
        protected virtual void ChangeDocumentInternal(int originalDocumentId, int newDocumentId)
        {
            // Prepare the parameters for the document ID change
            var parameters = new QueryDataParameters();
            parameters.Add("@OriginalDocumentID", originalDocumentId);
            parameters.Add("@NewDocumentID", newDocumentId);

            UpdateData("[DocumentID] = @NewDocumentID", parameters, "[DocumentID] = @OriginalDocumentID");
        }

        #endregion
    }
}

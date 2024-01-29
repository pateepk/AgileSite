using System;

using CMS.DataEngine;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Class providing TranslationSubmissionItemInfo management.
    /// </summary>
    public class TranslationSubmissionItemInfoProvider : AbstractInfoProvider<TranslationSubmissionItemInfo, TranslationSubmissionItemInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public TranslationSubmissionItemInfoProvider()
            : base(TranslationSubmissionItemInfo.TYPEINFO, new HashtableSettings
				{
					ID = true
				})
        {
        }

        #endregion


        #region "Public methods - Basic"
               
        /// <summary>
        /// Returns submission item with specified ID.
        /// </summary>
        /// <param name="itemId">Submission item ID.</param>        
        public static TranslationSubmissionItemInfo GetTranslationSubmissionItemInfo(int itemId)
        {
            return ProviderObject.GetInfoById(itemId);
        }


        /// <summary>
        /// Returns submission item with specified GUIID.
        /// </summary>
        /// <param name="itemGuid">Submission item GUIID.</param>        
        public static TranslationSubmissionItemInfo GetTranslationSubmissionItemInfo(Guid itemGuid)
        {
            return ProviderObject.GetInfoByGuid(itemGuid);
        }


        /// <summary>
        /// Sets (updates or inserts) specified submission item.
        /// </summary>
        /// <param name="itemObj">Submission item to be set.</param>
        public static void SetTranslationSubmissionItemInfo(TranslationSubmissionItemInfo itemObj)
        {
            ProviderObject.SetInfo(itemObj);
        }


        /// <summary>
        /// Deletes specified submission item.
        /// </summary>
        /// <param name="itemObj">Submission item to be deleted.</param>
        public static void DeleteTranslationSubmissionItemInfo(TranslationSubmissionItemInfo itemObj)
        {
            ProviderObject.DeleteInfo(itemObj);
        }


        /// <summary>
        /// Deletes submission item with specified ID.
        /// </summary>
        /// <param name="itemId">Submission item ID.</param>
        public static void DeleteTranslationSubmissionItemInfo(int itemId)
        {
            TranslationSubmissionItemInfo itemObj = GetTranslationSubmissionItemInfo(itemId);
            DeleteTranslationSubmissionItemInfo(itemObj);
        }


        /// <summary>
        /// Returns the query of all translation services.
        /// </summary>
        public static ObjectQuery<TranslationSubmissionItemInfo> GetTranslationSubmissionItems()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all submission items matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        public static ObjectQuery<TranslationSubmissionItemInfo> GetTranslationSubmissionItems(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetTranslationSubmissionItems().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
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

        #endregion
    }
}

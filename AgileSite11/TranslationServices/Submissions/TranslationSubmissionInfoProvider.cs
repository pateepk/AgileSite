using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;

namespace CMS.TranslationServices
{
    /// <summary>
    /// Class providing TranslationSubmissionInfo management.
    /// </summary>
    public class TranslationSubmissionInfoProvider : AbstractInfoProvider<TranslationSubmissionInfo, TranslationSubmissionInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public TranslationSubmissionInfoProvider()
            : base(TranslationSubmissionInfo.TYPEINFO, new HashtableSettings
                {
                    ID = true
                })
        {
        }

        #endregion


        #region "Public methods - Basic"


        /// <summary>
        /// Returns submission with specified ID.
        /// </summary>
        /// <param name="submissionId">Submission ID.</param>        
        public static TranslationSubmissionInfo GetTranslationSubmissionInfo(int submissionId)
        {
            return ProviderObject.GetInfoById(submissionId);
        }


        /// <summary>
        /// Returns submission with specified GUID.
        /// </summary>
        /// <param name="submissionGuid">Submission GUID.</param>        
        public static TranslationSubmissionInfo GetTranslationSubmissionInfo(Guid submissionGuid)
        {
            return ProviderObject.GetInfoByGuid(submissionGuid);
        }


        /// <summary>
        /// Returns submission with specified name.
        /// </summary>
        /// <param name="submissionName">Submission name.</param>                
        public static TranslationSubmissionInfo GetTranslationSubmissionInfo(string submissionName)
        {
            return ProviderObject.GetInfoByCodeName(submissionName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified submission.
        /// </summary>
        /// <param name="submissionObj">Submission to be set.</param>
        public static void SetTranslationSubmissionInfo(TranslationSubmissionInfo submissionObj)
        {
            ProviderObject.SetInfo(submissionObj);
        }


        /// <summary>
        /// Deletes specified submission.
        /// </summary>
        /// <param name="submissionObj">Submission to be deleted.</param>
        public static void DeleteTranslationSubmissionInfo(TranslationSubmissionInfo submissionObj)
        {
            ProviderObject.DeleteInfo(submissionObj);
        }


        /// <summary>
        /// Deletes submission with specified ID.
        /// </summary>
        /// <param name="submissionId">Submission ID.</param>
        public static void DeleteTranslationSubmissionInfo(int submissionId)
        {
            TranslationSubmissionInfo submissionObj = GetTranslationSubmissionInfo(submissionId);
            DeleteTranslationSubmissionInfo(submissionObj);
        }


        /// <summary>
        /// Returns the query of all translation submissions.
        /// </summary>
        public static ObjectQuery<TranslationSubmissionInfo> GetTranslationSubmissions()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns dataset of all submissions matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition.</param>
        /// <param name="orderBy">Order by expression.</param>
        /// <param name="topN">Number of records to be selected.</param>        
        /// <param name="columns">Columns to be selected.</param>
        public static ObjectQuery<TranslationSubmissionInfo> GetSubmissions(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetTranslationSubmissions()
                    .TopN(topN)
                    .Columns(columns)
                    .Where(where)
                    .OrderBy(orderBy);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns list of document names submitted within given submission.
        /// </summary>
        /// <param name="submissionId">ID of the submission</param>
        /// <param name="documentsCount">Maximal number of documents to return</param>
        public static List<string> GetSubmissionDocuments(int submissionId, int documentsCount)
        {
            return ProviderObject.GetSubmissionDocumentsInternal(submissionId, documentsCount);
        }


        /// <summary>
        /// Updates statuses for all submissions given by where condition.
        /// </summary>
        /// <param name="status">Translation status</param>
        /// <param name="where">Where condition</param>
        public static void UpdateStatuses(TranslationStatusEnum status, string where)
        {
            ProviderObject.UpdateStatusesInternal(status, where);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns list of document names submitted within given submission.
        /// </summary>
        /// <param name="submissionId">ID of the submission</param>
        /// <param name="documentsCount">Maximal number of documents to return</param>
        protected virtual List<string> GetSubmissionDocumentsInternal(int submissionId, int documentsCount)
        {
            List<string> result = new List<string>();

            // Get the data
            var tree = new TreeProvider();
            DataSet ds = tree.SelectNodes()
                .All()
                .TopN(documentsCount)
                .Column("DocumentName")
                .WhereIn("DocumentID", new IDQuery(TranslationSubmissionItemInfo.OBJECT_TYPE, "SubmissionItemObjectID").WhereEquals("SubmissionItemSubmissionID", submissionId))
                .OrderBy("DocumentName");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                result = ds.Tables[0].Rows.Cast<DataRow>().Select(t => t["DocumentName"].ToString()).ToList();
            }

            return result;
        }


        /// <summary>
        /// Updates statuses for all submissions given by where condition.
        /// </summary>
        /// <param name="status">Translation status</param>
        /// <param name="where">Where condition</param>
        protected virtual void UpdateStatusesInternal(TranslationStatusEnum status, string where)
        {
            var parameters = new QueryDataParameters();
            parameters.Add(new DataParameter("status", (int)status));

            UpdateData("SubmissionStatus = @status", parameters, where);
        }

        #endregion
    }
}

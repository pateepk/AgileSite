using System;
using System.Linq;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing VersionAttachmentInfo management.
    /// </summary>
    public class VersionAttachmentInfoProvider : AbstractInfoProvider<VersionAttachmentInfo, VersionAttachmentInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns version attachment with specified ID.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="attachmentHistoryId">Attachment version history ID</param>     
        public static VersionAttachmentInfo GetVersionAttachmentInfo(int versionHistoryId, int attachmentHistoryId)
        {
            return ProviderObject.GetVersionAttachmentInfoInternal(versionHistoryId, attachmentHistoryId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified version attachment binding.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="attachmentHistoryId">Attachment version history ID</param>
        public static void SetVersionAttachmentInfo(int versionHistoryId, int attachmentHistoryId)
        {
            VersionAttachmentInfo versionAttachment = new VersionAttachmentInfo
            {
                AttachmentHistoryID = attachmentHistoryId,
                VersionHistoryID = versionHistoryId
            };

            SetVersionAttachmentInfo(versionAttachment);
        }


        /// <summary>
        /// Sets (updates or inserts) specified version attachment binding.
        /// </summary>
        /// <param name="attachmentObj">Version attachment to be set.</param>
        public static void SetVersionAttachmentInfo(VersionAttachmentInfo attachmentObj)
        {
            ProviderObject.SetInfo(attachmentObj);
        }


        /// <summary>
        /// Deletes version attachment with specified ID.
        /// </summary>
        /// <param name="versionHistoryId">Document version history ID</param>
        /// <param name="attachmentHistoryId">Attachment version history ID</param>
        public static void DeleteVersionAttachmentInfo(int versionHistoryId, int attachmentHistoryId)
        {
            VersionAttachmentInfo attachmentObj = GetVersionAttachmentInfo(versionHistoryId, attachmentHistoryId);
            DeleteVersionAttachmentInfo(attachmentObj);
        }


        /// <summary>
        /// Deletes specified version attachment.
        /// </summary>
        /// <param name="attachmentObj">Version attachment to be deleted.</param>
        public static void DeleteVersionAttachmentInfo(VersionAttachmentInfo attachmentObj)
        {
            ProviderObject.DeleteInfo(attachmentObj);
        }


        /// <summary>
        /// Gets the query for all version attachments.
        /// </summary>
        public static ObjectQuery<VersionAttachmentInfo> GetVersionAttachments()
        {
            return ProviderObject.GetObjectQuery();
        }
        
        #endregion
        

        #region "Internal methods - Basic"

        /// <summary>
        /// Returns version attachment binding.
        /// </summary>
        /// <param name="versionHistoryId">Identifier for document history</param>
        /// <param name="attachmentHistoryId">Identifier for attachment history</param>
        protected virtual VersionAttachmentInfo GetVersionAttachmentInfoInternal(int versionHistoryId, int attachmentHistoryId)
        {
            WhereCondition condition = new WhereCondition().WhereEquals("VersionHistoryID", versionHistoryId)
                                                           .WhereEquals(VersionAttachmentInfo.TYPEINFO.ParentIDColumn, attachmentHistoryId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }

        #endregion


        #region "Copy method"

        /// <summary>
        /// Copies bindings between attachment history and version from source to target version.
        /// </summary>
        /// <param name="sourceVersionHistoryId">Source version ID.</param>
        /// <param name="targetVersionHistoryId">Target version ID.</param>
        internal static void CopyVersionAttachments(int sourceVersionHistoryId, int targetVersionHistoryId)
        {
            var bindings = GetVersionAttachments().WhereEquals("VersionHistoryID", sourceVersionHistoryId).ToList();
            bindings.ForEach(binding => binding.VersionHistoryID = targetVersionHistoryId);

            ProviderObject.BulkInsertInfos(bindings);
        }

        #endregion
    }
}

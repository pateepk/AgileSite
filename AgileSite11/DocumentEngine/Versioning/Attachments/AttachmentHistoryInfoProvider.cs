using System;
using System.Collections.Generic;

using CMS.DataEngine;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing attachment history management.
    /// </summary>
    public class AttachmentHistoryInfoProvider : AbstractInfoProvider<AttachmentHistoryInfo, AttachmentHistoryInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns the AttachmentHistory structure for the specified attachment history.
        /// </summary>
        /// <param name="attachmentHistoryId">ID of attachment history</param>
        public static AttachmentHistoryInfo GetAttachmentHistory(int attachmentHistoryId)
        {
            return ProviderObject.GetInfoById(attachmentHistoryId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified attachment history.
        /// </summary>
        /// <param name="attachmentHistory">Attachment history to set</param>
        public static void SetAttachmentHistory(AttachmentHistoryInfo attachmentHistory)
        {
            ProviderObject.SetInfo(attachmentHistory);
        }


        /// <summary>
        /// Deletes specified attachment history.
        /// </summary>
        /// <param name="attachmentHistory">Attachment history object</param>
        public static void DeleteAttachmentHistory(AttachmentHistoryInfo attachmentHistory)
        {
            ProviderObject.DeleteInfo(attachmentHistory);
        }


        /// <summary>
        /// Deletes specified attachment history.
        /// </summary>
        /// <param name="attachmentHistoryId">ID of attachment history</param>
        public static void DeleteAttachmentHistory(int attachmentHistoryId)
        {
            AttachmentHistoryInfo attachmentHistoryObj = GetAttachmentHistory(attachmentHistoryId);
            DeleteAttachmentHistory(attachmentHistoryObj);
        }


        /// <summary>
        /// Gets all attachment histories.
        /// </summary>
        /// <remarks>The IncludeBinaryData property and the BinaryData method don't load binary data 
        /// for attachments stored on the filesystem. To load binary data for attachments stored on the 
        /// filesystem, use the AttachmentBinary property of every record.</remarks>
        public static ObjectQuery<AttachmentHistoryInfo> GetAttachmentHistories()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Public methods - Advanced"
        
        /// <summary>
        /// Moves attachment histories of given document to new site.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="siteId">New site ID</param>
        public static void MoveHistories(int documentId, int siteId)
        {
            ProviderObject.MoveHistoriesInternal(documentId, siteId);
        }


        /// <summary>
        /// Changes attachment histories document.
        /// </summary>
        /// <param name="originalDocumentId">Document ID</param>
        /// <param name="newDocumentId">New document ID</param>
        public static void ChangeDocument(int originalDocumentId, int newDocumentId)
        {
            ProviderObject.ChangeDocumentInternal(originalDocumentId, newDocumentId);
        }


        /// <summary>
        /// Gets the attachment histories folder for storing them in file system
        /// </summary>
        /// <param name="siteName">Site name</param>
        internal static string GetAttachmentHistoriesFolder(string siteName)
        {
            return String.Format("~/App_Data/VersionHistory/Attachments/{0}", siteName);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(AttachmentHistoryInfo info)
        {
            if (info != null)
            {
                info.EnsureGUID();
            }

            base.SetInfo(info);
        }

        #endregion


        #region "Internal methods - Advanced"
        
        /// <summary>
        /// Moves attachment histories of given document to different site.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="siteId">New site ID</param>
        protected virtual void MoveHistoriesInternal(int documentId, int siteId)
        {
            UpdateData(
                new WhereCondition().WhereEquals("AttachmentDocumentID", documentId),
                new Dictionary<string, object> { { "AttachmentSiteID", siteId } },
                true
            );
        }


        /// <summary>
        /// Changes attachment histories document.
        /// </summary>
        /// <param name="originalDocumentId">Document ID</param>
        /// <param name="newDocumentId">New document ID</param>
        protected virtual void ChangeDocumentInternal(int originalDocumentId, int newDocumentId)
        {
            UpdateData(
                new WhereCondition().WhereEquals("AttachmentDocumentID", originalDocumentId),
                new Dictionary<string, object> { { "AttachmentDocumentID", newDocumentId } },
                true
            );
        }


        /// <summary>
        /// Indicates whether the given attachment name is unique.
        /// </summary>
        /// <param name="node">Node which the attachment belongs to</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="extension">Extension of file</param>
        /// <param name="currentAttachmentHistoryId">Current attachment ID</param>
        internal static bool IsUniqueAttachmentName(TreeNode node, string fileName, string extension, int currentAttachmentHistoryId)
        {
            var attachmentsWithSameName = AttachmentHistoryInfoProvider.GetAttachmentHistories()
                .InVersionExceptVariants(node.DocumentCheckedOutVersionHistoryID)
                .WhereEquals("AttachmentName", fileName)
                .WhereEquals("AttachmentExtension", extension)
                .WhereNotEquals("AttachmentHistoryID", currentAttachmentHistoryId);

            return (attachmentsWithSameName.Count == 0);
        }

        #endregion
    }
}
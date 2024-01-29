using System;
using System.Data;
using System.Text.RegularExpressions;
using System.Web;
using System.Linq;

using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Class providing attachment management.
    /// </summary>
    public class AttachmentInfoProvider : AbstractInfoProvider<AttachmentInfo, AttachmentInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Maximal length of the attachment name.
        /// </summary>
        public const int MAXATTACHMENTNAMELENGTH = 255;

        #endregion


        #region "Variables"

        /// <summary>
        /// Specifies how old (in hours) should be attachments deleted by the 'Delete old temporary attachments' scheduled task.
        /// </summary>
        private static int? mDeleteTemporaryAttachmentsOlderThan;

        #endregion


        #region "Public properties"
          
        /// <summary>
        /// Specifies how old (in hours) should be attachments deleted by the 'Delete old temporary attachments' scheduled task.
        /// </summary>
        private static int DeleteTemporaryAttachmentsOlderThan
        {
            get
            {
                if (mDeleteTemporaryAttachmentsOlderThan == null)
                {
                    mDeleteTemporaryAttachmentsOlderThan = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDeleteTemporaryAttachmentsOlderThan"], 24);
                }
                return mDeleteTemporaryAttachmentsOlderThan.Value;
            }
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Gets the attachment by the document ID and attachment GUID.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="attachmentGuid">GUID of the attachment</param>
        public static AttachmentInfo GetAttachmentInfo(int documentId, Guid attachmentGuid)
        {
            return ProviderObject.GetAttachmentInfoInternal(documentId, attachmentGuid);
        }


        /// <summary>
        /// Gets the attachment by the document ID and file name.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="fileName">File name</param>
        /// <param name="getBinary">Get the binary data?</param>
        public static AttachmentInfo GetAttachmentInfo(int documentId, string fileName, bool getBinary)
        {
            return ProviderObject.GetAttachmentInfoInternal(documentId, fileName, getBinary);
        }


        /// <summary>
        /// Returns the AttachmentInfo structure for the specified attachment.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Site name</param>
        public static AttachmentInfo GetAttachmentInfo(Guid attachmentGuid, string siteName)
        {
            return ProviderObject.GetAttachmentInfoInternal(attachmentGuid, siteName);
        }


        /// <summary>
        /// Returns the AttachmentInfo structure for the specified attachment.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Site name</param>
        public static AttachmentInfo GetAttachmentInfo(string attachmentGuid, string siteName)
        {
            return GetAttachmentInfo(new Guid(attachmentGuid), siteName);
        }


        /// <summary>
        /// Returns the AttachmentInfo structure for the specified attachment without binary data.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Site name</param>
        public static AttachmentInfo GetAttachmentInfoWithoutBinary(Guid attachmentGuid, string siteName)
        {
            return ProviderObject.GetAttachmentInfoWithoutBinaryInternal(attachmentGuid, siteName);
        }


        /// <summary>
        /// Returns the AttachmentInfo structure for the specified attachment.
        /// </summary>
        /// <param name="attachmentId">Attachment id</param>
        /// <param name="getBinary">If false, no binary data is retrieved from the database</param>
        public static AttachmentInfo GetAttachmentInfo(int attachmentId, bool getBinary)
        {
            return ProviderObject.GetAttachmentInfoInternal(attachmentId, getBinary);
        }


        /// <summary>
        /// Sets (updates or inserts) specified attachment.
        /// </summary>
        /// <param name="attachment">Attachment to set</param>
        /// <param name="resetSearchContent">If true, the search content field is set to null to be forced to load again</param>
        public static void SetAttachmentInfo(AttachmentInfo attachment, bool resetSearchContent = true)
        {
            ProviderObject.SetAttachmentInfoInternal(attachment, resetSearchContent);
        }


        /// <summary>
        /// Deletes specified attachment.
        /// </summary>
        /// <param name="atInfo">Attachment object</param>
        /// <param name="deletePhysicalFile">Delete physical file</param>
        public static void DeleteAttachmentInfo(AttachmentInfo atInfo, bool deletePhysicalFile = true)
        {
            ProviderObject.DeleteAttachmentInfoInternal(atInfo, deletePhysicalFile);
        }


        /// <summary>
        /// Deletes specified attachment.
        /// </summary>
        /// <param name="attachmentId">Attachment id</param>
        /// <param name="deletePhysicalFile">Delete physical file</param>
        public static void DeleteAttachmentInfo(int attachmentId, bool deletePhysicalFile = true)
        {
            AttachmentInfo atInfo = GetAttachmentInfo(attachmentId, false);
            DeleteAttachmentInfo(atInfo, deletePhysicalFile);
        }


        /// <summary>
        /// Returns object query of attachments with specified document id.
        /// </summary>
        /// <param name="documentId">Document id</param>
        /// <param name="getBinary">If false, no binary data is returned for the attachments</param>
        /// <remarks>The IncludeBinaryData property and the BinaryData method don't load binary data 
        /// for attachments stored on the filesystem. To load binary data for attachments stored on the 
        /// filesystem, use the AttachmentBinary property of every record.</remarks>
        public static ObjectQuery<AttachmentInfo> GetAttachments(int documentId, bool getBinary)
        {
            return GetAttachments().WhereID(AttachmentInfo.TYPEINFO.ParentIDColumn, documentId).BinaryData(getBinary);
        }


        /// <summary>
        /// Gets the query for all attachments
        /// </summary>
        /// <remarks>The IncludeBinaryData property and the BinaryData method don't load binary data 
        /// for attachments stored on the filesystem. To load binary data for attachments stored on the 
        /// filesystem, use the AttachmentBinary property of every record.</remarks>
        public static ObjectQuery<AttachmentInfo> GetAttachments()
        {
            return ProviderObject.GetObjectQuery();
        }
        

        /// <summary>
        /// Deletes all the attachments of specified document. 
        /// </summary>
        /// <param name="documentId">Document ID</param>
        public static void DeleteAttachments(int documentId)
        {
            var condition = new WhereCondition().WhereID(AttachmentInfo.TYPEINFO.ParentIDColumn, documentId);

            DeleteAttachments(condition);
        }


        /// <summary>
        /// Deletes the main attachments based on given where condition together with their variants.
        /// </summary>
        /// <param name="where">Where condition</param>
        internal static void DeleteAttachments(IWhereCondition where)
        {
            var attachments = GetAttachments()
                .ExceptVariants()
                .Where(where);

            foreach (var attachment in attachments)
            {
                DeleteAttachmentInfo(attachment);
            }
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Moves the attachment data to the target site.
        /// </summary>
        /// <param name="attachment">Attachment object</param>
        /// <param name="targetSiteId">Target site ID</param>
        public static void MoveAttachment(AttachmentInfo attachment, int targetSiteId)
        {
            ProviderObject.MoveAttachmentInternal(attachment, targetSiteId);
        }

        #endregion


        #region "Public methods - Temporary attachments"

        /// <summary>
        /// Gets a temporary attachment
        /// </summary>
        /// <param name="attachmentGuid">GUID of attachment</param>
        public static AttachmentInfo GetTemporaryAttachmentInfo(Guid attachmentGuid)
        {
            return GetAttachmentInfo(0, attachmentGuid);
        }
        

        /// <summary>
        /// Deletes old temporary attachments.
        /// </summary>
        public static void DeleteOldTemporaryAttachments()
        {
            ProviderObject.DeleteOldTemporaryAttachmentsInternal();
        }


        /// <summary>
        /// Deletes old temporary attachments.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        public static void DeleteTemporaryAttachments(int siteId)
        {
            ProviderObject.DeleteTemporaryAttachmentsInternal(siteId);
        }


        /// <summary>
        /// Adds the temporary attachment to the document, updates the attachment if currently present.
        /// </summary>
        /// <param name="formGuid">GUID of the form</param>
        /// <param name="attachmentGuid">GUID of the attachment (optional)</param>
        /// <param name="groupGuid">GUID of the attachment group (optional)</param>
        /// <param name="file">Attachment file path</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="width">New width of the image attachment (Use ImageHelper.AUTOSIZE to keep the width)</param>
        /// <param name="height">New height of the image attachment (Use ImageHelper.AUTOSIZE to keep the height)</param>
        /// <param name="maxSideSize">Maximal side size of the image attachment</param>
        public AttachmentInfo AddTemporaryAttachment(Guid formGuid, Guid attachmentGuid, Guid groupGuid, string file, int siteId, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            return AddTemporaryAttachment(formGuid, null, attachmentGuid, groupGuid, file, siteId, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds the temporary attachment to the document, updates the attachment if currently present.
        /// </summary>
        /// <param name="formGuid">GUID of the form</param>
        /// <param name="attachmentGuid">GUID of the attachment (optional)</param>
        /// <param name="groupGuid">GUID of the attachment group (optional)</param>
        /// <param name="file">Attachment file</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="width">New width of the image attachment (Use ImageHelper.AUTOSIZE to keep the width)</param>
        /// <param name="height">New height of the image attachment (Use ImageHelper.AUTOSIZE to keep the height)</param>
        /// <param name="maxSideSize">Maximal side size of the image attachment</param>
        public static AttachmentInfo AddTemporaryAttachment(Guid formGuid, Guid attachmentGuid, Guid groupGuid, HttpPostedFile file, int siteId, int width = ImageHelper.AUTOSIZE, int height = ImageHelper.AUTOSIZE, int maxSideSize = ImageHelper.AUTOSIZE)
        {
            return AddTemporaryAttachment(formGuid, null, attachmentGuid, groupGuid, file, siteId, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds the temporary attachment to the document, updates the attachment if currently present.
        /// </summary>
        /// <param name="formGuid">GUID of the form</param>
        /// <param name="guidColumnName">Column containing the Attachment GUID (optional)</param>
        /// <param name="attachmentGuid">GUID of the attachment (optional)</param>
        /// <param name="groupGuid">GUID of the attachment group (optional)</param>
        /// <param name="file">Attachment file (HttpPostedFile or file path)</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="width">New width of the image attachment (Use ImageHelper.AUTOSIZE to keep the width)</param>
        /// <param name="height">New height of the image attachment (Use ImageHelper.AUTOSIZE to keep the height)</param>
        /// <param name="maxSideSize">Maximal side size of the image attachment</param>
        public static AttachmentInfo AddTemporaryAttachment(Guid formGuid, string guidColumnName, Guid attachmentGuid, Guid groupGuid, AttachmentSource file, int siteId, int width, int height, int maxSideSize)
        {
            return ProviderObject.AddTemporaryAttachmentInternal(formGuid, guidColumnName, attachmentGuid, groupGuid, file, siteId, width, height, maxSideSize);
        }


        /// <summary>
        /// Adds the temporary attachment to the document, updates the attachment if currently present.
        /// </summary>
        /// <param name="attachmentGuid">GUID of the attachment</param>
        /// <param name="siteName">Site name</param>
        public static void DeleteTemporaryAttachment(Guid attachmentGuid, string siteName)
        {
            AttachmentInfo attachment = GetAttachmentInfoWithoutBinary(attachmentGuid, siteName);

            if (attachment != null)
            {
                // Delete the attachment from the database and disk
                DeleteAttachmentInfo(attachment);
            }
        }


        /// <summary>
        /// Method gets unique temporary attachment file name in the form scope.
        /// </summary>
        /// <param name="formGuid">GUID of the form</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="currentAttachmentId">Current attachment ID</param>
        /// <param name="siteId">Site ID</param>
        /// <returns>Unique attachment name</returns>
        public static string GetUniqueTemporaryAttachmentFileName(Guid formGuid, string fileName, int currentAttachmentId, int siteId)
        {
            return ProviderObject.GetUniqueTemporaryAttachmentFileNameInternal(formGuid, fileName, currentAttachmentId, siteId);
        }


        /// <summary>
        /// Finds out whether the given temporary attachment file name is unique.
        /// </summary>
        /// <param name="formGuid">GUID of the form</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="extension">Extension of file</param>
        /// <param name="currentAttachmentId">Current attachment ID</param>
        /// <returns>True if attachment name is unique</returns>
        public static bool IsUniqueTemporaryAttachmentName(Guid formGuid, string fileName, string extension, int currentAttachmentId)
        {
            return ProviderObject.IsUniqueTemporaryAttachmentNameInternal(formGuid, fileName, extension, currentAttachmentId);
        }

        #endregion


        #region "Public methods - Cache"

        /// <summary>
        /// Gets the cache key dependencies array for the attachment (cache item keys affected when the attachment changes).
        /// </summary>
        /// <param name="ai">Attachment</param>
        public static string[] GetDependencyCacheKeys(IAttachment ai)
        {
            return ProviderObject.GetDependencyCacheKeysInternal(ai);
        }


        /// <summary>
        /// Gets the cache key dependencies array for the attachment order change (cache item keys affected when the attachment order changes).
        /// </summary>
        /// <param name="documentId">Attachment document ID</param>
        public static string[] GetDependencyCacheKeys(int documentId)
        {
            return ProviderObject.GetDependencyCacheKeysInternal(documentId);
        }

        #endregion


        #region "Public methods - Settings keys"

        /// <summary>
        /// Returns the current settings whether the files should check document permissions.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool CheckFilesPermissions(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSCheckFilesPermissions");
        }


        /// <summary>
        /// Returns the current settings whether the files should check if the document is published.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool CheckPublishedFiles(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSCheckPublishedFiles");
        }

        #endregion


        #region "Public methods - Search"

        /// <summary>
        /// Clears the AttachmentSearchContent for all attachments in the DB.
        /// </summary>
        public static void ClearSearchCache()
        {
            ProviderObject.ClearSearchCacheInternal();
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Gets the attachment by the document ID and attachment GUID.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="attachmentGuid">GUID of the attachment</param>
        protected virtual AttachmentInfo GetAttachmentInfoInternal(int documentId, Guid attachmentGuid)
        {
            // Get the data
            var result =
                GetAttachments()
                    .WhereEquals(TypeInfo.GUIDColumn, attachmentGuid)
                    .BinaryData(false);

            if (documentId > 0)
            {
                // Regular attachments
                result.WhereEquals("AttachmentDocumentID", documentId);
            }
            else
            {
                // Temporary attachments
                result.WhereNull("AttachmentDocumentID");
            }

            return result.FirstOrDefault();
        }


        /// <summary>
        /// Gets the attachment by the document ID and file name.
        /// </summary>
        /// <param name="documentId">Document ID</param>
        /// <param name="fileName">File name</param>
        /// <param name="getBinary">Get the binary data?</param>
        protected virtual AttachmentInfo GetAttachmentInfoInternal(int documentId, string fileName, bool getBinary)
        {
            // Get the data
            return
                GetAttachments()
                    .ExceptVariants()
                    .WhereID(TypeInfo.ParentIDColumn, documentId)
                    .WhereEquals(TypeInfo.DisplayNameColumn, fileName)
                    .BinaryData(getBinary)
                    .FirstOrDefault();
        }


        /// <summary>
        /// Returns the AttachmentInfo structure for the specified attachment.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Site name</param>
        protected virtual AttachmentInfo GetAttachmentInfoInternal(Guid attachmentGuid, string siteName)
        {
            // Get the site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[AttachmentInfoProvider.GetAttachmentInfo]: Site name '" + siteName + "' not found.");
            }

            return GetInfoByGuid(attachmentGuid, si.SiteID);
        }


        /// <summary>
        /// Returns the AttachmentInfo structure for the specified attachment without binary data.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="siteName">Site name</param>
        protected virtual AttachmentInfo GetAttachmentInfoWithoutBinaryInternal(Guid attachmentGuid, string siteName)
        {
            // Get the site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si == null)
            {
                throw new Exception("[AttachmentInfoProvider.GetAttachmentInfoWithoutBinary]: Site name '" + siteName + "' not found.");
            }

            return GetObjectQuery().TopN(1).WithGuid(attachmentGuid).OnSite(siteName).BinaryData(false).FirstOrDefault();
        }


        /// <summary>
        /// Returns the AttachmentInfo structure for the specified attachment.
        /// </summary>
        /// <param name="attachmentId">Attachment id</param>
        /// <param name="getBinary">If false, no binary data is retrieved from the database</param>
        protected virtual AttachmentInfo GetAttachmentInfoInternal(int attachmentId, bool getBinary)
        {
            return GetObjectQuery().TopN(1)
                .WithID(attachmentId)
                .BinaryData(getBinary).FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified attachment.
        /// </summary>
        /// <param name="attachment">Attachment to set</param>
        /// <param name="resetSearchContent">If true, the search content field is set to null to be forced to load again</param>
        protected virtual void SetAttachmentInfoInternal(AttachmentInfo attachment, bool resetSearchContent = true)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            EnsureAttachmentSiteId(attachment);

            var siteName = SiteInfoProvider.GetSiteName(attachment.AttachmentSiteID);
            var locationType = FileHelper.FilesLocationType(siteName);
            var storeOnlyInFileSystem = locationType == FilesLocationTypeEnum.FileSystem;

            // Ensure the binary data if necessary
            BinaryData newBinaryData = null;
            if (!storeOnlyInFileSystem || (attachment.InputStream == null) || WebFarmHelper.WebFarmEnabled)
            {
                newBinaryData = attachment.Generalized.EnsureBinaryData();
            }
            else if ((attachment.InputStream != null) && (locationType != FilesLocationTypeEnum.Database))
            {
                // Init attachment binary from InputStream and clear it to ensure data for saving to disk
                newBinaryData = attachment.Generalized.EnsureBinaryData();
            }

            // If no file data are stored in DB - clear the binary column
            if (storeOnlyInFileSystem && (attachment.InputStream == null) && (attachment.AttachmentBinary != null))
            {
                attachment.AttachmentBinary = null;
            }

            // Ensure safe file name
            attachment.AttachmentName = URLHelper.GetSafeFileName(attachment.AttachmentName, siteName);

            // Save the search content
            attachment.SetValue("AttachmentSearchContent", resetSearchContent ? null : attachment.AttachmentSearchContent.GetData());

            // Use transaction
            using (var tr = BeginTransaction())
            {
                // Save the attachment record
                SetInfo(attachment);

                string guid = Convert.ToString(attachment.AttachmentGUID);

                if (locationType != FilesLocationTypeEnum.Database)
                {
                    AttachmentBinaryHelper.SaveFileToDisk(siteName, guid, guid, attachment.AttachmentExtension, newBinaryData, true);
                }
                else
                {
                    // Log web farm task
                    WebFarmHelper.CreateTask(DocumentTaskType.UpdateAttachment, "FileUpload", siteName, guid, guid, attachment.AttachmentExtension, true.ToString());
                }

                // Commit transaction
                tr.Commit();
            }

            // Drop the cache dependencies
            CacheHelper.TouchKeys(GetDependencyCacheKeys(attachment));
        }


        private static void EnsureAttachmentSiteId(AttachmentInfo attachment)
        {
            if ((attachment.AttachmentSiteID > 0) || (attachment.AttachmentDocumentID <= 0))
            {
                return;
            }

            var site = TreePathUtils.GetDocumentSite(attachment.AttachmentDocumentID);
            if (site == null)
            {
                return;
            }

            attachment.AttachmentSiteID = site.SiteID;
        }


        /// <summary>
        /// Deletes specified attachment.
        /// </summary>
        /// <param name="atInfo">Attachment object</param>
        /// <param name="deletePhysicalFile">Delete physical file</param>
        protected virtual void DeleteAttachmentInfoInternal(AttachmentInfo atInfo, bool deletePhysicalFile)
        {
            if (atInfo == null)
            {
                return;
            }

            // Delete all file occurrences in destination folder
            if (deletePhysicalFile)
            {
                AttachmentBinaryHelper.DeleteFile(atInfo.AttachmentGUID, SiteInfoProvider.GetSiteName(atInfo.AttachmentSiteID), false);
            }

            // Delete object
            atInfo.Generalized.DeleteData();

            // Drop the cache dependencies
            CacheHelper.TouchKeys(GetDependencyCacheKeys(atInfo));
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Moves the attachment data to the target site.
        /// </summary>
        /// <param name="attachment">Attachment object</param>
        /// <param name="targetSiteId">Target site ID</param>
        protected virtual void MoveAttachmentInternal(AttachmentInfo attachment, int targetSiteId)
        {
            // Check attachment            
            if (attachment == null)
            {
                throw new Exception("[AttachmentInfoProvider.MoveAttachment]: Attachment not found.");
            }

            // Get the target site
            var site = SiteInfoProvider.GetSiteInfo(targetSiteId);
            if (site == null)
            {
                throw new Exception("[AttachmentInfoProvider.MoveAttachment]: Target site not found.");
            }

            string filePath = null;
            bool deleteFile = false;

            // Get the binary data
            if (attachment.AttachmentBinary == null)
            {
                var originalSiteName = SiteInfoProvider.GetSiteName(attachment.AttachmentSiteID);
                filePath = AttachmentBinaryHelper.GetFilePhysicalPath(originalSiteName, attachment.AttachmentGUID.ToString(), attachment.AttachmentExtension);

                if (File.Exists(filePath))
                {
                    attachment.AttachmentBinary = File.ReadAllBytes(filePath);
                    deleteFile = true;
                }
            }
            attachment.AttachmentSiteID = targetSiteId;

            // Save the attachment to the new site
            SetAttachmentInfo(attachment);

            // Delete the attachment file from previous location
            if (deleteFile)
            {
                File.Delete(filePath);
            }
        }


        /// <summary>
        /// Indicates whether the given attachment name is unique.
        /// </summary>
        /// <param name="documentId">ID of the document</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="extension">Extension of file</param>
        /// <param name="currentAttachmentId">Current attachment ID</param>
        internal static bool IsUniqueAttachmentName(int documentId, string fileName, string extension, int currentAttachmentId)
        {
            var attachmentsWithSameName = GetAttachments()
                    .ExceptVariants()
                    .WhereEquals("AttachmentName", fileName)
                    .WhereEquals("AttachmentExtension", extension)
                    .WhereNotEquals("AttachmentID", currentAttachmentId)
                    .WhereEquals("AttachmentDocumentID", documentId);

            return (attachmentsWithSameName.Count == 0);
        }

        #endregion


        #region "Internal methods - Temporary attachments"
        
        /// <summary>
        /// Deletes old temporary attachments.
        /// </summary>
        protected virtual void DeleteOldTemporaryAttachmentsInternal()
        {
            var where = new WhereCondition()
                .WhereNotNull("AttachmentFormGUID")
                .Where(TypeInfo.TimeStampColumn, QueryOperator.LessThan, DateTime.Now.Subtract(TimeSpan.FromHours(DeleteTemporaryAttachmentsOlderThan)));

            DeleteAttachments(where);
        }


        /// <summary>
        /// Deletes old temporary attachments.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        protected virtual void DeleteTemporaryAttachmentsInternal(int siteId)
        {
            var condition = new WhereCondition()
                .WhereEquals(TypeInfo.SiteIDColumn, siteId)
                .WhereNotNull("AttachmentFormGUID");

            DeleteAttachments(condition);
        }

        
        /// <summary>
        /// Creates a new attachment info using the given data
        /// </summary>
        /// <param name="file">File object, HttpPostedFile, file path or attachment info</param>
        /// <param name="newGuid">Attachment GUID</param>
        /// <param name="documentId">Document ID</param>
        /// <param name="siteName">Site name</param>
        /// <remarks>Does not save the attachment to database</remarks>
        internal static DocumentAttachment CreateNewAttachment(AttachmentSource file, Guid newGuid, int documentId, string siteName)
        {
            DocumentAttachment attachment;

            // Posted file
            if (file.PostedFile != null)
            {
                attachment = (DocumentAttachment)new AttachmentInfo(file.PostedFile, documentId, newGuid);
                attachment.AttachmentSiteID = SiteInfoProvider.GetSiteID(siteName);

                return attachment;
            }

            // File path
            if (file.FilePath != null)
            {
                attachment = (DocumentAttachment)new AttachmentInfo(file.FilePath, documentId, newGuid);
                attachment.AttachmentSiteID = SiteInfoProvider.GetSiteID(siteName);

                return attachment;
            }

            // Attachment info
            var sourceAttachment = file.Attachment;
            if (sourceAttachment == null)
            {
                throw new InvalidOperationException("Only HttpPostedFile, file path or attachment info is allowed for file parameter.");
            }

            if (sourceAttachment.AttachmentSiteID <= 0)
            {
                throw new InvalidOperationException("Source attachment missing site.");
            }

            // New attachment as attachment info
            attachment = sourceAttachment.Clone(true);
            
            attachment.AttachmentDocumentID = documentId;
            attachment.AttachmentGUID = newGuid;
            attachment.AttachmentSiteID = SiteInfoProvider.GetSiteID(siteName);
            attachment.AttachmentFormGUID = Guid.Empty;

            if (attachment.AttachmentBinary == null)
            {
                attachment.AttachmentBinary = AttachmentBinaryHelper.GetAttachmentBinary(sourceAttachment);
            }

            return attachment;
        }


        /// <summary>
        /// Adds the temporary attachment to the document, updates the attachment if currently present.
        /// </summary>
        /// <param name="formGuid">GUID of the form</param>
        /// <param name="guidColumnName">Column containing the Attachment GUID (optional)</param>
        /// <param name="attachmentGuid">GUID of the attachment (optional)</param>
        /// <param name="groupGuid">GUID of the attachment group (optional)</param>
        /// <param name="file">Attachment file (HttpPostedFile or file path)</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="width">New width of the image attachment (Use ImageHelper.AUTOSIZE to keep the width)</param>
        /// <param name="height">New height of the image attachment (Use ImageHelper.AUTOSIZE to keep the height)</param>
        /// <param name="maxSideSize">Maximal side size of the image attachment</param>
        protected virtual AttachmentInfo AddTemporaryAttachmentInternal(Guid formGuid, string guidColumnName, Guid attachmentGuid, Guid groupGuid, AttachmentSource file, int siteId, int width, int height, int maxSideSize)
        {
            if (formGuid == Guid.Empty)
            {
                throw new Exception("[AttachmentInfoProvider.AddTemporaryAttachment]: Missing form GUID.");
            }

            // Get current temporary attachment
            DocumentAttachment currentAttachment = null;

            string siteName;
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteId);
            if (si != null)
            {
                siteName = si.SiteName;
            }
            else
            {
                throw new Exception("[AttachmentInfoProvider.AddTemporaryAttachment]: Site not found.");
            }

            // Get existing temporary attachment
            if (attachmentGuid != Guid.Empty)
            {
                currentAttachment = (DocumentAttachment)GetAttachmentInfoWithoutBinary(attachmentGuid, siteName);
            }

            DocumentAttachment attachment;

            // Update current temporary attachment or create a new one
            if (currentAttachment == null)
            {
                attachment = CreateNewAttachment(file, Guid.NewGuid(), 0, siteName);
            }
            else
            {
                attachment = CreateNewAttachment(file, currentAttachment.AttachmentGUID, 0, siteName);
                CopyAttachmentProperties(attachment, currentAttachment);

                attachment = currentAttachment;
            }

            // Ensure unique attachment file name
            attachment.AttachmentName = GetUniqueTemporaryAttachmentFileName(formGuid, attachment.AttachmentName, attachment.AttachmentID, siteId);

            // Set attachment grouped attachment flag
            attachment.AttachmentGroupGUID = groupGuid;

            // Set attachment unsorted attachment flag
            attachment.AttachmentIsUnsorted = String.IsNullOrEmpty(guidColumnName) && groupGuid == Guid.Empty;

            // Set form GUID
            attachment.AttachmentFormGUID = formGuid;

            // Ensure image attachment resize
            if (ImageHelper.IsImage(attachment.AttachmentExtension))
            {
                AttachmentBinaryHelper.ResizeImageAttachment(attachment, width, height, maxSideSize);
            }

            var attachmentInfo = attachment.GetAttachmentInfo();

            // Save the attachment
            SetAttachmentInfo(attachmentInfo);

            // Return current attachment version
            return attachmentInfo;
        }


        /// <summary>
        /// Takes attachment data from <paramref name="source"/> and sets corresponding fields in <paramref name="target"/>.
        /// </summary>
        internal static void CopyAttachmentProperties(DocumentAttachment source, DocumentAttachment target)
        {
            target.AttachmentBinary = source.AttachmentBinary;
            target.AttachmentName = source.AttachmentName;
            target.AttachmentExtension = source.AttachmentExtension;
            target.AttachmentSize = source.AttachmentSize;
            target.AttachmentMimeType = source.AttachmentMimeType;
            target.AttachmentImageHeight = source.AttachmentImageHeight;
            target.AttachmentImageWidth = source.AttachmentImageWidth;
        }


        /// <summary>
        /// Method gets unique temporary attachment file name in the form scope.
        /// </summary>
        /// <param name="formGuid">GUID of the form</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="currentAttachmentId">Current attachment ID</param>
        /// <param name="siteId">Site ID</param>
        /// <returns>Unique attachment name</returns>
        protected virtual string GetUniqueTemporaryAttachmentFileNameInternal(Guid formGuid, string fileName, int currentAttachmentId, int siteId)
        {
            if (formGuid == Guid.Empty)
            {
                throw new Exception("[AttachmentInfoProvider.GetUniqueTemporaryAttachmentFileName]: Missing form GUID.");
            }

            // Get site name
            string siteName = SiteInfoProvider.GetSiteInfo(siteId).SiteName;

            // Get extension and remove it from file name
            int indexOfExtension = fileName.LastIndexOf(".", StringComparison.Ordinal);
            string extension = string.Empty;
            if (indexOfExtension > 0)
            {
                extension = fileName.Substring(indexOfExtension).TrimStart('.');
                extension = '.' + URLHelper.GetSafeFileName(extension, siteName);
                fileName = fileName.Remove(indexOfExtension);
            }

            // Get safe file name
            fileName = URLHelper.GetSafeFileName(fileName, siteName);
            string originalName = fileName;

            // Append extension to safe file name
            fileName += extension;

            // Remove unique index
            if (originalName.EndsWith(")", StringComparison.Ordinal))
            {
                originalName = Regex.Replace(originalName, "[ " + URLHelper.ForbiddenCharactersReplacement(siteName) + "](\\(\\d+\\))$", "");
                if (originalName == string.Empty)
                {
                    originalName = fileName;
                }
            }

            // Get all which may possibly match 
            string searchFileName = TextHelper.LimitLength(originalName, MAXATTACHMENTNAMELENGTH - 6, string.Empty);

            // Get attachments
            DataSet ds = GetAttachments()
                .Columns("AttachmentID, AttachmentName")
                .WhereEquals("AttachmentFormGUID", formGuid)
                .WhereStartsWith(TypeInfo.DisplayNameColumn, searchFileName);

            // Check collisions
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                DataTable dt = ds.Tables[0];

                bool unique = false;
                int uniqueIndex = 0;
                do
                {
                    // Get matching rows
                    DataRow[] match = dt.Select("(AttachmentName = '" + SqlHelper.EscapeQuotes(fileName) + "') AND (AttachmentID <> " + currentAttachmentId + ")");
                    if (match.Length == 0)
                    {
                        // If not match, consider as unique
                        unique = true;
                    }
                    else
                    {
                        // If match (duplicity found), create new name
                        uniqueIndex++;
                        string uniqueString = " (" + uniqueIndex + ")";
                        int originalLength = MAXATTACHMENTNAMELENGTH - uniqueString.Length;
                        if (originalName.Length > originalLength)
                        {
                            fileName = originalName.Substring(0, originalLength) + uniqueString;
                        }
                        else
                        {
                            fileName = originalName + uniqueString;
                        }
                        // Get safe file name and append extension
                        fileName = URLHelper.GetSafeFileName(fileName, siteName) + extension;
                    }
                } while (!unique);
            }
            return fileName;
        }


        /// <summary>
        /// Indicates whether the given temporary attachment name is unique.
        /// </summary>
        /// <param name="formGuid">GUID of the form</param>
        /// <param name="fileName">Name of file</param>
        /// <param name="extension">Extension of file</param>
        /// <param name="currentAttachmentId">Current attachment ID</param>
        protected virtual bool IsUniqueTemporaryAttachmentNameInternal(Guid formGuid, string fileName, string extension, int currentAttachmentId)
        {
            var attachmentsWithSameName = GetAttachments()
                .Where(AttachmentInfo.TYPEINFO_TEMPORARY.TypeCondition.GetWhereCondition())
                .WhereEquals("AttachmentName", fileName)
                .WhereEquals("AttachmentExtension", extension)
                .WhereEquals("AttachmentFormGUID", formGuid)
                .WhereNotEquals("AttachmentID", currentAttachmentId)
                .WhereNull("AttachmentDocumentID");

            return (attachmentsWithSameName.Count == 0);
        }

        #endregion


        #region "Internal methods - Cache"

        /// <summary>
        /// Gets the cache key dependencies array for the attachment (cache item keys affected when the attachment changes).
        /// </summary>
        /// <param name="ai">Attachment</param>
        protected virtual string[] GetDependencyCacheKeysInternal(IAttachment ai)
        {
            if (ai == null)
            {
                return null;
            }

            // Create the default dependencies
            string[] result = new string[4];
            int i = 0;

            // Default dependencies
            result[i++] = "cms.attachment|all";
            if (ai.AttachmentDocumentID > 0)
            {
                result[i++] = "documentid|" + ai.AttachmentDocumentID;
                result[i++] = "documentid|" + ai.AttachmentDocumentID + "|attachments";
            }

            // Backward compatibility
            result[i] = "attachment|" + ai.AttachmentGUID.ToString().ToLowerInvariant();

            return result;
        }


        /// <summary>
        /// Gets the cache key dependencies array for the attachment order change (cache item keys affected when the attachment order changes).
        /// </summary>
        /// <param name="documentId">Attachment document ID</param>
        protected virtual string[] GetDependencyCacheKeysInternal(int documentId)
        {
            // Create the default dependencies
            string[] result = new string[3];
            int i = 0;

            // Default dependencies
            result[i++] = "cms.attachment|all";
            if (documentId > 0)
            {
                result[i++] = "documentid|" + documentId;
                result[i] = "documentid|" + documentId + "|attachments";
            }

            return result;
        }

        #endregion


        #region "Internal methods - Search"

        /// <summary>
        /// Clears the AttachmentSearchContent for all attachments in the DB.
        /// </summary>
        protected virtual void ClearSearchCacheInternal()
        {
            UpdateData("AttachmentSearchContent = NULL", null, null);
        }

        #endregion
    }
}
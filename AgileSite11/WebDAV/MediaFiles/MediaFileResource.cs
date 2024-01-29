using System;

using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.Base;
using CMS.Core;
using CMS.DocumentEngine;
using CMS.SiteProvider;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.WebDAV
{
    /// <summary>
    /// Class that represents the media file.
    /// </summary>
    internal class MediaFileResource : HierarchyItem, IResource
    {
        #region "Variables"

        private System.IO.Stream mInputStream = null;

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes media file resource.
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="name">Name</param>
        /// <param name="mimeType">Mime type</param>
        /// <param name="size">File size</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">Url parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent folder</param>
        public MediaFileResource(string path, string name, string mimeType, long size, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, created, modified, urlParser, engine, parent)
        {
            ContentLength = size;
            ContentType = mimeType;
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// File input stream of media file.
        /// </summary>
        public System.IO.Stream InputStream
        {
            get
            {
                if (mInputStream == null)
                {
                    MediaFileInfo mfi = UrlParser.MediaFileInfo;

                    if (mfi != null)
                    {
                        // Get the file path            
                        string filePath = MediaFileInfoProvider.GetMediaFilePath(SiteContext.CurrentSiteName, UrlParser.LibraryFolder, mfi.FilePath);
                        // Set input stream
                        mInputStream = FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                    }
                    else if (UrlParser.FileInfo != null)
                    {
                        // Get the file path
                        string filePath = UrlParser.FileInfo.FullName;
                        // Check if media file exists in file system
                        if (File.Exists(filePath))
                        {
                            // Set input stream
                            mInputStream = FileStream.New(filePath, FileMode.Open, FileAccess.Read, FileShare.None);
                        }
                    }
                }

                return mInputStream;
            }
        }

        #endregion


        #region "IResource Members"

        /// <summary>
        /// Gets the media type of the resource.
        /// </summary>
        public string ContentType
        {
            get;
            protected set;
        }


        /// <summary>
        /// Gets the size of the resource content in bytes.
        /// </summary>
        public long ContentLength
        {
            get;
            protected set;
        }


        /// <summary>
        /// Writes the content of the resource to the specified stream. 
        /// </summary>
        /// <param name="output">Output stream</param>
        /// <param name="byteStart">The zero-based byte offset in resource content at which to begin
        /// copying bytes to the output stream.</param>
        /// <param name="count">The number of bytes to be written to the output stream</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse WriteToStream(System.IO.Stream output, long byteStart, long count)
        {
            try
            {
                // Check 'Read' or 'Manage' permission
                if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(UrlParser.MediaLibraryInfo, new string[] { "read", "manage" }))
                {
                    // File path is safe (media file is imported in DB)
                    if (UrlParser.FilePath == UrlParser.PhysicalFilePath)
                    {
                        MediaFileInfo mfi = UrlParser.MediaFileInfo;
                        if (mfi != null)
                        {
                            // Get the file path            
                            string filePath = MediaFileInfoProvider.GetMediaFilePath(SiteContext.CurrentSiteName, UrlParser.LibraryFolder, mfi.FilePath);

                            WriteFile(output, byteStart, count, filePath);
                        }
                        else if (UrlParser.FileInfo != null)
                        {
                            WriteFile(output, byteStart, count, UrlParser.FileInfo.FullName);
                        }
                    }
                    // File path is unsafe (media file isn't imported in DB)
                    else
                    {
                        if (UrlParser.FileInfo != null)
                        {
                            WriteFile(output, byteStart, count, UrlParser.FileInfo.FullName);
                        }
                    }
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFileResource_WriteToStream", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new OkResponse();
        }


        /// <summary>
        /// Saves the content of the resource from the specified stream to the WebDAV repository.
        /// </summary>
        /// <param name="content">Stream to read the content of the resource from</param>
        /// <param name="contentType">Indicates the media type of the resource</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse SaveFromStream(System.IO.Stream content, string contentType)
        {
            try
            {
                if ((content != null) && (content.Length > 0))
                {
                    // Check ('File modify' permission) OR ('Create' permission AND zero size of media file)
                    if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(UrlParser.MediaLibraryInfo, "filemodify")
                        || (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(UrlParser.MediaLibraryInfo, "filecreate") && (UrlParser.MediaFileInfo != null) && (UrlParser.MediaFileInfo.FileSize == 0)))
                    {
                        MediaFileInfo mfi = UrlParser.MediaFileInfo;
                        // Media file is imported
                        if (mfi != null)
                        {
                            // Set stream and size
                            mfi.FileBinaryStream = content;
                            mfi.FileSize = content.Length;

                            // Resize media file
                            WebDAVHelper.ResizeMediaFileInfo(mfi);

                            // Save media file
                            MediaFileInfoProvider.SetMediaFileInfo(mfi, false);

                            // Clean binary data
                            WebDAVHelper.CleanBinaryData(mfi);
                        }
                        // Media file isn't imported
                        else if (UrlParser.FileInfo != null)
                        {
                            // Check if media file exists in file system
                            if (File.Exists(UrlParser.FileInfo.FullName))
                            {
                                StorageHelper.SaveFileToDisk(UrlParser.FileInfo.FullName, content);
                            }
                        }
                    }
                    else
                    {
                        return new AccessDeniedResponse();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFileResource_SaveFromStream", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new OkResponse();
        }

        #endregion


        #region "HierarchyItem Members Override"

        /// <summary>
        /// Creates a copy of this item with a new name in the destination folder.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <param name="deep">Indicates whether to copy entire subtree</param>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse CopyTo(IFolder folder, string destName, bool deep)
        {
            return new NoContentResponse();
        }


        /// <summary>
        /// Moves this item to the destination folder under a new name.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse MoveTo(IFolder folder, string destName)
        {
            string extension = IO.Path.GetExtension(destName);

            // Check whether the extension of the media file is allowed
            if (!WebDAVHelper.IsMediaFileExtensionAllowedForBrowseMode(extension, SiteContext.CurrentSiteName))
            {
                return new AccessDeniedResponse();
            }

            // Destination document is media folder
            if (folder is MediaFolder)
            {
                MediaFolder mf = folder as MediaFolder;

                // Check 'FileModify' permission
                bool allowed = WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(UrlParser.MediaLibraryInfo, "filemodify");

                // If target directory differs from source
                if (allowed && (UrlParser.MediaLibraryInfo.LibraryID != mf.UrlParser.MediaLibraryInfo.LibraryID))
                {
                    // Perform another permission check
                    allowed &= WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(mf.UrlParser.MediaLibraryInfo, "filemodify");
                }

                if (allowed)
                {
                    return MoveMediaFileToMediaFolder(mf, destName);
                }
            }
            // Destination document is attachment folder
            else if (folder is AttachmentFolder)
            {
                // Check 'FileDelete' permission
                if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(UrlParser.MediaLibraryInfo, "filedelete"))
                {
                    return MoveMediaFileToAttachmentFolder(folder as AttachmentFolder, destName);
                }
            }
            // Destination document is content folder
            else if (folder is ContentFolder)
            {
                // Check 'FileDelete' permission
                if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(UrlParser.MediaLibraryInfo, "filedelete"))
                {
                    return MoveMediaFileToContentFolder(folder as ContentFolder, destName);
                }
            }

            return new NoContentResponse();
        }


        /// <summary>
        /// Deletes media file.
        /// </summary>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse Delete()
        {
            try
            {
                // Check 'FileDelete' permission
                if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(UrlParser.MediaLibraryInfo, "filedelete"))
                {
                    return DeleteMediaFile();
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFileResource_Delete", "WebDAV", ex);
                return new ServerErrorResponse();
            }
        }


        /// <summary>
        /// Deletes media file.
        /// </summary>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse DeleteMediaFile()
        {
            try
            {
                if (UrlParser.MediaFileInfo != null)
                {
                    // Delete media file from DB and file system
                    MediaFileInfoProvider.DeleteMediaFileInfo(UrlParser.MediaFileInfo);
                }
                else if (UrlParser.FileInfo != null)
                {
                    // Delete media file from file system
                    UrlParser.FileInfo.Delete();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFileResource_DeleteMediaFile", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new NoContentResponse();
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Moves media file to media folder.
        /// </summary>
        /// <param name="mediaFolder">Destination media folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveMediaFileToMediaFolder(MediaFolder mediaFolder, string destName)
        {
            try
            {
                MediaLibraryInfo destMediaLibrary = mediaFolder.UrlParser.MediaLibraryInfo;
                MediaLibraryInfo sourceMediaLibrary = UrlParser.MediaLibraryInfo;

                int destLibraryID = (destMediaLibrary != null) ? destMediaLibrary.LibraryID : 0;

                // Move or rename only in the same media library
                // Note: Temporary condition - can be removed,
                //       but staging of move actions doesn't work properly
                if (destLibraryID == sourceMediaLibrary.LibraryID)
                {
                    string siteName = SiteContext.CurrentSiteName;
                    MediaFileInfo mfi = UrlParser.MediaFileInfo;
                    if (mfi != null)
                    {
                        string safeFileName = WebDAVHelper.GetSafeFileName(destName, siteName, WebDAVHelper.MAX_MEDIA_FILENAME_LENGTH);
                        string newSafePath = DirectoryHelper.CombinePath(mediaFolder.UrlParser.FilePath, safeFileName);

                        // Check if media file exists in DB
                        MediaFileInfo existingMediaFile = MediaFileInfoProvider.GetMediaFileInfo(destLibraryID, newSafePath);

                        // Media file exists
                        if (existingMediaFile != null)
                        {
                            // Ensure binary data
                            EnsureBinaryData(existingMediaFile);

                            // Resize media file
                            WebDAVHelper.ResizeMediaFileInfo(existingMediaFile);

                            // Save media file
                            MediaFileInfoProvider.SetMediaFileInfo(existingMediaFile, false);

                            // Delete this media file
                            DeleteMediaFile();

                            return new CreatedResponse();
                        }
                        else
                        {
                            // Original (source) file path
                            string origPath = mfi.FilePath;

                            // Move Media File
                            MediaFileInfoProvider.MoveMediaFile(siteName, sourceMediaLibrary.LibraryID, destLibraryID, origPath, newSafePath, false);

                            return new CreatedResponse();
                        }
                    }
                    else if (UrlParser.FileInfo != null)
                    {
                        // Original (source) file path
                        string origPath = UrlParser.PhysicalFilePath;

                        // Destination folder path + name
                        string newPath = DirectoryHelper.CombinePath(mediaFolder.UrlParser.FilePath, destName);

                        // Move Media File
                        MediaFileInfoProvider.MoveMediaFile(siteName, destLibraryID, origPath, newPath, false);

                        return new CreatedResponse();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFileResource_MoveMediaFileToMediaFolder", "WebDAV", ex);
            }

            return new NoContentResponse();
        }


        /// <summary>
        /// Moves media file to attachment folder.
        /// </summary>
        /// <param name="attachmentFolder">Destination media folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveMediaFileToAttachmentFolder(AttachmentFolder attachmentFolder, string destName)
        {
            try
            {
                TreeNode destNode = attachmentFolder.UrlParser.Node;
                UrlParser destParser = attachmentFolder.UrlParser;

                // Only attachment field or unsorted folder and 'Modify' permission on the document or user is group admin and check document isn't checked out by another user
                if (((destParser.ItemType == ItemTypeEnum.AttachmentUnsortedFolder) || (destParser.ItemType == ItemTypeEnum.AttachmentFieldNameFolder))
                    && (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(destNode, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(destParser.Group))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(destNode) && WebDAVHelper.CanCurrentUserApproveCustomStep(destNode))
                {
                    TreeProvider tree = destNode.TreeProvider;
                    // Get safe file name
                    string newFileName = WebDAVHelper.GetSafeFileName(destName, SiteContext.CurrentSiteName, AttachmentInfoProvider.MAXATTACHMENTNAMELENGTH);

                    // Get existing attachment guid
                    Guid existingGuid = WebDAVHelper.GetExistingAttachmentGuid(destNode, tree, newFileName, destParser.FieldName);

                    // Create new attachment from media file info
                    var newAttachment = WebDAVHelper.CreateAttachmentFromMediaFileInfo(existingGuid, newFileName, this, UrlParser.MediaLibraryInfo);
                    if (newAttachment != null)
                    {
                        // Create new attachment
                        if (attachmentFolder.UrlParser.ItemType == ItemTypeEnum.AttachmentUnsortedFolder)
                        {
                            // Save attachment
                            SaveAttachment(destNode, newAttachment, tree, null);

                            // Delete this media file
                            DeleteMediaFile();
                        }
                        else if (attachmentFolder.UrlParser.ItemType == ItemTypeEnum.AttachmentFieldNameFolder)
                        {
                            FormFieldInfo ffi = WebDAVHelper.GetFormFieldInfo(destNode, attachmentFolder.UrlParser.FieldName);
                            if ((ffi != null) && (ffi.DataType == FieldDataType.File) || (ffi.DataType == DocumentFieldDataType.DocAttachments))
                            {
                                // Save attachment
                                SaveAttachment(destNode, newAttachment, tree, ffi);

                                // Delete this media file
                                DeleteMediaFile();
                            }
                        }

                        return new CreatedResponse();
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFileResource_MoveMediaFileToAttachmentFolder", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new NoContentResponse();
        }


        /// <summary>
        /// Saves new attachment.
        /// </summary>
        /// <param name="node">Destination doucment</param>
        /// <param name="attachment">New attachment</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="ffi">Form field info (Null - if attachment is unsorted)</param>
        private void SaveAttachment(TreeNode node, DocumentAttachment attachment, TreeProvider tree, FormFieldInfo ffi)
        {
            int width = 0;
            int height = 0;
            int maxSideSize = 0;

            // Set width, height and max side size from settings
            WebDAVHelper.GetImageResizeValues(ffi, ref width, ref height, ref maxSideSize);

            // Check out documents
            bool documentWasCheckedOut = WebDAVHelper.CheckOutDocument(node, tree);

            // Ensure binary data
            EnsureBinaryData(attachment);

            // Set height and width of attachment
            WebDAVHelper.SetAttachmentHeightAndWidth(attachment);

            // Unsorted
            if (ffi == null)
            {
                attachment = DocumentHelper.AddAttachment(node, null, attachment.AttachmentGUID, Guid.Empty, attachment, width, height, maxSideSize);
            }
            // File
            else if (ffi.DataType == FieldDataType.File)
            {
                attachment = DocumentHelper.AddAttachment(node, ffi.Name, attachment.AttachmentGUID, Guid.Empty, attachment, width, height, maxSideSize);

                node.SetValue(ffi.Name, attachment.AttachmentGUID);
                // Update field attachment
                DocumentHelper.UpdateDocument(node, tree, ffi.Name);
            }
            // Documents attachment
            else if (ffi.DataType == DocumentFieldDataType.DocAttachments)
            {
                attachment = DocumentHelper.AddAttachment(node, null, attachment.AttachmentGUID, ffi.Guid, attachment, width, height, maxSideSize);
            }

            // Check in documents
            if (!documentWasCheckedOut)
            {
                WebDAVHelper.CheckInDocument(node, tree);
            }

            // Log synchronization task
            WebDAVHelper.LogSynchronization(node, TaskTypeEnum.UpdateDocument, tree);

            // Clean binary data
            WebDAVHelper.CleanBinaryData(attachment);
        }


        /// <summary>
        /// Moves media file to content folder.
        /// </summary>
        /// <param name="contentFolder">Destination media folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveMediaFileToContentFolder(ContentFolder contentFolder, string destName)
        {
            TreeNode newNode = null;
            bool newDocumentCreated = false;
            TreeProvider tree = null;

            try
            {
                TreeNode destNode = contentFolder.UrlParser.Node;
                tree = destNode.TreeProvider;
                UrlParser destParser = contentFolder.UrlParser;

                // Only content folder and check 'Create' permission on the document or user is group admin and check if document isn't checked out by another user
                if (((destParser.ItemType == ItemTypeEnum.ContentAliasPathFolder) || (destParser.ItemType == ItemTypeEnum.ContentCultureCodeFolder))
                    && (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(destNode, NodePermissionsEnum.Create) || WebDAVHelper.IsGroupAdministrator(destParser.Group))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(destNode))
                {
                    FormFieldInfo ffi = null;

                    string fileName = IO.Path.GetFileNameWithoutExtension(destName);
                    string safeFileName = WebDAVHelper.GetSafeFileName(fileName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
                    string aliasPath = destNode.NodeAliasPath + "/" + safeFileName;
                    string culture = destParser.CultureCode;

                    // Get existing document. If document does'nt exist, get document with default culture.
                    TreeNode existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, culture, true, SystemDocumentTypes.File, null,
                                                                       null, 1, false, null, tree);

                    if (existingNode != null)
                    {
                        newNode = existingNode;

                        // Existing document is default
                        if (newNode.DocumentCulture.ToLowerCSafe() != culture.ToLowerCSafe())
                        {
                            // Insert new culture
                            DocumentHelper.InsertNewCultureVersion(newNode, tree, culture);
                        }

                        ffi = WebDAVHelper.GetFormFieldInfo(newNode, "FileAttachment");
                    }
                    else
                    {
                        // Create new node - content
                        newNode = WebDAVHelper.GetNewContent(destName, destNode, tree, culture, out ffi);
                        // Insert document
                        DocumentHelper.InsertDocument(newNode, destNode, tree);
                        newDocumentCreated = true;
                    }

                    // Get safe file name
                    string newFileName = WebDAVHelper.GetSafeFileName(destName, SiteContext.CurrentSiteName, AttachmentInfoProvider.MAXATTACHMENTNAMELENGTH);

                    // Create new attachment from media file info
                    var newAttachment = WebDAVHelper.CreateAttachmentFromMediaFileInfo(Guid.NewGuid(), newFileName, this, UrlParser.MediaLibraryInfo);

                    if ((newAttachment != null) && (ffi != null) && (ffi.DataType == FieldDataType.File))
                    {
                        int width = 0;
                        int height = 0;
                        int maxSideSize = 0;

                        // Set width, height and max side size from settings
                        WebDAVHelper.GetImageResizeValues(ffi, ref width, ref height, ref maxSideSize);

                        // Ensure binary data
                        EnsureBinaryData(newAttachment);

                        // Set height and width of attachment
                        WebDAVHelper.SetAttachmentHeightAndWidth(newAttachment);

                        // Check out the document
                        WebDAVHelper.CheckOutDocument(newNode, tree);

                        // Add the file
                        newAttachment = DocumentHelper.AddAttachment(newNode, ffi.Name, Guid.Empty, Guid.Empty, newAttachment, width, height, maxSideSize);

                        // Set Group ID
                        if (destParser.Group != null)
                        {
                            newNode.SetValue("NodeGroupID", ValidationHelper.GetInteger(destParser.Group.GetValue("GroupID"), 0));
                        }

                        // Update the document
                        DocumentHelper.UpdateDocument(newNode, tree, ffi.Name);

                        // Check in the document
                        if (newNode.DocumentCheckedOutAutomatically)
                        {
                            WebDAVHelper.CheckInDocument(newNode, tree);
                        }

                        // Clean binary data
                        WebDAVHelper.CleanBinaryData(newAttachment);

                        // Delete this media file
                        DeleteMediaFile();
                    }
                }
                else
                {
                    return new NoContentResponse();
                }
            }
            catch (ApplicationException appEx)
            {
                EventLogProvider.LogEvent(EventType.WARNING, "MediaFileResource_MoveMediaFileToContentFolder",
                             "WebDAV", appEx.Message, null, MembershipContext.AuthenticatedUser.UserID, MembershipContext.AuthenticatedUser.UserName, 0, null, null, SiteContext.CurrentSiteID);

                return new ServerErrorResponse();
            }
            catch (Exception ex)
            {
                // Delete the document if something failed
                if (newDocumentCreated && (newNode != null) && (newNode.DocumentID > 0))
                {
                    DocumentHelper.DeleteDocument(newNode, tree, false, true);
                }

                EventLogProvider.LogException("MediaFileResource_MoveMediaFileToContentFolder", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new CreatedResponse();
        }


        /// <summary>
        /// Ensures attachment binary data.
        /// </summary>
        /// <param name="newAttachment">New attachment info</param>
        private void EnsureBinaryData(DocumentAttachment newAttachment)
        {
            if (newAttachment != null)
            {
                // Ensure binary data
                if (InputStream != null)
                {
                    newAttachment.AttachmentBinary = BinaryData.GetByteArrayFromStream(InputStream);
                    newAttachment.AttachmentSize = (int)InputStream.Length;
                }
            }
        }


        /// <summary>
        /// Ensures media file binary data.
        /// </summary>
        /// <param name="newMediaFile">New media file info</param>
        private void EnsureBinaryData(MediaFileInfo newMediaFile)
        {
            if (newMediaFile != null)
            {
                // Ensure binary data
                if (InputStream != null)
                {
                    newMediaFile.FileBinaryStream = InputStream;
                    newMediaFile.FileSize = InputStream.Length;
                }
            }
        }

        #endregion


        #region "ILock Members"

        /// <summary>
        /// Locks this item.
        /// </summary>
        /// <param name="lockInfo">Describes a lock on this item</param>
        public override WebDAVResponse Lock(ref LockInfo lockInfo)
        {
            try
            {
                MediaLibraryInfo library = UrlParser.MediaLibraryInfo;

                // Check if current user has ('Read' and 'FileModify') or ('FileCreate' and size of file is zero)
                if ((WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(library, "read") && WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(library, "filemodify"))
                    || (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(library, "filecreate") && (UrlParser.MediaFileInfo != null) && (UrlParser.MediaFileInfo.FileSize == 0)))
                {
                    return new OkResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFileResource_Lock", "WebDAV", ex);
                return new ConflictResponse();
            }

            return new ForbiddenResponse();
        }

        #endregion
    }
}
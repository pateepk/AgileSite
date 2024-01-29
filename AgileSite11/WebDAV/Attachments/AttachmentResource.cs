using System;

using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.DocumentEngine;
using CMS.SiteProvider;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

using Stream = System.IO.Stream;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.WebDAV
{
    /// <summary>
    /// Class that represents the attachment file.
    /// </summary>
    internal class AttachmentResource : BaseAttachmentResource
    {
        #region "Variables"

        private byte[] mBinaryData;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets binary data of attachment.
        /// </summary>
        public override byte[] BinaryData
        {
            get
            {
                if (mBinaryData != null)
                {
                    return mBinaryData;
                }

                try
                {
                    var attachment = UrlParser.Attachment;
                    if (attachment != null)
                    {
                        // Get binary data from DB
                        var tempAtt = DocumentHelper.GetAttachment(attachment.AttachmentGUID, SiteContext.CurrentSiteName);

                        if (tempAtt != null)
                        {
                            mBinaryData = tempAtt.AttachmentBinary;
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("AttachmentResource_BinaryData", "WebDAV", ex);
                }

                return mBinaryData;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes attachment resource.
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
        public AttachmentResource(string path, string name, string mimeType, long size, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, mimeType, size, created, modified, urlParser, engine, parent)
        {
        }

        #endregion


        #region "IResource Members"

        /// <summary>
        /// Saves the content of the resource from the specified stream to the WebDAV repository.
        /// </summary>
        /// <param name="content">Stream to read the content of the resource from</param>
        /// <param name="contentType">Indicates the media type of the resource</param>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse SaveFromStream(Stream content, string contentType)
        {
            if ((content != null) && (content.Length > 0))
            {
                bool documentWasCheckedOut = true;
                TreeNode node = UrlParser.Node;

                try
                {
                    var attachment = UrlParser.Attachment;
                    if (attachment != null)
                    {
                        // Check authorization to 'Modify' the document and field or unsorted attachment type or is group administrator and check if document is checked out by another user
                        if ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                            && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(node) && WebDAVHelper.CanCurrentUserApproveCustomStep(node)
                            && ((UrlParser.ItemType == ItemTypeEnum.UnsortedAttachmentFile) || (UrlParser.ItemType == ItemTypeEnum.FieldAttachmentFile)))
                        {
                            TreeProvider tree = UrlParser.TreeProvider;

                            // Check out the document
                            documentWasCheckedOut = WebDAVHelper.CheckOutDocument(node, tree);

                            // Check if attachment is temporary
                            if (attachment.AttachmentFormGUID != Guid.Empty)
                            {
                                // Delete temporary attachment
                                AttachmentInfoProvider.DeleteTemporaryAttachment(attachment.AttachmentGUID, SiteContext.CurrentSiteName);
                            }

                            // Set binary data
                            attachment.AttachmentBinary = Core.BinaryData.GetByteArrayFromStream(content);
                            attachment.AttachmentSize = (int)content.Length;

                            // Set height and width of attachment
                            WebDAVHelper.SetAttachmentHeightAndWidth(attachment);

                            int width = 0;
                            int height = 0;
                            int maxSideSize = 0;
                            bool taskLogged = false;

                            // Field attachment
                            if (UrlParser.ItemType == ItemTypeEnum.FieldAttachmentFile)
                            {
                                FormFieldInfo ffi = WebDAVHelper.GetFormFieldInfo(node, UrlParser.FieldName);

                                if (ffi != null)
                                {
                                    // Set width, height and max side size from settings
                                    WebDAVHelper.GetImageResizeValues(ffi, ref width, ref height, ref maxSideSize);

                                    attachment.SetValue("AttachmentIsUnsorted", null);

                                    // File attachment
                                    if (ffi.DataType == FieldDataType.File)
                                    {
                                        attachment = DocumentHelper.AddAttachment(node, ffi.Name, attachment.AttachmentGUID, Guid.Empty, attachment, width, height, maxSideSize);
                                        // Update attachment field
                                        node.SetValue(ffi.Name, attachment.AttachmentGUID);
                                        DocumentHelper.UpdateDocument(node, tree, ffi.Name);
                                        taskLogged = true;
                                    }
                                    // Grouped attachment
                                    else if (ffi.DataType == DocumentFieldDataType.DocAttachments)
                                    {
                                        Guid groupGuid = ffi.Guid;
                                        attachment = DocumentHelper.AddAttachment(node, null, attachment.AttachmentGUID, groupGuid, attachment, width, height, maxSideSize);
                                    }
                                }
                            }
                            // Unsorted attachment
                            else if (UrlParser.ItemType == ItemTypeEnum.UnsortedAttachmentFile)
                            {
                                // Set width, height and max side size from settings
                                WebDAVHelper.GetImageResizeValues(ref width, ref height, ref maxSideSize);

                                attachment = DocumentHelper.AddAttachment(node, null, attachment.AttachmentGUID, Guid.Empty, attachment, width, height, maxSideSize);
                            }

                            // Log synchronization task
                            if (!taskLogged)
                            {
                                WebDAVHelper.LogSynchronization(node, TaskTypeEnum.UpdateDocument, tree);
                            }

                            // Clean binary data
                            WebDAVHelper.CleanBinaryData(attachment);
                        }
                        else
                        {
                            return new AccessDeniedResponse();
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("AttachmentResource_SaveFromStream", "WebDAV", ex);
                    return new ServerErrorResponse();
                }
                finally
                {
                    if (!documentWasCheckedOut)
                    {
                        // Check in the document
                        WebDAVHelper.CheckInDocument(node, UrlParser.TreeProvider);
                    }
                }
            }

            return new OkResponse();
        }

        #endregion


        #region "HierarchyItem Members Override"

        /// <summary>
        /// Moves this item to the destination folder under a new name.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse MoveTo(IFolder folder, string destName)
        {
            try
            {
                string extension = IO.Path.GetExtension(destName);
                TreeNode sourceNode = UrlParser.Node;

                // Check allowed extensions of field or grouped attachment
                if (!WebDAVHelper.IsExtensionAllowedForBrowseMode(extension, SiteContext.CurrentSiteName))
                {
                    return new NoContentResponse();
                }

                BaseFolder baseFolder = folder as BaseFolder;
                if (baseFolder != null)
                {
                    UrlParser sourceParser = UrlParser;
                    UrlParser destParser = baseFolder.UrlParser;

                    // Check if source form field info is allowed empty
                    if ((sourceParser.ItemType == ItemTypeEnum.FieldAttachmentFile) && !WebDAVHelper.IsSameAttachmentFolder(sourceParser, destParser)
                        && !WebDAVHelper.IsAllowEmptyFormFieldInfo(sourceNode, sourceParser.FieldName))
                    {
                        return new NoContentResponse();
                    }

                    // Destination is attachment folder
                    if (baseFolder is AttachmentFolder)
                    {
                        // Get destination form field info
                        FormFieldInfo destFfi = WebDAVHelper.GetFormFieldInfo(destParser.Node, destParser.FieldName);

                        // Check whether the extension of the attachment is allowed
                        if (!WebDAVHelper.IsExtensionAllowedForBrowseMode(extension, destFfi, SiteContext.CurrentSiteName))
                        {
                            return new NoContentResponse();
                        }

                        return MoveAttachmentToAttachmentFolder(destName, destParser, destFfi);
                    }
                    // Destination is media folder
                    else if (baseFolder is MediaFolder)
                    {
                        return MoveAttachmentToMediaFolder(folder as MediaFolder, destName);
                    }
                    // Destination is content folder
                    else if (baseFolder is ContentFolder)
                    {
                        return MoveAttachmentToContentFolder(folder as ContentFolder, destName);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("AttachmentResource_MoveTo", "WebDAV", ex);
            }

            return new NoContentResponse();
        }


        /// <summary>
        /// Deletes this attachment.
        /// </summary>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse Delete()
        {
            TreeNode node = UrlParser.Node;

            // Check authorization to 'Modify' the document or group administrator and check if document is checked out by another user and current user can approve custom step
            if ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(node) && WebDAVHelper.CanCurrentUserApproveCustomStep(node)
                && ((UrlParser.ItemType == ItemTypeEnum.UnsortedAttachmentFile) || (UrlParser.ItemType == ItemTypeEnum.FieldAttachmentFile)))
            {
                try
                {
                    // Check form field is allowed empty
                    if ((UrlParser.ItemType == ItemTypeEnum.FieldAttachmentFile) && !WebDAVHelper.IsAllowEmptyFormFieldInfo(node, UrlParser.FieldName))
                    {
                        return new AccessDeniedResponse();
                    }

                    TreeProvider tree = UrlParser.TreeProvider;

                    // Check out the document
                    bool documentWasCheckedOut = WebDAVHelper.CheckOutDocument(node, tree);

                    // Delete attachment info
                    DeleteAttachmentInfo(node);

                    // Check in the document
                    if (!documentWasCheckedOut)
                    {
                        WebDAVHelper.CheckInDocument(node, tree);
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("AttachmentResource_Delete", "WebDAV", ex);
                    return new ServerErrorResponse();
                }
            }
            else
            {
                return new AccessDeniedResponse();
            }

            return new NoContentResponse();
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Deletes attachment info.
        /// </summary>
        /// <param name="node">Document</param>
        /// <returns>Indicates if attachment info is deleted</returns>
        private bool DeleteAttachmentInfo(TreeNode node)
        {
            bool result = false;

            try
            {
                if ((node != null) && (UrlParser.Attachment != null))
                {
                    var tree = node.TreeProvider;
                    var attachment = UrlParser.Attachment;

                    bool taskLogged = false;

                    // Field attachment
                    if (UrlParser.ItemType == ItemTypeEnum.FieldAttachmentFile)
                    {
                        FormFieldInfo ffi = WebDAVHelper.GetFormFieldInfo(node, UrlParser.FieldName);

                        if ((ffi != null) && ffi.AllowEmpty)
                        {
                            // File attachment
                            if (ffi.DataType == FieldDataType.File)
                            {
                                // Delete field attachment
                                DocumentHelper.DeleteAttachment(node, ffi.Name);
                                node.SetValue(ffi.Name, null);
                                // Update field attachment
                                DocumentHelper.UpdateDocument(node, tree, ffi.Name);
                                result = true;
                                taskLogged = true;
                            }
                            // Grouped attachment
                            else if (ffi.DataType == DocumentFieldDataType.DocAttachments)
                            {
                                DocumentHelper.DeleteAttachment(node, attachment.AttachmentGUID);
                                result = true;
                            }
                        }
                    }
                    // Unsorted attachment
                    else if (UrlParser.ItemType == ItemTypeEnum.UnsortedAttachmentFile)
                    {
                        // Delete unsorted attachment
                        DocumentHelper.DeleteAttachment(node, attachment.AttachmentGUID);
                        result = true;
                    }

                    // Log synchronization task if attachment was deleted and task wasn't logged
                    if (result && !taskLogged)
                    {
                        WebDAVHelper.LogSynchronization(node, TaskTypeEnum.UpdateDocument, tree);
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("AttachmentResource_DeleteAttachmentInfo", "WebDAV", ex);
            }

            return result;
        }


        /// <summary>
        /// Moves this item to the attachment folder.
        /// </summary>
        /// <param name="destName">Name of the destination item</param>
        /// <param name="destParser">Url parser of destination folder</param>
        /// <param name="destFormFieldInfo">Form field info of destination folder</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveAttachmentToAttachmentFolder(string destName, UrlParser destParser, FormFieldInfo destFormFieldInfo)
        {
            if ((destParser != null) && ((destParser.ItemType == ItemTypeEnum.AttachmentFieldNameFolder) || (destParser.ItemType == ItemTypeEnum.AttachmentUnsortedFolder)))
            {
                try
                {
                    TreeNode sourceNode = UrlParser.Node;
                    TreeNode destNode = destParser.Node;
                    string safeFileName = WebDAVHelper.GetSafeFileName(destName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
                    string extension = IO.Path.GetExtension(safeFileName);

                    if ((sourceNode != null) && (destNode != null))
                    {
                        TreeNode node = null;
                        UrlParser sourceParser = UrlParser;
                        TreeProvider tree = sourceParser.TreeProvider;

                        // Rename attachment name in the same document
                        if ((sourceNode.DocumentID == destNode.DocumentID))
                        {
                            // Check 'Modify' permission on the source document or group administrator and check if document is checked out by another user and current user can not approve custom step
                            if ((!WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify) && !WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                                || WebDAVHelper.IsDocumentCheckedOutByAnotherUser(sourceNode) 
                                || !WebDAVHelper.CanCurrentUserApproveCustomStep(sourceNode))
                            {
                                return new AccessDeniedResponse();
                            }

                            node = sourceNode;
                        }
                        // Move attachment to the other document
                        else
                        {
                            // Check 'Modify' permission on the source document and destination document or group administrator and check if documents are checked out by another user
                            if ((!WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify) && !WebDAVHelper.IsGroupAdministrator(sourceParser.Group))
                                || (!WebDAVHelper.IsCurrentUserAuthorizedPerDocument(destNode, NodePermissionsEnum.Modify) && !WebDAVHelper.IsGroupAdministrator(destParser.Group))
                                || WebDAVHelper.IsDocumentCheckedOutByAnotherUser(sourceNode) || WebDAVHelper.IsDocumentCheckedOutByAnotherUser(destNode)
                                || !WebDAVHelper.CanCurrentUserApproveCustomStep(sourceNode) || !WebDAVHelper.CanCurrentUserApproveCustomStep(destNode))
                            {
                                return new AccessDeniedResponse();
                            }

                            node = destNode;
                        }

                        DocumentAttachment newAttachmentInfo = null;

                        // MOVE unsorted attachment to [_unsorted] folder
                        if ((sourceParser.ItemType == ItemTypeEnum.UnsortedAttachmentFile) && (destParser.ItemType == ItemTypeEnum.AttachmentUnsortedFolder))
                        {
                            newAttachmentInfo = WebDAVHelper.GetExistingOrNewAttachment(node, sourceParser, destParser, safeFileName);

                            return MoveAttachment(sourceNode, destNode, null, null, newAttachmentInfo);
                        }
                        // MOVE unsorted attachment to field folder
                        else if ((sourceParser.ItemType == ItemTypeEnum.UnsortedAttachmentFile) && (destParser.ItemType == ItemTypeEnum.AttachmentFieldNameFolder) && (destFormFieldInfo != null))
                        {
                            if (destFormFieldInfo != null)
                            {
                                newAttachmentInfo = WebDAVHelper.GetExistingOrNewAttachment(node, sourceParser, destParser, safeFileName);

                                return MoveAttachment(sourceNode, destNode, null, destFormFieldInfo, newAttachmentInfo);
                            }
                        }
                        // MOVE field attachment to [_unsorted] folder
                        else if ((sourceParser.ItemType == ItemTypeEnum.FieldAttachmentFile) && (destParser.ItemType == ItemTypeEnum.AttachmentUnsortedFolder))
                        {
                            FormFieldInfo sourceFormFieldInfo = WebDAVHelper.GetFormFieldInfo(sourceNode, sourceParser.FieldName);

                            if (sourceFormFieldInfo != null)
                            {
                                newAttachmentInfo = WebDAVHelper.GetExistingOrNewAttachment(node, sourceParser, destParser, safeFileName);

                                return MoveAttachment(sourceNode, destNode, sourceFormFieldInfo, null, newAttachmentInfo);
                            }
                        }
                        // MOVE field attachment to field folder
                        else if ((sourceParser.ItemType == ItemTypeEnum.FieldAttachmentFile) && (destParser.ItemType == ItemTypeEnum.AttachmentFieldNameFolder))
                        {
                            FormFieldInfo sourceFormFieldInfo = WebDAVHelper.GetFormFieldInfo(sourceNode, sourceParser.FieldName);

                            if ((sourceFormFieldInfo != null) && (destFormFieldInfo != null))
                            {
                                newAttachmentInfo = WebDAVHelper.GetExistingOrNewAttachment(node, sourceParser, destParser, safeFileName);

                                return MoveAttachment(sourceNode, destNode, sourceFormFieldInfo, destFormFieldInfo, newAttachmentInfo);
                            }
                        }

                        return new NoContentResponse();
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("AttachmentResource_MoveAttachmentToAttachmentFolder", "WebDAV", ex);
                    return new ServerErrorResponse();
                }
            }

            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Delete old attachment and save new attachment.
        /// </summary>
        /// <param name="sourceNode">Source document</param>
        /// <param name="destNode">Destination document</param>
        /// <param name="sourceFfi">Source form field info</param>
        /// <param name="destFfi">Source form field info</param>
        /// <param name="newAttachment">New attachment info</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveAttachment(TreeNode sourceNode, TreeNode destNode, FormFieldInfo sourceFfi, FormFieldInfo destFfi, DocumentAttachment newAttachment)
        {
            // Unsorted, field or grouped attachment
            if ((destFfi == null) || (destFfi.DataType == FieldDataType.File) || (destFfi.DataType == DocumentFieldDataType.DocAttachments))
            {
                TreeProvider tree = sourceNode.TreeProvider;

                int width = 0;
                int height = 0;
                int maxSideSize = 0;
                bool sourceNodeWasCheckedOut = false;
                bool destinationNodeWasCheckedOut = false;

                // Check out the documents
                CheckOut(sourceNode, destNode, tree, out sourceNodeWasCheckedOut, out destinationNodeWasCheckedOut);

                // Ensure binary data
                EnsureBinaryData(newAttachment);

                // Set width, height and max side size from settings
                WebDAVHelper.GetImageResizeValues(destFfi, ref width, ref height, ref maxSideSize);

                // Set height and width of attachment
                WebDAVHelper.SetAttachmentHeightAndWidth(newAttachment);

                // Backup original value
                bool checkUniqueAttachmentNames = tree.CheckUniqueAttachmentNames;

                if (sourceNode.DocumentID == destNode.DocumentID)
                {
                    destNode = sourceNode;
                    if (UrlParser.Attachment.AttachmentGUID != newAttachment.AttachmentGUID)
                    {
                        // Disable check unique attachment name
                        tree.CheckUniqueAttachmentNames = false;
                    }
                }

                // Unsorted attachment
                if (destFfi == null)
                {
                    newAttachment = DocumentHelper.AddAttachment(destNode, null, newAttachment.AttachmentGUID, Guid.Empty, newAttachment, width, height, maxSideSize);
                    // Log synchronization task
                    WebDAVHelper.LogSynchronization(destNode, TaskTypeEnum.UpdateDocument, tree);
                }
                else if (destFfi.DataType == FieldDataType.File)
                {
                    // Field 'File' attachment
                    newAttachment = DocumentHelper.AddAttachment(destNode, destFfi.Name, newAttachment.AttachmentGUID, Guid.Empty, newAttachment, width, height, maxSideSize);
                    // Update field
                    destNode.SetValue(destFfi.Name, newAttachment.AttachmentGUID);
                    DocumentHelper.UpdateDocument(destNode, tree, destFfi.Name);
                }
                else if (destFfi.DataType == DocumentFieldDataType.DocAttachments)
                {
                    // Grouped 'DocumentAttachments' attachment
                    newAttachment = DocumentHelper.AddAttachment(destNode, null, newAttachment.AttachmentGUID, destFfi.Guid, newAttachment, width, height, maxSideSize);
                    // Log synchronization task
                    WebDAVHelper.LogSynchronization(destNode, TaskTypeEnum.UpdateDocument, tree);
                }

                // Set original value
                tree.CheckUniqueAttachmentNames = checkUniqueAttachmentNames;

                // Delete source attachment if source is different as destination
                if (UrlParser.Attachment.AttachmentGUID != newAttachment.AttachmentGUID)
                {
                    // Delete source attachment info
                    DeleteAttachmentInfo(sourceNode);
                }

                // Check in the documents
                CheckIn(sourceNode, destNode, tree, sourceNodeWasCheckedOut, destinationNodeWasCheckedOut);

                // Clean binary data
                WebDAVHelper.CleanBinaryData(newAttachment);
            }
            else
            {
                return new AccessDeniedResponse();
            }

            return new CreatedResponse();
        }


        /// <summary>
        /// Moves this item to the media folder.
        /// </summary>
        /// <param name="mediaFolder">Destination media folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveAttachmentToMediaFolder(MediaFolder mediaFolder, string destName)
        {
            try
            {
                UrlParser destParser = mediaFolder.UrlParser;
                TreeNode sourceNode = UrlParser.Node;

                // Check authorization to modify attachment or group administrator and check if document is checked out by another user and check authorization to create media file 
                if ((destParser.ItemType == ItemTypeEnum.MediaLibraryName)
                    && (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(sourceNode) && WebDAVHelper.CanCurrentUserApproveCustomStep(sourceNode)
                    && WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(destParser.MediaLibraryInfo, "filecreate"))
                {
                    // Safe file name
                    string safeFileName = WebDAVHelper.GetSafeFileName(destName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_MEDIA_FILENAME_LENGTH);

                    // Move attachment to media folder
                    if (MoveToMediaFolder(destParser.MediaLibraryInfo, safeFileName, destName, destParser.FilePath))
                    {
                        var tree = UrlParser.TreeProvider;
                        var nodeWasCheckedOut = WebDAVHelper.CheckOutDocument(sourceNode, tree);

                        DeleteAttachmentInfo(sourceNode);

                        if (!nodeWasCheckedOut)
                        {
                            WebDAVHelper.CheckInDocument(sourceNode, tree);
                        }
                    }
                    else
                    {
                        return new NoContentResponse();
                    }
                }
                else
                {
                    return new NoContentResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContentResource_MoveAttachmentToMediaFolder", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new CreatedResponse();
        }


        /// <summary>
        /// Moves attachment to content folder.
        /// </summary>
        /// <param name="contentFolder">Destination content folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveAttachmentToContentFolder(ContentFolder contentFolder, string destName)
        {
            TreeNode newNode = null;
            bool newDocumentCreated = false;
            TreeProvider tree = null;

            try
            {
                TreeNode sourceNode = UrlParser.Node;
                UrlParser destParser = contentFolder.UrlParser;
                TreeNode destNode = destParser.Node;

                // Check if source form field info can be empty
                if ((sourceNode != null) && (UrlParser.ItemType == ItemTypeEnum.FieldAttachmentFile) && !WebDAVHelper.IsAllowEmptyFormFieldInfo(sourceNode, UrlParser.FieldName))
                {
                    return new NoContentResponse();
                }

                // Check 'Modify' permission on the source document and destination document and check if documents are checked out by another user
                if ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                    && (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(destNode, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(destParser.Group))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(sourceNode) && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(destNode)
                    && WebDAVHelper.CanCurrentUserApproveCustomStep(sourceNode) && WebDAVHelper.CanCurrentUserApproveCustomStep(destNode)
                    && ((destParser.ItemType == ItemTypeEnum.ContentAliasPathFolder) || (destParser.ItemType == ItemTypeEnum.ContentCultureCodeFolder))
                    && ((UrlParser.ItemType == ItemTypeEnum.UnsortedAttachmentFile) || (UrlParser.ItemType == ItemTypeEnum.FieldAttachmentFile)))
                {
                    tree = UrlParser.TreeProvider;

                    FormFieldInfo contentFfi;

                    string fileName = IO.Path.GetFileNameWithoutExtension(destName);
                    string safeFileName = WebDAVHelper.GetSafeFileName(fileName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
                    string aliasPath = destNode.NodeAliasPath + "/" + safeFileName;

                    // Get existing node
                    TreeNode existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, destParser.CultureCode, false, SystemDocumentTypes.File, null, null, 1, false, null, tree);

                    if (existingNode != null)
                    {
                        newNode = existingNode;
                        contentFfi = WebDAVHelper.GetFormFieldInfo(newNode, "FileAttachment");
                    }
                    else
                    {
                        // Create new node - content
                        newNode = WebDAVHelper.GetNewContent(destName, destNode, tree, destParser.CultureCode, out contentFfi);

                        if (newNode == null)
                        {
                            return new NoContentResponse();
                        }

                        // Document exists
                        if (newNode.NodeID > 0)
                        {
                            // Insert new culture
                            if (newNode.DocumentCulture != UrlParser.CultureCode)
                            {
                                DocumentHelper.InsertNewCultureVersion(newNode, tree, UrlParser.CultureCode);
                            }
                        }
                        // Insert new document
                        else
                        {
                            DocumentHelper.InsertDocument(newNode, destNode, tree);
                        }

                        newDocumentCreated = true;
                    }

                    string newFileName = WebDAVHelper.GetSafeFileName(destName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);

                    // Get existing or create new attachment info
                    var newAttachment = WebDAVHelper.GetExistingOrNewAttachment(newNode, UrlParser, destParser, newFileName);

                    // Ensure binary data
                    EnsureBinaryData(newAttachment);

                    // Set height and width of attachment
                    WebDAVHelper.SetAttachmentHeightAndWidth(newAttachment);

                    // Get width, height and max side size from settings
                    int width = 0;
                    int height = 0;
                    int maxSideSize = 0;

                    // Set width, height and max side size from settings
                    WebDAVHelper.GetImageResizeValues(contentFfi, ref width, ref height, ref maxSideSize);

                    // Add the file
                    newAttachment = DocumentHelper.AddAttachment(newNode, contentFfi.Name, newAttachment.AttachmentGUID, Guid.Empty, newAttachment, width, height, maxSideSize);

                    // Update attachment field
                    newNode.SetValue(contentFfi.Name, newAttachment.AttachmentGUID);

                    // Set Group ID
                    if (destParser.Group != null)
                    {
                        newNode.SetValue("NodeGroupID", ValidationHelper.GetInteger(destParser.Group.GetValue("GroupID"), 0));
                    }

                    // Update the new document
                    DocumentHelper.UpdateDocument(newNode, tree, contentFfi.Name);

                    // Delete source attachment
                    if (UrlParser.Attachment.AttachmentGUID != newAttachment.AttachmentGUID)
                    {
                        var nodeWasCheckedOut = WebDAVHelper.CheckOutDocument(sourceNode, tree);

                        DeleteAttachmentInfo(sourceNode);

                        if (!nodeWasCheckedOut)
                        {
                            WebDAVHelper.CheckInDocument(sourceNode, tree);
                        }
                    }

                    // Clean binary data
                    WebDAVHelper.CleanBinaryData(newAttachment);
                }
                else
                {
                    return new NoContentResponse();
                }
            }
            catch (ApplicationException appEx)
            {
                EventLogProvider.LogEvent(EventType.WARNING, "AttachmentResource_MoveAttachmentToContentFolder",
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

                EventLogProvider.LogException("AttachmentResource_MoveAttachmentToContentFolder", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new CreatedResponse();
        }


        /// <summary>
        /// Checks out documents.
        /// </summary>
        /// <param name="sourceNode">Source document</param>
        /// <param name="destinationNode">Destination document</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="sourceNodeWasCheckedOut">Indicates if source document was checked</param>
        /// <param name="destinationNodeWasCheckedOut">Indicates if destination document was checked</param>
        private static void CheckOut(TreeNode sourceNode, TreeNode destinationNode, TreeProvider tree, out bool sourceNodeWasCheckedOut, out bool destinationNodeWasCheckedOut)
        {
            // The same document
            if (sourceNode.DocumentID == destinationNode.DocumentID)
            {
                // Check out the document
                sourceNodeWasCheckedOut = WebDAVHelper.CheckOutDocument(sourceNode, tree);
                destinationNodeWasCheckedOut = true;
            }
            else
            {
                // Check out source and the destination document
                sourceNodeWasCheckedOut = WebDAVHelper.CheckOutDocument(sourceNode, tree);
                destinationNodeWasCheckedOut = WebDAVHelper.CheckOutDocument(destinationNode, tree);
            }
        }


        /// <summary>
        /// Checks in documents.
        /// </summary>
        /// <param name="sourceNode">Source node</param>
        /// <param name="destinationNode">Destination node</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="sourceNodeWasCheckedOut">Indicates if source document was check out</param>
        /// <param name="destinationNodeWasCheckedOut">Indicates if destination document was check out</param>
        private static void CheckIn(TreeNode sourceNode, TreeNode destinationNode, TreeProvider tree, bool sourceNodeWasCheckedOut, bool destinationNodeWasCheckedOut)
        {
            // The same document
            if (sourceNode.DocumentID == destinationNode.DocumentID)
            {
                if (!sourceNodeWasCheckedOut)
                {
                    // Check in the document
                    WebDAVHelper.CheckInDocument(sourceNode, tree);
                }
            }
            else
            {
                // Check in source and the destination document
                if (!sourceNodeWasCheckedOut)
                {
                    WebDAVHelper.CheckInDocument(sourceNode, tree);
                }
                if (!destinationNodeWasCheckedOut)
                {
                    WebDAVHelper.CheckInDocument(destinationNode, tree);
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
                TreeNode node = UrlParser.Node;

                // Check if current user has permissions 'Read' and 'Modify' or is group administrator and document is not checkouted by another user and current user can approve custom step
                if ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, new [] { NodePermissionsEnum.Read, NodePermissionsEnum.Modify }) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(node) && WebDAVHelper.CanCurrentUserApproveCustomStep(node))
                {
                    return new OkResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("AttachmentResource_Lock", "WebDAV", ex);
                return new ConflictResponse();
            }

            return new ForbiddenResponse();
        }


        /// <summary>
        /// Removes lock with the specified token from this item or deletes lock-null item.
        /// </summary>
        /// <param name="lockToken">Lock with this token should be removed from the item</param>
        public override WebDAVResponse Unlock(string lockToken)
        {
            try
            {
                var attachment = UrlParser.Attachment;
                if (attachment != null)
                {
                    if (!string.IsNullOrEmpty(attachment.AttachmentHash))
                    {
                        TreeNode node = UrlParser.Node;

                        if (node != null)
                        {
                            var tree = UrlParser.TreeProvider;

                            // Get the node workflow
                            var wi = WorkflowManager.GetNodeWorkflow(node);
                            var vm = VersionManager.GetInstance(tree);

                            if (wi != null)
                            {
                                // Ensure the document version
                                vm.EnsureVersion(node, node.IsPublished);
                            }

                            // Clear hash
                            attachment.AttachmentHash = null;

                            attachment.AllowPartialUpdate = true;
                            DocumentHelper.UpdateAttachment(node, attachment);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("AttachmentResource_Unlock", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new NoContentResponse();
        }

        #endregion
    }
}
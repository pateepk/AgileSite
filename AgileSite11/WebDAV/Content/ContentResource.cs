using System;

using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Base;
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
    /// Class that represents the CMS file.
    /// </summary>
    internal class ContentResource : BaseAttachmentResource
    {
        #region "Variables"

        private byte[] mBinaryData;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets binary data of CMS file.
        /// </summary>
        public override byte[] BinaryData
        {
            get
            {
                if (mBinaryData == null)
                {
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
                        EventLogProvider.LogException("ContentResource_BinaryData", "WebDAV", ex);
                    }
                }

                return mBinaryData;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes content resource.
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
        public ContentResource(string path, string name, string mimeType, long size, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
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
                try
                {
                    var attachment = UrlParser.Attachment;
                    if (attachment != null)
                    {
                        // Node of current cms file
                        TreeNode node = UrlParser.Node;

                        bool isGroupAdmin = WebDAVHelper.IsGroupAdministrator(UrlParser.Group);

                        // Check if user is group administrator or has 'Modify' permission and document isn't checked out by another user
                        // Check zero size of CMS file and current user is group administrator or has 'Create' permission (node has the same permission as parent node)
                        if (((isGroupAdmin || WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Modify)) && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(node) && WebDAVHelper.CanCurrentUserApproveCustomStep(node))
                            || ((attachment.AttachmentSize == 0) && (isGroupAdmin || WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Create))))
                        {
                            TreeProvider tree = UrlParser.TreeProvider;

                            // Set binary data
                            attachment.AttachmentBinary = Core.BinaryData.GetByteArrayFromStream(content);
                            attachment.AttachmentSize = (int)content.Length;

                            // Set height and width of attachment
                            WebDAVHelper.SetAttachmentHeightAndWidth(attachment);

                            // Check if class exists
                            DataClassInfo ci = DataClassInfoProvider.GetDataClassInfo(SystemDocumentTypes.File);
                            if (ci == null)
                            {
                                throw new ApplicationException(string.Format("[ContentResource.SaveFromStream]: The class '{0}' wasn't found.", SystemDocumentTypes.File));
                            }

                            FormInfo fi = FormHelper.GetFormInfo(ci.ClassName, false);
                            if (fi != null)
                            {
                                FormFieldInfo ffi = fi.GetFormField("FileAttachment");

                                if (ffi != null)
                                {
                                    int width = 0;
                                    int height = 0;
                                    int maxSideSize = 0;

                                    // Set width, height and max side size from settings
                                    WebDAVHelper.GetImageResizeValues(ffi, ref width, ref height, ref maxSideSize);

                                    // Check if attachment is temporary
                                    if (attachment.AttachmentFormGUID != Guid.Empty)
                                    {
                                        // Delete temporary attachment
                                        AttachmentInfoProvider.DeleteTemporaryAttachment(attachment.AttachmentGUID, SiteContext.CurrentSiteName);
                                    }

                                    // Check out the document
                                    WebDAVHelper.CheckOutDocument(node, tree);

                                    string extension = IO.Path.GetExtension(UrlParser.FileName);
                                    attachment.AttachmentExtension = extension;
                                    attachment.AttachmentName = UrlParser.FileName;
                                    attachment.AttachmentMimeType = MimeTypeHelper.GetMimetype(extension);

                                    // Set height and width of attachment
                                    WebDAVHelper.SetAttachmentHeightAndWidth(attachment);

                                    // Save attachment
                                    attachment = DocumentHelper.AddAttachment(node, "FileAttachment", attachment.AttachmentGUID, attachment.AttachmentGroupGUID, attachment, width, height, maxSideSize);

                                    node.SetValue("FileAttachment", attachment.AttachmentGUID);
                                    // Set extension and type
                                    node.SetValue("DocumentType", extension);

                                    // Set Group ID
                                    if (UrlParser.Group != null)
                                    {
                                        node.SetValue("NodeGroupID", ValidationHelper.GetInteger(UrlParser.Group.GetValue("GroupID"), 0));
                                    }

                                    // Update the document
                                    DocumentHelper.UpdateDocument(node, tree, ffi.Name);

                                    // Check in the document
                                    if (node.DocumentCheckedOutAutomatically)
                                    {
                                        WebDAVHelper.CheckInDocument(node, tree);
                                    }
                                }
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
                    EventLogProvider.LogException("ContentResource_SaveFromStream", "WebDAV", ex);
                    return new ServerErrorResponse();
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
            string extension = IO.Path.GetExtension(destName);
            FormFieldInfo ffi = WebDAVHelper.GetFormFieldInfo(UrlParser.Node, "FileAttachment");

            // Check allowed extensions of cms file
            if (WebDAVHelper.IsExtensionAllowedForBrowseMode(extension, SiteContext.CurrentSiteName) && (ffi != null))
            {
                // Destination document is content folder
                if (folder is ContentFolder)
                {
                    string newNodeName = IO.Path.GetFileNameWithoutExtension(destName);

                    return MoveContentToContentFolder(folder as ContentFolder, newNodeName, extension, ffi);
                }
                // Destination document is attachment folder
                else if ((folder is AttachmentFolder) && (ffi.AllowEmpty))
                {
                    return MoveContentToAttachmentFolder(folder as AttachmentFolder, destName);
                }
                // Destination document is media folder
                else if ((folder is MediaFolder) && (ffi.AllowEmpty))
                {
                    return MoveContentToMediaFolder(folder as MediaFolder, destName);
                }
            }
            return new NoContentResponse();
        }


        /// <summary>
        /// Deletes CMS file.
        /// </summary>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse Delete()
        {
            try
            {
                TreeNode node = UrlParser.Node;
                // Check if document is CMS File
                bool isCMSFile = node.NodeClassName.EqualsCSafe(SystemDocumentTypes.File, true);

                // Check 'Delete' permission or group administrator and check if document is checked out by another user and current user can approve custom step
                if (isCMSFile && (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Delete) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(node) && WebDAVHelper.CanCurrentUserApproveCustomStep(node))
                {
                    return DeleteContent(false);
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContentResource_Delete", "WebDAV", ex);
                return new ServerErrorResponse();
            }
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Moves this item to the destination content folder under a new name.
        /// </summary>
        /// <param name="contentFolder">Destination content folder</param>
        /// <param name="newNodeName">New node name</param>
        /// <param name="extension">Extension</param>
        /// <param name="ffi">Form field info</param>
        private WebDAVResponse MoveContentToContentFolder(ContentFolder contentFolder, string newNodeName, string extension, FormFieldInfo ffi)
        {
            try
            {
                TreeNode sourceNode = UrlParser.Node;

                // Check allowed extension
                if ((sourceNode != null) && (contentFolder.UrlParser.Node != null) && WebDAVHelper.IsCMSFileExtensionAllowed(ffi, extension))
                {
                    TreeNode targetNode = contentFolder.UrlParser.Node;

                    // Check if move to himself
                    if (targetNode.DocumentID == sourceNode.DocumentID)
                    {
                        return new NoContentResponse();
                    }

                    // Check 'Modify' permission or group administrator and check if document is checked out by another user
                    if ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                        && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(sourceNode))
                    {
                        // Rename content
                        if (targetNode.DocumentID == sourceNode.Parent.DocumentID)
                        {
                            RenameDocument(sourceNode, newNodeName, extension);
                        }
                        else
                        {
                            string safeFileName = WebDAVHelper.GetSafeFileName(newNodeName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
                            // Move content to other node
                            MoveNode(sourceNode, targetNode, UrlParser.TreeProvider, safeFileName);
                        }
                    }

                    return new CreatedResponse();
                }
            }
            catch (ApplicationException appEx)
            {
                EventLogProvider.LogEvent(EventType.WARNING, "ContentResource_MoveContentToContentFolder",
                             "WebDAV", appEx.Message, null, MembershipContext.AuthenticatedUser.UserID, MembershipContext.AuthenticatedUser.UserName, 0, null, null, SiteContext.CurrentSiteID);

                return new ConflictResponse();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContentResource_MoveContentToContentFolder", "WebDAV", ex);
                return new ConflictResponse();
            }

            return new NoContentResponse();
        }


        /// <summary>
        /// Renames the document.
        /// </summary>
        /// <param name="node">The document</param>
        /// <param name="newName">New name</param>
        /// <param name="extension">Extension</param>
        private void RenameDocument(TreeNode node, string newName, string extension)
        {
            if (WebDAVHelper.CanCurrentUserApproveCustomStep(node))
            {
                TreeProvider tree = new TreeProvider(MembershipContext.AuthenticatedUser);

                // Ensure node alias
                newName = TreePathUtils.EnsureMaxNodeAliasLength(newName);

                // Check out document
                WebDAVHelper.CheckOutDocument(node, tree);

                // Set node alias
                string nodeAlias = TreePathUtils.GetSafeNodeAlias(newName, node.NodeSiteName);
                node.DocumentName = newName;
                node.NodeAlias = nodeAlias;
                node.SetValue("DocumentType", extension);

                string aliasPath = node.NodeAliasPath.Substring(0, node.NodeAliasPath.LastIndexOfCSafe("/")) + "/" + nodeAlias;
                // Backup of checking unique names
                bool checkUniqueNames = tree.CheckUniqueNames;

                // Get existing document
                TreeNode existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, UrlParser.CultureCode, true, SystemDocumentTypes.File, null, null, 1, false, null, tree);
                if (existingNode != null)
                {
                    tree.CheckUniqueNames = false;
                }

                // Update content
                DocumentHelper.UpdateDocument(node, tree);

                // Check in the document
                if (node.DocumentCheckedOutAutomatically)
                {
                    WebDAVHelper.CheckInDocument(node, tree);
                }

                if ((existingNode != null) && (node.NodeID != existingNode.NodeID))
                {
                    // Delete existing document
                    DocumentHelper.DeleteDocument(existingNode, tree, false, true);
                }

                tree.CheckUniqueNames = checkUniqueNames;
            }
        }


        /// <summary>
        /// Moves the node to the specified target.
        /// </summary>
        /// <param name="sourceNode">Node to move</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="safeFileName">Safe file name</param>
        private void MoveNode(TreeNode sourceNode, TreeNode targetNode, TreeProvider tree, string safeFileName)
        {
            // Check move permission
            if (!IsUserAuthorizedToMove(sourceNode, targetNode, sourceNode.NodeClassName))
            {
                throw new ApplicationException(string.Format("[ContentResource.MoveNode]: The '{0}' user doesn't have permissions to move the '{1}' document.",
                                                             MembershipContext.AuthenticatedUser.UserName, sourceNode.NodeAliasPath));
            }

            // If node parent ID is already the target ID, do not move it
            if (sourceNode.NodeParentID == targetNode.NodeID)
            {
                return;
            }

            // Get the document to copy
            if (sourceNode.NodeID == targetNode.NodeID)
            {
                throw new ApplicationException(string.Format("[ContentResource.MoveNode]: The '{0}' page cannot be moved to itself.", sourceNode.NodeAliasPath));
            }

            // Check cyclic movement (movement of the node to some of its child nodes)
            if ((targetNode.NodeSiteID == sourceNode.NodeSiteID) && targetNode.NodeAliasPath.StartsWithCSafe(sourceNode.NodeAliasPath + "/", true))
            {
                throw new ApplicationException(string.Format("[ContentResource.MoveNode]: The '{0}' page cannot be moved under it's child node.", sourceNode.NodeAliasPath));
            }

            // Check allowed child classes
            int nodeClassId = ValidationHelper.GetInteger(sourceNode.GetValue("NodeClassID"), 0);
            if (!DocumentHelper.IsDocumentTypeAllowed(targetNode, nodeClassId))
            {
                throw new ApplicationException(string.Format("[ContentResource.MoveNode]: The CMS File cannot be uploaded under the '{0}' document.", sourceNode.NodeAliasPath));
            }

            string nodeAlias = IO.Path.GetFileNameWithoutExtension(safeFileName);
            string aliasPath = targetNode.NodeAliasPath.Substring(targetNode.NodeAliasPath.LastIndexOfCSafe("/")) + "/" + nodeAlias;

            // Get existing document
            var existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, UrlParser.CultureCode, true, SystemDocumentTypes.File, null, null, 1, false, null, tree);
            if (existingNode != null)
            {
                // Get source attachment guid
                Guid sourceAttGuid = ValidationHelper.GetGuid(sourceNode.GetValue("FileAttachment"), Guid.Empty);

                if (sourceAttGuid != Guid.Empty)
                {
                    // Get source attachment
                    var newAttachment = DocumentHelper.GetAttachment(sourceNode, sourceAttGuid);
                    if (newAttachment != null)
                    {
                        FormFieldInfo ffi = WebDAVHelper.GetFormFieldInfo(existingNode, "FileAttachment");

                        // Set height and width of attachment
                        WebDAVHelper.SetAttachmentHeightAndWidth(newAttachment);

                        int width = 0;
                        int height = 0;
                        int maxSideSize = 0;

                        // Set width, height and max side size from settings
                        WebDAVHelper.GetImageResizeValues(ffi, ref width, ref height, ref maxSideSize);

                        // Check out the existing document
                        WebDAVHelper.CheckOutDocument(existingNode, tree);

                        // Add attachment from source document to existing document
                        newAttachment = DocumentHelper.AddAttachment(existingNode, "FileAttachment", newAttachment.AttachmentGUID, Guid.Empty, newAttachment, width, height, maxSideSize);

                        existingNode.SetValue("FileAttachment", newAttachment.AttachmentGUID);
                        // Update the document
                        DocumentHelper.UpdateDocument(existingNode, tree, ffi.Name);

                        // Check in the document
                        if (existingNode.DocumentCheckedOutAutomatically)
                        {
                            WebDAVHelper.CheckInDocument(existingNode, tree);
                        }

                        // Delete source node
                        DeleteContent(true);
                    }
                }
            }
            else
            {
                // Move the document
                DocumentHelper.MoveDocument(sourceNode, targetNode, tree);
            }
        }


        /// <summary>
        /// Moves this item to the destination attachment folder under a new name.
        /// </summary>
        /// <param name="attachmnetFolder">Destination attachment folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveContentToAttachmentFolder(AttachmentFolder attachmnetFolder, string destName)
        {
            try
            {
                TreeNode destNode = attachmnetFolder.UrlParser.Node;
                UrlParser destParser = attachmnetFolder.UrlParser;
                TreeNode sourceNode = UrlParser.Node;

                if (CurrentUserCanMove(sourceNode, destNode, destParser))
                {
                    TreeProvider tree = destNode.TreeProvider;

                    if (destParser.ItemType == ItemTypeEnum.AttachmentUnsortedFolder)
                    {
                        // Move CMS File to unsorted folder
                        MoveToAttachmentFolder(destNode, tree, destName, null, destParser);
                    }   
                    else if (destParser.ItemType == ItemTypeEnum.AttachmentFieldNameFolder)
                    {
                        FormFieldInfo destFfi = WebDAVHelper.GetFormFieldInfo(destNode, destParser.FieldName);

                        if (destFfi != null)
                        {
                            // Check field 'File' and source node and destination node must be different
                            // or move CMS File to grouped folder
                            if (((destFfi.DataType == FieldDataType.File) && (sourceNode.DocumentID != destNode.DocumentID))
                                || (destFfi.DataType == DocumentFieldDataType.DocAttachments))
                            {
                                MoveToAttachmentFolder(destNode, tree, destName, destFfi, destParser);
                            }
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
                EventLogProvider.LogException("ContentResource_MoveContentToAttachmentFolder", "WebDAV", ex);
                return new ConflictResponse();
            }

            return new CreatedResponse();
        }


        private bool CurrentUserCanMove(TreeNode sourceNode, TreeNode destNode, UrlParser destParser)
        {
            // Check 'Modify' and 'Destroy' permissions on the CMS File or group administrator and 'Modify' permission on the destination node or group administrator
            // and check if documents are checked out by another user
            return ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify)
                     && WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Destroy)) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                   && (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(destNode, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(destParser.Group))
                   && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(sourceNode) && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(destNode)
                   && WebDAVHelper.CanCurrentUserApproveCustomStep(destNode);
        }


        /// <summary>
        /// Moves content to attachment folder. 
        /// </summary>
        /// <param name="destNode">Destination document</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="destName">Destination name</param>
        /// <param name="ffi">Form field info</param>
        /// <param name="destParser">Destination URL parser</param>
        private void MoveToAttachmentFolder(TreeNode destNode, TreeProvider tree, string destName, FormFieldInfo ffi, UrlParser destParser)
        {
            // Get existing or new attachment info
            var newAttachment = WebDAVHelper.GetExistingOrNewAttachment(destNode, UrlParser, destParser, destName);
            if (newAttachment != null)
            {
                // Ensure binary data
                EnsureBinaryData(newAttachment);

                // Set height and width of attachment
                WebDAVHelper.SetAttachmentHeightAndWidth(newAttachment);

                int width = 0;
                int height = 0;
                int maxSideSize = 0;
                bool taskLogged = false;

                // Set width, height and max side size from settings
                WebDAVHelper.GetImageResizeValues(ref width, ref height, ref maxSideSize);

                // Check out document
                bool documentWasCheckedOut = WebDAVHelper.CheckOutDocument(destNode, tree);

                if (ffi == null)
                {
                    newAttachment = DocumentHelper.AddAttachment(destNode, null, newAttachment.AttachmentGUID, Guid.Empty, newAttachment, width, height, maxSideSize);
                    // Log synchronization task
                    WebDAVHelper.LogSynchronization(destNode, TaskTypeEnum.UpdateDocument, tree);
                }
                else if (ffi.DataType == FieldDataType.File)
                {
                    newAttachment = DocumentHelper.AddAttachment(destNode, ffi.Name, newAttachment.AttachmentGUID, Guid.Empty, newAttachment, width, height, maxSideSize);
                    destNode.SetValue(ffi.Name, newAttachment.AttachmentGUID);
                    DocumentHelper.UpdateDocument(destNode, tree);
                    taskLogged = true;
                }
                else if (ffi.DataType == DocumentFieldDataType.DocAttachments)
                {
                    newAttachment = DocumentHelper.AddAttachment(destNode, null, newAttachment.AttachmentGUID, ffi.Guid, newAttachment, width, height, maxSideSize);
                    // Log synchronization task
                    WebDAVHelper.LogSynchronization(destNode, TaskTypeEnum.UpdateDocument, tree);
                }

                TreeNode sourceNode = UrlParser.Node;

                if (sourceNode.DocumentID == destNode.DocumentID)
                {
                    sourceNode.SetValue("FileAttachment", null);
                    sourceNode.Update();
                }
                else
                {
                    // Destroy this CMS File
                    DeleteContent(true);
                }

                // Clean binary data
                WebDAVHelper.CleanBinaryData(newAttachment);

                if (!documentWasCheckedOut)
                {
                    // Check in the document
                    WebDAVHelper.CheckInDocument(destNode, UrlParser.TreeProvider);
                }

                // Log synchronization task
                if (!taskLogged)
                {
                    WebDAVHelper.LogSynchronization(destNode, TaskTypeEnum.UpdateDocument, tree);
                }
            }
        }


        /// <summary>
        /// Moves this item to the media folder.
        /// </summary>
        /// <param name="mediaFolder">Destination media folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse MoveContentToMediaFolder(MediaFolder mediaFolder, string destName)
        {
            try
            {
                UrlParser destParser = mediaFolder.UrlParser;
                TreeNode sourceNode = UrlParser.Node;

                // Check authorization to modify and destroy attachment or group administrator and check if document is checked out by another user and check authorization to create media file 
                if ((destParser.ItemType == ItemTypeEnum.MediaLibraryName)
                    && ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify)
                         && WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Destroy)) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(sourceNode)
                    && WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(destParser.MediaLibraryInfo, "filecreate"))
                {
                    // Safe file name
                    string safeFileName = WebDAVHelper.GetSafeFileName(destName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_MEDIA_FILENAME_LENGTH);

                    // Move attachment to media folder
                    if (MoveToMediaFolder(destParser.MediaLibraryInfo, safeFileName, destName, destParser.FilePath))
                    {
                        // Destroy cms file
                        DeleteContent(true);
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
                EventLogProvider.LogException("ContentResource_MoveContentToMediaFolder", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new CreatedResponse();
        }


        /// <summary>
        /// Determines whether current user is authorized to move document.
        /// </summary>
        /// <param name="sourceNode">Source node</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="sourceNodeClassName">Source node class name</param>
        /// <returns>True if authorized</returns>
        private static bool IsUserAuthorizedToMove(TreeNode sourceNode, TreeNode targetNode, string sourceNodeClassName)
        {
            bool isAuthorized = false;

            // Check 'Modify' permission to source document
            if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify))
            {
                // Check 'Create' permission to target document
                if (MembershipContext.AuthenticatedUser.IsAuthorizedToCreateNewDocument(targetNode, sourceNodeClassName))
                {
                    isAuthorized = true;
                }
            }

            return isAuthorized;
        }


        /// <summary>
        /// Deletes CMS File.
        /// </summary>
        /// <param name="destroy">Indicates if destroy document</param>
        private WebDAVResponse DeleteContent(bool destroy)
        {
            try
            {
                // Delete or destroy the document
                DocumentHelper.DeleteDocument(UrlParser.Node, UrlParser.TreeProvider, false, destroy);
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContentResource_DeleteContent", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new NoContentResponse();
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
                var attachment = UrlParser.Attachment;
                var node = UrlParser.Node;

                // Check current user is group administrator or check if current user has 'Modify' and size of file isn't zero or 'Create'
                // and size of file is zero and document is not checked out by another user and current user can approve custom step
                if ((WebDAVHelper.IsGroupAdministrator(UrlParser.Group) || (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Modify) && (attachment.AttachmentSize != 0))
                     || (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Create) && (attachment.AttachmentSize == 0)))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(node) && WebDAVHelper.CanCurrentUserApproveCustomStep(node))
                {
                    return new OkResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContentResource_Lock", "WebDAV", ex);
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
                TreeNode node = UrlParser.Node;

                if ((node != null) && (node.DocumentHash != null))
                {
                    // Clear hash
                    node.DocumentHash = null;
                    node.Update();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContentResource_Unlock", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new NoContentResponse();
        }

        #endregion
    }
}
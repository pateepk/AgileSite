using System;
using System.Collections.Generic;
using System.Data;

using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DocumentEngine;
using CMS.WorkflowEngine;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;
using CMS.DataEngine;

namespace CMS.WebDAV
{
    /// <summary>
    /// Class that represents content folder.
    /// </summary>
    internal class ContentFolder : BaseFolder, IFolder
    {
        #region "Constants"

        private const string NODE_COLUMNS = "NodeAlias, NodeParentID, NodeAliasPath, NodeClassID, NodeLinkedNodeID, NodeACLID, NodeSiteID, NodeOwner, DocumentCreatedWhen, DocumentModifiedWhen, DocumentID, DocumentType, DocumentCheckedOutVersionHistoryID, DocumentWorkflowStepID, DocumentCulture, ClassName";
        private const string NODE_COLUMNS_CMS_FILE = NODE_COLUMNS + ", FileAttachment";
        private const string ATTACHMENT_COLUMNS = "AttachmentName, AttachmentMimeType, AttachmentSize, AttachmentLastModified";

        #endregion


        #region "Contructors"

        /// <summary>
        /// Creates content folder.
        /// </summary>
        /// <param name="path">URL path</param>
        /// <param name="name">Folder name</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">Url parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent folder</param>
        public ContentFolder(string path, string name, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, created, modified, urlParser, engine, parent)
        {
        }

        #endregion


        #region "IFolder Members"

        /// <summary>
        /// Gets the array of this folder's children.
        /// </summary>
        public IHierarchyItem[] Children
        {
            get
            {
                List<IHierarchyItem> childrenItems = new List<IHierarchyItem>();

                if ((UrlParser.ItemType == ItemTypeEnum.ContentCultureCodeFolder) || (UrlParser.ItemType == ItemTypeEnum.ContentAliasPathFolder))
                {
                    // Get current node
                    TreeNode node = UrlParser.Node;

                    // Parent node
                    if (node != null)
                    {
                        try
                        {
                            GeneralizedInfo group = UrlParser.Group;

                            // Check 'ExploreTree' permission or group administrator
                            if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.ExploreTree) || WebDAVHelper.IsGroupAdministrator(group))
                            {
                                TreeProvider tree = node.TreeProvider;
                                string subAliasPath = node.NodeAliasPath.TrimEnd('/') + "/%";

                                // Get content documents
                                DataSet dsFiles = DocumentHelper.GetDocuments(SiteContext.CurrentSiteName, subAliasPath, UrlParser.CultureCode, false, SystemDocumentTypes.File, "NodeID <> " + node.NodeID, null, 1, false, 0, NODE_COLUMNS_CMS_FILE, tree);

                                if (!DataHelper.DataSourceIsEmpty(dsFiles))
                                {
                                    // Filter if user isn't group administrator
                                    if ((group == null) || !WebDAVHelper.IsGroupAdministrator(group))
                                    {
                                        // Filter 'Read' permission
                                        dsFiles = TreeSecurityProvider.FilterDataSetByPermissions(dsFiles, NodePermissionsEnum.Read, MembershipContext.AuthenticatedUser);
                                    }

                                    if (!DataHelper.DataSourceIsEmpty(dsFiles))
                                    {
                                        foreach (DataRow row in dsFiles.Tables[0].Rows)
                                        {
                                            TreeNode childNode = TreeNode.New(SystemDocumentTypes.File, row);

                                            // Get guid
                                            Guid guid = ValidationHelper.GetGuid(childNode.GetValue("FileAttachment"), Guid.Empty);
                                            if (guid != Guid.Empty)
                                            {
                                                var attDataSet = 
                                                    DocumentHelper.GetAttachments(childNode, false)
                                                        .ApplySettings(settings => settings
                                                            .Columns(ATTACHMENT_COLUMNS)
                                                            .WhereEquals("AttachmentGUID", guid)
                                                            .TopN(1)
                                                        )
                                                        .Result;

                                                // Check if dataset contains attachment
                                                if (!DataHelper.DataSourceIsEmpty(attDataSet))
                                                {
                                                    // Parameters from document
                                                    string extension = ValidationHelper.GetString(row["DocumentType"], string.Empty);
                                                    string nodeAlias = ValidationHelper.GetString(row["NodeAlias"], string.Empty);

                                                    DataRow attRow = attDataSet.Tables[0].Rows[0];

                                                    // Paremeters from attachment
                                                    DateTime modified = ValidationHelper.GetDateTime(attRow["AttachmentLastModified"], DateTime.Now);
                                                    string mimeType = ValidationHelper.GetString(attRow["AttachmentMimeType"], string.Empty);
                                                    long size = ValidationHelper.GetLong(attRow["AttachmentSize"], 0);

                                                    string name = nodeAlias + extension;
                                                    string path = String.Format("{0}/{1}", Path, name);

                                                    // Create content resource
                                                    ContentResource contentResource = new ContentResource(path, name, mimeType, size, modified, modified, null, Engine, this);
                                                    childrenItems.Add(contentResource);
                                                }
                                            }
                                        }
                                    }
                                }

                                string whereCondition = "NodeID <> " + node.NodeID;
                                if (!AllowedChildClassInfoProvider.CMSFileHasChildClass)
                                {
                                    // Do not include CMS.Files as folders
                                    whereCondition = SqlHelper.AddWhereCondition(whereCondition, string.Format("ClassName <> '{0}'", SystemDocumentTypes.File));
                                }

                                // Get sub nodes
                                DataSet dsChildren = tree.SelectNodes(SiteContext.CurrentSiteName, subAliasPath, UrlParser.CultureCode, false, null, whereCondition, null, 1, false, 0, NODE_COLUMNS);

                                if (!DataHelper.DataSourceIsEmpty(dsChildren))
                                {
                                    if (!DataHelper.DataSourceIsEmpty(dsChildren))
                                    {
                                        foreach (DataRow row in dsChildren.Tables[0].Rows)
                                        {
                                            string name = ValidationHelper.GetString(row["NodeAlias"], null);
                                            string path = String.Format("{0}/{1}", Path, name);

                                            DateTime created = ValidationHelper.GetDateTime(row["DocumentCreatedWhen"], DateTime.Now);
                                            DateTime modified = ValidationHelper.GetDateTime(row["DocumentModifiedWhen"], DateTime.Now);

                                            ContentFolder contentFolder = new ContentFolder(path, name, created, modified, null, Engine, this);
                                            childrenItems.Add(contentFolder);
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            EventLogProvider.LogException("ContentFolder_Children", "WebDAV", ex);
                        }
                    }
                }
                return childrenItems.ToArray();
            }
        }


        /// <summary>
        /// Creates new WebDAV resource with the specified name in this folder. 
        /// </summary>
        /// <param name="resourceName">Name of the resource to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateResource(string resourceName)
        {
            TreeNode newNode = null;
            bool newDocumentCreated = false;

            try
            {
                TreeNode parentNode = UrlParser.Node;

                // Check parent node
                if (parentNode == null)
                {
                    throw new ApplicationException("[ContentFolder.CreateResource]: The parent node wasn't selected.");
                }

                GeneralizedInfo group = UrlParser.Group;

                // Check 'Create' permission or is group administrator
                if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(parentNode, NodePermissionsEnum.Create) || WebDAVHelper.IsGroupAdministrator(group))
                {
                    string method = Engine.Request.HttpMethod.ToUpperCSafe();
                    if (method.EqualsCSafe("PUT"))
                    {
                        return CreateCMSFile(newNode, parentNode, group, resourceName, ref newDocumentCreated);
                    }
                    else
                    {
                        return CreateCMSFolder(newNode, parentNode, group, resourceName, ref newDocumentCreated);
                    }
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (ApplicationException appEx)
            {
                EventLogProvider.LogEvent(EventType.WARNING, "ContentFolder_CreateResource",
                             "WebDAV", appEx.Message, null, MembershipContext.AuthenticatedUser.UserID, MembershipContext.AuthenticatedUser.UserName, 0, null, null, SiteContext.CurrentSiteID);
            
                return new ServerErrorResponse();
            }
            catch (Exception ex)
            {
                // Delete the document if something failed
                if (newDocumentCreated && (newNode != null) && (newNode.DocumentID > 0))
                {
                    DocumentHelper.DeleteDocument(newNode, UrlParser.TreeProvider, false, true);
                }

                EventLogProvider.LogException("ContentFolder_CreateResource", "WebDAV", ex);
                return new ServerErrorResponse();
            }
        }



        /// <summary>
        /// Creates file with the specified name in this folder. 
        /// </summary>
        /// <param name="newNode">New tree node</param>
        /// <param name="parentNode">Parent tree node</param>
        /// <param name="group">Group info. Null if not under a group page.</param>
        /// <param name="resourceName">Name of the file to create</param>
        /// <param name="newDocumentCreated">Indicates if document was created.</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse CreateCMSFile(TreeNode newNode, TreeNode parentNode, GeneralizedInfo group, string resourceName, ref bool newDocumentCreated)
        {
            FormFieldInfo ffi;
            TreeProvider tree = UrlParser.TreeProvider;
            TreeNode existingNode;

            string fileName = IO.Path.GetFileNameWithoutExtension(resourceName);
            string safeFileName = WebDAVHelper.GetSafeFileName(fileName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
            string culture = UrlParser.CultureCode;
            string extension = IO.Path.GetExtension(resourceName);
            string where = String.Format("DocumentType = N'{0}'", SqlHelper.GetSafeQueryString(extension, false));

            // Check if CMS file is temporary
            if (safeFileName.StartsWithCSafe(WebDAVHelper.TEMPORARY_FILE_PREFIX))
            {
                // Node alias path of file without prefix
                string newAliasPath = String.Format("{0}/{1}", parentNode.NodeAliasPath, safeFileName.Remove(0, WebDAVHelper.TEMPORARY_FILE_PREFIX.Length));
                existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, newAliasPath, culture, true, SystemDocumentTypes.File, where, null, 1, false, null, tree);

                if (existingNode != null)
                {
                    // Temporary editing file
                    return new CreatedResponse();
                }
            }

            string fileNameWithExtension = safeFileName + extension;
            string aliasPath = String.Format("{0}/{1}", parentNode.NodeAliasPath, safeFileName);

            // Get existing document. If document doesn't exist, get document with default culture.
            existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, culture, true, SystemDocumentTypes.File, where, null, 1, false, null, tree);

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
                newNode = WebDAVHelper.GetNewContent(fileNameWithExtension, parentNode, tree, culture, out ffi);
                string path = String.Format("{0}/{1}", Path.TrimEnd('/'), resourceName);
                newNode.DocumentHash = WebDAVHelper.GetResourceHash(path);
                newNode.SetValue("DocumentType", extension);
                // Insert document
                DocumentHelper.InsertDocument(newNode, parentNode, tree);
                newDocumentCreated = true;
            }

            // Create attachment info
            var attachmentInfo = new DocumentAttachment
            {
                AttachmentName = WebDAVHelper.GetSafeFileName(fileNameWithExtension, SiteContext.CurrentSiteName, AttachmentInfoProvider.MAXATTACHMENTNAMELENGTH),
                AttachmentExtension = extension,
                AttachmentSize = 0,
                AttachmentMimeType = MimeTypeHelper.GetMimetype(extension),
                AttachmentFormGUID = newNode.NodeGUID,
                AttachmentSiteID = newNode.OriginalNodeSiteID
            };

            // Add temporary attachment
            AttachmentInfo newAttachment = AttachmentInfoProvider.AddTemporaryAttachment(newNode.NodeGUID, ffi.Name, attachmentInfo.AttachmentGUID, Guid.Empty, attachmentInfo, SiteContext.CurrentSiteID, 0, 0, 0);

            // Update the document
            newNode.SetValue(ffi.Name, newAttachment.AttachmentGUID);

            // Set Group ID
            if (group != null)
            {
                newNode.SetValue("NodeGroupID", ValidationHelper.GetInteger(group.GetValue("GroupID"), 0));
            }

            // Indicate automatic checkout
            WorkflowManager wm = WorkflowManager.GetInstance(tree);
            WorkflowInfo wi = wm.GetNodeWorkflow(newNode);
            if (wi != null)
            {
                bool useCheckInCheckOut = wi.UseCheckInCheckOut(SiteContext.CurrentSiteName);
                if (useCheckInCheckOut)
                {
                    newNode.SetValue("DocumentCheckedOutAutomatically", true);
                }
            }

            DocumentHelper.UpdateDocument(newNode, tree, ffi.Name);

            return new CreatedResponse();
        }


        /// <summary>
        /// Creates folder with the specified name in this folder. 
        /// </summary>
        /// <param name="newNode">New tree node</param>
        /// <param name="parentNode">Parent tree node</param>
        /// <param name="group">Group info. Null if not under a group page.</param>
        /// <param name="resourceName">Name of the file to create</param>
        /// <param name="newDocumentCreated">Indicates if document was created.</param>
        /// <returns>WebDAV response</returns>
        private WebDAVResponse CreateCMSFolder(TreeNode newNode, TreeNode parentNode, GeneralizedInfo group, string resourceName, ref bool newDocumentCreated)
        {
            TreeProvider tree = UrlParser.TreeProvider;

            string folderName = IO.Path.GetFileNameWithoutExtension(resourceName);
            string safeFileName = WebDAVHelper.GetSafeFileName(folderName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
            string culture = UrlParser.CultureCode;
            string aliasPath = String.Format("{0}/{1}", parentNode.NodeAliasPath, safeFileName);

            // Get existing document. If document doesn't exist, get document with default culture.
            TreeNode existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, culture, true, SystemDocumentTypes.Folder, null, null, 1, false, null, tree);

            if (existingNode != null)
            {
                newNode = existingNode;

                // Existing document is default
                if (newNode.DocumentCulture.ToLowerCSafe() != culture.ToLowerCSafe())
                {
                    // Insert new culture
                    DocumentHelper.InsertNewCultureVersion(newNode, tree, culture);
                }
            }
            else
            {
                // Create new node - content
                newNode = WebDAVHelper.GetNewContentFolder(folderName, parentNode, tree, culture);

                // Insert document
                DocumentHelper.InsertDocument(newNode, parentNode, tree);
                newDocumentCreated = true;
            }

            // Set Group ID
            if (group != null)
            {
                newNode.SetValue("NodeGroupID", ValidationHelper.GetInteger(group.GetValue("GroupID"), 0));
            }

            // Indicate automatic checkout
            WorkflowManager wm = WorkflowManager.GetInstance(tree);
            WorkflowInfo wi = wm.GetNodeWorkflow(newNode);
            if (wi != null)
            {
                bool useCheckInCheckOut = wi.UseCheckInCheckOut(SiteContext.CurrentSiteName);
                if (useCheckInCheckOut)
                {
                    newNode.SetValue("DocumentCheckedOutAutomatically", true);
                }
            }

            DocumentHelper.UpdateDocument(newNode, tree);

            return new CreatedResponse();
        }


        /// <summary>
        /// Creates new WebDAV folder with the specified name in this folder. 
        /// </summary>
        /// <param name="folderName">Name of the folder to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateFolder(string folderName)
        {
            return CreateResource(folderName);
        }

        #endregion


        #region "IHierarchyItem Members"


        /// <summary>
        /// Creates a copy of this item with a new name in the destination folder.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <param name="deep">Indicates whether to copy entire sub tree</param>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse CopyTo(IFolder folder, string destName, bool deep)
        {
            // Destination document is content folder
            if (folder is ContentFolder)
            {
                string newNodeName = IO.Path.GetFileNameWithoutExtension(destName);

                return CopyContentToContentFolder(folder as ContentFolder, newNodeName);
            }
            // Destination document is attachment folder
            else if (folder is AttachmentFolder)
            {
                return new ForbiddenResponse();
            }
            // Destination document is media folder
            else if (folder is MediaFolder)
            {
                return new ForbiddenResponse();
            }

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
            // Destination document is content folder
            if (folder is ContentFolder)
            {
                string newNodeName = IO.Path.GetFileNameWithoutExtension(destName);

                return MoveContentToContentFolder(folder as ContentFolder, newNodeName);
            }
            // Destination document is attachment folder
            else if (folder is AttachmentFolder)
            {
                return new ForbiddenResponse();
            }
            // Destination document is media folder
            else if (folder is MediaFolder)
            {
                return new ForbiddenResponse();
            }

            return new NoContentResponse();
        }


        /// <summary>
        /// Deletes this folder.
        /// </summary>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse Delete()
        {
            return DeleteFolder(false);
        }


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
                EventLogProvider.LogException("ContentFolder_Lock", "WebDAV", ex);
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
                EventLogProvider.LogException("ContentFolder_Unlock", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new NoContentResponse();
        }


        #endregion


        #region "Private methods"
        
        /// <summary>
        /// Deletes folder document.
        /// </summary>
        /// <param name="destroy">Indicates if folder document should be destroyed instead of moving to recycle bin.</param>
        private WebDAVResponse DeleteFolder(bool destroy)
        {
            TreeNode node = UrlParser.Node;

            // Check if document is CMS Folder
            bool isCMSFolder = node.IsFolder();

            // Check 'Delete' permission or is group administrator and check if document is checked out by another user
            if (isCMSFolder && (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Delete) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group)) && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(node))
            {
                try
                {
                    // Delete document
                    DocumentHelper.DeleteDocument(node, UrlParser.TreeProvider, false, destroy);
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("ContentFolder_Delete", "WebDAV", ex);
                    return new ServerErrorResponse();
                }
            }
            else
            {
                return new AccessDeniedResponse();
            }

            return new NoContentResponse();
        }

        /// <summary>
        /// Determines whether current user is authorized to move document.
        /// </summary>
        /// <param name="sourceNode">Source node</param>
        /// <param name="targetNode">Target node</param>
        /// <param name="sourceNodeClassName">Source node class name</param>
        /// <returns>True if authorized</returns>
        private static bool IsAuthorizedToCreateNewDocument(TreeNode sourceNode, TreeNode targetNode, string sourceNodeClassName)
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
        /// Moves this item to the destination content folder under a new name.
        /// </summary>
        /// <param name="contentFolder">Destination content folder</param>
        /// <param name="newNodeName">New node name</param>
        private WebDAVResponse CopyContentToContentFolder(ContentFolder contentFolder, string newNodeName)
        {
            try
            {
                TreeNode sourceNode = UrlParser.Node;

                if ((sourceNode != null) && (contentFolder.UrlParser.Node != null))
                {
                    TreeNode targetNode = contentFolder.UrlParser.Node;

                    // Check 'Modify' permission or group administrator and check if document is checked out by another user
                    if ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(sourceNode, NodePermissionsEnum.Modify) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                        && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(sourceNode))
                    {
                        string safeFileName = WebDAVHelper.GetSafeFileName(newNodeName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);

                        // Move content to other node
                        CopyNode(sourceNode, targetNode, UrlParser.TreeProvider, safeFileName);
                    }

                    return new CreatedResponse();
                }
            }
            catch (ApplicationException appEx)
            {
                EventLogProvider.LogEvent(EventType.WARNING, "ContentResource_CopyContentToContentFolder",
                             "WebDAV", appEx.Message, null, MembershipContext.AuthenticatedUser.UserID, MembershipContext.AuthenticatedUser.UserName, 0, null, null, SiteContext.CurrentSiteID);

                return new ConflictResponse();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("ContentResource_CopyContentToContentFolder", "WebDAV", ex);
                return new ConflictResponse();
            }

            return new NoContentResponse();
        }


        /// <summary>
        /// Moves this item to the destination content folder under a new name.
        /// </summary>
        /// <param name="contentFolder">Destination content folder</param>
        /// <param name="newNodeName">New node name</param>
        private WebDAVResponse MoveContentToContentFolder(ContentFolder contentFolder, string newNodeName)
        {
            try
            {
                TreeNode sourceNode = UrlParser.Node;

                // Check allowed extension
                if ((sourceNode != null) && (contentFolder.UrlParser.Node != null))
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
                            RenameDocument(sourceNode, newNodeName);
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
        private void RenameDocument(TreeNode node, string newName)
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

                string aliasPath = String.Format("{0}/{1}", node.NodeAliasPath.Substring(0, node.NodeAliasPath.LastIndexOfCSafe("/")), nodeAlias);
                // Backup of checking unique names
                bool checkUniqueNames = tree.CheckUniqueNames;

                // Get existing document
                TreeNode existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, UrlParser.CultureCode, true, SystemDocumentTypes.Folder, null, null, 1, false, null, tree);
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
        private void CopyNode(TreeNode sourceNode, TreeNode targetNode, TreeProvider tree, string safeFileName)
        {
            // Check permissions
            if (!IsAuthorizedToCreateNewDocument(sourceNode, targetNode, sourceNode.NodeClassName))
            {
                throw new ApplicationException(string.Format("[ContentFolder.CopyNode]: The '{0}' user doesn't have permissions to copy the '{1}' folder.",
                                                             MembershipContext.AuthenticatedUser.UserName, sourceNode.NodeAliasPath));
            }

            // Check cyclic movement (movement of the node to some of its child nodes)
            if ((targetNode.NodeSiteID == sourceNode.NodeSiteID) && targetNode.NodeAliasPath.StartsWithCSafe(sourceNode.NodeAliasPath + "/", true))
            {
                throw new ApplicationException(string.Format("[ContentFolder.CopyNode]: The '{0}' folder cannot be copied under it's child folders.", sourceNode.NodeAliasPath));
            }

            // Check allowed child classes
            int targetClassId = ValidationHelper.GetInteger(targetNode.GetValue("NodeClassID"), 0);
            int nodeClassId = ValidationHelper.GetInteger(sourceNode.GetValue("NodeClassID"), 0);
            if (!AllowedChildClassInfoProvider.IsChildClassAllowed(targetClassId, nodeClassId))
            {
                throw new ApplicationException(string.Format("[ContentFolder.CopyNode]: The CMS older cannot be created under the '{0}' document.", sourceNode.NodeAliasPath));
            }

            string nodeAlias = IO.Path.GetFileNameWithoutExtension(safeFileName);
            string aliasPath = String.Format("{0}/{1}", targetNode.NodeAliasPath.Substring(targetNode.NodeAliasPath.LastIndexOfCSafe("/")), nodeAlias);

            // Get existing document
            TreeNode existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, UrlParser.CultureCode, true, SystemDocumentTypes.Folder, null, null, 1, false, null, tree);
            if (existingNode != null)
            {
                throw new ApplicationException(string.Format("[ContentFolder.CopyNode]: The '{0}' folder already exists.", sourceNode.NodeAliasPath));
            }
            else
            {
                // Copy the document
                DocumentHelper.CopyDocument(sourceNode, targetNode, true, tree);
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
            // Check permissions
            if (!IsAuthorizedToCreateNewDocument(sourceNode, targetNode, sourceNode.NodeClassName))
            {
                throw new ApplicationException(string.Format("[ContentFolder.MoveNode]: The '{0}' user doesn't have permissions to move the '{1}' folder.",
                                                             MembershipContext.AuthenticatedUser.UserName, sourceNode.NodeAliasPath));
            }

            // If node parent ID is already the target ID, do not move it
            if (sourceNode.NodeParentID == targetNode.NodeID)
            {
                return;
            }

            if (sourceNode.NodeID == targetNode.NodeID)
            {
                throw new ApplicationException(string.Format("[ContentFolder.MoveNode]: The '{0}' folder cannot be moved to itself.", sourceNode.NodeAliasPath));
            }

            // Check cyclic movement (movement of the node to some of its child nodes)
            if ((targetNode.NodeSiteID == sourceNode.NodeSiteID) && targetNode.NodeAliasPath.StartsWithCSafe(sourceNode.NodeAliasPath + "/", true))
            {
                throw new ApplicationException(string.Format("[ContentFolder.MoveNode]: The '{0}' folder cannot be moved under it's child folders.", sourceNode.NodeAliasPath));
            }

            // Check allowed child classes
            int targetClassId = ValidationHelper.GetInteger(targetNode.GetValue("NodeClassID"), 0);
            int nodeClassId = ValidationHelper.GetInteger(sourceNode.GetValue("NodeClassID"), 0);
            if (!AllowedChildClassInfoProvider.IsChildClassAllowed(targetClassId, nodeClassId))
            {
                throw new ApplicationException(string.Format("[ContentFolder.MoveNode]: The CMS older cannot be created under the '{0}' document.", sourceNode.NodeAliasPath));
            }

            string nodeAlias = IO.Path.GetFileNameWithoutExtension(safeFileName);
            string aliasPath = String.Format("{0}/{1}", targetNode.NodeAliasPath.Substring(targetNode.NodeAliasPath.LastIndexOfCSafe("/")), nodeAlias);

            // Get existing document
            TreeNode existingNode = DocumentHelper.GetDocument(SiteContext.CurrentSiteName, aliasPath, UrlParser.CultureCode, true, SystemDocumentTypes.Folder, null, null, 1, false, null, tree);
            if (existingNode != null)
            {
                throw new ApplicationException(string.Format("[ContentFolder.MoveNode]: The '{0}' folder already exists.", sourceNode.NodeAliasPath));
            }
            else
            {
                // Move the document
                DocumentHelper.MoveDocument(sourceNode, targetNode, tree);
            }
        }


        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Data;

using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.DocumentEngine;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;
using CMS.DataEngine;
using CMS.Membership;

namespace CMS.WebDAV
{
    /// <summary>
    /// Class that represents the attachment folder.
    /// </summary>
    internal class AttachmentFolder : BaseFolder, IFolder
    {
        #region "Constants"

        private const string NODE_COLUMNS = "NodeAlias, DocumentCreatedWhen, DocumentModifiedWhen";
        private const string ATTACHMENT_COLUMNS = "AttachmentName, AttachmentMimeType, AttachmentSize, AttachmentLastModified";

        #endregion


        #region "Contructors"

        /// <summary>
        /// Creates attachment folder.
        /// </summary>
        /// <param name="path">URL path</param>
        /// <param name="name">Name</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">Url parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent folder</param>
        public AttachmentFolder(string path, string name, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
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

                try
                {
                    TreeNode node = UrlParser.Node;
                    if (node != null)
                    {
                        GeneralizedInfo group = UrlParser.Group;

                        switch (UrlParser.ItemType)
                        {
                                // Culture code folder or document folder
                            case ItemTypeEnum.AttachmentCultureFolder:
                            case ItemTypeEnum.AttachmentAliasPathFolder:
                                {
                                    string subAliasPath = node.NodeAliasPath.TrimEnd('/') + "/%";
                                    ;

                                    // Check permission 'ExploreTree' or is group administrator
                                    if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.ExploreTree) || WebDAVHelper.IsGroupAdministrator(group))
                                    {
                                        TreeProvider tree = UrlParser.TreeProvider;
                                        // Get sub nodes
                                        DataSet subNodes = tree.SelectNodes(SiteContext.CurrentSiteName, subAliasPath, UrlParser.CultureCode, false, null, "NodeID <> " + node.NodeID, null, 1, false, 0, NODE_COLUMNS);

                                        if (!DataHelper.DataSourceIsEmpty(subNodes))
                                        {
                                            foreach (DataRow row in subNodes.Tables[0].Rows)
                                            {
                                                string nodeAlias = ValidationHelper.GetString(row["NodeAlias"], string.Empty);
                                                DateTime created = ValidationHelper.GetDateTime(row["DocumentCreatedWhen"], DateTime.Now);
                                                DateTime modified = ValidationHelper.GetDateTime(row["DocumentModifiedWhen"], DateTime.Now);

                                                // Add sub folders
                                                string folderPath = Path + "/" + nodeAlias;
                                                AttachmentFolder contentFolder = new AttachmentFolder(folderPath, nodeAlias, created, modified, null, Engine, this);
                                                childrenItems.Add(contentFolder);
                                            }
                                        }
                                    }

                                    // Check permission 'Read' or is group administrator
                                    if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Read) || WebDAVHelper.IsGroupAdministrator(group))
                                    {
                                        DateTime created = ValidationHelper.GetDateTime(node.GetValue("DocumentCreatedWhen"), DateTime.Now);
                                        DateTime modified = ValidationHelper.GetDateTime(node.GetValue("DocumentModifiedWhen"), DateTime.Now);

                                        // Add [_unsorted] folder
                                        string unsortedFolderPath = Path + "/" + WebDAVSettings.UnsortedFolder;
                                        AttachmentFolder unsortedFolder = new AttachmentFolder(unsortedFolderPath, WebDAVSettings.UnsortedFolder, created, modified, null, Engine, this);
                                        childrenItems.Add(unsortedFolder);

                                        FormInfo fi = WebDAVHelper.GetFormInfo(node);
                                        if (fi != null)
                                        {
                                            string name = null;
                                            string folderPath = Path + "/";
                                            string path = null;
                                            // Grouped attachments
                                            foreach (FormFieldInfo ffi in fi.GetFields(DocumentFieldDataType.DocAttachments))
                                            {
                                                name = WebDAVHelper.GetFieldNameForUrl(ffi.Name);
                                                path = folderPath + name;

                                                // Add 'grouped folder'
                                                childrenItems.Add(new AttachmentFolder(path, name, created, modified, null, Engine, this));
                                            }
                                            // Field attachments
                                            foreach (FormFieldInfo ffi in fi.GetFields(FieldDataType.File))
                                            {
                                                name = WebDAVHelper.GetFieldNameForUrl(ffi.Name);
                                                path = folderPath + name;

                                                // Add 'field folder'
                                                childrenItems.Add(new AttachmentFolder(path, name, created, modified, null, Engine, this));
                                            }
                                        }
                                    }
                                }
                                break;

                                // '[_unsorted]' folder
                            case ItemTypeEnum.AttachmentUnsortedFolder:
                                {
                                    // Check 'Read' permission of document or is group administrator
                                    if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Read) || WebDAVHelper.IsGroupAdministrator(group))
                                    {
                                        var attDataSet = 
                                            DocumentHelper.GetAttachments(node, false)
                                                .ApplySettings(settings => settings
                                                    .Columns(ATTACHMENT_COLUMNS)
                                                    .WhereTrue("AttachmentIsUnsorted")
                                                ).Result;
                                            
                                        // Add attachment resource to the children list
                                        AddAttachmentResource(childrenItems, attDataSet);
                                    }
                                }
                                break;

                                // Field folder
                            case ItemTypeEnum.AttachmentFieldNameFolder:
                                {
                                    // Check 'Read' permission of document or is group administrator
                                    if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.Read) || WebDAVHelper.IsGroupAdministrator(group))
                                    {
                                        FormInfo fi = WebDAVHelper.GetFormInfo(node);

                                        if (fi != null)
                                        {
                                            FormFieldInfo ffi = fi.GetFormField(UrlParser.FieldName);

                                            if (ffi != null)
                                            {
                                                string where = null;

                                                // Grouped attachment
                                                if (ffi.DataType == DocumentFieldDataType.DocAttachments)
                                                {
                                                    where = "(AttachmentGroupGUID = N'" + ffi.Guid + "') AND (AttachmentIsUnsorted IS NULL)";
                                                }
                                                // File attachment
                                                else if (ffi.DataType == FieldDataType.File)
                                                {
                                                    Guid attachmentGUID = ValidationHelper.GetGuid(node.GetValue(ffi.Name), Guid.Empty);

                                                    // 'File' attachment exists
                                                    if (attachmentGUID != Guid.Empty)
                                                    {
                                                        where = "(AttachmentGroupGUID IS NULL) AND (AttachmentIsUnsorted IS NULL) AND (AttachmentGUID = N'" + attachmentGUID + "')";
                                                    }
                                                }
                                                else
                                                {
                                                    break;
                                                }

                                                if (where != null)
                                                {
                                                    var attDataSet = 
                                                        DocumentHelper.GetAttachments(node, false)
                                                            .ApplySettings(settings => settings
                                                                .Columns(ATTACHMENT_COLUMNS)
                                                                .Where(where)
                                                            )
                                                            .Result;

                                                    // Add attachment resource to the children list
                                                    AddAttachmentResource(childrenItems, attDataSet);
                                                }
                                            }
                                        }
                                    }
                                }
                                break;

                                // Nothing
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    EventLogProvider.LogException("AttachmentFolder_Children", "WebDAV", ex);
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
            try
            {
                TreeNode node = UrlParser.Node;
                // Get the existing document
                if (node == null)
                {
                    throw new ApplicationException("[AttachmentFolder.CreateResource]: The parent node wasn't selected.");
                }

                GeneralizedInfo group = UrlParser.Group;

                // Check 'Read' and 'Modify' permissions and unsorted or field folder or is group administrator and check if document is checked out by another user
                if ((WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, new NodePermissionsEnum[]
                                                                               {
                                                                                   NodePermissionsEnum.Read,
                                                                                   NodePermissionsEnum.Modify
                                                                               }) || WebDAVHelper.IsGroupAdministrator(group))
                    && !WebDAVHelper.IsDocumentCheckedOutByAnotherUser(node) && WebDAVHelper.CanCurrentUserApproveCustomStep(node)
                    && ((UrlParser.ItemType == ItemTypeEnum.AttachmentUnsortedFolder) || (UrlParser.ItemType == ItemTypeEnum.AttachmentFieldNameFolder)))
                {
                    // Get safe file name
                    string fileName = WebDAVHelper.GetSafeFileName(resourceName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
                    string extension = IO.Path.GetExtension(fileName);

                    // Create attachment info
                    var attachmentInfo = new DocumentAttachment
                    {
                        AttachmentName = fileName,
                        AttachmentExtension = extension,
                        AttachmentSize = 0,
                        AttachmentMimeType = MimeTypeHelper.GetMimetype(extension),
                        AttachmentFormGUID = node.NodeGUID,
                        AttachmentSiteID = node.OriginalNodeSiteID
                    };

                    // Set attachment hash
                    string path = Path.TrimEnd('/') + "/" + resourceName;
                    attachmentInfo.AttachmentHash = WebDAVHelper.GetResourceHash(path);

                    // Field attachment
                    if (UrlParser.ItemType == ItemTypeEnum.AttachmentFieldNameFolder)
                    {
                        var ffi = WebDAVHelper.GetFormFieldInfo(node, UrlParser.FieldName);
                        if (ffi != null)
                        {
                            // Check allowed extensions of field or grouped attachment
                            if (!WebDAVHelper.IsExtensionAllowedForBrowseMode(extension, ffi, SiteContext.CurrentSiteName))
                            {
                                return new AccessDeniedResponse();
                            }

                            // File attachment
                            if (ffi.DataType == FieldDataType.File)
                            {
                                DocumentAttachment currentAttachment = null;

                                Guid destGuid = ValidationHelper.GetGuid(node.GetValue(ffi.Name), Guid.Empty);
                                if (destGuid != Guid.Empty)
                                {
                                    // Get current attachmnent without binary data
                                    currentAttachment = DocumentHelper.GetAttachment(destGuid, SiteContext.CurrentSiteName, false);
                                }

                                if (currentAttachment != null)
                                {
                                    // Check if file is temporary
                                    if (WebDAVHelper.IsTemporaryFile(currentAttachment.AttachmentName, fileName))
                                    {
                                        return new CreatedResponse();
                                    }

                                    // Delete existing field 'File' attachment
                                    DocumentHelper.DeleteAttachment(node, ffi.Name);
                                }

                                attachmentInfo.SetValue("AttachmentIsUnsorted", null);

                                AttachmentInfoProvider.AddTemporaryAttachment(node.NodeGUID, ffi.Name, attachmentInfo.AttachmentGUID, Guid.Empty, attachmentInfo, SiteContext.CurrentSiteID, 0, 0, 0);
                            }
                            // Grouped attachment
                            else if (ffi.DataType == DocumentFieldDataType.DocAttachments)
                            {
                                AttachmentInfoProvider.AddTemporaryAttachment(node.NodeGUID, null, attachmentInfo.AttachmentGUID, ffi.Guid, attachmentInfo, SiteContext.CurrentSiteID, 0, 0, 0);
                            }

                            return new NoContentResponse();
                        }
                    }
                    else if (UrlParser.ItemType == ItemTypeEnum.AttachmentUnsortedFolder)
                    {
                        // Check allowed extensions of unsorted attachment
                        if (!WebDAVHelper.IsExtensionAllowedForBrowseMode(extension, SiteContext.CurrentSiteName))
                        {
                            return new AccessDeniedResponse();
                        }

                        AttachmentInfoProvider.AddTemporaryAttachment(node.NodeGUID, null, attachmentInfo.AttachmentGUID, Guid.Empty, attachmentInfo, SiteContext.CurrentSiteID, 0, 0, 0);
                    }
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (ApplicationException appEx)
            {
                EventLogProvider.LogEvent(EventType.WARNING, "AttachmentFolder_CreateResource",
                             "WebDAV", appEx.Message, null, MembershipContext.AuthenticatedUser.UserID, MembershipContext.AuthenticatedUser.UserName, 0, null, null, SiteContext.CurrentSiteID);

                return new NoContentResponse();
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("AttachmentFolder_CreateResource", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new CreatedResponse();
        }


        /// <summary>
        /// Creates new WebDAV folder with the specified name in this folder. 
        /// </summary>
        /// <param name="folderName">Name of the folder to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateFolder(string folderName)
        {
            return new AccessDeniedResponse();
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Add attachment resource to children list.
        /// </summary>
        /// <param name="itemList">Children list</param>
        /// <param name="attDataSet">Dataset attachment info (contains only specified columns)</param>
        private void AddAttachmentResource(List<IHierarchyItem> itemList, DataSet attDataSet)
        {
            if ((itemList != null) && !DataHelper.DataSourceIsEmpty(attDataSet))
            {
                foreach (DataRow row in attDataSet.Tables[0].Rows)
                {
                    // Prepare paramaters
                    string name = ValidationHelper.GetString(row["AttachmentName"], string.Empty);
                    string mimeType = ValidationHelper.GetString(row["AttachmentMimeType"], string.Empty);
                    long size = ValidationHelper.GetLong(row["AttachmentSize"], 0);
                    DateTime modified = ValidationHelper.GetDateTime(row["AttachmentLastModified"], DateTime.Now);

                    string path = Path + "/" + name;

                    AttachmentResource attachmentResource = new AttachmentResource(path, name, mimeType, size, modified, modified, null, Engine, this);
                    itemList.Add(attachmentResource);
                }
            }
        }

        #endregion
    }
}
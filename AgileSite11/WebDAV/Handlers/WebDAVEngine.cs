using System;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.DocumentEngine;
using CMS.DataEngine;

using ITHit.WebDAV.Server;

namespace CMS.WebDAV
{
    /// <summary>
    /// CMS WebDav engine.
    /// </summary>
    public class WebDAVEngine : Engine
    {
        #region "Variables"

        private UrlParser mUrlParser = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets URL parser.
        /// </summary>
        public UrlParser UrlParser
        {
            get
            {
                return mUrlParser;
            }
        }

        #endregion


        #region "Public Methods"        

        /// <summary>
        /// Gets hierarchy item objects by path.
        /// </summary>
        /// <param name="path">URL path</param>
        public override IHierarchyItem GetHierarchyItem(string path)
        {
            try
            {
                // Parse URL
                mUrlParser = new UrlParser(path);

                if (UrlParser != null)
                {
                    switch (UrlParser.ItemType)
                    {
                            // Root folder
                        case ItemTypeEnum.Root:
                            return GetFolder(path, WebDAVSettings.RootFolder);

                            // Groups folder
                        case ItemTypeEnum.Groups:
                            return GetFolder(path, WebDAVSettings.GroupsFolder);

                            // Group name folder
                        case ItemTypeEnum.GroupName:
                            return GetGroup(path);

                            // Attachment folder
                        case ItemTypeEnum.AttachmentFolder:
                        case ItemTypeEnum.AttachmentCultureFolder:
                        case ItemTypeEnum.AttachmentAliasPathFolder:
                        case ItemTypeEnum.AttachmentFieldNameFolder:
                        case ItemTypeEnum.AttachmentUnsortedFolder:
                            return GetAttachmentFolder(path);

                            // Attachment file
                        case ItemTypeEnum.UnsortedAttachmentFile:
                        case ItemTypeEnum.FieldAttachmentFile:
                            return GetAttachmentFile(path);

                            // Media folder
                        case ItemTypeEnum.MediaFolder:
                        case ItemTypeEnum.MediaLibraryName:
                            return GetMediaFolder(path);

                            // Media file
                        case ItemTypeEnum.MediaFile:
                            return GetMediaFile(path);

                            // Meta file
                        case ItemTypeEnum.MetaFile:
                        case ItemTypeEnum.MetaFileFolder:
                            return GetMetaFile(path);

                            // Content folder
                        case ItemTypeEnum.ContentFolder:
                        case ItemTypeEnum.ContentCultureCodeFolder:
                        case ItemTypeEnum.ContentAliasPathFolder:
                            return GetContentFolder(path);

                            // Content file
                        case ItemTypeEnum.ContentFile:
                            return GetContentFile(path);

                            // Not supported 'Community' module
                        case ItemTypeEnum.NotSupportedGroups:
                            {
                                const string eventCode = "WebDAVEngine_GetHierarchyItem";
                                const string eventDescription = "The 'Community' module is not loaded.";
                                EventLogProvider.LogEvent(EventType.WARNING, "WebDAV", eventCode, eventDescription, path);
                            }
                            break;

                            // Not supported 'MediaLibrary' module
                        case ItemTypeEnum.NotSupportedMediaLibrary:
                            {
                                const string eventCode = "WebDAVEngine_GetHierarchyItem";
                                const string eventDescription = "The 'MediaLibrary' module is not loaded.";
                                EventLogProvider.LogEvent(EventType.WARNING, "WebDAV", eventCode, eventDescription, path);
                            }
                            break;

                            // Not supported request
                        case ItemTypeEnum.NotSupported:
                            {
                                const string eventCode = "WebDAVEngine_GetHierarchyItem";
                                const string eventDescription = "Not supported request.";
                                EventLogProvider.LogEvent(EventType.ERROR, "WebDAV", eventCode, eventDescription, path);
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
                EventLogProvider.LogException("WebDAVEngine_GetHierarchyItem", "WebDAV", ex);
            }

            return null;
        }


        /// <summary>
        /// Gets hierarchy item objects by path.
        /// </summary>
        /// <param name="path">URL path</param>
        /// <param name="name">Name of new hierarchy item</param>
        /// <param name="outName">Name of new hierarchy item in allowed format</param>
        /// <returns></returns>
        public override IHierarchyItem GetHierarchyItemAndName(string path, string name, out string outName)
        {
            outName = null;

            // Call GetHierarchyItem so that UrlParser is initialized correctly.
            IHierarchyItem item = GetHierarchyItem(path);

            outName = UrlParser.GetAllowedName(name);

            return item;
        }

        #endregion


        #region "Private Methods"

        /// <summary>
        /// Gets attachment folder.
        /// </summary>
        /// <param name="path">Path of attachment folder</param>
        private IHierarchyItem GetAttachmentFolder(string path)
        {
            try
            {
                switch (UrlParser.ItemType)
                {
                        // 'Attachments' folder
                    case ItemTypeEnum.AttachmentFolder:
                        Folder contentFolder = new Folder(path, WebDAVSettings.AttachmentsFolder, DateTime.Now, DateTime.Now, UrlParser, this, null);
                        return contentFolder;

                        // Culture code folder
                    case ItemTypeEnum.AttachmentCultureFolder:
                        if (WebDAVHelper.CulturesCodes.Contains(UrlParser.CultureCode))
                        {
                            AttachmentFolder attachmentFolder = new AttachmentFolder(path, UrlParser.CultureCode, DateTime.Now, DateTime.Now, UrlParser, this, null);
                            return attachmentFolder;
                        }
                        break;

                        // Folder represents node
                    case ItemTypeEnum.AttachmentAliasPathFolder:
                        TreeNode node = UrlParser.Node;
                        if (node != null)
                        {
                            return GetAttachmentFolder(node.NodeAlias, path, NodePermissionsEnum.ExploreTree);
                        }
                        break;

                        // '[_unsorted]' folder
                    case ItemTypeEnum.AttachmentUnsortedFolder:
                        return GetAttachmentFolder(WebDAVSettings.UnsortedFolder, path, NodePermissionsEnum.Read);

                        // Folder of field name
                    case ItemTypeEnum.AttachmentFieldNameFolder:
                        string name = WebDAVHelper.GetFieldNameForUrl(UrlParser.FieldName);
                        return GetAttachmentFolder(name, path, NodePermissionsEnum.Read);

                        // Nothing
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("WebDAVEngine_GetAttachmentFolder", "WebDAV", ex);
            }

            return null;
        }


        /// <summary>
        /// Gets attachment file.
        /// </summary>
        /// <param name="path">Path of attachment file</param>
        private IHierarchyItem GetAttachmentFile(string path)
        {
            try
            {
                var attachment = UrlParser.Attachment;
                if (attachment != null)
                {
                    DateTime modifiedData = attachment.AttachmentLastModified;
                    // Create attachment info
                    AttachmentResource attachmentResource = new AttachmentResource(path, attachment.AttachmentName, attachment.AttachmentMimeType, attachment.AttachmentSize, modifiedData, modifiedData, UrlParser, this, null);
                    return attachmentResource;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("WebDAVEngine_GetAttachmentFile", "WebDAV", ex);
            }

            return null;
        }


        /// <summary>
        /// Gets media folder.
        /// </summary>
        /// <param name="path">URL media folder path</param>
        private IHierarchyItem GetMediaFolder(string path)
        {
            try
            {
                switch (UrlParser.ItemType)
                {
                        // 'Media' folder
                    case ItemTypeEnum.MediaFolder:
                        {
                            Folder mediaFolder = new Folder(path, WebDAVSettings.MediaFilesFolder, DateTime.Now, DateTime.Now, UrlParser, this, null);
                            return mediaFolder;
                        }

                        // Folder of media library name
                    case ItemTypeEnum.MediaLibraryName:
                        {
                            // Check authorization to read or manage
                            if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(UrlParser.MediaLibraryInfo, new string[] { "read", "manage" }))
                            {
                                DirectoryInfo di = WebDAVHelper.GetMediaDirectoryInfo(UrlParser.MediaLibraryInfo, UrlParser.PhysicalFilePath);
                                if (di != null)
                                {
                                    MediaFolder mediaFolder = new MediaFolder(path, UrlParser.LibraryName, di.CreationTime, di.LastWriteTime, UrlParser, this, null);
                                    return mediaFolder;
                                }
                            }
                        }
                        break;

                        // Nothing
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("WebDAVEngine_GetMediaFolder", "WebDAV", ex);
            }

            return null;
        }


        /// <summary>
        /// Gets media file.
        /// </summary>
        /// <param name="path">Path of media file</param>
        private IHierarchyItem GetMediaFile(string path)
        {
            try
            {
                // Media file is in DB
                MediaFileInfo mfi = UrlParser.MediaFileInfo;
                if (mfi != null)
                {
                    // Create media file resource
                    MediaFileResource mediaFileResource = new MediaFileResource(path, mfi.FileName, mfi.FileMimeType, mfi.FileSize, mfi.FileCreatedWhen, mfi.FileModifiedWhen, UrlParser, this, null);
                    return mediaFileResource;
                }

                // Gets file info from file system
                FileInfo fi = UrlParser.FileInfo;
                if (fi != null)
                {
                    string mimeType = MimeTypeHelper.GetMimetype(fi.Extension);

                    // Create media file resource
                    MediaFileResource mediaFileResource = new MediaFileResource(path, fi.Name, mimeType, fi.Length, fi.CreationTime, fi.LastWriteTime, UrlParser, this, null);
                    return mediaFileResource;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("WebDAVEngine_GetMediaFile", "WebDAV", ex);
            }

            return null;
        }


        /// <summary>
        /// Gets meta file resource.
        /// </summary>
        /// <param name="path">Path of meta file resource</param>
        /// <returns>Meta file</returns>
        private IHierarchyItem GetMetaFile(string path)
        {
            try
            {
                MetaFileInfo mfi = UrlParser.MetaFileInfo;

                if (mfi != null)
                {
                    DateTime modified = mfi.MetaFileLastModified;
                    // Create metafile resource
                    MetaFileResource metaFileResource = new MetaFileResource(path, mfi.MetaFileName, mfi.MetaFileMimeType, mfi.MetaFileSize, modified, modified, UrlParser, this, null);
                    return metaFileResource;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("WebDAVEngine_GetMetaFile", "WebDAV", ex);
            }

            // If path doesn't contain filename, returns empty item (for parent url, for example: 'cms/files/C12A7328-F81F-11D2-BA4B-00A0C93EC93B').
            // It's necessary for correct functionality.
            EmptyHierarchyItem emptyItem = new EmptyHierarchyItem(path, null, DateTime.Now, DateTime.Now, UrlParser, this, null);
            return emptyItem;
        }


        /// <summary>
        /// Gets content folder.
        /// </summary>
        /// <param name="path">Path of content folder</param>
        private IHierarchyItem GetContentFolder(string path)
        {
            try
            {
                switch (UrlParser.ItemType)
                {
                        // 'Content' folder
                    case ItemTypeEnum.ContentFolder:
                        Folder contentFolder = new Folder(path, WebDAVSettings.ContentFilesFolder, DateTime.Now, DateTime.Now, UrlParser, this, null);
                        return contentFolder;

                        // Culture code folder
                    case ItemTypeEnum.ContentCultureCodeFolder:
                        if (WebDAVHelper.CulturesCodes.Contains(UrlParser.CultureCode))
                        {
                            ContentFolder culturetFolder = new ContentFolder(path, UrlParser.CultureCode, DateTime.Now, DateTime.Now, UrlParser, this, null);
                            return culturetFolder;
                        }
                        break;

                        // Folder of content
                    case ItemTypeEnum.ContentAliasPathFolder:
                        TreeNode node = UrlParser.Node;
                        if (node != null)
                        {
                            // Check 'Browse tree' permission or group administrator
                            if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, NodePermissionsEnum.ExploreTree) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                            {
                                // Set created and modified date
                                DateTime created = ValidationHelper.GetDateTime(node.GetValue("DocumentCreatedWhen"), DateTime.Now);
                                DateTime modified = ValidationHelper.GetDateTime(node.GetValue("DocumentModifiedWhen"), DateTime.Now);

                                ContentFolder contentAliasFolder = new ContentFolder(path, node.NodeAlias, created, modified, UrlParser, this, null);
                                return contentAliasFolder;
                            }
                        }
                        break;

                        // Nothing
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("WebDAVEngine_GetContentFolder", "WebDAV", ex);
            }

            return null;
        }


        /// <summary>
        /// Gets cms file.
        /// </summary>
        /// <param name="path">Path of cms file</param>
        private IHierarchyItem GetContentFile(string path)
        {
            try
            {
                TreeNode node = UrlParser.Node;

                if (node != null)
                {
                    var attachment = UrlParser.Attachment;
                    if (attachment != null)
                    {
                        string extension = ValidationHelper.GetString(node.GetValue("DocumentType"), string.Empty);
                        string name = node.NodeAlias + extension;
                        DateTime date = attachment.AttachmentLastModified;

                        // Create content resource
                        ContentResource contentResource = new ContentResource(path, name, attachment.AttachmentMimeType, attachment.AttachmentSize, date, date, UrlParser, this, null);
                        return contentResource;
                    }
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("WebDAVEngine_GetContentFile", "WebDAV", ex);
            }

            return null;
        }


        /// <summary>
        /// Gets folder.
        /// </summary>
        /// <param name="path">URL path</param>
        /// <param name="name">Folder name</param>
        private Folder GetFolder(string path, string name)
        {
            Folder folder = new Folder(path, name, DateTime.Now, DateTime.Now, UrlParser, this, null);
            return folder;
        }


        /// <summary>
        /// Gets group folder.
        /// </summary>
        /// <param name="path">URL path of group name</param>
        private Folder GetGroup(string path)
        {
            GeneralizedInfo group = UrlParser.Group;
            // Check if current user is group administrator
            if (WebDAVHelper.IsGroupAdministrator(group))
            {
                string groupName = ValidationHelper.GetString(group.GetValue("GroupName"), string.Empty);
                DateTime created = ValidationHelper.GetDateTime(group.GetValue("GroupCreatedWhen"), DateTime.Now);
                DateTime modified = ValidationHelper.GetDateTime(group.GetValue("GroupLastModified"), DateTime.Now);
                Folder groupFolder = new Folder(path, groupName, created, modified, UrlParser, this, null);
                return groupFolder;
            }

            return null;
        }


        /// <summary>
        /// Checks permission and 'group administrator' and gets new AttachmentFolder instance.
        /// </summary>
        /// <param name="name">Folder name</param>
        /// <param name="path">Folder path</param>
        /// <param name="permission">Node permission to check</param>
        /// <returns>AttachmentFolder instance</returns>
        private AttachmentFolder GetAttachmentFolder(string name, string path, NodePermissionsEnum permission)
        {
            TreeNode node = UrlParser.Node;
            if (node != null)
            {
                // Check given permission or group administrator
                if (WebDAVHelper.IsCurrentUserAuthorizedPerDocument(node, permission) || WebDAVHelper.IsGroupAdministrator(UrlParser.Group))
                {
                    // Set created and modified date
                    DateTime created = ValidationHelper.GetDateTime(node.GetValue("DocumentCreatedWhen"), DateTime.Now);
                    DateTime modified = ValidationHelper.GetDateTime(node.GetValue("DocumentModifiedWhen"), DateTime.Now);

                    AttachmentFolder attAliasFolder = new AttachmentFolder(path, name, created, modified, UrlParser, this, null);
                    return attAliasFolder;
                }
            }

            return null;
        }

        #endregion
    }
}
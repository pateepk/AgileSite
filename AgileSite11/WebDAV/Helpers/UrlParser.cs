using System;
using System.Data;

using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DocumentEngine;
using CMS.DataEngine;

namespace CMS.WebDAV
{
    /// <summary>
    /// URL parser.
    /// </summary>
    public class UrlParser
    {
        #region "Variables"

        private readonly string mPath;
        private string mAliasPath;
        private string mLibraryFolder;
        private ItemTypeEnum mItemTypeEnum = ItemTypeEnum.None;
        private TreeNode mNode;
        private TreeProvider mTreeProvider;

        private BaseInfo mGroupInfo;
        private BaseInfo mMediaLibraryInfo;
        private BaseInfo mMediaFileInfo;

        private DocumentAttachment mAttachment;
        private MetaFileInfo mMetaFileInfo;
        private FileInfo mFileInfo;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets URL path.
        /// </summary>
        public string Path
        {
            get
            {
                return mPath;
            }
        }


        /// <summary>
        /// Gets file name.
        /// </summary>
        public string FileName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets culture code.
        /// </summary>
        public string CultureCode
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets file GUID.
        /// </summary>
        public Guid GUID
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets alias path.
        /// </summary>
        public string AliasPath
        {
            get
            {
                if ((mAliasPath == WebDAVSettings.ALIASPATH_ROOT) || (mAliasPath == ("/" + WebDAVSettings.ALIASPATH_ROOT)))
                {
                    mAliasPath = "/";
                }

                return mAliasPath;
            }
            private set
            {
                mAliasPath = value;
            }
        }


        /// <summary>
        /// Gets field name of attachment.
        /// </summary>
        public string FieldName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets group name.
        /// </summary>
        public string GroupName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets file path of media file.
        /// </summary>
        public string FilePath
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets physical file path (not safe) of media file.
        /// </summary>
        public string PhysicalFilePath
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets library name.
        /// </summary>
        public string LibraryName
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets library folder.
        /// </summary>
        public string LibraryFolder
        {
            get
            {
                if ((mLibraryFolder == null) && (MediaLibraryInfo != null))
                {
                    mLibraryFolder = MediaLibraryInfo.LibraryFolder;
                }

                return mLibraryFolder;
            }
        }


        /// <summary>
        /// Gets item type.
        /// </summary>
        public ItemTypeEnum ItemType
        {
            get
            {
                return mItemTypeEnum;
            }
            private set
            {
                mItemTypeEnum = value;
            }
        }


        /// <summary>
        /// Gets group info.
        /// </summary>
        public BaseInfo Group
        {
            get
            {
                if ((mGroupInfo == null) && (!string.IsNullOrEmpty(GroupName)))
                {
                    mGroupInfo = ModuleCommands.CommunityGetGroupInfoByName(GroupName, SiteContext.CurrentSiteName);
                }

                return mGroupInfo;
            }
            private set
            {
                mGroupInfo = value;
            }
        }


        /// <summary>
        /// Gets node.
        /// </summary>
        public TreeNode Node
        {
            get
            {
                if (mNode == null)
                {
                    string aliasPath = AliasPath;

                    try
                    {
                        if (Group != null)
                        {
                            Guid groupNodeGUID = ValidationHelper.GetGuid(Group.GetValue("GroupNodeGUID"), Guid.Empty);

                            if (groupNodeGUID == Guid.Empty)
                            {
                                string groupName = ValidationHelper.GetString(Group.GetValue("GroupName"), string.Empty);
                                throw new Exception(string.Format("[UrlParser.Node] : Group '{0}' doesn't have any pages location set.", groupName));
                            }

                            // Prepare the where condition
                            string where = "NodeGUID = N'" + groupNodeGUID + "'";
                            // Set alias path
                            aliasPath = WebDAVHelper.GetNodeAliasPath(where, TreeProvider);

                            if (AliasPath != "/")
                            {
                                aliasPath = (aliasPath == "/") ? AliasPath : (aliasPath + AliasPath);
                            }
                        }

                        string siteName = SiteContext.CurrentSiteName;
                        string whereCondition = "(NodeAliasPath = N'" + SqlHelper.GetSafeQueryString(aliasPath, false) + "' AND DocumentHash IS NULL) OR DocumentHash = N'" + WebDAVHelper.GetResourceHash(Path) + "'";

                        // Get node with coupled data
                        mNode = TreeProvider.SelectSingleNode(siteName, null, CultureCode, false, null, whereCondition, "DocumentHash DESC", TreeProvider.ALL_LEVELS, false);
                        if (mNode != null)
                        {
                            mNode = DocumentHelper.GetDocument(mNode, TreeProvider);

                            // Only for CMS File
                            if ((ItemType == ItemTypeEnum.ContentFile) && !string.IsNullOrEmpty(FileName))
                            {
                                // Check if document type equals to extension
                                if (!mNode.DocumentType.EqualsCSafe(IO.Path.GetExtension(FileName), true))
                                {
                                    mNode = null;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("UrlParser_Node", "WebDAV", ex);
                    }
                }

                return mNode;
            }
        }


        /// <summary>
        /// Gets Tree provider.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                return mTreeProvider ?? (mTreeProvider = new TreeProvider(MembershipContext.AuthenticatedUser));
            }
        }

        #endregion


        #region "Info Properties"

        /// <summary>
        /// Gets atttachment info.
        /// </summary>
        public DocumentAttachment Attachment
        {
            get
            {
                if ((mAttachment == null) && (Node != null))
                {
                    switch (ItemType)
                    {
                        // Unsorted, field and grouped attachment info
                        case ItemTypeEnum.UnsortedAttachmentFile:
                        case ItemTypeEnum.FieldAttachmentFile:
                            {
                                string where = null;
                                string fileName = SqlHelper.GetSafeQueryString(FileName, false);

                                // Unsorted attachment
                                if (ItemType == ItemTypeEnum.UnsortedAttachmentFile)
                                {
                                    // Unsorted attachment
                                    where = "((AttachmentGroupGUID IS NULL) AND (AttachmentIsUnsorted = 1) AND (AttachmentName = N'" + fileName + "'))" +
                                            " OR (AttachmentHash = N'" + WebDAVHelper.GetResourceHash(Path) + "')";
                                }
                                // Field or grouped attachment
                                else if (ItemType == ItemTypeEnum.FieldAttachmentFile)
                                {
                                    FormFieldInfo ffi = WebDAVHelper.GetFormFieldInfo(Node, FieldName);

                                    if ((ffi != null))
                                    {
                                        // Grouped attachment
                                        if (ffi.DataType == DocumentFieldDataType.DocAttachments)
                                        {
                                            where = "((AttachmentGroupGUID = N'" + ffi.Guid + "') AND (AttachmentIsUnsorted IS NULL OR AttachmentIsUnsorted = 0) AND (AttachmentName = N'" + fileName + "'))" +
                                                    " OR (AttachmentHash = N'" + WebDAVHelper.GetResourceHash(Path) + "')";
                                        }
                                        // File attachment
                                        else if (ffi.DataType == FieldDataType.File)
                                        {
                                            Guid attachmentGUID = ValidationHelper.GetGuid(Node.GetValue(ffi.Name), Guid.Empty);

                                            where = "((AttachmentGroupGUID IS NULL) AND (AttachmentIsUnsorted IS NULL  OR AttachmentIsUnsorted = 0) AND (AttachmentGUID = N'" + attachmentGUID +
                                                    "') AND (AttachmentName = N'" + fileName + "'))" + " OR (AttachmentHash = N'" + WebDAVHelper.GetResourceHash(Path) + "')";
                                        }
                                    }
                                }

                                var attDataSet = 
                                    DocumentHelper.GetAttachments(Node, false)
                                        .ApplySettings(settings => settings
                                            .Where(where)
                                            .TopN(1)
                                        )
                                        .Result;

                                // Try to find temporary attachment info - when is new attachment created
                                if (DataHelper.DataSourceIsEmpty(attDataSet))
                                {

                                    var whereNameOrHash = new WhereCondition()
                                        .WhereEquals("AttachmentName", fileName)
                                        .Or().WhereEquals("AttachmentHash", WebDAVHelper.GetResourceHash(Path));

                                    var whereGuid = new WhereCondition()
                                        .WhereEquals("AttachmentFormGUID", Node.NodeGUID);

                                    attDataSet = AttachmentInfoProvider.GetAttachments()
                                                                       .Where(whereNameOrHash)
                                                                       .And().Where(whereGuid)
                                                                       .And().WhereNull("AttachmentVariantParentID")
                                                                       .BinaryData(false)
                                                                       .TopN(1);
                                }

                                // Check if data source is not empty
                                if (!DataHelper.DataSourceIsEmpty(attDataSet))
                                {
                                    mAttachment = new DocumentAttachment(attDataSet.Tables[0].Rows[0]);
                                }
                            }
                            break;

                        // CMS file
                        case ItemTypeEnum.ContentFile:
                            {
                                // Get Guid
                                Guid fileGuid = ValidationHelper.GetGuid(Node.GetValue("FileAttachment"), Guid.Empty);

                                if (fileGuid != Guid.Empty)
                                {
                                    var attDataSet = 
                                        DocumentHelper.GetAttachments(Node, false)
                                            .ApplySettings(settings => settings
                                                .WhereEquals("AttachmentGUID", fileGuid)
                                                .TopN(1)
                                            )
                                            .Result;

                                    // Try to find temporary attachment info - when is new cms file created
                                    if (DataHelper.DataSourceIsEmpty(attDataSet))
                                    {
                                        // Get temporary attachment
                                        mAttachment = (DocumentAttachment)AttachmentInfoProvider.GetAttachmentInfo(fileGuid, SiteContext.CurrentSiteName);
                                    }
                                    else
                                    {
                                        mAttachment = new DocumentAttachment(attDataSet.Tables[0].Rows[0]);
                                    }
                                }
                            }
                            break;
                    }
                }

                return mAttachment;
            }
        }


        /// <summary>
        /// Gets media library info.
        /// </summary>
        public MediaLibraryInfo MediaLibraryInfo
        {
            get
            {
                if (mMediaLibraryInfo == null)
                {
                    if (Group != null)
                    {
                        // Get media library in group
                        mMediaLibraryInfo = MediaLibraryInfoProvider.GetMediaLibraryInfo(LibraryName,
                                                                                         SiteContext.CurrentSiteID, ValidationHelper.GetInteger(Group.GetValue("GroupID"), 0));
                    }
                    else
                    {
                        // Get media library
                        mMediaLibraryInfo = MediaLibraryInfoProvider.GetMediaLibraryInfo(LibraryName, SiteContext.CurrentSiteName);
                    }
                }

                return (MediaLibraryInfo)mMediaLibraryInfo;
            }
        }


        /// <summary>
        /// Gets media file info.
        /// </summary>
        public MediaFileInfo MediaFileInfo
        {
            get
            {
                if ((mMediaFileInfo == null) && (MediaLibraryInfo != null))
                {
                    // Get media file info
                    mMediaFileInfo = MediaFileInfoProvider.GetMediaFileInfo(SiteContext.CurrentSiteName, FilePath, MediaLibraryInfo.LibraryFolder);
                }

                return (MediaFileInfo)mMediaFileInfo;
            }
        }


        /// <summary>
        /// Gets metafile info.
        /// </summary>
        public MetaFileInfo MetaFileInfo
        {
            get
            {
                if (mMetaFileInfo == null)
                {
                    if ((GUID != Guid.Empty) && (!string.IsNullOrEmpty(FileName)))
                    {
                        // Get metafile without binary data
                        mMetaFileInfo = MetaFileInfoProvider.GetMetaFileInfoWithoutBinary(GUID, SiteContext.CurrentSiteName, true);
                    }
                }

                return mMetaFileInfo;
            }
        }


        /// <summary>
        /// Gets file info (media file in file system).
        /// </summary>
        public FileInfo FileInfo
        {
            get
            {
                if (mFileInfo == null)
                {
                    if (MediaLibraryInfo != null)
                    {
                        string libraryPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(MediaLibraryInfo.LibraryID);

                        string basePath = libraryPath;
                        if (PhysicalFilePath != null)
                        {
                            basePath += "/" + PhysicalFilePath;
                        }

                        basePath = IO.Path.EnsureBackslashes(basePath, true);

                        if (File.Exists(basePath))
                        {
                            mFileInfo = FileInfo.New(basePath);
                        }
                    }
                }

                return mFileInfo;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes object and parses URL path.
        /// </summary>
        /// <param name="path">URL path</param>
        public UrlParser(string path)
        {
            mPath = path;
            Parse(path);
        }

        #endregion


        #region "Public Methods"


        /// <summary>
        /// Gets allowed name from name. Allowed name can not contain certain characters.
        /// </summary>
        /// <param name="name">Name to be transformed into allowed name</param>
        /// <returns>Allowed name</returns>
        public string GetAllowedName(string name)
        {
            string safeFileName = null;
            switch (ItemType)
            {
                case ItemTypeEnum.MediaFolder:
                case ItemTypeEnum.MediaLibraryName:
                    safeFileName = WebDAVHelper.GetSafeFileName(name, SiteContext.CurrentSiteName, WebDAVHelper.MAX_MEDIA_FILENAME_LENGTH);
                    break;
                default:
                    safeFileName = name;
                    break;
            }
            return safeFileName;
        }


        #endregion


        #region "Private Methods"

        /// <summary>
        /// Parses URL path.
        /// </summary>
        /// <param name="path">URL path</param>
        private void Parse(string path)
        {
            ItemType = ItemTypeEnum.None;

            // Check if path is root path
            if (CMSString.Compare(path, WebDAVSettings.BasePath.TrimEnd('/'), true) == 0)
            {
                ItemType = ItemTypeEnum.Root;
                return;
            }

            // Attachments
            if (path.StartsWithCSafe(WebDAVSettings.AttachmentsBasePath, true))
            {
                path = path.Substring(WebDAVSettings.AttachmentsBasePath.Length);
                ParseAttachments(path);
            }
            // Media files
            else if (path.StartsWithCSafe(WebDAVSettings.MediaFilesBasePath, true))
            {
                path = path.Substring(WebDAVSettings.MediaFilesBasePath.Length);
                ParseMedia(path);
            }
            // Meta files
            else if (path.StartsWithCSafe(WebDAVSettings.MetaFilesBasePath, true))
            {
                path = path.Substring(WebDAVSettings.MetaFilesBasePath.Length);
                ParseMetaFiles(path);
            }
            // Content files
            else if (path.StartsWithCSafe(WebDAVSettings.ContentFilesBasePath, true))
            {
                path = path.Substring(WebDAVSettings.ContentFilesBasePath.Length);
                ParseContent(path);
            }
            // URL contains group name
            else if (path.StartsWithCSafe(WebDAVSettings.GroupsBasePath, true))
            {
                // Check if 'Community' module is loaded
                if (!WebDAVHelper.IsCommunityModuleLoaded)
                {
                    ItemType = ItemTypeEnum.NotSupportedGroups;
                    return;
                }

                path = path.Substring(WebDAVSettings.GroupsBasePath.Length);

                string[] splits = path.Split(new char[] { '/' }, 4);

                if (splits.Length == 1)
                {
                    ItemType = ItemTypeEnum.Groups;
                }
                else if (splits.Length == 2)
                {
                    GroupName = splits[1];
                    ItemType = ItemTypeEnum.GroupName;
                }
                else if ((splits.Length == 3) || (splits.Length == 4))
                {
                    GroupName = splits[1];
                    path = path.Substring(GroupName.Length + 1).TrimStart('/');
                }

                // If not decided yet
                if (ItemType == ItemTypeEnum.None)
                {
                    // Attachment
                    if (path.StartsWithCSafe(WebDAVSettings.AttachmentsFolder, true))
                    {
                        ParseAttachments(path);
                    }
                        // Media file
                    else if (path.StartsWithCSafe(WebDAVSettings.MediaFilesFolder, true))
                    {
                        ParseMedia(path);
                    }
                        // Pages
                    else if (path.StartsWithCSafe(WebDAVSettings.PagesFolder, true))
                    {
                        ParseContent(path);
                    }
                }
            }
        }


        /// <summary>
        /// Parses URL of attachment.
        /// </summary>
        /// <param name="path">Attachment URL</param>
        private void ParseAttachments(string path)
        {
            ItemType = ItemTypeEnum.AttachmentFolder;

            if (path.EndsWithCSafe("/"))
            {
                path = path.Remove(path.Length - 1);
            }

            string[] splits = path.Split('/');

            // Culture code folder
            if (splits.Length == 2)
            {
                CultureCode = splits[1];
                ItemType = ItemTypeEnum.AttachmentCultureFolder;
                AliasPath = "/";
            }
            else if (splits.Length >= 3)
            {
                CultureCode = splits[1];

                // Full path with file name
                if (splits[splits.Length - 1].Contains("."))
                {
                    FileName = splits[splits.Length - 1];
                    FileName = WebDAVHelper.GetSafeFileName(FileName, SiteContext.CurrentSiteName, AttachmentInfoProvider.MAXATTACHMENTNAMELENGTH);

                    // Unsorted attachment
                    if (CMSString.Compare(splits[splits.Length - 2], WebDAVSettings.UnsortedFolder, true) == 0)
                    {
                        ItemType = ItemTypeEnum.UnsortedAttachmentFile;
                    }
                    // Field attachment
                    else
                    {
                        FieldName = splits[splits.Length - 2];
                        // Remove brackets
                        FieldName = WebDAVHelper.GetFieldNameFromUrl(FieldName);

                        ItemType = ItemTypeEnum.FieldAttachmentFile;
                    }

                    // Get alias path
                    for (int i = 2; i < splits.Length - 2; i++)
                    {
                        AliasPath += "/" + splits[i];
                    }
                }
                // Unsorted attachment
                else if (CMSString.Compare(splits[splits.Length - 1], WebDAVSettings.UnsortedFolder, true) == 0)
                {
                    // Get alias path
                    for (int i = 2; i < splits.Length - 1; i++)
                    {
                        AliasPath += "/" + splits[i];
                    }

                    ItemType = ItemTypeEnum.AttachmentUnsortedFolder;
                }
                // Field name
                else if ((splits[splits.Length - 1].StartsWithCSafe(WebDAVHelper.FIELD_NAME_PREFIX)) && (splits[splits.Length - 1].EndsWithCSafe(WebDAVHelper.FIELD_NAME_SUFFIX)))
                {
                    FieldName = splits[splits.Length - 1];
                    // Remove brackets
                    FieldName = WebDAVHelper.GetFieldNameFromUrl(FieldName);

                    // Get alias path
                    for (int i = 2; i < splits.Length - 1; i++)
                    {
                        AliasPath += "/" + splits[i];
                    }

                    ItemType = ItemTypeEnum.AttachmentFieldNameFolder;
                }
                // Alias path
                else
                {
                    // Get alias path
                    for (int i = 2; i < splits.Length; i++)
                    {
                        AliasPath += "/" + splits[i];
                    }

                    ItemType = ItemTypeEnum.AttachmentAliasPathFolder;
                }

                if (AliasPath == null)
                {
                    // Set root alias path
                    AliasPath = "/";
                }
            }
        }


        /// <summary>
        /// Parses URL of media file.
        /// </summary>
        /// <param name="path">Media URL path</param>
        private void ParseMedia(string path)
        {
            // Check if module 'Media Library' is loaded
            if (!WebDAVHelper.IsMediaLibraryModuleLoaded)
            {
                ItemType = ItemTypeEnum.NotSupportedMediaLibrary;
                return;
            }

            ItemType = ItemTypeEnum.MediaFolder;

            string[] splits = path.Split(new char[] { '/' }, 3);

            // Library name folder
             if (splits.Length == 2)
            {
                LibraryName = splits[1];
                ItemType = ItemTypeEnum.MediaLibraryName;
            }
            // Full path
            else if (splits.Length == 3)
            {
                LibraryName = splits[1];
                PhysicalFilePath = splits[2];
                FilePath = PhysicalFilePath;

                // File path contains sub folders
                if (FilePath.Contains("/"))
                {
                    int index = FilePath.LastIndexOfCSafe("/");
                    // Folders path
                    string foldersPath = FilePath.Substring(0, index);
                    // File name
                    string fileName = FilePath.Substring(index + 1);
                    // Get safe file name
                    string safeFileName = WebDAVHelper.GetSafeFileName(fileName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_MEDIA_FILENAME_LENGTH);

                    if (!string.IsNullOrEmpty(safeFileName))
                    {
                        // New safe file path
                        FilePath = foldersPath + "/" + safeFileName;
                    }
                    else
                    {
                        FilePath = foldersPath;
                    }
                }
                else
                {
                    // Get safe file name
                    FilePath = WebDAVHelper.GetSafeFileName(FilePath, SiteContext.CurrentSiteName, WebDAVHelper.MAX_MEDIA_FILENAME_LENGTH);
                }

                ItemType = ItemTypeEnum.MediaLibraryName;

                // Get path of media file
                string filePath = MediaFileInfoProvider.GetMediaFilePath(SiteContext.CurrentSiteName, LibraryFolder, FilePath);

                // Path might be null for root media directory
                if (filePath != null)
                {
                    // Check if file exists in DB
                    if (File.Exists(filePath))
                    {
                        ItemType = ItemTypeEnum.MediaFile;
                        return;
                    }

                    // Get path of media file that is stored in file system
                    filePath = MediaFileInfoProvider.GetMediaFilePath(SiteContext.CurrentSiteName, LibraryFolder, PhysicalFilePath);
                    // Check if file exists in file system
                    if (File.Exists(filePath))
                    {
                        ItemType = ItemTypeEnum.MediaFile;
                    }
                }
            }
        }


        /// <summary>
        /// Parses URL of meta file.
        /// </summary>
        /// <param name="path">Meta file URL path</param>
        private void ParseMetaFiles(string path)
        {
            ItemType = ItemTypeEnum.MetaFileFolder;

            path = path.TrimStart('/');

            if (path.Contains("/"))
            {
                // Set GUID
                string guidString = path.Substring(0, path.IndexOfCSafe("/"));
                GUID = ValidationHelper.GetGuid(guidString, Guid.Empty);
            }

            if ((GUID.ToString().Length + 1) < path.Length)
            {
                // Set file name
                FileName = path.Substring(GUID.ToString().Length + 1);
            }

            if ((GUID != Guid.Empty) && (!string.IsNullOrEmpty(FileName)))
            {
                ItemType = ItemTypeEnum.MetaFile;
            }
        }


        /// <summary>
        /// Parses URL of content.
        /// </summary>
        /// <param name="path">Content URL path</param>
        private void ParseContent(string path)
        {
            ItemType = ItemTypeEnum.ContentFolder;

            string[] splits = path.Split(new char[] { '/' }, 3);

            // Culture code folder
            if (splits.Length == 2)
            {
                CultureCode = splits[1];
                AliasPath = "/";
                ItemType = ItemTypeEnum.ContentCultureCodeFolder;
            }
            else if (splits.Length == 3)
            {
                // Set culture code
                CultureCode = splits[1];
                // Set alias path
                AliasPath = splits[2];

                if (AliasPath.Contains("/"))
                {
                    int index = AliasPath.LastIndexOfCSafe("/");
                    string fileName = AliasPath.Substring(index + 1);

                    // Url path contains file name
                    if (!string.IsNullOrEmpty(fileName) && fileName.Contains("."))
                    {
                        AliasPath = TreePathUtils.GetSafeNodeAliasPath(AliasPath.Substring(0, index), SiteContext.CurrentSiteName);
                        FileName = WebDAVHelper.GetSafeFileName(fileName, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
                        AliasPath += "/" + IO.Path.GetFileNameWithoutExtension(FileName);
                        ItemType = ItemTypeEnum.ContentFile;
                    }
                    // Only alias path
                    else
                    {
                        AliasPath = TreePathUtils.GetSafeNodeAliasPath(AliasPath, SiteContext.CurrentSiteName);
                        ItemType = ItemTypeEnum.ContentAliasPathFolder;
                    }
                }
                else
                {
                    if (AliasPath.Contains("."))
                    {
                        FileName = WebDAVHelper.GetSafeFileName(AliasPath, SiteContext.CurrentSiteName, WebDAVHelper.MAX_ATTACHMENT_FILENAME_LENGTH);
                        ItemType = ItemTypeEnum.ContentFile;
                    }
                    else
                    {
                        ItemType = ItemTypeEnum.ContentAliasPathFolder;
                    }

                    AliasPath = IO.Path.GetFileNameWithoutExtension(AliasPath);
                    AliasPath = TreePathUtils.GetSafeNodeAliasPath(AliasPath, SiteContext.CurrentSiteName);
                }
            }
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.LicenseProvider;
using CMS.MediaLibrary;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.WorkflowEngine;

namespace CMS.WebDAV
{
    /// <summary>
    /// WebDAV helper.
    /// </summary>
    public static class WebDAVHelper
    {
        #region "Constants"

        /// <summary>
        /// Field name prefix.
        /// </summary>
        public const string FIELD_NAME_PREFIX = "[";


        /// <summary>
        /// Field name suffix.
        /// </summary>
        public const string FIELD_NAME_SUFFIX = "]";


        /// <summary>
        /// Maximal length of the media file name.
        /// </summary>
        public const int MAX_MEDIA_FILENAME_LENGTH = 250;


        /// <summary>
        /// Maximal length of the attachment name.
        /// </summary>
        public const int MAX_ATTACHMENT_FILENAME_LENGTH = AttachmentInfoProvider.MAXATTACHMENTNAMELENGTH;


        /// <summary>
        /// Prefix of temporary file.
        /// </summary>
        public const string TEMPORARY_FILE_PREFIX = "~$";

        #endregion


        #region "Variables"

        private static bool? mIsMediaLibraryModuleLoaded;
        private static bool? mIsCommunityModuleLoaded;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets culture codes of site.
        /// </summary>
        public static List<string> CulturesCodes
        {
            get
            {
                // Get site cultures
                return CultureSiteInfoProvider.GetSiteCultureCodes(SiteContext.CurrentSiteName);
            }
        }


        /// <summary>
        /// Indicates if module 'Media Library' is loaded.
        /// </summary>
        public static bool IsMediaLibraryModuleLoaded
        {
            get
            {
                if (mIsMediaLibraryModuleLoaded == null)
                {
                    mIsMediaLibraryModuleLoaded = ModuleEntryManager.IsModuleLoaded(ModuleName.MEDIALIBRARY);
                }

                return mIsMediaLibraryModuleLoaded.Value;
            }
        }


        /// <summary>
        /// Indicates if module 'Community' is loaded.
        /// </summary>
        public static bool IsCommunityModuleLoaded
        {
            get
            {
                if (mIsCommunityModuleLoaded == null)
                {
                    mIsCommunityModuleLoaded = ModuleEntryManager.IsModuleLoaded(ModuleName.COMMUNITY);
                }

                return mIsCommunityModuleLoaded.Value;
            }
        }

        #endregion


        #region "Public Methods"

        /// <summary>
        /// Returns true if the WebDAV is enabled for the given extension
        /// </summary>
        /// <param name="extension">Extension to check</param>
        /// <param name="siteName">Site name to check, if null, current site is used</param>
        public static bool IsWebDAVExtensionEnabled(string extension, string siteName = null)
        {
            siteName = siteName ?? SiteContext.CurrentSiteName;

            return (IsWebDAVEnabled(siteName) && WebDAVSettings.IsExtensionAllowedForEditMode(extension, siteName));
        }


        /// <summary>
        /// Indicates if the WebDAV support is enabled for given site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool IsWebDAVEnabled(string siteName = null)
        {
            if (!AbstractStockHelper<RequestStockHelper>.Contains("WebDAVEnabled"))
            {
                // Get current domain
                string domain = null;
                if (HttpContext.Current != null)
                {
                    domain = RequestContext.URL.Host;
                }

                siteName = siteName ?? SiteContext.CurrentSiteName;

                // Add value to request items
                bool webDAVEnabled = RequestHelper.IsWindowsAuthentication() && LicenseKeyInfoProvider.IsFeatureAvailable(domain, FeatureEnum.WebDav) && SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSEnableWebDAV");

                AbstractStockHelper<RequestStockHelper>.Add("WebDAVEnabled", webDAVEnabled);

                return webDAVEnabled;
            }

            return (bool)AbstractStockHelper<RequestStockHelper>.GetItem("WebDAVEnabled");
        }


        /// <summary>
        /// Checks if specified extension is allowed.
        /// </summary>
        /// <param name="formFieldInfo">Form field info</param>
        /// <param name="extension">File extension</param>
        /// <returns>TRUE if extension is allowed</returns>
        public static bool IsCMSFileExtensionAllowed(FormFieldInfo formFieldInfo, string extension)
        {
            // Files without extension are not supported
            if (String.IsNullOrEmpty(extension))
            {
                return false;
            }

            if (formFieldInfo != null)
            {
                bool useCustom = (ValidationHelper.GetString(formFieldInfo.Settings["extensions"], "") == "custom");
                string allowedTypes = useCustom ? formFieldInfo.Settings["allowed_extensions"] as string : null;

                bool match = false;


                // Add global allowed file extensions (from Settings)
                if (allowedTypes == null)
                {
                    string siteExtensions = SettingsKeyInfoProvider.GetValue(SiteContext.CurrentSiteName + ".CMSUploadExtensions");
                    allowedTypes += ";" + siteExtensions;
                }
                allowedTypes = ";" + allowedTypes.ToLowerCSafe() + ";";

                if ((allowedTypes != ";;") && (allowedTypes != ";;;"))
                {
                    // Remove '.' from the beginning of file type if it's present
                    extension = extension.ToLowerCSafe().TrimStart('.');

                    if (allowedTypes.Contains(";" + extension + ";") || allowedTypes.Contains(";." + extension + ";"))
                    {
                        match = true;
                    }
                }
                else
                {
                    match = true;
                }

                return match;
            }

            return false;
        }


        /// <summary>
        ///  Checks if media file extension is allowed for browse mode.
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="siteName">Site name</param>
        internal static bool IsMediaFileExtensionAllowedForBrowseMode(string extension, string siteName)
        {
            return IsExtensionAllowedForBrowseMode(extension, null, siteName, true);
        }


        /// <summary>
        ///  Checks if file extension is allowed for browse mode.
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="siteName">Site name</param>
        public static bool IsExtensionAllowedForBrowseMode(string extension, string siteName)
        {
            return IsExtensionAllowedForBrowseMode(extension, null, siteName);
        }


        /// <summary>
        /// Checks if file extension is allowed for browse mode.
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="formFieldInfo">Form field info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="mediaFile">Indicate whether you are checking for media-file or not (Parameter is optional to keep backward compatibility in API)</param>
        public static bool IsExtensionAllowedForBrowseMode(string extension, FormFieldInfo formFieldInfo, string siteName, bool mediaFile = false)
        {
            if (siteName == null)
            {
                throw new ArgumentNullException("siteName", "[WebDAVHelper : IsExtensionAllowedForBrowseMode] : Argument 'sitename' cannot be null.");
            }

            // Files without extension are not supported
            if (String.IsNullOrEmpty(extension))
            {
                return false;
            }

            string allowedTypes = null;

            if (formFieldInfo != null)
            {
                bool useCustom = (ValidationHelper.GetString(formFieldInfo.Settings["extensions"], "") == "custom");
                allowedTypes = useCustom ? formFieldInfo.Settings["allowed_extensions"] as string : null;
            }

            // Add global allowed file extensions (from Settings)
            if (allowedTypes == null)
            {
                allowedTypes = SettingsKeyInfoProvider.GetValue(siteName + (mediaFile ? ".CMSMediaFileAllowedExtensions" : ".CMSUploadExtensions"));
            }

            string[] allowedExtensions = allowedTypes.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            // Remove '.' from the beginning of file type if it's present
            extension = extension.ToLowerCSafe().TrimStart('.');

            return (!allowedExtensions.Any() || allowedExtensions.Any(p => p.Trim().TrimStart('.').ToLowerCSafe() == extension));
        }


        /// <summary>
        /// Returns the safe equivalent to the given file name.
        /// </summary>
        /// <param name="fileName">Original file name with extension</param>
        /// <param name="siteName">Site name</param>
        /// <param name="maxLength">Maximum length of file name</param>
        public static string GetSafeFileName(string fileName, string siteName, int maxLength)
        {
            // Get safe file name
            string safeFileName = URLHelper.GetSafeFileName(fileName, siteName);

            if (safeFileName.Length > maxLength)
            {
                string name = Path.GetFileNameWithoutExtension(safeFileName);
                string extension = Path.GetExtension(safeFileName);
                int extLength = extension.Length;

                if (extLength > maxLength)
                {
                    throw new Exception(String.Format("[WebDAVHelper.GetSafeFileName] : File '{0}' extension is too long.", safeFileName));
                }

                // Trim the safe file name
                safeFileName = name.Remove(maxLength - extLength);
                safeFileName += extension;
            }

            return safeFileName;
        }


        /// <summary>
        /// Gets name of field name for URL (e.g. [Field]).
        /// </summary>
        /// <param name="fieldName">Field name</param>
        public static string GetFieldNameForUrl(string fieldName)
        {
            return FIELD_NAME_PREFIX + fieldName + FIELD_NAME_SUFFIX;
        }


        /// <summary>
        /// Gets field name without brackets.
        /// </summary>
        /// <param name="fieldName">Field name with brackets</param>
        public static string GetFieldNameFromUrl(string fieldName)
        {
            fieldName = fieldName.StartsWithCSafe(FIELD_NAME_PREFIX) ? fieldName.Remove(0, 1) : fieldName;
            fieldName = fieldName.EndsWithCSafe(FIELD_NAME_SUFFIX) ? fieldName.Remove(fieldName.Length - 1) : fieldName;

            return fieldName;
        }


        /// <summary>
        /// Gets node alias path.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="treeProvider">Tree provider</param>
        public static string GetNodeAliasPath(string where, TreeProvider treeProvider)
        {
            string aliasPath = null;

            if (treeProvider == null)
            {
                treeProvider = new TreeProvider(MembershipContext.AuthenticatedUser);
            }

            DataSet ds = treeProvider.SelectNodes(SiteContext.CurrentSiteName, null, TreeProvider.ALL_CULTURES, false, null, @where, null, TreeProvider.ALL_LEVELS, false, 1, DocumentColumnLists.SELECTNODES_REQUIRED_COLUMNS + ",NodeAliasPath");

            // Check if data source is not empty and contains only one record
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Set alias path
                aliasPath = ValidationHelper.GetString(ds.Tables[0].Rows[0]["NodeAliasPath"], null);
            }

            return aliasPath;
        }


        /// <summary>
        /// Gets image autoresize values from settings (width, height, maxSideSize).
        /// </summary>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="maxSideSize">Max side size</param>
        public static void GetImageResizeValues(ref int width, ref int height, ref int maxSideSize)
        {
            GetImageResizeValues(null, ref width, ref height, ref maxSideSize);
        }


        /// <summary>
        /// Gets image autoresize values due to field info settings (width, height, maxSideSize).
        /// </summary>
        /// <param name="ffi">Form field info</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="maxSideSize">Max side size</param>
        public static void GetImageResizeValues(FormFieldInfo ffi, ref int width, ref int height, ref int maxSideSize)
        {
            bool useCustom = false;

            if (ffi != null)
            {
                useCustom = (ValidationHelper.GetString(ffi.Settings["autoresize"], "") == "custom");
            }

            // Custom settings
            if (useCustom)
            {
                width = ValidationHelper.GetInteger(ffi.Settings["autoresize_width"], 0);
                height = ValidationHelper.GetInteger(ffi.Settings["autoresize_height"], 0);
                maxSideSize = ValidationHelper.GetInteger(ffi.Settings["autoresize_maxsidesize"], 0);
            }
            // Site settings
            else
            {
                string siteName = SiteContext.CurrentSiteName;
                width = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSAutoResizeImageWidth");
                height = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSAutoResizeImageHeight");
                maxSideSize = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSAutoResizeImageMaxSideSize");
            }
        }


        /// <summary>
        /// Check out document.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="tree">Tree provider</param>
        /// <returns>TRUE if document was already checked out.</returns>
        public static bool CheckOutDocument(TreeNode node, TreeProvider tree)
        {
            if (node == null)
            {
                return false;
            }

            if (tree == null)
            {
                tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            }

            WorkflowManager workflowMan = WorkflowManager.GetInstance(tree);
            // Check if the document uses workflow
            bool useWorkflow = (workflowMan.GetNodeWorkflow(node) != null);

            int documentCheckedOutUserId = node.DocumentCheckedOutByUserID;

            if (useWorkflow && (documentCheckedOutUserId == 0))
            {
                // Check out the document
                VersionManager vm = VersionManager.GetInstance(tree);
                vm.CheckOut(node, node.IsPublished, true, true);
            }

            return (documentCheckedOutUserId > 0);
        }


        /// <summary>
        /// Check in document.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="tree">Tree provider</param>
        public static void CheckInDocument(TreeNode node, TreeProvider tree)
        {
            if (node == null)
            {
                return;
            }

            if (tree == null)
            {
                tree = new TreeProvider(MembershipContext.AuthenticatedUser);
            }

            WorkflowManager workflowMan = WorkflowManager.GetInstance(tree);

            bool useWorkflow = (workflowMan.GetNodeWorkflow(node) != null);
            int userId = node.DocumentCheckedOutByUserID;

            // Check if the document uses workflow and is checked-out by current user
            if (useWorkflow && (userId == MembershipContext.AuthenticatedUser.UserID))
            {
                // Check in the document
                VersionManager vm = VersionManager.GetInstance(tree);
                vm.CheckIn(node, null);
            }
        }


        /// <summary>
        /// If attachment is image then set height and width.
        /// </summary>
        /// <param name="attachmentInfo">Attachment info</param>
        public static void SetAttachmentHeightAndWidth(IAttachment attachmentInfo)
        {
            if (attachmentInfo != null)
            {
                if (ImageHelper.IsImage(attachmentInfo.AttachmentExtension))
                {
                    // Set image properties
                    var ih = new ImageHelper(attachmentInfo.AttachmentBinary);
                    if ((ih.ImageHeight > 0) || (ih.ImageWidth > 0))
                    {
                        attachmentInfo.AttachmentImageHeight = ih.ImageHeight;
                        attachmentInfo.AttachmentImageWidth = ih.ImageWidth;
                    }
                }
                else
                {
                    attachmentInfo.AttachmentImageHeight = 0;
                    attachmentInfo.AttachmentImageWidth = 0;
                }
            }
        }



        /// <summary>
        /// Resizes media file if is image.
        /// </summary>
        /// <param name="mediaFileInfo">Media file info object</param>
        public static void ResizeMediaFileInfo(MediaFileInfo mediaFileInfo)
        {
            // Read the data to memory only if it's an image (to get its properties)
            if ((mediaFileInfo != null) && ImageHelper.IsImage(mediaFileInfo.FileExtension))
            {
                string siteName = SiteContext.CurrentSiteName;

                // Get binary data from input stream
                if (mediaFileInfo.FileBinary == null)
                {
                    // Check binary data
                    if (mediaFileInfo.FileBinaryStream == null)
                    {
                        throw new Exception("[WebDAVHelper.ResizeMediaFileInfo]: Missing binary data.");
                    }
                    // Fill buffer with posted file binary data
                    int size = (int)mediaFileInfo.FileBinaryStream.Length;
                    mediaFileInfo.FileBinary = new byte[size];
                    System.IO.Stream stream = mediaFileInfo.FileBinaryStream;
                    stream.Read(mediaFileInfo.FileBinary, 0, size);
                    stream.Close();
                    stream.Dispose();
                    mediaFileInfo.FileBinaryStream = null;
                }

                if (mediaFileInfo.FileBinary != null)
                {
                    // Load image for resize
                    ImageHelper ih = new ImageHelper(mediaFileInfo.FileBinary);

                    int newWidth = ih.ImageWidth;
                    int newHeight = ih.ImageHeight;
                    long newSize = ih.SourceData.LongLength;

                    // Get resizing values from settings
                    int width = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSAutoResizeImageWidth");
                    int height = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSAutoResizeImageHeight");
                    int maxSideSize = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSAutoResizeImageMaxSideSize");

                    // Check if the autoresize of images is set
                    if ((width > 0) || (height > 0) || (maxSideSize > 0))
                    {
                        // Resize image if necessary
                        if (ih.CanResizeImage(width, height, maxSideSize))
                        {
                            mediaFileInfo.FileBinary = GetResizedImageData(ih, width, height, maxSideSize, out newWidth, out newHeight);
                            newSize = mediaFileInfo.FileBinary.LongLength;
                        }
                    }

                    // Set new width, height and size
                    mediaFileInfo.FileImageWidth = newWidth;
                    mediaFileInfo.FileImageHeight = newHeight;
                    mediaFileInfo.FileSize = newSize;
                }
            }
        }


        /// <summary>
        /// Creates media file info from attachment file info.
        /// </summary>
        /// <param name="currentMediaFile">Current media file</param>
        /// <param name="attachmentInfo">Attachment info</param>
        /// <returns>New media file info</returns>
        public static MediaFileInfo CreateMediaFileInfoFromAttachment(MediaFileInfo currentMediaFile, DocumentAttachment attachmentInfo)
        {
            if (attachmentInfo != null)
            {
                // Set existing media file or create new instance
                var newMediaFile = currentMediaFile ?? new MediaFileInfo();

                if (currentMediaFile == null)
                {
                    newMediaFile.FileGUID = Guid.NewGuid();
                }

                int userID = MembershipContext.AuthenticatedUser.UserID;

                // Copy properties
                newMediaFile.FileCreatedByUserID = userID;
                newMediaFile.FileMimeType = attachmentInfo.AttachmentMimeType;
                newMediaFile.FileModifiedByUserID = userID;
                newMediaFile.FileModifiedWhen = DateTime.Now;
                newMediaFile.FileDescription = attachmentInfo.AttachmentDescription;
                newMediaFile.FileTitle = attachmentInfo.AttachmentTitle;
                newMediaFile.FileSiteID = SiteContext.CurrentSiteID;
                newMediaFile.FileSize = attachmentInfo.AttachmentSize;
                newMediaFile.FileCustomData.LoadData(attachmentInfo.AttachmentCustomData.GetData());

                if ((attachmentInfo.AttachmentImageHeight > 0) || (attachmentInfo.AttachmentImageWidth > 0))
                {
                    newMediaFile.FileImageHeight = attachmentInfo.AttachmentImageHeight;
                    newMediaFile.FileImageWidth = attachmentInfo.AttachmentImageWidth;
                }

                return newMediaFile;
            }

            return null;
        }


        /// <summary>
        /// Gets new CMS Folder.
        /// </summary>
        /// <param name="name">CMS folder name</param>
        /// <param name="parentNode">Parent document</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="cultureCode">Culture code</param>
        /// <returns>New document</returns>
        public static TreeNode GetNewContentFolder(string name, TreeNode parentNode, TreeProvider tree, string cultureCode)
        {
            // Check parent node
            if (parentNode == null)
            {
                throw new ApplicationException("[WebDAVHelper.GetNewContentFolder]: The parent node wasn't selected.");
            }

            // Check if class exists
            DataClassInfo ci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(SystemDocumentTypes.Folder);
            if (ci == null)
            {
                throw new ApplicationException(String.Format("[WebDAVHelper.GetNewContentFolder]: The class '{0}' was not found.", SystemDocumentTypes.Folder));
            }

            #region "Check permissions"

            int parentClassId = ValidationHelper.GetInteger(parentNode.GetValue("NodeClassID"), 0);
            if (!AllowedChildClassInfoProvider.IsChildClassAllowed(parentClassId, ci.ClassID))
            {
                throw new ApplicationException(String.Format("[WebDAVHelper.GetNewContentFolder]: The folder '{0}' cannot be created under the page '{1}'.", name, parentNode.NodeAliasPath));
            }

            // Check user permissions
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedToCreateNewDocument(parentNode, SystemDocumentTypes.File))
            {
                throw new ApplicationException(String.Format("[WebDAVHelper.GetNewContentFolder]: The user '{0}' doesn't have permissions to create the page of type '{1}' in required location '{2}'.",
                                                              MembershipContext.AuthenticatedUser.UserName, SystemDocumentTypes.Folder, parentNode.NodeAliasPath));
            }

            #endregion

            string folderName = Path.GetFileNameWithoutExtension(name);

            // Ensure document name
            folderName = TreePathUtils.EnsureMaxNodeNameLength(folderName, SystemDocumentTypes.Folder);

            // Create new document
            TreeNode newNode = TreeNode.New(SystemDocumentTypes.Folder, tree);
            newNode.DocumentCulture = cultureCode;
            newNode.DocumentName = folderName;

            // Load default values
            FormHelper.LoadDefaultValues(newNode.NodeClassName, newNode);

            // Set NodeNodeSource if exists
            newNode.SetDocumentNameSource(folderName);

            // Set default template ID     
            newNode.SetDefaultPageTemplateID(ci.ClassDefaultPageTemplateID);

            // Return new node
            return newNode;
        }


        /// <summary>
        /// Gets new CMS File (only document without attachment).
        /// </summary>
        /// <param name="name">CMS file name</param>
        /// <param name="parentNode">Parent document</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="cultureCode">Culture code</param>
        /// <param name="ffi">Form file info</param>
        /// <returns>New document</returns>
        public static TreeNode GetNewContent(string name, TreeNode parentNode, TreeProvider tree, string cultureCode, out FormFieldInfo ffi)
        {
            ffi = null;

            // Check parent node
            if (parentNode == null)
            {
                throw new ApplicationException("[WebDAVHelper.GetExistingOrNewContent]: The parent node wasn't selected.");
            }

            // Check if class exists
            DataClassInfo ci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(SystemDocumentTypes.File);
            if (ci == null)
            {
                throw new ApplicationException(String.Format("[WebDAVHelper.GetExistingOrNewContent]: The class '{0}' was not found.", SystemDocumentTypes.File));
            }


            #region "Check permissions"

            if (!DocumentHelper.IsDocumentTypeAllowed(parentNode, ci.ClassID))
            {
                throw new ApplicationException(String.Format("[WebDAVHelper.GetExistingOrNewContent]: The file '{0}' cannot be uploaded under the page '{1}'.", name, parentNode.NodeAliasPath));
            }

            // Check user permissions
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedToCreateNewDocument(parentNode, SystemDocumentTypes.File))
            {
                throw new ApplicationException(String.Format("[WebDAVHelper.GetExistingOrNewContent]: The user '{0}' doesn't have permissions to create the page of type '{1}' in required location '{2}'.",
                                                             MembershipContext.AuthenticatedUser.UserName, SystemDocumentTypes.File, parentNode.NodeAliasPath));
            }

            #endregion


            FormInfo fi = FormHelper.GetFormInfo(ci.ClassName, false);
            ffi = fi.GetFormField("FileAttachment");

            string fileName = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(name);

            // Check the allowed extensions for creating CMS file
            if (!IsCMSFileExtensionAllowed(ffi, extension))
            {
                throw new ApplicationException(String.Format("[WebDAVHelper.GetExistingOrNewContent]: The files with extension '{0}' cannot be uploaded.", extension));
            }

            // Ensure document name
            fileName = TreePathUtils.EnsureMaxNodeNameLength(fileName, SystemDocumentTypes.File);

            // Create new document
            TreeNode newNode = TreeNode.New(SystemDocumentTypes.File, tree);
            newNode.DocumentCulture = cultureCode;
            newNode.DocumentName = fileName;
            newNode.SetValue("DocumentType", extension);

            // Load default values
            FormHelper.LoadDefaultValues(newNode.NodeClassName, newNode);

            // Set NodeNodeSource if exists
            newNode.SetDocumentNameSource(fileName);

            // Set default template ID     
            newNode.SetDefaultPageTemplateID(ci.ClassDefaultPageTemplateID);

            // Return new node
            return newNode;
        }


        /// <summary>
        /// Indicates if document is checked out by another user.
        /// </summary>
        /// <param name="node">Document</param>
        public static bool IsDocumentCheckedOutByAnotherUser(TreeNode node)
        {
            int userID = node.DocumentCheckedOutByUserID;

            if ((userID > 0) && (MembershipContext.AuthenticatedUser.UserID != userID))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Gets resource hash MD5 ('UserName'_'URL').
        /// </summary>
        /// <param name="path">URL path</param>
        public static string GetResourceHash(string path)
        {
            if (String.IsNullOrEmpty(path))
            {
                path = RequestContext.RawURL;
            }

            string input = MembershipContext.AuthenticatedUser.UserName + "_" + path;

            // Calculate MD5 hash from input
            MD5 md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);

            // Convert byte array to hex string
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }

            return sb.ToString();
        }


        /// <summary>
        /// Gets media directory info.
        /// </summary>
        /// <param name="library">Media library info</param>
        /// <param name="filePath">File path</param>
        public static DirectoryInfo GetMediaDirectoryInfo(MediaLibraryInfo library, string filePath)
        {
            DirectoryInfo directoryInfo = null;

            if (library != null)
            {
                string libraryPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(library.LibraryID);

                string basePath = libraryPath;
                if (filePath != null)
                {
                    filePath = Path.EnsureBackslashes(filePath, true);
                    basePath += "\\" + filePath;
                }

                if (Directory.Exists(basePath))
                {
                    directoryInfo = DirectoryInfo.New(basePath);
                }
            }

            return directoryInfo;
        }


        /// <summary>
        /// Gets site name.
        /// </summary>
        /// <param name="siteID">Site ID</param>
        public static string GetSiteName(int siteID)
        {
            SiteInfo siteInfo = SiteInfoProvider.GetSiteInfo(siteID);

            if (siteInfo != null)
            {
                return siteInfo.SiteName;
            }

            return null;
        }


        /// <summary>
        /// Indicates if document is under workflow.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="tree">Tree provider</param>
        public static bool UseWorkflow(TreeNode node, TreeProvider tree)
        {
            if (node != null)
            {
                if (tree == null)
                {
                    tree = new TreeProvider(MembershipContext.AuthenticatedUser);
                }

                WorkflowManager workflowMan = WorkflowManager.GetInstance(tree);

                return (workflowMan.GetNodeWorkflow(node) != null);
            }

            return false;
        }


        /// <summary>
        /// Returns True if current user has permission, otherwise returns False.
        /// </summary>
        /// <param name="mediaLibrary">Media library</param>
        /// <param name="permission">Permission code name</param>
        public static bool IsCurrentUserAuthorizedPerMediaLibrary(MediaLibraryInfo mediaLibrary, string permission)
        {
            if (!String.IsNullOrEmpty(permission))
            {
                return IsCurrentUserAuthorizedPerMediaLibrary(mediaLibrary, new[] { permission });
            }

            return false;
        }


        /// <summary>
        /// Returns True if current user is authorized at least on one permission, otherwise returns False.
        /// </summary>
        /// <param name="mediaLibrary">Media library</param>
        /// <param name="permissions">Permission array</param>
        public static bool IsCurrentUserAuthorizedPerMediaLibrary(MediaLibraryInfo mediaLibrary, string[] permissions)
        {
            if ((mediaLibrary != null) && (permissions != null))
            {
                var currentUser = MembershipContext.AuthenticatedUser;

                foreach (string permission in permissions)
                {
                    if (MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(mediaLibrary, permission, currentUser))
                    {
                        return true;
                    }
                }
            }

            return false;
        }


        /// <summary>
        /// Indicates if the current user is group administrator.
        /// </summary>
        /// <param name="groupInfo">Group info</param>
        public static bool IsGroupAdministrator(GeneralizedInfo groupInfo)
        {
            if (groupInfo != null)
            {
                // Check if user is group administrator
                return MembershipContext.AuthenticatedUser.IsGroupAdministrator(ValidationHelper.GetInteger(groupInfo.GetValue("GroupID"), 0));
            }

            return false;
        }


        /// <summary>
        /// Indicates if the current user is authorized per document.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="permission">Permisssion to check</param>
        public static bool IsCurrentUserAuthorizedPerDocument(TreeNode node, NodePermissionsEnum permission)
        {
            return IsCurrentUserAuthorizedPerDocument(node, new[] { permission });
        }


        /// <summary>
        /// Indicates if the current user is authorized per document.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="permissions">Permisssions to check</param>
        public static bool IsCurrentUserAuthorizedPerDocument(TreeNode node, NodePermissionsEnum[] permissions)
        {
            if (node != null)
            {
                return (MembershipContext.AuthenticatedUser.IsAuthorizedPerDocument(node, permissions) == AuthorizationResultEnum.Allowed);
            }

            return false;
        }


        /// <summary>
        /// Creates the synchronization task if document doesn't use workflow.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="taskType">Task type</param>
        /// <param name="tree">TreeProvider to use</param>
        /// <returns>Returns new synchronization task</returns>
        public static void LogSynchronization(TreeNode node, TaskTypeEnum taskType, TreeProvider tree)
        {
            // Log synchronization task if document doesn't use workflow
            if (!UseWorkflow(node, tree))
            {
                DocumentSynchronizationHelper.LogDocumentChange(node, taskType, tree);
            }
        }


        /// <summary>
        /// Indicates if source and destionation attachment folders are the same.
        /// </summary>
        /// <param name="sourceParser">Source URL parser</param>
        /// <param name="destParser">Destination URL parser</param>
        public static bool IsSameAttachmentFolder(UrlParser sourceParser, UrlParser destParser)
        {
            if ((sourceParser != null) && (destParser != null))
            {
                return ((sourceParser.AliasPath == destParser.AliasPath) && (sourceParser.CultureCode == destParser.CultureCode)
                        && (sourceParser.FieldName == destParser.FieldName) && (sourceParser.GroupName == destParser.GroupName));
            }

            return false;
        }


        /// <summary>
        /// Indicates if attachment is stored in file system.
        /// </summary>
        /// <param name="node">Document</param>
        public static bool IsAttachmentStoredInFileSystem(TreeNode node)
        {
            if (node == null)
            {
                return false;
            }

            int versionHistoryId = node.DocumentCheckedOutVersionHistoryID;
            var filesLocationType = FileHelper.FilesLocationType(SiteContext.CurrentSiteName);

            // Ensure physical file path
            return ((versionHistoryId == 0) && (filesLocationType != FilesLocationTypeEnum.Database));
        }


        /// <summary>
        /// Indicates if new file name is temporary (contains prefix '~$').
        /// </summary>
        /// <param name="fileName">Current file name</param>
        /// <param name="newFileName">New file name</param>
        public static bool IsTemporaryFile(string fileName, string newFileName)
        {
            string tempFileName = TEMPORARY_FILE_PREFIX + fileName;
            return (CMSString.Compare(tempFileName, newFileName, true) == 0);
        }

        #endregion


        #region "Internal Methods"

        /// <summary>
        /// Gets existing attachment guid.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="tree">Tree provider</param>
        /// <param name="attachmentName">Attachment name</param>
        /// <param name="fieldName">Field name</param>
        /// <returns>Existing attachment GUID</returns>
        internal static Guid GetExistingAttachmentGuid(TreeNode node, TreeProvider tree, string attachmentName, string fieldName)
        {
            Guid existingGuid = Guid.Empty;

            if (node != null)
            {
                // Prepare where condition
                string where = "AttachmentName = N'" + SqlHelper.GetSafeQueryString(attachmentName, false) + "'";

                // Unsorted attachment
                if (String.IsNullOrEmpty(fieldName))
                {
                    @where = SqlHelper.AddWhereCondition(@where, "AttachmentIsUnsorted = 1");
                }
                else
                {
                    @where = SqlHelper.AddWhereCondition(@where, "AttachmentIsUnsorted IS NULL OR AttachmentIsUnsorted = 0");

                    FormFieldInfo destFfi = GetFormFieldInfo(node, fieldName);
                    if (destFfi != null)
                    {
                        if (destFfi.DataType == DocumentFieldDataType.DocAttachments)
                        {
                            // Add AttachmentGroupGUID condition
                            @where = SqlHelper.AddWhereCondition(@where, "AttachmentGroupGUID = N'" + destFfi.Guid + "'");
                        }
                        else
                        {
                            // Add AttachmentGroupGUID condition
                            @where = SqlHelper.AddWhereCondition(@where, "AttachmentGroupGUID IS NULL");
                        }
                    }
                }

                var attDs = 
                    DocumentHelper.GetAttachments(node, true)
                        .ApplySettings(settings => settings
                            .Columns("AttachmentGUID")
                            .Where(new WhereCondition(@where))
                            .TopN(1)
                        )
                        .Result;

                if (!DataHelper.DataSourceIsEmpty(attDs))
                {
                    existingGuid = ValidationHelper.GetGuid(attDs.Tables[0].Rows[0]["AttachmentGUID"], Guid.Empty);
                }
            }

            return existingGuid;
        }


        /// <summary>
        /// Gets new or existing attachment info.
        /// </summary>
        /// <param name="destNode">Destination document</param>
        /// <param name="sourceParser">Source Url parser</param>
        /// <param name="destParser">Destination Url parser</param>
        /// <param name="newSafeName">New safe attachment name</param>
        internal static DocumentAttachment GetExistingOrNewAttachment(TreeNode destNode, UrlParser sourceParser, UrlParser destParser, string newSafeName)
        {
            DocumentAttachment newAttachment = null;
            var sourceAttachmentInfo = sourceParser.Attachment;

            var tree = destParser.TreeProvider;

            if ((destNode != null) && (sourceAttachmentInfo != null))
            {
                // Get existing attachment GUID
                Guid existingAttachmentGuid = GetExistingAttachmentGuid(destNode, tree, newSafeName, destParser.FieldName);

                if (existingAttachmentGuid == Guid.Empty)
                {
                    // Rename current attachment
                    newAttachment = sourceAttachmentInfo;

                    // If source and destination folder are different, clone source attachment.
                    if (!IsSameAttachmentFolder(sourceParser, destParser))
                    {
                        newAttachment = sourceAttachmentInfo.Clone(true);
                    }
                }
                else
                {
                    // Clone source attachment and set existing guid
                    newAttachment = sourceAttachmentInfo.Clone(true);
                    newAttachment.AttachmentGUID = existingAttachmentGuid;
                }

                // Set new name and other properties
                newAttachment.AttachmentName = newSafeName;
                newAttachment.AttachmentExtension = Path.GetExtension(newSafeName);
                newAttachment.AttachmentMimeType = MimeTypeHelper.GetMimetype(newAttachment.AttachmentExtension);
                newAttachment.AttachmentLastModified = DateTime.Now;
            }

            return newAttachment;
        }


        /// <summary>
        /// Creates attachment info from media file info.
        /// </summary>
        /// <param name="attachmentGuid">Attachment GUID</param>
        /// <param name="fileName">File name</param>
        /// <param name="mediaFileResource">Media file resource</param>
        /// <param name="mediaLibraryInfo">Media library info</param>
        /// <returns>New attachment info</returns>
        internal static DocumentAttachment CreateAttachmentFromMediaFileInfo(Guid attachmentGuid, string fileName, MediaFileResource mediaFileResource, MediaLibraryInfo mediaLibraryInfo)
        {
            if ((mediaFileResource != null) && (mediaLibraryInfo != null))
            {
                var newAttachment = new DocumentAttachment();

                // Set properties
                newAttachment.AttachmentGUID = (attachmentGuid == Guid.Empty) ? Guid.NewGuid() : attachmentGuid;
                newAttachment.AttachmentName = fileName;
                newAttachment.AttachmentExtension = Path.GetExtension(fileName);
                newAttachment.AttachmentMimeType = MimeTypeHelper.GetMimetype(newAttachment.AttachmentExtension);
                newAttachment.AttachmentLastModified = DateTime.Now;
                newAttachment.AttachmentSiteID = mediaLibraryInfo.LibrarySiteID;

                MediaFileInfo mediaFileInfo = mediaFileResource.UrlParser.MediaFileInfo;

                // Set properties from media files in the DB
                if (mediaFileInfo != null)
                {
                    // Set height and width
                    if ((mediaFileInfo.FileImageHeight > 0) || (mediaFileInfo.FileImageWidth > 0))
                    {
                        newAttachment.AttachmentImageHeight = mediaFileInfo.FileImageHeight;
                        newAttachment.AttachmentImageWidth = mediaFileInfo.FileImageWidth;
                    }

                    // Set title and description
                    newAttachment.AttachmentTitle = mediaFileInfo.FileTitle;
                    newAttachment.AttachmentDescription = mediaFileInfo.FileDescription;
                    newAttachment.AttachmentCustomData.LoadData(mediaFileInfo.FileCustomData.GetData());
                }

                return newAttachment;
            }

            return null;
        }


        /// <summary>
        /// Gets Form field info.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="fieldName">Field name</param>
        internal static FormFieldInfo GetFormFieldInfo(TreeNode node, string fieldName)
        {
            FormInfo fi = GetFormInfo(node);

            if ((fi != null) && !String.IsNullOrEmpty(fieldName))
            {
                return fi.GetFormField(fieldName);
            }

            return null;
        }


        /// <summary>
        /// Gets form info.
        /// </summary>
        /// <param name="node">Document</param>
        internal static FormInfo GetFormInfo(TreeNode node)
        {
            if (node != null)
            {
                DataClassInfo dci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(node.NodeClassName);

                if (dci != null)
                {
                    return FormHelper.GetFormInfo(dci.ClassName, true);
                }
            }

            return null;
        }


        /// <summary>
        /// Indicates if form field info allows empty value.
        /// </summary>
        /// <param name="node">Document</param>
        /// <param name="fieldName">Field name</param>
        internal static bool IsAllowEmptyFormFieldInfo(TreeNode node, string fieldName)
        {
            FormFieldInfo ffi = GetFormFieldInfo(node, fieldName);
            return ((ffi != null) && ffi.AllowEmpty);
        }


        /// <summary>
        /// Gets resized image binary data, width and height.
        /// </summary>
        /// <param name="ih">Instance of ImageHelper</param>
        /// <param name="width">Required width</param>
        /// <param name="height">Required height</param>
        /// <param name="maxSideSize">Required width max side size</param>
        /// <param name="newHeight">New image width</param>
        /// <param name="newWidth">New image height</param>
        internal static byte[] GetResizedImageData(ImageHelper ih, int width, int height, int maxSideSize, out int newWidth, out int newHeight)
        {
            int[] dimensions = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, ih.ImageWidth, ih.ImageHeight);
            newWidth = dimensions[0];
            newHeight = dimensions[1];
            return ih.GetResizedImageData(newWidth, newHeight, MediaFileInfoProvider.ThumbnailQuality);
        }


        /// <summary>
        /// Cleans media file binary data.
        /// </summary>
        /// <param name="mediaFile">Media file whose data should be cleared</param>
        internal static void CleanBinaryData(MediaFileInfo mediaFile)
        {
            if (mediaFile != null)
            {
                mediaFile.FileBinary = null;
                if (mediaFile.FileBinaryStream != null)
                {
                    mediaFile.FileBinaryStream.Close();
                    mediaFile.FileBinaryStream.Dispose();
                    mediaFile.FileBinaryStream = null;
                }
            }
        }


        /// <summary>
        /// Cleans attachment binary data.
        /// </summary>
        /// <param name="attachment">Attachment info</param>
        internal static void CleanBinaryData(DocumentAttachment attachment)
        {
            if (attachment != null)
            {
                // Clean binary data
                attachment.AttachmentBinary = null;                
            }
        }


        /// <summary>
        /// Returns file info of media file that is stored in file system.
        /// </summary>
        /// <param name="library">Media library</param>
        /// <param name="mediaFilePath">Media file path</param>
        internal static FileInfo GetFileInfo(MediaLibraryInfo library, string mediaFilePath)
        {
            if (library != null)
            {
                string filesFolderPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(SiteContext.CurrentSiteName, library.LibraryFolder);
                string path = filesFolderPath + "/" + mediaFilePath;

                if (File.Exists(path))
                {
                    return FileInfo.New(path);
                }
            }

            return null;
        }


        /// <summary>
        /// Indicates if current user can approve custom step. If document doesn't use workflow, returns True.
        /// </summary>
        /// <param name="node">Document</param>
        internal static bool CanCurrentUserApproveCustomStep(TreeNode node)
        {
            var manager = node.WorkflowManager;
            var workflow = manager.GetNodeWorkflow(node);
            if (workflow == null)
            {
                return true;
            }

            var currentStep = manager.GetStepInfo(node);
            if ((currentStep == null) || currentStep.StepIsDefault)
            {
                return true;
            }

            return manager.CheckStepPermissions(node, MembershipContext.AuthenticatedUser, WorkflowActionEnum.Approve);
        }

        #endregion
    }
}

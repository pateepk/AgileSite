using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;

namespace CMS.WebDAV
{
    /// <summary>
    /// WebDAV settings class.
    /// </summary>
    public static class WebDAVSettings
    {
        #region "Variables"

        private static string mRootFolder = null;
        private static string mAttachmentsFolder = "attachments";
        private static string mMediaFilesFolder = "media";
        private static string mMetaFilesFolder = "metafiles";
        private static string mContentFilesFolder = "content";
        private static string mGroupsFolder = "groups";
        private static string mPagesFolder = "pages";
        private static string mUnsortedFolder = "[_unsorted]";

        private static string mBasePath = "/cms/files/";
        private static string mAttachmentsBasePath = mBasePath + mAttachmentsFolder;
        private static string mMediaFilesBasePath = mBasePath + mMediaFilesFolder;
        private static string mMetaFilesBasePath = mBasePath + mMetaFilesFolder;
        private static string mContentFilesBasePath = mBasePath + mContentFilesFolder;
        private static string mGroupsBasePath = mBasePath + mGroupsFolder;

        #endregion


        #region "Folder name properties"

        /// <summary>
        /// Name of root folder (from BasePath), for example 'files'.
        /// </summary>
        public static string RootFolder
        {
            get
            {
                if (mRootFolder == null)
                {
                    string basePath = BasePath.Trim('/');
                    int index = basePath.Contains("/") ? basePath.LastIndexOfCSafe('/') + 1 : 0;
                    mRootFolder = basePath.Substring(index);
                }

                return mRootFolder;
            }
        }


        /// <summary>
        /// Name of the attachments folder, for example: 'attachments'.
        /// </summary>
        public static string AttachmentsFolder
        {
            get
            {
                return mAttachmentsFolder;
            }
            set
            {
                mAttachmentsFolder = value;
            }
        }


        /// <summary>
        /// Name of the media files folder, for example: 'media'.
        /// </summary>
        public static string MediaFilesFolder
        {
            get
            {
                return mMediaFilesFolder;
            }
            set
            {
                mMediaFilesFolder = value;
            }
        }


        /// <summary>
        /// Name of the meta files folder, for example: 'metafiles'.
        /// </summary>
        public static string MetaFilesFolder
        {
            get
            {
                return mMetaFilesFolder;
            }
            set
            {
                mMetaFilesFolder = value;
            }
        }


        /// <summary>
        /// Name of the content files under community group folder, for example: 'pages'.
        /// </summary>
        public static string PagesFolder
        {
            get
            {
                return mPagesFolder;
            }
            set
            {
                mPagesFolder = value;
            }
        }


        /// <summary>
        /// Name of the content files folder, for example: 'content'.
        /// </summary>
        public static string ContentFilesFolder
        {
            get
            {
                return mContentFilesFolder;
            }
            set
            {
                mContentFilesFolder = value;
            }
        }


        /// <summary>
        /// Name of the unsorted attachments folder, for example: '[unsorted]'.
        /// </summary>
        public static string UnsortedFolder
        {
            get
            {
                return mUnsortedFolder;
            }
            set
            {
                mUnsortedFolder = value;
            }
        }


        /// <summary>
        /// Name of the community groups folder, for example: 'groups'.
        /// </summary>
        public static string GroupsFolder
        {
            get
            {
                return mGroupsFolder;
            }
            set
            {
                mGroupsFolder = value;
            }
        }

        #endregion


        #region "Path properties"

        /// <summary>
        /// Gets or sets base path, for example: '/cms/files/'. 
        /// </summary>
        public static string BasePath
        {
            get
            {
                return mBasePath;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    mBasePath = value.Trim('/') + "/";
                }
            }
        }


        /// <summary>
        /// Gets base attachments path.
        /// </summary>
        public static string AttachmentsBasePath
        {
            get
            {
                return BasePath + AttachmentsFolder;
            }
        }


        /// <summary>
        /// Gets base media files path.
        /// </summary>
        public static string MediaFilesBasePath
        {
            get
            {
                return BasePath + MediaFilesFolder;
            }
        }


        /// <summary>
        /// Gets base meta files path.
        /// </summary>
        public static string MetaFilesBasePath
        {
            get
            {
                return BasePath + MetaFilesFolder;
            }
        }


        /// <summary>
        /// Gets base content files path.
        /// </summary>
        public static string ContentFilesBasePath
        {
            get
            {
                return BasePath + ContentFilesFolder;
            }
        }


        /// <summary>
        /// Gets base groups path.
        /// </summary>
        public static string GroupsBasePath
        {
            get
            {
                return BasePath + GroupsFolder;
            }
        }

        #endregion


        #region "Node alias path replacement constants"

        /// <summary>
        /// Replacement for root node alias path.
        /// </summary>
        public const string ALIASPATH_ROOT = "_";

        #endregion


        #region "Automatic image resize constants"

        /// <summary>
        /// Image resize width.
        /// </summary>
        public const string WIDTH = "width";


        /// <summary>
        /// Image resize height.
        /// </summary>
        public const string HEIGHT = "height";


        /// <summary>
        /// Image resize maximum side size.
        /// </summary>
        public const string MAX_SIDE_SIZE = "maxSideSize";

        #endregion

        
        #region "Public Methods"

        /// <summary>
        /// Gets allowed file extensions supported by WebDAV for given site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetAllowedExtensions(string siteName)
        {
            return SettingsKeyInfoProvider.GetValue(siteName + ".CMSWebDAVExtensions");
        }


        /// <summary>
        /// Checks if file extension is allowed for edit mode.
        /// </summary>
        /// <param name="extension">File extension</param>
        /// <param name="siteName">Site name</param>
        public static bool IsExtensionAllowedForEditMode(string extension, string siteName)
        {
            // Files without extension are not supported
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            // Get allowed extensions
            string extensions = GetAllowedExtensions(siteName);

            // Setting is empty -> all extensions are allowed
            if (string.IsNullOrEmpty(extensions))
            {
                return true;
            }

            extensions = ";" + extensions.ToLowerCSafe() + ";";
            // Remove '.' from the beginning of file extension if it's present
            extension = extension.ToLowerCSafe().TrimStart('.');

            return (extensions.Contains(";" + extension + ";") || extensions.Contains(";." + extension + ";"));
        }

        #endregion
    }
}
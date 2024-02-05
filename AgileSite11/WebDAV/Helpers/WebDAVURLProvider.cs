using System;
using System.Text;

using CMS.Base;
using CMS.Helpers;

namespace CMS.WebDAV
{
    /// <summary>
    /// URL Provider for WebDAV access
    /// </summary>
    public class WebDAVURLProvider : AbstractBaseProvider<WebDAVURLProvider>
    {
        #region "Methods"
        
        /// <summary>
        /// Gets attachment file relative URL for WebDAV module.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Document culture code</param>
        /// <param name="fieldName">Field name for grouped attachment (optional)</param>
        /// <param name="fileName">File name</param>
        /// <param name="groupName">Community group name (optional)</param>
        public static string GetAttachmentWebDAVUrl(string siteName, string aliasPath, string cultureCode, string fieldName, string fileName, string groupName)
        {
            return ProviderObject.GetAttachmentWebDAVUrlInternal(siteName, aliasPath, cultureCode, fieldName, fileName, groupName);
        }


        /// <summary>
        /// Gets content file relative URL for WebDAV module.
        /// </summary>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Node culture code</param>
        /// <param name="groupName">Community group name (optional)</param>
        public static string GetContentFileWebDAVUrl(string aliasPath, string cultureCode, string groupName)
        {
            return ProviderObject.GetContentFileWebDAVUrlInternal(aliasPath, cultureCode, groupName);
        }


        /// <summary>
        /// Gets media file relative URL for WebDAV.
        /// </summary>
        /// <param name="libraryName">Library name</param>
        /// <param name="filePath">File path</param>
        /// <param name="groupName">Community group name (optional)</param>
        public static string GetMediaFileWebDAVUrl(string libraryName, string filePath, string groupName)
        {
            return ProviderObject.GetMediaFileWebDAVUrlInternal(libraryName, filePath, groupName);
        }
        

        /// <summary>
        /// Gets meta file relative URL.
        /// </summary>
        /// <param name="GUID">Meta file GUID</param>
        /// <param name="fileName">Meta file name</param>
        /// <param name="siteName">Site name</param>
        public static string GetMetaFileWebDAVUrl(Guid GUID, string fileName, string siteName)
        {
            return ProviderObject.GetMetaFileWebDAVUrlInternal(GUID, fileName, siteName);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets attachment file relative URL for WebDAV module.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Document culture code</param>
        /// <param name="fieldName">Field name for grouped attachment (optional)</param>
        /// <param name="fileName">File name</param>
        /// <param name="groupName">Community group name (optional)</param>
        protected virtual string GetAttachmentWebDAVUrlInternal(string siteName, string aliasPath, string cultureCode, string fieldName, string fileName, string groupName)
        {
            // Get safe filename
            fileName = URLHelper.GetSafeFileName(fileName, siteName);

            // If no field is specified, unsorted attachment is required
            if (fieldName == null)
            {
                fieldName = WebDAVSettings.UnsortedFolder;
            }

            StringBuilder sb = new StringBuilder();

            // Community group is provided, generate path for community group attachment
            if (!String.IsNullOrEmpty(groupName))
            {
                sb.Append("~" + WebDAVSettings.GroupsBasePath);
                sb.Append("/" + groupName);
                sb.Append("/" + WebDAVSettings.AttachmentsFolder);
            }
            // Get standard attachment path
            else
            {
                sb.Append("~" + WebDAVSettings.AttachmentsBasePath);
            }

            if (!String.IsNullOrEmpty(cultureCode))
            {
                sb.Append("/" + cultureCode);
            }
            if (!String.IsNullOrEmpty(aliasPath))
            {
                // Do not include root alias path
                if (aliasPath != "/")
                {
                    sb.Append(aliasPath);
                }
            }

            if (CMSString.Compare(fieldName, WebDAVSettings.UnsortedFolder, true) == 0)
            {
                sb.Append("/" + fieldName);
            }
            else
            {
                sb.Append("/[" + fieldName + "]");
            }

            if (!String.IsNullOrEmpty(fileName))
            {
                sb.Append("/" + fileName);
            }

            return sb.ToString();
        }


        /// <summary>
        /// Gets content file relative URL for WebDAV module.
        /// </summary>
        /// <param name="aliasPath">Node alias path</param>
        /// <param name="cultureCode">Node culture code</param>
        /// <param name="groupName">Community group name (optional)</param>
        protected virtual string GetContentFileWebDAVUrlInternal(string aliasPath, string cultureCode, string groupName)
        {
            StringBuilder sb = new StringBuilder();

            // Community group is provided, generate path for community group content file
            if (!String.IsNullOrEmpty(groupName))
            {
                sb.Append("~" + WebDAVSettings.GroupsBasePath);
                sb.Append("/" + groupName);
                sb.Append("/" + WebDAVSettings.PagesFolder);
            }
            // Get standard content file path
            else
            {
                sb.Append("~" + WebDAVSettings.ContentFilesBasePath);
            }

            if (!String.IsNullOrEmpty(cultureCode))
            {
                sb.Append("/" + cultureCode);
            }
            if (!String.IsNullOrEmpty(aliasPath))
            {
                // Do not include root alias path
                if (aliasPath != "/")
                {
                    sb.Append(aliasPath);
                }
            }

            return sb.ToString();
        }


        /// <summary>
        /// Gets media file relative URL for WebDAV.
        /// </summary>
        /// <param name="libraryName">Library name</param>
        /// <param name="filePath">File path</param>
        /// <param name="groupName">Community group name (optional)</param>
        protected virtual string GetMediaFileWebDAVUrlInternal(string libraryName, string filePath, string groupName)
        {
            StringBuilder sb = new StringBuilder();

            // Community group is provided, generate path for community group media file
            if (!String.IsNullOrEmpty(groupName))
            {
                sb.Append("~" + WebDAVSettings.GroupsBasePath);
                sb.Append("/" + groupName);
                sb.Append("/" + WebDAVSettings.MediaFilesFolder);
            }
            // Get standard media file path
            else
            {
                sb.Append("~" + WebDAVSettings.MediaFilesBasePath);
            }

            if (!String.IsNullOrEmpty(libraryName))
            {
                sb.Append("/" + libraryName);
            }
            if (!String.IsNullOrEmpty(filePath))
            {
                sb.Append("/" + filePath);
            }

            return sb.ToString();
        }

        
        /// <summary>
        /// Gets meta file relative URL.
        /// </summary>
        /// <param name="GUID">Meta file GUID</param>
        /// <param name="fileName">Meta file name</param>
        /// <param name="siteName">Site name</param>
        protected virtual string GetMetaFileWebDAVUrlInternal(Guid GUID, string fileName, string siteName)
        {
            // Get safe filename
            fileName = URLHelper.GetSafeFileName(fileName, siteName);

            StringBuilder sb = new StringBuilder();

            sb.Append("~" + WebDAVSettings.MetaFilesBasePath);

            if (GUID != Guid.Empty)
            {
                sb.Append("/" + GUID);
            }
            if (!string.IsNullOrEmpty(fileName))
            {
                sb.Append("/" + fileName);
            }

            return sb.ToString();
        }

        #endregion
    }
}

using System;
using System.Web;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;
using CMS.DataEngine;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// URL provider for the media files.
    /// </summary>
    public class MediaFileURLProvider : AbstractBaseProvider<MediaFileURLProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns relative URL path to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="fileInfo">Media file info object</param>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        public static string GetMediaFileUrl(MediaFileInfo fileInfo, string siteName, string libraryFolder)
        {
            if (fileInfo != null)
            {
                return ProviderObject.GetMediaFileUrlInternal(fileInfo, siteName, libraryFolder, fileInfo.FilePath);
            }

            return null;
        }


        /// <summary>
        /// Returns relative URL path to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="filePath">File path within the library folder</param>
        public static string GetMediaFileUrl(string siteName, string libraryFolder, string filePath)
        {
            return ProviderObject.GetMediaFileUrlInternal(null, siteName, libraryFolder, filePath);
        }


        /// <summary>
        /// Returns relative URL path to the media file which is rewritten to calling GetMediaFile.aspx page where user permissions are checked.
        /// </summary>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        public static string GetMediaFileUrl(Guid fileGuid, string fileName)
        {
            return ProviderObject.GetMediaFileUrlInternal(fileGuid, fileName);
        }


        /// <summary>
        /// Returns absolute URL path to the media file including http://, user permissions are not checked
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="filePath">File path within the library folder</param>
        public static string GetMediaFileAbsoluteUrl(string siteName, string libraryFolder, string filePath)
        {
            return ProviderObject.GetMediaFileAbsoluteUrlInternal(siteName, libraryFolder, filePath);
        }


        /// <summary>
        /// Returns absolute URL path to the media file including http:// which is rewritten to calling GetMediaFile.aspx page where user permissions are checked
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        public static string GetMediaFileAbsoluteUrl(string siteName, Guid fileGuid, string fileName)
        {
            return ProviderObject.GetMediaFileAbsoluteUrlInternal(siteName, fileGuid, fileName);
        }


        /// <summary>
        /// Returns absolute URL path for current domain to the media file including http:// which is rewritten to calling GetMediaFile.aspx page where user permissions are checked
        /// </summary>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        public static string GetMediaFileAbsoluteUrl(Guid fileGuid, string fileName)
        {
            return GetMediaFileAbsoluteUrl(null, fileGuid, fileName);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns relative URL path to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="fileInfo">Media file info object</param>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="filePath">File path within the library folder</param>
        protected virtual string GetMediaFileUrlInternal(MediaFileInfo fileInfo, string siteName, string libraryFolder, string filePath)
        {
            if (!String.IsNullOrEmpty(siteName) && !String.IsNullOrEmpty(libraryFolder) && !String.IsNullOrEmpty(filePath))
            {
                if (MediaLibraryHelper.IsExternalLibrary(siteName, libraryFolder))
                {
                    MediaFileInfo mfi = fileInfo;
                    if (mfi == null)
                    {
                        mfi = MediaFileInfoProvider.GetMediaFileInfo(siteName, filePath, libraryFolder);
                    }

                    if (mfi != null)
                    {
                        return GetMediaFileUrlInternal(mfi.FileGUID, mfi.FileName);
                    }
                }

                // Create library root piece of URL
                string libraryRoot = MediaLibraryHelper.GetMediaLibrariesFolder(siteName);
                if (String.IsNullOrEmpty(libraryRoot))
                {
                    // Use site name folder always to ensure backward compatibility when no custom root folder path is specified
                    libraryRoot = String.Format("~/{0}/media", siteName);
                }
                else
                {
                    if (!libraryRoot.StartsWithCSafe("~/"))
                    {
                        // Avoid to have '~/~' in the path
                        libraryRoot = "~/" + libraryRoot.TrimStart('~');
                    }

                    // Check if site specific folder should be used
                    if (MediaLibraryHelper.UseMediaLibrariesSiteFolder(siteName))
                    {
                        libraryRoot = String.Format("{0}/{1}", libraryRoot.TrimEnd('/'), siteName);
                    }
                }

                // Create file path piece of URL
                filePath = String.IsNullOrEmpty(libraryFolder) ? filePath.Trim('/') : String.Format("{0}/{1}", libraryFolder.Trim('/'), filePath);

                //Escape special characters for URL 
                filePath = URLHelper.EscapeSpecialCharacters(filePath);

                string relative = String.Format("{0}/{1}", libraryRoot.TrimEnd('/'), HttpUtility.UrlPathEncode(filePath));

                string result = GetExternalStorageUrl(relative, siteName);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                return relative;
            }

            return null;
        }


        /// <summary>
        /// Returns relative URL path to the media file which is rewritten to calling GetMediaFile.aspx page where user permissions are checked.
        /// </summary>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        protected virtual string GetMediaFileUrlInternal(Guid fileGuid, string fileName)
        {
            if ((fileGuid != Guid.Empty) && !String.IsNullOrEmpty(fileName))
            {
                return String.Format("~/getmedia/{0}/{1}{2}", Convert.ToString(fileGuid), HttpUtility.UrlPathEncode(fileName), SettingsKeyInfoProvider.GetFilesUrlExtension(SiteContext.CurrentSiteName));
            }

            return null;
        }


        /// <summary>
        /// Returns absolute URL path to the media file including http://, user permissions are not checked
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="filePath">File path within the library folder</param>
        protected virtual string GetMediaFileAbsoluteUrlInternal(string siteName, string libraryFolder, string filePath)
        {
            if (String.IsNullOrEmpty(siteName) || String.IsNullOrEmpty(libraryFolder) || String.IsNullOrEmpty(filePath))
            {
                return null;
            }

            if (MediaLibraryHelper.IsExternalLibrary(siteName, libraryFolder))
            {
                MediaFileInfo mfi = MediaFileInfoProvider.GetMediaFileInfo(siteName, filePath, libraryFolder);
                if (mfi != null)
                {
                    return GetMediaFileAbsoluteUrl(siteName, mfi.FileGUID, AttachmentHelper.GetFullFileName(mfi.FileName, mfi.FileExtension));
                }
            }

            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteName);
            if (site != null)
            {
                string relative = GetMediaFileUrl(siteName, libraryFolder, filePath);

                string result = GetExternalStorageUrl(relative, siteName);
                if (!string.IsNullOrEmpty(result))
                {
                    return result;
                }

                return GetAbsoluteUrl(relative, site);
            }

            return null;
        }


        /// <summary>
        /// Returns absolute URL path to the media file including http:// which is rewritten to calling GetMediaFile.aspx page where user permissions are checked
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        protected virtual string GetMediaFileAbsoluteUrlInternal(string siteName, Guid fileGuid, string fileName)
        {
            if ((fileGuid != Guid.Empty) && !String.IsNullOrEmpty(fileName))
            {
                string relative = GetMediaFileUrl(fileGuid, fileName);
                SiteInfo site = SiteInfoProvider.GetSiteInfo(siteName);
                if (site != null)
                {
                    return GetAbsoluteUrl(relative, site);
                }
                else
                {
                    return URLHelper.GetAbsoluteUrl(relative, RequestContext.FullDomain);
                }
            }

            return null;
        }

        #endregion


        #region "Private methods"

        private static string GetAbsoluteUrl(string relativeUrl, SiteInfo site)
        {
            if (site.SiteIsContentOnly)
            {
                // Public CDN URLs are already in an absolute form
                if (!relativeUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && !relativeUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    return URLHelper.CombinePath(relativeUrl, '/', site.SitePresentationURL, null);
                }

                return relativeUrl;
            }
            else
            {
                return URLHelper.GetAbsoluteUrl(relativeUrl, site.DomainName);
            }
        }


        /// <summary>
        /// Returns URL for External storage or null.
        /// </summary>
        /// <param name="relative">Relative path from root.</param>
        /// <param name="siteName">Site name.</param>
        private string GetExternalStorageUrl(string relative, string siteName)
        {
            // Absolute URL - external storage
            if (relative.StartsWithCSafe("http://", true) || relative.StartsWithCSafe("https://", true))
            {
                return relative;
            }
            
            // Remove starting "~"
            if (relative.StartsWithCSafe("~"))
            {
                relative = relative.Substring(1);
            }

            if (!StorageHelper.IsExternalStorage(relative))
            {
                return null;
            }

            if (string.IsNullOrEmpty(relative))
            {
                return null;
            }

            // If URL contains national letter then is URL encoded - decode it to original format
            if (CMSHttpContext.Current != null)
            {
                relative = CMSHttpContext.Current.Server.UrlDecode(relative);
            }

            return File.GetFileUrl(relative, siteName);
        }

        #endregion
    }
}
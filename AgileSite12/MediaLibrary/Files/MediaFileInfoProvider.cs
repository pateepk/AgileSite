using System;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Class providing media file info management.
    /// </summary>
    public class MediaFileInfoProvider : AbstractInfoProvider<MediaFileInfo, MediaFileInfoProvider>
    {
        #region "Variables"

        /// <summary>
        /// Lock object for ensuring of the physical files.
        /// </summary>
        private static readonly object ensureFileLock = new object();


        /// <summary>
        /// Gets the maximal file size which is allowed for media files staging synchronization.
        /// </summary>
        private static readonly CMSLazy<long> mMaxStagingFileSize = new CMSLazy<long>(() => ValidationHelper.GetLong(SettingsHelper.AppSettings["CMSMediaFileMaxStagingSize"], int.MaxValue) * 1024);



        /// <summary>
        /// Gets the maximal file size which is allowed for media files versioning.
        /// </summary>
        private static readonly CMSLazy<long> mMaxVersioningFileSize = new CMSLazy<long>(() => ValidationHelper.GetLong(SettingsHelper.AppSettings["CMSMediaFileMaxVersioningSize"], int.MaxValue) * 1024);


        /// <summary>
        /// Thumbnail quality.
        /// </summary>
        private static int? mThumbnailQuality;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Thumbnail quality.
        /// </summary>
        public static int ThumbnailQuality
        {
            get
            {
                // Use default system quality settings
                if (mThumbnailQuality == null)
                {
                    mThumbnailQuality = ImageHelper.DefaultQuality;
                }

                return mThumbnailQuality.Value;
            }

            set
            {
                mThumbnailQuality = value;
            }
        }


        /// <summary>
        /// Gets the maximal file size in bytes which is allowed for media files staging synchronization.
        /// </summary>
        public static long MaxStagingFileSize
        {
            get
            {
                return mMaxStagingFileSize.Value;
            }
        }


        /// <summary>
        /// Gets the maximal file size in bytes which is allowed for media files versioning.
        /// </summary>
        public static long MaxVersioningFileSize
        {
            get
            {
                return mMaxVersioningFileSize.Value;
            }
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns the MediaFileInfo structure for the specified media file.
        /// </summary>
        /// <param name="mediaFileId">Media file ID</param>
        public static MediaFileInfo GetMediaFileInfo(int mediaFileId)
        {
            return ProviderObject.GetInfoById(mediaFileId);
        }


        /// <summary>
        /// Returns media file with specified GUID.
        /// </summary>
        /// <param name="guid">Media file GUID</param>
        /// <param name="siteName">Site name</param>
        public static MediaFileInfo GetMediaFileInfo(Guid guid, string siteName)
        {
            return ProviderObject.GetMediaFileInfoInternal(guid, siteName);
        }


        /// <summary>
        /// Returns the MediaFileInfo structure for the specified media file.
        /// </summary>
        /// <param name="mediaLibraryId">Media library ID</param>
        /// <param name="mediaFilePath">File path</param>
        public static MediaFileInfo GetMediaFileInfo(int mediaLibraryId, string mediaFilePath)
        {
            return ProviderObject.GetMediaFileInfoInternal(mediaLibraryId, mediaFilePath);
        }


        /// <summary>
        /// Returns the MediaFileInfo structure for the specified media file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="mediaFilePath">File path</param>
        /// <param name="libraryFolder">Library folder name</param>
        public static MediaFileInfo GetMediaFileInfo(string siteName, string mediaFilePath, string libraryFolder)
        {
            return ProviderObject.GetMediaFileInfoInternal(siteName, mediaFilePath, libraryFolder);
        }


        /// <summary>
        /// Sets (updates or inserts) specified media file.
        /// </summary>
        /// <param name="mediaFile">Media file to set</param>
        /// <param name="ensureUniqueFileName">Indicates if unique file name should be ensured</param>
        /// <param name="userId">ID of the user performing set action</param>
        public static void SetMediaFileInfo(MediaFileInfo mediaFile, bool ensureUniqueFileName = true, int userId = 0)
        {
            if (userId == 0)
            {
                userId = CMSActionContext.CurrentUser.UserID;
            }

            ProviderObject.SetMediaFileInfoInternal(mediaFile, true, userId, ensureUniqueFileName);
        }


        /// <summary>
        /// Deletes specified media file.
        /// </summary>
        /// <param name="mediaFileId">Media file ID</param>
        public static void DeleteMediaFileInfo(int mediaFileId)
        {
            MediaFileInfo infoObj = GetMediaFileInfo(mediaFileId);
            DeleteMediaFileInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified media file.
        /// </summary>
        /// <param name="infoObj">Media file object</param>
        public static void DeleteMediaFileInfo(MediaFileInfo infoObj)
        {
            ProviderObject.DeleteMediaFileInfoInternal(infoObj);
        }


        /// <summary>
        /// Returns the query for all media files.
        /// </summary>        
        public static ObjectQuery<MediaFileInfo> GetMediaFiles()
        {
            return ProviderObject.GetMediaFilesInternal();
        }


        /// <summary>
        /// Returns dataset of files matching given criteria from database.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="orderBy">ORDER BY parameter</param>
        /// <param name="topN">TOP N parameter</param>
        /// <param name="columns">Selected columns</param>
        public static ObjectQuery<MediaFileInfo> GetMediaFiles(string where, string orderBy = null, int topN = -1, string columns = null)
        {
            return GetMediaFiles().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Import media file into database.
        /// </summary>
        /// <param name="mediaFile">Media file to import</param>
        /// <param name="userId">ID of the user performing set action</param>
        public static void ImportMediaFileInfo(MediaFileInfo mediaFile, int userId = 0)
        {
            if (userId == 0)
            {
                userId = CMSActionContext.CurrentUser.UserID;
            }

            ProviderObject.ImportMediaFileInfoInternal(mediaFile, userId);
        }


        /// <summary>
        /// Deletes media file from file system.
        /// </summary>
        /// <param name="siteId">Site id</param>
        /// <param name="libraryId">Library id</param>
        /// <param name="filePath">Sub path to file</param>
        /// <param name="onlyFile">Indicates if only file should be deleted</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        public static void DeleteMediaFile(int siteId, int libraryId, string filePath, bool onlyFile = false, bool synchronization = false)
        {
            ProviderObject.DeleteMediaFileInternal(siteId, libraryId, filePath, onlyFile, synchronization);
        }


        /// <summary>
        /// Deletes media file preview from file system.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryId">Library ID</param>
        /// <param name="filePath">File path</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        public static void DeleteMediaFilePreview(string siteName, int libraryId, string filePath, bool synchronization = false)
        {
            ProviderObject.DeleteMediaFilePreviewInternal(siteName, libraryId, filePath, synchronization);

        }


        /// <summary>
        /// Deletes file records of files matching specified criteria.
        /// </summary>
        /// <param name="path">Path of the files to delete</param>
        /// <param name="libraryId">ID of the library where the files belongs to</param>
        public static void DeleteMediaFiles(string path, int libraryId)
        {
            ProviderObject.DeleteMediaFilesInternal(path, libraryId);
        }


        /// <summary>
        /// Gets the cache key dependencies array for the media file (cache item keys affected when the meta file changes).
        /// </summary>
        /// <param name="fi">File</param>
        /// <param name="preview">Indicates if preview cache key should be created</param>
        public static string[] GetDependencyCacheKeys(MediaFileInfo fi, bool preview)
        {
            return ProviderObject.GetDependencyCacheKeysInternal(fi, preview);

        }


        /// <summary>
        /// Saves media file to disk and returns the applied file path.
        /// </summary>
        /// <param name="siteName">Name of the site of the media library</param>
        /// <param name="libraryFolder">Media library root folder</param>
        /// <param name="librarySubFolderPath">Subfolder path</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileExtension">Extension of the file</param>
        /// <param name="fileData">File data</param>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="ensureUniqueFileName">Indicates if unique file name should be ensured</param>
        public static string SaveFileToDisk(string siteName, string libraryFolder, string librarySubFolderPath, string fileName, string fileExtension, Guid fileGuid, BinaryData fileData, bool synchronization, bool ensureUniqueFileName = true)
        {
            return ProviderObject.SaveFileToDiskInternal(siteName, libraryFolder, librarySubFolderPath, fileName, fileExtension, fileGuid, fileData, synchronization, ensureUniqueFileName, false, null, null);
        }


        /// <summary>
        /// Deletes media file preview thumbnails.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        public static void DeleteMediaFilePreviewThumbnails(MediaFileInfo fileInfo)
        {
            ProviderObject.DeleteMediaFilePreviewThumbnailsInternal(fileInfo);
        }



        /// <summary>
        /// Deletes media file thumbnails.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        public static void DeleteMediaFileThumbnails(MediaFileInfo fileInfo)
        {
            ProviderObject.DeleteMediaFileThumbnailsInternal(fileInfo);
        }


        /// <summary>
        /// Copy media file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="origPath">Old file path within the library folder</param>
        /// <param name="newPath">New file path within the library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="userId">ID of the user performing copy action</param>
        public static void CopyMediaFile(string siteName, int libraryID, string origPath, string newPath, bool synchronization = false, int userId = 0)
        {
            if (userId == 0)
            {
                userId = CMSActionContext.CurrentUser.UserID;
            }

            ProviderObject.CopyMediaFileInternal(siteName, libraryID, origPath, newPath, synchronization, userId);
        }


        /// <summary>
        /// Moves media file within one library.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="origPath">Old file path within the library folder</param>
        /// <param name="newPath">New file path within the library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        public static void MoveMediaFile(string siteName, int libraryID, string origPath, string newPath, bool synchronization = false)
        {
            MoveMediaFile(siteName, libraryID, libraryID, origPath, newPath, synchronization);
        }


        /// <summary>
        /// Moves media file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="originalLibraryID">Original library ID</param>
        /// <param name="newLibraryID">New library ID</param>
        /// <param name="origPath">Old file path within the library folder</param>
        /// <param name="newPath">New file path within the library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        public static void MoveMediaFile(string siteName, int originalLibraryID, int newLibraryID, string origPath, string newPath, bool synchronization = false)
        {
            ProviderObject.MoveMediaFileInternal(siteName, originalLibraryID, newLibraryID, origPath, newPath, synchronization);
        }

        #endregion


        #region "Public methods - GetMediaFile"

        /// <summary>
        /// Ensures the thumbnail file.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="width">File width</param>
        /// <param name="height">File height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        /// <param name="usePreview">Use preview file</param>
        public static string EnsureThumbnailFile(MediaFileInfo fileInfo, string siteName, int width = 0, int height = 0, int maxSideSize = 0, bool usePreview = false)
        {
            return ProviderObject.EnsureThumbnailFileInternal(fileInfo, siteName, width, height, maxSideSize, usePreview);
        }


        /// <summary>
        /// Returns image thumbnail from the disk or create a new one if doesn't exist yet.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="libraryFolder">Library folder</param>
        /// <param name="siteName">Site name</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        public static byte[] GetImageThumbnail(MediaFileInfo fileInfo, string libraryFolder, string siteName, int width = 0, int height = 0, int maxSideSize = 0)
        {
            return ProviderObject.GetImageThumbnailInternal(fileInfo, libraryFolder, siteName, width, height, maxSideSize);
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder</param>
        public static byte[] GetFile(MediaFileInfo fileInfo, string libraryFolder, string siteName)
        {
            return ProviderObject.GetFileInternal(fileInfo, libraryFolder, siteName);
        }

        #endregion


        #region "Public methods - Physical path"

        /// <summary>
        /// Returns physical path to the media file.
        /// </summary>
        /// <param name="fileId">Media file ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaFilePath(int fileId, string siteName, string webFullPath)
        {
            // Get file info
            MediaFileInfo fileInfo = GetMediaFileInfo(fileId);

            return GetMediaFilePath(fileInfo, siteName, webFullPath);
        }


        /// <summary>
        /// Returns physical path to the media file.
        /// </summary>
        /// <param name="fileInfo">Media file info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaFilePath(MediaFileInfo fileInfo, string siteName, string webFullPath)
        {
            if ((fileInfo != null) && (!String.IsNullOrEmpty(siteName)))
            {
                return GetMediaFilePath(fileInfo.FilePath, fileInfo.FileLibraryID, siteName, webFullPath);
            }
            return null;
        }


        /// <summary>
        /// Returns physical path to the media file.
        /// </summary>
        /// <param name="libraryId">Media library ID</param>
        /// <param name="filePath">File path</param>
        public static string GetMediaFilePath(int libraryId, string filePath)
        {
            if ((libraryId > 0) && (!String.IsNullOrEmpty(filePath)))
            {
                return GetMediaFilePath(filePath, libraryId);
            }
            return null;
        }


        /// <summary>
        /// Returns physical path to the given file path and library.
        /// </summary>
        /// <param name="filePath">Media file path</param>
        /// <param name="fileLibraryId">Library ID of the media file</param>
        /// <param name="siteName">Site name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaFilePath(string filePath, int fileLibraryId, string siteName = null, string webFullPath = null)
        {
            if ((filePath != null) && (fileLibraryId > 0))
            {
                MediaLibraryInfo mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(fileLibraryId);
                if (mli != null)
                {
                    // Site is not given externally, get it from media library
                    if (String.IsNullOrEmpty(siteName))
                    {
                        siteName = SiteInfoProvider.GetSiteName(mli.LibrarySiteID);
                    }

                    return GetMediaFilePath(siteName, mli.LibraryFolder, filePath, webFullPath);
                }
            }

            return null;
        }


        /// <summary>
        /// Returns physical path to the media file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="filePath">File path within the library folder</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaFilePath(string siteName, string libraryFolder, string filePath, string webFullPath = null)
        {
            return ProviderObject.GetMediaFilePathInternal(siteName, libraryFolder, filePath, webFullPath);
        }


        /// <summary>
        /// Returns physical path to the thumbnail folder.
        /// </summary>
        /// <param name="libraryId">Media library ID</param>
        /// <param name="filePath">File path from database</param>
        public static string GetThumbnailPath(int libraryId, string filePath)
        {
            return GetThumbnailPath(null, filePath, libraryId);
        }


        /// <summary>
        /// Returns physical path to the thumbnail folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="filePath">File path from database</param>
        /// <param name="libraryId">Media library ID</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetThumbnailPath(string siteName, string filePath, int libraryId, string webFullPath = null)
        {
            return ProviderObject.GetThumbnailPathInternal(siteName, filePath, libraryId, webFullPath);
        }


        /// <summary>
        /// Returns physical path to the thumbnail.
        /// </summary>
        /// <param name="siteName">Site name of the site thumbnail is related to</param>
        /// <param name="fileName">File name of the file thumbnail is related to</param>
        /// <param name="fileExtension">Extension of the file thumbnail is related to</param>
        /// <param name="path">Path of the original file</param>
        /// <param name="width">Width of the thumbnail file</param>
        /// <param name="height">Height of the thumbnail file</param>
        /// <param name="addHiddenFolder">Indicates if hidden folder should be inserted into path</param>
        /// <param name="addFileExtension">Indicates if file extension should be inserted into thumbnail file name</param>
        public static string GetThumbnailPath(string siteName, string fileName, string fileExtension, string path, int width, int height, bool addHiddenFolder, bool addFileExtension)
        {
            return ProviderObject.GetThumbnailPathInternal(siteName, fileName, fileExtension, path, width, height, addHiddenFolder, addFileExtension);
        }


        /// <summary>
        /// Returns preview file path for media file info.
        /// If no preview found returns NULL
        /// </summary>
        /// <param name="fileInfo">Media file info</param>
        public static string GetPreviewFilePath(MediaFileInfo fileInfo)
        {
            if (fileInfo != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(fileInfo.FileSiteID);
                if (si != null)
                {
                    string searchPattern = GetPreviewFilePath(fileInfo.FilePath, si.SiteName, fileInfo.FileLibraryID);
                    try
                    {
                        if (Directory.Exists(Path.GetDirectoryName(searchPattern)))
                        {
                            string[] files = Directory.GetFiles(Path.GetDirectoryName(searchPattern), Path.GetFileName(searchPattern));
                            if ((files != null) && (files.Length > 0))
                            {
                                return files[0];
                            }
                        }
                    }
                    catch (System.IO.PathTooLongException)
                    {
                        return null;
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Returns preview file path for search pattern (.* as extension).
        /// </summary>
        /// <param name="filePath">File path from database</param>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryId">Library ID</param>
        public static string GetPreviewFilePath(string filePath, string siteName, int libraryId)
        {
            return ProviderObject.GetPreviewFilePathInternal(filePath, siteName, libraryId);
        }

        #endregion


        #region "Public methods - Image"

        /// <summary>
        /// Checks whether the image should be processed (resized) by the Image manager (if the destination size is smaller).
        /// </summary>
        /// <param name="fileInfo">Media file info to check</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="maxSideSize">Max side size</param>
        public static bool CanResizeImage(MediaFileInfo fileInfo, int width, int height, int maxSideSize)
        {
            if (fileInfo == null)
            {
                return false;
            }

            return ShouldResize(maxSideSize, width, height, fileInfo.FileImageWidth, fileInfo.FileImageHeight);
        }


        /// <summary>
        /// Checks whether the image should be processed (resized) by the Image manager (if the destination size is smaller).
        /// </summary>
        /// <param name="filePath">File path</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="maxSideSize">Max side size</param>
        public static bool CanResizeImage(string filePath, int width, int height, int maxSideSize)
        {
            // Check if file exists and if its image
            if ((String.IsNullOrEmpty(filePath)) || (!File.Exists(filePath)) || (!ImageHelper.IsImage(Path.GetExtension(filePath))))
            {
                return false;
            }

            // Get image
            using (FileStream str = FileStream.New(filePath, FileMode.Open, FileAccess.Read))
            {
                Bitmap img = new Bitmap(str);

                // Get dimensions
                int imgWidth = img.Width;
                int imgHeight = img.Height;

                // Dispose image
                img.Dispose();

                return ShouldResize(maxSideSize, width, height, imgWidth, imgHeight);
            }
        }


        /// <summary>
        /// Returns true if image should be scaled down.
        /// </summary>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="imageWidth">Image width</param>
        /// <param name="imageHeight">Image height</param>
        public static bool ShouldResize(int maxSideSize, int width, int height, int imageWidth, int imageHeight)
        {
            return ProviderObject.ShouldResizeInternal(maxSideSize, width, height, imageWidth, imageHeight);
        }


        /// <summary>
        /// Returns image thumbnail from the disk or create a new one if doesn't exist yet.
        /// </summary>
        /// <param name="originalFilePath">Original file path</param>
        /// <param name="originalWidth">Original width</param>
        /// <param name="originalHeight">Original height</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static byte[] GetThumbnail(string originalFilePath, int originalWidth, int originalHeight, int width, int height)
        {
            return ProviderObject.GetThumbnailInternal(originalFilePath, originalWidth, originalHeight, width, height);
        }

        #endregion


        #region "Public methods - URL"

        /// <summary>
        /// Returns relative URL path to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="filePath">File path within the library folder</param>
        [Obsolete("Use MediaFileURLProvider.GetMediaFileUrl(string siteName, string libraryFolder, string filePath) method instead.")]
        public static string GetMediaFileUrl(string siteName, string libraryFolder, string filePath)
        {
            return MediaFileURLProvider.GetMediaFileUrl(siteName, libraryFolder, filePath);
        }


        /// <summary>
        /// Returns relative URL path to the media file which is rewritten to calling GetMediaFile.aspx page where user permissions are checked.
        /// </summary>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        [Obsolete("Use MediaFileURLProvider.GetMediaFileUrl(Guid fileGuid, string fileName) method instead.")]
        public static string GetMediaFileUrl(Guid fileGuid, string fileName)
        {
            return MediaFileURLProvider.GetMediaFileUrl(fileGuid, fileName);
        }


        /// <summary>
        /// Returns absolute URL path to the media file including http:// which is rewritten to calling GetMediaFile.aspx page where user permissions are checked
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        [Obsolete("Use MediaFileURLProvider.GetMediaFileAbsoluteUrl(string siteName, Guid fileGuid, string fileName) method instead.")]
        public static string GetMediaFileAbsoluteUrl(string siteName, Guid fileGuid, string fileName)
        {
            return MediaFileURLProvider.GetMediaFileAbsoluteUrl(siteName, fileGuid, fileName);
        }


        /// <summary>
        /// Returns absolute URL path for current domain to the media file including http:// which is rewritten to calling GetMediaFile.aspx page where user permissions are checked
        /// </summary>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        [Obsolete("Use MediaFileURLProvider.GetMediaFileAbsoluteUrl(Guid fileGuid, string fileName) method instead.")]
        public static string GetMediaFileAbsoluteUrl(Guid fileGuid, string fileName)
        {
            return MediaFileURLProvider.GetMediaFileAbsoluteUrl(fileGuid, fileName);
        }


        /// <summary>
        /// Returns absolute URL path to the media file including http://, user permissions are not checked
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="filePath">File path within the library folder</param>
        [Obsolete("Use MediaFileURLProvider.GetMediaFileAbsoluteUrl(string siteName, string libraryFolder, string filePath) method instead.")]
        public static string GetMediaFileAbsoluteUrl(string siteName, string libraryFolder, string filePath)
        {
            return MediaFileURLProvider.GetMediaFileAbsoluteUrl(siteName, libraryFolder, filePath);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns media file with specified GUID.
        /// </summary>
        /// <param name="guid">Media file GUID</param>
        /// <param name="siteName">Site name</param>
        protected virtual MediaFileInfo GetMediaFileInfoInternal(Guid guid, string siteName)
        {
            SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
            if (si != null)
            {
                return GetInfoByGuid(guid, si.SiteID);
            }
            return null;
        }


        /// <summary>
        /// Returns the MediaFileInfo structure for the specified media file.
        /// </summary>
        /// <param name="mediaLibraryId">Media library ID</param>
        /// <param name="mediaFilePath">File path</param>
        protected virtual MediaFileInfo GetMediaFileInfoInternal(int mediaLibraryId, string mediaFilePath)
        {
            if ((mediaLibraryId > 0) && (mediaFilePath != null))
            {
                // Prepare the where condition
                string where = String.Format("(FileLibraryID = {0}) AND (FilePath = N'{1}')", mediaLibraryId, Path.EnsureSlashes(mediaFilePath).Replace("'", "''"));

                return GetObjectQuery().TopN(1).Where(new WhereCondition(where)).FirstOrDefault();
            }

            return null;
        }


        /// <summary>
        /// Returns the MediaFileInfo structure for the specified media file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="mediaFilePath">File path</param>
        /// <param name="libraryFolder">Library folder name</param>
        protected virtual MediaFileInfo GetMediaFileInfoInternal(string siteName, string mediaFilePath, string libraryFolder)
        {
            if (!String.IsNullOrEmpty(siteName) && !String.IsNullOrEmpty(mediaFilePath))
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(siteName);
                if (si != null)
                {
                    // Prepare the where condition
                    WhereCondition condition = new WhereCondition()
                                                    .WhereEquals("FileSiteID", si.SiteID)
                                                    .WhereEquals("FilePath", Path.EnsureSlashes(mediaFilePath));

                    if (!String.IsNullOrEmpty(libraryFolder))
                    {
                        condition.WhereIn("FileLibraryID", new IDQuery<MediaLibraryInfo>()
                                                                        .WhereEquals("LibraryFolder", libraryFolder));
                    }

                    // Get the data
                    return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
                }
            }
            return null;
        }


        /// <summary>
        /// Sets MediaFileInfo internal.
        /// </summary>
        /// <param name="mediaFile">Media file info</param>
        /// <param name="saveFileToDisk">Save file to disk</param>
        /// <param name="userId">ID of the user performing set action</param>
        /// <param name="ensureUniqueFileName">Indicates if unique file name should be ensured</param>
        protected virtual void SetMediaFileInfoInternal(MediaFileInfo mediaFile, bool saveFileToDisk, int userId, bool ensureUniqueFileName)
        {
            if (mediaFile != null)
            {
                bool saveToDisk = false;
                string filePath = null;
                SiteInfo si = null;
                MediaLibraryInfo mli = null;
                string subFolderPath = string.Empty;

                // Save file to disk if the there is some data
                if ((saveFileToDisk) && ((mediaFile.FileBinaryStream != null) || (mediaFile.FileBinary != null)))
                {
                    mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(mediaFile.FileLibraryID);
                    if (mli != null)
                    {
                        si = SiteInfoProvider.GetSiteInfo(mediaFile.FileSiteID);
                        if (si != null)
                        {
                            saveToDisk = true;

                            int lastSlash = mediaFile.FilePath.LastIndexOfCSafe('/');
                            if (lastSlash > 0)
                            {
                                subFolderPath = mediaFile.FilePath.Substring(0, lastSlash);
                            }

                            // Ensure physical structure on disk
                            mediaFile.FilePath = CheckAndEnsureFilePath(si.SiteName, mli.LibraryFolder, subFolderPath, mediaFile.FileName, mediaFile.FileExtension, ensureUniqueFileName, out filePath);

                            // Set file name
                            mediaFile.FileName = Path.GetFileNameWithoutExtension(mediaFile.FilePath);
                        }
                    }
                }

                // Refresh image dimensions if not provided (eg. WebDAV in some cases)
                if (ImageHelper.IsImage(mediaFile.FileExtension) && (mediaFile.FileImageWidth == 0) && (mediaFile.FileImageHeight == 0))
                {
                    mli = mli ?? MediaLibraryInfoProvider.GetMediaLibraryInfo(mediaFile.FileLibraryID);
                    if (mli != null)
                    {
                        si = si ?? SiteInfoProvider.GetSiteInfo(mediaFile.FileSiteID);
                        if (si != null)
                        {
                            // Get full path to media library folder
                            string folderPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(si.SiteName, mli.LibraryFolder);

                            string absFilePath = DirectoryHelper.CombinePath(folderPath, mediaFile.FilePath);
                            if (File.Exists(absFilePath))
                            {
                                Image img = new ImageHelper().GetImage(absFilePath);
                                if (img != null)
                                {
                                    mediaFile.FileImageHeight = img.Height;
                                    mediaFile.FileImageWidth = img.Width;
                                }
                            }
                        }
                    }
                }
                else if (!ImageHelper.IsImage(mediaFile.FileExtension) && ((mediaFile.FileImageWidth > 0) || (mediaFile.FileImageHeight > 0)))
                {
                    mediaFile.FileImageHeight = 0;
                    mediaFile.FileImageWidth = 0;
                }

                // If file was successfully created on the disk make a new record in the DB
                if (CMSActionContext.CurrentUpdateSystemFields)
                {
                    if (mediaFile.FileID <= 0)
                    {
                        mediaFile.FileCreatedByUserID = userId;
                    }
                    mediaFile.FileModifiedByUserID = userId;
                }

                using (var scope = BeginTransaction())
                {
                    try
                    {
                        // Check uniqueness
                        if (!mediaFile.IsFilePathUnique())
                        {
                            throw new MediaFilePathNotUniqueException(String.Format(CoreServices.Localization.GetString("mediafile.filepathnotunique"), mediaFile.FilePath));
                        }

                        mediaFile.SetDefaultDataFromFormDefinition();

                        SetInfo(mediaFile);

                        // Save physical file to disk
                        if (saveToDisk)
                        {
                            mediaFile.FilePath = SaveFileToDiskInternal(si.SiteName, mli.LibraryFolder, subFolderPath, mediaFile.FileName, mediaFile.FileExtension, mediaFile.FileGUID, mediaFile.FileBinary ?? (BinaryData)mediaFile.FileBinaryStream, false, ensureUniqueFileName, true, filePath, mediaFile.FilePath);

                            // Delete media file thumbnails
                            DeleteMediaFileThumbnails(mediaFile);
                        }
                    }
                    finally
                    {
                        // Ensure binary stream is closed also in case exception in saving to database
                        if (mediaFile.FileBinaryStream != null)
                        {
                            mediaFile.FileBinaryStream.Close();
                        }
                    }

                    scope.Commit();
                }

                // Drop the cache dependencies
                if (mediaFile.Generalized.TouchCacheDependencies)
                {
                    CacheHelper.TouchKeys(GetDependencyCacheKeys(mediaFile, false));
                }
            }
        }


        /// <summary>
        /// Deletes specified media file.
        /// </summary>
        /// <param name="infoObj">Media file object</param>
        protected virtual void DeleteMediaFileInfoInternal(MediaFileInfo infoObj)
        {
            // Delete media file first from DB because object versioning
            DeleteInfo(infoObj);

            if (infoObj != null)
            {
                DeleteMediaFile(infoObj.FileSiteID, infoObj.FileLibraryID, infoObj.FilePath);
            }
        }


        /// <summary>
        /// Returns the query for all media files.
        /// </summary>        
        protected virtual ObjectQuery<MediaFileInfo> GetMediaFilesInternal()
        {
            return ProviderObject.GetObjectQuery();
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Import media file into database.
        /// </summary>
        /// <param name="mediaFile">Media file to import</param>
        /// <param name="userId">ID of the user performing set action</param>
        protected virtual void ImportMediaFileInfoInternal(MediaFileInfo mediaFile, int userId)
        {
            if (mediaFile != null)
            {
                ProviderObject.SetMediaFileInfoInternal(mediaFile, false, userId, true);

                if (WebFarmHelper.WebFarmEnabled)
                {
                    MediaLibraryInfo mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(mediaFile.FileLibraryID);
                    if (mli != null)
                    {
                        var path = DirectoryHelper.CombinePath(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(mediaFile.FileLibraryID), mediaFile.FilePath);
                        byte[] fileBinary = File.ReadAllBytes(path);

                        WebFarmHelper.CreateIOTask(new UpdateMediaWebFarmTask
                        {
                            TaskFilePath = path,
                            TaskBinaryData = fileBinary,
                            SiteName = SiteContext.CurrentSiteName,
                            LibraryFolder = mli.LibraryFolder,
                            LibrarySubFolderPath = Path.GetDirectoryName(mediaFile.FilePath),
                            FileName = mediaFile.FileName,
                            FileExtension = mediaFile.FileExtension,
                            FileGuid = mediaFile.FileGUID
                        });
                    }
                }
            }
        }


        /// <summary>
        /// Deletes media file from filesystem.
        /// </summary>
        /// <param name="siteID">Site id</param>
        /// <param name="libraryID">Library id</param>
        /// <param name="filePath">Sub path to file</param>
        /// <param name="onlyFile">Indicates if only file should be deleted</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        protected virtual void DeleteMediaFileInternal(int siteID, int libraryID, string filePath, bool onlyFile, bool synchronization)
        {
            filePath = Path.EnsureSlashes(filePath);

            // Delete file from the disk
            SiteInfo site = SiteInfoProvider.GetSiteInfo(siteID);
            MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryID);
            if ((site != null) && (library != null))
            {
                string path = GetMediaFilePath(site.SiteName, library.LibraryFolder, filePath);
                if (File.Exists(path))
                {
                    try
                    {
                        // Delete file
                        File.Delete(path);
                    }
                    catch
                    {
                    }
                }

                // Ensure path size
                if (path.Length < 260)
                {
                    // Check if hidden folder exist
                    string dirName = Path.GetDirectoryName(path);
                    string hiddenFolderName = MediaLibraryHelper.GetMediaFileHiddenFolder(site.SiteName);
                    if (Directory.Exists(DirectoryHelper.CombinePath(dirName, hiddenFolderName)))
                    {
                        string fileName = Path.GetFileNameWithoutExtension(path);
                        string extension = Path.GetExtension(path);

                        // Get hidden files for current file
                        string[] hiddenFiles = null;
                        try
                        {
                            hiddenFiles = Directory.GetFiles(DirectoryHelper.CombinePath(dirName, hiddenFolderName),
                                                             String.Format("{0}_{1}*", fileName, extension.ToLowerCSafe().TrimStart('.')));
                        }
                        catch
                        {
                        }
                        if ((hiddenFiles != null) && (hiddenFiles.Length > 0))
                        {
                            string mediaPreviewSuffix = MediaLibraryHelper.GetMediaFilePreviewSuffix(site.SiteName);

                            // Delete each hidden file
                            foreach (string hiddenFilePath in hiddenFiles)
                            {
                                try
                                {
                                    if (onlyFile)
                                    {
                                        string hiddenFile = Path.GetFileNameWithoutExtension(hiddenFilePath);
                                        string prefix = String.Format("{0}_{1}", fileName, Path.GetExtension(path).ToLowerCSafe().TrimStart('.'));
                                        string suffix = hiddenFile.Substring(prefix.Length);
                                        // Delete only file thumbnails
                                        if (!suffix.StartsWithCSafe(mediaPreviewSuffix))
                                        {
                                            File.Delete(hiddenFilePath);
                                        }
                                    }
                                    else
                                    {
                                        File.Delete(hiddenFilePath);
                                    }
                                }
                                catch
                                {
                                }
                            }
                        }
                    }
                }
            }

            if (!synchronization)
            {
                WebFarmHelper.CreateTask(new DeleteFileMediaWebFarmTask
                {
                    DestinationLibraryId = libraryID,
                    SiteId = siteID,
                    DestinationPath = filePath,
                    ApplyOnlyOnFiles = onlyFile
                });
            }
        }
        

        /// <summary>
        /// Deletes media file preview from filesystem.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="filePath">File path</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        protected virtual void DeleteMediaFilePreviewInternal(string siteName, int libraryID, string filePath, bool synchronization)
        {
            filePath = Path.EnsureSlashes(filePath);

            // Delete file from the disk
            MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryID);
            if (library != null)
            {
                string path = GetMediaFilePath(siteName, library.LibraryFolder, filePath);
                path = DirectoryHelper.CombinePath(Path.GetDirectoryName(path), MediaLibraryHelper.GetMediaFileHiddenFolder(siteName));

                // Check if hidden folder exist
                if (Directory.Exists(path))
                {
                    string previewSearchPattern = MediaLibraryHelper.GetPreviewFileName(Path.GetFileNameWithoutExtension(filePath), Path.GetExtension(filePath), string.Empty, siteName);
                    if (previewSearchPattern != null)
                    {
                        // Get hidden files for current file
                        string[] previewFiles = Directory.GetFiles(path, previewSearchPattern.TrimEnd('.') + "*");
                        if (previewFiles.Length > 0)
                        {
                            // Delete each hidden file
                            Array.ForEach(previewFiles, File.Delete);
                        }
                    }
                }
            }

            if (!synchronization)
            {
                WebFarmHelper.CreateTask(new DeleteFilePreviewMediaWebFarmTask
                {
                    SiteName = siteName,
                    DestinationLibraryId = libraryID,
                    DestinationPath = filePath
                });
            }
        }


        /// <summary>
        /// Updates the file path of all the files matching specified criteria.
        /// </summary>
        /// <param name="libraryId">ID of the library where the files belongs to</param>
        /// <param name="newPath">New file path of the files</param>
        /// <param name="originalPath">Old file path of the files</param>
        internal static void UpdateFilesPath(string originalPath, string newPath, int libraryId)
        {
            var path = Path.EnsureSlashes(originalPath, true) + '/';
            var where = new WhereCondition()
                .WhereStartsWith("FilePath", path)
                .WhereEquals("FileLibraryID", libraryId);

            where.Parameters.Add("NewPath", Path.EnsureSlashes(newPath, true));
            where.Parameters.Add("OriginalPathLength", path.Length);

            ProviderObject.UpdateData("[FilePath]=@NewPath + Substring([FilePath], @OriginalPathLength, Len([FilePath]))", where.Parameters, where.WhereCondition);
        }


        /// <summary>
        /// Saves media file to disk and returns the applied file path.
        /// </summary>
        /// <param name="siteName">Name of the site of the media library</param>
        /// <param name="libraryFolder">Media library root folder</param>
        /// <param name="librarySubFolderPath">Subfolder path</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileExtension">Extension of the file</param>
        /// <param name="fileData">File data</param>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="ensureUniqueFileName">Indicates if unique file name should be ensured</param>
        /// <param name="skipChecks">Skip check for file path and ensuring physical file path</param>
        /// <param name="fileSubFolderPath">File subfolder path</param>
        /// <param name="filePath">File path</param>
        protected virtual string SaveFileToDiskInternal(string siteName, string libraryFolder, string librarySubFolderPath, string fileName, string fileExtension, Guid fileGuid, BinaryData fileData, bool synchronization, bool ensureUniqueFileName, bool skipChecks, string filePath, string fileSubFolderPath)
        {
            // If a file is going to be overwritten during synchronization, try to delete all thumbnails.
            if (!ensureUniqueFileName && synchronization)
            {
                MediaFileInfo info = GetMediaFileInfo(fileGuid, siteName);
                try
                {
                    DeleteMediaFileThumbnails(info);
                }
                catch
                {
                    // Pass
                }
            }

            if (!skipChecks)
            {
                fileSubFolderPath = CheckAndEnsureFilePath(siteName, libraryFolder, librarySubFolderPath, fileName, fileExtension, ensureUniqueFileName, out filePath);
                fileSubFolderPath = MediaLibraryHelper.EnsurePhysicalPath(fileSubFolderPath);
            }

            // Save specified file
            if (fileData != null)
            {
                StorageHelper.SaveFileToDisk(filePath, fileData, false);

                // If the action is not caused by synchronization, create the web farm task
                if (!synchronization && CMSActionContext.CurrentLogWebFarmTasks)
                {
                    WebFarmHelper.CreateIOTask(new UpdateMediaWebFarmTask
                    {
                        TaskFilePath = filePath,
                        TaskBinaryData = fileData,
                        SiteName = siteName,
                        LibraryFolder = libraryFolder,
                        LibrarySubFolderPath = librarySubFolderPath,
                        FileName = fileName,
                        FileExtension = fileExtension,
                        FileGuid = fileGuid
                    });
                }

                fileData.Close();

                if (synchronization)
                {
                    // Drop the cache dependencies if method run under synchronization
                    CacheHelper.TouchKey("mediafile|" + fileGuid.ToString().ToLowerCSafe(), false, false);
                    CacheHelper.TouchKey("mediafilepreview|" + fileGuid.ToString().ToLowerCSafe(), false, false);
                }
            }

            return Path.EnsureSlashes(fileSubFolderPath);
        }


        /// <summary>
        /// Moves media file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="originalLibraryID">Original library ID</param>
        /// <param name="newLibraryID">New library ID</param>
        /// <param name="origPath">Old file path within the library folder</param>
        /// <param name="newPath">New file path within the library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        protected virtual void MoveMediaFileInternal(string siteName, int originalLibraryID, int newLibraryID, string origPath, string newPath, bool synchronization)
        {
            origPath = Path.EnsureSlashes(origPath);
            newPath = Path.EnsureSlashes(newPath);

            bool fileExists = false;

            // Use new path as original path if current move is under web farm
            string infoObjPath = synchronization ? newPath : origPath;

            MediaFileInfo fileInfo = GetMediaFileInfo(originalLibraryID, infoObjPath);
            string origPhysicalPath = GetMediaFilePath(originalLibraryID, origPath);
            string newPhysicalPath = GetMediaFilePath(newLibraryID, newPath);

            // Ensure unique file name
            if (File.Exists(newPhysicalPath))
            {
                newPhysicalPath = MediaLibraryHelper.EnsureUniqueFileName(newPhysicalPath);
                newPath = String.Format("{0}/{1}", Path.GetDirectoryName(newPath), Path.GetFileName(newPhysicalPath));

                fileExists = true;
            }

            // Ensure original file version
            if (fileInfo != null)
            {
                SynchronizationHelper.EnsureObjectVersion(fileInfo);
            }

            // Data must be moved before setting object because of Object versioning
            if (File.Exists(origPhysicalPath))
            {
                // Move file
                File.Move(origPhysicalPath, newPhysicalPath);

                fileExists = true;
            }

            // Get hidden files
            string hiddenFolderName = MediaLibraryHelper.GetMediaFileHiddenFolder(siteName);
            string[] hiddenFiles = null;

            // If exists hidden folder get hidden files
            if (Directory.Exists(DirectoryHelper.CombinePath(Path.GetDirectoryName(origPhysicalPath), hiddenFolderName)))
            {
                hiddenFiles = Directory.GetFiles(DirectoryHelper.CombinePath(Path.GetDirectoryName(origPhysicalPath), hiddenFolderName), String.Format("{0}_{1}*", Path.GetFileNameWithoutExtension(origPhysicalPath), Path.GetExtension(origPhysicalPath).ToLowerCSafe().TrimStart('.')));
            }

            // Copy hidden files
            if ((hiddenFiles != null) && (hiddenFiles.Length > 0))
            {
                foreach (string hiddenPath in hiddenFiles)
                {
                    string hiddenName = Path.GetFileName(hiddenPath);
                    string newPathDir = Path.GetDirectoryName(newPhysicalPath);

                    // Ensure hidden folder
                    DirectoryHelper.EnsureDiskPath(DirectoryHelper.CombinePath(newPathDir, hiddenFolderName) + "\\", newPathDir);

                    var hiddenFileNameSuffix = GetFileNameSuffix(newPath, origPhysicalPath, hiddenName);

                    // Move hidden file
                    File.Move(hiddenPath, DirectoryHelper.CombinePath(newPathDir, hiddenFolderName, Path.GetFileNameWithoutExtension(newPhysicalPath)) + hiddenFileNameSuffix);
                }
            }

            // Skip if under synchronization
            if ((fileInfo != null) && !synchronization)
            {
                // Set new file path and name
                fileInfo.FilePath = Path.EnsureSlashes(newPath).TrimStart('/');
                fileInfo.FileName = Path.GetFileNameWithoutExtension(newPath);
                fileInfo.FileExtension = Path.GetExtension(newPath);
                fileInfo.FileMimeType = MimeTypeHelper.GetMimetype(fileInfo.FileExtension);
                fileInfo.FileLibraryID = newLibraryID;

                SetMediaFileInfo(fileInfo);

                FileInfo fi = FileInfo.New(newPhysicalPath);
                if (fi != null)
                {
                    fi.LastWriteTime = fileInfo.FileModifiedWhen;
                }

                fileExists = true;
            }

            if (!fileExists)
            {
                throw new Exception("File could not be found.");
            }

            if (!synchronization)
            {
                WebFarmHelper.CreateTask(new MoveFileMediaWebFarmTask
                {
                    SiteName = siteName,
                    SourceLibraryId = originalLibraryID,
                    DestinationLibraryId = newLibraryID,
                    SourcePath = origPath,
                    DestinationPath = newPath
                });
            }
        }


        /// <summary>
        /// Copy media file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="origPath">Old file path within the library folder</param>
        /// <param name="newPath">New file path within the library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="userId">ID of the user performing copy action</param>
        protected virtual void CopyMediaFileInternal(string siteName, int libraryID, string origPath, string newPath, bool synchronization, int userId)
        {
            origPath = Path.EnsureSlashes(origPath);
            newPath = Path.EnsureSlashes(newPath);

            bool fileExists = false;

            MediaFileInfo fileInfo = GetMediaFileInfo(libraryID, origPath);
            string origPhysicalPath = GetMediaFilePath(libraryID, origPath);
            string newPhysicalPath = GetMediaFilePath(libraryID, newPath);

            // Ensure unique file name only on source server (web farm task will overwrite media file if instance is using shared storage)
            if (!synchronization && File.Exists(newPhysicalPath))
            {
                newPhysicalPath = MediaLibraryHelper.EnsureUniqueFileName(newPhysicalPath);
                newPath = Path.EnsureSlashes(String.Format("{0}/{1}", Path.GetDirectoryName(newPath), Path.GetFileName(newPhysicalPath))).TrimStart('/');

                fileExists = true;
            }

            string newFileName = Path.GetFileNameWithoutExtension(newPhysicalPath);

            // Data must be copied before setting object because of Object versioning
            if (File.Exists(origPhysicalPath))
            {
                // Copy file
                File.Copy(origPhysicalPath, newPhysicalPath);

                fileExists = true;
            }

            // Get hidden files
            string hiddenFolderName = MediaLibraryHelper.GetMediaFileHiddenFolder(siteName);
            string hiddenFolderPath = DirectoryHelper.CombinePath(Path.GetDirectoryName(origPhysicalPath), hiddenFolderName);

            // If exists hidden folder get hidden files
            string[] hiddenFiles = Directory.Exists(hiddenFolderPath) ? Directory.GetFiles(hiddenFolderPath, String.Format("{0}_{1}*", Path.GetFileNameWithoutExtension(origPhysicalPath), Path.GetExtension(origPhysicalPath).ToLowerCSafe().TrimStart('.'))) : null;
            // Copy hidden files
            if ((hiddenFiles != null) && (hiddenFiles.Length > 0))
            {
                foreach (string hiddenPath in hiddenFiles)
                {
                    string hiddenName = Path.GetFileName(hiddenPath);
                    string newPathDir = Path.GetDirectoryName(newPhysicalPath);

                    // Ensure hidden folder
                    DirectoryHelper.EnsureDiskPath(DirectoryHelper.CombinePath(newPathDir, hiddenFolderName) + "\\", newPathDir);
                    // Copy hidden files
                    File.Copy(hiddenPath, DirectoryHelper.CombinePath(newPathDir, hiddenFolderName, newFileName) + hiddenName.Substring(Path.GetFileNameWithoutExtension(origPhysicalPath).Length));
                }
            }

            // Skip if under synchronization
            if ((fileInfo != null) && !synchronization)
            {
                // Create clone, set new file path and name and save it as new file
                MediaFileInfo copyFile = fileInfo.Clone(true);

                copyFile.FilePath = newPath;
                copyFile.FileName = newFileName;
                copyFile.FileExtension = Path.GetExtension(newPhysicalPath);
                copyFile.FileMimeType = MimeTypeHelper.GetMimetype(copyFile.FileExtension);

                SetMediaFileInfo(copyFile, true, userId);

                fileExists = true;
            }

            if (!fileExists)
            {
                throw new Exception("File could not be found.");
            }

            if (!synchronization)
            {
                WebFarmHelper.CreateTask(new CopyFileMediaWebFarmTask
                {
                    SiteName = siteName,
                    DestinationLibraryId = libraryID,
                    SourcePath = origPath,
                    DestinationPath = newPath
                });
            }
        }


        /// <summary>
        /// Deletes media file thumbnails.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        protected virtual void DeleteMediaFileThumbnailsInternal(MediaFileInfo fileInfo)
        {
            if (fileInfo != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(fileInfo.FileSiteID);
                if (si != null)
                {
                    string libraryPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(fileInfo.FileLibraryID);
                    string filePath = Path.GetDirectoryName(fileInfo.FilePath);
                    string hiddenFolder = MediaLibraryHelper.GetMediaFileHiddenFolder(si.SiteName);

                    string folderPath = DirectoryHelper.CombinePath(libraryPath, (!String.IsNullOrEmpty(filePath)) ? filePath : string.Empty, hiddenFolder);

                    try
                    {
                        if (Directory.Exists(folderPath))
                        {
                            string pattern = String.Format("{0}_{1}_", fileInfo.FileName, fileInfo.FileExtension.TrimStart('.'));
                            Regex thumbReg = RegexHelper.GetRegex(Regex.Escape(pattern) + "\\d+_\\d+\\.", true);
                            string[] files = Directory.GetFiles(folderPath, pattern + "*");
                            if ((files != null) && (files.Length > 0))
                            {
                                foreach (string file in files)
                                {
                                    if (thumbReg.IsMatch(file))
                                    {
                                        File.Delete(file);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!(exception is UnauthorizedAccessException || exception is System.IO.PathTooLongException)) throw;
                    }
                }
            }
        }


        /// <summary>
        /// Deletes media file preview thumbnails.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        protected virtual void DeleteMediaFilePreviewThumbnailsInternal(MediaFileInfo fileInfo)
        {
            if (fileInfo != null)
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(fileInfo.FileSiteID);
                if (si != null)
                {
                    string libraryPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(fileInfo.FileLibraryID);
                    string filePath = Path.GetDirectoryName(fileInfo.FilePath);
                    string hiddenFolder = MediaLibraryHelper.GetMediaFileHiddenFolder(si.SiteName);
                    string previewSuffix = MediaLibraryHelper.GetMediaFilePreviewSuffix(si.SiteName);

                    string folderPath = DirectoryHelper.CombinePath(libraryPath, (!String.IsNullOrEmpty(filePath) ? filePath : string.Empty), hiddenFolder);

                    try
                    {
                        if (Directory.Exists(folderPath))
                        {
                            string pattern = String.Format("{0}_{1}{2}_", fileInfo.FileName, fileInfo.FileExtension.TrimStart('.'), previewSuffix);
                            Regex thumbReg = RegexHelper.GetRegex(Regex.Escape(pattern) + "\\d+_\\d+\\.", true);
                            string[] files = Directory.GetFiles(folderPath, pattern + "*");
                            if ((files != null) && (files.Length > 0))
                            {
                                foreach (string file in files)
                                {
                                    if (thumbReg.IsMatch(file))
                                    {
                                        File.Delete(file);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        if (!(exception is UnauthorizedAccessException || exception is System.IO.PathTooLongException)) throw;
                    }
                }
            }
        }


        /// <summary>
        /// Deletes file records of files matching specified criteria.
        /// </summary>
        /// <param name="path">Path of the files to delete</param>
        /// <param name="libraryId">ID of the library where the files belongs to</param>
        protected virtual void DeleteMediaFilesInternal(string path, int libraryId)
        {
            string filePath = String.IsNullOrEmpty(path) ? string.Empty : Path.EnsureSlashes(path).Replace("'", "''").TrimEnd('/') + "/";

            // Get all medial files from database
            using (DataSet ds = GetMediaFiles(String.Format("FileLibraryID = {0} AND FilePath LIKE N'{1}%' ", libraryId, SqlHelper.EscapeLikeQueryPatterns(filePath))))
            {
                // Loop thru all records in database
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        // Delete media file info from DB and  file from disc
                        DeleteMediaFileInfo(new MediaFileInfo(dr));
                    }
                }
            }
        }


        /// <summary>
        /// Gets the cache key dependencies array for the media file (cache item keys affected when the meta file changes).
        /// </summary>
        /// <param name="fi">File</param>
        /// <param name="preview">Indicates if preview cache key should be created</param>
        protected virtual string[] GetDependencyCacheKeysInternal(MediaFileInfo fi, bool preview)
        {
            if (fi == null)
            {
                return null;
            }

            string byGuid = "mediafile|" + fi.FileGUID.ToString().ToLowerCSafe();

            if (preview)
            {
                string previewCache = "mediafilepreview|" + fi.FileGUID.ToString().ToLowerCSafe();
                return new[] { byGuid, previewCache };
            }

            return new[] { byGuid };
        }

        #endregion


        #region "Internal methods - GetMediaFile"

        /// <summary>
        /// Ensures the thumbnail file.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="width">File width</param>
        /// <param name="height">File height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        /// <param name="usePreview">Use preview file</param>
        protected virtual string EnsureThumbnailFileInternal(MediaFileInfo fileInfo, string siteName, int width, int height, int maxSideSize, bool usePreview = false)
        {
            if (fileInfo != null)
            {
                MediaLibraryInfo li = MediaLibraryInfoProvider.GetMediaLibraryInfo(fileInfo.FileLibraryID);
                if (li != null)
                {
                    int originalWidth = 0;
                    int originalHeight = 0;
                    int[] newDims = new int[2];
                    string filePath = null;
                    string originalFilePath = null;

                    if (usePreview)
                    {
                        // Try to find the preview file for the media file
                        string previewPath = GetPreviewFilePath(fileInfo.FilePath, siteName, fileInfo.FileLibraryID);
                        string[] files = Directory.GetFiles(Path.GetDirectoryName(previewPath), Path.GetFileName(previewPath));
                        if (files.Length > 0)
                        {
                            // If some preview was found, get the dimensions
                            previewPath = files[0];

                            originalFilePath = files[0];
                            using (FileStream str = FileStream.New(previewPath, FileMode.Open, FileAccess.Read))
                            {
                                Bitmap img = new Bitmap(str);

                                // Get new dimensions from preview file
                                originalWidth = img.Width;
                                originalHeight = img.Height;
                                img.Dispose();
                            }

                            newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

                            string thumbDirectory = Path.GetDirectoryName(previewPath).Replace(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(siteName, li.LibraryFolder), string.Empty);

                            // Preview thumbnail file path
                            filePath = GetThumbnailPath(siteName, Path.GetFileNameWithoutExtension(files[0]), Path.GetExtension(files[0]), thumbDirectory, newDims[0], newDims[1], false, false);
                        }
                    }

                    // If preview file not found use original file
                    if ((originalWidth == 0) && (originalHeight == 0))
                    {
                        // Get new dimensions
                        originalWidth = fileInfo.FileImageWidth;
                        originalHeight = fileInfo.FileImageHeight;
                        newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

                        // If new thumbnail dimensions are different from the original ones, get resized file
                        bool resize = (((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0));
                        if (!resize)
                        {
                            // No need to resize, return path to the original file
                            return fileInfo.FilePath;
                        }

                        // Thumbnail file path
                        filePath = GetThumbnailPath(siteName, fileInfo.FileName, fileInfo.FileExtension, Path.GetDirectoryName(fileInfo.FilePath), newDims[0], newDims[1], true, true);
                    }

                    // Get full thumbnail path
                    string thumbnailFilePath = GetMediaFilePath(siteName, li.LibraryFolder, filePath);
                    if (File.Exists(thumbnailFilePath))
                    {
                        return Path.EnsureSlashes(filePath);
                    }

                    lock (ensureFileLock)
                    {
                        // Check the file existence again (in case other thread created it)
                        if (File.Exists(thumbnailFilePath))
                        {
                            return thumbnailFilePath;
                        }

                        byte[] data;
                        if (usePreview)
                        {
                            // Get thumbnail data from preview file
                            data = GetThumbnail(originalFilePath, originalWidth, originalHeight, newDims[0], newDims[1]);
                        }
                        else
                        {
                            // Get thumbnail data from original media file
                            data = GetImageThumbnailInternal(fileInfo, li.LibraryFolder, siteName, newDims[0], newDims[1], 0);
                        }
                        if (data != null)
                        {
                            // Create new file if the file does not exist
                            try
                            {
                                string subFolderPath = string.Empty;
                                int lastSlash = fileInfo.FilePath.LastIndexOfCSafe('/');
                                if (lastSlash > 0)
                                {
                                    subFolderPath = fileInfo.FilePath.Substring(0, lastSlash);
                                }

                                subFolderPath = String.IsNullOrEmpty(subFolderPath) ? MediaLibraryHelper.GetMediaFileHiddenFolder(siteName) : DirectoryHelper.CombinePath(subFolderPath, MediaLibraryHelper.GetMediaFileHiddenFolder(siteName));

                                string diskFilePath = String.Empty;
                                using (new CMSActionContext { LogWebFarmTasks = false })
                                {
                                    diskFilePath = SaveFileToDisk(siteName, li.LibraryFolder, subFolderPath, Path.GetFileNameWithoutExtension(thumbnailFilePath), Path.GetExtension(thumbnailFilePath), fileInfo.FileGUID, data, false);
                                }

                                // Save the data to the disk                        
                                return diskFilePath;
                            }
                            catch (Exception ex)
                            {
                                EventLogProvider.LogException("MediaFile", "SaveThumbnailFileToDisk", ex);
                            }
                        }
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Returns image thumbnail from the disk or create a new one if doesn't exist yet.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="libraryFolder">Library folder</param>
        /// <param name="siteName">Site name</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        protected virtual byte[] GetImageThumbnailInternal(MediaFileInfo fileInfo, string libraryFolder, string siteName, int width, int height, int maxSideSize)
        {
            if (fileInfo == null)
            {
                return null;
            }

            byte[] thumbnail = null;
            byte[] data = null;

            if (ImageHelper.IsImage(fileInfo.FileExtension))
            {
                // Get new dimensions
                int originalWidth = fileInfo.FileImageWidth;
                int originalHeight = fileInfo.FileImageHeight;
                int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

                // Ensure the source data
                data = GetFileInternal(fileInfo, libraryFolder, siteName);

                // Resize the image
                if (data != null)
                {
                    ImageHelper imgHelper = new ImageHelper(data, originalWidth, originalHeight);
                    thumbnail = imgHelper.GetResizedImageData(newDims[0], newDims[1], ThumbnailQuality);
                }
            }

            // If no thumbnail created, return original size
            return thumbnail ?? data;
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder</param>
        protected virtual byte[] GetFileInternal(MediaFileInfo fileInfo, string libraryFolder, string siteName)
        {
            byte[] fileContent = null;

            if (fileInfo != null)
            {
                // Get the file path            
                string filePath = GetMediaFilePath(siteName, libraryFolder, fileInfo.FilePath);
                if (File.Exists(filePath))
                {
                    try
                    {
                        // Get file contents from file system
                        fileContent = File.ReadAllBytes(filePath);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("MediaFile", "GetFile", ex);
                    }
                }
            }

            return fileContent;
        }

        #endregion


        #region "Internal methods - Physical path"

        /// <summary>
        /// Returns physical path to the media file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="filePath">File path within the library folder</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        protected virtual string GetMediaFilePathInternal(string siteName, string libraryFolder, string filePath, string webFullPath)
        {
            if (!String.IsNullOrEmpty(filePath))
            {
                string folderPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(siteName, libraryFolder, webFullPath);

                return DirectoryHelper.CombinePath(folderPath, MediaLibraryHelper.EnsurePhysicalPath(filePath));
            }

            return null;
        }


        /// <summary>
        /// Returns physical path to the thumbnail folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="filePath">File path from database</param>
        /// <param name="libraryId">Media library ID</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        protected virtual string GetThumbnailPathInternal(string siteName, string filePath, int libraryId, string webFullPath = null)
        {
            if (!String.IsNullOrEmpty(filePath))
            {
                MediaLibraryInfo mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryId);
                if (mli != null)
                {
                    // Get site name from media library
                    if (String.IsNullOrEmpty(siteName))
                    {
                        SiteInfo si = SiteInfoProvider.GetSiteInfo(mli.LibrarySiteID);
                        if (si == null)
                        {
                            return null;
                        }
                        siteName = si.SiteName;
                    }

                    string hiddenFolderName = MediaLibraryHelper.GetMediaFileHiddenFolder(siteName);
                    string filePhysicalPath = GetMediaFilePath(siteName, mli.LibraryFolder, filePath, webFullPath);
                    return DirectoryHelper.CombinePath(Path.GetDirectoryName(filePhysicalPath), hiddenFolderName);
                }
            }
            return null;
        }


        /// <summary>
        /// Returns physical path to the thumbnail.
        /// </summary>
        /// <param name="siteName">Site name of the site thumbnail is related to</param>
        /// <param name="fileName">File name of the file thumbnail is related to</param>
        /// <param name="fileExtension">Extension of the file thumbnail is related to</param>
        /// <param name="path">Path of the original file</param>
        /// <param name="width">Width of the thumbnail file</param>
        /// <param name="height">Height of the thumbnail file</param>
        /// <param name="addHiddenFolder">Indicates if hidden folder should be inserted into path</param>
        /// <param name="addFileExtension">Indicates if file extension should be inserted into thumbnail file name</param>
        protected virtual string GetThumbnailPathInternal(string siteName, string fileName, string fileExtension, string path, int width, int height, bool addHiddenFolder, bool addFileExtension)
        {
            if ((!String.IsNullOrEmpty(fileName)) && (!String.IsNullOrEmpty(fileExtension)))
            {
                string filepath;

                if (addHiddenFolder)
                {
                    string hiddenFolderName = MediaLibraryHelper.GetMediaFileHiddenFolder(siteName);

                    // Insert hidden folder into file path
                    if (String.IsNullOrEmpty(path))
                    {
                        filepath = hiddenFolderName;
                    }
                    else
                    {
                        filepath = path.EndsWithCSafe(hiddenFolderName) ? path : DirectoryHelper.CombinePath(path, hiddenFolderName);
                    }
                }
                else
                {
                    filepath = path;
                }

                // Add file extension if needed
                if (addFileExtension)
                {
                    fileName = String.Format("{0}_{1}", fileName, fileExtension.ToLowerCSafe().TrimStart('.'));
                }

                // Get thumbnail file path
                string thumbnailPath = GetFilePhysicalPath(ImageHelper.GetImageThumbnailFileName(fileName, width, height), fileExtension, filepath);

                return thumbnailPath.Trim('\\');
            }

            return null;
        }


        /// <summary>
        /// Returns preview file path for search pattern (.* as extension).
        /// </summary>
        /// <param name="filePath">File path from database</param>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryId">Library ID</param>
        protected virtual string GetPreviewFilePathInternal(string filePath, string siteName, int libraryId)
        {
            // Get preview file folder path and setup it correctly
            string path = Path.GetDirectoryName(filePath);
            string hiddenFolderName = MediaLibraryHelper.GetMediaFileHiddenFolder(siteName);
            if (path.EndsWithCSafe(hiddenFolderName))
            {
                path = path.Remove(path.Length - hiddenFolderName.Length);
            }
            if ((path != string.Empty) && (!path.EndsWithCSafe("\\")))
            {
                path += "\\";
            }

            // Get preview file path search pattern
            return DirectoryHelper.CombinePath(MediaLibraryInfoProvider.GetMediaLibraryFolderPath(libraryId), path, hiddenFolderName, MediaLibraryHelper.GetPreviewFileName(Path.GetFileNameWithoutExtension(filePath), Path.GetExtension(filePath), ".*", siteName));
        }

        #endregion


        #region "Internal methods - Image"

        /// <summary>
        /// Returns true if image should be scaled down.
        /// </summary>
        /// <param name="maxSideSize">Max side size</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        /// <param name="imageWidth">Image width</param>
        /// <param name="imageHeight">Image height</param>
        protected virtual bool ShouldResizeInternal(int maxSideSize, int width, int height, int imageWidth, int imageHeight)
        {
            // Resize only when bigger than required 
            if (maxSideSize > 0)
            {
                if ((maxSideSize < imageWidth) || (maxSideSize < imageHeight))
                {
                    return true;
                }
            }
            else
            {
                if ((width > 0) && (imageWidth > width))
                {
                    return true;
                }
                if ((height > 0) && (imageHeight > height))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns image thumbnail from the disk or create a new one if doesn't exist yet.
        /// </summary>
        /// <param name="originalFilePath">Original file path</param>
        /// <param name="originalWidth">Original width</param>
        /// <param name="originalHeight">Original height</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        protected virtual byte[] GetThumbnailInternal(string originalFilePath, int originalWidth, int originalHeight, int width, int height)
        {
            if (originalFilePath == null)
            {
                return null;
            }

            byte[] thumbnail = null;

            if ((File.Exists(originalFilePath)) && (ImageHelper.IsImage(Path.GetExtension(originalFilePath))))
            {
                byte[] data = File.ReadAllBytes(originalFilePath);

                // Resize the image
                if (data != null)
                {
                    ImageHelper imgHelper = new ImageHelper(data, originalWidth, originalHeight);
                    thumbnail = imgHelper.GetResizedImageData(width, height, ThumbnailQuality);
                }
            }
            return thumbnail;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Saves media file to disk and returns the applied file path.
        /// </summary>
        /// <param name="siteName">Name of the site of the media library</param>
        /// <param name="libraryFolder">Media library root folder</param>
        /// <param name="librarySubFolderPath">Subfolder path</param>
        /// <param name="fileName">Name of the file</param>
        /// <param name="fileExtension">Extension of the file</param>
        /// <param name="ensureUniqueFileName">Indicates if unique file name should be ensured</param>
        /// <param name="filePath">New file path</param>
        private string CheckAndEnsureFilePath(string siteName, string libraryFolder, string librarySubFolderPath, string fileName, string fileExtension, bool ensureUniqueFileName, out string filePath)
        {
            string filesFolderPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(siteName, libraryFolder);

            if (String.IsNullOrEmpty(filesFolderPath))
            {
                throw new Exception("[MediaFileInfoProvider.CheckAndEnsureFilePath]: Physical library path doesn't exist.");
            }

            string completeFolder = filesFolderPath;

            // Append subfolder path
            librarySubFolderPath = librarySubFolderPath.TrimStart('\\');
            if (!String.IsNullOrEmpty(librarySubFolderPath))
            {
                completeFolder = DirectoryHelper.CombinePath(filesFolderPath, librarySubFolderPath);
            }

            // Check permission for specified folder
            if (!DirectoryHelper.CheckPermissions(completeFolder))
            {
                throw new PermissionException(String.Format("[MediaFileInfoProvider.CheckAndEnsureFilePath]: Access to the path '{0}' is denied.", filesFolderPath));
            }

            // Get file path                                
            filePath = DirectoryHelper.CombinePath(completeFolder, fileName) + fileExtension;

            // Check unique file name and update info on file name if required
            if (ensureUniqueFileName)
            {
                filePath = MediaLibraryHelper.EnsureUniqueFileName(filePath);
            }

            string newFileName = Path.GetFileName(filePath);
            string fileSubFolderPath = (librarySubFolderPath != string.Empty) ? DirectoryHelper.CombinePath(librarySubFolderPath, newFileName) : newFileName;

            // Ensure disk path
            DirectoryHelper.EnsureDiskPath(filePath, MediaLibraryHelper.GetMediaRootFolderPath(siteName));

            return Path.EnsureSlashes(fileSubFolderPath);
        }


        /// <summary>
        /// Returns physical path to the file.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="extension">File extension</param>
        /// <param name="path">File path</param>
        private string GetFilePhysicalPath(string fileName, string extension, string path)
        {
            // Returns path to the thumbnail according destination of original file
            return String.Format("{0}.{1}", DirectoryHelper.CombinePath(MediaLibraryHelper.EnsurePhysicalPath(path), fileName), extension.TrimStart('.'));
        }


        private static string GetFileNameSuffix(string newPath, string originalPhysicalPath, string hiddenName)
        {
            var hiddenFileNameSuffix = hiddenName.Substring(Path.GetFileNameWithoutExtension(originalPhysicalPath).Length);

            string origExtension = Path.GetExtension(originalPhysicalPath).ToLowerCSafe().TrimStart('.');
            string newExtension = Path.GetExtension(newPath).ToLowerCSafe().TrimStart('.');

            // Extension pattern (e.g. _jpg_) must be replaced with new one if old extension and new extension are different. 
            if (origExtension != newExtension)
            {
                hiddenFileNameSuffix = hiddenFileNameSuffix.Replace(string.Format("_{0}_", origExtension), string.Format("_{0}_", newExtension));
            }

            return hiddenFileNameSuffix;
        }

        #endregion
    }
}
 
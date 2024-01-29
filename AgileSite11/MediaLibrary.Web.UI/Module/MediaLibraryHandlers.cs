using System;
using System.Web;
using System.Text.RegularExpressions;

using CMS.Base;
using CMS.DataEngine;
using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Helper class for media library dialog's event handlers initialization.
    /// </summary>
    internal class MediaLibraryHandlers
    {
        #region "Methods"

        /// <summary>
        /// Initializes the event handler for getting media data.
        /// </summary>
        public static void Init()
        {
            DialogHandlers.GetMediaData.Before += GetMediaData_Before;
        }

        #endregion


        #region "Event handlers"

        /// <summary>
        /// GetMediaData event handler.
        /// </summary>
        protected static void GetMediaData_Before(object sender, GetMediaDataEventArgs e)
        {
            string url = e.Url.ToLowerCSafe();
            string siteName = e.SiteName;

            // If get media type of URL
            if ((url.Contains("getmediafile.aspx")) || (url.Contains("getmedia")))
            {
                Match m = CMSDialogHelper.GuidReg.Match(url);
                if (m.Success)
                {
                    Guid guid = new Guid(m.Groups[0].Value);

                    // Get source from media library
                    var mediaSource = GetMediaSource(guid, siteName);
                    if (mediaSource == null)
                    {
                        e.MediaSource = new MediaSource();
                    }
                    else if (mediaSource.SourceType == MediaSourceEnum.MediaLibraries)
                    {
                        e.MediaSource = mediaSource;
                    }
                }

                e.EventHandled = true;
            }

            // URL leads to Azure or Amazon storage
            if (url.Contains("getazurefile.aspx") || url.Contains("getamazonfile.aspx"))
            {
                // Convert file path to URL
                string path = URLHelper.GetUrlParameter(url, "path");
                url = path.Replace(@"\", "/");
            }

            // Check URL for media library folder
            string mediaLibraryFolder = SettingsKeyInfoProvider.GetValue(siteName + ".CMSMediaLibrariesFolder");
            if (String.IsNullOrEmpty(mediaLibraryFolder))
            {
                mediaLibraryFolder = "/media/";
            }
            else
            {
                // Check if site specific folder should be used
                if (MediaLibraryHelper.UseMediaLibrariesSiteFolder(siteName))
                {
                    mediaLibraryFolder = String.Format("/{0}/{1}/", mediaLibraryFolder.TrimStart('~').Trim('/'), siteName);
                }
                else
                {
                    mediaLibraryFolder = String.Format("/{0}/", mediaLibraryFolder.TrimStart('~').Trim('/'));
                }
            }

            // Ensure toLower
            mediaLibraryFolder = mediaLibraryFolder.ToLowerCSafe();

            // If media library direct URL 
            if (url.Contains(mediaLibraryFolder))
            {
                string mediaUrl = url;

                int queryIndex = url.IndexOfCSafe('?');
                if (queryIndex > 0)
                {
                    mediaUrl = mediaUrl.Remove(queryIndex);
                }

                int index = mediaUrl.IndexOfCSafe(mediaLibraryFolder);
                mediaUrl = mediaUrl.Substring(index + mediaLibraryFolder.Length);
                int slashIndex = mediaUrl.IndexOfCSafe('/');
                string filePath = mediaUrl.Substring(slashIndex + 1);
                string libraryFolder = "";
                if (slashIndex > 0)
                {
                    // Not in root of library
                    libraryFolder = mediaUrl.Substring(0, slashIndex);
                }

                // Get source from media library
                var mediaSource = GetMediaSource(HttpUtility.UrlDecode(filePath), siteName, libraryFolder);
                if (mediaSource == null)
                {
                    e.MediaSource = new MediaSource();
                }
                else if (mediaSource.SourceType == MediaSourceEnum.MediaLibraries)
                {
                    e.MediaSource = mediaSource;
                }

                e.EventHandled = true;
            }

        }

        #endregion

        
        #region "Private methods"

        /// <summary>
        /// Returns MediaSource for given media file.
        /// </summary>
        /// <param name="fileGuid">Media file GUID</param>
        /// <param name="siteName">Site name</param>
        private static MediaSource GetMediaSource(Guid fileGuid, string siteName)
        {
            var mfi = MediaFileInfoProvider.GetMediaFileInfo(fileGuid, siteName);
            return GetMediaSource(mfi);
        }


        /// <summary>
        /// Returns MediaSource for given media file.
        /// </summary>
        /// <param name="filePath">Media file path</param>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        private static MediaSource GetMediaSource(string filePath, string siteName, string libraryFolder)
        {
            var mfi = MediaFileInfoProvider.GetMediaFileInfo(siteName, filePath, libraryFolder);
            return GetMediaSource(mfi);
        }

        
        /// <summary>
        /// Returns MediaSource for given media file.
        /// </summary>
        /// <param name="mfi">Media file data</param>
        private static MediaSource GetMediaSource(MediaFileInfo mfi)
        {
            if (mfi == null)
            {
                return null;
            }

            var mli = MediaLibraryInfoProvider.GetMediaLibraryInfo(mfi.FileLibraryID);
            if (mli == null)
            {
                return null;
            }

            var source = new MediaSource
            {
                MediaFileGuid = mfi.FileGUID,
                MediaFileID = mfi.FileID,
                MediaFilePath = mfi.FilePath,
                MediaFileLibraryID = mli.LibraryID,
                MediaFileLibraryGroupID = mli.LibraryGroupID,
                SourceType = MediaSourceEnum.MediaLibraries,
                Extension = mfi.FileExtension,
                MediaWidth = mfi.FileImageWidth,
                MediaHeight = mfi.FileImageHeight,
                FileSize = mfi.FileSize,
                FileName = mfi.FileName,
                SiteID = mli.LibrarySiteID
            };

            return source;
        }

        #endregion
    }
}

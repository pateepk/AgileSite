using System;

using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.MediaLibrary.Web.UI
{
    /// <summary>
    /// Summary description for MediaLibraryTransformationFunctions.
    /// </summary>
    public class MediaLibraryTransformationFunctions
    {
        /// <summary>
        /// Returns direct URL to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="libraryId">Media library ID</param>
        /// <param name="filePath">File path</param>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        /// <param name="useSecureLinks">Generate secure link</param>
        /// <param name="downloadlink">Determines whether disposition parameter should be added to permanent link</param>
        public static string GetMediaFileUrl(object libraryId, object filePath, object fileGuid, object fileName, object useSecureLinks, object downloadlink)
        {
            string url = GetMediaFileUrl(libraryId, filePath, fileGuid, fileName, useSecureLinks);
            if (ValidationHelper.GetBoolean(useSecureLinks, true) && ValidationHelper.GetBoolean(downloadlink, true))
            {
                url = URLHelper.AddParameterToUrl(url, "disposition", "attachment");
            }
            return url;
        }


        /// <summary>
        /// Returns direct URL to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="libraryId">Media library ID</param>
        /// <param name="filePath">File path</param>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        /// <param name="useSecureLinks">Generate secure link</param>
        public static string GetMediaFileUrl(object libraryId, object filePath, object fileGuid, object fileName, object useSecureLinks)
        {
            MediaLibraryInfo libInfo = MediaLibraryInfoProvider.GetMediaLibraryInfo(ValidationHelper.GetInteger(libraryId, 0));

            if (libInfo == null || filePath == null || fileName == null)
            {
                return String.Empty;
            }

            if (ValidationHelper.GetBoolean(useSecureLinks, true))
            {
                return MediaFileInfoProvider.GetMediaFileUrl(ValidationHelper.GetGuid(fileGuid, Guid.Empty), fileName.ToString());
            }

            return MediaFileInfoProvider.GetMediaFileUrl(SiteContext.CurrentSiteName, libInfo.LibraryFolder, filePath.ToString());
        }


        /// <summary>
        /// Returns direct URL to the media file, user permissions are not checked.
        /// </summary>
        /// <param name="libraryId">Media library ID</param>
        /// <param name="filePath">File path</param>
        public static string GetMediaFileDirectUrl(object libraryId, object filePath)
        {
            MediaLibraryInfo libInfo = MediaLibraryInfoProvider.GetMediaLibraryInfo(ValidationHelper.GetInteger(libraryId, 0));

            if (libInfo == null || filePath == null)
            {
                return String.Empty;
            }

            return MediaFileInfoProvider.GetMediaFileUrl(SiteContext.CurrentSiteName, libInfo.LibraryFolder, filePath.ToString());
        }


        /// <summary>
        /// Returns URL to media file which is rewritten to calling GetMediaFile.aspx page where user permissions are checked.
        /// </summary>
        /// <param name="fileGuid">File GUID</param>
        /// <param name="fileName">File name</param>
        public static string GetMediaFileUrl(object fileGuid, object fileName)
        {
            if (fileName == null)
            {
                return String.Empty;
            }

            return MediaFileInfoProvider.GetMediaFileUrl(ValidationHelper.GetGuid(fileGuid, Guid.Empty), fileName.ToString());
        }


        /// <summary>
        /// Returns URL to detail of media file.
        /// </summary>
        /// <param name="fileId">File ID</param>
        public static string GetMediaFileDetailUrl(object fileId)
        {
            return GetMediaFileDetailUrl("fileId", fileId);
        }


        /// <summary>
        /// Returns URL to detail of media file.
        /// </summary>
        /// <param name="parameter">Query parameter</param>
        /// <param name="fileId">File ID</param>
        public static string GetMediaFileDetailUrl(string parameter, object fileId)
        {
            if (fileId != null)
            {
                return URLHelper.UpdateParameterInUrl(
                    URLHelper.RemoveProtocolAndDomain(RequestContext.CurrentURL), 
                    parameter, 
                    fileId.ToString());
            }
            return String.Empty;
        }
    }
}
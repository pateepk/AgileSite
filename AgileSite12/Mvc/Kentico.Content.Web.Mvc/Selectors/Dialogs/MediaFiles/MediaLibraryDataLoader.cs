using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.MediaLibrary;
using CMS.Membership;

using Kentico.Content.Web.Mvc;
using Kentico.Web.Mvc;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Loads data for media files selector.
    /// </summary>
    internal class MediaLibraryDataLoader : IMediaLibraryDataLoader
    {
        private const int THUMBNAIL_SIZE_SMALL = 70;
        private const int THUMBNAIL_SIZE_MEDIUM = 160;
        private const int THUMBNAIL_SIZE_LARGE = 320;


        /// <summary>
        /// Loads files from given library in given folder.
        /// </summary>
        /// <param name="libraryName">Media library name.</param>
        /// <param name="folderPath">Path of the selected folder.</param>
        /// <param name="allowedExtensions">Semicolon separated list of allowed file extensions.</param>
        /// <param name="urlHelper">URL helper.</param>
        public IEnumerable<MediaLibraryFileItem> LoadFiles(string libraryName, string folderPath, string allowedExtensions, UrlHelper urlHelper)
        {
            var library = GetLibrary(libraryName);
            if (library == null)
            {
                return Enumerable.Empty<MediaLibraryFileItem>();
            }

            if (urlHelper == null)
            {
                throw new ArgumentNullException(nameof(urlHelper));
            }

            var query = MediaFileInfoProvider.GetMediaFiles()
                .WhereEquals("FileLibraryID", library.LibraryID)
                .WhereStartsWith("FilePath", folderPath)
                .Where("FilePath", QueryOperator.NotLike, $"{SqlHelper.EscapeLikeQueryPatterns(folderPath)}%/%")
                .Where(new MediaFilesSelectorAllowedExtensions(allowedExtensions).GetWhereCondition());

            return query.ToList()
                .Select(file =>
                {
                    return GetItem(file, urlHelper);
                })
                .OrderBy(file => file.Name);
        }


        /// <summary>
        /// Loads files with given file GUIDs filtered by permissions.
        /// </summary>
        /// <param name="fileGUIDs">List of file GUIDs.</param>
        /// <param name="urlHelper">URL helper.</param>
        public IEnumerable<MediaLibraryFileItem> LoadFiles(IEnumerable<Guid> fileGUIDs, UrlHelper urlHelper)
        {
            if (urlHelper == null)
            {
                throw new ArgumentNullException(nameof(urlHelper));
            }

            var fileGUIDsList = fileGUIDs.ToList();
            if (fileGUIDsList.Count == 0)
            {
                return Enumerable.Empty<MediaLibraryFileItem>();
            }

            var siteService = Service.Resolve<ISiteService>();
            var files = MediaFileInfoProvider.GetMediaFiles()
                .WhereIn("FileGUID", fileGUIDsList)
                .OnSite(siteService.CurrentSite.SiteName)
                .ToList();

            var libraryIds = files
                .Select(file => file.FileLibraryID)
                .Distinct();

            var librariesPermissions = new Dictionary<string, bool>();
            foreach (var id in libraryIds)
            {
                var library = MediaLibraryInfoProvider.GetMediaLibraryInfo(id);
                librariesPermissions.Add(library.LibraryName, CheckLibraryAccess(library));
            }

            return fileGUIDs.Select(fileGuid =>
            {
                var file = files.FirstOrDefault(f => f.FileGUID == fileGuid);
                if (file != null)
                {
                    var item = GetItem(file, urlHelper);
                    item.IsValid = librariesPermissions[item.LibraryName];
                    return item;
                }
                return GetDummyItem(fileGuid);
            }).OrderBy(file => file.Name);
        }


        /// <summary>
        /// Loads media libraries for current site filtered by permissions.
        /// </summary>
        public IEnumerable<MediaLibraryItem> LoadLibraries()
        {
            var siteService = Service.Resolve<ISiteService>();
            var libraries = MediaLibraryInfoProvider.GetMediaLibraries()
                .OnSite(siteService.CurrentSite.SiteID)
                .ToList()
                .Where(library => CheckLibraryAccess(library));

            return libraries.Select(library => new MediaLibraryItem
            {
                Name = library.LibraryDisplayName,
                Identifier = library.LibraryName,
                CreateFile = MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(library, "FileCreate", MembershipContext.AuthenticatedUser)
            })
            .OrderBy(library => library.Name);
        }


        private static MediaLibraryInfo GetLibrary(string libraryName)
        {
            var siteService = Service.Resolve<ISiteService>();
            return MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryName, siteService.CurrentSite.SiteName);
        }


        private static bool CheckLibraryAccess(MediaLibraryInfo library)
        {
            return MediaLibraryInfoProvider.IsUserAuthorizedPerLibrary(library, "LibraryAccess", MembershipContext.AuthenticatedUser);
        }


        private MediaLibraryFileItem GetDummyItem(Guid fileGUID)
        {
            return new MediaLibraryFileItem
            {
                FileGUID = fileGUID,
                IsValid = false
            };
        }


        private MediaLibraryFileItem GetItem(MediaFileInfo file, UrlHelper urlHelper)
        {
            var fileGuid = file.FileGUID;
            var fileName = AttachmentHelper.GetFullFileName(file.FileName, file.FileExtension);
            var url = urlHelper.Kentico().AuthenticateUrl(MediaFileURLProvider.GetMediaFileUrl(fileGuid, fileName)).ToString();

            return new MediaLibraryFileItem
            {
                FileGUID = fileGuid,
                Name = file.FileName,
                Extension = file.FileExtension,
                Url = urlHelper.Content(url),
                ThumbnailUrls = GetThumbnailUrls(urlHelper, file, url),
                MimeType = file.FileMimeType,
                Size = file.FileSize,
                Title = file.FileTitle,
                Description = file.FileDescription,
                LibraryName = file.Parent.Generalized.ObjectCodeName,
                SiteName = file.Parent.Generalized.ObjectSiteName,
                FolderPath = GetPath(file.FilePath)
            };
        }


        private string GetPath(string path)
        {
            int lastSlash = path.LastIndexOf('/');
            if (lastSlash > 0)
            {
                return path.Substring(0, lastSlash);
            }

            return string.Empty;
        }


        private MediaLibraryFileThumbnails GetThumbnailUrls(UrlHelper urlHelper, MediaFileInfo file, string url)
        {
            if (!ImageHelper.IsImage(file.FileExtension))
            {
                return null;
            }

            return new MediaLibraryFileThumbnails
            {
                Small = urlHelper.Kentico().ImageUrl(url, SizeConstraint.MaxWidthOrHeight(THUMBNAIL_SIZE_SMALL)),
                Medium = urlHelper.Kentico().ImageUrl(url, SizeConstraint.MaxWidthOrHeight(THUMBNAIL_SIZE_MEDIUM)),
                Large = urlHelper.Kentico().ImageUrl(url, SizeConstraint.MaxWidthOrHeight(THUMBNAIL_SIZE_LARGE)),
            };
        }
    }
}

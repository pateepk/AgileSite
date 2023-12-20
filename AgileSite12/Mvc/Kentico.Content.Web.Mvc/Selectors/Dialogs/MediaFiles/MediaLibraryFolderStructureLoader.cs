using System;
using System.Collections.Generic;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.IO;
using CMS.MediaLibrary;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Loads folder structure of a media library.
    /// </summary>
    internal class MediaLibraryFolderStructureLoader : IMediaLibraryFolderStructureLoader
    {
        private string mHiddenFolder;


        /// <summary>
        /// Loads folder structure of a media library.
        /// </summary>
        /// <param name="libraryName">Media library name.</param>
        /// <remarks>Hidden folders are skipped.</remarks>
        public MediaLibraryFolderNode Load(string libraryName)
        {
            var library = GetLibrary(libraryName);
            if (library == null)
            {
                return new MediaLibraryFolderNode
                {
                    Name = libraryName,
                    Children = new List<MediaLibraryFolderNode>(),
                    Path = string.Empty
                };
            }

            var mediaLibraryStructure = new MediaLibraryFolderNode
            {
                Name = library.LibraryDisplayName,
                Children = new List<MediaLibraryFolderNode>(),
                Path = string.Empty
            };

            var fullLibraryPath = GetFullLibraryPath(library);
            var subdirectories = Directory.GetDirectories(fullLibraryPath);
            foreach (string subdirectory in subdirectories)
            {
                LoadSubDirectories(library, subdirectory, mediaLibraryStructure);
            }

            return mediaLibraryStructure;
        }


        private static MediaLibraryInfo GetLibrary(string libraryName)
        {
            var siteService = Service.Resolve<ISiteService>();
            return MediaLibraryInfoProvider.GetMediaLibraryInfo(libraryName, siteService.CurrentSite.SiteName);
        }


        private void LoadSubDirectories(MediaLibraryInfo library, string dir, MediaLibraryFolderNode parent)
        {
            var folderName = Path.GetFileName(dir);

            if (IsHiddenFolder(library, folderName))
            {
                return;
            }

            var child = new MediaLibraryFolderNode
            {
                Name = folderName,
                Children = new List<MediaLibraryFolderNode>(),
                Path = $"{parent.Path}/{folderName}".TrimStart('/')
            };
            parent.Children.Add(child);

            var subdirectories = Directory.GetDirectories(dir);
            foreach (string directory in subdirectories)
            {
                LoadSubDirectories(library, directory, child);
            }
        }


        private bool IsHiddenFolder(MediaLibraryInfo library, string folderName)
        {
            string hiddenFolder = GetHiddenFolder(library);
            return string.Equals(folderName, hiddenFolder, StringComparison.InvariantCultureIgnoreCase);
        }


        private string GetHiddenFolder(MediaLibraryInfo library)
        {
            if (string.IsNullOrEmpty(mHiddenFolder))
            {
                string siteName = new SiteInfoIdentifier(library.LibrarySiteID);
                mHiddenFolder = MediaLibraryHelper.GetMediaFileHiddenFolder(siteName);
            }

            return mHiddenFolder;
        }


        private static string GetFullLibraryPath(MediaLibraryInfo library)
        {
            string siteName = new SiteInfoIdentifier(library.LibrarySiteID);
            var mediaLibraryFolder = MediaLibraryHelper.GetMediaRootFolderPath(siteName);
            var fullLibraryPath = DirectoryHelper.CombinePath(mediaLibraryFolder, library.LibraryFolder);
            return fullLibraryPath;
        }
    }
}

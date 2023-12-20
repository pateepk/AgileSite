using System;
using System.Collections.Generic;
using System.Web.Mvc;

namespace Kentico.Components.Web.Mvc.Dialogs
{
    /// <summary>
    /// Loads data for media files selector.
    /// </summary>
    internal interface IMediaLibraryDataLoader
    {
        /// <summary>
        /// Loads files from given library in given folder.
        /// </summary>
        /// <param name="libraryName">Media library name.</param>
        /// <param name="folderPath">Path of the selected folder.</param>
        /// <param name="allowedExtensions">Semicolon separated list of allowed file extensions.</param>
        /// <param name="urlHelper">URL helper.</param>
        IEnumerable<MediaLibraryFileItem> LoadFiles(string libraryName, string folderPath, string allowedExtensions, UrlHelper urlHelper);


        /// <summary>
        /// Loads files with given file GUIDs filtered by permissions.
        /// </summary>
        /// <param name="fileGUIDs">List of file GUIDs.</param>
        /// <param name="urlHelper">URL helper.</param>
        IEnumerable<MediaLibraryFileItem> LoadFiles(IEnumerable<Guid> fileGUIDs, UrlHelper urlHelper);



        /// <summary>
        /// Loads media libraries for current site filtered by permissions.
        /// </summary>
        IEnumerable<MediaLibraryItem> LoadLibraries();
    }
}
using System;
using System.Collections.Generic;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.MediaLibrary;
using CMS.Membership;
using CMS.Base;
using CMS.SiteProvider;

using ITHit.WebDAV.Server;
using ITHit.WebDAV.Server.Response;

namespace CMS.WebDAV
{
    /// <summary>
    /// Class that represents media folder.
    /// </summary>
    internal class MediaFolder : BaseFolder, IFolder
    {
#pragma warning disable BH1014 // Do not use System.IO
        #region "Constructors"

        /// <summary>
        /// Creates media folder.
        /// </summary>
        /// <param name="path">URL path of media folder</param>
        /// <param name="name">Media folder name</param>
        /// <param name="created">Creation date</param>
        /// <param name="modified">Modification date</param>
        /// <param name="urlParser">Url parser</param>
        /// <param name="engine">WebDAV engine</param>
        /// <param name="parent">Parent media folder</param>
        public MediaFolder(string path, string name, DateTime created, DateTime modified, UrlParser urlParser, WebDAVEngine engine, IFolder parent)
            : base(path, name, created, modified, urlParser, engine, parent)
        {
        }

        #endregion


        #region "IFolder Members"

        /// <summary>
        /// Gets the array of this folder's children.
        /// </summary>
        public IHierarchyItem[] Children
        {
            get
            {
                List<IHierarchyItem> childrenItems = new List<IHierarchyItem>();

                switch (UrlParser.ItemType)
                {
                    case ItemTypeEnum.MediaLibraryName:
                        {
                            MediaLibraryInfo library = UrlParser.MediaLibraryInfo;

                            if (library != null)
                            {
                                // Check authorization to read or manage
                                if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(library, new string[] { "read", "manage" }))
                                {
                                    string siteName = SiteContext.CurrentSiteName;

                                    // Get physical library path
                                    string libraryPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(library.LibraryID);

                                    string urlPath = libraryPath;

                                    if (UrlParser.FilePath != null)
                                    {
                                        urlPath += "/" + UrlParser.PhysicalFilePath;
                                    }

                                    // Create directory from url path
                                    DirectoryInfo di = DirectoryInfo.New(urlPath);

                                    // Get hidden and preview folders
                                    string hiddenFolder = MediaLibraryHelper.GetMediaFileHiddenFolder(siteName);
                                    string previewFolder = MediaLibraryHelper.GetMediaFilePreviewSuffix(siteName);

                                    // Gets subdirectories
                                    foreach (DirectoryInfo directoryInfo in di.GetDirectories())
                                    {
                                        if ((directoryInfo.Name != hiddenFolder) && (directoryInfo.Name != previewFolder))
                                        {
                                            // Create media folder
                                            string folderPath = Path + "/" + directoryInfo.Name;
                                            MediaFolder subFolder = new MediaFolder(folderPath, directoryInfo.Name, directoryInfo.CreationTime, directoryInfo.LastWriteTime, null, Engine, this);
                                            childrenItems.Add(subFolder);
                                        }
                                    }

                                    // Gets files
                                    foreach (FileInfo fi in di.GetFiles())
                                    {
                                        string path = Path + "/" + fi.Name;
                                        string mimeType = MimeTypeHelper.GetMimetype(fi.Extension);

                                        // Create media file resource
                                        MediaFileResource mediaResource = new MediaFileResource(path, fi.Name, mimeType, fi.Length, fi.CreationTime, fi.LastWriteTime, null, Engine, this);
                                        childrenItems.Add(mediaResource);
                                    }
                                }
                            }
                        }
                        break;

                    // Nothing
                    default:
                        break;
                }

                return childrenItems.ToArray();
            }
        }


        /// <summary>
        /// Creates new WebDAV resource with the specified name in this folder. 
        /// </summary>
        /// <param name="resourceName">Name of the resource to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateResource(string resourceName)
        {
            MediaLibraryInfo library = UrlParser.MediaLibraryInfo;

            // Check 'FileCreate'
            if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(library, "filecreate"))
            {
                string siteName = SiteContext.CurrentSiteName;
                string safeFileName = WebDAVHelper.GetSafeFileName(resourceName, siteName, WebDAVHelper.MAX_MEDIA_FILENAME_LENGTH);
                string fileExtension = IO.Path.GetExtension(safeFileName);

                // Check whether the extension of the media file is allowed
                if (WebDAVHelper.IsMediaFileExtensionAllowedForBrowseMode(fileExtension, SiteContext.CurrentSiteName))
                {
                    // Get subpath
                    string librarySubFolderPath = UrlParser.FilePath;

                    string filesFolderPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(siteName, library.LibraryFolder);
                    string newFileName = null;

                    if (!string.IsNullOrEmpty(filesFolderPath))
                    {
                        string completeFolder = filesFolderPath;

                        // Append subfolder path
                        if (!string.IsNullOrEmpty(librarySubFolderPath))
                        {
                            librarySubFolderPath = librarySubFolderPath.TrimEnd('/');

                            completeFolder = DirectoryHelper.CombinePath(filesFolderPath, librarySubFolderPath);
                        }

                        // Get file path              
                        string filePath = DirectoryHelper.CombinePath(completeFolder, safeFileName);
                        newFileName = IO.Path.GetFileName(filePath);
                    }

                    string path = null;
                    if (string.IsNullOrEmpty(librarySubFolderPath))
                    {
                        path = newFileName;
                    }
                    else
                    {
                        path = librarySubFolderPath + "/" + newFileName;
                    }

                    string fileName = IO.Path.GetFileNameWithoutExtension(newFileName);

                    // Create new media file info
                    MediaFileInfo mfi = new MediaFileInfo();
                    mfi.FileExtension = fileExtension;
                    mfi.FileBinary = new byte[0];
                    mfi.FileSize = 0;
                    mfi.FileMimeType = MimeTypeHelper.GetMimetype(fileExtension);
                    mfi.FileTitle = string.Empty;
                    mfi.FileName = fileName;
                    mfi.FileDescription = string.Empty;
                    if (path != null)
                    {
                        mfi.FilePath = path.Trim('/');
                    }
                    mfi.FileLibraryID = library.LibraryID;
                    mfi.FileSiteID = SiteContext.CurrentSiteID;
                    int currentUserID = MembershipContext.AuthenticatedUser.UserID;
                    mfi.FileCreatedByUserID = currentUserID;
                    mfi.FileModifiedByUserID = currentUserID;
                    mfi.FileGUID = Guid.NewGuid();

                    try
                    {
                        // Save media file info
                        MediaFileInfoProvider.SetMediaFileInfo(mfi, false);
                    }
                    catch (Exception ex)
                    {
                        EventLogProvider.LogException("MediaFolder_CreateResource", "WebDAV", ex);
                        return new ServerErrorResponse();
                    }

                    return new CreatedResponse();
                }
            }

            return new AccessDeniedResponse();
        }


        /// <summary>
        /// Creates new WebDAV folder with the specified name in this folder. 
        /// </summary>
        /// <param name="folderName">Name of the folder to create</param>
        /// <returns>WebDAV response</returns>
        public WebDAVResponse CreateFolder(string folderName)
        {
            try
            {
                MediaLibraryInfo mli = UrlParser.MediaLibraryInfo;

                // Check 'Manage' or 'FolderCreate' permission
                if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(mli, "foldercreate"))
                {
                    string libraryPath = MediaLibraryInfoProvider.GetMediaLibraryFolderPath(mli.LibraryID);
                    libraryPath += "/" + UrlParser.FilePath;
                    IO.Path.EnsureBackslashes(libraryPath, true);

                    DirectoryInfo directory = DirectoryInfo.New(libraryPath);
                    directory.CreateSubdirectory(folderName);
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFolder_CreateFolder", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new CreatedResponse();
        }

        #endregion


        #region "IHierarchyItem Members"

        /// <summary>
        /// Moves this item to the destination folder under a new name.
        /// </summary>
        /// <param name="folder">Destination folder</param>
        /// <param name="destName">Name of the destination item</param>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse MoveTo(IFolder folder, string destName)
        {
            MediaFolder destFolder = folder as MediaFolder;
            if (destFolder == null)
            {
                return new ConflictResponse();
            }

            try
            {
                MediaLibraryInfo library = UrlParser.MediaLibraryInfo;
                int destLibraryID = destFolder.UrlParser.MediaLibraryInfo.LibraryID;

                // Move or rename only in the same media library and check 'FolderModify' permission
                if ((destLibraryID == library.LibraryID) && WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(library, "foldermodify"))
                {
                    int index = GetMediaFolderIndex(library.LibraryName, Path);

                    string folderPath = Path.Substring(index);

                    // Get destination folder path
                    string newFolderPath = (destFolder.Path.Length > index) ? destFolder.Path.Substring(index) : "";

                    // Root in media library
                    if (string.IsNullOrEmpty(newFolderPath))
                    {
                        newFolderPath = destName;
                    }
                    // Sub directory
                    else
                    {
                        if (newFolderPath.EndsWithCSafe(System.IO.Path.DirectorySeparatorChar.ToString()) || newFolderPath.EndsWithCSafe(System.IO.Path.AltDirectorySeparatorChar.ToString()))
                        {
                            newFolderPath += destName;
                        }
                        else
                        {
                            newFolderPath += System.IO.Path.DirectorySeparatorChar.ToString() + destName;
                        }
                    }

                    MediaLibraryInfoProvider.RenameMediaLibraryFolder(SiteContext.CurrentSiteName, library.LibraryID, folderPath, newFolderPath, false, false);
                }
                else
                {
                    return new AccessDeniedResponse();
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFolder_MoveTo", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new CreatedResponse();
        }


        /// <summary>
        /// Deletes this folder.
        /// </summary>
        /// <returns>WebDAV response</returns>
        public override WebDAVResponse Delete()
        {
            try
            {
                MediaLibraryInfo mli = UrlParser.MediaLibraryInfo;

                // Check 'FolderDelete' permission
                if (WebDAVHelper.IsCurrentUserAuthorizedPerMediaLibrary(mli, "folderdelete"))
                {
                    string libraryName = mli.LibraryName;

                    // Get folder path
                    string folderPath = Path.Substring(GetMediaFolderIndex(libraryName, Path));

                    // Delete media folder
                    MediaLibraryInfoProvider.DeleteMediaLibraryFolder(SiteContext.CurrentSiteName, mli.LibraryID, folderPath, false);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("MediaFolder_Delete", "WebDAV", ex);
                return new ServerErrorResponse();
            }

            return new NoContentResponse();
        }


        /// <summary>
        /// Gets index determining start of media folder name in given path.
        /// </summary>
        /// <param name="libraryName">Name of media library</param>
        /// <param name="path">Path to examine</param>
        /// <returns>Index determining start of media folder name in given path</returns>
        private int GetMediaFolderIndex(string libraryName, string path)
        {
            string indexPath = path;

            // To trim media library prefix
            int index = path.IndexOf(WebDAVSettings.MediaFilesFolder, StringComparison.Ordinal) + WebDAVSettings.MediaFilesFolder.Length + 1;
            indexPath = indexPath.Substring(index);

            string groupName = UrlParser.GroupName;
            if (!string.IsNullOrEmpty(groupName))
            {
                if (indexPath.StartsWithCSafe(groupName + "/"))
                {
                    int groupIndex = indexPath.IndexOf(groupName + "/", StringComparison.Ordinal) + groupName.Length + 1;
                    index += groupIndex;
                    indexPath = indexPath.Substring(groupIndex);
                }
            }

            // Get folder path
            index += indexPath.IndexOf(libraryName, StringComparison.Ordinal) + libraryName.Length + 1;

            return index;
        }

        #endregion

#pragma warning restore BH1014 // Do not use System.IO
    }
}
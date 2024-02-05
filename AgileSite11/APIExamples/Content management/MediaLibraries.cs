using System;

using CMS.MediaLibrary;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Modules;
using CMS.Helpers;

namespace APIExamples
{
    /// <summary>
    /// Holds API examples related to media libraries.
    /// </summary>
    /// <pageTitle>Media libraries</pageTitle>
    internal class MediaLibrariesMain
    {
        /// <summary>
        /// Holds media library API examples.
        /// </summary>
        /// <groupHeading>Media libraries</groupHeading>
        private class MediaLibraries
        {
            /// <heading>Creating a media library</heading>
            private void CreateMediaLibrary()
            {
                // Creates a new media library object
                MediaLibraryInfo newLibrary = new MediaLibraryInfo();

                // Sets the library properties
                newLibrary.LibraryDisplayName = "New library";
                newLibrary.LibraryName = "NewLibrary";
                newLibrary.LibraryDescription = "This media library was created through the API.";
                newLibrary.LibraryFolder = "NewLibrary";
                newLibrary.LibrarySiteID = SiteContext.CurrentSiteID;

                // Saves the new media library to the database
                MediaLibraryInfoProvider.SetMediaLibraryInfo(newLibrary);
            }


            /// <heading>Updating a media library</heading>
            private void GetAndUpdateMediaLibrary()
            {
                // Gets the media library
                MediaLibraryInfo updateLibrary = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (updateLibrary != null)
                {
                    // Updates the library properties
                    updateLibrary.LibraryDisplayName = updateLibrary.LibraryDisplayName.ToLower();

                    // Saves the updated media library to the database
                    MediaLibraryInfoProvider.SetMediaLibraryInfo(updateLibrary);
                }
            }


            /// <heading>Updating multiple media libraries</heading>
            private void GetAndBulkUpdateMediaLibraries()
            {
                // Gets all media libraries on the current site whose code name starts with 'New'
                var libraries = MediaLibraryInfoProvider.GetMediaLibraries()
                                                            .WhereEquals("LibrarySiteID", SiteContext.CurrentSiteID)
                                                            .WhereStartsWith("LibraryName", "New");

                
                // Loops through individual media libraries
                foreach (MediaLibraryInfo library in libraries)
                {
                    // Updates the library properties
                    library.LibraryDisplayName = library.LibraryDisplayName.ToUpper();

                    // Saves the updated media library to the database
                    MediaLibraryInfoProvider.SetMediaLibraryInfo(library);
                }
            }


            /// <heading>Deleting a media library</heading>
            private void DeleteMediaLibrary()
            {
                // Gets the media library
                MediaLibraryInfo deleteLibrary = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (deleteLibrary != null)
                {
                    // Deletes the media library
                    MediaLibraryInfoProvider.DeleteMediaLibraryInfo(deleteLibrary);
                }
            }
        }


        /// <summary>
        /// Holds media file and folder API examples.
        /// </summary>
        /// <groupHeading>Media folders and files</groupHeading>
        private class MediaFoldersFiles
        {
            /// <heading>Creating a media library folder</heading>
            private void CreateMediaFolder()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (library != null)
                {
                    // Creates the "NewFolder" folder within the media library
                    MediaLibraryInfoProvider.CreateMediaLibraryFolder(SiteContext.CurrentSiteName, library.LibraryID, "NewFolder");
                }
            }


            /// <heading>Creating a media library file</heading>
            private void CreateMediaFile()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (library != null)
                {
                    // Prepares a path to a local file
                    string filePath = @"C:\Files\images\Image.png";

                    // Prepares a CMS.IO.FileInfo object representing the local file
                    CMS.IO.FileInfo file = CMS.IO.FileInfo.New(filePath);
                    
                    if (file != null)
                    {
                        // Creates a new media library file object
                        MediaFileInfo mediaFile = new MediaFileInfo(filePath, library.LibraryID);  

                        // Sets the media library file properties
                        mediaFile.FileName = "Image";
                        mediaFile.FileTitle = "File title";
                        mediaFile.FileDescription = "This file was added through the API.";
                        mediaFile.FilePath = "NewFolder/Image/"; // Sets the path within the media library's folder structure
                        mediaFile.FileExtension = file.Extension;
                        mediaFile.FileMimeType = MimeTypeHelper.GetMimetype(file.Extension);
                        mediaFile.FileSiteID = SiteContext.CurrentSiteID;
                        mediaFile.FileLibraryID = library.LibraryID;
                        mediaFile.FileSize = file.Length;

                        // Saves the media library file
                        MediaFileInfoProvider.SetMediaFileInfo(mediaFile);
                    }
                }
            }


            /// <heading>Updating a media library file</heading>
            private void GetAndUpdateMediaFile()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (library != null)
                {
                    // Gets the media file
                    MediaFileInfo updateFile = MediaFileInfoProvider.GetMediaFileInfo(library.LibraryID, "NewFolder/Image.png");

                    if (updateFile != null)
                    {
                        // Updates the media library file properties
                        updateFile.FileDescription = updateFile.FileDescription.ToLower();

                        // Saves the media library file
                        MediaFileInfoProvider.SetMediaFileInfo(updateFile);
                    }
                }
            }


            /// <heading>Updating multiple media library files</heading>
            private void GetAndBulkUpdateMediaFiles()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (library != null)
                {
                    // Gets all .png files from the "NewFolder" folder of the specified media library
                    var mediaFiles = MediaFileInfoProvider.GetMediaFiles()
                                                                .WhereEquals("FileLibraryID", library.LibraryID)
                                                                .WhereEquals("FileExtension", ".png")
                                                                .WhereStartsWith("FilePath", "NewFolder");
                    
                    // Loops through individual media library files
                    foreach (MediaFileInfo mediaFile in mediaFiles)
                    {
                        // Updates the media file properties
                        mediaFile.FileDescription = mediaFile.FileDescription.ToUpper();

                        // Saves the media library file
                        MediaFileInfoProvider.SetMediaFileInfo(mediaFile);
                    }
                }
            }


            /// <heading>Deleting a media library file</heading>
            private void DeleteMediaFile()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (library != null)
                {
                    // Gets the media file
                    MediaFileInfo deleteFile = MediaFileInfoProvider.GetMediaFileInfo(library.LibraryID, "NewFolder/Image.png");

                    if (deleteFile != null)
                    {
                        // Deletes the media file
                        MediaFileInfoProvider.DeleteMediaFileInfo(deleteFile);
                    }
                }
            }


            /// <heading>Deleting a media library folder</heading>
            private void DeleteMediaFolder()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (library != null)
                {
                    // Deletes the "NewFolder" folder within the media library
                    MediaLibraryInfoProvider.DeleteMediaLibraryFolder(SiteContext.CurrentSiteName, library.LibraryID, "NewFolder", false);
                }
            }
        }


        /// <summary>
        /// Holds media library security API examples.
        /// </summary>
        /// <groupHeading>Media library security</groupHeading>
        private class MediaLibrarySecurity
        {
            /// <heading>Setting the security options for a media library</heading>
            private void SetMediaLibrarySecurity()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                if (library != null)
                {
                    // Allows the content of the media library to be viewable for all users                                       
                    library.Access = SecurityAccessEnum.AllUsers;

                    // Allows management of the media library's files and folders only for assigned roles
                    library.FileCreate = SecurityAccessEnum.AuthorizedRoles;
                    library.FolderCreate = SecurityAccessEnum.AuthorizedRoles;
                    library.FolderModify = SecurityAccessEnum.AuthorizedRoles;
                    library.FileDelete = SecurityAccessEnum.AuthorizedRoles;
                    library.FolderDelete = SecurityAccessEnum.AuthorizedRoles;
                    library.FileModify = SecurityAccessEnum.AuthorizedRoles;

                    // Saves the updated media library to the database
                    MediaLibraryInfoProvider.SetMediaLibraryInfo(library);
                }
            }


            /// <heading>Allowing a media library action for a role</heading>
            private void AddRolePermissionToLibrary()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                // Gets the role
                RoleInfo libraryRole = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteID);

                // Gets the "Create file" media library permission
                PermissionNameInfo libraryPermission = PermissionNameInfoProvider.GetPermissionNameInfo("FileCreate", "CMS.MediaLibrary", null);

                // Checks that all of the objects exist
                if ((library != null) && (libraryRole != null) && (libraryPermission != null))
                {
                    // Creates an object representing a relationship between the media library permission and the role
                    MediaLibraryRolePermissionInfo rolePermission = new MediaLibraryRolePermissionInfo();
                    rolePermission.LibraryID = library.LibraryID;
                    rolePermission.RoleID = libraryRole.RoleID;
                    rolePermission.PermissionID = libraryPermission.PermissionId;

                    // Assigns the role to the media library permission
                    // In this case, members the role are authorized to create files in the media library
                    MediaLibraryRolePermissionInfoProvider.SetMediaLibraryRolePermissionInfo(rolePermission);
                }
            }


            /// <heading>Removing a role's authorization for a media library action</heading>
            private void RemoveRolePermissionFromLibrary()
            {
                // Gets the media library
                MediaLibraryInfo library = MediaLibraryInfoProvider.GetMediaLibraryInfo("NewLibrary", SiteContext.CurrentSiteName);

                // Gets the role
                RoleInfo libraryRole = RoleInfoProvider.GetRoleInfo("Admin", SiteContext.CurrentSiteID);

                // Gets the "Create file" media library permission
                PermissionNameInfo libraryPermission = PermissionNameInfoProvider.GetPermissionNameInfo("FileCreate", "CMS.MediaLibrary", null);

                // Checks that all of the objects exist
                if ((library != null) && (libraryRole != null) && (libraryPermission != null))
                {
                    // Gets the object representing the relationship between the media library permission and the role
                    MediaLibraryRolePermissionInfo rolePermission = 
                        MediaLibraryRolePermissionInfoProvider.GetMediaLibraryRolePermissionInfo(library.LibraryID, libraryRole.RoleID, libraryPermission.PermissionId);

                    if (rolePermission != null)
                    {
                        // Removes the role from the media library permission
                        MediaLibraryRolePermissionInfoProvider.DeleteMediaLibraryRolePermissionInfo(rolePermission);
                    }
                }
            }
        }
    }
}

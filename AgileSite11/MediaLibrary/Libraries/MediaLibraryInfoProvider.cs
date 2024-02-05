using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.Synchronization;
using System.Data;

namespace CMS.MediaLibrary
{
    /// <summary>
    /// Class providing media library info management.
    /// </summary>
    public class MediaLibraryInfoProvider : AbstractInfoProvider<MediaLibraryInfo, MediaLibraryInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Current media library.
        /// </summary>
        public const string CURRENT_LIBRARY = "##CURRENT_LIBRARY##";

        #endregion


        #region "Private fields"

        /// <summary>
        /// Indicates if media files physical files should be deleted.
        /// </summary>
        private static bool mDeletePhysicalFiles = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates if media files physical files should be deleted.
        /// </summary>
        public static bool DeletePhysicalFiles
        {
            get
            {
                return mDeletePhysicalFiles && CMSActionContext.CurrentDeletePhysicalFiles;
            }
            set
            {
                mDeletePhysicalFiles = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public MediaLibraryInfoProvider()
            : base(MediaLibraryInfo.TYPEINFO, new HashtableSettings
            {
                ID = true,
                Name = true
            })
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns media library info specified by library name and site name.
        /// Do not use for retrieving group MLs (use GetMediaLibraryInfo(string libraryName, int siteId, int groupId) instead). 
        /// </summary>
        /// <param name="libraryName">Library name</param>
        /// <param name="siteName">Site name</param>
        public static MediaLibraryInfo GetMediaLibraryInfo(string libraryName, string siteName)
        {
            int siteId = SiteInfoProvider.GetSiteID(siteName);
            int groupId = ModuleCommands.CommunityGetCurrentGroupID();

            return GetMediaLibraryInfo(libraryName, siteId, groupId);
        }


        /// <summary>
        /// Returns media library info specified by library name, site name and groupId.
        /// </summary>
        /// <param name="libraryName">Library name</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="groupId">Group ID</param>
        public static MediaLibraryInfo GetMediaLibraryInfo(string libraryName, int siteId, int groupId)
        {
            return ProviderObject.GetInfoByCodeName(libraryName, siteId, groupId);
        }


        /// <summary>
        /// Returns the MediaLibraryInfo structure for the specified media library.
        /// </summary>
        /// <param name="mediaLibraryId">MediaLibrary id</param>
        public static MediaLibraryInfo GetMediaLibraryInfo(int mediaLibraryId)
        {
            return ProviderObject.GetInfoById(mediaLibraryId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified media library.
        /// </summary>
        /// <param name="mediaLibrary">MediaLibrary to set</param>
        public static void SetMediaLibraryInfo(MediaLibraryInfo mediaLibrary)
        {
            ProviderObject.SetInfo(mediaLibrary);
        }


        /// <summary>
        /// Deletes specified media library.
        /// </summary>
        /// <param name="mediaLibraryId">Media library id</param>
        public static void DeleteMediaLibraryInfo(int mediaLibraryId)
        {
            MediaLibraryInfo infoObj = GetMediaLibraryInfo(mediaLibraryId);
            DeleteMediaLibraryInfo(infoObj);
        }

        /// <summary>
        /// Deletes specified media library.
        /// </summary>
        /// <param name="infoObj">MediaLibrary object</param>
        public static void DeleteMediaLibraryInfo(MediaLibraryInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Returns the query for all media libraries.
        /// </summary>        
        public static ObjectQuery<MediaLibraryInfo> GetMediaLibraries()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns information on libraries matching specified criteria.
        /// </summary>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement to use</param>
        /// <param name="topN">Top N records</param>
        /// <param name="columns">List of columns to be returned</param>
        public static ObjectQuery<MediaLibraryInfo> GetMediaLibraries(string where, string orderBy = null, int topN = 0, string columns = null)
        {
            return GetMediaLibraries().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Delete media libraries only from database for selected group.
        /// </summary>
        /// <param name="groupId">Group ID</param>
        public static ArrayList DeleteMediaLibrariesInfos(int groupId)
        {
            return ProviderObject.DeleteMediaLibraryInfosInternal(groupId);
        }

        #endregion


        #region "Public methods - Folder"

        /// <summary>
        /// Delete folder from media library.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Media library ID</param>
        /// <param name="folderPath">Path to the folder within the library</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        public static void DeleteMediaLibraryFolder(string siteName, int libraryID, string folderPath, bool synchronization, bool logSynchronization = true)
        {
            ProviderObject.DeleteMediaLibraryFolderInternal(siteName, libraryID, folderPath, synchronization, logSynchronization);
        }

        /// <summary>
        /// Delete media library root folder from media root directory.
        /// ~/[site name]/media/
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="folder">Media library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        public static void DeleteMediaLibraryFolder(string siteName, string folder, bool synchronization, bool logSynchronization = true)
        {
            ProviderObject.DeleteMediaLibraryFolderInternal(siteName, folder, synchronization, logSynchronization);
        }


        /// <summary>
        /// Delete all media libraries folders.
        /// </summary>
        /// <param name="sitename">Site name</param>
        /// <param name="folders">List of folder to delete</param>
        public static void DeleteMediaLibrariesFolders(string sitename, ArrayList folders)
        {
            ProviderObject.DeleteMediaLibrariesFoldersInternal(sitename, folders);
        }


        /// <summary>
        /// Creates folder within specified library.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="newFolderPath">New folder path</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        public static void CreateMediaLibraryFolder(string siteName, int libraryID, string newFolderPath, bool synchronization = false, bool logSynchronization = true)
        {
            ProviderObject.CreateMediaLibraryFolderInternal(siteName, libraryID, newFolderPath, synchronization, logSynchronization);
        }


        /// <summary>
        /// Rename folder within specified library.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="folderPath">Original folder path</param>
        /// <param name="newFolderPath">New folder path</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        public static void RenameMediaLibraryFolder(string siteName, int libraryID, string folderPath, string newFolderPath, bool synchronization = false, bool logSynchronization = true)
        {
            ProviderObject.RenameMediaLibraryFolderInternal(siteName, libraryID, folderPath, newFolderPath, synchronization, logSynchronization);
        }


        /// <summary>
        /// Copy media library folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="origPath">Old folder path within the library folder</param>
        /// <param name="newPath">New folder path within the library folder</param>
        /// <param name="userId">ID of the user performing the action</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        public static void CopyMediaLibraryFolder(string siteName, int libraryID, string origPath, string newPath, int userId, bool synchronization = false, bool logSynchronization = true)
        {
            ProviderObject.CopyMediaLibraryFolderInternal(siteName, libraryID, origPath, newPath, synchronization, logSynchronization, userId, null);
        }


        /// <summary>
        /// Copy media library folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="origPath">Old folder path within the library folder</param>
        /// <param name="newPath">New folder path within the library folder</param>
        /// <param name="userId">ID of the user performing the action</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        /// <param name="fileGUIDs">List of original file GUIDs and their copied ones when staging is used</param>
        internal static void CopyMediaLibraryFolder(string siteName, int libraryID, string origPath, string newPath, int userId, bool synchronization, bool logSynchronization, Dictionary<Guid, Guid> fileGUIDs)
        {
            ProviderObject.CopyMediaLibraryFolderInternal(siteName, libraryID, origPath, newPath, synchronization, logSynchronization, userId, fileGUIDs);
        }


        /// <summary>
        /// Moves media library folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="origPath">Old folder path within the library folder</param>
        /// <param name="newPath">New folder path within the library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        public static void MoveMediaLibraryFolder(string siteName, int libraryID, string origPath, string newPath, bool synchronization = false, bool logSynchronization = true)
        {
            ProviderObject.MoveMediaLibraryFolderInternal(siteName, libraryID, origPath, newPath, synchronization, logSynchronization);
        }

        #endregion


        #region "Public methods - Security"

        /// <summary>
        /// Add security where condition to the existing where condition.
        /// </summary>
        /// <param name="where">Existing where condition</param>
        /// <param name="communityGroupId">Community group ID</param>
        /// <returns>Returns where condition</returns>
        public static string CombineSecurityWhereCondition(string where, int communityGroupId)
        {
            return ProviderObject.CombineSecurityWhereConditionInternal(where, communityGroupId);
        }


        /// <summary>
        /// Returns True if current user is granted with specified media library permission, otherwise returns False.
        /// </summary>
        /// <param name="libraryInfo">Media library data</param>
        /// <param name="permission">Permission code name</param>
        /// <param name="userInfo">User to check</param>
        public static bool IsUserAuthorizedPerLibrary(MediaLibraryInfo libraryInfo, string permission, CurrentUserInfo userInfo = null)
        {
            return ProviderObject.IsUserAuthorizedPerLibraryInternal(libraryInfo, permission, userInfo);
        }

        #endregion


        #region "Public methods - Retrieving path"

        /// <summary>
        /// Returns physical path to the specified library.
        /// </summary>
        /// <param name="libraryId">Library ID</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaLibraryFolderPath(int libraryId, string webFullPath = null)
        {
            return GetMediaLibraryFolderPath(libraryId, null, webFullPath);
        }


        /// <summary>
        /// Returns physical path to the specified library.
        /// </summary>
        /// <param name="libraryId">Library ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaLibraryFolderPath(int libraryId, string siteName, string webFullPath)
        {
            MediaLibraryInfo libInfo = GetMediaLibraryInfo(libraryId);

            return GetMediaLibraryFolderPath(libInfo, siteName, webFullPath);
        }


        /// <summary>
        /// Gets the folder path for the given media library
        /// </summary>
        /// <param name="libInfo">Media library info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaLibraryFolderPath(MediaLibraryInfo libInfo, string siteName = null, string webFullPath = null)
        {
            if (libInfo != null)
            {
                // Site is not given externally, get it from media library
                if (String.IsNullOrEmpty(siteName))
                {
                    siteName = SiteInfoProvider.GetSiteName(libInfo.LibrarySiteID);
                }

                return GetMediaLibraryFolderPath(siteName, libInfo.LibraryFolder, webFullPath);
            }

            return null;
        }


        /// <summary>
        /// Returns physical path to the specified library folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        public static string GetMediaLibraryFolderPath(string siteName, string libraryFolder, string webFullPath = null)
        {
            return ProviderObject.GetMediaLibraryFolderPathInternal(siteName, libraryFolder, webFullPath);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(MediaLibraryInfo info)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info), "No MediaLibraryInfo object set.");
            }

            // Check unique media library root path in the site scope
            MediaLibraryInfo existingFolder = GetMediaLibraries()
                                                    .Column("LibraryID")
                                                    .WhereEquals("LibraryFolder", info.LibraryFolder)
                                                    .WhereEquals("LibrarySiteID", info.LibrarySiteID)
                                                    .FirstOrDefault();

            if ((existingFolder != null) && (info.LibraryID != existingFolder.LibraryID))
            {
                throw new InvalidOperationException(String.Format("Media library with same root folder '{0}' already exists for current site.", info.LibraryFolder));
            }

            bool newLibrary = (info.LibraryID == 0);

            if (info.LibraryTeaserGuid != Guid.Empty)
            {
                if (String.IsNullOrEmpty(info.LibraryTeaserPath) || info.ItemChanged(TypeInfo.CodeNameColumn))
                {
                    // Update teaser path
                    info.LibraryTeaserPath = MetaFileInfoProvider.GetMetaFileUrl(info.LibraryTeaserGuid, ValidationHelper.GetSafeFileName(info.LibraryName));
                }
            }
            else
            {
                if (!String.IsNullOrEmpty(info.LibraryTeaserPath))
                {
                    // Clear if path is set and GUID is removed
                    info.LibraryTeaserPath = null;
                }
            }

            // Update media library data
            base.SetInfo(info);

            // If library record in database created successfully
            if (newLibrary)
            {
                try
                {
                    // Create library folder
                    SiteInfo si = SiteInfoProvider.GetSiteInfo(info.LibrarySiteID);
                    CreateMediaLibraryFolder(si.SiteName, info.LibraryID, null, false, false);
                }
                catch (Exception ex)
                {
                    // Log exception while creating the root folder
                    EventLogProvider.LogException("Media library", "CREATEOBJ", ex);
                    throw;
                }
            }
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Delete media libraries only from database for selected group.
        /// </summary>
        /// <param name="groupId">Group ID</param>
        private ArrayList DeleteMediaLibraryInfosInternal(int groupId)
        {
            ArrayList result = new ArrayList();

            // Get all media libraries which belong to the group
            var libraries = GetMediaLibraries("LibraryGroupID = " + groupId);
            if (!DataHelper.DataSourceIsEmpty(libraries))
            {
                foreach (MediaLibraryInfo library in libraries)
                {
                    library.Generalized.DeleteFiles = false;

                    // Delete one by one
                    DeleteMediaLibraryInfo(library);

                    // Add library folder into result array
                    result.Add(library.LibraryFolder);
                }
            }

            return result;
        }

        #endregion


        #region "Internal methods - Folder"

        /// <summary>
        /// Delete folder from media library.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Media library ID</param>
        /// <param name="folderPath">Path to the folder within the library</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        protected virtual void DeleteMediaLibraryFolderInternal(string siteName, int libraryID, string folderPath, bool synchronization, bool logSynchronization)
        {
            folderPath = Path.EnsureSlashes(folderPath);

            MediaLibraryInfo libInfo = GetMediaLibraryInfo(libraryID);
            if (libInfo != null)
            {
                // Delete files records from DB
                MediaFileInfoProvider.DeleteMediaFiles(folderPath, libraryID);

                // Delete directory and all sub-directories if exists yet
                string libFolder = GetMediaLibraryFolderPath(siteName, DirectoryHelper.CombinePath(libInfo.LibraryFolder, MediaLibraryHelper.EnsurePhysicalPath(folderPath)));
                if (Directory.Exists(libFolder))
                {
                    try
                    {
                        Directory.Delete(libFolder, true);
                    }
                    catch (Exception ex)
                    {
                        // Log exception while deleting folder
                        EventLogProvider.LogException("Media library", "DELETEOBJ", ex);
                        throw;
                    }
                }

                if (!synchronization)
                {
                    WebFarmHelper.CreateIOTask(MediaTaskType.DeleteMediaFolder, libFolder, null, "deletemediafolder", siteName, libraryID.ToString(), folderPath);

                    if (logSynchronization)
                    {
                        // Log synchronization task
                        MediaLibraryHelper.LogSynchronization(siteName, libraryID, folderPath, null, TaskTypeEnum.DeleteMediaFolder, true);
                    }
                }
            }
        }

        /// <summary>
        /// Delete media library root folder from media root directory.
        /// ~/[site name]/media/
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="folder">Media library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        protected virtual void DeleteMediaLibraryFolderInternal(string siteName, string folder, bool synchronization, bool logSynchronization)
        {
            if (!String.IsNullOrEmpty(folder))
            {
                // Physical path of directory to delete
                string path = DirectoryHelper.CombinePath(MediaLibraryHelper.GetMediaRootFolderPath(siteName), MediaLibraryHelper.EnsurePhysicalPath(folder));

                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                }

                if (!synchronization)
                {
                    folder = Path.EnsureSlashes(folder);
                    WebFarmHelper.CreateIOTask(MediaTaskType.DeleteMediaFolder, path, null, "deletemediafolder", siteName, folder);

                    if (logSynchronization)
                    {
                        // Log synchronization task
                        MediaLibraryHelper.LogSynchronization(siteName, -1, folder, null, TaskTypeEnum.DeleteMediaRootFolder, SynchronizationInfoProvider.ENABLED_SERVERS, true);
                    }
                }
            }
        }


        /// <summary>
        /// Delete all media libraries folders.
        /// </summary>
        /// <param name="sitename">Site name</param>
        /// <param name="folders">List of folder to delete</param>
        protected virtual void DeleteMediaLibrariesFoldersInternal(string sitename, ArrayList folders)
        {
            if ((folders != null) && (folders.Count > 0))
            {
                foreach (string folder in folders)
                {
                    DeleteMediaLibraryFolder(sitename, folder, false);
                }
            }
        }


        /// <summary>
        /// Creates folder within specified library.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="newFolderPath">New folder path</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        protected virtual void CreateMediaLibraryFolderInternal(string siteName, int libraryID, string newFolderPath, bool synchronization, bool logSynchronization)
        {
            MediaLibraryInfo mli = GetMediaLibraryInfo(libraryID);
            if (mli != null)
            {
                newFolderPath = newFolderPath ?? string.Empty;
                string newPhysicalFolderPath = GetMediaLibraryFolderPath(siteName, DirectoryHelper.CombinePath(mli.LibraryFolder, MediaLibraryHelper.EnsurePhysicalPath(newFolderPath)));
                // Create new folder
                Directory.CreateDirectory(newPhysicalFolderPath);

                if (!synchronization)
                {
                    newFolderPath = Path.EnsureSlashes(newFolderPath);
                    WebFarmHelper.CreateIOTask(MediaTaskType.CreateMediaFolder, newPhysicalFolderPath, null, "updatemediafolder", siteName, libraryID.ToString(), newFolderPath);

                    if (logSynchronization)
                    {
                        // Log synchronization task
                        MediaLibraryHelper.LogSynchronization(siteName, libraryID, newFolderPath, null, TaskTypeEnum.CreateMediaFolder, true);
                    }
                }
            }
        }


        /// <summary>
        /// Rename folder within specified library.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="folderPath">Original folder path</param>
        /// <param name="newFolderPath">New folder path</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        protected virtual void RenameMediaLibraryFolderInternal(string siteName, int libraryID, string folderPath, string newFolderPath, bool synchronization, bool logSynchronization)
        {
            MediaLibraryInfo mli = GetMediaLibraryInfo(libraryID);
            if ((mli != null) && (!string.IsNullOrEmpty(newFolderPath)))
            {
                if (!string.IsNullOrEmpty(folderPath))
                {
                    // Get new and old physical paths
                    string rootFolderPath = GetMediaLibraryFolderPath(siteName, mli.LibraryFolder);
                    string newPhysicalFolderPath = DirectoryHelper.CombinePath(rootFolderPath, MediaLibraryHelper.EnsurePhysicalPath(newFolderPath));
                    string oldPhysicalFolderPath = DirectoryHelper.CombinePath(rootFolderPath, MediaLibraryHelper.EnsurePhysicalPath(folderPath));

                    // Get old and new database path for update
                    string libFolderPath = GetMediaLibraryFolderPath(libraryID) + "\\";
                    string oldDBpath = oldPhysicalFolderPath.Substring(libFolderPath.Length);
                    string newDBPath = newPhysicalFolderPath.Substring(libFolderPath.Length);

                    // Update files path in transaction according to the new folder path
                    MediaFileInfoProvider.UpdateFilesPath(oldDBpath, newDBPath, libraryID);

                    // Rename the directory
                    Directory.Move(oldPhysicalFolderPath, newPhysicalFolderPath);

                    if (!synchronization)
                    {
                        folderPath = Path.EnsureSlashes(folderPath);
                        newFolderPath = Path.EnsureSlashes(newFolderPath);
                        WebFarmHelper.CreateTask(MediaTaskType.RenameMediaFolder, "updatemediafolder", siteName, libraryID.ToString(), folderPath, newFolderPath);

                        if (logSynchronization)
                        {
                            // Log synchronization task
                            MediaLibraryHelper.LogSynchronization(siteName, libraryID, folderPath, newFolderPath, TaskTypeEnum.RenameMediaFolder, true);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Copy media library folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="origPath">Old folder path within the library folder</param>
        /// <param name="newPath">New folder path within the library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        /// <param name="userId">ID of the user performing the action</param>
        /// <param name="fileGUIDs">List of original file GUIDs and their copied ones when staging is used</param>
        protected virtual void CopyMediaLibraryFolderInternal(string siteName, int libraryID, string origPath, string newPath, bool synchronization, bool logSynchronization, int userId, Dictionary<Guid, Guid> fileGUIDs)
        {
            string folderPath = GetMediaLibraryFolderPath(libraryID);
            string origPhysicalPath = DirectoryHelper.CombinePath(folderPath, MediaLibraryHelper.EnsurePhysicalPath(origPath));
            string newPhysicalPath = DirectoryHelper.CombinePath(folderPath, MediaLibraryHelper.EnsurePhysicalPath(newPath));
            int libPathLength = folderPath.Length;

            DirectoryInfo dir = DirectoryInfo.New(origPhysicalPath);
            MediaLibraryHelper.CopyRecursiveInternal(libraryID, libraryID, dir, newPhysicalPath, origPath, libPathLength, newPhysicalPath, !synchronization, userId, fileGUIDs: fileGUIDs);

            if (!synchronization)
            {
                origPath = Path.EnsureSlashes(origPath);
                newPath = Path.EnsureSlashes(newPath);
                // Log web farm task
                WebFarmHelper.CreateTask(MediaTaskType.CopyMediaFolder, "copymediafolder", siteName, libraryID.ToString(), origPath, newPath);

                if (logSynchronization)
                {
                    // Log synchronization task
                    MediaLibraryHelper.LogSynchronization(siteName, libraryID, origPath, newPath, TaskTypeEnum.CopyMediaFolder, true);
                }
            }
        }


        /// <summary>
        /// Moves media library folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryID">Library ID</param>
        /// <param name="origPath">Old folder path within the library folder</param>
        /// <param name="newPath">New folder path within the library folder</param>
        /// <param name="synchronization">Indicates if method is running by web farms synchronization</param>
        /// <param name="logSynchronization">Indicates if staging task should be logged</param>
        protected virtual void MoveMediaLibraryFolderInternal(string siteName, int libraryID, string origPath, string newPath, bool synchronization, bool logSynchronization)
        {
            string folderPath = GetMediaLibraryFolderPath(libraryID);
            string origPhysicalPath = DirectoryHelper.CombinePath(folderPath, MediaLibraryHelper.EnsurePhysicalPath(origPath));
            string newPhysicalPath = DirectoryHelper.CombinePath(folderPath, MediaLibraryHelper.EnsurePhysicalPath(newPath));

            Directory.Move(origPhysicalPath, newPhysicalPath);

            // Update database paths
            MediaFileInfoProvider.UpdateFilesPath(origPath, newPath, libraryID);

            if (!synchronization)
            {
                origPath = Path.EnsureSlashes(origPath);
                newPath = Path.EnsureSlashes(newPath);
                // Create web farm task
                WebFarmHelper.CreateTask(MediaTaskType.MoveMediaFolder, "movemediafolder", siteName, libraryID.ToString(), origPath, newPath);

                if (logSynchronization)
                {
                    // Log synchronization task
                    MediaLibraryHelper.LogSynchronization(siteName, libraryID, origPath, newPath, TaskTypeEnum.MoveMediaFolder, true);
                }
            }
        }

        #endregion


        #region "Internal methods - Security"

        /// <summary>
        /// Add security where condition to the existing where condition.
        /// </summary>
        /// <param name="where">Existing where condition</param>
        /// <param name="communityGroupId">Community group ID</param>
        /// <returns>Returns where condition</returns>
        protected virtual string CombineSecurityWhereConditionInternal(string where, int communityGroupId)
        {
            // Public                         x < 1000000
            // Authenticated        999999  < x < 2000000
            // Access denied        3999999 < x < 5000000
            // Authenticated roles  1999999 < x < 3000000   
            // Group members        2999999 < x < 4000000

            // If current user doesn't exist, return nothing
            var currentUser = MembershipContext.AuthenticatedUser;
            if (currentUser == null)
            {
                return "(0=1)";
            }

            if (!AuthenticationHelper.IsAuthenticated())
            {
                if (!String.IsNullOrEmpty(where))
                {
                    where = String.Format("({0})  AND ", where);
                }

                where += "(LibraryAccess < 1000000)";
            }
            else
            {
                // Global admin can see everything
                if (currentUser.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
                {
                    return where;
                }

                // Group admin can see everything
                if (communityGroupId > 0)
                {
                    if (currentUser.IsGroupAdministrator(communityGroupId))
                    {
                        return where;
                    }
                }

                // Get user roles
                string roleIds = currentUser.GetRoleIdList(true, true, SiteContext.CurrentSiteName);
                if (!String.IsNullOrEmpty(roleIds))
                {
                    roleIds = String.Format(" AND RoleID IN ({0})", roleIds);
                }

                if (!String.IsNullOrEmpty(where))
                {
                    where = String.Format("({0}) AND ", where);
                }

                // Access denied, All users Authenticated
                where += " (LibraryAccess < 5000000) AND ((LibraryAccess < 1000000) OR (LibraryAccess > 999999 AND LibraryAccess < 2000000)";

                // Group members
                if (communityGroupId > 0)
                {
                    // Is group member, admin or community admin
                    if (currentUser.IsGroupAdministrator(communityGroupId) || currentUser.IsGroupMember(communityGroupId))
                    {
                        where += "OR ((LibraryAccess > 2999999 AND LibraryAccess < 4000000))";
                    }
                }

                // Authorized roles
                where += String.Format(" OR (LibraryAccess > 1999999 AND LibraryAccess < 3000000) AND ((SELECT Count(RoleID) FROM Media_LibraryRolePermission WHERE Media_LibraryRolePermission.LibraryID = LibraryID AND PermissionID = (SELECT TOP 1 PermissionID FROM CMS_Permission WHERE PermissionName = 'LibraryAccess') {0}) > 0))", roleIds);
            }
            return where;
        }


        /// <summary>
        /// Returns True if current user is granted with specified media library permission, otherwise returns False.
        /// </summary>
        /// <param name="libraryInfo">Media library data</param>
        /// <param name="permission">Permission code name</param>
        /// <param name="userInfo">User to check</param>
        protected virtual bool IsUserAuthorizedPerLibraryInternal(MediaLibraryInfo libraryInfo, string permission, CurrentUserInfo userInfo)
        {
            if (libraryInfo == null || permission == null)
            {
                return false;
            }

            if (userInfo == null)
            {
                userInfo = AuthenticationHelper.IsAuthenticated() ? MembershipContext.AuthenticatedUser : AuthenticationHelper.GlobalPublicUser;
            }

            // Reserve the log item
            DataRow dr = SecurityDebug.StartSecurityOperation("IsUserAuthorizedPerLibrary");

            // True if user is global administrator
            bool result = userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin);

            // True if is group library and user is group admin
            if ((libraryInfo.LibraryGroupID > 0) && userInfo.IsGroupAdministrator(libraryInfo.LibraryGroupID))
            {
                result = true;
            }
            else
            {
                string siteName = SiteInfoProvider.GetSiteName(libraryInfo.LibrarySiteID);

                // Check the particular permission
                switch (permission.ToLowerInvariant())
                {
                    case "read":
                        result = CheckResourcePermission(libraryInfo.LibraryGroupID, "read", userInfo, siteName);
                        break;

                    case "manage":
                        result = CheckResourcePermission(libraryInfo.LibraryGroupID, "manage", userInfo, siteName);
                        break;

                    case "filecreate":
                        result = CheckPermission(libraryInfo.LibraryID, libraryInfo.LibraryGroupID, permission, libraryInfo.FileCreate, userInfo) ||
                                 CheckResourcePermission(libraryInfo.LibraryGroupID, "manage", userInfo, siteName);
                        break;

                    case "filedelete":
                        result = CheckPermission(libraryInfo.LibraryID, libraryInfo.LibraryGroupID, permission, libraryInfo.FileDelete, userInfo) ||
                                 CheckResourcePermission(libraryInfo.LibraryGroupID, "manage", userInfo, siteName);
                        break;

                    case "filemodify":
                        result = CheckPermission(libraryInfo.LibraryID, libraryInfo.LibraryGroupID, permission, libraryInfo.FileModify, userInfo) ||
                                 CheckResourcePermission(libraryInfo.LibraryGroupID, "manage", userInfo, siteName);
                        break;

                    case "foldercreate":
                        result = CheckPermission(libraryInfo.LibraryID, libraryInfo.LibraryGroupID, permission, libraryInfo.FolderCreate, userInfo) ||
                                 CheckResourcePermission(libraryInfo.LibraryGroupID, "manage", userInfo, siteName);
                        break;

                    case "folderdelete":
                        result = CheckPermission(libraryInfo.LibraryID, libraryInfo.LibraryGroupID, permission, libraryInfo.FolderDelete, userInfo) ||
                                 CheckResourcePermission(libraryInfo.LibraryGroupID, "manage", userInfo, siteName);
                        break;

                    case "foldermodify":
                        result = CheckPermission(libraryInfo.LibraryID, libraryInfo.LibraryGroupID, permission, libraryInfo.FolderModify, userInfo) ||
                                 CheckResourcePermission(libraryInfo.LibraryGroupID, "manage", userInfo, siteName);
                        break;

                    case "libraryaccess":
                        result = CheckPermission(libraryInfo.LibraryID, libraryInfo.LibraryGroupID, permission, libraryInfo.Access, userInfo) ||
                                 CheckResourcePermission(libraryInfo.LibraryGroupID, "libraryaccess", userInfo, siteName);
                        break;
                }
            }

            // Log the operation
            if (dr != null)
            {
                SecurityDebug.FinishSecurityOperation(dr, userInfo.UserName, libraryInfo.LibraryID.ToString(), permission, result, null);
            }

            return result;
        }

        #endregion


        #region "Internal methods - Retrieving path"

        /// <summary>
        /// Returns physical path to the specified library folder.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="libraryFolder">Library folder name</param>
        /// <param name="webFullPath">Physical path to the root of the web project (e.g. c:\WebProject\), if it is not specified web application physical path is used</param>
        protected virtual string GetMediaLibraryFolderPathInternal(string siteName, string libraryFolder, string webFullPath = null)
        {
            if (String.IsNullOrEmpty(siteName))
            {
                return null;
            }

            return MediaLibraryHelper.GetMediaRootFolderPath(siteName, webFullPath) + libraryFolder;
        }


        #endregion


        #region "Private methods"

        /// <summary>
        /// Checks the specified permission for resource (cms.groups if library group id is specified, cms.medialibrary otherwise).
        /// </summary>
        /// <param name="libraryGroupId">ID of the library group</param>
        /// <param name="permissionName">Name of the permission</param>
        /// <param name="userInfo">User info to check</param>
        /// <param name="siteName">Site name</param>
        private bool CheckResourcePermission(int libraryGroupId, string permissionName, CurrentUserInfo userInfo, string siteName)
        {
            // Ensure site name
            if (String.IsNullOrEmpty(siteName))
            {
                siteName = SiteContext.CurrentSiteName;
            }

            if (string.Equals(permissionName, "read", StringComparison.InvariantCultureIgnoreCase) || string.Equals(permissionName, "libraryaccess", StringComparison.InvariantCultureIgnoreCase))
            {
                // Check read permissions
                if (libraryGroupId > 0)
                {
                    return userInfo.IsAuthorizedPerResource("cms.groups", "Read", siteName);
                }
                return userInfo.IsAuthorizedPerResource("cms.medialibrary", "Read", siteName);
            }

            // Check manage permissions
            if (libraryGroupId > 0)
            {
                return userInfo.IsAuthorizedPerResource("cms.groups", "Manage", siteName);
            }

            return userInfo.IsAuthorizedPerResource("cms.medialibrary", "Manage", siteName);
        }


        /// <summary>
        /// Checks specified media library permission.
        /// </summary>
        /// <param name="libraryId">Media library ID</param>
        /// <param name="libraryGroupId">Media library group ID</param>
        /// <param name="permissionName">Name of the permission which is checked</param>
        /// <param name="permissionValue">Value of the permission</param>
        /// <param name="userInfo">User info to check</param>
        private bool CheckPermission(int libraryId, int libraryGroupId, string permissionName, SecurityAccessEnum permissionValue, UserInfo userInfo)
        {
            switch (permissionValue)
            {
                case SecurityAccessEnum.Nobody:
                    return false;

                case SecurityAccessEnum.AllUsers:
                    return true;

                case SecurityAccessEnum.GroupMembers:
                    return ModuleCommands.CommunityIsMemberOfGroup(userInfo.UserID, libraryGroupId);

                case SecurityAccessEnum.AuthenticatedUsers:
                    return !userInfo.IsPublic();

                case SecurityAccessEnum.AuthorizedRoles:
                    return IsAuthorizedPerLibrary(libraryId, permissionName, userInfo.UserID);
            }

            return false;
        }


        /// <summary>
        /// Returns true if the user is authorized per given library.
        /// </summary>
        /// <param name="mediaLibraryId">Library ID</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="userId">User ID</param>
        private bool IsAuthorizedPerLibrary(int mediaLibraryId, string permissionName, int userId)
        {
            // Prepare parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", userId);
            parameters.Add("@LibraryID", mediaLibraryId);
            parameters.Add("@PermissionName", permissionName);
            parameters.Add("@ValidTo", DateTime.Now);

            // Get the data from DB
            using (DataSet permissions = ConnectionHelper.ExecuteQuery("media.library.isauthorized", parameters))
            {
                if (!DataHelper.DataSourceIsEmpty(permissions))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    using TypedDataSet = InfoDataSet<MetaFileInfo>;

    /// <summary>
    /// Class providing MetaFileInfo management.
    /// </summary>
    public class MetaFileInfoProvider : AbstractInfoProvider<MetaFileInfo, MetaFileInfoProvider>
    {
        #region "Variables"

        private static bool mDeletePhysicalFiles = true;

        /// <summary>
        /// Lock object for ensuring of the physical files.
        /// </summary>
        private static readonly object ensureFileLock = new object();

        #endregion


        #region "Public properties"

        /// <summary>
        /// Full path to the root of the web.
        /// </summary>
        public static string WebApplicationPhysicalPath
        {
            get
            {
                return SystemContext.WebApplicationPhysicalPath;
            }
        }


        /// <summary>
        /// Indicates if physical files should be deleted.
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


        #region "Public static methods"

        /// <summary>
        /// Returns all metafiles.
        /// </summary>
        public static ObjectQuery<MetaFileInfo> GetMetaFiles()
        {
            return ProviderObject.GetMetaFilesInternal();
        }


        /// <summary>
        /// Returns the MetaFileInfo structure for the specified metaFile.
        /// </summary>
        /// <param name="metaFileId">MetaFile id</param>
        public static MetaFileInfo GetMetaFileInfo(int metaFileId)
        {
            return ProviderObject.GetInfoById(metaFileId);
        }


        /// <summary>
        /// Returns the MetaFileInfo structure for the specified metaFile.
        /// </summary>
        /// <param name="metaFileGuid">MetaFile guid</param>
        /// <param name="siteName">Site name</param>
        /// <param name="globalOrLocal">If true, global (local) files are allowed when local (global) not found</param>
        public static MetaFileInfo GetMetaFileInfo(Guid metaFileGuid, string siteName, bool globalOrLocal)
        {
            return ProviderObject.GetMetaFileInfoInternal(metaFileGuid, siteName, globalOrLocal);
        }


        /// <summary>
        /// Returns the MetaFileInfo structure for the specified metaFile without the binary data.
        /// </summary>
        /// <param name="metaFileGuid">MetaFile guid</param>
        /// <param name="siteName">Site name</param>
        /// <param name="globalOrLocal">If true, global (local) files are allowed when local (global) not found</param>
        public static MetaFileInfo GetMetaFileInfoWithoutBinary(Guid metaFileGuid, string siteName, bool globalOrLocal)
        {
            return ProviderObject.GetMetaFileInfoInternal(metaFileGuid, siteName, globalOrLocal, false);
        }


        /// <summary>
        /// Sets (updates or inserts) specified metaFile.
        /// </summary>
        /// <param name="metaFile">MetaFile to set</param>
        public static void SetMetaFileInfo(MetaFileInfo metaFile)
        {
            ProviderObject.SetInfo(metaFile);
        }


        /// <summary>
        /// Deletes specified metaFile.
        /// </summary>
        /// <param name="metaFileObj">MetaFile object</param>
        public static void DeleteMetaFileInfo(MetaFileInfo metaFileObj)
        {
            ProviderObject.DeleteInfo(metaFileObj);
        }


        /// <summary>
        /// Deletes specified metaFile.
        /// </summary>
        /// <param name="metaFileId">MetaFile id</param>
        public static void DeleteMetaFileInfo(int metaFileId)
        {
            MetaFileInfo metaFileObj = GetMetaFileInfo(metaFileId);
            DeleteMetaFileInfo(metaFileObj);
        }


        /// <summary>
        /// Gets all the files with specified where/order by.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order By</param>
        /// <param name="columns">Data columns to return</param>
        /// <param name="topN">Specifies number of returned recordsd</param>
        public static TypedDataSet GetMetaFiles(string where, string orderBy, string columns = null, int topN = 0)
        {
            int totalRecords = -1;
            return GetMetaFiles(where, orderBy, columns, topN, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Gets all the files with specified where/order by.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order By</param>
        /// <param name="columns">Data columns to return</param>
        /// <param name="topN">Specifies number of returned recordsd</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of available records</param>
        public static TypedDataSet GetMetaFiles(string where, string orderBy, string columns, int topN, int offset, int maxRecords, ref int totalRecords)
        {
            return ProviderObject.GetMetaFilesInternal(columns, where, orderBy, topN, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Gets the file list of the files for certain object.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order By</param>
        public static TypedDataSet GetMetaFiles(int objectId, string objectType, string group, string where, string orderBy)
        {
            return GetMetaFiles(objectId, objectType, group, where, orderBy, null, 0);
        }


        /// <summary>
        /// Gets the file list of the files for certain object.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order By</param>
        /// <param name="columns">Data columns to return</param>
        /// <param name="topN">Specifies number of returned recordsd</param>
        public static TypedDataSet GetMetaFiles(int objectId, string objectType, string group, string where, string orderBy, string columns, int topN)
        {
            int totalRecords = 0;
            return GetMetaFiles(objectId, objectType, group, where, orderBy, columns, topN, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Gets the file list of the files for certain object.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order By</param>
        /// <param name="columns">Data columns to return</param>
        /// <param name="topN">Specifies number of returned recordsd</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of available records</param>
        public static TypedDataSet GetMetaFiles(int objectId, string objectType, string group, string where, string orderBy, string columns, int topN, int offset, int maxRecords, ref int totalRecords)
        {
            if (objectId <= 0)
            {
                return null;
            }

            where = GetWhereCondition(objectId, objectType, group, where);

            return GetMetaFiles(where, orderBy, columns, topN, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Gets the file list of the files for certain object with the binary file data. Loads the binaries from file system if the binary is missing.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order By</param>
        public static TypedDataSet GetMetaFilesWithBinary(int objectId, string objectType, string group, string where, string orderBy)
        {
            // Get all files
            TypedDataSet ds = GetMetaFiles(objectId, objectType, group, where, orderBy);

            // Ensure binary data
            if (ds != null)
            {
                EnsureMetaFileBinaries(ds.Tables[0]);
            }

            return ds;
        }


        /// <summary>
        /// Gets the file list of the files for certain object without the binary file data.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order By</param>
        public static TypedDataSet GetMetaFilesWithoutBinary(int objectId, string objectType, string group, string where, string orderBy)
        {
            return ProviderObject.GetMetaFilesWithoutBinaryInternal(objectId, objectType, group, where, orderBy);
        }


        /// <summary>
        /// Will call the previous method will all the remaining parameters null.
        /// </summary>
        /// <param name="objectId">Object ID</param>
        /// <param name="objectType">Object type</param>
        public static TypedDataSet GetMetaFiles(int objectId, string objectType)
        {
            return GetMetaFiles(objectId, objectType, null, null, null);
        }


        /// <summary>
        /// Deletes all files associated to the given object.
        /// </summary>
        /// <param name="infoObject">Info object</param>
        /// <param name="category">Meta files category</param>
        public static void DeleteFiles(GeneralizedInfo infoObject, string category)
        {
            ProviderObject.DeleteFilesInternal(infoObject, category);
        }


        /// <summary>
        /// Deletes all files associated with the given object.
        /// </summary>
        /// <param name="objectId">Object ID</param>
        /// <param name="objectType">Object type</param>
        /// <param name="category">Meta files category</param>
        public static bool DeleteFiles(int objectId, string objectType, string category = null)
        {
            return ProviderObject.DeleteFilesInternal(objectId, objectType, category);
        }


        /// <summary>
        /// Updates the object meta files from the given DataTable. Returns true if some metafiles were updated.
        /// </summary>
        /// <param name="infoObj">Info object for which the metafiles update</param>
        /// <param name="filesDt">Table of the new files (with binaries)</param>
        /// <param name="filesObjectId">Object ID in the new files table</param>
        /// <param name="th">If provided, <see cref="ColumnsTranslationEvents.TranslateColumns"/> events are raised with given <see cref="TranslationHelper"/>.</param>
        /// <param name="onLoadData">Load data event handler</param>
        /// <param name="logSynchronization">Indicates if staging tasks should be logged</param>
        /// <param name="parentIsNew">If true, the parent object is a new object, therefore is no need to update or delete existing (there are none)</param>
        /// <param name="onlyAddNew">If true, only new metafiles are added and existing are not updated</param>
        /// <returns>Returns the GUID of the thumbnail meta file if found</returns>
        public static bool UpdateMetaFiles(BaseInfo infoObj, DataTable filesDt, int filesObjectId, TranslationHelper th, LoadDataEventHandler onLoadData = null, bool logSynchronization = true, bool parentIsNew = false, bool onlyAddNew = false)
        {
            var genObj = infoObj.Generalized;

            int objectId = genObj.ObjectID;
            string objectType = infoObj.TypeInfo.ObjectType;
            int siteId = genObj.ObjectSiteID;

            string objectWhere = "MetaFileObjectID = " + filesObjectId + " AND MetaFileObjectType = '" + SqlHelper.GetSafeQueryString(objectType, false) + "'";

            var someMetafilesUpdated = false;

            // Get the object files
            DataRow[] files = null;
            if (filesDt != null)
            {
                files = filesDt.Select(objectWhere);
            }

            Guid oldThumbnailGuid = genObj.ObjectThumbnailGUID;
            Guid newThumbnailGuid = Guid.Empty;

            Guid oldIconGuid = genObj.ObjectIconGUID;
            Guid newIconGuid = Guid.Empty;

            if ((files != null) && (files.Length > 0))
            {
                // Table of mapping [GUID -> ID]
                var existingIds = new SafeDictionary<Guid, int>();

                if (!parentIsNew)
                {
                    #region "Remove unwanted existing"

                    // Remove additional meta files
                    DataSet existingFiles = GetMetaFilesWithoutBinary(objectId, objectType, null, null, null);
                    if (!DataHelper.DataSourceIsEmpty(existingFiles))
                    {
                        // Remove all that are not present in the source table
                        foreach (DataRow dr in existingFiles.Tables[0].Rows)
                        {
                            Guid fileGuid = ValidationHelper.GetGuid(dr["MetaFileGUID"], Guid.Empty);
                            int fileId = ValidationHelper.GetInteger(dr["MetaFileID"], 0);

                            existingIds[fileGuid] = fileId;

                            var newFile = filesDt.Select(objectWhere + " AND MetaFileGUID = '" + fileGuid + "'");
                            if (newFile.Length == 0)
                            {
                                // Delete the file
                                MetaFileInfo metaFileObj = new MetaFileInfo(dr);

                                // Disable synchronization
                                if (!logSynchronization)
                                {
                                    metaFileObj.Generalized.LogSynchronization = SynchronizationTypeEnum.None;
                                }

                                DeleteMetaFileInfo(metaFileObj);

                                someMetafilesUpdated = true;
                            }
                        }
                    }

                    #endregion
                }

                // Update the files
                foreach (DataRow dr in files)
                {
                    MetaFileInfo file = new MetaFileInfo(dr);

                    // Disable synchronization
                    if (!logSynchronization)
                    {
                        file.Generalized.LogSynchronization = SynchronizationTypeEnum.None;
                    }

                    // Load the data
                    onLoadData?.Invoke(file);

                    file.MetaFileObjectID = objectId;
                    file.MetaFileID = 0;
                    file.MetaFileSiteID = siteId;

                    // Get existing
                    int existingId = ValidationHelper.GetInteger(existingIds[file.MetaFileGUID], 0);
                    if (existingId > 0)
                    {
                        // When only new files are added, do not process item which already exists
                        if (onlyAddNew)
                        {
                            continue;
                        }

                        file.MetaFileID = existingId;
                    }

                    if (th != null && ColumnsTranslationEvents.TranslateColumns.IsBound)
                    {
                        ColumnsTranslationEvents.TranslateColumns.StartEvent(th, MetaFileInfo.OBJECT_TYPE, file);
                    }

                    SetMetaFileInfo(file);

                    someMetafilesUpdated = true;

                    // Update parent GUIDs
                    if (file.IsThumbnail(infoObj.TypeInfo))
                    {
                        newThumbnailGuid = file.MetaFileGUID;
                    }
                    if (file.IsIcon(infoObj.TypeInfo))
                    {
                        newIconGuid = file.MetaFileGUID;
                    }
                }
            }
            else if (!parentIsNew)
            {
                // Delete all meta files
                someMetafilesUpdated |= DeleteFiles(objectId, objectType);
            }

            bool updated = false;

            // Update parent GUIDs
            if (newThumbnailGuid != oldThumbnailGuid)
            {
                genObj.ObjectThumbnailGUID = newThumbnailGuid;
                updated = true;
            }
            if (newIconGuid != oldIconGuid)
            {
                genObj.ObjectIconGUID = newIconGuid;
                updated = true;
            }

            // Update the object if file GUIDs were updated
            if (updated)
            {
                infoObj.Update();
            }

            return someMetafilesUpdated;
        }


        /// <summary>
        /// Duplicates metafiles for specified object. Returns old guid/new guid list.
        /// </summary>
        /// <param name="sourceObjectId">Source object</param>
        /// <param name="targetObjectId">Target object</param>
        /// <param name="objectType">Type</param>
        /// <param name="category">Category</param>
        /// <param name="convList">List containing old and new guids</param>
        public static void CopyMetaFiles(int sourceObjectId, int targetObjectId, string objectType, string category, List<Guid> convList)
        {
            CopyMetaFiles(sourceObjectId, targetObjectId, objectType, category, objectType, category, convList);
        }


        /// <summary>
        /// Duplicates metafiles for specified object. Returns old guid/new guid list.
        /// </summary>
        /// <param name="sourceObjectId">Source object</param>
        /// <param name="targetObjectId">Target object</param>
        /// <param name="sourceObjectType">Source object type</param>
        /// <param name="sourceCategory">Source object category</param>
        /// <param name="targetObjectType">Target object type</param>
        /// <param name="targetCategory">Target object category</param>
        /// <param name="convList">List containing old and new guids</param>
        public static void CopyMetaFiles(int sourceObjectId, int targetObjectId, string sourceObjectType, string sourceCategory, string targetObjectType, string targetCategory, List<Guid> convList)
        {
            // Get IDs of all files attached to source object
            DataSet ds = GetMetaFiles(sourceObjectId, sourceObjectType, targetCategory, null, null, "MetaFileID", -1);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                try
                {
                    // Duplicate all files - one by one
                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        // Get object with binary
                        MetaFileInfo tmp = GetMetaFileInfo(ValidationHelper.GetInteger(dr["MetaFileID"], 0));
                        tmp.MetaFileID = 0;

                        // Get metafile's site name if any
                        string siteName = tmp.Generalized.ObjectSiteName;

                        Guid oldGuid = tmp.MetaFileGUID;
                        Guid newGuid = Guid.NewGuid();
                        string newGuidString = newGuid.ToString();
                        var filesLocationType = FileHelper.FilesLocationType(siteName);
                        var useFileSystemOnly = filesLocationType == FilesLocationTypeEnum.FileSystem;

                        // Saving metafiles in file system
                        if (filesLocationType != FilesLocationTypeEnum.Database)
                        {
                            if (tmp.MetaFileBinary != null)
                            {
                                SaveFileToDisk(siteName, newGuidString, newGuidString, tmp.MetaFileExtension, tmp.MetaFileBinary, true);
                            }
                            else
                            {
                                string path = GetFilePhysicalPath(siteName, oldGuid.ToString(), tmp.MetaFileExtension);
                                if (File.Exists(path))
                                {
                                    FileStream fs = File.OpenRead(path);
                                    SaveFileToDisk(siteName, newGuidString, newGuidString, tmp.MetaFileExtension, fs, true);
                                }
                            }

                            // Delete data from database, if requested
                            if (useFileSystemOnly)
                            {
                                tmp.MetaFileBinary = null;
                            }
                        }

                        // Saving metafiles in database
                        if (!useFileSystemOnly)
                        {
                            if (tmp.MetaFileBinary == null)
                            {
                                string path = GetFilePhysicalPath(siteName, oldGuid.ToString(), tmp.MetaFileExtension);
                                if (File.Exists(path))
                                {
                                    tmp.MetaFileBinary = File.ReadAllBytes(path);
                                }
                            }
                        }

                        // Store old an new guid for later use if requested
                        if (convList != null)
                        {
                            convList.Add(oldGuid);
                            convList.Add(newGuid);
                        }

                        tmp.MetaFileGUID = newGuid;
                        tmp.MetaFileObjectID = targetObjectId;
                        tmp.MetaFileGroupName = targetCategory;
                        tmp.MetaFileObjectType = targetObjectType;

                        SetMetaFileInfo(tmp);
                    }
                }
                catch (Exception)
                {
                    // Delete all garbage when something fails
                    DeleteFiles(targetObjectId, sourceObjectType, sourceCategory);
                    DeleteFiles(targetObjectId, targetObjectType, targetCategory);
                    throw;
                }
            }
        }


        /// <summary>
        /// Moves metafiles for one object to another.
        /// </summary>
        /// <param name="sourceObjectId">Source object ID</param>
        /// <param name="targetObjectId">Target object ID</param>
        /// <param name="sourceObjectType">Source object type</param>
        /// <param name="sourceCategory">Source category</param>
        /// <param name="targetObjectType">Target object type</param>
        /// <param name="targetCategory">Target category</param>
        public static void MoveMetaFiles(int sourceObjectId, int targetObjectId, string sourceObjectType, string sourceCategory, string targetObjectType, string targetCategory)
        {
            // Get all files attached to source object
            DataSet ds = GetMetaFiles(sourceObjectId, sourceObjectType, sourceCategory, null, null, "MetaFileID", -1);

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Get object with binary
                    MetaFileInfo tmp = GetMetaFileInfo(ValidationHelper.GetInteger(dr["MetaFileID"], 0));

                    // Change object ID
                    tmp.MetaFileObjectID = targetObjectId;
                    tmp.MetaFileObjectType = targetObjectType;
                    tmp.MetaFileGroupName = targetCategory;
                    SetMetaFileInfo(tmp);
                }
            }
        }


        /// <summary>
        /// Returns the current settings whether the thumbnails should be generated.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool GenerateThumbnails(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSGenerateThumbnails");
        }


        /// <summary>
        /// Gets the meta file where condition.
        /// </summary>
        /// <param name="metaFileGuid">File GUID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="globalOrLocal">If true, global (local) files are allowed when local (global) not found</param>
        public static string GetGUIDWhereCondition(Guid metaFileGuid, string siteName, bool globalOrLocal)
        {
            // Prepare the where condition
            string where = "MetaFileGUID = '" + metaFileGuid + "'";

            // Get the site
            if ((siteName != null) && (siteName.Trim() != ""))
            {
                // Local file
                int siteId = ProviderHelper.GetId(PredefinedObjectType.SITE, siteName);
                if (siteId <= 0)
                {
                    throw new Exception("[MetaFileInfoProvider.GetMetaFileInfo]: Site name '" + siteName + "' not found.");
                }

                where += " AND (MetaFileSiteID = " + siteId;

                // Global variant
                if (globalOrLocal)
                {
                    where += " OR MetaFileSiteID IS NULL";
                }

                where += ")";
            }
            else
            {
                // Global file
                if (globalOrLocal)
                {
                    where += " AND MetaFileSiteID IS NULL";
                }
            }

            return where;
        }


        /// <summary>
        /// Save file to the disk.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="guid">File GUID</param>
        /// <param name="fileName">File name</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="fileData">File data (byte[] or Stream)</param>
        /// <param name="deleteOldFiles">Indicates whether files in destination folder with mask '[guid]*.*' should be deleted</param>
        protected static string SaveFileToDisk(string siteName, string guid, string fileName, string fileExtension, BinaryData fileData, bool deleteOldFiles)
        {
            return SaveFileToDisk(siteName, guid, fileName, fileExtension, fileData, deleteOldFiles, false);
        }


        /// <summary>
        /// Saves file to the disk.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="guid">File GUID</param>
        /// <param name="fileName">File name</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="fileData">File data (byte[] or Stream)</param>
        /// <param name="deleteOldFiles">Indicates whether files in destination folder with mask '[guid]*.*' should be deleted</param>
        /// <param name="synchronization">Indicates if this function is called from "ProcessTask"</param>
        public static string SaveFileToDisk(string siteName, string guid, string fileName, string fileExtension, BinaryData fileData, bool deleteOldFiles, bool synchronization)
        {
            if (!string.IsNullOrEmpty(fileName))
            {
                string filesFolderPath = GetFilesFolderPath(siteName);

                // Ensure disk path
                DirectoryHelper.EnsureDiskPath(filesFolderPath, WebApplicationPhysicalPath);

                // Check permission for specified folder
                if (!DirectoryHelper.CheckPermissions(filesFolderPath))
                {
                    throw new PermissionException("[MetaFileInfoProvider.SaveFileToDisk]: Access to the path '" + filesFolderPath + "' is denied.");
                }

                // Get file path
                string filePath = filesFolderPath + DirectoryHelper.CombinePath(fileName.Substring(0, 2), GetFullFileName(fileName, fileExtension));

                // Ensurte disk path
                DirectoryHelper.EnsureDiskPath(filePath, WebApplicationPhysicalPath);

                // Delete all file occurrences in destination folder
                if (deleteOldFiles)
                {
                    //delete all file occurrences in destination folder
                    DeleteFile(siteName, fileName, false, synchronization);
                }

                // Save specified file
                if (fileData != null)
                {
                    StorageHelper.SaveFileToDisk(filePath, fileData, false);

                    // If the action is not caused by synchronization, create the web farm task
                    if (!synchronization)
                    {
                        WebFarmHelper.CreateIOTask(new UpdateMetaFileWebFarmTask
                        {
                            SiteName = siteName,
                            FileGuid = guid,
                            FileName = fileName,
                            FileExtension = fileExtension,
                            DeleteOldFiles = deleteOldFiles,
                            TaskFilePath = filePath,
                            TaskBinaryData = fileData
                        });
                    }

                    fileData.Close();

                    if (synchronization)
                    {
                        // Drop the cache dependencies
                        CacheHelper.TouchKey("metafile|" + ValidationHelper.GetString(guid, Guid.Empty.ToString()).ToLowerInvariant(), false, false);
                    }
                }

                return filePath;
            }
            else
            {
                throw new Exception("[MetaFileInfoProvider.SaveFileToDisk]: GUID of the file is not specified.");
            }
        }


        /// <summary>
        /// Returns full file name ([name.extension] if extension is specified) or ([name] only if extension is not specified).
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="fileExtension">File extension</param>
        public static string GetFullFileName(string fileName, string fileExtension)
        {
            return AttachmentHelper.GetFullFileName(fileName, fileExtension);
        }


        /// <summary>
        /// Delete all files with the same name ([name].*).
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="deleteDirectory">Determines whether delete specified directory or not</param>
        /// <param name="synchronization">Indicates wehther the method is called due to synchronization</param>
        public static void DeleteFile(string siteName, string fileName, bool deleteDirectory, bool synchronization)
        {
            // Delete physical files only if enabled
            if (DeletePhysicalFiles)
            {
                // Get file folde path
                string directoryPath = GetFileFolder(siteName, fileName);
                if (Directory.Exists(directoryPath))
                {
                    DirectoryInfo di = DirectoryInfo.New(directoryPath);

                    // Select all files with the same name ( '<guid>*.*' )
                    FileInfo[] filesInfos = di.GetFiles(fileName + "*.*");

                    if (filesInfos != null)
                    {
                        // Delete all selected files
                        foreach (FileInfo file in filesInfos)
                        {
                            File.Delete(file.FullName);
                            if (!synchronization)
                            {
                                WebFarmHelper.CreateIOTask(new DeleteMetaFileWebFarmTask
                                {
                                    SiteName = siteName,
                                    FileName = fileName,
                                    DeleteDirectory = deleteDirectory,
                                    TaskFilePath = file.FullName
                                });
                            }
                        }

                        if (deleteDirectory)
                        {
                            // If the folder is empty, delete it
                            filesInfos = di.GetFiles();
                            if (filesInfos.Length == 0)
                            {
                                DirectoryHelper.DeleteDirectory(directoryPath);
                            }
                        }
                    }
                }

                // If it is a meta file associated with the site, try to remove it from folder for global meta files
                if (!string.IsNullOrEmpty(siteName))
                {
                    DeleteFile(null, fileName, deleteDirectory);
                }
            }
        }


        /// <summary>
        /// Gets the where condition for meta files.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        public static string GetWhereCondition(int objectId, string objectType, string group = null, string where = null)
        {
            bool isEmpty = DataHelper.IsEmpty(where);

            // Add MetaFileObjectID to wherecondition
            if (objectId > 0)
            {
                if (!isEmpty)
                {
                    where += " AND ";
                }
                where += "(MetaFileObjectID = " + objectId + ")";
                isEmpty = false;
            }

            // Add MetaFileObjectType to wherecondition
            if (!DataHelper.IsEmpty(objectType))
            {
                if (!isEmpty)
                {
                    where += " AND ";
                }
                where += "(MetaFileObjectType = '" + SqlHelper.GetSafeQueryString(objectType, false) + "')";
                isEmpty = false;
            }

            // Add MetaFileGroupName to wherecondition
            if (!DataHelper.IsEmpty(group))
            {
                if (!isEmpty)
                {
                    where += " AND ";
                }
                where += "(MetaFileGroupName = N'" + SqlHelper.GetSafeQueryString(group, false) + "')";
            }

            return where;
        }


        /// <summary>
        /// Ensures the binary data in the DataTable of metafiles.
        /// </summary>
        /// <param name="dt">DataTable with the data</param>
        public static void EnsureMetaFileBinaries(DataTable dt)
        {
            if (!DataHelper.DataSourceIsEmpty(dt))
            {
                // Init the binary data
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["MetaFileBinary"] == DBNull.Value)
                    {
                        MetaFileInfo file = new MetaFileInfo(dr);

                        dr["MetaFileBinary"] = GetFileBinary(file.Generalized.ObjectSiteName, GetFullFileName(file.MetaFileGUID.ToString(), file.MetaFileExtension));
                    }
                }
            }
        }


        /// <summary>
        /// Returns list of all object types which have some metafiles attached.
        /// </summary>
        /// <param name="siteId">ID of the site from which the object types should be recieved</param>
        public static List<string> GetMetaFilesObjectTypes(int siteId)
        {
            List<string> objTypes = new List<string>();

            string where = "";
            if (siteId > 0)
            {
                where = " WHERE MetaFileSiteID = " + siteId;
            }

            DataSet ds = ConnectionHelper.ExecuteQuery("SELECT DISTINCT MetaFileObjectType FROM CMS_MetaFile " + where, null, QueryTypeEnum.SQLQuery);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    string objType = dr[0].ToString();
                    if (!string.IsNullOrEmpty(objType))
                    {
                        objTypes.Add(objType);
                    }
                }
            }

            return objTypes;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Delete all files with the same name ([name].*).
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="deleteDirectory">Determines whether delete specified directory or not</param>
        internal static void DeleteFile(string siteName, string fileName, bool deleteDirectory)
        {
            DeleteFile(siteName, fileName, deleteDirectory, false);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns all metafiles.
        /// </summary>
        protected virtual ObjectQuery<MetaFileInfo> GetMetaFilesInternal()
        {
            return GetObjectQuery();
        }


        /// <summary>
        /// Returns the MetaFileInfo structure for the specified metaFile.
        /// </summary>
        /// <param name="metaFileGuid">MetaFile guid</param>
        /// <param name="siteName">Site name</param>
        /// <param name="globalOrLocal">If true, global (local) files are allowed when local (global) not found</param>
        /// <param name="binaryData">Indicates if binary data should be retrieved</param>
        protected virtual MetaFileInfo GetMetaFileInfoInternal(Guid metaFileGuid, string siteName, bool globalOrLocal, bool binaryData = true)
        {
            // Prepare the where condition
            string where = GetGUIDWhereCondition(metaFileGuid, siteName, globalOrLocal);
            string orderBy = (siteName == null) ? "MetaFileSiteID ASC" : "MetaFileSiteID DESC";

            return GetMetaFiles().Where(where).OrderBy(orderBy).TopN(1).BinaryData(binaryData).FirstOrDefault();
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(MetaFileInfo info)
        {
            if (info == null)
            {
                throw new Exception("[MetaFileInfoProvider.SetMetaFileInfo]: No MetaFileInfo object set.");
            }

            // Get site name
            string siteName = info.Generalized.ObjectSiteName;

            BinaryData newBinaryData = null;
            var filesLocationType = FileHelper.FilesLocationType(siteName);

            // Ensure the binary data if necessary
            if ((filesLocationType != FilesLocationTypeEnum.FileSystem) || (info.InputStream == null) || WebFarmHelper.WebFarmEnabled)
            {
                newBinaryData = info.Generalized.EnsureBinaryData();
            }
            else if ((info.InputStream != null) && (filesLocationType != FilesLocationTypeEnum.Database))
            {
                // Init metafile binary from InputStream and clear it to ensure data for saving to disk
                newBinaryData = info.Generalized.EnsureBinaryData();
            }

            // No file data are stored in DB - clear the binary column
            if (filesLocationType == FilesLocationTypeEnum.FileSystem)
            {
                info.MetaFileBinary = null;
            }

            // Reset the width and height field if not an image
            if (!ImageHelper.IsImage(info.MetaFileExtension))
            {
                info.MetaFileImageHeight = 0;
                info.MetaFileImageWidth = 0;
            }

            var oldInfo = GetMetaFileInfo(info.MetaFileID);
            var oldChecksum = oldInfo?.BinaryDataChecksum;
            var newChecksum = newBinaryData?.Checksum;
            var dataChanged = (oldChecksum != newChecksum);

            // Save the metafile record
            base.SetInfo(info);

            string guid = Convert.ToString(info.MetaFileGUID);

            // Save file to disk
            if (dataChanged)
            {
                if (filesLocationType != FilesLocationTypeEnum.Database)
                {
                    try
                    {
                        SaveFileToDisk(siteName, guid, guid, info.MetaFileExtension, newBinaryData, true);
                    }
                    catch (PermissionException)
                    {
                        // Delete metafile record from DB when saving file failed
                        DeleteInfo(info);
                        throw;
                    }
                }
                else
                {
                    // Ensures that changes are visible immediately, i.e. clearing cache is not needed
                    WebFarmHelper.CreateTask(new UpdateMetaFileWebFarmTask
                    {
                        SiteName = siteName,
                        FileGuid = guid,
                        FileName = guid,
                        FileExtension = info.MetaFileExtension,
                        DeleteOldFiles = true
                    });
                }
            }
            else
            {
                // Update timestamp
                UpdatePhysicalFileLastWriteTime(siteName, guid, info.MetaFileExtension);
            }
        }


        /// <param name="columns">Data columns to return</param>
        /// <param name="where">Where condition to filter data</param>
        /// <param name="orderBy">Order by statement</param>
        /// <param name="topN">Specifies number of returned records</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total number of available records</param>
        protected virtual TypedDataSet GetMetaFilesInternal(string columns, string where, string orderBy, int topN, int offset, int maxRecords, ref int totalRecords)
        {
            var query = GetMetaFiles().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(true);

            query.Offset = offset;
            query.MaxRecords = maxRecords;

            var result = query.TypedResult;
            totalRecords = query.TotalRecords;

            return result;
        }


        /// <summary>
        /// Gets the file list of the files for certain object without the binary file data.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order By</param>
        protected virtual TypedDataSet GetMetaFilesWithoutBinaryInternal(int objectId, string objectType, string group, string where, string orderBy)
        {
            if (objectId <= 0)
            {
                return null;
            }

            where = GetWhereCondition(objectId, objectType, group, where);

            return GetMetaFiles().Where(where).OrderBy(orderBy).BinaryData(false).TypedResult;
        }


        /// <summary>
        /// Delete all files associated to the given object.
        /// </summary>
        /// <param name="objectId">Object id</param>
        /// <param name="objectType">Meta files type</param>
        /// <param name="category">Category</param>
        protected virtual bool DeleteFilesInternal(int objectId, string objectType, string category)
        {
            bool someFilesDeleted = false;

            // Get the meta files
            DataSet ds = GetMetaFilesWithoutBinary(objectId, objectType, category, null, null);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Delete the meta file
                    MetaFileInfo metaFileObj = new MetaFileInfo(dr);
                    DeleteMetaFileInfo(metaFileObj);

                    someFilesDeleted = true;
                }
            }

            return someFilesDeleted;
        }


        /// <summary>
        /// Deletes all files associated to the given object.
        /// </summary>
        /// <param name="infoObject">Info object</param>
        /// <param name="category">Meta files category</param>
        protected virtual void DeleteFilesInternal(GeneralizedInfo infoObject, string category)
        {
            DeleteFilesInternal(infoObject.ObjectID, infoObject.TypeInfo.ObjectType, category);
        }

        #endregion


        #region "File management methods"

        /// <summary>
        /// Ensures the file in the file system and returns the path to the file.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="siteName">Site name</param>
        public static string EnsurePhysicalFile(MetaFileInfo fileInfo, string siteName)
        {
            var filesLocationType = FileHelper.FilesLocationType(siteName);
            if ((filesLocationType == FilesLocationTypeEnum.Database) || (fileInfo == null))
            {
                return null;
            }

            // Check if the file exists
            string stringGuid = fileInfo.MetaFileGUID.ToString();
            string path = GetFilePhysicalPath(siteName, stringGuid, fileInfo.MetaFileExtension);
            if (File.Exists(path))
            {
                return path;
            }

            lock (ensureFileLock)
            {
                // Check the file existence again (in case other thread created it)
                if (File.Exists(path))
                {
                    return path;
                }

                // Create new file if the file does not exist
                byte[] data = fileInfo.EnsureBinaryData(false);
                if (data != null)
                {
                    try
                    {
                        return SaveFileToDisk(siteName, stringGuid, stringGuid, fileInfo.MetaFileExtension, data, true);
                    }
                    catch (Exception ex)
                    {
                        CoreServices.EventLog.LogException("MetaFile", "SaveFileToDisk", ex);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Ensures the thumbnail file.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="width">File width</param>
        /// <param name="height">File height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        public static string EnsureThumbnailFile(MetaFileInfo fileInfo, string siteName, int width, int height, int maxSideSize)
        {
            var filesLocationType = FileHelper.FilesLocationType(siteName);
            if ((filesLocationType == FilesLocationTypeEnum.Database) || !GenerateThumbnails(siteName) || (fileInfo == null))
            {
                return null;
            }

            // Get new dimensions
            int originalWidth = fileInfo.MetaFileImageWidth;
            int originalHeight = fileInfo.MetaFileImageHeight;
            int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

            // If new thumbnail dimensions are different from the original ones, get resized file
            bool resize = (((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0));
            if (!resize)
            {
                return EnsurePhysicalFile(fileInfo, siteName);
            }

            // Check if the file exists
            string stringGuid = fileInfo.MetaFileGUID.ToString();
            string path = GetThumbnailPhysicalPath(siteName, stringGuid, fileInfo.MetaFileExtension, newDims[0], newDims[1]);
            if (File.Exists(path))
            {
                return path;
            }

            lock (ensureFileLock)
            {
                // Check the file existence again (in case other thread created it)
                if (File.Exists(path))
                {
                    return path;
                }

                // Create new file if the file does not exist
                byte[] data = GetImageThumbnail(fileInfo, siteName, newDims[0], newDims[1], 0);
                if (data != null)
                {
                    try
                    {
                        // Save the data to the disk
                        string fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, newDims[0], newDims[1]);
                        return SaveFileToDisk(siteName, stringGuid, fileName, fileInfo.MetaFileExtension, data, false, true);
                    }
                    catch (Exception ex)
                    {
                        CoreServices.EventLog.LogException("MetaFile", "SaveThumbnailFileToDisk", ex);
                    }
                }
            }

            return null;
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="guid">Guid of the file to get</param>
        /// <param name="siteName">Site name</param>
        public static byte[] GetFile(Guid guid, string siteName)
        {
            byte[] fileContent = null;
            var filesLocationType = FileHelper.FilesLocationType(siteName);

            if (filesLocationType != FilesLocationTypeEnum.Database)
            {
                // Files are stored in file system - Get from the file system primarily
                MetaFileInfo fileInfo = GetMetaFileInfoWithoutBinary(guid, siteName, true);
                if (fileInfo != null)
                {
                    fileContent = GetFile(fileInfo, siteName);
                }
            }
            else if (filesLocationType != FilesLocationTypeEnum.FileSystem)
            {
                // Get from the database
                fileContent = GetFileBinary(guid, 0, siteName, false);
            }

            return fileContent;
        }


        /// <summary>
        /// Returns the file from disk or (if not available on the disk) from database.
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="siteName">Site name</param>
        public static byte[] GetFile(MetaFileInfo fileInfo, string siteName)
        {
            byte[] fileContent = null;

            if (fileInfo != null)
            {
                // Global object file
                if (fileInfo.MetaFileSiteID <= 0)
                {
                    siteName = null;
                }

                var filesLocationType = FileHelper.FilesLocationType(siteName);

                if (filesLocationType != FilesLocationTypeEnum.Database)
                {
                    // Files are stored in file system - Get from the file system primarily
                    bool fileValid = false;

                    // Get file path
                    string filePath = GetExistingFilePhysicalPath(siteName, fileInfo.MetaFileGUID.ToString(), fileInfo.MetaFileExtension);

                    // Check if the file on the disk is valid (can use the file from disk)
                    if (!String.IsNullOrEmpty(filePath))
                    {
                        // If the size is valid, load from the file system
                        FileInfo fi = FileInfo.New(filePath);
                        if (fi.LastWriteTime >= fileInfo.MetaFileLastModified)
                        {
                            fileValid = true;
                        }
                    }

                    if (fileValid)
                    {
                        // Get file contents from file system
                        fileContent = File.ReadAllBytes(filePath);
                    }
                    else
                    {
                        // If the file has not been found, seek it in database and save it in file system
                        fileContent = GetFileBinary(fileInfo.MetaFileGUID, fileInfo.MetaFileID, siteName, true);

                        // If no content found but file still exists, return the file
                        if ((fileContent == null) && File.Exists(filePath))
                        {
                            fileContent = File.ReadAllBytes(filePath);
                        }
                    }
                }
                else if (filesLocationType != FilesLocationTypeEnum.FileSystem)
                {
                    // Get from the database
                    fileContent = GetFileBinary(fileInfo.MetaFileGUID, fileInfo.MetaFileID, siteName, false);
                }
            }

            return fileContent;
        }


        /// <summary>
        /// Returns meta file binary and optionaly store it in file system.
        /// </summary>
        /// <param name="guid">Guid of the file to get</param>
        /// <param name="id">ID of the file to get</param>
        /// <param name="siteName">Site name</param>
        /// <param name="storeInFileSystem">If true, given meta file is stored in file system</param>
        private static byte[] GetFileBinary(Guid guid, int id, string siteName, bool storeInFileSystem)
        {
            string stringGuid = guid.ToString();
            byte[] binary = null;

            // Get attachment with binary from database
            MetaFileInfo fileInfo = (id > 0) ? GetMetaFileInfo(id) : GetMetaFileInfo(guid, siteName, false);

            if (fileInfo != null)
            {
                // Get file content from meta file
                binary = fileInfo.MetaFileBinary;

                // Save file to the disk for next use
                if ((storeInFileSystem) && (fileInfo.MetaFileBinary != null))
                {
                    try
                    {
                        SaveFileToDisk(siteName, stringGuid, stringGuid, fileInfo.MetaFileExtension, fileInfo.MetaFileBinary, true);
                    }
                    catch (Exception ex)
                    {
                        CoreServices.EventLog.LogException("MetaFileInfoProvider.GetFileBinary", "E", ex);
                    }
                }
            }

            return binary;
        }


        /// <summary>
        /// Returns the file binary from disk.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileName">Name of the file to get, including extension</param>
        public static byte[] GetFileBinary(string siteName, string fileName)
        {
            byte[] fileContent = null;

            // Get file name and file extension
            string extension = "";
            string file = "";

            int dotIndex = fileName.IndexOf(".", StringComparison.OrdinalIgnoreCase);
            if (dotIndex > 1)
            {
                file = fileName.Substring(0, dotIndex);
                extension = fileName.Substring(dotIndex);
            }

            string filePath = GetFilePhysicalPath(siteName, file, extension);
            if (File.Exists(filePath))
            {
                // Get file contents from file system
                fileContent = File.ReadAllBytes(filePath);
            }

            return fileContent;
        }


        /// <summary>
        /// Update file last modified attribute, so it's evaluated us up to date.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileName">File name</param>
        /// <param name="extension">File extension</param>
        internal static void UpdatePhysicalFileLastWriteTime(string siteName, string fileName, string extension)
        {
            var filesLocationType = FileHelper.FilesLocationType(siteName);

            // Update timestamp
            if (filesLocationType == FilesLocationTypeEnum.Database)
            {
                return;
            }

            // Get file path
            string filePath = GetExistingFilePhysicalPath(siteName, fileName, extension);

            if (!String.IsNullOrEmpty(filePath))
            {
                // If the size is valid, load from the file system
                FileInfo fi = FileInfo.New(filePath);
                fi.LastWriteTime = DateTime.Now;
            }
        }


        /// <summary>
        /// Checks whether the image should be processed (resized) by the Image manager (if the destination size is smaller).
        /// </summary>
        /// <param name="fileInfo">Meta file info to check</param>
        /// <param name="width">New width</param>
        /// <param name="height">New height</param>
        /// <param name="maxSideSize">Max side size</param>
        public static bool CanResizeImage(MetaFileInfo fileInfo, int width, int height, int maxSideSize)
        {
            if (fileInfo == null)
            {
                return false;
            }

            // Resize only when bigger than required
            if (maxSideSize > 0)
            {
                if ((maxSideSize < fileInfo.MetaFileImageWidth) || (maxSideSize < fileInfo.MetaFileImageHeight))
                {
                    return true;
                }
            }
            else
            {
                if ((width > 0) && (fileInfo.MetaFileImageWidth > width))
                {
                    return true;
                }
                if ((height > 0) && (fileInfo.MetaFileImageHeight > height))
                {
                    return true;
                }
            }

            return false;
        }


        /// <summary>
        /// Returns image thumbnail (from the disk - if already exists, or create new one and save it to disk - if doesnt exist).
        /// </summary>
        /// <param name="fileInfo">File info</param>
        /// <param name="siteName">Site name</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        public static byte[] GetImageThumbnail(MetaFileInfo fileInfo, string siteName, int width, int height, int maxSideSize)
        {
            if (fileInfo == null)
            {
                return null;
            }

            byte[] thumbnail = null;
            byte[] data = fileInfo.MetaFileBinary;

            if (ImageHelper.IsImage(fileInfo.MetaFileExtension))
            {
                // Get new dimensions
                int originalWidth = fileInfo.MetaFileImageWidth;
                int originalHeight = fileInfo.MetaFileImageHeight;
                int[] newDims = ImageHelper.EnsureImageDimensions(width, height, maxSideSize, originalWidth, originalHeight);

                // If new thumbnail dimensions are different from the original ones, resize the file
                bool resize = (((newDims[0] != originalWidth) || (newDims[1] != originalHeight)) && (newDims[0] > 0) && (newDims[1] > 0));
                var filesLocationType = FileHelper.FilesLocationType(siteName);

                if (resize && (filesLocationType != FilesLocationTypeEnum.Database))
                {
                    thumbnail = GetImageThumbnailFile(fileInfo.MetaFileGUID, siteName, newDims[0], newDims[1]);
                }

                // Create the thumbnail if not yet present
                if (thumbnail == null)
                {
                    // If no data available, ensure the source data
                    if (data == null)
                    {
                        data = GetFile(fileInfo, siteName);
                    }

                    // Resize the image
                    if ((data != null) && (resize))
                    {
                        ImageHelper imgHelper = new ImageHelper(data, originalWidth, originalHeight);
                        thumbnail = imgHelper.GetResizedImageData(newDims[0], newDims[1]);
                    }
                }
            }

            // If no thumbnail created, return original size
            return thumbnail ?? data;
        }


        /// <summary>
        /// Returns image thumbnail (from the disk - if already exists, or create new one and save it to disk - if doesnt exist).
        /// </summary>
        /// <param name="guid">File GUID</param>
        /// <param name="imageData">Image data</param>
        /// <param name="extension">Image extenstion</param>
        /// <param name="siteName">Site name</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <param name="maxSideSize">Maximum side size</param>
        /// <param name="originalWidth">Original width of the image</param>
        /// <param name="originalHeight">Original height of the image</param>
        public static byte[] GetImageThumbnail(Guid guid, byte[] imageData, string extension, string siteName, int width, int height, int maxSideSize, int originalWidth, int originalHeight)
        {
            byte[] thumbnail = null;

            // Process image if data available and file is image
            try
            {
                if ((imageData != null) && ImageHelper.IsImage(extension))
                {
                    // Get new dimensions
                    ImageHelper imgHelper = new ImageHelper(imageData, originalWidth, originalHeight);
                    int[] newDims = imgHelper.EnsureImageDimensions(width, height, maxSideSize);

                    // If new thumbnail dimensions are different from the original ones, resize the file
                    if (((newDims[0] != imgHelper.ImageWidth) || (newDims[1] != imgHelper.ImageHeight)) && ((newDims[0] > 0) && (newDims[1] > 0)))
                    {
                        var filesLocationType = FileHelper.FilesLocationType(siteName);
                        var storeFilesInFileSystem = filesLocationType != FilesLocationTypeEnum.Database;

                        // Try to get image thumbnail from the disk
                        if (storeFilesInFileSystem)
                        {
                            thumbnail = GetImageThumbnailFile(guid, siteName, newDims[0], newDims[1]);
                        }

                        if (thumbnail == null)
                        {
                            thumbnail = imgHelper.GetResizedImageData(newDims[0], newDims[1]);

                            // Save image thumbnail to disk if necessary
                            if (storeFilesInFileSystem && GenerateThumbnails(siteName))
                            {
                                string thumbnailFileName = ImageHelper.GetImageThumbnailFileName(guid.ToString(), newDims[0], newDims[1]);
                                SaveFileToDisk(siteName, guid.ToString(), thumbnailFileName, extension, thumbnail, false, true);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error
                CoreServices.EventLog.LogException("MetaFileManager", "CreateThumbnail", ex);
            }

            return thumbnail ?? imageData;
        }


        /// <summary>
        /// Returns the image thumbnail from the disk.
        /// </summary>
        /// <param name="guid">Guid of the file to get</param>
        /// <param name="siteName">Site name</param>
        /// <param name="height">Image thumbnail width</param>
        /// <param name="width">Image thumbnail height</param>
        public static byte[] GetImageThumbnailFile(Guid guid, string siteName, int width, int height)
        {
            var filesLocationType = FileHelper.FilesLocationType(siteName);

            // Files are stored in database
            if (filesLocationType == FilesLocationTypeEnum.Database)
            {
                return null;
            }

            // Get meta file without binary
            MetaFileInfo fileInfo = GetMetaFileInfoWithoutBinary(guid, siteName, true);
            if (fileInfo == null)
            {
                return null;
            }

            byte[] fileContent = null;

            string stringGuid = guid.ToString();
            string fileExtension = fileInfo.MetaFileExtension;

            string fileName = ImageHelper.GetImageThumbnailFileName(stringGuid, width, height);

            // Get file path
            string filePath = GetExistingFilePhysicalPath(siteName, fileName, fileExtension);

            // Get the data
            if (!String.IsNullOrEmpty(filePath))
            {
                fileContent = File.ReadAllBytes(filePath);
            }

            return fileContent;
        }


        /// <summary>
        /// Resizes specified metafiles to the required dimensions.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        /// <param name="width">Image widht</param>
        /// <param name="height">Image height</param>
        /// <param name="maxSideSize">Image max side size</param>
        public static void ResizeMetaFiles(int objectId, string objectType, string group, string where, int width, int height, int maxSideSize)
        {
            // Get required files with binary data
            DataSet dsFiles = GetMetaFilesWithBinary(objectId, objectType, group, where, "");
            if (!DataHelper.DataSourceIsEmpty(dsFiles))
            {
                foreach (DataRow dr in dsFiles.Tables[0].Rows)
                {
                    MetaFileInfo file = new MetaFileInfo(dr);

                    // If file data found AND it is an image AND it can be resized
                    if ((file.MetaFileBinary != null) && ImageHelper.IsImage(file.MetaFileExtension) &&
                        CanResizeImage(file, width, height, maxSideSize))
                    {
                        // Get meta file site name
                        string siteName = file.Generalized.ObjectSiteName;

                        // Create image thumbnail
                        byte[] thumbnail = GetImageThumbnail(file.MetaFileGUID, file.MetaFileBinary, file.MetaFileExtension, siteName, width, height, maxSideSize, file.MetaFileImageWidth, file.MetaFileImageHeight);
                        if (thumbnail != null)
                        {
                            ImageHelper ih = new ImageHelper(thumbnail);
                            if (ih.SourceData != null)
                            {
                                // Update width, height and size
                                file.MetaFileBinary = thumbnail;
                                file.MetaFileImageHeight = ih.ImageHeight;
                                file.MetaFileImageWidth = ih.ImageWidth;
                                file.MetaFileSize = ih.SourceData.Length;

                                // Save changes
                                SetMetaFileInfo(file);
                            }
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Changes images quality using GDI+ built in JPEG encoder.
        /// </summary>
        /// <param name="objectId">ID of specific object</param>
        /// <param name="objectType">Object type</param>
        /// <param name="group">Group name</param>
        /// <param name="where">Where condition</param>
        /// <param name="quality">New image quality, from 0 (lowest quality) to 100 (highest quality)</param>
        public static void SetMetaFilesQuality(int objectId, string objectType, string group, string where, int quality)
        {
            // Get required files with binary data
            var metaFiles = GetMetaFilesWithBinary(objectId, objectType, group, where, "");
            if (!DataHelper.DataSourceIsEmpty(metaFiles))
            {
                foreach (var file in metaFiles)
                {
                    if ((file.MetaFileBinary != null) && ImageHelper.IsImage(file.MetaFileExtension))
                    {
                        // Ensure image quality
                        if (quality > 100)
                        {
                            quality = 100;
                        }
                        else if (quality < 0)
                        {
                            quality = 0;
                        }

                        // Modify image quality
                        var ih = new ImageHelper(file.MetaFileBinary);
                        file.MetaFileBinary = ih.ImageToBytes(ih.ImageData, quality);
                        file.MetaFileSize = file.MetaFileBinary.Length;

                        // Save changes
                        SetMetaFileInfo(file);
                    }
                }
            }
        }

        #endregion


        #region "Physical path methods"

        /// <summary>
        /// Returns physical path to the file.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="guid">File GUID</param>
        /// <param name="extension">File extension</param>
        public static string GetFilePhysicalPath(string siteName, string guid, string extension)
        {
            // Get meta file physical path
            return GetFileFolder(siteName, guid) + AttachmentHelper.GetFullFileName(guid, extension);
        }


        /// <summary>
        /// Returns physical path to the file if exists, otherwise null.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="fileName">File name</param>
        /// <param name="extension">File extension</param>
        private static string GetExistingFilePhysicalPath(string siteName, string fileName, string extension)
        {
            // Get file path
            string filePath = GetFilePhysicalPath(siteName, fileName, extension);

            // If it is a meta file associated with the given site AND corresponding file was not found
            if ((!string.IsNullOrEmpty(siteName)) && !File.Exists(filePath))
            {
                // Get file path based on the folder for global meta files
                filePath = GetFilePhysicalPath(null, fileName, extension);
            }

            return File.Exists(filePath) ? filePath : null;
        }


        /// <summary>
        /// Returns physical path to the thumbnail.
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="guid">File GUID</param>
        /// <param name="extension">File extension</param>
        /// <param name="width">Width</param>
        /// <param name="height">Height</param>
        public static string GetThumbnailPhysicalPath(string siteName, string guid, string extension, int width, int height)
        {
            // Get thumbnail file name and physical file path
            string fileName = ImageHelper.GetImageThumbnailFileName(guid, width, height);
            string filePath = GetFilePhysicalPath(siteName, fileName, extension);

            return filePath;
        }


        /// <summary>
        /// Returns physical path to the meta file folder (meta files folder path + subfolder).
        /// </summary>
        /// <param name="siteName">Site name</param>
        /// <param name="guid">Meta file GUID</param>
        private static string GetFileFolder(string siteName, string guid)
        {
            // Subfolder name start with first two letters of the names of files which are placed in the folder
            string subfolder = AttachmentHelper.GetFileSubfolder(guid);

            return DirectoryHelper.CombinePath(GetFilesFolderPath(siteName) + subfolder) + "\\";
        }


        /// <summary>
        /// Returns physical path to folder with meta files which are associated with the specified site.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static string GetFilesFolderPath(string siteName)
        {
            // Get meta files folder path from the settings
            string filesFolderPath = FileHelper.FilesFolder(siteName);

            // Folder is not specified in settings -> get default files folder path
            if (filesFolderPath == "")
            {
                if (!string.IsNullOrEmpty(siteName))
                {
                    // Site meta files
                    filesFolderPath = DirectoryHelper.CombinePath(WebApplicationPhysicalPath, siteName, "metafiles") + "\\";
                }
                else
                {
                    // Global meta files
                    filesFolderPath = DirectoryHelper.CombinePath(WebApplicationPhysicalPath, "CMSFiles") + "\\";
                }
            }
            // Folder is specified in settings -> ensure files folder path
            else
            {
                // Get full physical path
                filesFolderPath = FileHelper.GetFullFolderPhysicalPath(filesFolderPath, WebApplicationPhysicalPath);

                // Check if site specific folder should be used
                if (FileHelper.UseSiteSpecificCustomFolder(siteName))
                {
                    filesFolderPath += siteName + "\\";
                }
            }

            return filesFolderPath;
        }

        #endregion


        #region "URL methods"

        /// <summary>
        /// Returns meta file url.
        /// </summary>
        /// <param name="metaFileGuid">Meta file GUID</param>
        /// <param name="fileName">File name without extension</param>
        public static string GetMetaFileUrl(Guid metaFileGuid, string fileName)
        {
            return MetaFileURLProvider.GetMetaFileUrl(metaFileGuid, fileName);
        }

        #endregion
    }
}
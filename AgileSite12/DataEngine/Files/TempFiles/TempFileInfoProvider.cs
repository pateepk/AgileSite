using System;
using System.Data;
using System.Linq;

using CMS.Core;
using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class providing TempFileInfo management.
    /// </summary>
    public class TempFileInfoProvider : AbstractInfoProvider<TempFileInfo, TempFileInfoProvider>
    {
        #region "Constants"

        /// <summary>
        /// Name of the parent folder for image editor temporary files.
        /// </summary>
        public const string IMAGE_EDITOR_FOLDER = "ImageEditor";


        /// <summary>
        /// Name of the parent folder for multifile uploader temporary files.
        /// </summary>
        public const string MULTIFILE_UPLOADER_FOLDER = "MultiFileUploader";

        #endregion


        #region "Variables"

        /// <summary>
        /// Lock object for ensuring of the physical files.
        /// </summary>
        private static object ensureFileLock = new object();


        /// <summary>
        /// Temporary files folder path.
        /// </summary>
        private static string mTemporaryFilesFolderPath;

        #endregion


        #region "Properties"

        /// <summary>
        /// Temporary files folder path (default: ~/App_Data/CMSTemp/)
        /// </summary>
        public static string TemporaryFilesFolderPath
        {
            get
            {
                return mTemporaryFilesFolderPath ?? (mTemporaryFilesFolderPath = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSTemporaryFilesFolderPath"], "~/App_Data/CMSTemp/"));
            }
            set
            {
                mTemporaryFilesFolderPath = value;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns all temp files.
        /// </summary>
        public static ObjectQuery<TempFileInfo> GetTempFiles()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns temporary file with specified ID.
        /// </summary>
        /// <param name="tempFileId">Temporary file ID</param>
        public static TempFileInfo GetTempFileInfo(int tempFileId)
        {
            return ProviderObject.GetTempFileInfoInternal(tempFileId);
        }


        /// <summary>
        /// Returns temporary file with specified number.
        /// </summary>
        /// <param name="scopeGuid">GUID of the parent scope</param>
        /// <param name="number">Temporary file number within the parent scope</param>
        public static TempFileInfo GetTempFileInfo(Guid scopeGuid, int number)
        {
            return ProviderObject.GetTempFileInfoInternal(scopeGuid, number);
        }


        /// <summary>
        /// Sets (updates or inserts) specified temporary file.
        /// </summary>
        /// <param name="tempFileObj">Temporary file to be set</param>
        public static void SetTempFileInfo(TempFileInfo tempFileObj)
        {
            ProviderObject.SetTempFileInfoInternal(tempFileObj);
        }


        /// <summary>
        /// Deletes specified temporary file.
        /// </summary>
        /// <param name="tempFileObj">Temporary file to be deleted</param>
        public static void DeleteTempFileInfo(TempFileInfo tempFileObj)
        {
            ProviderObject.DeleteTempFileInfoInternal(tempFileObj);
        }


        /// <summary>
        /// Deletes temporary file with specified ID.
        /// </summary>
        /// <param name="tempFileId">Temporary file ID</param>
        public static void DeleteTempFileInfo(int tempFileId)
        {
            TempFileInfo tempFileObj = GetTempFileInfo(tempFileId);
            DeleteTempFileInfo(tempFileObj);
        }


        /// <summary>
        /// Deletes all temporary files for the given parent scope.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeGuid">GUID of the parent scope</param>
        public static void DeleteTempFiles(string parentFolder, Guid scopeGuid)
        {
            ProviderObject.DeleteTempFilesInternal(parentFolder, scopeGuid);
        }


        /// <summary>
        /// Deletes all the temporary files with number between lower and upper bound for the given parent scope.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeGuid">GUID of the parent scope</param>
        /// <param name="lowerBound">Number of the temporary file which should be deleted as the first</param>
        /// <param name="upperBound">Number of the temporary file which should be deleted as the last</param>
        public static void DeleteTempFiles(string parentFolder, Guid scopeGuid, int lowerBound, int upperBound)
        {
            ProviderObject.DeleteTempFilesInternal(parentFolder, scopeGuid, lowerBound, upperBound);
        }


        /// <summary>
        /// Deletes old temporary files from the specified parent folder.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="olderThan">All temporary files older than this date/time are deleted</param>
        public static void DeleteTempFiles(string parentFolder, DateTime olderThan)
        {
            ProviderObject.DeleteTempFilesInternal(parentFolder, olderThan);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns temporary file with specified ID.
        /// </summary>
        /// <param name="tempFileId">Temporary file ID</param>
        protected virtual TempFileInfo GetTempFileInfoInternal(int tempFileId)
        {
            if (tempFileId <= 0)
            {
                return null;
            }

            // Get data from database
            return GetObjectQuery().BinaryData(true).WhereEquals("FileID", tempFileId).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Returns temporary file from the specified parent scope and with specified number.
        /// </summary>
        /// <param name="scopeGuid">GUID of the parent scope</param>
        /// <param name="number">Number of the temporary file to return</param>
        protected virtual TempFileInfo GetTempFileInfoInternal(Guid scopeGuid, int number)
        {
            return GetTempFiles().WhereEquals("FileParentGUID", scopeGuid).WhereEquals("FileNumber", number).TopN(1).BinaryData(true).FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified temporary file.
        /// </summary>
        /// <param name="tempFileObj">Temporary file to be set</param>
        protected virtual void SetTempFileInfoInternal(TempFileInfo tempFileObj)
        {
            if (tempFileObj == null)
            {
                throw new Exception("[TempFileInfoProvider.SetTempFileInfo]: No TempFileInfo object set.");
            }

            // Use transaction
            using (var tr = BeginTransaction())
            {
                // Ensure physical file
                EnsurePhysicalFile(tempFileObj);

                var locationType = FileHelper.FilesLocationType(tempFileObj.FileSiteName);
                if (locationType == FilesLocationTypeEnum.FileSystem)
                {
                    tempFileObj.FileBinary = null;
                }

                if (tempFileObj.FileID > 0)
                {
                    tempFileObj.Generalized.UpdateData();
                }
                else
                {
                    tempFileObj.Generalized.InsertData();
                }

                // Commit transaction
                tr.Commit();
            }
        }


        /// <summary>
        /// Deletes specified temporary file.
        /// </summary>
        /// <param name="tempFileObj">Temporary file to be deleted</param>
        protected virtual void DeleteTempFileInfoInternal(TempFileInfo tempFileObj)
        {
            if (tempFileObj == null)
            {
                return;
            }

            // Use transaction
            using (var tr = BeginTransaction())
            {
                // Delete temporary file
                DeleteInfo(tempFileObj);

                var locationType = FileHelper.FilesLocationType(tempFileObj.FileSiteName);
                if (locationType != FilesLocationTypeEnum.Database)
                {
                    string path = GetTempFilePath(tempFileObj.FileDirectory, tempFileObj.FileParentGUID, tempFileObj.FileNumber, tempFileObj.FileExtension);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }

                tr.Commit();
            }
        }


        /// <summary>
        /// Deletes all the temporary files from the given parent scope.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeGuid">GUID of the parent scope</param>
        protected virtual void DeleteTempFilesInternal(string parentFolder, Guid scopeGuid)
        {
            DeleteTempFilesInternal(parentFolder, scopeGuid, -1, -1);
        }


        /// <summary>
        /// Deletes all the temporary files with number between lower and upper bound from the given parent scope.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeGuid">GUID of the parent scope</param>
        /// <param name="lowerBound">Number of temporary file which should be deleted as the first</param>
        /// <param name="upperBound">Number of the temporary file which should be deleted as the last</param>
        protected virtual void DeleteTempFilesInternal(string parentFolder, Guid scopeGuid, int lowerBound, int upperBound)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@ParentGUID", scopeGuid);

            string where;
            if ((lowerBound < 0) && (upperBound < 0))
            {
                // No additional where condition (delete all versions according to scopeGuid)
                where = "";
            }
            else if (lowerBound <= upperBound)
            {
                where = "FileNumber > " + lowerBound + " AND FileNumber <= " + upperBound;
            }
            else
            {
                where = "(FileNumber >= 0 AND FileNumber <= " + upperBound + ") OR FileNumber > " + lowerBound;
            }

            // Delete from DB
            ConnectionHelper.ExecuteQuery("temp.file.deleteversionsbyguid", parameters, where);

            // Get hidden files for current file
            string[] files = null;
            string directoryPath = GetTempFilesFolderPath(parentFolder, scopeGuid);
            try
            {
                files = Directory.GetFiles(directoryPath);
            }
            catch
            {
            }

            if (files != null)
            {
                // Delete all files with number higher than lastVersion
                foreach (string file in files)
                {
                    int number = ValidationHelper.GetInteger(Path.GetFileNameWithoutExtension(file), 0);
                    if (lowerBound < upperBound)
                    {
                        if (number > lowerBound && number <= upperBound)
                        {
                            File.Delete(file);
                        }
                    }
                    else
                    {
                        if ((number > lowerBound) || ((number <= upperBound) && number >= 0))
                        {
                            File.Delete(file);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Deletes old temporary files from the specified parent folder.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="olderThan">All temporary files older than this date/time are deleted</param>
        protected virtual void DeleteTempFilesInternal(string parentFolder, DateTime olderThan)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@OlderThan", olderThan);

            string where = "FileDirectory = '" + SqlHelper.GetSafeQueryString(parentFolder, false) + "'";

            DataSet ds = ConnectionHelper.ExecuteQuery("temp.file.getversionsolderthan", parameters, where, null, 0, "FileNumber, FileExtension, FileDirectory, FileParentGUID");
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    Guid scopeGuid = ValidationHelper.GetGuid(dr["FileParentGUID"], Guid.Empty);
                    int number = ValidationHelper.GetInteger(dr["FileNumber"], 0);
                    string ext = ValidationHelper.GetString(dr["FileExtension"], "");

                    string path = GetTempFilePath(parentFolder, scopeGuid, number, ext);
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                }
            }

            // Delete files from DB
            ConnectionHelper.ExecuteQuery("temp.file.deleteversionsolderthan", parameters);
        }

        #endregion


        #region "File management methods"

        /// <summary>
        /// Ensures the file in the file system.
        /// </summary>
        /// <param name="fileInfo">Temporary file</param>
        public static void EnsurePhysicalFile(TempFileInfo fileInfo)
        {
            var locationType = FileHelper.FilesLocationType(fileInfo.FileSiteName);
            if ((fileInfo == null) || (locationType == FilesLocationTypeEnum.Database))
            {
                return;
            }

            // Check if the file exists
            string path = GetTempFilePath(fileInfo.FileDirectory, fileInfo.FileParentGUID, fileInfo.FileNumber, fileInfo.FileExtension);
            lock (ensureFileLock)
            {
                // Create new file if the file does not exist
                byte[] data = fileInfo.FileBinary;
                if (data == null)
                {
                    // Load the data from the database
                    if (data == null)
                    {
                        TempFileInfo file = GetTempFileInfo(fileInfo.FileID);
                        if (file != null)
                        {
                            data = file.FileBinary;
                        }
                    }
                }

                if (data == null)
                {
                    return;
                }

                try
                {
                    DirectoryHelper.EnsureDiskPath(path, SystemContext.WebApplicationPhysicalPath);
                    StorageHelper.SaveFileToDisk(path, data);
                }
                catch (Exception ex)
                {
                    CoreServices.EventLog.LogException("TempFile", "SaveFileToDisk", ex);
                }
            }
        }


        /// <summary>
        /// Returns the temporary file binary data from disk.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeGuid">GUID of the parent scope</param>
        /// <param name="fileNumber">Temporary file number</param>
        /// <param name="fileExtension">Temporary file extension</param>
        public static byte[] GetTempFileBinary(string parentFolder, Guid scopeGuid, int fileNumber, string fileExtension)
        {
            return GetTempFileBinary(parentFolder, scopeGuid.ToString(), fileNumber, fileExtension);
        }


        /// <summary>
        /// Returns the temporary file binary data from disk.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeFolder">Scope folder which is located under the parent folder</param>
        /// <param name="fileNumber">Temporary file number</param>
        /// <param name="fileExtension">Temporary file extension</param>
        public static byte[] GetTempFileBinary(string parentFolder, string scopeFolder, int fileNumber, string fileExtension)
        {
            byte[] fileContent = null;

            string filePath = GetTempFilePath(parentFolder, scopeFolder, fileNumber, fileExtension);
            if (File.Exists(filePath))
            {
                // Get file contents from file system
                fileContent = File.ReadAllBytes(filePath);
            }

            return fileContent;
        }


        /// <summary>
        /// Returns physical path to the folder where temporary files are located.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeGuid">Parent scope GUID</param>
        public static string GetTempFilesFolderPath(string parentFolder, Guid scopeGuid)
        {
            return GetTempFilesFolderPath(parentFolder, scopeGuid.ToString());
        }


        /// <summary>
        /// Returns physical path to the folder where temporary files are located.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeFolder">Scope folder which is located under the parent folder</param>
        public static string GetTempFilesFolderPath(string parentFolder, string scopeFolder)
        {
            if (!string.IsNullOrEmpty(parentFolder))
            {
                if (!string.IsNullOrEmpty(scopeFolder))
                {
                    scopeFolder = Path.EnsureEndBackslash(scopeFolder);
                }

                return DirectoryHelper.CombinePath(GetTempFilesRootFolderPath(), parentFolder, scopeFolder.Substring(0, 2), scopeFolder);
            }

            return "";
        }


        /// <summary>
        /// Returns physical path to the root folder of all temporary files.
        /// </summary>
        public static string GetTempFilesRootFolderPath()
        {
            return GetTempFilesRootFolderPath(null);
        }


        /// <summary>
        /// Returns physical path to the root folder of all temporary files.
        /// </summary>
        /// <param name="webFullPath">Full path to the root of the web project (e.g. c:\WebProject\)</param>
        public static string GetTempFilesRootFolderPath(string webFullPath)
        {
            return FileHelper.GetFullFolderPhysicalPath(TemporaryFilesFolderPath, webFullPath);
        }


        /// <summary>
        /// Returns physical path to the specified temporary file.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeGuid">GUID of the parent scope</param>
        /// <param name="fileNumber">Temporary file number</param>
        /// <param name="fileExtension">Temporary file extension</param>
        public static string GetTempFilePath(string parentFolder, Guid scopeGuid, int fileNumber, string fileExtension)
        {
            return GetTempFilePath(parentFolder, scopeGuid.ToString(), fileNumber, fileExtension);
        }


        /// <summary>
        /// Returns physical path to the specified temporary file.
        /// </summary>
        /// <param name="parentFolder">Parent folder which is located under the temporary files root folder</param>
        /// <param name="scopeFolder">Scope folder which is located under the parent folder</param>
        /// <param name="fileNumber">Temporary file number</param>
        /// <param name="fileExtension">Temporary file extension</param>
        public static string GetTempFilePath(string parentFolder, string scopeFolder, int fileNumber, string fileExtension)
        {
            string dirPath = GetTempFilesFolderPath(parentFolder, scopeFolder);
            string fileName = String.Format("{0}.{1}", fileNumber, fileExtension.TrimStart('.'));
            return DirectoryHelper.CombinePath(dirPath, fileName);
        }

        #endregion
    }
}
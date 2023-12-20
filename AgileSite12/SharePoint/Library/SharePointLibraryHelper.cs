using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;

using CMS.Base;
using CMS.DataEngine;
using CMS.Helpers;

using SystemIO = System.IO;


namespace CMS.SharePoint
{
    /// <summary>
    /// Contains useful methods for SharePoint libraries.
    /// </summary>
    internal class SharePointLibraryHelper : AbstractHelper<SharePointLibraryHelper>
    {
        /// <summary>
        /// Error number of <see cref="SqlException"/> thrown when unique constraint is violated.
        /// </summary>
        private const int DUPLICATE_KEY_ROW_EXCEPTION_ERROR_NUMBER = 2601;


        #region "Public methods"

        /// <summary>
        /// Synchronizes the SharePoint library so that the local and server files are the same.
        /// The synchronization utilizes the modification time of each processed file.
        /// </summary>
        /// <param name="sharePointLibrary">SharePointLibraryInfo to be synchronized with the SharePoint server.</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public static void SynchronizeLibrary(SharePointLibraryInfo sharePointLibrary)
        {
            HelperObject.SynchronizeLibraryInternal(sharePointLibrary);
        }


        /// <summary>
        /// Uploads a new file to the specified SharePoint library.
        /// The newly uploaded file is immediately available both on the SharePoint server and locally in the library.
        /// </summary>
        /// <param name="sharePointLibrary">SharePointLibraryInfo where to upload the file.</param>
        /// <param name="contentStream">Stream containing binary data of the file.</param>
        /// <param name="libraryRelativeUrl">Relative URL of the file within library (i.e. "myPicture.jpeg" uploads file to the root folder of the library).</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="libraryRelativeUrl"/> already exists.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public static void UploadFile(SharePointLibraryInfo sharePointLibrary, SystemIO.Stream contentStream, string libraryRelativeUrl)
        {
            HelperObject.UploadFileInternal(sharePointLibrary, contentStream, libraryRelativeUrl);
        }


        /// <summary>
        /// Updates file in SharePoint library with new content and name.
        /// The <paramref name="newFileName"/> can be the same as the old one.
        /// The updated file is immediately available both on the SharePoint server and locally in the library.
        /// </summary>
        /// <param name="sharePointFile">SharePoint file to be updated.</param>
        /// <param name="contentStream">Stream containing the new file's binary content.</param>
        /// <param name="newFileName">File name of the updated file.</param>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="newFileName"/> identifies an already existing file within the library.</exception>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when file identified by <paramref name="sharePointFile"/> does not exist on SharePoint server.</exception>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public static void UpdateFile(SharePointFileInfo sharePointFile, SystemIO.Stream contentStream, string newFileName)
        {
            HelperObject.UpdateFileInternal(sharePointFile, contentStream, newFileName);
        }


        /// <summary>
        /// Deletes the specified file on the SharePoint server and from the local library.
        /// Deleting a non-existent file within SharePoint library (i.e. file is out of sync) deletes the file from the local library as well.
        /// </summary>
        /// <param name="sharePointFile">SharePoint file to be deleted</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public static void DeleteFile(SharePointFileInfo sharePointFile)
        {
            HelperObject.DeleteFileInternal(sharePointFile);
        }


        /// <summary>
        /// Moves the specified file to the recycle bin of the SharePoint server.
        /// Deletes the file from the local library.
        /// Returns the identifier of the new recycle bin item on SharePoint.
        /// Recycling a non-existent file within SharePoint library (i.e. file is out of sync) returns empty GUID and deletes the file from the local library as well.
        /// </summary>
        /// <param name="sharePointFile">SharePoint file to be recycled on the SharePoint server</param>
        /// <returns>GUID of the new recycle bin item on SharePoint, or <see cref="Guid.Empty"/> when file does not exist.</returns>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public static Guid RecycleFile(SharePointFileInfo sharePointFile)
        {
            return HelperObject.RecycleFileInternal(sharePointFile);
        }

        #endregion


        #region "Protected methods"

        /// <summary>
        /// Synchronizes the SharePoint library so that the local and server files are the same.
        /// The synchronization utilizes the modification time of each processed file.
        /// </summary>
        /// <param name="sharePointLibrary">SharePointLibraryInfo to be synchronized with the SharePoint server.</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected virtual void SynchronizeLibraryInternal(SharePointLibraryInfo sharePointLibrary)
        {
            // Verify that the connection is available. Libraries with no connection are read-only and thus can not be synchronized
            SharePointConnectionInfo sharePointConnection = GetSharePointConnectionBySharePointLibrary(sharePointLibrary, "Library can not be synchronized.");

            // Retrieve list of files from the server
            var connectionData = sharePointConnection.ToSharePointConnectionData();
            ISharePointListService listService = SharePointServices.GetService<ISharePointListService>(connectionData);
            ISharePointFileService fileService = SharePointServices.GetService<ISharePointFileService>(connectionData);
            SharePointView sharePointView = new SharePointView
            {
                ViewFields = new List<string> {"FileRef", "Modified"}, // Only those fields are needed
                Scope = SharePointViewScope.FILES_ONLY
            };
            var serverFiles = listService.GetListItems(sharePointLibrary.SharePointLibraryListTitle, null, sharePointView);

            // Retrieve list of files which are locally represented and prepare them in a dictionary
            var localFiles = SharePointFileInfoProvider.GetSharePointFiles(sharePointLibrary.SharePointLibraryID);
            Dictionary<string, SharePointFileInfo> localFileRefDictionary = new Dictionary<string, SharePointFileInfo>();
            localFiles.ForEachObject(it => localFileRefDictionary.Add(it.SharePointFileServerRelativeURL, it));

            // Synchronize local and server lists of files
            PerformLibrarySynchronization(sharePointLibrary, fileService, serverFiles, localFileRefDictionary);
        }


        /// <summary>
        /// Uploads a new file to the specified SharePoint library.
        /// The newly uploaded file is immediately available both on the SharePoint server and locally in the library.
        /// </summary>
        /// <param name="sharePointLibrary">SharePointLibraryInfo where to upload the file.</param>
        /// <param name="contentStream">Stream containing binary data of the file.</param>
        /// <param name="libraryRelativeUrl">Relative URL of the file within library (i.e. "myPicture.jpeg" uploads file to the root folder of the library).</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="libraryRelativeUrl"/> already exists.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected virtual void UploadFileInternal(SharePointLibraryInfo sharePointLibrary, SystemIO.Stream contentStream, string libraryRelativeUrl)
        {
            SharePointConnectionInfo connectionInfo = GetSharePointConnectionBySharePointLibrary(sharePointLibrary, "File can not be uploaded.");

            // Prepare needed services - for file upload and synchronization
            var connectionData = connectionInfo.ToSharePointConnectionData();
            ISharePointFileService fileService = SharePointServices.GetService<ISharePointFileService>(connectionData);
            ISharePointListService listService = SharePointServices.GetService<ISharePointListService>(connectionData);

            // Upload the binary content
            string serverRelativeUrl = CreateServerRelativeUrl(connectionInfo.SharePointConnectionSiteUrl, sharePointLibrary.SharePointLibraryListTitle, libraryRelativeUrl);
            fileService.UploadFile(serverRelativeUrl, contentStream, false);

            // Retrieve information about the new file from the SharePoint library
            DataRow fileToBeCreatedDataRow = RetrieveListItemByFileRef(listService, sharePointLibrary.SharePointLibraryListTitle, serverRelativeUrl, new List<string> { "Modified" });

            if (fileToBeCreatedDataRow != null)
            {
                using (CMSTransactionScope transactionScope = new CMSTransactionScope())
                {
                    // Handles situation when a file is deleted from SharePoint, its local copy still exists and file of the same name is uploaded.
                    var oldFile = SharePointFileInfoProvider.GetSharePointFiles(sharePointLibrary.SharePointLibraryID).WhereEquals("SharePointFileServerRelativeURL", serverRelativeUrl).TopN(1).FirstOrDefault();
                    if (oldFile != null)
                    {
                        SharePointFileInfoProvider.DeleteSharePointFileInfo(oldFile);
                    }
                    CreateSharePointFile(sharePointLibrary, fileService, serverRelativeUrl, ValidationHelper.GetDateTime(fileToBeCreatedDataRow["Modified"], DateTimeHelper.ZERO_TIME));
                    transactionScope.Commit();
                }
            }
        }


        /// <summary>
        /// Updates file in SharePoint library with new content and name.
        /// The <paramref name="newFileName"/> can be the same as the old one.
        /// The updated file is immediately available both on the SharePoint server and locally in the library.
        /// </summary>
        /// <param name="sharePointFile">SharePoint file to be updated.</param>
        /// <param name="contentStream">Stream containing the new file's binary content.</param>
        /// <param name="newFileName">File name of the updated file.</param>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="newFileName"/> identifies an already existing file within the library.</exception>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when file identified by <paramref name="sharePointFile"/> does not exist on SharePoint server.</exception>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        /// <remarks>
        /// Trying to update a file which no longer exists on SharePoint server removes the file locally and throws <see cref="SystemIO.FileNotFoundException"/>.
        /// </remarks>
        protected virtual void UpdateFileInternal(SharePointFileInfo sharePointFile, SystemIO.Stream contentStream, string newFileName)
        {
            SharePointLibraryInfo libraryInfo = GetSharePointLibraryBySharePointFile(sharePointFile);

            SharePointConnectionInfo connectionInfo = GetSharePointConnectionBySharePointLibrary(libraryInfo, "File can not be updated.");

            // Prepare needed services - for file update and synchronization
            var connectionData = connectionInfo.ToSharePointConnectionData();
            ISharePointFileService fileService = SharePointServices.GetService<ISharePointFileService>(connectionData);
            ISharePointListService listService = SharePointServices.GetService<ISharePointListService>(connectionData);
            string newServerRelativeUrl = GetNewFileServerRelativeUrl(sharePointFile.SharePointFileServerRelativeURL, newFileName);

            UpdateFileOnSharePointServerOrRemoveFromLibrary(fileService, sharePointFile, contentStream, newServerRelativeUrl);

            // Retrieve information about the updated file from the SharePoint library
            DataRow fileToBeCreatedDataRow = RetrieveListItemByFileRef(listService, libraryInfo.SharePointLibraryListTitle, newServerRelativeUrl, new List<string> { "Modified" });

            if (fileToBeCreatedDataRow != null)
            {
                using (CMSTransactionScope transactionScope = new CMSTransactionScope())
                {
                    if (sharePointFile.SharePointFileServerRelativeURL != newServerRelativeUrl)
                    {
                        // Handles situation when a file is deleted from SharePoint, its local copy still exists and the update (renaming the file) is performed towards the file name of the deleted file
                        var oldFile = SharePointFileInfoProvider.GetSharePointFiles(libraryInfo.SharePointLibraryID).WhereEquals("SharePointFileServerRelativeURL", newServerRelativeUrl).TopN(1).FirstOrDefault();
                        if (oldFile != null)
                        {
                            SharePointFileInfoProvider.DeleteSharePointFileInfo(oldFile);
                        }
                    }
                    UpdateSharePointFile(fileService, sharePointFile, newServerRelativeUrl, ValidationHelper.GetDateTime(fileToBeCreatedDataRow["Modified"], DateTimeHelper.ZERO_TIME));

                    transactionScope.Commit();
                }
            }
        }


        /// <summary>
        /// Deletes the specified file on the SharePoint server and from the local library.
        /// Deleting a non-existent file within SharePoint library (i.e. file is out of sync) deletes the file from the local library as well.
        /// </summary>
        /// <param name="sharePointFile">SharePoint file to be deleted</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected virtual void DeleteFileInternal(SharePointFileInfo sharePointFile)
        {
            SharePointLibraryInfo libraryInfo = GetSharePointLibraryBySharePointFile(sharePointFile);

            SharePointConnectionInfo connectionInfo = GetSharePointConnectionBySharePointLibrary(libraryInfo, "File can not be deleted.");

            // Delete the file on SharePoint
            ISharePointFileService fileService = SharePointServices.GetService<ISharePointFileService>(connectionInfo.ToSharePointConnectionData());
            fileService.DeleteFile(sharePointFile.SharePointFileServerRelativeURL);

            // Delete the file locally
            SharePointFileInfoProvider.DeleteSharePointFileInfo(sharePointFile);
        }


        /// <summary>
        /// Moves the specified file to the recycle bin of the SharePoint server.
        /// Deletes the file from the local library.
        /// Returns the identifier of the new recycle bin item on SharePoint.
        /// Recycling a non-existent file within SharePoint library (i.e. file is out of sync) returns empty GUID and deletes the file from the local library as well.
        /// </summary>
        /// <param name="sharePointFile">SharePoint file to be recycled on the SharePoint server</param>
        /// <returns>GUID of the new recycle bin item on SharePoint, or <see cref="Guid.Empty"/> when file does not exist.</returns>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected virtual Guid RecycleFileInternal(SharePointFileInfo sharePointFile)
        {
            SharePointLibraryInfo libraryInfo = GetSharePointLibraryBySharePointFile(sharePointFile);

            SharePointConnectionInfo connectionInfo = GetSharePointConnectionBySharePointLibrary(libraryInfo, "File can not be recycled.");

            // Recycle the file on SharePoint
            ISharePointFileService fileService = SharePointServices.GetService<ISharePointFileService>(connectionInfo.ToSharePointConnectionData());
            Guid recycleBinItemGuid = fileService.RecycleFile(sharePointFile.SharePointFileServerRelativeURL);

            // Delete the file locally
            SharePointFileInfoProvider.DeleteSharePointFileInfo(sharePointFile);

            return recycleBinItemGuid;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets <see cref="SharePointLibraryInfo"/> for given SharePoint file, or throws exception.
        /// </summary>
        /// <param name="sharePointFile">File for which to search the library.</param>
        /// <returns>Library of the file.</returns>
        /// <exception cref="ArgumentException">When <see cref="SharePointFileInfo.SharePointFileSharePointLibraryID"/> does not specify an existing library.</exception>
        private static SharePointLibraryInfo GetSharePointLibraryBySharePointFile(SharePointFileInfo sharePointFile)
        {
            SharePointLibraryInfo libraryInfo = SharePointLibraryInfoProvider.GetSharePointLibraryInfo(sharePointFile.SharePointFileSharePointLibraryID);
            if (libraryInfo == null)
            {
                throw new ArgumentException(String.Format("The SharePoint file's library ID {0} is invalid.", sharePointFile.SharePointFileSharePointLibraryID), "sharePointFile");
            }

            return libraryInfo;
        }


        /// <summary>
        /// Gets <see cref="SharePointConnectionInfo"/> for given SharePoint library, or throws exception.
        /// Additional error message to the default exception message can be provided.
        /// </summary>
        /// <param name="sharePointLibrary">Library for which to search the connection.</param>
        /// <param name="additionalErrorMessage">Additional error message.</param>
        /// <returns>Connection of the library.</returns>
        /// <exception cref="SharePointConnectionNotFoundException">When library has not valid connection defined (is read-only).</exception>
        private static SharePointConnectionInfo GetSharePointConnectionBySharePointLibrary(SharePointLibraryInfo sharePointLibrary, string additionalErrorMessage = null)
        {
            SharePointConnectionInfo connectionInfo = SharePointConnectionInfoProvider.GetSharePointConnectionInfo(sharePointLibrary.SharePointLibrarySharePointConnectionID);
            if (connectionInfo == null)
            {
                // The library is read-only
                string errorMessage = String.Format("Share point library '{0}' with ID {1} has no connection.", sharePointLibrary.SharePointLibraryName, sharePointLibrary.SharePointLibraryID);
                if (!String.IsNullOrEmpty(additionalErrorMessage))
                {
                    errorMessage += " " + additionalErrorMessage;
                }
                throw new SharePointConnectionNotFoundException(errorMessage);
            }

            return connectionInfo;
        }


        /// <summary>
        /// Performs SharePoint library synchronization.
        /// The columns "FileRef" and "Modified" of the <paramref name="serverFiles"/> data set's table must be set.
        /// </summary>
        /// <param name="sharePointLibrary">SharePointLibraryInfo to which the files belong.</param>
        /// <param name="fileService">SharePoint file service used for server file's content and metadata retrieval.</param>
        /// <param name="serverFiles">DataSet containing server file listing.</param>
        /// <param name="localFileRefDictionary">Dictionary containing mapping of FileRef path to SharePointFileInfo. The dictionary is modified during the process of synchronization and shall not be used afterwards.</param>
        private void PerformLibrarySynchronization(SharePointLibraryInfo sharePointLibrary, ISharePointFileService fileService, DataSet serverFiles, Dictionary<string, SharePointFileInfo> localFileRefDictionary)
        {
            // Keep track of files to be processed
            List<DataRow> filesToBeCreated = new List<DataRow>();
            List<KeyValuePair<SharePointFileInfo, DataRow>> filesToBeUpdated = new List<KeyValuePair<SharePointFileInfo, DataRow>>();

            if (!DataHelper.DataSourceIsEmpty(serverFiles))
            {
                foreach (DataRow serverFile in serverFiles.Tables[0].Rows)
                {
                    var serverFileModified = ValidationHelper.GetDateTime(serverFile["Modified"], DateTimeHelper.ZERO_TIME);
                    string serverFileRef = (string)serverFile["FileRef"];

                    if (localFileRefDictionary.ContainsKey(serverFileRef))
                    {
                        var localFile = localFileRefDictionary[serverFileRef];

                        // Update the local file's representation upon change
                        if (localFile.SharePointFileServerLastModified != serverFileModified)
                        {
                            filesToBeUpdated.Add(new KeyValuePair<SharePointFileInfo, DataRow>(localFile, serverFile));
                        }

                        // Remove the file from the dictionary so that in the end it contains only files for removal
                        localFileRefDictionary.Remove(serverFileRef);
                    }
                    else
                    {
                        // Create the local file's representation if the file is new in the library
                        filesToBeCreated.Add(serverFile);
                    }
                }
            }

            // Perform synchronization changes in the SharePointLibrary
            CreateSharePointFiles(sharePointLibrary, fileService, filesToBeCreated);
            UpdateSharePointFiles(fileService, filesToBeUpdated);
            DeleteSharePointFiles(localFileRefDictionary.Values);
        }


        /// <summary>
        /// Synchronizes properties which are common to <see cref="SharePointFileInfo"/> and <see cref="ISharePointFile"/>.
        /// The <paramref name="sharePointFile"/> is considered to be the source and <paramref name="fileInfo"/> the target.
        /// </summary>
        /// <param name="fileInfo">Local representation of SharePoint file.</param>
        /// <param name="sharePointFile">Server representation of SharePoint file.</param>
        private static void SynchronizeCommonFileProperties(SharePointFileInfo fileInfo, ISharePointFile sharePointFile)
        {
            byte[] binaryContent = sharePointFile.GetContentBytes();
            fileInfo.SharePointFileETag = sharePointFile.ETag;
            fileInfo.SharePointFileExtension = sharePointFile.Extension;
            fileInfo.SharePointFileMimeType = sharePointFile.MimeType;
            fileInfo.SharePointFileName = sharePointFile.Name;
            fileInfo.SharePointFileServerRelativeURL = sharePointFile.ServerRelativeUrl;
            fileInfo.SharePointFileBinary = binaryContent;
            fileInfo.SharePointFileSize = (binaryContent != null) ? binaryContent.Length : 0;
        }


        /// <summary>
        /// Creates local representation of SharePoint files from a list containing DataRow items describing each file.
        /// The fields "FileRef" and "Modified" of each DataRow must be specified.
        /// All necessary data is retrieved from the SharePoint server using <paramref name="fileService"/>.
        /// Since the library content can change during the processing of all files, files which are not found on SharePoint are skipped,
        /// files which are found in local library are not being added again.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library in which to store the files.</param>
        /// <param name="fileService">SharePoint file service used for files retrieval.</param>
        /// <param name="filesToBeCreated">Enumeration of data rows representing files to be added.</param>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        private static void CreateSharePointFiles(SharePointLibraryInfo libraryInfo, ISharePointFileService fileService, IEnumerable<DataRow> filesToBeCreated)
        {
            foreach (DataRow fileToBeCreatedDataRow in filesToBeCreated)
            {
                try
                {
                    CreateSharePointFile(libraryInfo, fileService, (string)fileToBeCreatedDataRow["FileRef"], ValidationHelper.GetDateTime(fileToBeCreatedDataRow["Modified"], DateTimeHelper.ZERO_TIME));
                }
                catch (SystemIO.FileNotFoundException)
                {
                    // The library content can change during the synchronization, skip files which have been removed from the SharePoint
                }
            }
        }


        /// <summary>
        /// Creates local representation of SharePoint file from its server relative URL and last modification time.
        /// Returns true if the file has been created.
        /// Returns false if the file is already present in the library.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library in which to store the file.</param>
        /// <param name="fileService">SharePoint file service used for file retrieval.</param>
        /// <param name="fileServerRelativeUrl">Server relative URL of created file.</param>
        /// <param name="fileServerLastModified">Last modification time of the file - taken from the corresponding list item, not directly from the file's metadata.</param>
        /// <returns>True on successful file creation, false if the file already exists in the library.</returns>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        /// <remarks>
        /// The newly created file does not use the last modification time from the file's metadata, because it can not be retrieved in one query for multiple files.
        /// Therefore future library synchronization would be ineffective.
        /// </remarks>
        private static bool CreateSharePointFile(SharePointLibraryInfo libraryInfo, ISharePointFileService fileService, string fileServerRelativeUrl, DateTime fileServerLastModified)
        {
            SharePointFileInfo localFileInfo = new SharePointFileInfo();

            try
            {
                // Retrieve file's metadata and binary content
                ISharePointFile serverFile = fileService.GetFile(fileServerRelativeUrl);

                // Synchronize the file's properties
                SynchronizeCommonFileProperties(localFileInfo, serverFile);
                localFileInfo.SharePointFileServerLastModified = fileServerLastModified;
                localFileInfo.SharePointFileSharePointLibraryID = libraryInfo.SharePointLibraryID;
                localFileInfo.SharePointFileSiteID = libraryInfo.SharePointLibrarySiteID;

                SharePointFileInfoProvider.SetSharePointFileInfo(localFileInfo);

                return true;
            }
            catch (Exception ex)
            {
                SqlException sqlException = ex.InnerException as SqlException;
                if ((sqlException != null) && (sqlException.Number == DUPLICATE_KEY_ROW_EXCEPTION_ERROR_NUMBER))
                {
                    // File has been already created within the library
                    return false;
                }

                throw;
            }
        }


        /// <summary>
        /// Updates local representation of SharePoint files from a list containing corresponding pairs of SharePointFileInfo and DataRow.
        /// Since the library content can change during the processing of all files, files which are not found on SharePoint are skipped.
        /// </summary>
        /// <param name="fileService">SharePoint file service used for files retrieval.</param>
        /// <param name="filesToBeUpdated">Enumeration of corresponding SharePointFileInfo objects and data rows representing files to be updated.</param>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        private static void UpdateSharePointFiles(ISharePointFileService fileService, IEnumerable<KeyValuePair<SharePointFileInfo, DataRow>> filesToBeUpdated)
        {
            foreach (KeyValuePair<SharePointFileInfo, DataRow> fileToBeUpdated in filesToBeUpdated)
            {
                try
                {
                    SharePointFileInfo localFileInfo = fileToBeUpdated.Key;
                    DataRow serverFileDataRow = fileToBeUpdated.Value;

                    UpdateSharePointFile(fileService, localFileInfo, (string)serverFileDataRow["FileRef"], ValidationHelper.GetDateTime(serverFileDataRow["Modified"], DateTimeHelper.ZERO_TIME));
                }
                catch (SystemIO.FileNotFoundException)
                {
                    // The library content can change during the synchronization, skip files which have been removed from the SharePoint
                }
            }
        }



        /// <summary>
        /// Updates local representation of SharePoint file from a DataRow representing the list item on SharePoint. Can handle rename of the file.
        /// Returns true if the file has been updated.
        /// Returns false if the file name has changed and the file is already present in the library.
        /// </summary>
        /// <param name="fileService">SharePoint file service used for files retrieval.</param>
        /// <param name="localFileInfo">Existing SharePointFileInfo object and data row representing the file.</param>
        /// <param name="fileServerRelativeUrl">Server relative URL of the file after update.</param>
        /// <param name="fileServerLastModified">Last modification time of the file - taken from the corresponding list item, not directly from the file's metadata.</param>
        /// <returns>True on successful update, false if the file is being renamed and the file is already present locally.</returns>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        private static bool UpdateSharePointFile(ISharePointFileService fileService, SharePointFileInfo localFileInfo, string fileServerRelativeUrl, DateTime fileServerLastModified)
        {
            try
            {
                // Retrieve file's metadata and binary content
                ISharePointFile serverFile = fileService.GetFile(fileServerRelativeUrl);

                // Synchronize the file's properties
                SynchronizeCommonFileProperties(localFileInfo, serverFile);
                // Use the modification time of the SharePoint list item - it can be retrieved for all library files in one query
                localFileInfo.SharePointFileServerLastModified = ValidationHelper.GetDateTime(fileServerLastModified, DateTimeHelper.ZERO_TIME);

                SharePointFileInfoProvider.SetSharePointFileInfo(localFileInfo);

                return true;
            }
            catch (Exception ex)
            {
                SqlException sqlException = ex.InnerException as SqlException;
                if ((sqlException != null) && (sqlException.Number == DUPLICATE_KEY_ROW_EXCEPTION_ERROR_NUMBER))
                {
                    // File has been already created within the library (can occur when update is performed when synchronization is running)
                    return false;
                }

                throw;
            }
        }


        /// <summary>
        /// Updates file's content and name on SharePoint server. If the file does not exists within the server (usually caused by library being out of sync),
        /// removes the file from local SharePoint library and throws <see cref="SystemIO.FileNotFoundException"/>.
        /// </summary>
        /// <param name="fileService">File service to be used for accessing SharePoint server.</param>
        /// <param name="sharePointFile">SharePoint file to be updated on the server.</param>
        /// <param name="contentStream">Stream containing the new file's binary content.</param>
        /// <param name="newServerRelativeUrl">Server relative URL of the file after update.</param>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="newServerRelativeUrl"/> identifies an already existing file within the library.</exception>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when file identified by <paramref name="sharePointFile"/> does not exist on SharePoint server.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        private static void UpdateFileOnSharePointServerOrRemoveFromLibrary(ISharePointFileService fileService, SharePointFileInfo sharePointFile, SystemIO.Stream contentStream, string newServerRelativeUrl)
        {
            try
            {
                fileService.UpdateFile(sharePointFile.SharePointFileServerRelativeURL, contentStream, newServerRelativeUrl);
            }
            catch (SystemIO.FileNotFoundException)
            {
                // Occurs when trying to update a file which has been deleted from SharePoint - delete the file locally as well
                SharePointFileInfoProvider.DeleteSharePointFileInfo(sharePointFile);

                throw;
            }
        }


        /// <summary>
        /// Deletes local representation of SharePoint files specified in the <paramref name="filesToBeDeleted"/>.
        /// </summary>
        /// <param name="filesToBeDeleted">Enumeration of files to be deleted.</param>
        private static void DeleteSharePointFiles(IEnumerable<SharePointFileInfo> filesToBeDeleted)
        {
            foreach (SharePointFileInfo fileToBeDeletedInfo in filesToBeDeleted)
            {
                SharePointFileInfoProvider.DeleteSharePointFileInfo(fileToBeDeletedInfo);
            }
        }


        /// <summary>
        /// Creates SharePoint server relative URL from SharePoint site URL, title of the SharePoint library and relative URL within the library.
        /// </summary>
        /// <param name="sharePointSiteUrl">SharePoint site URL</param>
        /// <param name="listTitle">Title of the library or list</param>
        /// <param name="libraryRelativeUrl">Relative URL within the library or list (optionally containing a leading slash)</param>
        /// <returns>SharePoint server relative URL</returns>
        private static string CreateServerRelativeUrl(string sharePointSiteUrl, string listTitle, string libraryRelativeUrl)
        {
            Uri serverUri = new Uri(sharePointSiteUrl);
            StringBuilder serverRelativeUrl = new StringBuilder();
            serverRelativeUrl.Append(serverUri.AbsolutePath.TrimEnd('/')).Append("/").Append(listTitle).Append("/").Append(libraryRelativeUrl.TrimStart('/'));

            return serverRelativeUrl.ToString();
        }


        /// <summary>
        /// Gets new file server relative URL based on server relative URL of an old file and a new file name.
        /// </summary>
        /// <param name="oldServerRelativeUrl">Original server relative path identifying an old file.</param>
        /// <param name="newFileName">File name of the new file.</param>
        /// <returns>Server relative path to the new file.</returns>
        private static string GetNewFileServerRelativeUrl(string oldServerRelativeUrl, string newFileName)
        {
            int lastSlashIndex = oldServerRelativeUrl.LastIndexOf('/');
            if (lastSlashIndex == -1)
            {
                // URL contained only the file name
                return newFileName;
            }

            return oldServerRelativeUrl.Substring(0, lastSlashIndex + 1) + newFileName;
        }


        /// <summary>
        /// Retrieves list item as a DataRow from SharePoint list or library.
        /// </summary>
        /// <param name="listService">List service to be used for SharePoint server querying.</param>
        /// <param name="listTitle">Title of the list or library to retrieve the item from.</param>
        /// <param name="serverRelativeUrl">Server relative URL (FileRef) of the retrieved item.</param>
        /// <param name="viewFields">Fields to be retrieved.</param>
        /// <returns>DataRow containing file whose FileRef column equals to <paramref name="serverRelativeUrl"/>, or null if none found.</returns>
        private DataRow RetrieveListItemByFileRef(ISharePointListService listService, string listTitle, string serverRelativeUrl, List<string> viewFields)
        {
            SharePointView sharePointView = new SharePointView
            {
                ViewFields = viewFields,
                Scope = SharePointViewScope.FILES_ONLY
            };
            SharePointListItemsSelection listItemsSelection = new SharePointListItemsSelection("FileRef", "Text", serverRelativeUrl);
            DataSet listItems = listService.GetListItems(listTitle, null, sharePointView, listItemsSelection);
            if (DataHelper.DataSourceIsEmpty(listItems))
            {
                return null;
            }

            return listItems.Tables[0].Rows[0];
        }

        #endregion
    }
}

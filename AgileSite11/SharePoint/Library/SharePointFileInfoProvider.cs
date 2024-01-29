using System;
using System.Linq;
using SystemIO = System.IO;

using CMS.DataEngine;

namespace CMS.SharePoint
{
    /// <summary>
    /// Class providing SharePointFileInfo management.
    /// </summary>
    public class SharePointFileInfoProvider : AbstractInfoProvider<SharePointFileInfo, SharePointFileInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public SharePointFileInfoProvider()
            : base(SharePointFileInfo.TYPEINFO)
        {
        }

        #endregion


        #region "Public methods - Basic"

        /// <summary>
        /// Returns a query for all the SharePointFileInfo objects.
        /// </summary>
        /// <param name="getBinary">If false, no binary data is returned for the SharePoint file.</param>
        public static ObjectQuery<SharePointFileInfo> GetSharePointFiles(bool getBinary = false)
        {
            return ProviderObject.GetSharePointFilesInternal(getBinary);
        }


        /// <summary>
        /// Returns SharePointFileInfo with specified ID.
        /// </summary>
        /// <param name="id">SharePointFileInfo ID</param>
        /// <param name="getBinary">If false, no binary data is returned for the SharePoint file.</param>
        public static SharePointFileInfo GetSharePointFileInfo(int id, bool getBinary = false)
        {
            return ProviderObject.GetSharePointFileInfoInternal(id, getBinary);
        }


        /// <summary>
        /// Returns SharePointFileInfo with specified GUID.
        /// </summary>
        /// <param name="guid">SharePointFileInfo GUID</param>      
        /// <param name="getBinary">If false, no binary data is returned for the SharePoint file.</param>          
        public static SharePointFileInfo GetSharePointFileInfo(Guid guid, bool getBinary = false)
        {
            return ProviderObject.GetSharePointFileInfoInternal(guid, getBinary);
        }


        /// <summary>
        /// Sets (updates or inserts) specified SharePointFileInfo.
        /// </summary>
        /// <param name="infoObj">SharePointFileInfo to be set</param>
        public static void SetSharePointFileInfo(SharePointFileInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified SharePointFileInfo.
        /// </summary>
        /// <param name="infoObj">SharePointFileInfo to be deleted</param>
        public static void DeleteSharePointFileInfo(SharePointFileInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes SharePointFileInfo with specified ID.
        /// </summary>
        /// <param name="id">SharePointFileInfo ID</param>
        public static void DeleteSharePointFileInfo(int id)
        {
            SharePointFileInfo infoObj = GetSharePointFileInfo(id);
            DeleteSharePointFileInfo(infoObj);
        }

        #endregion


        #region "Public methods - Advanced"


        /// <summary>
        /// Returns a query for all the SharePointFileInfo objects of a specified library.
        /// </summary>
        /// <param name="libraryId">Library ID</param>
        /// <param name="getBinary">If false, no binary data is returned for the SharePoint file.</param>
        public static ObjectQuery<SharePointFileInfo> GetSharePointFiles(int libraryId, bool getBinary = false)
        {
            return ProviderObject.GetSharePointFilesInternal(libraryId);
        }


        /// <summary>
        /// Uploads a new file to the specified SharePoint library.
        /// The newly uploaded file is immediately available both on the SharePoint server and locally in the library.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library to which the file is uploaded.</param>
        /// <param name="contentStream">Stream containing binary data of the file.</param>
        /// <param name="libraryRelativeUrl">Relative URL of the file within library (i.e. "myPicture.jpeg" uploads file to the root folder of the library).</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined (is read-only).</exception>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="libraryRelativeUrl"/> already exists.</exception>
        public static void UploadFile(SharePointLibraryInfo libraryInfo, SystemIO.Stream contentStream, string libraryRelativeUrl)
        {
            ProviderObject.UploadFileInternal(libraryInfo, contentStream, libraryRelativeUrl);
        }


        /// <summary>
        /// Updates file in SharePoint library with new content and name.
        /// The <paramref name="newFileName"/> can be the same as the old one.
        /// The updated file is immediately available both on the SharePoint server and locally in the library.
        /// </summary>
        /// <param name="fileInfo">SharePoint file to be updated.</param>
        /// <param name="contentStream">Stream containing the new file's binary content.</param>
        /// <param name="newFileName">File name of the updated file.</param>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="newFileName"/> identifies an already existing file within the library.</exception>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when file identified by <paramref name="fileInfo"/> does not exist on SharePoint server.</exception>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public static void UpdateFile(SharePointFileInfo fileInfo, SystemIO.Stream contentStream, string newFileName)
        {
            ProviderObject.UpdateFileInternal(fileInfo, contentStream, newFileName);
        }


        /// <summary>
        /// Deletes the specified file on the SharePoint server and from the local library.
        /// Deleting a non-existent file within SharePoint library (i.e. file is out of sync) deletes the file from the local library as well.
        /// </summary>
        /// <param name="fileInfo">SharePoint file to be deleted</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public static void DeleteFile(SharePointFileInfo fileInfo)
        {
            ProviderObject.DeleteFileInternal(fileInfo);
        }


        /// <summary>
        /// Moves the specified file to the recycle bin of the SharePoint server.
        /// Deletes the file from the local library.
        /// Returns the identifier of the new recycle bin item on SharePoint.
        /// Recycling a non-existent file within SharePoint library (i.e. file is out of sync) returns empty GUID and deletes the file from the local library as well.
        /// </summary>
        /// <param name="fileInfo">SharePoint file to be recycled on the SharePoint server</param>
        /// <returns>GUID of the new recycle bin item on SharePoint, or <see cref="Guid.Empty"/> when file does not exist.</returns>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public static Guid RecycleFile(SharePointFileInfo fileInfo)
        {
            return ProviderObject.RecycleFileInternal(fileInfo);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns a query for all the SharePointFileInfo objects.
        /// </summary>
        /// <param name="getBinary">If false, no binary data is returned for the SharePoint file.</param>
        protected virtual ObjectQuery<SharePointFileInfo> GetSharePointFilesInternal(bool getBinary = false)
        {
            return GetObjectQuery().BinaryData(getBinary);
        }


        /// <summary>
        /// Returns SharePointFileInfo with specified ID.
        /// </summary>
        /// <param name="id">SharePointFileInfo ID</param> 
        /// <param name="getBinary">If false, no binary data is returned for the SharePoint file.</param>       
        protected virtual SharePointFileInfo GetSharePointFileInfoInternal(int id, bool getBinary = false)
        {
            return GetObjectQuery().WithID(id).BinaryData(getBinary).TopN(1).FirstOrDefault();
        }


        /// <summary>
        /// Returns SharePointFileInfo with specified GUID.
        /// </summary>
        /// <param name="guid">SharePointFileInfo GUID</param>
        /// <param name="getBinary">If false, no binary data is returned for the SharePoint file.</param>
        protected virtual SharePointFileInfo GetSharePointFileInfoInternal(Guid guid, bool getBinary = false)
        {
            return GetObjectQuery().WithGuid(guid).BinaryData(getBinary).TopN(1).FirstOrDefault();
        }

        #endregion


        #region "Internal methods - Advanced"


        /// <summary>
        /// Returns a query for all the SharePointFileInfo objects of a specified library.
        /// </summary>
        /// <param name="libraryId">Library ID</param>
        /// <param name="getBinary">If false, no binary data is returned for the SharePoint file.</param>
        protected virtual ObjectQuery<SharePointFileInfo> GetSharePointFilesInternal(int libraryId, bool getBinary = false)
        {
            return GetSharePointFilesInternal(getBinary).WhereEquals("SharePointFileSharePointLibraryID", libraryId);
        }


        /// <summary>
        /// Uploads a new file to the specified SharePoint library.
        /// The newly uploaded file is immediately available both on the SharePoint server and locally in the library.
        /// </summary>
        /// <param name="libraryInfo">SharePoint library to which the file is uploaded.</param>
        /// <param name="contentStream">Stream containing binary data of the file.</param>
        /// <param name="libraryRelativeUrl">Relative URL of the file within library (i.e. "myPicture.jpeg" uploads file to the root folder of the library).</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined (is read-only).</exception>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="libraryRelativeUrl"/> already exists.</exception>
        protected virtual void UploadFileInternal(SharePointLibraryInfo libraryInfo, SystemIO.Stream contentStream, string libraryRelativeUrl)
        {
            SharePointLibraryHelper.UploadFile(libraryInfo, contentStream, libraryRelativeUrl);
        }


        /// <summary>
        /// Updates file in SharePoint library with new content and name.
        /// The <paramref name="newFileName"/> can be the same as the old one.
        /// The updated file is immediately available both on the SharePoint server and locally in the library.
        /// </summary>
        /// <param name="fileInfo">SharePoint file to be updated.</param>
        /// <param name="contentStream">Stream containing the new file's binary content.</param>
        /// <param name="newFileName">File name of the updated file.</param>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="newFileName"/> identifies an already existing file within the library.</exception>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when file identified by <paramref name="fileInfo"/> does not exist on SharePoint server.</exception>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected virtual void UpdateFileInternal(SharePointFileInfo fileInfo, SystemIO.Stream contentStream, string newFileName)
        {
            SharePointLibraryHelper.UpdateFile(fileInfo, contentStream, newFileName);
        }


        /// <summary>
        /// Deletes the specified file on the SharePoint server and from the local library.
        /// Deleting a non-existent file within SharePoint library (i.e. file is out of sync) deletes the file from the local library as well.
        /// </summary>
        /// <param name="fileInfo">SharePoint file to be deleted</param>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected virtual void DeleteFileInternal(SharePointFileInfo fileInfo)
        {
            SharePointLibraryHelper.DeleteFile(fileInfo);
        }


        /// <summary>
        /// Moves the specified file to the recycle bin of the SharePoint server.
        /// Deletes the file from the local library.
        /// Returns the identifier of the new recycle bin item on SharePoint.
        /// Recycling a non-existent file within SharePoint library (i.e. file is out of sync) returns empty GUID and deletes the file from the local library as well.
        /// </summary>
        /// <param name="fileInfo">SharePoint file to be recycled on the SharePoint server</param>
        /// <returns>GUID of the new recycle bin item on SharePoint, or <see cref="Guid.Empty"/> when file does not exist.</returns>
        /// <exception cref="SharePointConnectionNotFoundException">Thrown when the SharePoint library has no valid connection defined.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected virtual Guid RecycleFileInternal(SharePointFileInfo fileInfo)
        {
            return SharePointLibraryHelper.RecycleFile(fileInfo);
        }

        #endregion
    }
}
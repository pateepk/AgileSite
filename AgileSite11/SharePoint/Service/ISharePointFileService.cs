using System;
using System.Linq;
using System.Net;
using System.Text;

using SystemIO = System.IO;

namespace CMS.SharePoint
{
    /// <summary>
    /// Provides access to SharePoint files.
    /// </summary>
    public interface ISharePointFileService : ISharePointService
    {
        /// <summary>
        /// Gets SharePoint file identified by server relative URL.
        /// The file may be loaded eagerly or lazily (depends on implementation). In such case an invalid serverRelativeUrl is not reported immediately.
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL</param>
        /// <returns>SharePoint file, or null</returns>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server (and eager loading is used).</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        ISharePointFile GetFile(string serverRelativeUrl);


        /// <summary>
        /// Uploads the file's content specified as a stream to the <paramref name="serverRelativeUrl"/> location.
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL of the file.</param>
        /// <param name="stream">Stream with the file content.</param>
        /// <param name="overwriteExisting">If true, an existing file will be overwritten when it already exists.</param>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when trying to upload to <paramref name="serverRelativeUrl"/> location which already exists and <paramref name="overwriteExisting"/> is set to false.</exception>
        /// <exception cref="ArgumentException">Thrown when trying to upload to <paramref name="serverRelativeUrl"/> location which already exists, <paramref name="overwriteExisting"/> is set to true, but the location can not be overwritten.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serverRelativeUrl"/> is null.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="serverRelativeUrl"/> is in invalid format.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        void UploadFile(string serverRelativeUrl, SystemIO.Stream stream, bool overwriteExisting);


        /// <summary>
        /// Deletes the file specified by <paramref name="serverRelativeUrl"/> from the SharePoint server.
        /// Deleting a non-existent file within SharePoint site is a void action.
        /// Throws exception when <paramref name="serverRelativeUrl"/> is invalid (does not point within any SharePoint site).
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL of the file.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serverRelativeUrl"/> does not point within any SharePoint library.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serverRelativeUrl"/> is null.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        void DeleteFile(string serverRelativeUrl);


        /// <summary>
        /// Moves the file specified by <paramref name="serverRelativeUrl"/> to the recycle bin of the SharePoint server.
        /// Returns the identifier of the new recycle bin item.
        /// Recycling a non-existent file within SharePoint site returns empty GUID.
        /// Throws exception when <paramref name="serverRelativeUrl"/> is invalid (does not point within any SharePoint site).
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL of the file.</param>
        /// <returns>GUID of the new recycle bin item, or <see cref="Guid.Empty"/> when file does not exist.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serverRelativeUrl"/> does not point within any SharePoint library.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serverRelativeUrl"/> is null.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        Guid RecycleFile(string serverRelativeUrl);


        /// <summary>
        /// Updates file identified by <paramref name="serverRelativeUrl"/>. New content and file name can be provided.
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL of the file being updated.</param>
        /// <param name="stream">Stream providing file's binary content.</param>
        /// <param name="newServerRelativeUrl">New server relative URL of the file.</param>
        /// <exception cref="SharePointFileAlreadyExistsException">Thrown when <paramref name="newServerRelativeUrl"/> identifies existing file.</exception>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when file identified by <paramref name="serverRelativeUrl"/> does not exist on SharePoint server.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        void UpdateFile(string serverRelativeUrl, SystemIO.Stream stream, string newServerRelativeUrl);
    }
}

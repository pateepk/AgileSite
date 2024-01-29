using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using CMS.Base;

using Microsoft.SharePoint.Client;

using SystemIO = System.IO;


namespace CMS.SharePoint
{
    /// <summary>
    /// Provides common methods suitable for SharePoint services handling files.
    /// </summary>
    internal abstract class SharePointAbstractFileService : SharePointAbstractService
    {
        #region "Constants"

        /// <summary>
        /// Message of <see cref="ClientRequestException"/> thrown when saving a file that already exists (and overwrite is set to false).
        /// It does not seem like there is any publicly available error code associated with such error condition.
        /// </summary>
        private const string FILE_ALREADY_EXISTS_SHAREPOINT_EXCEPTION_MESSAGE = "The file already exists.";


        /// <summary>
        /// Error code of <see cref="ServerException"/> thrown when trying to move file to location, which already exists.
        /// </summary>
        private const int FILE_ALREADY_EXISTS_SERVER_ERROR_CODE = -2130575257;


        /// <summary>
        /// Error code of <see cref="ServerException"/> thrown when trying to move file which does not exist.
        /// </summary>
        private const int NO_SUCH_FILE_OR_FOLDER_SERVER_ERROR_CODE = -2130575338;

        #endregion


        #region "ISharePointFileService methods - partial implementation"

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
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public void UploadFile(string serverRelativeUrl, SystemIO.Stream stream, bool overwriteExisting)
        {
            try
            {
                ClientContext context = CreateClientContext();
                Microsoft.SharePoint.Client.File.SaveBinaryDirect(context, serverRelativeUrl, stream, overwriteExisting);
            }
            catch (ClientRequestException ex)
            {
                if (ex.Message.EqualsCSafe(FILE_ALREADY_EXISTS_SHAREPOINT_EXCEPTION_MESSAGE, true))
                {
                    throw new SharePointFileAlreadyExistsException(ex.Message, ex);
                }

                throw;
            }
            catch (WebException ex)
            {
                var response = ex.Response as HttpWebResponse;
                if ((ex.Status == WebExceptionStatus.ProtocolError) && (response != null) && (response.StatusCode == HttpStatusCode.InternalServerError))
                {
                    // Occurs when serverRelativeUrl denotes a location that is not valid for writing (i.e. site root folder)
                    throw new ArgumentException("The specified server relative URL '" + serverRelativeUrl + "' is not valid for writing the file.", "serverRelativeUrl");
                }

                throw;
            }
            catch (ArgumentNullException ex)
            {
                if (ex.ParamName.EqualsCSafe("serverRelativeUrl", true))
                {
                    // Occurs when the serverRelativeUrl is null
                    throw new ArgumentNullException(String.Format("The server relative URL '{0}' is not valid.", serverRelativeUrl), ex);
                }

                throw;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                if (ex.ParamName.EqualsCSafe("serverRelativeUrl", true))
                {
                    // Occurs when the serverRelativeUrl is empty string or does not start with a slash
                    throw new ArgumentOutOfRangeException(String.Format("The server relative URL '{0}' is not valid.", serverRelativeUrl), "serverRelativeUrl");
                }

                throw;
            }
        }


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
        public void UpdateFile(string serverRelativeUrl, SystemIO.Stream stream, string newServerRelativeUrl)
        {
            ClientContext context = CreateClientContext();
            Microsoft.SharePoint.Client.File file = context.Web.GetFileByServerRelativeUrl(serverRelativeUrl);
            file.MoveTo(newServerRelativeUrl, MoveOperations.None);

            FileSaveBinaryInformation fileSaveBinaryInformation = new FileSaveBinaryInformation();
            if (ConnectionData.SharePointConnectionSharePointVersion == SharePointVersion.SHAREPOINT_2010)
            {
                // SharePoint 2010 does not support the ContentStream property
                fileSaveBinaryInformation.Content = ReadStreamToBuffer(stream);
            }
            else
            {
                fileSaveBinaryInformation.ContentStream = stream;
            }
            file.SaveBinary(fileSaveBinaryInformation);

            try
            {
                ExecuteQuery(context);
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorCode == FILE_ALREADY_EXISTS_SERVER_ERROR_CODE)
                {
                    throw new SharePointFileAlreadyExistsException(String.Format("A file of name '{0}' already exists within library.", newServerRelativeUrl), ex);
                }
                if (ex.ServerErrorCode == NO_SUCH_FILE_OR_FOLDER_SERVER_ERROR_CODE)
                {
                    throw new SystemIO.FileNotFoundException(ex.Message, ex);
                }

                throw new SharePointServerException(ex.Message, ex);
            }
        }


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
        public void DeleteFile(string serverRelativeUrl)
        {
            ClientContext context = CreateClientContext();
            Microsoft.SharePoint.Client.File file = context.Web.GetFileByServerRelativeUrl(serverRelativeUrl);
            file.DeleteObject();
            ExecuteFileQuery(context, serverRelativeUrl);
        }


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
        public Guid RecycleFile(string serverRelativeUrl)
        {
            ClientContext context = CreateClientContext();
            Microsoft.SharePoint.Client.File file = context.Web.GetFileByServerRelativeUrl(serverRelativeUrl);
            ClientResult<Guid> recycleItemIdentifier = file.Recycle();
            ExecuteFileQuery(context, serverRelativeUrl);

            return recycleItemIdentifier.Value;
        }


        /// <summary>
        /// Executes query specified by <paramref name="context"/> while taking proper care of some typical file related SharePoint exceptions.
        /// </summary>
        /// <param name="context">Context on which to execute the query</param>
        /// <param name="serverRelativeUrl">Server relative URL of the file manipulated by the query.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="serverRelativeUrl"/> does not point within any SharePoint library.</exception>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="serverRelativeUrl"/> is null.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        private void ExecuteFileQuery(ClientContext context, string serverRelativeUrl)
        {
            try
            {
                ExecuteQuery(context);
            }
            catch (ServerException ex)
            {
                if (ex.ServerErrorTypeName.EqualsCSafe("System.ArgumentException"))
                {
                    // Occurs when the serverRelativeUrl does not identify any SharePoint site or when it does not even start with a slash
                    throw new ArgumentException(String.Format("The server relative URL '{0}' is not valid.", serverRelativeUrl), "serverRelativeUrl", ex);
                }

                throw new SharePointServerException(ex.Message, ex);
            }
            catch (ArgumentNullException ex)
            {
                if (ex.ParamName.EqualsCSafe("serverRelativeUrl", true))
                {
                    // Occurs when the serverRelativeUrl is null
                    throw new ArgumentNullException(String.Format("The server relative URL '{0}' is not valid.", serverRelativeUrl), ex);
                }

                throw;
            }
            catch (ArgumentException ex)
            {
                if (ex.ParamName.EqualsCSafe("serverRelativeUrl", true))
                {
                    // Occurs when the serverRelativeUrl is empty string
                    throw new ArgumentException(String.Format("The server relative URL '{0}' is not valid.", serverRelativeUrl), "serverRelativeUrl", ex);
                }

                throw;
            }
        }


        /// <summary>
        /// Reads the <paramref name="stream"/> to the end and returns all bytes read.
        /// </summary>
        /// <param name="stream">Stream to be read.</param>
        /// <returns>Bytes obtained from the stream.</returns>
        private byte[] ReadStreamToBuffer(SystemIO.Stream stream)
        {
            int bytesRead;
            const int bufferSize = 4096;
            byte[] buffer = new byte[bufferSize];
            List<byte> fileContent = new List<byte>();

            while ((bytesRead = stream.Read(buffer, 0, bufferSize)) > 0)
            {
                fileContent.AddRange(buffer);
                if (bytesRead != bufferSize)
                {
                    // Truncate bytes if block does not equal to buffer size.
                    int spareBytes = bufferSize - bytesRead;
                    fileContent.RemoveRange(fileContent.Count - spareBytes, spareBytes);
                }
            }

            return fileContent.ToArray();
        }


        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes SharePoint connection for any service built on this abstract service.
        /// </summary>
        /// <param name="connectionData">Connection data</param>
        protected SharePointAbstractFileService(SharePointConnectionData connectionData)
            : base(connectionData)
        {
            
        }

        #endregion
    }
}

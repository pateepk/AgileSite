using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

using CMS.Base;
using CMS.Helpers;
using CMS.IO;

using Microsoft.SharePoint.Client;

using File = Microsoft.SharePoint.Client.File;
using SystemIO = System.IO;


namespace CMS.SharePoint
{
    /// <summary>
    /// Implementation of SharePoint file for version 2010.
    /// The file or its properties are loaded lazily.
    /// </summary>
    internal class SharePoint2010File : ISharePointFile
    {
        #region "Fields"

        /// <summary>
        /// SharePoint service instance this instance came from.
        /// </summary>
        protected readonly SharePointAbstractService mFileService;


        /// <summary>
        /// Indicates whether metadata has been loaded from SharePoint server.
        /// </summary>
        protected bool metadataLoaded;
        protected readonly object mMetadataLock = new object();

        protected byte[] mContent;
        protected readonly object mContentLock = new object();

        protected string mServerRelativeUrl;
        protected string mETag;
        protected string mExtension;
        protected string mMimeType;
        protected string mName;
        protected DateTime? mTimeCreated;
        protected DateTime? mTimeLastModified;
        protected string mTitle;

        #endregion


        #region "Properties"

        /// <summary>
        /// File's ETag (entity tag) from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public string ETag
        {
            get
            {
                EnsureMetadata();

                return mETag;
            }
        }


        /// <summary>
        /// File's extension.
        /// Determined by <see cref="ServerRelativeUrl"/>.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        public string Extension
        {
            get
            {
                if ((mExtension == null) && (ServerRelativeUrl) != null)
                {
                    mExtension = Path.GetExtension(ServerRelativeUrl);
                }

                return mExtension;
            }
        }


        /// <summary>
        /// File's MIME type.
        /// Determined by <see cref="ServerRelativeUrl"/>.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        public string MimeType
        {
            get
            {
                if ((mMimeType == null) && (Extension != null))
                {
                    mMimeType = MimeTypeHelper.GetMimetype(Extension);
                }

                return mMimeType;
            }
        }


        /// <summary>
        /// Length of the file's binary content.
        /// The SharePoint 2010 does not support this property.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown always.</exception>
        /// <seealso cref="IsLengthSupported"/>
        public long Length
        {
            get
            {
                throw new NotSupportedException("[SharePoint2010File.Length._get]: Could not get the property value of Length. The property is not supported in version 2010.");
            }
        }


        /// <summary>
        /// Tells you whether <see cref="Length"/> property is supported.
        /// The support is SharePoint version dependent.
        /// </summary>
        public bool IsLengthSupported
        {
            get
            {
                return false;
            }
        }


        /// <summary>
        /// File's name from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public string Name
        {
            get
            {
                EnsureMetadata();

                return mName;
            }
        }


        /// <summary>
        /// Gets SharePoint server relative URL this instance refers to.
        /// </summary>
        public string ServerRelativeUrl
        {
            get
            {
                return mServerRelativeUrl;
            }
            set
            {
                mServerRelativeUrl = value;
            }
        }


        /// <summary>
        /// File's creation time from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public DateTime? TimeCreated
        {
            get
            {
                EnsureMetadata();

                return mTimeCreated;
            }
        }


        /// <summary>
        /// File's last modification time from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public DateTime? TimeLastModified
        {
            get
            {
                EnsureMetadata();

                return mTimeLastModified;
            }
        }


        /// <summary>
        /// File's title from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public string Title
        {
            get
            {
                EnsureMetadata();

                return mTitle;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the file's content as an array of bytes.
        /// </summary>
        /// <returns>Array of bytes containing SharePoint file's binary data, or null if ServerRelativeUrl is null.</returns>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public byte[] GetContentBytes()
        {
            if (mContent != null)
            {
                return mContent;
            }

            if (ServerRelativeUrl == null)
            {
                return null;
            }

            lock (mContentLock)
            {
                if (mContent != null)
                {
                    return mContent;
                }

                SystemIO.Stream stream = null;
                try
                {
                    stream = GetContentStream();

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

                    return mContent = fileContent.ToArray();
                }
                finally
                {
                    if (stream != null)
                    {
                        stream.Close();
                    }
                }
            }

        }


        /// <summary>
        /// Gets the file's content as a stream.
        /// </summary>
        /// <returns>Stream providing SharePoint file's binary data, or null if ServerRelativeUrl is null.</returns>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public SystemIO.Stream GetContentStream()
        {
            if (ServerRelativeUrl == null)
            {
                return null;
            }

            var context = mFileService.CreateClientContext();
            try
            {
                FileInformation fi = File.OpenBinaryDirect(context, ServerRelativeUrl);

                return fi.Stream;
            }
            catch (ServerException ex)
            {
                throw new SharePointServerException(ex.Message, ex);
            }
            catch (WebException ex)
            {
                if ((ex.Response != null) && ((HttpWebResponse)ex.Response).StatusCode == HttpStatusCode.NotFound)
                {
                    throw new SystemIO.FileNotFoundException(String.Format("The requested SharePoint file '{0}' was not found.", ServerRelativeUrl), ex);
                }

                throw;
            }
        }


        /// <summary>
        /// Ensures file's metadata are loaded from SharePoint server.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected virtual void EnsureMetadata()
        {
            if (metadataLoaded)
            {
                return;
            }

            lock (mMetadataLock)
            {
                if (metadataLoaded)
                {
                    return;
                }
            
                if (ServerRelativeUrl == null)
                {
                    return;
                }


                try
                {
                    ClientContext context = mFileService.CreateClientContext();
                    File file = context.Web.GetFileByServerRelativeUrl(ServerRelativeUrl);
                    context.Load(file);
                    mFileService.ExecuteQuery(context);

                    mETag = file.ETag;
                    mName = file.Name;
                    mTimeCreated = file.TimeCreated;
                    mTimeLastModified = file.TimeLastModified;
                    mTitle = file.Title;
                }
                catch (ServerException ex)
                {
                    if (ex.ServerErrorTypeName.EqualsCSafe("System.IO.FileNotFoundException") || ex.ServerErrorTypeName.EqualsCSafe("System.IO.DirectoryNotFoundException") || ex.ServerErrorTypeName.EqualsCSafe("System.ArgumentException"))
                    {
                        // Both error type names are caused by invalid server relative URL
                        throw new SystemIO.FileNotFoundException(String.Format("The requested SharePoint file '{0}' was not found.", ServerRelativeUrl), ex);
                    }

                    throw new SharePointServerException(ex.Message, ex);
                }
                finally
                {
                    metadataLoaded = true;
                }
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates a new SharePoint2010File instance linked to originating SharePoint2010FileService.
        /// </summary>
        /// <param name="fileService">SharePoint service this instance came from.</param>
        /// <param name="serverRelativeUrl">Server relative URL of the file.</param>
        internal SharePoint2010File(SharePointAbstractService fileService, string serverRelativeUrl)
        {
            mFileService = fileService;
            ServerRelativeUrl = serverRelativeUrl;
        }

        #endregion
    }
}
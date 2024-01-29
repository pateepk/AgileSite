using System;
using System.Linq;
using System.Net;
using System.Text;

using CMS.Base;

using Microsoft.SharePoint.Client;

using IOExceptions = System.IO;


namespace CMS.SharePoint
{
    /// <summary>
    /// Implementation of SharePoint file for version 2013.
    /// The file or its properties are loaded lazily.
    /// </summary>
    /// <remarks>The inheritance of SharePoint2010File class is purely for the reason of code reuse and must not be relied on. The only guarantee is the implementation of ISharePointFile.</remarks>
    internal class SharePoint2013File : SharePoint2010File, ISharePointFile
    {
        #region "Fields"

        protected long mLength;

        #endregion


        #region "Properties"

        /// <summary>
        /// Length of the file's binary content.
        /// </summary>
        /// <exception cref="IOExceptions.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        /// <seealso cref="IsLengthSupported"/>
        public new long Length
        {
            get
            {
                EnsureMetadata();

                return mLength;
            }
        }


        /// <summary>
        /// Tells you whether <see cref="Length"/> property is supported.
        /// The support is SharePoint version dependent.
        /// </summary>
        public new bool IsLengthSupported
        {
            get
            {
                return true;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures file's metadata are loaded from SharePoint server.
        /// </summary>
        /// <exception cref="IOExceptions.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        protected override void EnsureMetadata()
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
                    mLength = file.Length;
                    mName = file.Name;
                    mTimeCreated = file.TimeCreated;
                    mTimeLastModified = file.TimeLastModified;
                    mTitle = file.Title;
                }
                catch (ServerException ex)
                {
                    if (ex.ServerErrorTypeName.EqualsCSafe("System.IO.FileNotFoundException") || ex.ServerErrorTypeName.EqualsCSafe("System.ArgumentException"))
                    {
                        // Both error type names are caused by invalid server relative URL
                        throw new IOExceptions.FileNotFoundException(String.Format("The requested SharePoint file '{0}' was not found.", ServerRelativeUrl), ex);
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
        /// Creates a new SharePoint2013File instance linked to originating SharePoint2013FileService.
        /// </summary>
        /// <param name="fileService">File service this instance came from.</param>
        /// <param name="serverRelativeUrl">Server relative URL of the file.</param>
        internal SharePoint2013File(SharePoint2013FileService fileService, string serverRelativeUrl)
            : base(fileService, serverRelativeUrl)
        {

        }

        #endregion
    }
}

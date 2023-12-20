using System;
using System.Linq;
using System.Net;
using System.Text;

using IOExceptions = System.IO;


namespace CMS.SharePoint
{
    /// <summary>
    /// Provides access to SharePoint files.
    /// </summary>
    internal class SharePoint2010FileService : SharePointAbstractFileService, ISharePointFileService
    {
        /// <summary>
        /// Creates a new SharePoint 2010 ISharePointFileService 
        /// </summary>
        /// <param name="connectionData">Connection data</param>
        public SharePoint2010FileService(SharePointConnectionData connectionData)
            : base(connectionData)
        {

        }


        /// <summary>
        /// Gets SharePoint file identified by server relative URL.
        /// The file may be loaded lazily. In such case an invalid serverRelativeUrl is not reported immediately.
        /// </summary>
        /// <param name="serverRelativeUrl">Server relative URL</param>
        /// <returns>SharePoint file, or null</returns>
        /// <exception cref="IOExceptions.FileNotFoundException">Thrown when requested file is not present at SharePoint server (and eager loading is used).</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        public ISharePointFile GetFile(string serverRelativeUrl)
        {
            SharePoint2010File file = new SharePoint2010File(this, serverRelativeUrl);

            return file;
        }
    }
}

using System;
using System.Linq;
using System.Net;
using System.Text;

namespace CMS.SharePoint
{
    /// <summary>
    /// Provides access to information about SharePoint site.
    /// </summary>
    public interface ISharePointSiteService : ISharePointService
    {
        /// <summary>
        /// Gets SharePoint site URL.
        /// </summary>
        /// <returns>SharePoint site URL.</returns>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        string GetSiteUrl();

    }
}

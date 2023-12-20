using System;
using System.Net;

using SystemIO = System.IO;

namespace CMS.SharePoint
{
    /// <summary>
    /// Represents version independent SharePoint file.
    /// The file or its properties may be loaded eagerly or lazily (depends on implementation).
    /// </summary>
    public interface ISharePointFile
    {
        /// <summary>
        /// File's ETag (entity tag) from SharePoint server.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        string ETag
        {
            get;
        }


        /// <summary>
        /// File's extension.
        /// Determined by <see cref="ServerRelativeUrl"/>.
        /// </summary>
        string Extension
        {
            get;
        }


        /// <summary>
        /// File's MIME type.
        /// Determined by <see cref="ServerRelativeUrl"/>.
        /// </summary>
        string MimeType
        {
            get;
        }


        /// <summary>
        /// Length of the file's binary content.
        /// May not be supported by all SharePoint server versions.
        /// </summary>
        /// <exception cref="NotSupportedException">Thrown when the property is not supported.</exception>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        /// <seealso cref="IsLengthSupported"/>
        long Length
        {
            get;
        }


        /// <summary>
        /// Tells you whether <see cref="Length"/> property is supported.
        /// The support is SharePoint version dependent.
        /// </summary>
        bool IsLengthSupported
        {
            get;
        }


        /// <summary>
        /// File's name from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        string Name
        {
            get;
        }


        /// <summary>
        /// Gets SharePoint server relative URL this instance refers to.
        /// </summary>
        string ServerRelativeUrl
        {
            get;
        }


        /// <summary>
        /// File's creation time from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        DateTime? TimeCreated
        {
            get;
        }


        /// <summary>
        /// File's last modification time from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        DateTime? TimeLastModified
        {
            get;
        }


        /// <summary>
        /// File's title from SharePoint server.
        /// Null if <see cref="ServerRelativeUrl"/> is null.
        /// </summary>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        string Title
        {
            get;
        }


        /// <summary>
        /// Gets the file's content as an array of bytes.
        /// </summary>
        /// <returns>Array of bytes containing SharePoint file's binary data</returns>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        byte[] GetContentBytes();


        /// <summary>
        /// Gets the file's content as a stream.
        /// </summary>
        /// <returns>Stream providing SharePoint file's binary data</returns>
        /// <exception cref="SystemIO.FileNotFoundException">Thrown when requested file is not present at SharePoint server.</exception>
        /// <exception cref="SharePointServerException">Thrown whenever server error occurs.</exception>
        /// <exception cref="WebException">Thrown when error related to network or protocol occurs.</exception>
        /// <exception cref="SharePointConnectionNotSupportedException">Thrown when given connectionData contains unsupported authentication mode or invalid URL.</exception>
        /// <exception cref="SharePointCCSDKException">Thrown when identity client library "msoidcliL.dll" needed for SharePoint Online authentication is missing. See documentation for details.</exception>
        /// <exception cref="Exception">Thrown in case of unexpected error condition.</exception>
        SystemIO.Stream GetContentStream();
    }
}

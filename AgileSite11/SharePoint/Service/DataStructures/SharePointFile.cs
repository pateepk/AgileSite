using System;
using System.Linq;
using System.Text;

using IOStreams = System.IO;


namespace CMS.SharePoint
{
    /// <summary>
    /// Represents general SharePoint file.
    /// </summary>
    internal class SharePointFile: ISharePointFile
    {
        private readonly byte[] mContentBytes;


        /// <summary>
        /// File's ETag (entity tag) from SharePoint server.
        /// </summary>
        public string ETag
        {
            get;
            set;
        }


        /// <summary>
        /// File's extension.
        /// </summary>
        public string Extension
        {
            get;
            set;
        }


        /// <summary>
        /// File's MIME type.
        /// </summary>
        public string MimeType
        {
            get;
            set;
        }


        /// <summary>
        /// Length of the file's binary content.
        /// </summary>
        public long Length
        {
            get
            {
                return mContentBytes.Length;
            }
        }


        /// <summary>
        /// Tells you whether <see cref="Length"/> property is supported.
        /// </summary>
        public bool IsLengthSupported
        {
            get
            {
                return true;
            }
        }


        /// <summary>
        /// File's name from SharePoint server.
        /// </summary>
        public string Name
        {
            get;
            set;
        }


        /// <summary>
        /// Gets SharePoint server relative URL this instance refers to.
        /// </summary>
        public string ServerRelativeUrl
        {
            get;
            set;
        }


        /// <summary>
        /// File's creation time from SharePoint server.
        /// </summary>
        public DateTime? TimeCreated
        {
            get;
            set;
        }


        /// <summary>
        /// File's last modification time from SharePoint server.
        /// </summary>
        public DateTime? TimeLastModified
        {
            get;
            set;
        }


        /// <summary>
        /// File's title from SharePoint server.
        /// </summary>
        public string Title
        {
            get;
            set;
        }


        /// <summary>
        /// Initializes a new SharePointFile instance.
        /// </summary>
        /// <param name="contentBytes">File content.</param>
        public SharePointFile(byte[] contentBytes)
        {
            if (contentBytes == null)
            {
                throw new ArgumentNullException("contentBytes");
            }

            mContentBytes = contentBytes;
        }


        /// <summary>
        /// Gets the file's content as an array of bytes.
        /// </summary>
        /// <returns>Array of bytes containing SharePoint file's binary data</returns>
        public byte[] GetContentBytes()
        {
            return mContentBytes;
        }


        /// <summary>
        /// Gets the file's content as a stream.
        /// </summary>
        /// <returns>Stream providing SharePoint file's binary data</returns>
        public IOStreams.Stream GetContentStream()
        {
            if (mContentBytes != null)
            {
                return new IOStreams.MemoryStream(mContentBytes);
            }

            return null;
        }
    }
}

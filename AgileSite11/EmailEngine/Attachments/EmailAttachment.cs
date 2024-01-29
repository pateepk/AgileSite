using System;
using System.Net.Mail;
using System.Net.Mime;

using SystemIO = System.IO;
namespace CMS.EmailEngine
{
    /// <summary>
    /// Extends .net e-mail attachment.
    /// </summary>
    public class EmailAttachment : Attachment
    {
        #region "Properties"

        /// <summary>
        /// Site ID of the attachment's source object.
        /// </summary>
        public int SiteID
        {
            get;
            set;
        }


        /// <summary>
        /// GUID of the attachment's source object (e.g. MetaFileGUID).
        /// </summary>
        public Guid AttachmentGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Last modification time of the attachement's source object.
        /// </summary>
        public DateTime LastModified
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes a new instance of the EmailAttachment class with the specified content string.
        /// </summary>
        /// <param name="fileName">File name</param>
        public EmailAttachment(string fileName)
            : base(fileName)
        {
        }


        /// <summary>
        /// Initializes a new instance of the EmailAttachment class with the specified content string and MIME type information.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="mediaType">MIME type information</param>
        public EmailAttachment(string fileName, string mediaType)
            : base(fileName, mediaType)
        {
        }


        /// <summary>
        /// Initializes a new instance of the EmailAttachment class with the specified content string and System.Net.Mime.ContentType.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="contentType">Content type</param>
        public EmailAttachment(string fileName, ContentType contentType)
            : base(fileName, contentType)
        {
        }


        /// <summary>
        /// Initializes a new instance of the EmailAttachment class with the specified stream and name.
        /// </summary>
        /// <param name="contentStream">Stream</param>
        /// <param name="name">File name</param>
        public EmailAttachment(SystemIO.Stream contentStream, string name)
            : base(contentStream, name)
        {
        }


        /// <summary>
        /// Initializes a new instance of the EmailAttachment class with the specified stream and content type.
        /// </summary>
        /// <param name="contentStream">Stream</param>
        /// <param name="contentType">Content type</param>
        public EmailAttachment(SystemIO.Stream contentStream, ContentType contentType)
            : base(contentStream, contentType)
        {
        }


        /// <summary>
        /// Initializes a new instance of the EmailAttachment class with the specified stream, name and MIME type information.
        /// </summary>
        /// <param name="contentStream">Stream</param>
        /// <param name="name">File name</param>
        /// <param name="mediaType">MIME type information</param>
        public EmailAttachment(SystemIO.Stream contentStream, string name, string mediaType)
            : base(contentStream, name, mediaType)
        {
        }


        /// <summary>
        /// Initializes a new instance of the EmailAttachment class with the specified stream, name, GUID, time stamp and site ID.
        /// </summary>
        /// <param name="contentStream">Stream</param>
        /// <param name="name">File name</param>
        /// <param name="guid">Attachment GUID - e.g. metafile GUID</param>
        /// <param name="lastModified">Time stamp of the attachment</param>
        /// <param name="siteId">Site ID - could be 0</param>
        public EmailAttachment(SystemIO.Stream contentStream, string name, Guid guid, DateTime lastModified, int siteId)
            : base(contentStream, name)
        {
            AttachmentGUID = guid;
            LastModified = lastModified;
            SiteID = siteId;
        }

        #endregion
    }
}
using System;

using CMS.Helpers;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Represents a document attachment regardless whether it is a current version or not.
    /// </summary>
    public sealed class Attachment
    {
        /// <summary>
        /// The current attachment.
        /// </summary>
        private readonly IAttachment mAttachment;
        

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class with the specified attachment.
        /// </summary>
        /// <param name="attachment">The attachment or attachment version.</param>
        /// <exception cref="ArgumentNullException"><paramref name="attachment"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="attachment"/> is not an attachment or attachment version.</exception>
        public Attachment(IAttachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException("attachment");
            }

            mAttachment = attachment;
        }


        /// <summary>
        /// Gets the file name of the attachment or attachment version.
        /// </summary>
        public string Name
        {
            get
            {
                return mAttachment.AttachmentName;
            }
        }


        /// <summary>
        /// Gets the file name extension of the attachment or attachment version.
        /// </summary>
        public string Extension
        {
            get
            {
                return mAttachment.AttachmentExtension;
            }
        }


        /// <summary>
        /// Gets the size, in bytes, of the attachment or attachment version.
        /// </summary>
        public int Size
        {
            get
            {
                return mAttachment.AttachmentSize;
            }
        }


        /// <summary>
        /// Gets the MIME type of the attachment or attachment version.
        /// </summary>
        public string MimeType
        {
            get
            {
                return mAttachment.AttachmentMimeType;
            }
        }


        /// <summary>
        /// Gets the binary content of the attachment or attachment version.
        /// </summary>
        public byte[] Content
        {
            get
            {
                return mAttachment.AttachmentBinary;
            }
        }


        /// <summary>
        /// Gets the width, in pixels, of the attachment or attachment version.
        /// </summary>
        public int ImageWidth
        {
            get
            {
                return mAttachment.AttachmentImageWidth;
            }
        }


        /// <summary>
        /// Gets the height, in pixels, of the attachment or attachment version.
        /// </summary>
        public int ImageHeight
        {
            get
            {
                return mAttachment.AttachmentImageHeight;
            }
        }


        /// <summary>
        /// Gets the globally unique identifier of the attachment or attachment version.
        /// </summary>
        public Guid GUID
        {
            get
            {
                return mAttachment.AttachmentGUID;
            }
        }


        /// <summary>
        /// Gets the date and time when the attachment or attachment version was last modified.
        /// </summary>
        public DateTime LastModified
        {
            get
            {
                return mAttachment.AttachmentLastModified;
            }
        }


        /// <summary>
        /// Gets the string representation of the attachment or attachment version hash.
        /// </summary>
        public string Hash
        {
            get
            {
                return mAttachment.AttachmentHash;
            }
        }


        /// <summary>
        /// Gets the title of the attachment or attachment version.
        /// </summary>
        public string Title
        {
            get
            {
                return mAttachment.AttachmentTitle;
            }
        }


        /// <summary>
        /// Gets the description of the attachment or attachment version.
        /// </summary>
        public string Description
        {
            get
            {
                return mAttachment.AttachmentDescription;
            }
        }


        /// <summary>
        /// Gets the identifier of the attachment version.
        /// </summary>
        /// <remarks>
        /// The identifier of the attachment that is not versioned is 0.
        /// </remarks>
        public int VersionID
        {
            get
            {
                return ValidationHelper.GetInteger(mAttachment.GetValue("AttachmentHistoryID"), 0);
            }
        }
    }
}

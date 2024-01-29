using CMS.DataEngine;
using CMS.Helpers;
using System;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Attachment interface to unify access to attachment objects.
    /// </summary>
    public interface IAttachment : IInfo
    {
        /// <summary>
        /// Gets the file name of the attachment or attachment version.
        /// </summary>
        string AttachmentName
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the file name extension of the attachment or attachment version.
        /// </summary>
        string AttachmentExtension
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the size, in bytes, of the attachment or attachment version.
        /// </summary>
        int AttachmentSize
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the MIME type of the attachment or attachment version.
        /// </summary>
        string AttachmentMimeType
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the binary content of the attachment or attachment version.
        /// </summary>
        byte[] AttachmentBinary
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the width, in pixels, of the attachment or attachment version.
        /// </summary>
        int AttachmentImageWidth
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the height, in pixels, of the attachment or attachment version.
        /// </summary>
        int AttachmentImageHeight
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the globally unique identifier of the attachment or attachment version.
        /// </summary>
        Guid AttachmentGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the date and time when the attachment or attachment version was last modified.
        /// </summary>
        DateTime AttachmentLastModified
        {
            get;
            set;
        }


        /// <summary>
        /// Stores the attachment order.
        /// </summary>
        int AttachmentOrder
        {
            get;
            set;
        }


        /// <summary>
        /// Attachment custom data.
        /// </summary>
        ContainerCustomData AttachmentCustomData
        {
            get;
        }


        /// <summary>
        /// Extracted content from attachment binary used for search indexing.
        /// </summary>
        XmlData AttachmentSearchContent
        {
            get;
        }


        /// <summary>
        /// Gets the string representation of the attachment or attachment version hash.
        /// </summary>
        string AttachmentHash
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the title of the attachment or attachment version.
        /// </summary>
        string AttachmentTitle
        {
            get;
            set;
        }


        /// <summary>
        /// Gets the description of the attachment or attachment version.
        /// </summary>
        string AttachmentDescription
        {
            get;
            set;
        }


        /// <summary>
        /// Holds the GUID of the document field (group) under which the grouped attachment belongs.
        /// </summary>
        Guid AttachmentGroupGUID
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if attachment is an unsorted attachment.
        /// </summary>
        bool AttachmentIsUnsorted
        {
            get;
            set;
        }


        /// <summary>
        /// Related document Document ID.
        /// </summary>
        int AttachmentDocumentID
        {
            get;
            set;
        }


        /// <summary>
        /// Attachment site ID.
        /// </summary>
        int AttachmentSiteID
        {
            get;
            set;
        }


        /// <summary>
        /// Text identification of used variant definition.
        /// </summary>
        string AttachmentVariantDefinitionIdentifier
        {
            get;
            set;
        }


        /// <summary>
        /// Attachment variant parent ID.
        /// </summary>
        int AttachmentVariantParentID
        {
            get;
            set;
        }
    }
}
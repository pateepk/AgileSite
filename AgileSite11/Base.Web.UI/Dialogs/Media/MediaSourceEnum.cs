using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Enumeration defining the source of the media.
    /// </summary>
    public enum MediaSourceEnum
    {
        /// <summary>
        /// Standard attachment.
        /// </summary>
        Attachment,

        /// <summary>
        /// Document attachments.
        /// </summary>
        DocumentAttachments,

        /// <summary>
        /// Documents from content tree.
        /// </summary>
        Content,

        /// <summary>
        /// Media files from media libraries.
        /// </summary>
        MediaLibraries,

        /// <summary>
        /// Media files from another website.
        /// </summary>
        Web,

        /// <summary>
        /// Physical file.
        /// </summary>
        PhysicalFile,

        /// <summary>
        /// Meta file.
        /// </summary>
        MetaFile
    }
}
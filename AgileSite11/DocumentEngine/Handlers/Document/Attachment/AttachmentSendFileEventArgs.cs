using CMS.Base;

namespace CMS.DocumentEngine.Internal
{
    /// <summary>
    /// Attachment send file event arguments.
    /// </summary>
    public sealed class AttachmentSendFileEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Creates an instance of <see cref="AttachmentSendFileEventArgs"/> class.
        /// </summary>
        /// <param name="attachment">Output attachment.</param>
        /// <param name="siteName">Site name.</param>
        /// <param name="isLiveSite">Indicates if handler requested on a live site.</param>
        /// <param name="isMultipart">Indicates whether it is multipart range request.</param>
        /// <param name="isRangeRequest">Indicates whether it is range request.</param>
        public AttachmentSendFileEventArgs(CMSOutputAttachment attachment, string siteName, bool isLiveSite, bool isMultipart, bool isRangeRequest)
        {
            Attachment = attachment;
            SiteName = siteName;
            IsLiveSite = isLiveSite;
            IsMultipart = isMultipart;
            IsRangeRequest = isRangeRequest;
        }


        /// <summary>
        /// Attachment object for output.
        /// </summary>
        public CMSOutputAttachment Attachment
        {
            get;
        }


        /// <summary>
        /// Site name of the site context of the request.
        /// </summary>
        public string SiteName
        {
            get;
        }


        /// <summary>
        /// Indicates if live site mode.
        /// </summary>
        public bool IsLiveSite
        {
            get;
        }


        /// <summary>
        /// Indicates whether it is multipart range request.
        /// </summary>
        public bool IsMultipart
        {
            get;
        }


        /// <summary>
        /// Indicates whether it is range request.
        /// </summary>
        public bool IsRangeRequest
        {
            get;
        }
    }
}

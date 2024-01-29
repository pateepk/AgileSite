using System;

namespace WTE.Communication
{
    /// <summary>
    /// Class for representing an email attachment
    /// </summary>
    public class Attachment
    {
        /// <summary>
        /// Flags that determine how MIME attachments are added to the message.
        /// </summary>
        [Flags]
        public enum MIMEHeaderFlags
        {
            None = 0, ///< No Flags
            NoContentType = 1, ///< Do not automatically add content type header.
            NoContentTransferEncoding = 2, ///< Do not automatically add content transfer encoding header.
            NoDisposition = 4,///< Do not automatically add the disposition headers.
            NoDescription = 8, ///< Do not automatically add the description header.
            GenericContentType = 16, ///< Use generic content type application/octet-stream.
        }

        /// <summary>
        /// Provides enumerated values for e-mail attachment encoding.
        /// </summary>
        public enum AttachmentEncoding
        {
            None = 0, ///< Specifies that no encoding is to be used on the attachment.
            Base64 = 1, ///< Specifies that Base64 encoding is to be used on the attachment.
            QuotedPrintable = 2, ///< Specifies that Quoted-Printable encoding is to be used on the attachment.
            EightBit = 4, ///< Specifies that the attachment data is in 8bit format.
        }

        /// <summary>
        /// Filename
        /// </summary>
        public string Filename;

        /// <summary>
        /// friendly name for attachment
        /// </summary>
        public string Name;

        /// <summary>
        /// attachment data
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Encoding flags
        /// </summary>
        public AttachmentEncoding Encoding = AttachmentEncoding.Base64;

        /// <summary>
        /// Mime flags
        /// </summary>
        public MIMEHeaderFlags MimeFlags = MIMEHeaderFlags.GenericContentType;
    }
}
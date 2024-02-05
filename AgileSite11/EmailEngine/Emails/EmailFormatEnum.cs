using System;

namespace CMS.EmailEngine
{
    /// <summary>
    /// Represents type of the email format used by EmailMessage object.
    /// </summary>
    public enum EmailFormatEnum
    {
        /// <summary>
        /// Email HTML format.
        /// </summary>
        Html = 0,

        /// <summary>
        /// Email plain text format.
        /// </summary>
        PlainText = 1,

        /// <summary>
        /// 
        /// </summary>
        Both = 2,

        /// <summary>
        /// Default value taken from settings.
        /// </summary>
        Default = 3
    }
}
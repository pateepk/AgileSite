using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Enumeration defining the output format.
    /// </summary>
    public enum OutputFormatEnum
    {
        /// <summary>
        /// Output as media in HTML format.
        /// </summary>
        HTMLMedia,

        /// <summary>
        /// Output as link in HTML format.
        /// </summary>
        HTMLLink,

        /// <summary>
        /// Output as media in BB code.
        /// </summary>
        BBMedia,

        /// <summary>
        /// Output as link in BB code.
        /// </summary>
        BBLink,

        /// <summary>
        /// Output as URL address.
        /// </summary>
        URL,

        /// <summary>
        /// Output as Node GUID.
        /// </summary>
        NodeGUID,

        /// <summary>
        /// Custom output format.
        /// </summary>
        Custom
    }
}
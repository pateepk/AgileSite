using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Document con type enumeration
    /// </summary>
    [Flags]
    public enum IconType
    {
        /// <summary>
        /// Linked document
        /// </summary>
        Linked = 0x1,

        /// <summary>
        /// Not translated document
        /// </summary>
        NotTranslated = 0x2,

        /// <summary>
        /// Redirected document
        /// </summary>
        Redirected = 0x4,

        /// <summary>
        /// Archived document
        /// </summary>
        Archived = 0x8,

        /// <summary>
        /// Published document
        /// </summary>
        Published = 0x10,

        /// <summary>
        /// Not published document
        /// </summary>
        NotPublished = 0x20,

        /// <summary>
        /// Checked out document
        /// </summary>
        CheckedOut = 0x40,

        /// <summary>
        /// Document version not published
        /// </summary>
        VersionNotPublished = 0x80,
    }
}

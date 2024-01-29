using System;
using System.Collections;
using System.Data;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Type of object processing enumeration.
    /// </summary>
    public enum ProcessObjectEnum
    {
        /// <summary>
        /// Default.
        /// </summary>
        Default = 0,

        /// <summary>
        /// Process all objects.
        /// </summary>
        All = 1,

        /// <summary>
        /// Process only selected.
        /// </summary>
        Selected = 2,

        /// <summary>
        /// Do not process.
        /// </summary>
        None = 3,

        /// <summary>
        /// Add site binding(special configuration).
        /// </summary>
        SiteBinding = 4
    }
}
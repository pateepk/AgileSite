using System;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Export type enumeration.
    /// </summary>
    public enum ExportTypeEnum : int
    {
        /// <summary>
        /// No objects.
        /// </summary>
        None = 0,

        /// <summary>
        /// All objects.
        /// </summary>
        All = 1,

        /// <summary>
        /// Site objects.
        /// </summary>
        Site = 2,

        /// <summary>
        /// Default.
        /// </summary>
        Default = 3
    }
}
using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Available libraries enumeration.
    /// </summary>
    public enum AvailableLibrariesEnum
    {
        /// <summary>
        /// All libraries available.
        /// </summary>
        All,

        /// <summary>
        /// None library available.
        /// </summary>
        None,

        /// <summary>
        /// Only current library according library context available.
        /// </summary>
        OnlyCurrentLibrary,

        /// <summary>
        /// Only specified single library available.
        /// </summary>
        OnlySingleLibrary
    }
}
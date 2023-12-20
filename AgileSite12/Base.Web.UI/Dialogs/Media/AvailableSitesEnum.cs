using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Available sites enumeration.
    /// </summary>
    public enum AvailableSitesEnum
    {
        /// <summary>
        /// All sites available.
        /// </summary>
        All,

        /// <summary>
        /// Only current site according site context available.
        /// </summary>
        OnlyCurrentSite,

        /// <summary>
        /// Only specified single site available.
        /// </summary>
        OnlySingleSite
    }
}
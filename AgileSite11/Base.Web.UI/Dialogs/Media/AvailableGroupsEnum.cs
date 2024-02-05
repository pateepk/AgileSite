using System;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Available group enumeration.
    /// </summary>
    public enum AvailableGroupsEnum
    {
        /// <summary>
        /// All groups available.
        /// </summary>
        All,

        /// <summary>
        /// None group available.
        /// </summary>
        None,

        /// <summary>
        /// Only current group according group context available.
        /// </summary>
        OnlyCurrentGroup,

        /// <summary>
        /// Only specified single group available.
        /// </summary>
        OnlySingleGroup
    }
}
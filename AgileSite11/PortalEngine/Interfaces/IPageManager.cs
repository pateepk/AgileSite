using System;

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page manager interface.
    /// </summary>
    public interface IPageManager
    {
        /// <summary>
        /// Current view mode.
        /// </summary>
        ViewModeEnum ViewMode
        {
            get;
        }


        /// <summary>
        /// Page site name.
        /// </summary>
        string SiteName
        {
            get;
            set;
        }


        /// <summary>
        /// Returns true if the user is authorized for current document.
        /// </summary>
        bool IsAuthorized
        {
            get;
        }


        /// <summary>
        /// Causes clearing the control data cache.
        /// </summary>
        void ClearCache();


        /// <summary>
        /// Gets or sets the value that indicates whether permissions for current document should be checked
        /// </summary>
        bool CheckPermissions
        {
            get;
            set;
        }
    }
}
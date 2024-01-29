using CMS.Modules;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Represents an object on dashboard.
    /// This object is represented by one or more UI elements.
    /// So far it can be application or single object.
    /// </summary>
    internal class DashboardItem
    {
        /// <summary>
        /// Application UI element.
        /// </summary>
        public UIElementInfo Application
        {
            get;
            set;
        }


        /// <summary>
        /// Single object UI element.
        /// </summary>
        public UIElementInfo SingleObject
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether the object should be visible on dashboard or not (single object form another site, insufficient permission, license, etc.)
        /// </summary>
        public bool IsVisible
        {
            get;
            set;
        }
    }
}

using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Class representing breadcrumbs item.
    /// </summary>
    public class BreadcrumbItem : NavigationItem
    {
        #region "Properties"

        /// <summary>
        /// Position of a tab.
        /// </summary>
        public int Index
        {
            get;
            set;
        }


        /// <summary>
        /// Target for hyperlink.
        /// </summary>
        public string Target
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BreadcrumbItem()
        {
            Index = -1;
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Text;

namespace CMS.UIControls
{
    /// <summary>
    /// Represents data structure that holds key attributes of application. Possible to use it as item in lists.
    /// </summary>
    internal class ApplicationCategoryRecord
    {
        #region "Properties" 

        /// <summary>
        /// Number of child elements.
        /// </summary>
        public int ChildCount
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether category visibility condition is fulfilled.
        /// </summary>
        public bool IsVisible
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates whether category is permitted.
        /// </summary>
        public bool IsAccessible
        {
            get;
            set;
        }


        /// <summary>
        /// Order of the category.
        /// </summary>
        public int Order
        {
            get;
            set;
        }

        
        /// <summary>
        /// Indicates whether only global applications are displayed in the category.
        /// </summary>
        public bool ShowOnlyGlobalApps
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="childCount">Number of child elements</param>
        /// <param name="isVisible">Indicates whether category visibility condition is fulfilled</param>
        /// <param name="isAccessible">Indicates whether category is permitted</param>
        /// <param name="order">Order of the category</param>
        public ApplicationCategoryRecord(int childCount, bool isVisible, bool isAccessible, int order)
        {
            ChildCount = childCount;
            IsVisible = isVisible;
            IsAccessible = isAccessible;
            Order = order;
        }

        #endregion
    }
}

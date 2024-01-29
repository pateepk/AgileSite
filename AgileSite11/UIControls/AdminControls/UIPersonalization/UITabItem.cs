using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// UITab control item.
    /// </summary>
    public class UITabItem : NavigationItem
    {
        #region "Variables"

        private List<UITabItem> mSubItems;

        #endregion


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
        /// Name of tab.
        /// </summary>
        public string TabName
        {
            get;
            set;
        }


        /// <summary>
        /// If true, default permission check is skipped for this item
        /// </summary>
        public bool SkipCheckPermissions
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if this tab is to be replaced by its child tabs. 
        /// </summary>
        public bool Expand
        {
            get;
            set;
        }

        
        /// <summary>
        /// Tab items to be shown under this tab.
        /// </summary>
        public List<UITabItem> SubItems
        {
            get
            {
                return mSubItems ?? (mSubItems = new List<UITabItem>());
            }
        }


        /// <summary>
        /// Indicates if tab item has any sub items.
        /// </summary>
        public bool HasSubItems
        {
            get
            {
                return (mSubItems != null) && (mSubItems.Count > 0);
            }
        }


        /// <summary>
        /// Parent tab of this tab.
        /// </summary>
        public UITabItem ParentTabItem
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public UITabItem()
        {
            Index = -1;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks TabItem for emptiness.
        /// </summary>
        /// <returns>TRUE if all fields are equal to null.</returns>
        public bool IsEmpty()
        {
            return (Text == null) && (OnClientClick == null) && (RedirectUrl == null) && (Tooltip == null) && (CssClass == null);
        }

        #endregion
    }
}

using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.UIControls
{
    /// <summary>
    /// Arguments for the UniTree's OnGetImage event handler
    /// </summary>
    public class UniTreeImageArgs : CMSEventArgs
    {
        #region "Properties"

        /// <summary>
        /// Input tree node for event
        /// </summary>
        public UniTreeNode TreeNode
        {
            get;
            set;
        }


        /// <summary>
        /// Output image path for tree node.
        /// </summary>
        public String ImagePath
        {
            get;
            set;
        }


        /// <summary>
        /// Output CSS class for font icon.
        /// </summary>
        public String IconClass
        {
            get;
            set;
        }

        #endregion
    }
}

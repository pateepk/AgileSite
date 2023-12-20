using System;
using System.Linq;
using System.Text;
using System.Web.UI.WebControls;

using CMS.Modules;

namespace CMS.UIControls
{
    /// <summary>
    /// Basic class for UI menu.
    /// </summary>
    public class UIMenu : CMSUserControl
    {
        #region "Properties & Variables"

        private string mTargetFrame = "frameMain";

        /// <summary>
        /// Default target frame for urls
        /// </summary>
        public string TargetFrame
        {
            get
            {
                return mTargetFrame;
            }
            set
            {
                mTargetFrame = value;
            }
        }

        #endregion


        #region "Custom events"

        /// <summary>
        /// Node created delegate.
        /// </summary>
        public delegate TreeNode NodeCreatedEventHandler(UIElementInfo uiElement, TreeNode defaultNode);


        /// <summary>
        /// Node created event handler.
        /// </summary>
        public event NodeCreatedEventHandler OnNodeCreated;

        #endregion


        #region "Event handlers"

        /// <summary>
        /// On node created handler.
        /// </summary>
        /// <param name="uiElement">UI Element</param>
        /// <param name="defaultNode">Default tree node</param>
        protected TreeNode RaiseOnNodeCreated(UIElementInfo uiElement, TreeNode defaultNode)
        {
            // Raise the node created handler
            if (OnNodeCreated != null)
            {
                return OnNodeCreated(uiElement, defaultNode);
            }

            return defaultNode;
        }

        #endregion
    }
}

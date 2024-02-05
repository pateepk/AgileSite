using System;
using System.ComponentModel;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Placeholder for context menus.
    /// </summary>
    [ToolboxItem(false)]
    public class ContextMenuPlaceHolder : CMSPlaceHolder
    {
        #region "Variables"

        private CMSUpdatePanel pnlInfo = new CMSUpdatePanel { ID = "ctxM" };

        #endregion


        #region "Properties"

        /// <summary>
        /// Inner control to be used as a context menu container
        /// </summary>
        public Control InnerControl
        {
            get
            {
                return pnlInfo.ContentTemplateContainer;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures ContextMenuPlaceholder for current page
        /// </summary>
        /// <param name="page">Page instance</param>
        /// <param name="container">Container control (PlaceHolder control)</param>
        public static void EnsurePlaceholder(Page page, Control container)
        {
            // Try ensure context menu placeholder
            if (page.Items[typeof(ContextMenuPlaceHolder)] == null)
            {
                try
                {
                    ContextMenuPlaceHolder plcMenu = new ContextMenuPlaceHolder();
                    plcMenu.ID = "plcCtx";
                    container.Controls.Add(plcMenu);
                }
                catch
                {
                    // Do not throw an error
                }
            }
        }


        /// <summary>
        /// Init event handler
        /// </summary>
        /// <param name="e"></param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Keep information that context menu placeholder exists on current page
            Page.Items[typeof(ContextMenuPlaceHolder)] = this;
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            EnsureChildControls();
        }


        /// <summary>
        /// Creates child controls
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            // Encapsulate in update panel if script manager is registered
            ScriptManager sManager = ScriptManager.GetCurrent(Page);
            if (sManager != null)
            {
                // Update panel
                Controls.Add(pnlInfo);
            }
        }

        #endregion
    }
}
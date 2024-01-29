using System;
using System.ComponentModel;
using System.Web.UI;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// User menu container class (calls the user context menu if not set else).
    /// </summary>
    [ToolboxItem(false)]
    public class UserMenuContainer : ContextMenuContainer
    {
        /// <summary>
        /// OnInit event.
        /// </summary>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            // Enable mouse click
            EnableMouseClick = true;

            // Init the menu
            InitializeMenu();
        }


        /// <summary>
        /// Render override.
        /// </summary>
        /// <param name="writer">Writer</param>
        protected override void Render(HtmlTextWriter writer)
        {
            if (Context == null)
            {
                writer.Write(" [ UserMenuContainer : " + ID + " ]");
                return;
            }

            base.Render(writer);
        }


        /// <summary>
        /// OnDataBinding event.
        /// </summary>
        protected override void OnDataBinding(EventArgs e)
        {
            base.OnDataBinding(e);

            // Init the menu
            InitializeMenu();
        }


        /// <summary>
        /// Initilaizes the menu.
        /// </summary>
        private void InitializeMenu()
        {
            if (string.IsNullOrEmpty(MenuControlPath) && !string.IsNullOrEmpty(Parameter))
            {
                MenuControlPath = "~/CMSAdminControls/ContextMenus/UserContextMenu.ascx";

                if (MenuControl != null)
                {
                    MenuControl.Dynamic = true;
                    MenuControl.CssClass = ContextMenuCssClass;
                    MenuControl.ResourcePrefix = ResourcePrefix;
                    MenuControl.ContainerUniqueID = UniqueID;
                }
            }
        }


        /// <summary>
        /// Reload menu data.
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">Event arguments</param>
        protected override void ReloadMenuData(object sender, EventArgs e)
        {
            if ((MenuControl != null) && (MenuControl.ContextMenuControl != null))
            {
                MenuControl.ContextMenuControl.ReloadData();
            }
        }
    }
}
using System;
using System.ComponentModel;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Web;
using System.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// .NET representation of the UI Layout JavaScript plug-in. Adds a footer pane automatically when used in dialogs
    /// </summary>
    [ParseChildren(typeof(UILayoutPane), ChildrenAsProperties = true),
    AspNetHostingPermission(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal),
    AspNetHostingPermission(SecurityAction.Demand, Level = AspNetHostingPermissionLevel.Minimal),
    DefaultProperty("Panes"),
    ToolboxData("<{0}:UIElementLayout runat=\"server\"> </{0}:UIElementLayout>")]
    public class UIElementLayout : UILayout
    {
        #region "Variables"

        private string mFooterControlPath = "~/CMSModules/AdminControls/Controls/UIControls/DialogFooter.ascx";

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates whether the footer pane should be displayed
        /// </summary>
        public bool DisplayFooter
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the path to the footer control.
        /// </summary>
        public string FooterControlPath
        {
            get
            {
                return mFooterControlPath;
            }
            set
            {
                mFooterControlPath = value;
            }
        }

        #endregion


        #region "Page methods"

        /// <summary>
        /// Load event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            // Preliminary check for pane consistency
            if (DisplayFooter
                && !Panes.Exists(p => p.Direction == PaneDirectionEnum.South))
            {
                // Insert the default dialog footer
                UILayoutPane dialogFooterPane = new UILayoutPane
                {
                    ID = "paneDialogFooter",
                    Direction = PaneDirectionEnum.South,
                    ControlPath = FooterControlPath,
                    Resizable = false,
                    Slidable = false,
                    RenderAs = HtmlTextWriterTag.Div,
                    SpacingOpen = 0
                };

                Controls.Add(dialogFooterPane);
                Panes.Add(dialogFooterPane);
            }

            base.OnLoad(e);
        }

        #endregion
    }
}

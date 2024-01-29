using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Separator for context menu.
    /// </summary>
    [ToolboxItem(false)]
    public class ContextMenuSeparator : Panel
    {
        #region "Variables"

        private static string DEFAULT_CSSCLASS = "Separator";

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContextMenuSeparator()
        {
            Controls.Add(new LiteralControl("&nbsp;"));

            // Add default class name if it is not already set
            if (!(" " + CssClass + " ").Contains(" " + DEFAULT_CSSCLASS + " "))
            {
                CssClass = DEFAULT_CSSCLASS;
            }
            EnableViewState = false;
        }

        #endregion
    }
}

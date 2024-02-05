using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for site selection with all for global admin.
    /// </summary>
    [ToolboxData("<{0}:SelectSiteWithAllForAdmin runat=server></{0}:SelectSiteWithAllForAdmin>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectSiteWithAllForAdmin : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectSiteWithAllForAdmin()
        {
            FormControlName = "SiteSelectorWithAllFieldForGlobalAdmin";
        }


        #region "Methods"

        /// <summary>
        /// OnLoad event handler
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            SetValue("UseCodeNameForSelection", true);
        }

        #endregion
    }
}
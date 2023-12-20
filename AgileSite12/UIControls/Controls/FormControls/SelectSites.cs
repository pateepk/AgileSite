using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the site selection.
    /// </summary>
    [ToolboxData("<{0}:SelectSites runat=server></{0}:SelectSites>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectSites : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectSites()
        {
            FormControlName = "SimpleCheckboxSiteSelector";
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
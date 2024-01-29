using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the site culture selection with (all) option.
    /// </summary>
    [ToolboxData("<{0}:SiteCultureSelectorAll runat=server></{0}:SiteCultureSelectorAll>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SiteCultureSelectorAll : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SiteCultureSelectorAll()
        {
            FormControlName = "SiteCultureSelectorAll";
        }
    }
}
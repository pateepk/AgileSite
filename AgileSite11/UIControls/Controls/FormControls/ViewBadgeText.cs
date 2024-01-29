using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for view badge text.
    /// </summary>
    [ToolboxData("<{0}:ViewBadgeText runat=server></{0}:ViewBadgeText>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewBadgeText : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ViewBadgeText()
        {
            FormControlName = "ViewerBadgeText";
        }
    }
}
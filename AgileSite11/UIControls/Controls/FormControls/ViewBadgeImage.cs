using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for view integer number.
    /// </summary>
    [ToolboxData("<{0}:ViewBadgeImage runat=server></{0}:ViewBadgeImage>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewBadgeImage : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ViewBadgeImage()
        {
            FormControlName = "ViewBadgeImage";
        }
    }
}
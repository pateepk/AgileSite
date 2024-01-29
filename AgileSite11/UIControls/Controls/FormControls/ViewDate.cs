using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for view date.
    /// </summary>
    [ToolboxData("<{0}:ViewDate runat=server></{0}:ViewDate>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewDate : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ViewDate()
        {
            FormControlName = "ViewDate";
        }
    }
}
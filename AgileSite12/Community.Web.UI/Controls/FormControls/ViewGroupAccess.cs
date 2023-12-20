using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Form control for view group access.
    /// </summary>
    [ToolboxData("<{0}:ViewGroupAccess runat=server></{0}:ViewGroupAccess>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewGroupAccess : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ViewGroupAccess()
        {
            FormControlName = "ViewGroupAccess";
        }
    }
}
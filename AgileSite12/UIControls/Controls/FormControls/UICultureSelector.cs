using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the UI culture selection.
    /// </summary>
    [ToolboxData("<{0}:UICultureSelector runat=server></{0}:UICultureSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class UICultureSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public UICultureSelector()
        {
            FormControlName = "UICultureSelector";
        }
    }
}
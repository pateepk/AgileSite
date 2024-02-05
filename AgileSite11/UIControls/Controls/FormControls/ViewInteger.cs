using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for view integer number.
    /// </summary>
    [ToolboxData("<{0}:ViewInteger runat=server></{0}:ViewInteger>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewInteger : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ViewInteger()
        {
            FormControlName = "ViewIntegerNumber";
        }
    }
}
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the multiple users selection.
    /// </summary>
    [ToolboxData("<{0}:SelectUsers runat=server></{0}:SelectUsers>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectUsers : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectUsers()
        {
            FormControlName = "MultipleUserSelector";
        }
    }
}
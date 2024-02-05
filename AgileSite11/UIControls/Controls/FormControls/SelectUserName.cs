using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the user name selection.
    /// </summary>
    [ToolboxData("<{0}:SelectUserName runat=server></{0}:SelectUserName>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectUserName : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectUserName()
        {
            FormControlName = "UserNameSelector";
        }
    }
}
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for view user gender.
    /// </summary>
    [ToolboxData("<{0}:ViewUserGender runat=server></{0}:ViewUserGender>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewUserGender : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ViewUserGender()
        {
            FormControlName = "ViewUserGender";
        }
    }
}
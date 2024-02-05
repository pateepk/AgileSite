using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for view secure text.
    /// </summary>
    [ToolboxData("<{0}:ViewSecureText runat=server></{0}:ViewSecureText>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ViewSecureText : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ViewSecureText()
        {
            FormControlName = "ViewSecureText";
        }
    }
}
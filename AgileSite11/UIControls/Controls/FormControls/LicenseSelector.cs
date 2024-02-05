using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the license selection.
    /// </summary>
    [ToolboxData("<{0}:LicenseSelector runat=server></{0}:LicenseSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class LicenseSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public LicenseSelector()
        {
            FormControlName = "LicenseSelector";
        }
    }
}
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Form control for the report selection with defining parameters.
    /// </summary>
    [ToolboxData("<{0}:ReportSelector runat=server></{0}:ReportSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ReportSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ReportSelector()
        {
            FormControlName = "ReportSelectorDropDown";
        }
    }
}

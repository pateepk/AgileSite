using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Form control for the report value selection.
    /// </summary>
    [ToolboxData("<{0}:ReportValueSelector runat=server></{0}:ReportValueSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ReportValueSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ReportValueSelector()
        {
            FormControlName = "ReportValueSelector";
        }
    }
}

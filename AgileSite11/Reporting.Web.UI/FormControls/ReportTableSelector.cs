using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Form control for the report table selection.
    /// </summary>
    [ToolboxData("<{0}:ReportTableSelector runat=server></{0}:ReportTableSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ReportTableSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ReportTableSelector()
        {
            FormControlName = "ReportTableSelector";
        }
    }
}

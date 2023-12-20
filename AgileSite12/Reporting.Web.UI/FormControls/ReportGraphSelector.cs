using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Form control for the report graph selection.
    /// </summary>
    [ToolboxData("<{0}:ReportGraphSelector runat=server></{0}:ReportGraphSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class ReportGraphSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ReportGraphSelector()
        {
            FormControlName = "ReportGraphSelector";
        }
    }
}

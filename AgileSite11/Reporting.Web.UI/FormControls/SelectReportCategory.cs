using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Reporting.Web.UI
{
    /// <summary>
    /// Form control for the report category selection.
    /// </summary>
    [ToolboxData("<{0}:SelectReportCategory runat=server></{0}:SelectReportCategory>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectReportCategory : FormControl
    {
        #region "Constructor"

        /// <summary>
        /// Constructor.
        /// </summary>
        public SelectReportCategory()
        {
            FormControlName = "ReportCategorySelector";
        }

        #endregion
    }
}
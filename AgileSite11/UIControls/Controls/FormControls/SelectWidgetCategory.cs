using System;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the widget category selection.
    /// </summary>
    [ToolboxData("<{0}:SelectWidgetCategory runat=server></{0}:SelectWidgetCategory>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectWidgetCategory : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectWidgetCategory()
        {
            FormControlName = "WidgetCategorySelector";
        }
    }
}

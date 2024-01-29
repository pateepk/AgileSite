using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the event log type selection.
    /// </summary>
    [ToolboxData("<{0}:EventLogTypeSelector runat=server></{0}:EventLogTypeSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class EventLogTypeSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public EventLogTypeSelector()
        {
            FormControlName = "EventLogTypeSelector";
        }
    }
}
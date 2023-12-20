using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Notifications.Web.UI
{
    /// <summary>
    /// Form control for the notification gateway selection.
    /// </summary>
    [ToolboxData("<{0}:MultipleNotificationGatewaySelector runat=server></{0}:MultipleNotificationGatewaySelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class MultipleNotificationGatewaySelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MultipleNotificationGatewaySelector()
        {
            FormControlName = "MultipleNotificationGatewaySelector";
        }
    }
}

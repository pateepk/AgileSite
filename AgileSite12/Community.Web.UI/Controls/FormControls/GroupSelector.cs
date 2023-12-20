using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Form control for the forum group selection.
    /// </summary>
    [ToolboxData("<{0}:GroupSelector runat=server></{0}:GroupSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class GroupSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public GroupSelector()
        {
            FormControlName = "GroupsSelector";
        }
    }
}
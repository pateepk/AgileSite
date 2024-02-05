using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.Forums.Web.UI
{
    /// <summary>
    /// Form control the forum selection with (all) option.
    /// </summary>
    [ToolboxData("<{0}:FullForumSelector runat=server></{0}:FullForumSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FullForumSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public FullForumSelector()
        {
            FormControlName = "FullForumSelector";
        }
    }
}
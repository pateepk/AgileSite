using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the search index selection.
    /// </summary>
    [ToolboxData("<{0}:IndexSelector runat=server></{0}:IndexSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class IndexSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public IndexSelector()
        {
            FormControlName = "SearchIndexSelector";
        }
    }
}
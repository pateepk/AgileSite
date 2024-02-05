using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.UIControls
{
    /// <summary>
    /// Form control for the document CSS stylesheet selection.
    /// </summary>
    [ToolboxData("<{0}:DocumentStylesheetSelector runat=server></{0}:DocumentStylesheetSelector>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class DocumentStylesheetSelector : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public DocumentStylesheetSelector()
        {
            FormControlName = "DocumentStylesheetSelector";
        }
    }
}

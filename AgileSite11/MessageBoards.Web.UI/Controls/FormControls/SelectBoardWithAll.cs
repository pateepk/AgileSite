using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.FormEngine.Web.UI;

namespace CMS.MessageBoards.Web.UI
{
    /// <summary>
    /// Form control for the message board selection with all option.
    /// </summary>
    [ToolboxData("<{0}:SelectBoardWithAll runat=server></{0}:SelectBoardWithAll>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class SelectBoardWithAll : FormControl
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SelectBoardWithAll()
        {
            FormControlName = "MessageBoardSelectorWithAll";
        }
    }
}

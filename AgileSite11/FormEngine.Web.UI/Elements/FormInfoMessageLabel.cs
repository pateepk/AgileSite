using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form info message label for the whole form.
    /// </summary>
    [ToolboxData("<{0}:FormInfoMessageLabel runat=server/>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FormInfoMessageLabel : LocalizedLabel
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormInfoMessageLabel()
        {
            CssClass = "InfoLabel";
            Visible = false;
        }

        #endregion
    }
}
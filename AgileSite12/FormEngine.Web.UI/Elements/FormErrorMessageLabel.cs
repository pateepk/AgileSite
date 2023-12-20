using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.Base.Web.UI;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form error message label for the whole form.
    /// </summary>
    [ToolboxData("<{0}:FormErrorMessageLabel runat=server/>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FormErrorMessageLabel : LocalizedLabel
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormErrorMessageLabel()
        {
            CssClass = "ErrorLabel";
            Visible = false;
        }

        #endregion
    }
}
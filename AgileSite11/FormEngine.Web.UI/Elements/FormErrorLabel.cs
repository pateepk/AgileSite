using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

namespace CMS.FormEngine.Web.UI
{
    /// <summary>
    /// Form error label for a specific form field.
    /// </summary>
    [ToolboxData("<{0}:FormErrorLabel runat=server/>")]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class FormErrorLabel : FormLabel
    {
        #region "Constructors"

        /// <summary>
        /// Constructor.
        /// </summary>
        public FormErrorLabel()
        {
            CssClass = "EditingFormErrorLabel";
            Visible = false;
            UseFFI = false;
        }

        #endregion
    }
}
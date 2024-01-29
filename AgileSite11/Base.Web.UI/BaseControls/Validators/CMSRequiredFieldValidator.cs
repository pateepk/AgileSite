using System;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// CMSRequiredFieldValidator, inherited from System.Web.UI.WebControls.RequiredFieldValidator. Handles generally design of required field validator.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSRequiredFieldValidator : RequiredFieldValidator
    {
        /// <summary>
        /// Initializes default CSS class for the validator.
        /// </summary>
        public CMSRequiredFieldValidator()
        {
            CssClass = "form-control-error";
        }
    }
}
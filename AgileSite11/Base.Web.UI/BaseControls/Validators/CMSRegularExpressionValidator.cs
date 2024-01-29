using System;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// CMSRegularExpressionValidator, inherited from System.Web.UI.WebControls.RegularExpressionValidator. Handles generally design of regular expression field validator.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSRegularExpressionValidator : RegularExpressionValidator
    {
        /// <summary>
        /// Initializes default CSS class for the validator.
        /// </summary>
        public CMSRegularExpressionValidator()
        {
            CssClass = "form-control-error";
        }
    }
}
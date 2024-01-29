using System;
using System.ComponentModel;
using System.Web.UI.WebControls;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// CMSRangeValidator, inherited from System.Web.UI.WebControls.RangeValidator. Handles generally design of range field validator.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSRangeValidator : RangeValidator
    {
        /// <summary>
        /// Initializes default CSS class for the validator.
        /// </summary>
        public CMSRangeValidator()
        {
            CssClass = "form-control-error";
        }
    }
}
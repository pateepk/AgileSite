using System;
using System.Globalization;
using System.Web.UI.Design;

namespace CMS.CKEditor.Web.UI
{
    /// <summary>
    /// CKEditor designer class for visual studio integration.
    /// </summary>
    public class CKEditorDesigner : ControlDesigner
    {
        #region "Public methods"

        /// <summary>
        /// Gets the HTML code for the design time
        /// </summary>
        public override string GetDesignTimeHtml()
        {
            CKEditorControl control = (CKEditorControl)Component;
            return String.Format(CultureInfo.InvariantCulture,
                                 "<table width=\"{0}\" height=\"{1}\" bgcolor=\"#f5f5f5\" bordercolor=\"#c7c7c7\" cellpadding=\"0\" cellspacing=\"0\" border=\"1\"><tr><td valign=\"middle\" align=\"center\"><h3>CKEditor - <em>'{2}'</em></h3></td></tr></table>",
                                 control.Width.Value == 0 ? "100%" : control.Width.ToString(),
                                 control.Height,
                                 control.ID);
        }

        #endregion
    }
}
using System;
using System.ComponentModel;

using CMS.Base.Web.UI;
using CMS.CKEditor.Web.UI;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// HTML editor control.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSHtmlEditor : CKEditorControl, IInputControl
    {
        /// <summary>
        /// PreRender event handler
        /// </summary>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            ScriptHelper.RegisterModule(Page, "CMS/Editor");
        }
    }
}

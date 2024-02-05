using System;
using System.ComponentModel;
using System.Web.UI.WebControls;
using System.Web.UI;

using CMS.Base.Web.UI;
using CMS.Helpers;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// File upload control.
    /// </summary>
    [ToolboxItem(false)]
    public class CMSFileUpload : FileUpload
    {
        /// <summary>
        /// Load event handler.
        /// </summary>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            // Add client script to ensure the proper encoding type
            if (ControlsHelper.IsInAsyncPostback(Page))
            {
                ScriptHelper.RegisterClientScriptBlock(Page, typeof(string), "UploadMultipart", ScriptHelper.GetScript("document.forms[0].encoding = 'multipart/form-data';"));
            }
        }
    }
}

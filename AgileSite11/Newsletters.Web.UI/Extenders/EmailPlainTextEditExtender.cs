using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.Newsletters.Web.UI;
using CMS.PortalEngine.Web.UI;
using CMS.UIControls;

[assembly: RegisterCustomClass("EmailPlainTextEditExtender", typeof(EmailPlainTextEditExtender))]

namespace CMS.Newsletters.Web.UI
{
    /// <summary>
    /// Newsletter issue plain text UIForm extender.
    /// </summary>
    public class EmailPlainTextEditExtender : ControlExtender<UIForm>
    {
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            Control.Page.Load += Page_Load;
        }


        private void BtnSaveAndClose_Click(object sender, EventArgs e)
        {
            Control.SaveData(null);
        }


        private void Page_Load(object sender, EventArgs eventArgs)
        {
            var page = Control.Page as CMSUIPage;
            if (page != null)
            {
                var btnSaveAndClose = new LocalizedButton();

                btnSaveAndClose.ResourceString = "general.saveandclose";
                btnSaveAndClose.Click += BtnSaveAndClose_Click;
                Control.SubmitButton.Visible = false;

                if (page.IsDialog)
                {
                    page.DialogFooter.AddControl(btnSaveAndClose);
                }
            }
        }
    }
}

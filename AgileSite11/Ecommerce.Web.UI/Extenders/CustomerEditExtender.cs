using System;

using CMS;
using CMS.Base.Web.UI;
using CMS.PortalEngine.Web.UI;

[assembly: RegisterCustomClass("CustomerEditExtender", typeof(CMS.Ecommerce.Web.UI.CustomerEditExtender))]

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Extender for Customer edit - General tab
    /// </summary>
    public class CustomerEditExtender : ControlExtender<UIForm>
    {
        /// <summary>
        /// Initializes the extender.
        /// </summary>
        public override void OnInit()
        {
            if (Control != null)
            {
                Control.OnAfterSave += Control_OnAfterSave;
            }
        }


        /// <summary>
        /// Refresh breadcrumbs after save, to ensure correct display text
        /// </summary>
        void Control_OnAfterSave(object sender, EventArgs e)
        {
            var customer = Control.EditedObject as CustomerInfo;

            if (customer != null)
            {
                ScriptHelper.RefreshTabHeader(Control.Page, customer.CustomerInfoName);
            }
        }
    }
}
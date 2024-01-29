using System;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.ContactManagement.Web.UI;
using CMS.FormEngine;
using CMS.UIControls;
using CMS.WorkflowEngine;

[assembly: RegisterCustomClass("ContactTabsExtender", typeof(ContactTabsExtender))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Extender for contact detail tabs UIElement.
    /// </summary>
    public class ContactTabsExtender : UITabsExtender<UITabs>
    {
        private ContactInfo Contact
        {
            get
            {
                return Control.UIContext.EditedObject as ContactInfo;
            }
        }


        /// <summary>
        /// OnInit event handler.
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();
            if (Contact == null)
            {
                return;
            }

            Control.Page.Load += Page_Load;
        }


        /// <summary>
        /// Initialization of tabs.
        /// </summary>
        public override void OnInitTabs()
        {
            if (Contact == null)
            {
                return;
            }

            Control.OnTabCreated += OnTabCreated;
        }


        private void Page_Load(object sender, EventArgs e)
        {
            // Check permission read
            AuthorizationHelper.AuthorizedReadContact(true);
        }


        private void OnTabCreated(object sender, TabCreatedEventArgs e)
        {
            if (e.Tab == null)
            {
                return;
            }

            switch (e.Tab.TabName.ToLowerCSafe())
            {
                case "contactcustomfields":
                    // Check if contact has any custom fields
                    var formInfo = FormHelper.GetFormInfo("OM.Contact", false);
                    if (!formInfo.GetFields(true, false, false).Any())
                    {
                        e.Tab = null;
                    }
                    break;

                case "contactaccounts":
                    if (!AuthorizationHelper.AuthorizedReadContact(false))
                    {
                        e.Tab = null;
                    }
                    break;

                case "contactprocesses":
                    // Marketing automation
                    if (!WorkflowInfoProvider.IsMarketingAutomationAllowed())
                    {
                        e.Tab = null;

                    }
                    break;
            }
        }
    }
}

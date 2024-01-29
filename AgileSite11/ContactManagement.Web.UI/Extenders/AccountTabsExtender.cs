using System;
using System.Linq;

using CMS;
using CMS.Base;
using CMS.ContactManagement.Web.UI;
using CMS.FormEngine;
using CMS.UIControls;

[assembly: RegisterCustomClass("AccountTabsExtender", typeof(AccountTabsExtender))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Extender for account detail tabs UIElement.
    /// </summary>
    public class AccountTabsExtender : UITabsExtender
    {
        private AccountInfo Account
        {
            get
            {
                return Control.UIContext.EditedObject as AccountInfo;
            }
        }


        /// <summary>
        /// OnInit event handler.
        /// </summary>
        public override void OnInit()
        {
            base.OnInit();
            if (Account == null)
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
            if (Account == null)
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
                case "account.customfields":
                    // Check if contact has any custom fields
                    var formInfo = FormHelper.GetFormInfo("OM.Account", false);
                    if (!formInfo.GetFields(true, false, false).Any())
                    {
                        e.Tab = null;
                    }
                    break;

                case "account.contacts":
                    // Display contacts tab only if user is authorized to read contacts
                    if (!AuthorizationHelper.AuthorizedReadContact(false))
                    {
                        e.Tab = null;
                    }
                    break;
            }
        }
    }
}

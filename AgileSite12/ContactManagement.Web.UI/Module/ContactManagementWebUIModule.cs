using CMS;
using CMS.ContactManagement.Web.UI;
using CMS.ContactManagement.Web.UI.Internal;
using CMS.Core;
using CMS.DataEngine;
using CMS.PortalEngine.Internal;

[assembly: RegisterModule(typeof(ContactManagementWebUIModule))]

namespace CMS.ContactManagement.Web.UI
{
    /// <summary>
    /// Represents the Contact management Web UI module.
    /// </summary>
    internal class ContactManagementWebUIModule : Module
    {
        /// <summary>
        /// Instantiates new instance of <see cref="ContactManagementWebUIModule"/>.
        /// </summary>
        public ContactManagementWebUIModule()
            : base(new ModuleMetadata("CMS.ContactManagement.Web.UI"))
        {
        }


        /// <summary>
        /// Initializes the module.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            RegisterContactDetailAccountResolver();
        }


        /// <summary>
        /// Registers <see cref="ContactDetailsAccountResolver"/> for resolving the contact account detail field. 
        /// </summary>
        private void RegisterContactDetailAccountResolver()
        {
            var uiLinkProvider = Service.Resolve<IUILinkProvider>();
            var contactDetailAccountResolver = new ContactDetailsAccountResolver(uiLinkProvider);
            var contactDetailControllerService = Service.Resolve<IContactDetailsControllerService>();

            contactDetailControllerService.RegisterContactDetailsFieldResolver("ContactAccounts", contactDetailAccountResolver);
        }
    }
}
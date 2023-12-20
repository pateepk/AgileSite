using System.Web.UI;
using System.Web.UI.WebControls;

using CMS.Base;
using CMS.Core;
using CMS.Membership;

namespace CMS.Ecommerce.Web.UI
{
    /// <summary>
    /// Base page for the E-commerce store settings pages to apply global settings to the pages.
    /// </summary>
    public class CMSEcommerceStoreSettingsPage : CMSEcommerceConfigurationPage, IPostBackEventHandler
    {
        /// <summary>
        /// Check whether the user is authorized to modify configuration and returns true if so. 
        /// This method needs to be overridden to save store settings.
        /// </summary>
        protected virtual void CheckPermissionsAndSave()
        {
            // Check 'EcommerceModify' permission
            if (!MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_MODIFY))
            {
                RedirectToAccessDenied(ModuleName.ECOMMERCE, EcommercePermissions.CONFIGURATION_MODIFY);
                return;
            }

            SaveChanges();
        }


        /// <summary>
        /// Override this method to handle saving of store settings.
        /// </summary>
        protected virtual void SaveChanges()
        {
        }


        #region "Event Handlers"

        /// <summary>
        /// Handles default actions performed on the master header (Save changes action).
        /// </summary>
        protected virtual void StoreSettingsActions_ActionPerformed(object sender, CommandEventArgs e)
        {
            switch (e.CommandName.ToLowerCSafe())
            {
                case "save":
                    CheckPermissionsAndSave();
                    break;
            }
        }


        /// <summary>
        /// Handles postback event with argument "save". Permissions are checked and method SaveChanges() 
        /// is called as reaction.
        /// </summary>
        /// <param name="eventArgument">Postback argument</param>
        public virtual void RaisePostBackEvent(string eventArgument)
        {
            if (eventArgument.ToLowerCSafe() == "save")
            {
                CheckPermissionsAndSave();
            }
        }

        #endregion
    }
}
using CMS.DataEngine;
using CMS.MacroEngine;

namespace CMS.Ecommerce
{
    /// <summary>
    /// Provides handlers for Ecommerce and MacroEngine modules interaction.
    /// </summary>
    internal static class MacroEngineHandlers
    {
        /// <summary>
        /// Initializes the macro rules handlers.
        /// </summary>
        public static void Init()
        {
            MacroRuleInfo.TYPEINFO.Events.CheckPermissions.Before += CheckPermissions_Before;
        }


        /// <summary>
        /// Checks permissions for e-commerce macro rules.
        /// </summary>
        private static void CheckPermissions_Before(object sender, ObjectSecurityEventArgs e)
        {
            var rule = e.Object as MacroRuleInfo;
            var permission = e.Permission;

            if (rule != null)
            {
                // Special permission check for e-commerce macro rules
                switch (rule.MacroRuleResourceName.ToLowerInvariant())
                {
                    case "com.catalogdiscount":
                    case "com.orderdiscount":
                        {
                            switch (permission)
                            {
                                case PermissionsEnum.Read:
                                    e.Result = ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.CONFIGURATION_READ, e.SiteName, e.User).ToAuthorizationResultEnum();

                                    break;

                                case PermissionsEnum.Create:
                                case PermissionsEnum.Delete:
                                case PermissionsEnum.Modify:
                                case PermissionsEnum.Destroy:
                                    e.Result = ECommerceHelper.IsUserAuthorizedForPermission(EcommercePermissions.CONFIGURATION_MODIFYGLOBAL, e.SiteName, e.User).ToAuthorizationResultEnum();

                                    break;
                            }

                            // Skip default check
                            e.Cancel();
                        }

                        break;
                }
            }
        }
    }
}

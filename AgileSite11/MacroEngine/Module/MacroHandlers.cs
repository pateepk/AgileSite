using CMS.Base;
using CMS.DataEngine;

namespace CMS.MacroEngine
{
    internal static class MacroHandlers
    {
        /// <summary>
        /// Pre-initialization of the module.
        /// </summary>
        public static void PreInit()
        {
            SqlInstallationHelper.RunQuery.Before += EnsureValidMacrosWithinInstallation;
        }


        /// <summary>
        /// Runs before query execution within installation.
        /// </summary>
        private static void EnsureValidMacrosWithinInstallation(object sender, QueryEventArgs e)
        {
            // Refresh security parameters, so the default data imported to DB are correctly signed
            // In this case we must use the identity name for administrator because identities are not yet available within installation
            e.Query = MacroSecurityProcessor.AddSecurityParameters(e.Query, new MacroIdentityOption { IdentityName = MacroIdentityInfoProvider.DEFAULT_GLOBAL_ADMINISTRATOR_IDENTITY_NAME }, null);
        }
    }
}

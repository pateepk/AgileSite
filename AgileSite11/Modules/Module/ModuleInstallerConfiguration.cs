using System;
using System.Linq;
using System.Text;

using CMS.Base;


namespace CMS.Modules.Internal
{
    /// <summary>
    /// Holds configuration information for module installation.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public static class ModuleInstallerConfiguration
    {
        /// <summary>
        /// Gets or sets function providing user for the purpose of module installation.
        /// </summary>
        /// <remarks>
        /// The provided user is used in the process of macro signing.
        /// </remarks>
        public static Func<IUserInfo> ModuleInstallerUserProvider
        {
            get;
            set;
        }


        /// <summary>
        /// Gets user to be used in module installation.
        /// </summary>
        internal static IUserInfo GetInstallationUser()
        {
            return (ModuleInstallerUserProvider != null) ? ModuleInstallerUserProvider() : null;
        }
    }
}

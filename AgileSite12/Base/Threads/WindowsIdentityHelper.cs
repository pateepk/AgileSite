using System;
using System.Security.Principal;

namespace CMS.Base
{
    /// <summary>
    /// Helper class for managing Windows identity impersonation.
    /// </summary>
    public static class WindowsIdentityHelper
    {
#if NETFULLFRAMEWORK
        /// <summary>
        /// Run given <paramref name="action"/> under user represented by <paramref name="identity"/>.
        /// </summary>
        /// <param name="identity">Represents user used for impersonation.</param>
        /// <param name="action">Action to run.</param>
        public static void Impersonate(WindowsIdentity identity, Action action)
        {
            try
            {
                using (identity.Impersonate())
                {
                    action();
                }
            }
            // Suppress exceptions during impersonation
            catch
            {
            }
        }
#endif

#if NETSTANDARD
        /// <summary>
        /// Run given <paramref name="action"/> under user represented by <paramref name="identity"/>.
        /// </summary>
        /// <param name="identity">Represents user used for impersonation.</param>
        /// <param name="action">Action to run.</param>
        public static void Impersonate(WindowsIdentity identity, Action action)
        {
            WindowsIdentity.RunImpersonated(identity.AccessToken, action);
        }
#endif
    }
}

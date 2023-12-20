using System;
using System.Security.Principal;

using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Provides a principal object retrieved from the virtual context.
    /// </summary>
    internal class VirtualContextPrincipal : IPrincipal
    {
        /// <summary>
        /// Determines whether the current principal belongs to the specified role.
        /// </summary>
        /// <param name="role">The name of the role for which to check membership.</param>
        /// <returns>
        /// true if the current principal is a member of the specified role; otherwise, false.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">Throws when the <see cref="VirtualContext"/> is not initialized.</exception>
        public bool IsInRole(string role)
        {
            if (!VirtualContext.IsInitialized)
            {
                throw new InvalidOperationException("Virtual context is not initialized");
            }

            return MembershipContext.AuthenticatedUser.IsInRole(role, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Gets the identity of the current principal.
        /// </summary>
        public IIdentity Identity
        {
            get;
        } = new GenericIdentity(ValidationHelper.GetString(VirtualContext.GetItem(VirtualContext.PARAM_USERNAME), String.Empty));
    }
}

using System;
using System.Configuration.Provider;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Security;
using System.Web.UI;

namespace CMS.Membership
{
    internal class WindowsTokenRoleService : IWindowsTokenRoleService
    {
        /// <summary>Gets a list of the Windows groups that a user is in.</summary>
        /// <returns>A string array containing the names of all the Windows groups that the specified user is in.</returns>
        /// <param name="userName">The user to return the list of Windows groups for in the form DOMAIN\userName. </param>
        /// <exception cref="ProviderException">
        /// The currently executing user does not have an authenticated <see cref="WindowsIdentity" /> attached to <see cref="Page.User" />. 
        /// For non-HTTP scenarios, the currently executing user does not have an authenticated <see cref="WindowsIdentity" /> attached to <see cref="Thread.CurrentPrincipal" />.
        /// -or-
        /// <paramref name="userName" /> does not match the <see cref="WindowsIdentity.Name" /> of the current <see cref="WindowsIdentity" />.
        /// -or-
        /// A failure occurred while retrieving the user's Windows group information.
        /// </exception>
        /// <exception cref="ArgumentNullException"><paramref name="userName" /> is null.</exception>
        /// <exception cref="HttpException">The trust level is less than <see cref="AspNetHostingPermissionLevel.Low" />.</exception>
        public string[] GetRolesForUser(string userName)
        {
            return new WindowsTokenRoleProvider().GetRolesForUser(userName);
        }
    }
}
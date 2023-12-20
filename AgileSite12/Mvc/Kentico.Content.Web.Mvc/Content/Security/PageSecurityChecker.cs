using CMS.DataEngine;
using CMS.Membership;

namespace Kentico.Content.Web.Mvc
{
    /// <summary>
    /// Checks page permissions for Page builder feature.
    /// </summary>
    internal sealed class PageSecurityChecker : IPageSecurityChecker
    {
        private readonly IVirtualContextPageRetriever retriever;


        /// <summary>
        /// Creates an instance of <see cref="PageSecurityChecker"/>.
        /// </summary>
        /// <param name="retriever">Retrieves page from virtual context.</param>
        public PageSecurityChecker(IVirtualContextPageRetriever retriever)
        {
            this.retriever = retriever;
        }


        /// <summary>
        /// Checks page permissions.
        /// </summary>
        /// <param name="permission">Permission to check.</param>
        /// <returns><c>True</c> if current user is granted with given permissions.</returns>
        public bool Check(PermissionsEnum permission)
        {
            var page = retriever.Retrieve();
            if (page == null)
            {
                return false;
            }

            return page.CheckPermissions(permission, page.NodeSiteName, MembershipContext.AuthenticatedUser);
        }
    }
}
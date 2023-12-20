using System;

using CMS.Core;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Encapsulates methods related to community group security.
    /// </summary>
    internal class GroupSecurityHelper
    {
        /// <summary>
        /// Returns true if given <paramref name="user"/> has <paramref name="permissionName"/> permission for <see cref="ModuleName.GROUPS"/> module on the site given by <paramref name="siteId"/>
        /// parameter or is an administrator of the group given by <paramref name="groupId"/> parameter. If given <paramref name="siteId"/> represents different site than 
        /// the site that the group is assigned to, it returns false.
        /// </summary>
        /// <param name="user">User</param>
        /// <param name="groupId">Group ID</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteId">Site ID</param>
        public static bool IsUserAuthorizedPerGroup(UserInfo user, int groupId, string permissionName, int siteId)
        {
            if (user == null)
            {
                return false;
            }

            var group = GroupInfoProvider.GetGroupInfo(groupId);

            if (group == null)
            {
                return false;
            }

            var site = SiteInfoProvider.GetSiteInfo(siteId);

            if (site == null)
            {
                return false;
            }

            if (group.GroupSiteID != siteId)
            {
                return false;
            }

            if (user.IsGroupAdministrator(groupId))
            {
                return true;
            }

            if (user.IsAuthorizedPerResource(ModuleName.GROUPS, permissionName, site.SiteName))
            {
                return true;
            }

            return false;
        }
    }
}

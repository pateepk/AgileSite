using CMS.Core;
using CMS.Membership;

namespace CMS.UIControls
{
    /// <summary>
    /// Container for arguments of <see cref="CMSDeskPage.OnCheckGroupPermissions"/> event handler.
    /// </summary>
    public class CheckGroupPermissionArgs
    {
        /// <summary>
        /// User whose authorization to community group is examined.
        /// </summary>
        public UserInfo User { get; }


        /// <summary>
        /// ID of the community group.
        /// </summary>
        public int GroupId { get; }


        /// <summary>
        /// Name of permission which the user should have for <see cref="ModuleName.GROUPS"/> module.
        /// </summary>
        public string PermissionName { get; }


        /// <summary>
        /// Site ID.
        /// </summary>
        public int SiteId { get; }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="user">User info</param>
        /// <param name="groupId">Group ID</param>
        /// <param name="permissionName">Permission name</param>
        /// <param name="siteId">Site ID</param>
        public CheckGroupPermissionArgs(UserInfo user, int groupId, string permissionName, int siteId)
        {
            User = user;
            GroupId = groupId;
            PermissionName = permissionName;
            SiteId = siteId;
        }
    }
}

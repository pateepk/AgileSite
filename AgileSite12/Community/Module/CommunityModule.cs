using System;

using CMS;
using CMS.Base;
using CMS.Community;
using CMS.DataEngine;
using CMS.DocumentEngine.Internal;
using CMS.MacroEngine;
using CMS.Membership;

[assembly: RegisterModule(typeof(CommunityModule))]

namespace CMS.Community
{
    /// <summary>
    /// Represents the Community module.
    /// </summary>
    public class CommunityModule : Module
    {
        #region "Constants"

        /// <summary>
        /// Name of email template type for group member.
        /// </summary>
        public const string GROUP_MEMBER_EMAIL_TEMPLATE_TYPE_NAME = "groupmember";


        /// <summary>
        /// Name of email template type for group invitation.
        /// </summary>
        public const string GROUP_INVITATION_EMAIL_TEMPLATE_TYPE_NAME = "groupinvitation";


        /// <summary>
        /// Name of email template type for group member invitation.
        /// </summary>
        public const string GROUP_MEMBER_INVITATION_EMAIL_TEMPLATE_TYPE_NAME = "groupmemberinvitation";

        #endregion


        /// <summary>
        /// Default constructor
        /// </summary>
        public CommunityModule()
            : base(new CommunityModuleMetadata())
        {
        }


        /// <summary>
        /// Initializes the module
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();

            UserSiteInfo.TYPEINFO.Events.Delete.After += RemoveUsersFromSiteGroups;
            DocumentNodeDataInfo.TYPEINFO.Events.Delete.After += ClearGroupPageLocation;

            ImportSpecialActions.Init();

            RegisterContext<CommunityContext>("CommunityContext");

            ExtendList<MacroResolverStorage, MacroResolver>.With("GroupMemberResolver").WithLazyInitialization(() => CommunityResolvers.GroupMemberResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("GroupMemberInvitationResolver").WithLazyInitialization(() => CommunityResolvers.GroupMemberInvitationResolver);
            ExtendList<MacroResolverStorage, MacroResolver>.With("GroupInvitationResolver").WithLazyInitialization(() => CommunityResolvers.GroupInvitationResolver);
        }


        private void RemoveUsersFromSiteGroups(object sender, ObjectEventArgs args)
        {
            // Removes all user's groups of specified site when he's removed from it
            var userSiteInfo = (UserSiteInfo)args.Object;

            GroupMemberInfoProvider.RemoveUsersFromSiteGroups(userSiteInfo.UserID, userSiteInfo.SiteID);
        }


        private void ClearGroupPageLocation(object sender, ObjectEventArgs args)
        {
            var nodeData = (DocumentNodeDataInfo)args.Object;

            GroupInfoProvider.ClearGroupPageLocation(nodeData.NodeSiteID, nodeData.NodeGUID);
        }


        /// <summary>
        /// Registers the object type of this module
        /// </summary>
        protected override void RegisterCommands()
        {
            base.RegisterCommands();

            RegisterCommand("GetUserGroups", GetUserGroups);
            RegisterCommand("GetGroupInfo", GetGroupInfo);
            RegisterCommand("GetGroupInfoByGuid", GetGroupInfoByGuid);
            RegisterCommand("GetGroupInfoByName", GetGroupInfoByName);
            RegisterCommand("GetGroupProfilePath", GetGroupProfilePath);
            RegisterCommand("GetMemberProfilePath", GetMemberProfilePath);
            RegisterCommand("GetGroupManagementPath", GetGroupManagementPath);
            RegisterCommand("GetMemberManagementPath", GetMemberManagementPath);
            RegisterCommand("SiteHasGroup", SiteHasGroup);
            RegisterCommand("CheckGroupPermission", CheckGroupPermission);
            RegisterCommand("IsMemberOfGroup", IsMemberOfGroup);
        }


        /// <summary>
        /// Gets user groups
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static InfoDataSet<GroupMemberInfo> GetUserGroups(object[] parameters)
        {
            int memberUserId = (int)parameters[0];
            string columns = string.Empty;
            if (parameters.Length == 2)
            {
                columns = (string)parameters[1];
            }

            // Get group members
            var where = new WhereCondition().WhereEquals("MemberStatus", 0).WhereEquals("MemberUserID", memberUserId);
            return GroupMemberInfoProvider.GetGroupMembers().Where(where).Columns(columns).TypedResult;
        }


        /// <summary>
        /// Gets group info by ID
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static GroupInfo GetGroupInfo(object[] parameters)
        {
            int groupId = (int)parameters[0];

            return GroupInfoProvider.GetGroupInfo(groupId);
        }


        /// <summary>
        /// Gets group info by GUID
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static GroupInfo GetGroupInfoByGuid(object[] parameters)
        {
            Guid groupGuid = (Guid)parameters[0];

            return GroupInfoProvider.GetGroupInfo(groupGuid);
        }


        /// <summary>
        /// Gets group info by name
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static GroupInfo GetGroupInfoByName(object[] parameters)
        {
            string groupName = (string)parameters[0];
            string siteName = (string)parameters[1];

            return GroupInfoProvider.GetGroupInfo(groupName, siteName);
        }


        /// <summary>
        /// Gets group profile path
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static string GetGroupProfilePath(object[] parameters)
        {
            string objectName = (string)parameters[0];
            string siteName = (string)parameters[1];

            return GroupInfoProvider.GetGroupProfilePath(objectName, siteName);
        }


        /// <summary>
        /// Gets member profile path
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static string GetMemberProfilePath(object[] parameters)
        {
            string objectName = (string)parameters[0];
            string siteName = (string)parameters[1];

            return GroupMemberInfoProvider.GetMemberProfilePath(objectName, siteName);
        }


        /// <summary>
        /// Gets group management path
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static string GetGroupManagementPath(object[] parameters)
        {
            string objectName = (string)parameters[0];
            string siteName = (string)parameters[1];

            return GroupInfoProvider.GetGroupManagementPath(objectName, siteName);
        }


        /// <summary>
        /// Gets member management path
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static string GetMemberManagementPath(object[] parameters)
        {
            string objectName = (string)parameters[0];
            string siteName = (string)parameters[1];

            return GroupMemberInfoProvider.GetMemberManagementPath(objectName, siteName);
        }


        /// <summary>
        /// Checks whether the site has groups
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object SiteHasGroup(object[] parameters)
        {
            int siteId = (int)parameters[0];
            var siteGroups = GroupInfoProvider.GetGroups().WhereEquals("GroupSiteID", siteId);

            return (siteGroups.Count > 0);
        }


        /// <summary>
        /// Check the group permission
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object CheckGroupPermission(object[] parameters)
        {
            string permissionName = (string)parameters[0];
            int groupId = (int)parameters[1];

            return GroupInfoProvider.CheckPermission(permissionName, groupId);
        }


        /// <summary>
        /// Checks whether the user is member of group
        /// </summary>
        /// <param name="parameters">Parameters array</param>
        private static object IsMemberOfGroup(object[] parameters)
        {
            int userId = (int)parameters[0];
            int groupId = (int)parameters[1];

            return GroupMemberInfoProvider.IsMemberOfGroup(userId, groupId);
        }
    }
}
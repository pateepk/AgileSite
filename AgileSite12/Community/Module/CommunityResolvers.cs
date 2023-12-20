using CMS.DataEngine;
using CMS.MacroEngine;
using CMS.Membership;

namespace CMS.Community
{
    /// <summary>
    /// Resolvers used in e-mail templates and other macro visual components.
    /// </summary>
    public class CommunityResolvers : ResolverDefinition
    {
        #region "Variables"

        private static MacroResolver mGroupMemberResolver = null;
        private static MacroResolver mGroupsInvitationResolver = null;
        private static MacroResolver mGroupsAcceptedInvitationResolver = null;

        #endregion


        /// <summary>
        /// Groups e-mail template macro resolver.
        /// </summary>
        public static MacroResolver GroupMemberResolver
        {
            get
            {
                if (mGroupMemberResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Group", ModuleManager.GetReadOnlyObject(GroupInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("MemberUser", ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE));

                    mGroupMemberResolver = resolver;
                }

                return mGroupMemberResolver;
            }
        }


        /// <summary>
        /// Groups invitation e-mail template macro resolver.
        /// </summary>
        public static MacroResolver GroupInvitationResolver
        {
            get
            {
                if (mGroupsInvitationResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Invitation", ModuleManager.GetReadOnlyObject("community.invitation"));
                    resolver.SetNamedSourceData("Group", ModuleManager.GetReadOnlyObject(GroupInfo.OBJECT_TYPE));

                    RegisterStringValues(resolver, new[] { "AcceptionURL", "InvitedBy" });

                    mGroupsInvitationResolver = resolver;
                }

                return mGroupsInvitationResolver;
            }
        }


        /// <summary>
        /// Groups accepted invitation e-mail template macro resolver.
        /// </summary>
        public static MacroResolver GroupMemberInvitationResolver
        {
            get
            {
                if (mGroupsAcceptedInvitationResolver == null)
                {
                    MacroResolver resolver = MacroResolver.GetInstance();

                    resolver.SetNamedSourceData("Sender", ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Recipient", ModuleManager.GetReadOnlyObject(UserInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("GroupMember", ModuleManager.GetReadOnlyObject(GroupMemberInfo.OBJECT_TYPE));
                    resolver.SetNamedSourceData("Group", ModuleManager.GetReadOnlyObject(GroupInfo.OBJECT_TYPE));

                    mGroupsAcceptedInvitationResolver = resolver;
                }

                return mGroupsAcceptedInvitationResolver;
            }
        }
    }
}
using System;

using CMS.Community;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds groups API examples.
    /// </summary>
    /// <pageTitle>Groups</pageTitle>
    internal class GroupsMain
    {
        /// <summary>
        /// Holds groups API examples.
        /// </summary>
        /// <groupHeading>Groups</groupHeading>
        private class Groups
        {
            /// <heading>Creating a group</heading>
            private void CreateGroup()
            {
                // Creates a new group object
                GroupInfo newGroup = new GroupInfo();

                // Sets the properties for the group
                newGroup.GroupDisplayName = "New group";
                newGroup.GroupName = "NewGroup";
                newGroup.GroupSiteID = SiteContext.CurrentSiteID;
                newGroup.GroupDescription = "";
                newGroup.GroupApproveMembers = GroupApproveMembersEnum.AnyoneCanJoin;
                newGroup.GroupAccess = SecurityAccessEnum.AllUsers;
                newGroup.GroupApproved = true;
                newGroup.GroupApprovedByUserID = MembershipContext.CurrentUserProfile.UserID;
                newGroup.GroupCreatedByUserID = MembershipContext.CurrentUserProfile.UserID;
                newGroup.AllowCreate = SecurityAccessEnum.GroupMembers;
                newGroup.AllowDelete = SecurityAccessEnum.GroupMembers;
                newGroup.AllowModify = SecurityAccessEnum.GroupMembers;
                newGroup.GroupNodeGUID = Guid.Empty;

                // Saves the group to the database
                GroupInfoProvider.SetGroupInfo(newGroup);
            }


            /// <heading>Updating a group</heading>
            private void GetAndUpdateGroup()
            {
                // Gets the group
                GroupInfo updateGroup = GroupInfoProvider.GetGroupInfo("NewGroup", SiteContext.CurrentSiteName);
                if (updateGroup != null)
                {
                    // Updates the group properties
                    updateGroup.GroupDisplayName = updateGroup.GroupDisplayName.ToLowerInvariant();

                    // Saves the changes to the database
                    GroupInfoProvider.SetGroupInfo(updateGroup);
                }
            }


            /// <heading>Updating multiple groups</heading>
            private void GetAndBulkUpdateGroups()
            {
                // Gets the groups whose name starts with 'NewGroup'
                var groups = GroupInfoProvider.GetGroups().WhereStartsWith("GroupName", "NewGroup");

                // Loops through the group objects
                foreach (GroupInfo group in groups)
                {
                    // Updates the group properties
                    group.GroupDisplayName = group.GroupDisplayName.ToUpper();

                    // Saves the changes to the database
                    GroupInfoProvider.SetGroupInfo(group);
                }
            }


            /// <heading>Deleting a group</heading>
            private void DeleteGroup()
            {
                // Gets the group
                GroupInfo deleteGroup = GroupInfoProvider.GetGroupInfo("NewGroup", SiteContext.CurrentSiteName);

                if (deleteGroup != null)
                {
                    // Deletes the group
                    GroupInfoProvider.DeleteGroupInfo(deleteGroup);
                }
            }
        }


        /// <summary>
        /// Holds group member API examples.
        /// </summary>
        /// <groupHeading>Group members</groupHeading>
        private class GroupMembers
        {
            /// <heading>Creating a group member</heading>
            private void CreateGroupMember()
            {
                // Gets the group
                GroupInfo group = GroupInfoProvider.GetGroupInfo("NewGroup", SiteContext.CurrentSiteName);

                if (group != null)
                {
                    // Gets the user
                    UserInfo user = UserInfoProvider.GetUserInfo("Andy");

                    if (user != null)
                    {
                        // Creates a group member object
                        GroupMemberInfo newMember = new GroupMemberInfo();

                        // Sets the member properties
                        newMember.MemberGroupID = group.GroupID;
                        newMember.MemberApprovedByUserID = MembershipContext.CurrentUserProfile.UserID;
                        newMember.MemberApprovedWhen = DateTime.Now;
                        newMember.MemberInvitedByUserID = MembershipContext.CurrentUserProfile.UserID;
                        newMember.MemberUserID = user.UserID;
                        newMember.MemberJoined = DateTime.Now;
                        newMember.MemberComment = "New member added through the API.";

                        // Saves the member to the database
                        GroupMemberInfoProvider.SetGroupMemberInfo(newMember);
                    }
                }
            }


            /// <heading>Updating a group member</heading>
            private void GetAndUpdateGroupMember()
            {
                // Gets the group 
                GroupInfo group = GroupInfoProvider.GetGroupInfo("NewGroup", SiteContext.CurrentSiteName);

                if (group != null)
                {
                    // Gets the user
                    UserInfo user = UserInfoProvider.GetUserInfo("Andy");

                    if (user != null)
                    {
                        // Gets the group member matching the user
                        GroupMemberInfo updateMember = GroupMemberInfoProvider.GetGroupMemberInfo(user.UserID, group.GroupID);
                        if (updateMember != null)
                        {
                            // Updates the member properties
                            updateMember.MemberComment = updateMember.MemberComment.ToLowerInvariant();

                            // Saves the changes to the database
                            GroupMemberInfoProvider.SetGroupMemberInfo(updateMember);
                        }
                    }
                }
            }


            /// <heading>Updating multiple group members</heading>
            private void GetAndBulkUpdateGroupMembers()
            {
                // Gets the group 
                GroupInfo group = GroupInfoProvider.GetGroupInfo("NewGroup", SiteContext.CurrentSiteName);

                if (group != null)
                {
                    // Gets the group members
                    ObjectQuery<GroupMemberInfo> members = GroupMemberInfoProvider.GetGroupMembers().WhereEquals("MemberGroupID", group.GroupID);

                    // Loops through individual group members
                    foreach (GroupMemberInfo member in members)
                    {
                        // Updates the member properties
                        member.MemberComment = member.MemberComment.ToUpper();

                        // Saves the changes to the database
                        GroupMemberInfoProvider.SetGroupMemberInfo(member);
                    }
                }
            }


            /// <heading>Deleting a group member</heading>
            private void DeleteGroupMember()
            {
                // Gets the group 
                GroupInfo group = GroupInfoProvider.GetGroupInfo("NewGroup", SiteContext.CurrentSiteName);

                if (group != null)
                {
                    // Gets the user
                    UserInfo user = UserInfoProvider.GetUserInfo("Andy");

                    if (user != null)
                    {
                        // Gets the group member matching the user
                        GroupMemberInfo deleteMember = GroupMemberInfoProvider.GetGroupMemberInfo(user.UserID, group.GroupID);

                        if (deleteMember != null)
                        {
                            // Deletes the group member
                            GroupMemberInfoProvider.DeleteGroupMemberInfo(deleteMember);
                        }
                    }
                }
            }


            /// <heading>Creating an invitation to a group</heading>
            private void CreateInvitation()
            {
                // Gets the group 
                GroupInfo group = GroupInfoProvider.GetGroupInfo("NewGroup", SiteContext.CurrentSiteName);

                if (group != null)
                {
                    // Creates a group invitation 
                    InvitationInfo newInvitation = new InvitationInfo();

                    // Sets invitation properties
                    newInvitation.InvitationComment = "Group invitation created through the API.";
                    newInvitation.InvitationGroupID = group.GroupID;
                    newInvitation.InvitationUserEmail = "user@localhost.local";
                    newInvitation.InvitedByUserID = MembershipContext.AuthenticatedUser.UserID;
                    newInvitation.InvitationCreated = DateTime.Now;
                    newInvitation.InvitationValidTo = DateTime.Now.AddDays(1);

                    // Saves the invitation to the database
                    InvitationInfoProvider.SetInvitationInfo(newInvitation);

                    // Sends the invitation email
                    InvitationInfoProvider.SendInvitationEmail(newInvitation, SiteContext.CurrentSiteName);
                }
            }
        }
    }
}

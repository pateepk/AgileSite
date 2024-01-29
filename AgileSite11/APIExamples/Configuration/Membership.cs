using System;
using System.Linq;

using CMS.Membership;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds membership API examples.
    /// </summary>
    /// <pageTitle>Membership</pageTitle>
    internal class Membership
    {
        /// <heading>Creating a new membership</heading>
        private void CreateMembership()
        {
            // Creates a new membership object
            MembershipInfo newMembership = new MembershipInfo();

            // Sets the membership properties
            newMembership.MembershipDisplayName = "New membership";
            newMembership.MembershipName = "NewMembership";
            newMembership.MembershipSiteID = SiteContext.CurrentSiteID;

            // Saves the membership to the database
            MembershipInfoProvider.SetMembershipInfo(newMembership);
        }


        /// <heading>Updating an existing membership</heading>
        private void GetAndUpdateMembership()
        {
            // Gets the membership
            MembershipInfo updateMembership = MembershipInfoProvider.GetMembershipInfo("NewMembership", SiteContext.CurrentSiteName);
            if (updateMembership != null)
            {
                // Updates the membership properties
                updateMembership.MembershipDisplayName = updateMembership.MembershipDisplayName.ToLower();

                // Saves the changes to the database
                MembershipInfoProvider.SetMembershipInfo(updateMembership);
            }
        }


        /// <heading>Updating multiple memberships</heading>
        private void GetAndBulkUpdateMemberships()
        {
            // Gets all memberships whose name starts with 'NewMembership'
            var memberships = MembershipInfoProvider.GetMemberships().WhereStartsWith("MembershipName", "NewMembership");

            // Loops through individual memberships
            foreach (MembershipInfo modifyMembership in memberships)
            {
                // Updates the  membership properties
                modifyMembership.MembershipDisplayName = modifyMembership.MembershipDisplayName.ToUpper();

                // Saves the changes
                MembershipInfoProvider.SetMembershipInfo(modifyMembership);
            }
        }


        /// <heading>Deleting a membership</heading>
        private void DeleteMembership()
        {
            // Gets the membership
            MembershipInfo deleteMembership = MembershipInfoProvider.GetMembershipInfo("NewMembership", SiteContext.CurrentSiteName);

            if (deleteMembership != null)
            {
                // Deletes the membership
                MembershipInfoProvider.DeleteMembershipInfo(deleteMembership);
            }
        }
        
        /// <heading>Including a role in a membership</heading>
        private void AddMembershipToRole()
        {
            // Gets role and membership objects
            RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteID);
            MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("NewMembership", SiteContext.CurrentSiteName);

            if ((role != null) && (membership != null))
            {              
                // Adds the role to the membership
                MembershipRoleInfoProvider.AddMembershipToRole(membership.MembershipID, role.RoleID);
            }
        }
        
        /// <heading>Removing a role from a membership</heading>
        private void RemoveMembershipFromRole()
        {
            // Gets role and membership objects
            RoleInfo role = RoleInfoProvider.GetRoleInfo("Rolename", SiteContext.CurrentSiteID);
            MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("NewMembership", SiteContext.CurrentSiteName);

            if ((role != null) && (membership != null))
            {
                // Removes the role from the membership
                MembershipRoleInfoProvider.RemoveMembershipFromRole(membership.MembershipID, role.RoleID);
            }
        }


        /// <heading>Assigning a membership to a user</heading>
        private void AddMembershipToUser()
        {
            // Gets user and membership objects
            UserInfo user = UserInfoProvider.GetUserInfo("Username");
            MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("NewMembership", SiteContext.CurrentSiteName);

            if ((user != null) && (membership != null))
            {
                // Grants the membership to the user (valid for 3 minutes)
                MembershipUserInfoProvider.AddMembershipToUser(membership.MembershipID, user.UserID, DateTime.Now.AddMinutes(3));
            }
        }


        /// <heading>Removing a membership from a user</heading>
        private void RemoveMembershipFromUser()
        {
            // Gets user and membership objects
            UserInfo user = UserInfoProvider.GetUserInfo("Username");
            MembershipInfo membership = MembershipInfoProvider.GetMembershipInfo("NewMembership", SiteContext.CurrentSiteName);

            if ((user != null) && (membership != null))
            {
                // Removes the membership from the user
                MembershipUserInfoProvider.RemoveMembershipFromUser(membership.MembershipID, user.UserID);
            }
        }


        /// <heading>Getting all valid memberships of a user</heading>
        private void GetValidMemberships()
        {
            // Gets the current user
            UserInfo user = MembershipContext.AuthenticatedUser;

            if (user != null)
            {
                // Gets all valid memberships assigned to the user.
                // Requires a using statement for the 'System.Linq' namespace.
                // The boolean parameter of the IsInMembership method is set to true,
                // so both memberships on the current site and global memberships are loaded.
                var memberships = MembershipInfoProvider.GetMemberships()
                    .Where(membership => user.IsInMembership(membership.MembershipName, SiteContext.CurrentSiteName, true));

                // Loops through the memberships
                foreach (MembershipInfo membership in memberships)
                {
                    // Process the membership
                }
            }
        }
    }
}

using System;

using CMS.Membership;
using CMS.Base;
using CMS.SiteProvider;

namespace APIExamples
{
    /// <summary>
    /// Holds badge API examples.
    /// </summary>
    /// <pageTitle>Badges</pageTitle>
    internal class Badges
    {
        /// <heading>Creating a badge</heading>
        private void CreateBadge()
        {
            // Creates a new badge object
            BadgeInfo newBadge = new BadgeInfo();

            // Sets the properties for the new badge
            newBadge.BadgeDisplayName = "New badge";
            newBadge.BadgeName = "NewBadge";
            newBadge.BadgeTopLimit = 50;
            newBadge.BadgeImageURL = "Objects/CMS_Badge/Default/siteadmin.gif";
            newBadge.BadgeIsAutomatic = true;

            // Saves the badge to the database
            BadgeInfoProvider.SetBadgeInfo(newBadge);
        }


        /// <heading>Updating a badge</heading>
        private void GetAndUpdateBadge()
        {
            // Gets the badge
            BadgeInfo updateBadge = BadgeInfoProvider.GetBadgeInfo("NewBadge");
            if (updateBadge != null)
            {
                // Updates the badge properties
                updateBadge.BadgeDisplayName = updateBadge.BadgeDisplayName.ToLowerCSafe();

                // Saves the badge changes to the database
                BadgeInfoProvider.SetBadgeInfo(updateBadge);
            }
        }


        /// <heading>Updating multiple badges</heading>
        private void GetAndBulkUpdateBadges()
        {
            // Gets all badges whose code name starts with 'NewBadge'
            var badges = BadgeInfoProvider.GetBadges().WhereStartsWith("BadgeName", "NewBadge");

            // Loops through individual badges
            foreach (BadgeInfo modifyBadge in badges)
            {
                // Updates the badge properties
                modifyBadge.BadgeDisplayName = modifyBadge.BadgeDisplayName.ToUpper();

                // Saves the changed badge to the database
                BadgeInfoProvider.SetBadgeInfo(modifyBadge);
            }
        }        


        /// <heading>Assigning a badge to a user</heading>
        private void AddBadgeToUser()
        {
            // Gets the user 
            UserInfo user = UserInfoProvider.GetUserInfo("Andy");

            // Gets the badge
            BadgeInfo badge = BadgeInfoProvider.GetBadgeInfo("NewBadge");

            if ((user != null) && (badge != null))
            {
                // Assigns the badge to the user
                user.UserSettings.UserBadgeID = badge.BadgeID;

                // Saves the updated user object to the database
                UserInfoProvider.SetUserInfo(user);
            }
        }


        /// <heading>Updating activity points for users</heading>
        private void UpdateActivityPoints()
        {
            // Gets the user
            UserInfo user = UserInfoProvider.GetUserInfo("Andy");

            if (user != null)
            {
                // Adds activity points for the user
                // The number of points depends on the type of the activity (blog comment post in this case) and the number of points assigned to the activity in the settings
                BadgeInfoProvider.UpdateActivityPointsToUser(ActivityPointsEnum.BlogCommentPost, user.UserID, SiteContext.CurrentSiteName, true);
            }
        }


        /// <heading>Removing a badge from a user</heading>
        private void RemoveBadgeFromUser()
        {
            // Gets the user
            UserInfo user = UserInfoProvider.GetUserInfo("Andy");

            if (user != null)
            {
                //Removes the user's current badge
                user.UserSettings.UserBadgeID = 0;

                // Saves the updated user to the database
                UserInfoProvider.SetUserInfo(user);
            }
        }


        /// <heading>Deleting a badge</heading>
        private void DeleteBadge()
        {
            // Gets the badge
            BadgeInfo deleteBadge = BadgeInfoProvider.GetBadgeInfo("NewBadge");

            if (deleteBadge != null)
            {
                // Deletes the badge
                BadgeInfoProvider.DeleteBadgeInfo(deleteBadge);
            }
        }
    }
}

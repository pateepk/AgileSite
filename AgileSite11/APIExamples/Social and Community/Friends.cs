using System;

using CMS.Community;
using CMS.Membership;
using CMS.Helpers;
using CMS.DataEngine;

namespace APIExamples
{
    /// <summary>
    /// Holds friend API examples.
    /// </summary>
    /// <pageTitle>Friends</pageTitle>
    internal class Friends
    {
        /// <heading>Requesting a friendship</heading>
        private void RequestFriendship()
        {
            // Creates a sample user to which the friendship request will be sent
            UserInfo newFriend = new UserInfo();
            newFriend.UserName = "Johnny";
            newFriend.FullName = "John Smith";            

            UserInfoProvider.SetUserInfo(newFriend);

            // Creates a friendship object
            FriendInfo newFriendship = new FriendInfo();

            // Sets the friendship properties
            newFriendship.FriendUserID = MembershipContext.AuthenticatedUser.UserID;
            newFriendship.FriendRequestedUserID = newFriend.UserID;
            newFriendship.FriendRequestedWhen = DateTime.Now;
            newFriendship.FriendComment = "Sample friend request comment.";
            newFriendship.FriendStatus = FriendshipStatusEnum.Waiting;

            // Saves the friend to the database
            FriendInfoProvider.SetFriendInfo(newFriendship);
        }


        /// <heading>Approving a friendship</heading>
        private void ApproveFriendship()
        {
            // Gets the users involved in the friendship (request)
            UserInfo user = MembershipContext.AuthenticatedUser;
            UserInfo friend = UserInfoProvider.GetUserInfo("Johnny");

            if (friend != null)
            {
                // Gets the friendship between the users
                FriendInfo updateFriendship = FriendInfoProvider.GetFriendInfo(user.UserID, friend.UserID);
                if (updateFriendship != null)
                {
                    // Approves the firendship
                    updateFriendship.FriendStatus = FriendshipStatusEnum.Approved;
                    updateFriendship.FriendRejectedBy = 0;
                    updateFriendship.FriendApprovedBy = user.UserID;
                    updateFriendship.FriendApprovedWhen = DateTime.Now;

                    // Saves the changes to the database
                    FriendInfoProvider.SetFriendInfo(updateFriendship);
                }
            }
        }


        /// <heading>Rejecting a friendship</heading>
        private void RejectFriendship()
        {
            // Gets the users involved in the friendship (request)
            UserInfo user = MembershipContext.AuthenticatedUser;
            UserInfo friend = UserInfoProvider.GetUserInfo("Johnny");

            if (friend != null)
            {
                // Gets the friendship between the users
                FriendInfo updateFriendship = FriendInfoProvider.GetFriendInfo(user.UserID, friend.UserID);

                // Rejects the friendship
                updateFriendship.FriendStatus = FriendshipStatusEnum.Rejected;
                updateFriendship.FriendApprovedBy = 0;
                updateFriendship.FriendRejectedBy = user.UserID;
                updateFriendship.FriendRejectedWhen = DateTime.Now;

                // Saves the changes to the database
                FriendInfoProvider.SetFriendInfo(updateFriendship);
            }
        }


        /// <heading>Updating multiple friends</heading>
        private void GetAndBulkUpdateFriends()
        {
            // Gets a user
            UserInfo user = UserInfoProvider.GetUserInfo("Johnny");

            if (user != null)
            {
                // Gets all friendships of the user that are not approved yet or have been rejected
                InfoDataSet<FriendInfo> friends = FriendInfoProvider.GetRequestedFriends(user.UserID);

                // Loops through individual friends
                foreach (FriendInfo modifyFriend in friends)
                {
                    // Approves all friendships
                    modifyFriend.FriendStatus = FriendshipStatusEnum.Approved;
                    modifyFriend.FriendRejectedBy = 0;
                    modifyFriend.FriendApprovedBy = user.UserID;
                    modifyFriend.FriendApprovedWhen = DateTime.Now;

                    // Saves the changes to the database
                    FriendInfoProvider.SetFriendInfo(modifyFriend);
                }
            }
        }


        /// <heading>Deleting friends</heading>
        private void GetAndBulkDeleteFriends()
        {
            // Gets a user
            UserInfo user = UserInfoProvider.GetUserInfo("Johnny");

            if (user != null)
            {
                // Gets all of the user's friends
                InfoDataSet<FriendInfo> friends = FriendInfoProvider.GetFullUserFriends(user.UserID);

                // Deletes all friends
                foreach (FriendInfo deleteFriend in friends)
                {
                    FriendInfoProvider.DeleteFriendInfo(deleteFriend);
                }
            }
        }
    }
}
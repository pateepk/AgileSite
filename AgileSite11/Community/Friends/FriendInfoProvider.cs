using System;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Membership;

namespace CMS.Community
{
    using TypedDataSet = InfoDataSet<FriendInfo>;

    /// <summary>
    /// Class providing FriendInfo management.
    /// </summary>
    public class FriendInfoProvider : AbstractInfoProvider<FriendInfo, FriendInfoProvider>
    {
        #region "Public methods"

        /// <summary>
        /// Returns all friends.
        /// </summary>
        public static ObjectQuery<FriendInfo> GetFriends()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the FriendInfo structure for the specified friend.
        /// </summary>
        /// <param name="friendId">Friend id</param>
        public static FriendInfo GetFriendInfo(int friendId)
        {
            return ProviderObject.GetInfoById(friendId);
        }


        /// <summary>
        /// Returns the FriendInfo structure specified by GUID.
        /// </summary>
        /// <param name="guid">GUID of FriendInfo</param>
        public static FriendInfo GetFriendInfo(Guid guid)
        {
            return ProviderObject.GetInfoByGuid(guid);
        }


        /// <summary>
        /// Returns the FriendInfo structure for the specified users.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="requestedUserId">Requested user id</param>
        public static FriendInfo GetFriendInfo(int userId, int requestedUserId)
        {
            return ProviderObject.GetFriendInfoInternal(userId, requestedUserId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified friend.
        /// </summary>
        /// <param name="friend">Friend to set</param>
        public static void SetFriendInfo(FriendInfo friend)
        {
            SetFriendInfo(friend, true);
        }


        /// <summary>
        /// Sets (updates or inserts) specified friend.
        /// </summary>
        /// <param name="friend">Friend to set</param>
        /// <param name="updateCurrentUser">Update current user friends hashtable</param>
        public static void SetFriendInfo(FriendInfo friend, bool updateCurrentUser)
        {
            // Check license
            if (!String.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Friends);
            }

            ProviderObject.SetFriendInfoInternal(friend, updateCurrentUser);
        }


        /// <summary>
        /// Deletes specified friend.
        /// </summary>
        /// <param name="friend">Friend object</param>
        public static void DeleteFriendInfo(FriendInfo friend)
        {
            DeleteFriendInfo(friend, true);
        }


        /// <summary>
        /// Deletes specified friend.
        /// </summary>
        /// <param name="friend">Friend object</param>
        /// <param name="updateCurrentUser">Update current user friends hashtable</param>
        public static void DeleteFriendInfo(FriendInfo friend, bool updateCurrentUser)
        {
            ProviderObject.DeleteFriendInfoInternal(friend, updateCurrentUser);
        }


        /// <summary>
        /// Deletes specified friend.
        /// </summary>
        /// <param name="friendId">Friend id</param>
        public static void DeleteFriendInfo(int friendId)
        {
            DeleteFriendInfo(GetFriendInfo(friendId));
        }


        /// <summary>
        /// Gets all friends filtered out by where condition and ordered by given expression.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        /// <returns>DataSet with friends.</returns>
        [Obsolete("Use GetFriends() instead.")]
        public static TypedDataSet GetFriends(string where, string orderBy, int topN = 0, string columns = null)
        {
            return ProviderObject.GetFriendsInternal(where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets all user friendship relations.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        /// <returns>DataSet with friends.</returns>
        public static TypedDataSet GetUserFriendshipRelations(int userId, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetUserFriendshipRelationsInternal(userId, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets strictly friends for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>DataSet with friends.</returns>
        public static TypedDataSet GetFullUserFriends(int userId)
        {
            return ProviderObject.GetApprovedUserFriendsInternal(userId, null, null, 0, null);
        }


        /// <summary>
        /// Gets strictly friends for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        [Obsolete("Use method GetApprovedUserFriends(int) instead")]
        public static TypedDataSet GetApprovedUserFriends(int userId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetApprovedUserFriendsInternal(userId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets all friendship requests for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>DataSet with friends ready for approval.</returns>
        public static TypedDataSet GetFullFriendsForApproval(int userId)
        {
            return ProviderObject.GetFullFriendsForApprovalInternal(userId, null, null, 0, null);
        }


        /// <summary>
        /// Gets all friendship requests for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        [Obsolete("Use method GetFullFriendsForApproval(int) instead")]
        public static TypedDataSet GetFullFriendsForApproval(int userId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetFullFriendsForApprovalInternal(userId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets all rejected friendship requests for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>DataSet with rejected friends.</returns>
        public static TypedDataSet GetFullRejectedFriends(int userId)
        {
            return ProviderObject.GetFullRejectedFriendsInternal(userId, null, null, 0, null);
        }


        /// <summary>
        /// Gets all rejected friendship requests for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        [Obsolete("Use method GetFullRejectedFriends(int) instead")]
        public static TypedDataSet GetFullRejectedFriends(int userId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetFullRejectedFriendsInternal(userId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets requested friendships by user (non-approved, rejected).
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>DataSet with requested friendships.</returns>
        public static TypedDataSet GetRequestedFriends(int userId)
        {
            return ProviderObject.GetRequestedFriendsInternal(userId, null, null, 0, null);
        }


        /// <summary>
        /// Gets requested friendships by user (non-approved, rejected).
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        [Obsolete("Use method GetRequestedFriends(int) instead")]
        public static TypedDataSet GetRequestedFriends(int userId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetRequestedFriendsInternal(userId, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Determines strictly whether friendship relation exists.
        /// </summary>
        /// <param name="userId">User requesting friendship</param>
        /// <param name="requestedUserId">User participating in friendship</param>
        public static bool FriendshipExists(int userId, int requestedUserId)
        {
            return ProviderObject.FriendshipExistsInternal(userId,requestedUserId);
        }


        /// <summary>
        /// Returns DataSet with all user's friends with user data based on where condition.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        public static TypedDataSet GetFullUserFriends(int userId, string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetFullUserFriendsInternal(userId, where, orderBy, topN, columns);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Returns the FriendInfo structure for the specified users.
        /// </summary>
        /// <param name="userId">User id</param>
        /// <param name="requestedUserId">Requested user id</param>
        protected virtual FriendInfo GetFriendInfoInternal(int userId, int requestedUserId)
        {
            var where = GetWhereCondition(userId, requestedUserId);
            return GetFriends().TopN(1).Where(new WhereCondition(where)).FirstOrDefault();
        }


        /// <summary>
        /// Sets (updates or inserts) specified friend.
        /// </summary>
        /// <param name="friend">Friend to set</param>
        /// <param name="updateCurrentUser">Update current user friends hashtable</param>
        protected virtual void SetFriendInfoInternal(FriendInfo friend, bool updateCurrentUser)
        {
            ProviderObject.SetInfo(friend);

            // Update friends hashtable for current user
            if (updateCurrentUser)
            {
                UpdateUserStatus(friend);
            }
        }


        /// <summary>
        /// Deletes specified friend.
        /// </summary>
        /// <param name="friend">Friend object</param>
        /// <param name="updateCurrentUser">Update current user friends hashtable</param>
        protected virtual void DeleteFriendInfoInternal(FriendInfo friend, bool updateCurrentUser)
        {
            if (friend != null)
            {
                // Update friends hashtable for current user
                if (updateCurrentUser)
                {
                    friend.FriendStatus = FriendshipStatusEnum.None;
                    UpdateUserStatus(friend);
                }

                ProviderObject.DeleteInfo(friend);
            }
        }


        /// <summary>
        /// Gets all friends filtered out by where condition and ordered by given expression.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        /// <returns>DataSet with friends.</returns>
        [Obsolete("Use GetFriends() instead.")]
        protected virtual TypedDataSet GetFriendsInternal(string where, string orderBy, int topN, string columns)
        {
            return GetFriends()
                .Where(new WhereCondition(where))
                .OrderBy(orderBy)
                .TopN(topN)
                .Columns(columns)
                .BinaryData(true)
                .TypedResult;
        }


        /// <summary>
        /// Gets all user friendship relations.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        /// <returns>DataSet with friends.</returns>
        protected virtual TypedDataSet GetUserFriendshipRelationsInternal(int userId, string orderBy, int topN, string columns)
        {
            return GetFriends()
                .Where(new WhereCondition().WhereEquals("FriendUserID", userId).Or().WhereEquals("FriendRequestedUserID", userId))
                .OrderBy(orderBy)
                .TopN(topN)
                .Columns(columns)
                .TypedResult;
        }


        /// <summary>
        /// Gets strictly friends for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        protected virtual TypedDataSet GetApprovedUserFriendsInternal(int userId, string where, string orderBy, int topN, string columns)
        {
            string defaultWhere = "FriendStatus = " + Convert.ToInt32(FriendshipStatusEnum.Approved);
            string whereCondition = SqlHelper.AddWhereCondition(defaultWhere, where);

            return GetFullUserFriendsInternal(userId, whereCondition, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets all friendship requests for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        protected virtual TypedDataSet GetFullFriendsForApprovalInternal(int userId, string where, string orderBy, int topN, string columns)
        {
            string defaultWhere = "FriendStatus = " + Convert.ToInt32(FriendshipStatusEnum.Waiting) +
                                  "  AND (FriendUserID <> " + userId + ")";
            string whereCondition = SqlHelper.AddWhereCondition(defaultWhere, where);

            return GetFullUserFriendsInternal(userId, whereCondition, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets all rejected friendship requests for given user with full user information.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        protected virtual TypedDataSet GetFullRejectedFriendsInternal(int userId, string where, string orderBy, int topN, string columns)
        {
            string defaultWhere = "(FriendStatus = " + Convert.ToInt32(FriendshipStatusEnum.Rejected) + ") AND (FriendRejectedBy = " + userId + ")";
            string whereCondition = SqlHelper.AddWhereCondition(defaultWhere, where);

            return GetFullUserFriendsInternal(userId, whereCondition, orderBy, topN, columns);
        }


        /// <summary>
        /// Gets requested friendships by user (non-approved, rejected).
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        protected virtual TypedDataSet GetRequestedFriendsInternal(int userId, string where, string orderBy, int topN, string columns)
        {
            string defaultWhere = " ((FriendStatus=" + Convert.ToInt32(FriendshipStatusEnum.Waiting) + " AND FriendUserID=" + userId + ") OR (FriendStatus=" + Convert.ToInt32(FriendshipStatusEnum.Rejected) + " AND FriendRejectedBy<>" + userId + "))";
            string whereCondition = SqlHelper.AddWhereCondition(defaultWhere, where);

            return GetFullUserFriendsInternal(userId, whereCondition, orderBy, topN, columns);
        }


        /// <summary>
        /// Determines strictly whether friendship relation exists.
        /// </summary>
        /// <param name="userId">User requesting friendship</param>
        /// <param name="requestedUserId">User participating in friendship</param>
        protected virtual bool FriendshipExistsInternal(int userId, int requestedUserId)
        {
            var where = GetWhereCondition(userId, requestedUserId);

            return GetFriends()
                .Where(where)
                .HasResults();
        }


        /// <summary>
        /// Returns DataSet with all user's friends with user data based on where condition.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        protected virtual TypedDataSet GetFullUserFriendsInternal(int userId, string where, string orderBy, int topN, string columns)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@UserID", userId);
            parameters.EnsureDataSet<FriendInfo>();

            return ConnectionHelper.ExecuteQuery("community.friend.selectfriendships", parameters, where, orderBy, topN, columns).As<FriendInfo>();
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Update user status.
        /// </summary>
        /// <param name="friend">Friend info</param>
        private static void UpdateUserStatus(FriendInfo friend)
        {
            var currentUser = MembershipContext.AuthenticatedUser;
            int userId = 0;
            if (currentUser.UserID == friend.FriendRequestedUserID)
            {
                userId = friend.FriendUserID;
            }
            else if (currentUser.UserID == friend.FriendUserID)
            {
                userId = friend.FriendRequestedUserID;
            }

            currentUser.UpdateFriendStatus(userId, friend.FriendStatus);
        }


        private static string GetWhereCondition(int userId, int requestedUserId)
        {
            return "(FriendUserID=" + userId + " AND FriendRequestedUserID=" + requestedUserId + ") OR (FriendUserID=" + requestedUserId + " AND FriendRequestedUserID=" + userId + ")";
        }

        #endregion
    }
}
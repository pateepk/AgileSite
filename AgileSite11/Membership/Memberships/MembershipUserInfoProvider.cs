using System;
using System.Linq;

using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Membership
{
    using TypedDataSet = InfoDataSet<MembershipUserInfo>;

    /// <summary>
    /// Class providing MembershipUserInfo management.
    /// </summary>
    public class MembershipUserInfoProvider : AbstractInfoProvider<MembershipUserInfo, MembershipUserInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns the query for all relationships between memberships and users.
        /// </summary>   
        public static ObjectQuery<MembershipUserInfo> GetMembershipUsers()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns relationship between specified membership and User.
        /// </summary>
        /// <param name="membershipId">Membership ID</param>
        /// <param name="userId">User ID</param>
        public static MembershipUserInfo GetMembershipUserInfo(int membershipId, int userId)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            return ProviderObject.GetMembershipUserInfoInternal(membershipId, userId);
        }


        /// <summary>
        /// Sets relationship between specified membership and User.
        /// </summary>
        /// <param name="infoObj">Membership-User relationship to be set</param>
        /// <remarks>
        /// Calling this method does not invalidate the user whose membership binding is being set. Consider using <see cref="AddMembershipToUser"/>
        /// or invalidating the <see cref="UserInfo"/> explicitly.
        /// </remarks>
        /// <seealso cref="UserInfoProvider.InvalidateUser"/>
        /// <seealso cref="BaseInfo.GeneralizedInfoWrapper.Invalidate"/>
        public static void SetMembershipUserInfo(MembershipUserInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Deletes relationship between specified membership and user.
        /// </summary>
        /// <param name="infoObj">Membership-user relationship to be deleted</param>
        /// <remarks>
        /// Calling this method does not invalidate the user the membership binding is being removed from. Consider using <see cref="RemoveMembershipFromUser"/>
        /// or invalidating the <see cref="UserInfo"/> explicitly.
        /// </remarks>
        /// <seealso cref="UserInfoProvider.InvalidateUser"/>
        /// <seealso cref="BaseInfo.GeneralizedInfoWrapper.Invalidate"/>
        public static void DeleteMembershipUserInfo(MembershipUserInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Sets relationship between specified membership and user.
        /// </summary>	
        /// <param name="membershipId">Membership ID</param>
        /// <param name="userId">User ID</param>
        /// <param name="validTo">Date to which membership is valid for user</param>
        /// <param name="sendNotification">Indicates, if notification will be sent to user when membership expires</param>
        /// <remarks>
        /// Invalidates the <see cref="UserInfo"/> object of the user whose membership is being modified.
        /// </remarks>
        public static void AddMembershipToUser(int membershipId, int userId, DateTime validTo, bool sendNotification = false)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            MembershipUserInfo infoObj = ProviderObject.CreateInfo();

            infoObj.MembershipID = membershipId;
            infoObj.UserID = userId;
            infoObj.ValidTo = validTo;
            infoObj.SendNotification = sendNotification;

            SetMembershipUserInfo(infoObj);

            UserInfoProvider.InvalidateUser(userId);
        }


        /// <summary>
        /// Deletes relationship between specified membership and user.
        /// </summary>
        /// <param name="membershipId">Membership ID</param>
        /// <param name="userId">User ID</param>
        /// <remarks>
        /// Invalidates the <see cref="UserInfo"/> object of the user whose membership is being modified.
        /// </remarks>
        public static void RemoveMembershipFromUser(int membershipId, int userId)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            MembershipUserInfo infoObj = GetMembershipUserInfo(membershipId, userId);
            DeleteMembershipUserInfo(infoObj);

            UserInfoProvider.InvalidateUser(userId);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Returns dataset of all expiring memberships matching given parameters along with relevant user and membership information.
        /// </summary>
        /// <param name="days">Number of days in which the memberships will expire.</param>
        /// <param name="siteId">Site ID. Set to 0 for global expiring memberships.</param>
        /// <param name="where">Additional where condition</param>
        /// <param name="onlyWithSendNotification">Get only memberships with send notification flag set to true.</param>
        public static TypedDataSet GetExpiringMemberships(int days, int siteId, string where, bool onlyWithSendNotification)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, "") != "")
            {
                LicenseHelper.RequestFeature(RequestContext.CurrentDomain, FeatureEnum.Membership);
            }

            return ProviderObject.GetExpiringMembershipsInternal(days, siteId, where, onlyWithSendNotification);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns relationship between specified membership and user.
        /// </summary>
        /// <param name="membershipId">Membership ID</param>
        /// <param name="userId">User ID</param>
        protected virtual MembershipUserInfo GetMembershipUserInfoInternal(int membershipId, int userId)
        {
            var condition = new WhereCondition()
                .WhereEquals("MembershipID", membershipId)
                .WhereEquals("UserID", userId);

            return GetObjectQuery().TopN(1).Where(condition).FirstOrDefault();
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Returns dataset of all expiring memberships matching given parameters along with relevant user and membership information.
        /// </summary>
        /// <param name="days">Number of days in which the memberships will expire.</param>
        /// <param name="siteId">Site ID</param>
        /// <param name="where">Additional where condition</param>
        /// <param name="onlyWithSendNotification">Get only memberships with send notification flag set to true.</param>
        protected virtual TypedDataSet GetExpiringMembershipsInternal(int days, int siteId, string where, bool onlyWithSendNotification)
        {
            QueryDataParameters queryParams = new QueryDataParameters();

            // Set query parameters
            queryParams.Add("@Days", days);
            queryParams.Add("@SiteID", siteId);
            queryParams.Add("@OnlyWithSendNotification", onlyWithSendNotification);
            queryParams.EnsureDataSet<MembershipUserInfo>();

            // Return query results ordered by user ID
            return ConnectionHelper.ExecuteQuery("cms.membershipuser.selectexpiring", queryParams, where).As<MembershipUserInfo>();
        }

        #endregion
    }
}
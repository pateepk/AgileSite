using System;
using System.Linq;

using CMS.Base;
using CMS.DataEngine;

namespace CMS.Membership
{
    /// <summary>
    /// Class providing BadgeInfo management.
    /// </summary>
    public class BadgeInfoProvider : AbstractInfoProvider<BadgeInfo, BadgeInfoProvider>
    {
        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        public BadgeInfoProvider()
            : base(BadgeInfo.TYPEINFO, new HashtableSettings
				{
					ID = true,
					Name = true,
					Load = LoadHashtableEnum.All
				})
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns the BadgeInfo structure for the specified badge.
        /// </summary>
        /// <param name="badgeId">Badge id</param>
        public static BadgeInfo GetBadgeInfo(int badgeId)
        {
            return ProviderObject.GetInfoById(badgeId);
        }


        /// <summary>
        /// Returns the BadgeInfo structure for the specified badge.
        /// </summary>
        /// <param name="badgeName">Badge name</param>
        public static BadgeInfo GetBadgeInfo(string badgeName)
        {
            return ProviderObject.GetInfoByCodeName(badgeName);
        }


        /// <summary>
        /// Sets (updates or inserts) specified badge.
        /// </summary>
        /// <param name="badge">Badge to set</param>
        public static void SetBadgeInfo(BadgeInfo badge)
        {
            ProviderObject.SetBadgeInfoInternal(badge);
        }


        /// <summary>
        /// Deletes specified badge.
        /// </summary>
        /// <param name="infoObj">Badge object</param>
        public static void DeleteBadgeInfo(BadgeInfo infoObj)
        {
            ProviderObject.DeleteInfo(infoObj);
        }


        /// <summary>
        /// Deletes specified badge.
        /// </summary>
        /// <param name="badgeId">Badge id</param>
        public static void DeleteBadgeInfo(int badgeId)
        {
            BadgeInfo infoObj = GetBadgeInfo(badgeId);
            DeleteBadgeInfo(infoObj);
        }


        /// <summary>
        /// Adds (subtracts) activity points to specified user.
        /// </summary>
        /// <param name="pointsType">Activity points type</param>
        /// <param name="userID">User ID</param>
        /// <param name="siteName">Site name</param>
        /// <param name="add">If true add points else subtract points</param>
        public static void UpdateActivityPointsToUser(ActivityPointsEnum pointsType, int userID, string siteName, bool add)
        {
            ProviderObject.LoadInfos();

            // Site name must be set
            if (String.IsNullOrEmpty(siteName))
            {
                return;
            }

            // Get user info
            UserInfo ui = UserInfoProvider.GetUserInfo(userID);
            if ((ui != null) && !ui.IsPublic())
            {
                // Get points' values from settings
                int points = 0;
                switch (pointsType)
                {
                    case ActivityPointsEnum.BlogCommentPost:
                        points = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSActivityPointsForBlogCommentPost");
                        break;

                    case ActivityPointsEnum.ForumPost:
                        points = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSActivityPointsForForumPost");
                        break;

                    case ActivityPointsEnum.MessageBoardPost:
                        points = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSActivityPointsForMessageBoardPost");
                        break;

                    case ActivityPointsEnum.BlogPosts:
                        points = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSActivityPointsForBlogPost");
                        break;
                }

                // Check whether points should be added or subtracted
                if (!add)
                {
                    points = points * (-1);
                }

                // Update user counts
                UserInfoProvider.UpdateUserCounts(pointsType, userID, points);

                // Update badge
                UpdateUserBadge(ui);

                UserInfoProvider.SetUserInfo(ui);
            }
        }


        /// <summary>
        /// Assigns to the given user an appropriate badge according to his activity points.
        /// </summary>
        /// <remarks>
        /// Doesn't do anything if user's badge is not automatic.
        /// </remarks>
        /// <param name="userInfo">User to be updated</param>
        public static void UpdateUserBadge(UserInfo userInfo)
        {
            ProviderObject.UpdateUserBadgeInternal(userInfo);
        }


        /// <summary>
        /// Returns the query for all badges.
        /// </summary>
        public static ObjectQuery<BadgeInfo> GetBadges()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Re-distributes user badges according to current badge collection.
        /// This operation is time-consuming for large amount of users. It's recommended
        /// call this method from separate thread.
        /// </summary>
        public static void BadgeCollectionChanged()
        {
            ProviderObject.BadgeCollectionChangedInternal();
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Sets (updates or inserts) specified badge.
        /// </summary>
        /// <param name="badge">Badge to set</param>
        protected virtual void SetBadgeInfoInternal(BadgeInfo badge)
        {
            bool userDependChange = false;

            if (badge != null)
            {
                if (badge.BadgeID > 0)
                {
                    BadgeInfo bi = GetInfoById(badge.BadgeID);
                    if (bi != null)
                    {
                        badge.BadgeGUID = bi.BadgeGUID;
                        if ((bi.BadgeTopLimit != badge.BadgeTopLimit) || (bi.BadgeIsAutomatic != badge.BadgeIsAutomatic))
                        {
                            userDependChange = true;
                        }
                    }
                }
                else
                {
                    userDependChange = true;
                }
            }

            SetInfo(badge);

            if (userDependChange)
            {
                // Re-distribute user badges
                CMSThread thread = new CMSThread(BadgeCollectionChanged);
                thread.Start();
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(BadgeInfo info)
        {
            base.DeleteInfo(info);

            // Re-distribute user badges
            CMSThread thread = new CMSThread(BadgeCollectionChanged);
            thread.Start();
        }


        /// <summary>
        /// Assigns to the given user an appropriate badge according to his activity points.
        /// </summary>
        /// <remarks>
        /// Doesn't do anything if user's badge is not automatic.
        /// </remarks>
        /// <param name="userInfo">User to be updated</param>
        protected virtual void UpdateUserBadgeInternal(UserInfo userInfo)
        {
            if (userInfo == null)
            {
                throw new ArgumentNullException("userInfo");
            }

            var userSettings = userInfo.UserSettings;
            var badge = GetBadgeInfo(userSettings.UserBadgeID);
            if ((badge == null) || badge.BadgeIsAutomatic)
            {
                badge = GetAutomaticBadge(userSettings.UserActivityPoints);
                if (badge != null)
                {
                    userSettings.UserBadgeID = badge.BadgeID;
                }
            }
        }


        private BadgeInfo GetAutomaticBadge(int activityPoints)
        {
            return GetBadges()
                .TopN(1)
                .WhereTrue("BadgeIsAutomatic")
                .WhereGreaterThan("BadgeTopLimit", activityPoints)
                .OrderBy("BadgeTopLimit")
                .FirstOrDefault();
        }


        /// <summary>
        /// Re-distributes user badges according to current badge collection.
        /// This operation is time-consuming for large amount of users. It's recommended
        /// call this method from separate thread.
        /// </summary>
        protected virtual void BadgeCollectionChangedInternal()
        {
            var typeInfo = ObjectTypeManager.GetTypeInfo(PredefinedObjectType.USERSETTINGS);
            var eventArgs = new BulkUpdateEventArgs(typeInfo, new WhereCondition()
                .WhereEqualsOrNull("UserBadgeID", 0)
                .Or()
                .WhereNotIn("UserBadgeID", GetBadges()
                    .Columns("BadgeID")
                    .WhereFalse("BadgeIsAutomatic")), new[] { "UserBadgeID" });

            using (var bulkUpdateEvent = TypeInfo.Events.BulkUpdate.StartEvent(eventArgs))
            {
                if (bulkUpdateEvent.CanContinue())
                {
                    ConnectionHelper.ExecuteQuery("cms.badge.updateuserbadge", null);
                    UserInfo.TYPEINFO.InvalidateAllObjects();
                }

                bulkUpdateEvent.FinishEvent();
            }
        }

        #endregion
    }
}
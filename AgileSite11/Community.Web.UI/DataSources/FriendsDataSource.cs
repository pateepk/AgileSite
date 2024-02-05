using System;
using System.Web.UI;

using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Friends data source server control.
    /// </summary>
    [ToolboxData("<{0}:FriendsDataSource runat=server />"), Serializable]
    public class FriendsDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private FriendshipStatusEnum mFriendStatus = FriendshipStatusEnum.None;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets UserID for which friends should be loaded.
        /// </summary>
        public int UserID
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets friend status condition.
        /// </summary>
        public FriendshipStatusEnum FriendStatus
        {
            get
            {
                return mFriendStatus;
            }
            set
            {
                mFriendStatus = value;
            }
        }

        #endregion


        #region "Methods, events, handlers"

        /// <summary>
        /// Gets data source from DB.
        /// </summary>
        /// <returns>Dataset as object</returns>
        protected override object GetDataSourceFromDB()
        {
            if (StopProcessing)
            {
                return null;
            }

            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }
            // Select friends by status
            string where = "(FriendStatus = " + Convert.ToInt32(FriendStatus) + " )";

            // Create WHERE condition
            if (!String.IsNullOrEmpty(WhereCondition))
            {
                where = "(" + WhereCondition + ") AND " + where;
            }

            return FriendInfoProvider.GetFullUserFriends(UserID, where, OrderBy, TopN, SelectedColumns);
        }


        /// <summary>
        /// Gets the default cache dependencies for the data source.
        /// </summary>
        public override string GetDefaultCacheDependencies()
        {
            // Get default dependencies
            string result = base.GetDefaultCacheDependencies();

            if (result != null)
            {
                result += "\n";
            }

            result += "community.friend|all";

            return result;
        }


        /// <summary>
        /// Gets cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "friendsdatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, TopN, SiteName, SelectedColumns, FriendStatus, UserID };
        }

        #endregion
    }
}
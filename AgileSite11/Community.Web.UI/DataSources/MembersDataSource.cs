using System;
using System.Data;
using System.Web.UI;

using CMS.Base;
using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Members data source server control.
    /// </summary>
    [ToolboxData("<{0}:MembersDataSource runat=server />"), Serializable]
    public class MembersDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private bool mSelectOnlyApproved = true;
        private string mGroupName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets select only approved property.
        /// </summary>
        public bool SelectOnlyApproved
        {
            get
            {
                return mSelectOnlyApproved;
            }
            set
            {
                mSelectOnlyApproved = value;
            }
        }


        /// <summary>
        /// Gets or sets group name to specify group members.
        /// </summary>
        public string GroupName
        {
            get
            {
                return mGroupName;
            }
            set
            {
                mGroupName = value;
            }
        }

        #endregion


        #region "Methods, events, handlers"

        /// <summary>
        /// Gets datasource from DB.
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

            DataSet ds = null;

            // Get site ID
            string site = DataHelper.GetNotEmpty(SiteName, SiteContext.CurrentSiteName);
            SiteInfo si = SiteInfoProvider.GetSiteInfo(site);
            if (si != null)
            {
                int siteID = si.SiteID;

                string where = "(SiteID = " + siteID + ")";

                if (SelectOnlyApproved)
                {
                    where += " AND (MemberStatus = 0)";
                }

                // Create where condition
                if (!String.IsNullOrEmpty(GroupName))
                {
                    GroupInfo gi;

                    if ((CommunityContext.CurrentGroup != null) && (GroupName.ToLowerCSafe() == CommunityContext.CurrentGroup.GroupName.ToLowerCSafe()) && (CommunityContext.CurrentGroup.GroupSiteID == si.SiteID))
                    {
                        gi = CommunityContext.CurrentGroup;
                    }
                    else
                    {
                        gi = GroupInfoProvider.GetGroupInfo(GroupName, si.SiteName);
                    }

                    if (gi == null)
                    {
                        return null;
                    }

                    if (gi.GroupSiteID == siteID)
                    {
                        where += " AND (MemberGroupID = " + gi.GroupID + ")";
                    }
                }
                else
                {
                    return null;
                }

                // Add condition to show only non hidden and enabled users
                where += " AND (UserIsHidden = 0 OR UserIsHidden IS NULL) AND " + UserInfoProvider.USER_ENABLED_WHERE_CONDITION;

                // Combine where conditions
                if (!String.IsNullOrEmpty(WhereCondition))
                {
                    where = "(" + WhereCondition + ") AND " + where;
                }

                // Get all group members in given site
                ds = GroupMemberInfoProvider.GetCompleteSiteMembers(where, OrderBy, TopN, SelectedColumns);
            }

            return ds;
        }


        /// <summary>
        /// Gets the default cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "membersdatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, GroupName, SelectOnlyApproved };
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

            result += "community.groupmember|all";

            return result;
        }

        #endregion
    }
}
using System;
using System.Web.UI;
using System.Data;
using System.Linq;

using CMS.Base;
using CMS.DocumentEngine.Web.UI;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;
using CMS.DocumentEngine;

namespace CMS.Forums.Web.UI
{
    /// <summary>
    /// Forum posts data source server control.
    /// </summary>
    [ToolboxData("<{0}:ForumPostsDataSource runat=server />"), Serializable]
    public class ForumPostsDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private int mGroupID;
        private bool mShowGroupPosts;
        private bool mSelectOnlyApproved = true;
        private string mForumName = String.Empty;
        private bool mCheckPermissions = true;
        private bool mDistinct;

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
        /// Gets or sets forum name for which forum posts should be obtained.
        /// </summary>
        public string ForumName
        {
            get
            {
                return mForumName;
            }
            set
            {
                mForumName = value;
            }
        }


        /// <summary>
        /// Gets or sets if permissions to access posts should be checked.
        /// </summary>  
        public bool CheckPermissions
        {
            get
            {
                return mCheckPermissions;
            }
            set
            {
                mCheckPermissions = value;
            }
        }


        /// <summary>
        /// Gets or sets the group ID.
        /// </summary>
        public int GroupID
        {
            get
            {
                return mGroupID;
            }
            set
            {
                mGroupID = value;
            }
        }


        /// <summary>
        /// Indicates if the group posts should be included. (If no group ID is provided.).
        /// </summary>
        public bool ShowGroupPosts
        {
            get
            {
                return mShowGroupPosts;
            }
            set
            {
                mShowGroupPosts = value;
            }
        }


        /// <summary>
        /// Indicates whether distinct keyword should be used.
        /// </summary>
        public bool Distinct
        {
            get
            {
                return mDistinct;
            }
            set
            {
                mDistinct = value;
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

            DataSet ds = null;

            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            // Get site ID
            int siteId = 0;
            if (!String.IsNullOrEmpty(SiteName))
            {
                SiteInfo si = SiteInfoProvider.GetSiteInfo(SiteName);

                if (si != null)
                {
                    siteId = si.SiteID;
                }
            }

            string secondPartWhere = String.Empty;
            if ((!String.IsNullOrEmpty(ForumName)) && (ForumName == "ad_hoc_forum"))
            {
                ForumInfo fi = ForumInfoProvider.GetForumInfoByDocument(DocumentContext.CurrentDocument.DocumentID);
                if (fi != null)
                {
                    secondPartWhere += "ForumID = " + fi.ForumID;
                }
            }
            else
            {
                // WHERE condition for forum posts
                if (GroupID != 0)
                {
                    secondPartWhere += "(GroupGroupID = " + GroupID + ") AND ";
                }
                else if (!ShowGroupPosts)
                {
                    secondPartWhere += "(GroupGroupID IS NULL) AND ";
                }
                secondPartWhere += "GroupSiteID=" + siteId + (SelectOnlyApproved ? " AND PostApproved=1  " : "") + ((ForumName != String.Empty) ? " AND ForumName = N'" + SqlHelper.GetSafeQueryString(ForumName, false) + "' " : "");
            }


            // Check if any data should be selected
            if (secondPartWhere != String.Empty)
            {
                string where = WhereCondition;

                // Create WHERE condition
                if (!String.IsNullOrEmpty(where))
                {
                    where = "(" + where + ") AND (" + secondPartWhere + ")";
                }
                else
                {
                    where = secondPartWhere;
                }

                if (CheckPermissions)
                {
                    string securityWhere = ForumInfoProvider.CombineSecurityWhereCondition(null, GroupID);

                    if (ShowGroupPosts && (GroupID <= 0) && !String.IsNullOrEmpty(securityWhere))
                    {
                        CurrentUserInfo cui = MembershipContext.AuthenticatedUser;
                        string roleMemberWhere = "(ForumAccess > 299999 AND ForumAccess < 400000)";

                        // If not global administrator and community admin solve group membership
                        if ((!cui.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin) && !cui.IsAuthorizedPerResource("CMS.Groups", "Manage")) && (cui.Groups != null))
                        {
                            // Get list of user groups
                            int[] groupIds = new int[cui.Groups.Count];
                            cui.Groups.Keys.CopyTo(groupIds, 0);

                            // Combine where condition
                            roleMemberWhere = SqlHelper.AddWhereCondition(roleMemberWhere, SqlHelper.GetWhereCondition("GroupGroupID", groupIds.AsEnumerable()));
                        }

                        // Combine original where condition with group membership condition   
                        securityWhere = SqlHelper.AddWhereCondition(securityWhere, roleMemberWhere, "OR");
                    }
                    where = SqlHelper.AddWhereCondition(where, securityWhere);
                }

                // Execute query and return dataset
                var query = ForumInfoProvider.GetForums().TopN(TopN);
                if (Distinct)
                {
                    query.Distinct();
                }
                query.Columns(SelectedColumns).From("View_Forums_GroupForumPost_Joined").Where(where).OrderBy(OrderBy);
                ds = query;
            }

            return ds;
        }


        /// <summary>
        /// Gets the default cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "forumpostsdatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, GroupID, TopN, ForumName, ShowGroupPosts, CheckPermissions, Distinct };
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

            result += "forums.forumpost|all";
            result += "\ncommunity.group|byid|" + GroupID;

            return result;
        }

        #endregion
    }
}
using System;
using System.Web.UI;

using CMS.DocumentEngine.Web.UI;
using CMS.Helpers;
using CMS.SiteProvider;

namespace CMS.Community.Web.UI
{
    /// <summary>
    /// Groups data source server control.
    /// </summary>
    [ToolboxData("<{0}:GroupsDataSource runat=server />"), Serializable]
    public class GroupsDataSource : CMSBaseDataSource
    {
        #region "Variables"

        private bool mSelectOnlyApproved = true;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Gets or sets Select only approved condition.
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

        #endregion


        #region "Methods, events, handlers"

        /// <summary>
        /// Gets datasource from DB.
        /// </summary>
        /// <returns>Dataset as object</returns>
        protected override object GetDataSourceFromDB()
        {
            // Initialize properties with dependence on filter settings
            if (SourceFilterControl != null)
            {
                SourceFilterControl.InitDataProperties(this);
            }

            string where = null;

            // Get site ID
            string site = DataHelper.GetNotEmpty(SiteName, SiteContext.CurrentSiteName);
            SiteInfo si = SiteInfoProvider.GetSiteInfo(site);
            if (si != null)
            {
                where = "(GroupSiteID = " + si.SiteID + ")";
            }

            // Select only approved groups
            if (SelectOnlyApproved)
            {
                if (!String.IsNullOrEmpty(where))
                {
                    where += " AND ";
                }
                where += "(GroupApproved = 1)";
            }

            // Create WHERE condition
            if (!String.IsNullOrEmpty(WhereCondition))
            {
                where = "(" + WhereCondition + ") AND " + where;
            }

            return GroupInfoProvider.GetGroupsView(where, OrderBy, TopN, SelectedColumns);
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

            result += "community.group|all";

            // Group member
            result += "\n";
            result += "community.groupmember|all";

            return result;
        }


        /// <summary>
        /// Gets cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "groupsdatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, TopN, SiteName, SelectOnlyApproved, SelectedColumns };
        }

        #endregion
    }
}
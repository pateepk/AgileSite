using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.Design;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.DocumentEngine.Web.UI
{
    /// <summary>
    /// Users data source server control.
    /// </summary>
    [ToolboxData("<{0}:UsersDataSource runat=server />"), Serializable]
    [ToolboxItem(typeof(WebControlToolboxItem))]
    public class UsersDataSource : CMSBaseDataSource
    {

        #region "Properties"

        /// <summary>
        /// Gets or sets show only approved users property.
        /// </summary>
        public bool SelectOnlyApproved
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets select hidden users property.
        /// </summary>
        public bool SelectHidden
        {
            get;
            set;
        }

        #endregion


        #region "Methods, events, handlers"

        /// <summary>
        /// Constructor
        /// </summary>
        public UsersDataSource()
        {
            SelectOnlyApproved = true;
        }


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
            SourceFilterControl?.InitDataProperties(this);

            try
            {
                // Get site ID
                string site = DataHelper.GetNotEmpty(SiteName, SiteContext.CurrentSiteName);
                SiteInfo si = SiteInfoProvider.GetSiteInfo(site);
                int siteID = si.SiteID;

                // Create where condition
                string where = UserInfoProvider.USER_ENABLED_WHERE_CONDITION + " AND (UserID IN (SELECT UserID FROM CMS_UserSite WHERE SiteID = " + siteID + "))";

                // Don't select hidden users
                if (!SelectHidden)
                {
                    where += " AND ((UserIsHidden = 0) OR (UserIsHidden IS NULL))";
                }

                // Select only approved users
                if (SelectOnlyApproved)
                {
                    where += " AND ((UserWaitingForApproval = 0) OR (UserWaitingForApproval IS NULL))";
                }

                // Combine where conditions
                if (!String.IsNullOrEmpty(WhereCondition))
                {
                    where = "(" + WhereCondition + ") AND " + where;
                }

                // Get all users in given site
                var query = UserInfoProvider.GetUsersDataWithSettings().Where(new WhereCondition(where)).TopN(TopN).Columns(SelectedColumns);

                if (!string.IsNullOrEmpty(OrderBy))
                {
                    var orderByClauses = OrderBy.Split(',');
                    foreach (var clause in orderByClauses)
                    {
                        string direction;
                        var column = SqlHelper.GetOrderByColumnName(clause, out direction);

                        query = SqlHelper.ORDERBY_DESC.Equals(direction, StringComparison.OrdinalIgnoreCase)
                                ? query.OrderByDescending(column)
                                : query.OrderByAscending(column);
                    }
                }

                return query;
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("UsersDataSource", "GetData", ex, SiteContext.CurrentSiteID);
            }

            return null;
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

            result += "cms.user|all";

            return result;
        }


        /// <summary>
        /// Gets default cache key.
        /// </summary>
        protected override object[] GetDefaultCacheKey()
        {
            return new object[] { "usersdatasource", CacheHelper.BaseCacheKey, ClientID, WhereCondition, OrderBy, TopN, SelectedColumns, SelectOnlyApproved, SelectHidden };
        }

        #endregion
    }
}
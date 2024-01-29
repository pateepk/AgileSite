using System;
using System.Collections.Generic;
using System.Linq;

using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Membership;
using CMS.SiteProvider;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Loads dictionary containing default user dashboard setting and corresponding dashboard items for the given user and site. 
    /// </summary>
    internal class UserRolesDefaultDashboardItemsLoader : IDefaultDashboardItemsLoader
    {
        private readonly IDashboardItemProvider mDashboardItemProvider;


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="dashboardItemProvider">Instance of <see cref="IDashboardItemProvider"/></param>
        /// <exception cref="ArgumentNullException"><paramref name="dashboardItemProvider"/> is null</exception>
        public UserRolesDefaultDashboardItemsLoader(IDashboardItemProvider dashboardItemProvider)
        {
            if (dashboardItemProvider == null)
            {
                throw new ArgumentNullException("dashboardItemProvider");
            }

            mDashboardItemProvider = dashboardItemProvider;
        }


        /// <summary>
        /// Gets dictionary containing default user dashboard setting and corresponding dashboard items for the given user and site. 
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> or <paramref name="site"/> is null</exception>
        public Dictionary<UserDashboardSetting, DashboardItem> GetDefaultDashboardItems(UserInfo user, SiteInfo site)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (site == null)
            {
                throw new ArgumentNullException("site");
            }

            var roleIDs = user.GetRoleIdList(true, true, site.SiteName)
                              .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                              .Select(s => Convert.ToInt32(s))
                              .OrderBy(id => id)
                              .ToList();

            return mDashboardItemProvider.GetFilteredApplicationsForRoles(user, roleIDs)
                                         .OrderBy(app => app.ElementDisplayName)
                                         .ToDictionary(application => new UserDashboardSetting
                                         {
                                             ApplicationGuid = application.ElementGUID
                                         }, application => new DashboardItem
                                         {
                                             Application = application,
                                             IsVisible = true
                                         });
        }
    }
}
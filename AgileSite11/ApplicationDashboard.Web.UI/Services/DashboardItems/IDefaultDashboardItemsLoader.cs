using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.ApplicationDashboard.Web.UI.Internal;
using CMS.Membership;
using CMS.SiteProvider;

[assembly: RegisterImplementation(typeof(IDefaultDashboardItemsLoader), typeof(UserRolesDefaultDashboardItemsLoader), Priority = CMS.Core.RegistrationPriority.Fallback)]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Loads dictionary containing default user dashboard setting and corresponding dashboard items for the given user and site. 
    /// </summary>
    internal interface IDefaultDashboardItemsLoader
    {
        /// <summary>
        /// Gets dictionary containing default user dashboard setting and corresponding dashboard items for the given user and site. 
        /// </summary>
        Dictionary<UserDashboardSetting, DashboardItem> GetDefaultDashboardItems(UserInfo user, SiteInfo site);
    }
}

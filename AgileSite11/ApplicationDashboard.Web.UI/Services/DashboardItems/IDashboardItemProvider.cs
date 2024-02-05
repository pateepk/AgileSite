using System;
using System.Collections.Generic;

using CMS;
using CMS.ApplicationDashboard.Web.UI;
using CMS.Membership;
using CMS.Modules;

[assembly: RegisterImplementation(typeof(IDashboardItemProvider), typeof(DashboardItemProvider), Priority = CMS.Core.RegistrationPriority.Fallback, Lifestyle = CMS.Core.Lifestyle.Transient)]

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Contains methods for obtaining filtered lists of dashboard items.
    /// </summary>
    internal interface IDashboardItemProvider
    {
        /// <summary>
        /// Gets list of UI elements filtered for given user and roles.
        /// </summary>
        IList<UIElementInfo> GetFilteredApplicationsForRoles(UserInfo user, List<int> roleIDs);


        /// <summary>
        /// Gets list of UI elements filtered for given user and applications Guids.
        /// </summary>
        IList<UIElementInfo> GetFilteredApplicationsByGuids(UserInfo user, List<Guid> uiElementGuids);


        /// <summary>
        /// Gets UI element filtered for given user and application Guid.
        /// </summary>
        UIElementInfo GetFilteredApplicationByGuid(UserInfo user, Guid uiElementGuid);
    }
}
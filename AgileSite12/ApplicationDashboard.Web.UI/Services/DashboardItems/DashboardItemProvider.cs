using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Modules;
using CMS.UIControls;

namespace CMS.ApplicationDashboard.Web.UI
{
    /// <summary>
    /// Contains methods for obtaining filtered lists of applications.
    /// </summary>
    internal class DashboardItemProvider : IDashboardItemProvider
    {
        /// <summary>
        /// Gets list of UI elements filtered for given user and roles.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> or <paramref name="roleIDs"/> is null</exception>
        public IList<UIElementInfo> GetFilteredApplicationsForRoles(UserInfo user, List<int> roleIDs)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (roleIDs == null)
            {
                throw new ArgumentNullException("roleIDs");
            }

            var applicationsWhere = new WhereCondition()
                .WhereIn("ElementID", new IDQuery(RoleApplicationInfo.OBJECT_TYPE, "ElementID")
                .WhereIn("RoleID", roleIDs));

            return LoadAndFilterApplications(user, applicationsWhere);
        }


        /// <summary>
        /// Gets list of UI elements filtered for given user and applications Guids.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> or <paramref name="uiElementGuids"/> is null</exception>
        public IList<UIElementInfo> GetFilteredApplicationsByGuids(UserInfo user, List<Guid> uiElementGuids)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            if (uiElementGuids == null)
            {
                throw new ArgumentNullException("uiElementGuids");
            }

            var applicationsWhere = new WhereCondition().WhereIn("ElementGUID", uiElementGuids.Select(g => g.ToString()).ToList());

            return LoadAndFilterApplications(user, applicationsWhere);
        }


        /// <summary>
        /// Gets UI element filtered for given user and application Guid.
        /// </summary>
        /// <exception cref="ArgumentNullException"><paramref name="user"/> is null</exception>
        public UIElementInfo GetFilteredApplicationByGuid(UserInfo user, Guid uiElementGuid)
        {
            if (user == null)
            {
                throw new ArgumentNullException("user");
            }

            var applicationsWhere = new WhereCondition().WhereEquals("ElementGUID", uiElementGuid);

            return LoadAndFilterApplications(user, applicationsWhere).FirstOrDefault();
        }

        
        /// <summary>
        /// Gets list of UI elements filtered for given user and where condition.
        /// </summary>
        private IList<UIElementInfo> LoadAndFilterApplications(UserInfo user, WhereCondition applicationsWhere)
        {
            DataSet applicationsDataSet = ApplicationUIHelper.LoadApplications(applicationsWhere);

            applicationsDataSet = ApplicationUIHelper.FilterApplications(applicationsDataSet, user, false);

            if (DataHelper.DataSourceIsEmpty(applicationsDataSet))
            {
                return new List<UIElementInfo>();
            }

            return applicationsDataSet.Tables[0].Rows.Cast<DataRow>().Select(row => new UIElementInfo(row)).ToList();
        }
    }
}
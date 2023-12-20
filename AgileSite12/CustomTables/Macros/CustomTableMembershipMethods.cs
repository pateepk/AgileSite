using System.Data;

using CMS;
using CMS.CustomTables;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.SiteProvider;
using CMS.Core;
using CMS.DataEngine;

[assembly: RegisterExtension(typeof(CustomTableMembershipMethods), typeof(UserInfo))]

namespace CMS.CustomTables
{
    /// <summary>
    /// Custom table membership methods - wrapping methods for macro resolver.
    /// </summary>
    internal class CustomTableMembershipMethods : MacroMethodContainer
    {
        /// <summary>
        /// Returns true if user has read permission on at least one custom table.
        /// </summary>
        /// <param name="context">Evaluation context with child resolver</param>
        /// <param name="parameters">Method parameters</param>
        [MacroMethod(typeof(bool), "Returns true if user has read permission on at least one custom table", 1, IsHidden = true)]
        [MacroMethodParam(0, "user", typeof(object), "User info object.")]
        public static object IsAuthorizedPerCustomTablesData(EvaluationContext context, params object[] parameters)
        {
            UserInfo ui = parameters[0] as UserInfo;
            if (ui == null)
            {
                return false;
            }

            // User has module permission for READ, we don't need to check custom tables
            if (MembershipContext.AuthenticatedUser.IsAuthorizedPerResource(ModuleName.CUSTOMTABLES, "Read"))
            {
                return true;
            }

            var data = CustomTableHelper.GetCustomTableClasses(SiteContext.CurrentSiteID)
                                        .Columns("ClassID")
                                        .WhereIn("ClassID", new IDQuery<ClassSiteInfo>("ClassID")
                                                                .WhereEquals("SiteID", SiteContext.CurrentSiteID));
            // No custom tables are defined on this site
            if (DataHelper.DataSourceIsEmpty(data))
            {
                return false;
            }

            // Check permission to each custom table if user is not authorized to read all (from module)
            DataSet ds = CustomTableHelper.FilterTablesByPermission(data);

            return !DataHelper.DataSourceIsEmpty(ds);
        }
    }
}

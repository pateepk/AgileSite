using System;
using System.Collections.Generic;
using System.Data;

using CMS.CMSImportExport;
using CMS.Membership;
using CMS.Helpers;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Handles special actions during the workflow export process.
    /// </summary>
    internal static class ExportWorkflow
    {
        #region "Constants"

        const string WORKFLOWSTEPROLE_TABLE = "cms_workflowsteproles";

        #endregion


        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        internal static void Init()
        {
            ImportExportEvents.GetExportData.After += GetExportData_After;
        }


        private static void GetExportData_After(object sender, ExportGetDataEventArgs e)
        {
            var objectType = e.ObjectType;
            var data = e.Data;

            // Filter out all step-role bindings that belong to other sites
            if ((e.Settings.SiteId > 0) && (data != null) && (objectType == WorkflowInfo.OBJECT_TYPE) && data.Tables.Contains(WORKFLOWSTEPROLE_TABLE))
            {
                // Get all role IDs available on current site (including global roles)
                var roleIDsSet = new HashSet<int>();
                RoleInfoProvider.GetRoles().WhereEqualsOrNull("SiteID", e.Settings.SiteId).
                    AsIDQuery().ForEachRow(row => roleIDsSet.Add(ValidationHelper.GetInteger(row[0], 0)));

                // Remove unwanted step-role bindings
                var dataTableRows = data.Tables[WORKFLOWSTEPROLE_TABLE].Rows;
                for (int row = dataTableRows.Count - 1; row >= 0; row--)
                {
                    if (!roleIDsSet.Contains(ValidationHelper.GetInteger(dataTableRows[row]["RoleID"], 0)))
                    {
                        dataTableRows.RemoveAt(row);
                    }
                }
            }
        }

        #endregion
    }
}
using System;
using System.Data;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;

namespace CMS.WorkflowEngine
{
    using TypedDataSet = InfoDataSet<WorkflowScopeInfo>;

    /// <summary>
    /// Workflow scope info provider.
    /// </summary>
    public class WorkflowScopeInfoProvider : AbstractInfoProvider<WorkflowScopeInfo, WorkflowScopeInfoProvider>
    {
        #region "Methods"

        /// <summary>
        /// Returns all workflow scopes.
        /// </summary>
        public static ObjectQuery<WorkflowScopeInfo> GetWorkflowScopes()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns object with specified GUID.
        /// </summary>
        /// <param name="guid">Object GUID</param>
        /// <param name="siteId">Site ID</param>
        public static WorkflowScopeInfo GetWorkflowScopeInfoByGUID(Guid guid, int siteId)
        {
            return ProviderObject.GetInfoByGuid(guid, siteId);
        }


        /// <summary>
        /// Sets the specified workflow scope data.
        /// </summary>
        /// <param name="infoObj">Workflow scope data object</param>
        /// <exception cref="Exception">Throws exception if license limitation are not fulfilled</exception>
        public static void SetWorkflowScopeInfo(WorkflowScopeInfo infoObj)
        {
            ProviderObject.SetInfo(infoObj);
        }


        /// <summary>
        /// Returns the WorkflowScopeInfo structure for the specified workflow scope ID.
        /// </summary>
        /// <param name="scopeId">ID of the workflow scope to retrieve</param>
        public static WorkflowScopeInfo GetWorkflowScopeInfo(int scopeId)
        {
            return ProviderObject.GetInfoById(scopeId);
        }


        /// <summary>
        /// Deletes the specified workflow scope.
        /// </summary>
        /// <param name="wsi">Workflow scope object to delete</param>
        public static void DeleteWorkflowScopeInfo(WorkflowScopeInfo wsi)
        {
            ProviderObject.DeleteInfo(wsi);
        }


        /// <summary>
        /// Deletes the specified workflow scope.
        /// </summary>
        /// <param name="scopeId">Workflow scope ID to delete</param>
        public static void DeleteWorkflowScopeInfo(int scopeId)
        {
            WorkflowScopeInfo wsi = GetWorkflowScopeInfo(scopeId);
            DeleteWorkflowScopeInfo(wsi);
        }


        /// <summary>
        /// Returns the Dataset of all workflow steps for the given workflow.
        /// </summary>        
        /// <param name="workflowId">Workflow ID</param>
        public static TypedDataSet GetWorkflowScopes(int workflowId)
        {
            return GetWorkflowScopes().WhereEquals("ScopeWorkflowID", workflowId).TypedResult;
        }


        /// <summary>
        /// Returns the Dataset of all workflow steps.
        /// </summary>        
        /// <param name="where">Where condition</param>
        [Obsolete("Use method GetWorkflowScopes() instead")]
        public static TypedDataSet GetWorkflowScopes(string where)
        {
            return GetWorkflowScopes().Where(where).TypedResult;
        }


        /// <summary>
        /// Deletes the scopes of specified workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        public static void DeleteWorkflowScopes(int workflowId)
        {
            ProviderObject.DeleteWorkflowScopesInternal(workflowId);
        }


        /// <summary>
        /// Gets all workflow scopes.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by clause</param>
        /// <param name="topN">Number of records to return</param>
        /// <param name="columns">Columns to select, null or empty returns all columns</param>
        [Obsolete("Use method GetWorkflowScopes() instead")]
        public static TypedDataSet GetWorkflowScopes(string where, string orderBy, int topN, string columns)
        {
            return GetWorkflowScopes().Where(where).OrderBy(orderBy).TopN(topN).Columns(columns).TypedResult;
        }


        /// <summary>
        /// Checks if domain has license supporting workflow feature.
        /// </summary>
        /// <param name="wfsi">Scope info to check</param>        
        public static bool LicenseVersionCheck(WorkflowScopeInfo wfsi)
        {
            if (wfsi == null)
            {
                return true;
            }

            // Get scope site
            SiteInfo si = SiteInfoProvider.GetSiteInfo(wfsi.ScopeSiteID);
            if (si == null)
            {
                return true;
            }

            // Get limitations
            int versionLimitations = LicenseKeyInfoProvider.VersionLimitations(si.DomainName, FeatureEnum.WorkflowVersioning, false);

            // Return true if feature is without any limitations
            switch (versionLimitations)
            {
                case LicenseHelper.LIMITATIONS_UNLIMITED:
                    return true;

                case LicenseHelper.LIMITATIONS_BASICWORKFLOW:
                    {
                        // Only three steps - edit, publish, archive, ok
                        int currentCount = WorkflowStepInfoProvider.GetWorkflowSteps(wfsi.ScopeWorkflowID).Columns("StepID").Count;
                        if (currentCount <= 3)
                        {
                            return true;
                        }
                    }
                    break;

                case LicenseHelper.LIMITATIONS_NOITEMS:
                    break;
            }

            // License limit reached, log event
            string message = ResHelper.GetString("licenselimitation.workflow");
            EventLogProvider.LogEvent(EventType.WARNING, "Workflow", LicenseHelper.LICENSE_LIMITATION_EVENTCODE, message, RequestContext.CurrentURL);

            return false;
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Deletes the scopes of specified workflow.
        /// </summary>
        /// <param name="workflowId">Workflow ID</param>
        protected void DeleteWorkflowScopesInternal(int workflowId)
        {
            DataSet scopesDS = GetWorkflowScopes(workflowId);
            if (!DataHelper.DataSourceIsEmpty(scopesDS))
            {
                // Delete all scopes
                foreach (DataRow dr in scopesDS.Tables[0].Rows)
                {
                    // Delete the scope
                    int scopeId = ValidationHelper.GetInteger(dr["ScopeID"], 0);
                    DeleteWorkflowScopeInfo(scopeId);
                }
            }
        }

        #endregion
    }
}
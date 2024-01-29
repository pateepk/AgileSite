using System;
using System.ServiceModel.Activation;
using System.Linq;
using System.Text;

using CMS.Automation;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WorkflowEngine;
using CMS.EventLog;
using CMS.SiteProvider;
using CMS.Membership;

namespace CMS.WebServices
{
    /// <summary>
    /// Automation process graph service implementation.
    /// </summary>
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Required)]
    public class AutomationDesignerService : WorkflowDesignerService
    {
        #region "Methods"
        
        /// <summary>
        /// Gets event log source
        /// </summary>
        protected override string GetEventLogSource()
        {
            return "AUTOMATIONSERVICE";
        }


        /// <summary>
        /// Check user permissions.
        /// </summary>
        protected override bool IsAuthorized()
        {
            // Validate session token
            if (!SecurityHelper.ValidateSessionToken(CookieHelper.GetValue(CookieName.SessionToken), CookieName.SessionToken))
            {
                return false;
            }

            // Check permissions
            return WorkflowStepInfoProvider.CanUserManageAutomationProcesses(MembershipContext.AuthenticatedUser, SiteContext.CurrentSiteName);
        }


        /// <summary>
        /// Deletes node with dependencies check
        /// </summary>
        /// <param name="id">Node ID</param>
        protected override ServiceResponse DeleteNode(int id)
        {
            try
            {
                WorkflowStepInfoProvider.DeleteWorkflowStepInfo(id);
            }
            catch (CheckDependenciesException)
            {
                return GetBadRequestResponse(ResHelper.GetString("ma.process.CannotDeleteStepUsed"));
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException(GetEventLogSource(), "EXCEPTION", ex);
                return new ServiceResponse(ResponseStatusEnum.InternalError, ResHelper.GetString("workflowservice.removingstepfailed"));
            }

            return new ServiceResponse(ResponseStatusEnum.OK);
        }

        #endregion
    }
}

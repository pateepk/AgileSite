using System;

using CMS.Helpers;
using CMS.UIControls;

namespace CMS.WorkflowEngine.Web.UI
{
    /// <summary>
    /// Base page for the workflow pages.
    /// </summary>
    public abstract class CMSWorkflowPage : CMSPage
    {
        #region "Variables"

        private WorkflowInfo mCurrentWorkflow = null;
        private int mWorkflowId = 0;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current workflow ID
        /// </summary>
        public int WorkflowId
        {
            get
            {
                if (mWorkflowId <= 0)
                {
                    mWorkflowId = QueryHelper.GetInteger("workflowid", 0);
                }
                return mWorkflowId;
            }
        }


        /// <summary>
        /// Current workflow
        /// </summary>
        public virtual WorkflowInfo CurrentWorkflow
        {
            get
            {
                if (WorkflowId == 0)
                {
                    int workflowStepID = QueryHelper.GetInteger("workflowstepid", 0);
                    if (workflowStepID != 0)
                    {
                        var step =  WorkflowStepInfoProvider.GetWorkflowStepInfo(workflowStepID);
                        if(step != null)
                        {
                            mWorkflowId = step.StepWorkflowID;
                        }
                    }
                }

                if (mCurrentWorkflow == null)
                {
                    mCurrentWorkflow = WorkflowInfoProvider.GetWorkflowInfo(WorkflowId);
                }

                return mCurrentWorkflow;
            }
        }


        /// <summary>
        /// Type of the workflow
        /// </summary>
        public WorkflowTypeEnum WorkflowType
        {
            get
            {
                return (WorkflowTypeEnum)QueryHelper.GetInteger("type", 0);
            }
        }

        #endregion        
        

        #region "Methods"

        /// <summary>
        /// Ensures that current user can manage automation processes.
        /// </summary>
        protected void CheckProcessManagePermission()
        {
            if (!WorkflowStepInfoProvider.CanUserManageAutomationProcesses(CurrentUser, CurrentSiteName))
            {
                RedirectToAccessDenied(ResHelper.GetString("general.modifynotallowed"));
            }
        }

        #endregion


        /// <summary>
        /// Constructor
        /// </summary>
        public CMSWorkflowPage()
        {
            Load += BasePage_Load;
        }


        /// <summary>
        /// Load event handler
        /// </summary>
        protected void BasePage_Load(object sender, EventArgs e)
        {
            RedirectToSecured();

            SetRTL();
            SetBrowserClass();
            AddNoCacheTag();

            if(CurrentWorkflow == null)
            {
                return;
            }

            switch(CurrentWorkflow.WorkflowType)
            {
                case WorkflowTypeEnum.Automation:
                    CheckProcessManagePermission();
                    break;
                default:
                    CheckGlobalAdministrator();
                    break;
            }
        }
    }
}
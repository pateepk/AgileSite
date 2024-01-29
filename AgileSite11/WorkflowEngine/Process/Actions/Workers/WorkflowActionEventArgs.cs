using System;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Base;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Workflow action event arguments
    /// </summary>
    public class WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType> : CMSEventArgs, IWorkflowActionEventArgs
        where InfoType : BaseInfo
        where StateInfoType : BaseInfo
        where ActionEnumType : struct, IConvertible
    {
        #region "Variables"

        private WorkflowActionInfo mActionDefinition = null;
        private WorkflowStepInfo mActionStep = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Manager.
        /// </summary>
        public AbstractWorkflowManager<InfoType, StateInfoType, ActionEnumType> Manager
        {
            get;
            internal set;
        }


        /// <summary>
        /// Action definition.
        /// </summary>
        public WorkflowActionInfo ActionDefinition
        {
            get
            {
                if (mActionDefinition == null)
                {
                    mActionDefinition = WorkflowActionInfoProvider.GetWorkflowActionInfo(ActionStep.StepActionID);
                }

                return mActionDefinition;
            }
        }


        /// <summary>
        /// Parameters of action.
        /// </summary>
        public ObjectParameters Parameters
        {
            get
            {
                if (ActionStep != null)
                {
                    return ActionStep.StepActionParameters;
                }

                return null;
            }
        }


        /// <summary>
        /// User running action.
        /// </summary>
        public UserInfo User
        {
            get;
            internal set;
        }


        /// <summary>
        /// Current step.
        /// </summary>
        public WorkflowStepInfo ActionStep
        {
            get
            {
                return mActionStep;
            }
            internal set
            {
                mActionDefinition = null;
                mActionStep = value;
            }
        }


        /// <summary>
        /// Current step.
        /// </summary>
        public WorkflowStepInfo InitialStep
        {
            get;
            internal set;
        }


        /// <summary>
        /// Current step.
        /// </summary>
        public WorkflowStepInfo OriginalStep
        {
            get;
            internal set;
        }


        /// <summary>
        /// Current workflow.
        /// </summary>
        public WorkflowInfo Workflow
        {
            get;
            internal set;
        }


        /// <summary>
        /// Current info object.
        /// </summary>
        public InfoType InfoObject
        {
            get;
            internal set;
        }


        /// <summary>
        /// Info object representing automation state. Used for automation processes.
        /// </summary>
        public StateInfoType StateObject
        {
            get;
            internal set;
        }


        /// <summary>
        /// Comment used when action moves to next step.
        /// </summary>
        public string Comment
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if the process should be stopped. Process is not moved to the next step.
        /// </summary>
        public bool StopProcessing
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public WorkflowActionEventArgs()
        {
        }

        #endregion
    }
}

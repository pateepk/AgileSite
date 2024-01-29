using System;
using System.Collections;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.MacroEngine;
using CMS.Membership;
using CMS.Base;

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// General base class for workflow actions.
    /// </summary>
    public abstract class BaseWorkflowAction<InfoType, StateInfoType, ActionEnumType> : IWorkflowAction
        where InfoType : BaseInfo
        where StateInfoType : BaseInfo
        where ActionEnumType : struct, IConvertible
    {
        #region "Variables"

        private MacroResolver mMacroResolver = null;
        private Hashtable mMacroTable = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Action arguments
        /// </summary>
        public IWorkflowActionEventArgs Arguments
        {
            get;
            internal set;
        }


        /// <summary>
        /// Hash table for resolved parameters values
        /// </summary>
        private Hashtable MacroTable
        {
            get
            {
                return mMacroTable ?? (mMacroTable = new Hashtable(StringComparer.InvariantCultureIgnoreCase));
            }
        }

        #endregion


        #region "Support properties for better usability"

        /// <summary>
        /// Manager.
        /// </summary>
        public AbstractWorkflowManager<InfoType, StateInfoType, ActionEnumType> Manager
        {
            get
            {
                return ((WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType>)Arguments).Manager;
            }
        }


        /// <summary>
        /// Action definition.
        /// </summary>
        public WorkflowActionInfo ActionDefinition
        {
            get
            {
                return Arguments.ActionDefinition;
            }
        }


        /// <summary>
        /// Parameters of action.
        /// </summary>
        public ObjectParameters Parameters
        {
            get
            {
                return Arguments.Parameters;
            }
        }


        /// <summary>
        /// User running action.
        /// </summary>
        public UserInfo User
        {
            get
            {
                return Arguments.User;
            }
        }


        /// <summary>
        /// Current step.
        /// </summary>
        public WorkflowStepInfo ActionStep
        {
            get
            {
                return Arguments.ActionStep;
            }
        }


        /// <summary>
        /// Current step.
        /// </summary>
        public WorkflowStepInfo InitialStep
        {
            get
            {
                return Arguments.InitialStep;
            }
        }


        /// <summary>
        /// Current step.
        /// </summary>
        public WorkflowStepInfo OriginalStep
        {
            get
            {
                return Arguments.OriginalStep;
            }
        }


        /// <summary>
        /// Current workflow.
        /// </summary>
        public WorkflowInfo Workflow
        {
            get
            {
                return Arguments.Workflow;
            }
        }


        /// <summary>
        /// Current info object.
        /// </summary>
        public InfoType InfoObject
        {
            get
            {
                return ((WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType>)Arguments).InfoObject;
            }
            set
            {
                ((WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType>)Arguments).InfoObject = value;
            }
        }


        /// <summary>
        /// Current state object.
        /// </summary>
        public StateInfoType StateObject
        {
            get
            {
                return ((WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType>)Arguments).StateObject;
            }
            set
            {
                ((WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType>)Arguments).StateObject = value;
            }
        }


        /// <summary>
        /// Comment used when action moves to next step.
        /// </summary>
        public string Comment
        {
            get
            {
                return Arguments.Comment;
            }
            set
            {
                Arguments.Comment = value;
            }
        }


        /// <summary>
        /// Macro resolver for action.
        /// </summary>
        public MacroResolver MacroResolver
        {
            get
            {
                if (mMacroResolver == null)
                {
                    mMacroResolver = GetDefaultMacroResolver();
                }
                return mMacroResolver;
            }
        }

        #endregion

        /// <summary>
        /// Executes action. You can use parameters to allow users to modify the behavior.
        /// </summary>
        /// <example>
        /// To access the values of parameters, you can use this code.
        /// <code>
        /// GetResolvedParameter("StringParameterName", string.Empty);
        /// GetResolvedParameter("BooleanParameterName", false);
        /// </code>
        /// </example>
        public abstract void Execute();


        /// <summary>
        /// Prepares macro resolver for usage.
        /// </summary>
        protected virtual MacroResolver GetDefaultMacroResolver()
        {
            // Get resolver
            MacroResolver resolver = Manager.GetEmailResolver(InfoObject, StateObject, User, OriginalStep, ActionStep, Workflow, WorkflowHelper.GetWorkflowActionString(WorkflowActionEnum.Approve), Comment).CreateChild();

            // Register macro for action parameters
            foreach (var column in Parameters.ColumnNames)
            {
                resolver.SetNamedSourceData("Parameters." + column, resolver.ResolveMacros(ValidationHelper.GetString(Parameters[column], "")));
            }

            return resolver;
        }


        /// <summary>
        /// Gets resolved value of parameter.
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="defaultValue">Default value</param>
        protected virtual ReturnType GetResolvedParameter<ReturnType>(string parameterName, ReturnType defaultValue)
        {
            parameterName = parameterName.ToLowerCSafe();
            if (!MacroTable.Contains(parameterName))
            {
                string resolved = MacroResolver.ResolveMacros(ValidationHelper.GetString(Parameters[parameterName], null));
                MacroTable[parameterName] = string.IsNullOrEmpty(resolved) ? defaultValue : ValidationHelper.GetValue<ReturnType>(resolved);
            }

            // Check type
            var val = MacroTable[parameterName];
            if (val is ReturnType)
            {
                return (ReturnType)MacroTable[parameterName];
            }

            return defaultValue;
        }


        /// <summary>
        /// Processes action.
        /// </summary>
        /// <param name="args">Arguments of action</param>
        public void Process(IWorkflowActionEventArgs args)
        {
            // Check arguments type
            if (args as WorkflowActionEventArgs<InfoType, StateInfoType, ActionEnumType> == null)
            {
                throw new NotSupportedException("[BaseWorkflowAction.Process]: Unsupported arguments type.");
            }

            Arguments = args;
            Execute();
        }


        /// <summary>
        /// Logs message to event log.
        /// </summary>
        /// <param name="eventType">Type of the event. Please use predefined constants from EventLogProvider</param>
        /// <param name="eventCode">Event code (UPDATEDOC, SENDEMAIL, DELETE, etc.)</param>
        /// <param name="message">Message</param>
        /// <param name="infoObj">Info object</param>
        public virtual void LogMessage(string eventType, string eventCode, string message, InfoType infoObj)
        {
            Manager.LogMessage(eventType, eventCode, message, infoObj, User);
        }
    }
}

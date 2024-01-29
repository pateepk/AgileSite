using System;

using CMS.Helpers;
using CMS.Base;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Class for automation helper methods.
    /// </summary>
    public class AutomationHelper
    {
        /// <summary>
        /// Returns formatted string with process status name and CSS class. Only 'Processing' status is returned without CSS class.
        /// </summary>
        /// <param name="processStatus">Process status</param>
        public static string GetProcessStatus(ProcessStatusEnum processStatus)
        {
            string status = WorkflowHelper.GetProcessStatusString(processStatus);

            switch (processStatus)
            {
                case ProcessStatusEnum.Pending:
                    return HTMLHelper.SpanMsg(status, "alert-status-warning");

                case ProcessStatusEnum.Finished:
                    return HTMLHelper.SpanMsg(status, "alert-status-success");

                default:
                    return status;
            }
        }


        /// <summary>
        /// Returns formatted trigger name.
        /// </summary>
        /// <param name="type">Type of trigger</param>
        /// <param name="objectType">Object type</param>
        public static string GetTriggerName(WorkflowTriggerTypeEnum type, string objectType)
        {
            return String.Format("{0} {1}", ResHelper.GetString(TypeHelper.GetObjectTypeResourceKey(objectType)), WorkflowHelper.GetWorkflowTriggerTypeString(type).ToLowerCSafe());
        }
    }
}
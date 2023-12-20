using CMS.EventLog;
using CMS.Helpers;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Class for starting automation process on contact.
    /// </summary>
    public class StartProcessAction : AutomationAction
    {
        /// <summary>
        /// Activity title.
        /// </summary>
        protected virtual string ProcessName
        {
            get
            {
                return GetResolvedParameter<string>("ProcessName", null);
            }
        }


        /// <summary>
        /// Executes the action.
        /// </summary>
        public override void Execute()
        {
            // Get process
            WorkflowInfo process = WorkflowInfoProvider.GetWorkflowInfo(ProcessName, WorkflowTypeEnum.Automation);
            if (process != null)
            {
                if (process.WorkflowID == Workflow.WorkflowID)
                {
                    LogMessage(EventType.ERROR, "STARTPROCESS", ResHelper.GetAPIString("ma.action.startprocess.sameprocess", "Cannot start the same process."), InfoObject);
                }
                else
                {
                    try
                    {
                        // Start process
                        AutomationManager.StartProcess(InfoObject, process.WorkflowID);
                    }
                    catch (ProcessRecurrenceException e)
                    {
                        LogMessage(EventType.WARNING, "STARTPROCESS", e.Message, InfoObject);
                    }
                    catch (ProcessDisabledException e)
                    {
                        LogMessage(EventType.WARNING, "STARTPROCESS", e.Message, InfoObject);
                    }
                }
            }
            else
            {
                LogMessage(EventType.ERROR, "STARTPROCESS", ResHelper.GetAPIString("ma.action.startprocess.noprocess", "Automation process not found."), InfoObject);
            }
        }
    }
}

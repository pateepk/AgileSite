using CMS.DataEngine;
using CMS.WorkflowEngine;

namespace CMS.Automation
{
    /// <summary>
    /// Workflow handlers
    /// </summary>
    internal class AutomationHandlers
    {
        /// <summary>
        /// Initializes the marketing automation handlers.
        /// </summary>
        public static void Init()
        {
            ObjectEvents.Delete.Before += Delete_Before;
            WorkflowInfo.TYPEINFO.Events.Update.Before += Workflow_Update_Before;
            WorkflowInfo.TYPEINFO_PROCESS.Events.Delete.After += ClearTriggersCache;
        }


        /// <summary>
        /// Removes all triggers from cache.      
        /// </summary>
        private static void ClearTriggersCache(object sender, ObjectEventArgs e)
        {
            TriggerHelper.ClearHashtable(null);
        }


        /// <summary>
        /// Fires before workflow insert or update.
        /// </summary>
        private static void Workflow_Update_Before(object sender, ObjectEventArgs e)
        {
            var infoObj = e.Object as WorkflowInfo;

            // Clear trigger hashtables if automation process was enabled/disabled to reload actual state
            if ((infoObj != null) && (infoObj.WorkflowType == WorkflowTypeEnum.Automation) && infoObj.ItemChanged("WorkflowEnabled"))
            {
                e.CallWhenFinished(() => TriggerHelper.ClearHashtable(null));
            }
        }


        /// <summary>
        /// Executes before deletion of the object.
        /// Handles removing triggers for targeted deleted object by its type and ID.
        /// </summary>
        private static void Delete_Before(object sender, ObjectEventArgs e)
        {
            var obj = e.Object;

            if (obj.TypeInfo.IsTriggerTarget)
            {
                ObjectWorkflowTriggerInfoProvider.DeleteObjectsTriggers(obj.TypeInfo.ObjectType, new[] { obj.Generalized.ObjectID });
            }
        }
    }
}

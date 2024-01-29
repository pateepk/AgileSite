using CMS.CMSImportExport;
using CMS.Helpers;

namespace CMS.Scheduler
{
    /// <summary>
    /// Handles special actions during the import process.
    /// </summary>
    internal static class ImportSpecialActions
    {
        #region "Methods"

        /// <summary>
        /// Initializes export handlers
        /// </summary>
        public static void Init()
        {
            SpecialActionsEvents.ProcessMainObject.Before += ProcessMainObject_Before;
        }


        private static void ProcessMainObject_Before(object sender, ImportEventArgs e)
        {
            var settings = e.Settings;
            var taskInfo = e.Object as TaskInfo;
            var parameters = e.Parameters;

            using (new ImportSpecialCaseContext(settings))
            {
                if ((taskInfo == null) || (parameters.ObjectProcessType != ProcessObjectEnum.All))
                {
                    // object type is not TaskInfo or processing objects is not set
                    return;
                }

                if (!parameters.SkipObjectUpdate)
                {
                    // Only for existing objects, reset TaskExecutions counter
                    taskInfo.TaskExecutions = 0;

                    // Ensure enabled tasks are started after import
                    if (taskInfo.TaskEnabled && taskInfo.TaskNextRunTime == DateTimeHelper.ZERO_TIME)
                    {
                        var taskInterval = SchedulingHelper.DecodeInterval(taskInfo.TaskInterval);
                        taskInfo.TaskNextRunTime = SchedulingHelper.GetFirstRunTime(taskInterval);
                    }
                }

                // Reset TaskExecutingServerName as it only has meaning in current local context
                taskInfo.TaskExecutingServerName = null;
            }
        }

        #endregion
    }
}
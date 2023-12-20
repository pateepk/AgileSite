using System;
using System.Collections.Generic;

using CMS.DataEngine;
using CMS.Synchronization;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Import/export synchronization
    /// </summary>
    internal class ImportExportSynchronization
    {
        #region "Methods"

        /// <summary>
        /// Initializes the actions for synchronization
        /// </summary>
        public static void Init()
        {
            SynchronizationActionManager.RegisterAction(LogExport, CheckLogExport);
        }


        private static bool CheckLogExport(LogObjectChangeSettings settings)
        {
            if (!settings.WorkerCall)
            {
                settings.LogExportTask &= ExportTaskInfoProvider.CheckExportLogging(settings.InfoObj, settings.TaskType);
            }

            return settings.LogExportTask;
        }


        private static IEnumerable<ISynchronizationTask> LogExport(LogObjectChangeSettings settings)
        {
            List<ISynchronizationTask> tasks = null;

            if (settings.LogExportTask)
            {
                tasks = new List<ISynchronizationTask> { ExportTaskInfoProvider.LogTask(settings.InfoObj, settings.TaskType) };
            }

            return tasks;
        }

        #endregion
    }
}

using System;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Scheduler
{
    /// <summary>
    /// Provides an ITask interface for the temporary files deletion.
    /// </summary>
    public class DeleteImageEditorHistory : ITask
    {
        /// <summary>
        /// Executes the delete action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                // Delete temporary files older than the value specified in web.config
                int hours = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDeleteImageEditorHistoryOlderThan"], 24);
                TempFileInfoProvider.DeleteTempFiles(TempFileInfoProvider.IMAGE_EDITOR_FOLDER, DateTime.Now.AddHours(-hours));

                return null;
            }
            catch (Exception e)
            {
                // Log the exception
                EventLogProvider.LogException("Content", "EXCEPTION", e);

                return e.Message;
            }
        }
    }
}
using System;
using System.Web;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Scheduler;
using CMS.Base;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Provides an ITask interface for the temporary uploads deletion.
    /// </summary>
    public class TemporaryUploadsCleaner : ITask
    {
        /// <summary>
        /// Executes the publish action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                int hours = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDeleteTemporaryUploadFilesOlderThan"], 24);

                // Prepare the context
                DateTime time = DateTime.Now.Subtract(TimeSpan.FromHours(hours));

                // Delete the files
                StorageHelper.DeleteOldFiles(UploaderHelper.TempPath, time, false);

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
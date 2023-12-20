using System;
using System.Collections.Generic;

using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Scheduler;

namespace CMS.Base.Web.UI
{
    /// <summary>
    /// Provides an ITask interface for the temporary uploads deletion.
    /// </summary>
    public class TemporaryUploadsCleaner : ITask
    {
        internal static readonly HashSet<CleanupAction> CleanupActions = new HashSet<CleanupAction>();


        /// <summary>
        /// Executes the publish action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                int hours = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSDeleteTemporaryUploadFilesOlderThan"], 2);

                // Prepare the context
                DateTime time = DateTime.Now.Subtract(TimeSpan.FromHours(hours));

                // Delete the files
                StorageHelper.DeleteOldFiles(UploaderHelper.TempPath, time, false);

                foreach(var cleanupAction in CleanupActions)
                {
                    StorageHelper.DeleteOldFiles(DirectoryInfo.New(cleanupAction.FolderPath), time, false, cleanupAction.Recursive, cleanupAction.DeleteFileCallback);
                }

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
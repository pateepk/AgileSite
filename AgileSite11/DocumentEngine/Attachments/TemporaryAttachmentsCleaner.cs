using System;

using CMS.EventLog;
using CMS.Scheduler;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Provides an ITask interface for the temporary attachments deletion.
    /// </summary>
    public class TemporaryAttachmentsCleaner : ITask
    {
        /// <summary>
        /// Executes the purge action.
        /// </summary>
        /// <param name="task">Task to process</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                AttachmentInfoProvider.DeleteOldTemporaryAttachments();

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
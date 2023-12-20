using System;
using System.Security.Principal;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Class cleaning tasks that are not effective for application's current state.
    /// </summary>
    internal static class WebFarmTaskCleaner
    {
        /// <summary>
        /// Deletes memory synchronization web farm tasks which were created before application start.
        /// </summary>
        public static void DeleteOldMemorySynchronizationTasksAsync()
        {
            try
            {
                WebFarmTaskProcessor.Current.ProcessingPaused = true;
                var windowsIdentity = WindowsIdentity.GetCurrent();

                var thread = new CMSThread(() => DeleteOldMemorySynchronizationTasksInternal(windowsIdentity), new ThreadSettings
                {
                    // Context values are not needed 
                    UseEmptyContext = true
                })
                { };

                thread.OnStop += (o, sender) => { WebFarmTaskProcessor.Current.ProcessingPaused = false; };
                thread.Start();
            }
            catch (Exception e)
            {
                WebFarmTaskProcessor.Current.ProcessingPaused = false;
                EventLogProvider.LogException("WebFarmTaskCleaner", "MEMORYTASKSREMOVAL", e);
            }
        }


        /// <summary>
        /// Deletes memory synchronization web farm tasks which were created before application start.
        /// </summary>
        /// <param name="windowsIdentity">Windows identity.</param>
        internal static void DeleteOldMemorySynchronizationTasksInternal(WindowsIdentity windowsIdentity)
        {
            WindowsImpersonationContext ctx = null;

            try
            {
                // Impersonate current thread
                ctx = windowsIdentity?.Impersonate();

                WebFarmTaskInfoProvider.DeleteServerMemoryTasks(WebFarmContext.ServerId, CMSApplication.ApplicationStart);
            }
            finally
            {
                ctx?.Undo();
            }
        }
    }
}

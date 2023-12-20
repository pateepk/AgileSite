using System;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.IO;
using CMS.Scheduler;
using CMS.SiteProvider;

namespace CMS.OutputFilter
{
    /// <summary>
    /// Scheduler task to clean the output file system cache. Deletes all files older than current file system output cache minutes
    /// </summary>
    public class FileSystemCacheCleaner : ITask
    {
        /// <summary>
        /// Executes the scheduled task
        /// </summary>
        /// <param name="task">Task to execute</param>
        public string Execute(TaskInfo task)
        {
            try
            {
                var context = CMSHttpContext.Current;
                if (context != null)
                {
                    string siteName = SiteContext.CurrentSiteName;
                    if (!String.IsNullOrEmpty(siteName))
                    {
                        int minutes = SettingsKeyInfoProvider.GetIntValue(siteName + ".CMSFileSystemOutputCacheMinutes");
                        if (minutes >= 0)
                        {
                            // Prepare the context
                            string path = StorageHelper.GetFullFilePhysicalPath(OutputHelper.PersistentDirectory, null).TrimEnd('\\') + "\\" + siteName;
                            DateTime time = DateTime.Now.Subtract(TimeSpan.FromMinutes(minutes));

                            // Delete the files
                            StorageHelper.DeleteOldFiles(path, time);
                        }
                    }
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

using System;
using System.Data;
using System.IO;
using System.Threading;

using CMS.Base;
using CMS.Core;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Web sync helper.
    /// </summary>
    internal class AnonymousTasksProcesor
    {
#pragma warning disable BH1014 // Do not use System.IO

        #region "Constants"

        /// <summary>
        /// File name of the file for web farm server notification
        /// </summary>
        public const string NOTIFY_FILENAME = "webfarm.sync";

        #endregion


        #region "Variables"

        private static string mNotifyPath;
        private static FileSystemWatcher mNotifyWatcher;
        private static int busy;

        #endregion


        #region "Private properties"

        /// <summary>
        /// Physical path to the watcher folder for web farm server notification.
        /// </summary>
        internal static string NotifyPath
        {
            get
            {
                return mNotifyPath ?? (mNotifyPath = GetNotificationPath(SystemContext.WebApplicationPhysicalPath));
            }
            set
            {
                mNotifyPath = value;
            }
        }


        /// <summary>
        /// File watcher to notify web farm server to process tasks.
        /// </summary>
        internal static FileSystemWatcher NotifyWatcher
        {
            get
            {
                if (!SystemContext.IsRunningOnAzure && (mNotifyWatcher == null))
                {
                    // Initialize sync file
                    NotifyServer(NotifyPath);

                    // Init watcher
                    mNotifyWatcher = new FileSystemWatcher(NotifyPath, NOTIFY_FILENAME);
                    mNotifyWatcher.NotifyFilter = NotifyFilters.LastWrite;
                    mNotifyWatcher.EnableRaisingEvents = true;
                }

                return mNotifyWatcher;
            }
            set
            {
                mNotifyWatcher = value;
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Gets default notification path for given server physical path
        /// </summary>
        /// <param name="serverPhysicalPath">Server physical path</param>
        internal static string GetNotificationPath(string serverPhysicalPath)
        {
            if (serverPhysicalPath == null)
            {
                return null;
            }

            return serverPhysicalPath.TrimEnd('\\') + "\\App_Data\\CMSModules\\WebFarm";
        }


        /// <summary>
        /// Notifies a web farm server about a new task.
        /// </summary>
        /// <param name="notificationPath">Notification path in UNC format. Use WebSyncHelperClass.GetNotificationPath(string serverPhysicalPath) method to get default system notification path.</param>
        internal static void NotifyServer(string notificationPath)
        {
            string path = Path.Combine(SystemContext.WebApplicationPhysicalPath, notificationPath);

            // Ensure path
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            // Touch file to restart service
            path = IO.Path.EnsureEndBackslash(path) + NOTIFY_FILENAME;
            using (var stream = new StreamWriter(new FileStream(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)))
            {
                stream.Write(true);
            }
        }


        /// <summary>
        /// Initializes file system watchers
        /// </summary>
        internal static void RegisterWatchers()
        {
            // Register file system watchers for next changes
            if (SystemContext.IsRunningOnAzure)
            {
                return;
            }
            if (SystemContext.IsFullTrustLevel)
            {
                NotifyWatcher.Changed += NotifyWatcher_Changed;
            }
            else
            {
                CoreServices.EventLog.LogEvent("W", "Web farms", "FileSystemWatcherNotEnabled", "The file system watcher was not enabled as it requires fully trusted application.");
            }
        }


        /// <summary>
        /// Handles changed event of file system watcher.
        /// </summary>
        /// <param name="sender">File system watcher</param>
        /// <param name="e">File system event argument</param>
        internal static void NotifyWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Allow empty context - This thread isn't raised from request 
                CMSThread.AllowEmptyContext();

                // Temporarily disable raising events because event OnChange is called twice when file is changed
                NotifyWatcher.EnableRaisingEvents = false;

                // Locking mechanism to prevent entering multiple notifications to processing part
                if (Interlocked.Exchange(ref busy, 1) == 1)
                {
                    return;
                }
                try
                {
                    // Process web farm tasks
                    while (new WebFarmTaskProcessor().ProcessTasks(GetAnonymousTasks(), WebFarmTaskInfoProvider.DeleteAnonymousTasks))
                    {
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref busy, 0);
                }
            }
            catch (Exception ex)
            {
                // Log exception
                CoreServices.EventLog.LogException("FileSystemWatcher", "Changed", ex);
            }
            finally
            {
                // Enable raising events
                NotifyWatcher.EnableRaisingEvents = true;
            }
        }


        /// <summary>
        /// Gets dataset with anonymous tasks with additional information about presence of binary data of the tasks.
        /// </summary>
        private static DataSet GetAnonymousTasks()
        {
            return WebFarmTaskInfoProvider.GetWebFarmTasksInternal()
                .Columns("TaskID", "TaskType", "TaskTextData", "TaskTarget", "CAST(CASE WHEN TaskBinaryData IS NULL THEN 0 ELSE 1 END AS bit) as TaskHasBinaryData")
                .WhereEquals("TaskIsAnonymous", 1)
                .WhereNull("TaskErrorMessage")
                .TopN(WebFarmTaskProcessor.TaskBatchSize)
                .Result;
        }

        #endregion

#pragma warning restore BH1014 // Do not use System.IO
    }
}
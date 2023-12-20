using System;
using System.Data;
using System.IO;
using System.Threading;

using CMS.Base;
using CMS.Core;

namespace CMS.WebFarmSync
{
    /// <summary>
    /// Class responsible for processing anonymous tasks.
    /// </summary>
    /// <remarks>
    /// Anonymous tasks are a way to run webfarm tasks on instance without webfarms enabled. Processor also bypasses license limitations.
    /// Processing is started by <see cref="NotifyServer()"/> and a batch of size <see cref="WebFarmTaskProcessor.TaskBatchSize"/> is processed.
    /// </remarks>
    /// <seealso cref="WebFarmTaskProcessor"/>
    internal class AnonymousTasksProcessor
    {
#pragma warning disable BH1014 // Do not use System.IO

        /// <summary>
        /// File name of the file for web farm server notification
        /// </summary>
        public const string NOTIFY_FILENAME = "webfarm.sync";

        private const string DEFAULT_NOTIFICATION_PATH = "App_Data\\CMSModules\\WebFarm";
               
        private static string mNotificationPath;
        private static FileSystemWatcher mNotifyWatcher;
        private static int busy;


        /// <summary>
        /// Physical path to the watcher folder for web farm server notification.
        /// </summary>
        private static string NotificationPath => mNotificationPath ?? (mNotificationPath = GetNotificationPath());


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
                    NotifyServer();

                    // Init watcher
                    mNotifyWatcher = new FileSystemWatcher(NotificationPath, NOTIFY_FILENAME);
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


        /// <summary>
        /// Gets default notification path for given server physical path
        /// </summary>
        private static string GetNotificationPath()
        {
            if (SystemContext.WebApplicationPhysicalPath == null)
            {
                return null;
            }

            return Path.Combine(SystemContext.WebApplicationPhysicalPath, DEFAULT_NOTIFICATION_PATH);
        }


        /// <summary>
        /// Notifies instance to process a batch of anonymous webfarm tasks.
        /// </summary>
        internal static void NotifyServer()
        {
            var notificationPath = NotificationPath;

            // Ensure path
            if (!Directory.Exists(notificationPath))
            {
                Directory.CreateDirectory(notificationPath);
            }

            // Touch file to restart service
            notificationPath = IO.Path.EnsureEndBackslash(notificationPath) + NOTIFY_FILENAME;
            using (var stream = new StreamWriter(new FileStream(notificationPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite)))
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

            NotifyWatcher.Changed += NotifyWatcher_Changed;
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

#pragma warning restore BH1014 // Do not use System.IO
    }
}
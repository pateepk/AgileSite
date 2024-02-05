using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Synchronization
{
    /// <summary>
    /// Overall staging methods.
    /// </summary>
    public class StagingTaskRunner
    {
        #region "Constants and variables"

        /// <summary>
        /// Authentication of the token was processed.
        /// </summary>
        public const string AUTHENTICATION_PROCESSED = "AUTH_PROCESSED";

        private static bool? mTreatServerNamesAsInstances;
        private int serverId;
        private int siteId;
        private Action<string> logCallback;
        private ServerInfo server;
        private List<int> deleteList = new List<int>();
        private List<int> deleteListGlobal = new List<int>();

        #endregion


        #region "Properties"

        ///<summary>
        /// Indicates whether to delete instance global tasks (tasks belonging to servers with same ServerName) after synchronization
        ///</summary>
        public static bool TreatServerNamesAsInstances
        {
            get
            {
                if (mTreatServerNamesAsInstances == null)
                {
                    mTreatServerNamesAsInstances = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSStagingTreatServerNamesAsInstances"], false);
                }

                return mTreatServerNamesAsInstances.Value;
            }
            set
            {
                mTreatServerNamesAsInstances = value;
            }
        }

        #endregion


        #region "Common methods"

        /// <summary>
        /// Returns true if the staging server is enabled.
        /// </summary>
        /// <param name="siteName">Site name to check</param>
        public static bool IsServerEnabled(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSStagingServiceEnabled");
        }


        /// <summary>
        /// Returns server authentication type for specified site.
        /// </summary>
        /// <param name="siteName">Site name to check</param>
        public static ServerAuthenticationEnum ServerAuthenticationType(string siteName)
        {
            switch (SettingsKeyInfoProvider.GetValue(siteName + ".CMSStagingServiceAuthentication").ToLowerCSafe())
            {
                // X509 authentication
                case "x509":
                    return ServerAuthenticationEnum.X509;

                // Username/password authentication - default
                default:
                    return ServerAuthenticationEnum.UserName;
            }
        }


        /// <summary>
        /// Returns the SHA1 hash byte array for given password string.
        /// </summary>
        /// <param name="password">Password string</param>
        public static byte[] GetSHA1HashByteArray(string password)
        {
            SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
            byte[] inputBuffer = Encoding.Unicode.GetBytes(password);
            byte[] result = sha1.ComputeHash(inputBuffer);
            return result;
        }


        /// <summary>
        /// Returns the hash for the specified password.
        /// </summary>
        /// <param name="password">Password to hash</param>
        public static string GetSHA1Hash(string password)
        {
            password = Convert.ToBase64String(GetSHA1HashByteArray(password));
            return password;
        }


        /// <summary>
        /// Returns the task type string.
        /// </summary>
        /// <param name="taskType">Task type</param>
        public static string GetTaskTypeString(TaskTypeEnum taskType)
        {
            return TaskHelper.GetTaskTypeString(taskType);
        }


        /// <summary>
        /// Returns the task type enumeration value.
        /// </summary>
        /// <param name="taskType">String task type representation</param>
        public static TaskTypeEnum GetTaskTypeEnum(string taskType)
        {
            return TaskHelper.GetTaskTypeEnum(taskType);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Initializes helper for synchronization.
        /// </summary>
        /// <param name="serverId">ID of synchronized server (zero for all servers)</param>
        /// <param name="siteId">ID of synchronized site.</param>
        /// <param name="logCallback">Callback to be called </param>
        public StagingTaskRunner(int serverId = SynchronizationInfoProvider.ENABLED_SERVERS, int siteId = 0, Action<string> logCallback = null)
        {
            this.serverId = serverId;
            this.siteId = siteId;
            this.logCallback = logCallback ?? new Action<string>((r) => { });

            if (serverId > 0)
            {
                server = ServerInfoProvider.GetServerInfo(serverId);
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Runs the task synchronization for specified server and list of task IDs.
        /// </summary>
        /// <param name="taskIDs">DataSet with task IDs and titles to synchronize</param>
        public string RunSynchronization(DataSet taskIDs)
        {
            return RunSynchronization(new InfoDataSet<StagingTaskInfo>(taskIDs).Select(x => x.TaskID));
        }


        /// <summary>
        /// Runs the task synchronization for specified server and list of task IDs.
        /// </summary>
        /// <param name="taskIDs">Task IDs to synchronize</param>
        public string RunSynchronization(IEnumerable<string> taskIDs)
        {
            return RunSynchronization(ValidationHelper.GetIntegers(taskIDs.ToArray(), 0));
        }


        /// <summary>
        /// Runs the task synchronization for specified server and task.
        /// </summary>
        /// <param name="taskID">Task ID to synchronize</param>
        public string RunSynchronization(int taskID)
        {
            return RunSynchronization(Enumerable.Repeat(taskID, 1));
        }


        /// <summary>
        /// Runs the task synchronization for specified server and list of task IDs.
        /// </summary>
        /// <param name="taskIDs">Task IDs to synchronize</param>
        public string RunSynchronization(IEnumerable<int> taskIDs)
        {
            return RunSynchronizationInternal(StagingTaskInfoProvider.GetTasksForSynchronization(taskIDs, serverId));
        }


        /// <summary>
        /// Runs the task synchronization for specified server and list of tasks.
        /// </summary>
        /// <param name="tasks">Tasks to synchronize</param>
        private string RunSynchronizationInternal(IEnumerable<StagingTaskInfo> tasks)
        {
            if (tasks == null || !tasks.Any())
            {
                return null;
            }

            string result = null;

            // Handle the event
            using (var h = StagingEvents.Synchronize.StartEvent())
            {
                if (h.CanContinue())
                {
                    StagingTaskInfo task = null;
                    var standAlone = tasks.Count() == 1;
                    var tasksSyncs = SynchronizationInfoProvider.GetTasksSynchronization(tasks.Select(x => x.TaskID).ToList(), serverId, siteId).OrderBy("SynchronizationTaskID");

                    // Run the synchronization
                    foreach (var taskSync in tasksSyncs)
                    {
                        if (task == null || task.TaskID != taskSync.SynchronizationTaskID)
                        {
                            task = tasks.First(t => t.TaskID == taskSync.SynchronizationTaskID);
                            logCallback(string.Format(ResHelper.GetAPIString("synchronization.running", "Processing '{0}' task"), HTMLHelper.HTMLEncode(task.TaskTitle)));
                        }
                        result += RunSynchronization(task, taskSync, standAlone);
                    }
                }

                DeleteProcessedTasks();

                if (String.IsNullOrEmpty(result))
                {
                    h.FinishEvent();
                }
            }

            return result;
        }


        /// <summary>
        /// Removes tasks that were processed by current instance.
        /// </summary>
        private void DeleteProcessedTasks()
        {
            SynchronizationInfoProvider.DeleteInstanceGlobalTasks(deleteListGlobal, server);
            SynchronizationInfoProvider.DeleteSynchronizationInfos(deleteList, serverId, siteId);

            var taskIDs = deleteListGlobal.Union(deleteList).ToList();

            StagingTaskInfoProvider.DeleteOrphanedTasks(taskIDs);
        }


        /// <summary>
        /// Runs the task synchronization for specified server.
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="taskSync">Task synchronization info</param>
        /// <param name="standAlone">If true, the task is synchronized on it's own</param>
        private string RunSynchronization(StagingTaskInfo task, SynchronizationInfo taskSync, bool standAlone)
        {
            if (DataHelper.GetNotEmpty(RequestContext.CurrentDomain, string.Empty) != string.Empty)
            {
                LicenseHelper.CheckFeatureAndRedirect(RequestContext.CurrentDomain, FeatureEnum.Staging);
            }

            string result = null;
            
            if (task == null || taskSync == null)
            {
                return string.Empty;
            }

            // Run the synchronization handler
            AdvancedHandler sh = null;
            if (standAlone)
            {
                sh = StagingEvents.Synchronize.StartEvent();
            }

            if (sh.CanContinue())
            {
                using (sh)
                {
                    // Handle the event
                    using (var h = StagingEvents.SynchronizeTask.StartEvent(task))
                    {
                        if (h.CanContinue())
                        {
                            result = RunTaskSynchronization(task, taskSync);
                        }

                        // Finish the SynchronizeTask event in case of success
                        if (String.IsNullOrEmpty(result))
                        {
                            h.FinishEvent();
                        }
                    }

                    if (sh != null)
                    {
                        // Finish the Synchronize event in case of success
                        if (String.IsNullOrEmpty(result))
                        {
                            sh.FinishEvent();
                        }
                    }
                }
            }
            return result;
        }




        /// <summary>
        /// Runs the synchronization for a single task
        /// </summary>
        /// <param name="task">Task data</param>
        /// <param name="taskSync">Task synchronization info</param>
        private string RunTaskSynchronization(StagingTaskInfo task, SynchronizationInfo taskSync)
        {
            // Get the server info
            ServerInfo si = server ?? ServerInfoProvider.GetServerInfo(taskSync.SynchronizationServerID);
            if (si == null)
            {
                throw new Exception("[StagingHelper.RunSynchronization]: Server ID '" + taskSync.SynchronizationServerID + "' not found.");
            }

            // Run the task
            ISyncClient client = Service.Resolve<ISyncClient>();
            client.Server = si;
            string result = client.RunTask(task);

            if (!string.IsNullOrEmpty(result))
            {
                // Log the error
                LogSynchronizationError(task, taskSync, result);
            }
            else
            {
                // Delete instance global tasks
                if (TreatServerNamesAsInstances && (task.TaskSiteID == 0))
                {
                    deleteListGlobal.Add(taskSync.SynchronizationTaskID);
                }

                // Delete the synchronization record
                deleteList.Add(taskSync.SynchronizationTaskID);
            }

            return result;
        }


        /// <summary>
        /// Logs the synchronization error.
        /// </summary>
        /// <param name="task">Task</param>
        /// <param name="taskSync">Task synchronization info</param>
        /// <param name="result">Result</param>
        private void LogSynchronizationError(StagingTaskInfo task, SynchronizationInfo taskSync, string result)
        {
            if (taskSync == null)
            {
                throw new ArgumentNullException("taskSync");
            }
            if (task == null)
            {
                return;
            }

            // Get the task
            try
            {
                // Update/create synchronization record
                taskSync.SynchronizationLastRun = DateTime.Now;
                taskSync.SynchronizationErrorMessage = result;
                SynchronizationInfoProvider.SetSynchronizationInfo(taskSync);
            }
            catch
            {
                // Error wasn't logged most probably because the task has been already deleted
            }
        }

        #endregion
    }
}
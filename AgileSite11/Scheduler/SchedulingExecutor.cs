using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Localization;
using CMS.MacroEngine;
using CMS.Modules;
using CMS.SiteProvider;
using CMS.WebFarmSync;

namespace CMS.Scheduler
{
    /// <summary>
    /// Methods for executing scheduled tasks.
    /// </summary>
    public static class SchedulingExecutor
    {
        #region "Variables"

        /// <summary>
        /// Counter of running tasks.
        /// </summary>
        private static readonly Lazy<IPerformanceCounter> mRunningTasks = new Lazy<IPerformanceCounter>(Service.Resolve<IPerformanceCounter>);


        /// <summary>
        /// Request timeout.
        /// </summary>
        private static int mScriptTimeout = -1;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the script timeout in seconds.
        /// </summary>
        public static int ScriptTimeout
        {
            get
            {
                if (mScriptTimeout < 0)
                {
                    mScriptTimeout = ValidationHelper.GetInteger(SettingsHelper.AppSettings["CMSSchedulerScriptTimeout"], 7200);
                }

                return mScriptTimeout;
            }
            set
            {
                mScriptTimeout = value;
            }
        }


        /// <summary>
        /// Counter of running tasks.
        /// </summary>
        public static IPerformanceCounter RunningTasks
        {
            get
            {
                return mRunningTasks.Value;
            }
        }


        /// <summary>
        /// Indicates if corrupted tasks should be re-initialized before next task execution.
        /// Occurs if the last execution failed.
        /// </summary>
        private static bool ReInitTasks
        {
            get;
            set;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets all scheduled tasks and executes each of them.
        /// </summary>
        /// <param name="siteName">Name of site task should originate from</param>
        /// <param name="serverName">Name of server that executes the tasks</param>
        public static void ExecuteScheduledTasks(string siteName, string serverName)
        {
            // Disable debugging if needed
            var debugs = DebugHelper.DisableSchedulerDebug();

            if (!SchedulingHelper.EnableScheduler)
            {
                return;
            }

            // Set the culture
            string culture = CultureHelper.GetDefaultCultureCode(siteName);
            if (culture != null)
            {
                System.Globalization.CultureInfo cultureInfo = CultureHelper.GetCultureInfo(culture);
                Thread.CurrentThread.CurrentCulture = cultureInfo;
                Thread.CurrentThread.CurrentUICulture = cultureInfo;
            }

            if (ReInitTasks)
            {
                // Re-initialize corrupted tasks
                ReInitCorruptedTasks();

                ReInitTasks = false;
            }

            // Execute all tasks, batch by batch
            var lastProcessedTaskId = 0;
            do
            {
                InfoDataSet<TaskInfo> fetchedRows = FetchTasksBatch(siteName, serverName, lastProcessedTaskId);
                if (DataHelper.DataSourceIsEmpty(fetchedRows))
                {
                    break;
                }

                lastProcessedTaskId = ExecuteTasksBatch(fetchedRows, siteName);
            } while (lastProcessedTaskId > 0);

            // Restore original debug settings
            DebugHelper.RestoreDebugSettings(debugs);
        }


        /// <summary>
        /// Executes specified task.
        /// </summary>
        /// <param name="taskInfo">Task to execute</param>
        /// <param name="siteName">Current site name of the execution context</param>
        public static void ExecuteTask(TaskInfo taskInfo, string siteName)
        {
            try
            {
                if (taskInfo == null)
                {
                    return;
                }

                taskInfo.CurrentSiteName = siteName;
                if (taskInfo.TaskRunInSeparateThread && SystemContext.IsWebSite)
                {
                    if (!TaskResourceAvailable(taskInfo))
                    {
                        return;
                    }

                    // Run task in separate thread, if not run from external application
                    TaskExecutor.RunAsync(taskInfo, WindowsIdentity.GetCurrent());
                }
                else
                {
                    // Run task in standard way
                    ExecuteTask(taskInfo);
                }
            }
            catch (Exception ex)
            {
                EventLogProvider.LogException("Scheduler", "ExecuteTask", ex);

                ReInitTasks = true;
            }
        }


        /// <summary>
        /// Executes specified task.
        /// </summary>
        public static void ExecuteTask(TaskInfo taskInfo)
        {
            if (taskInfo == null)
            {
                throw new ArgumentNullException(nameof(taskInfo));
            }

            if (!SchedulingHelper.EnableScheduler|| !TaskResourceAvailable(taskInfo))
            {
                return;
            }
            
            PrepareTaskForExecution(taskInfo);
            ExecuteTaskInternal(taskInfo);
        }


        /// <summary>
        /// Gets the task instance for execution
        /// </summary>
        /// <param name="taskInfo">Task info</param>
        internal static object GetTaskInstance(TaskInfo taskInfo)
        {
            if (taskInfo == null)
            {
                return null;
            }

            return ClassHelper.GetClass(taskInfo.TaskAssemblyName, taskInfo.TaskClass);
        }


        /// <summary>
        /// Gets the delegate from the given task object
        /// </summary>
        /// <param name="taskImplementation">Task implementation</param>
        internal static Func<TaskInfo, string> GetTaskDelegate(object taskImplementation)
        {
            // Try advanced task first
            var iTask = taskImplementation as ITask;
            if (iTask != null)
            {
                return taskInfo => iTask.Execute(taskInfo);
            }

            // Try simple worker task
            var worker = taskImplementation as IWorkerTask;
            if (worker != null)
            {
                return _ => worker.Execute();
            }

            return null;
        }


        /// <summary>
        /// Returns query of <see cref="TaskInfo"/> objects (scheduled tasks) that
        /// were interrupted during their scheduled execution.
        /// </summary>
        internal static ObjectQuery<TaskInfo> GetRelevantCorruptedTasks()
        {
            // Return enabled tasks with RUNNING status
            var tasks = TaskInfoProvider.GetTaskToReInit();

            if (!WebFarmContext.WebFarmEnabled)
            {
                return tasks;
            }

            // Return list of names of servers that are currently considered to be operating (able to execute scheduled tasks on their own)
            var serverNames = GetOperatingServerNames().ToList();
            if (!serverNames.Any())
            {
                return tasks;
            }

            return tasks.Where(
                new WhereCondition()
                    .WhereEmpty("TaskExecutingServerName")
                    .Or()
                    .WhereEquals("TaskExecutingServerName", WebFarmHelper.ServerName)
                    .Or()
                    .WhereNotIn("TaskExecutingServerName", serverNames));
        }


        /// <summary>
        /// Checks and returns whether the resource specified by task is available or not.
        /// Saves the "Last result" message if resource is not available.
        /// </summary>
        /// <param name="task">Task to check the resource availability</param>
        private static bool TaskResourceAvailable(TaskInfo task)
        {
            if (ResourceInfoProvider.IsResourceAvailable(task.TaskResourceID))
            {
                return true;
            }

            // Get the module
            var resourceInfo = ResourceInfoProvider.GetResourceInfo(task.TaskResourceID);
            if (resourceInfo == null)
            {
                return false;
            }

            // Module not available
            task.TaskLastResult = String.Format(ResHelper.GetString("ScheduledTask.NotInstalled"), resourceInfo.ResourceDisplayName);
            TaskInfoProvider.SetTaskInfo(task);
            return false;
        }


        /// <summary>
        /// Set task properties in DB so it is not executed by another server and executing server name is known and so forth
        /// </summary>
        /// <param name="taskInfo">Task to prepare for execution</param>
        private static void PrepareTaskForExecution(TaskInfo taskInfo)
        {
            // Does not apply for ad-hoc tasks
            if (taskInfo.TaskID <= 0)
            {
                return;
            }

            // Set TaskStatus to Running so no other scheduler picks this task (prevent double execution)
            taskInfo.TaskIsRunning = true;

            // Set executing server name (will remain empty for single-server scenarios and external services by design)
            taskInfo.TaskExecutingServerName = WebFarmHelper.ServerName;

            // Store info in DB only changes were made, i.e. task was not fetched automatically by FetchTasksToRun procedure
            if (taskInfo.ChangedColumns().Any())
            {
                TaskInfoProvider.SetTaskInfo(taskInfo);
            }
        }


        /// <summary>
        /// Returns a small set of tasks that are supposed to be executed.
        /// </summary>
        /// <param name="siteName">Name of site task should belong to</param>
        /// <param name="serverName">Name of server that executes the tasks</param>
        /// <param name="lastProcessedTaskId">ID of last task that was processed. All fetched tasks will have higher ID.</param>
        private static InfoDataSet<TaskInfo> FetchTasksBatch(string siteName, string serverName, int lastProcessedTaskId)
        {
            InfoDataSet<TaskInfo> fetchedRows;
            if (SchedulingHelper.UseExternalService)
            {
                bool externalService = !SystemContext.IsWebSite;

                // Get batch of tasks that should be processed by application or external service
                fetchedRows = TaskInfoProvider.FetchTasksToRun(siteName, serverName, lastProcessedTaskId, externalService);
            }
            else
            {
                // Get batch of all tasks
                fetchedRows = TaskInfoProvider.FetchTasksToRun(siteName, serverName, lastProcessedTaskId);
            }

            return fetchedRows;
        }


        /// <summary>
        /// Executes each <see cref="TaskInfo"/> stored in <paramref name="fetchedTasks"/>.
        /// </summary>
        /// <param name="fetchedTasks">Tasks to execute</param>
        /// <param name="siteName">Current site name of the execution context</param>
        /// <returns>ID of last executed task</returns>
        private static int ExecuteTasksBatch(InfoDataSet<TaskInfo> fetchedTasks, string siteName)
        {
            if (HttpContext.Current != null)
            {
                // Set timeout
                HttpContext.Current.Server.ScriptTimeout = ScriptTimeout;
            }

            // Run the tasks
            var lastExecutedTaskId = 0;
            var localizedMessageFormat = "({1}) " + LocalizationHelper.GetString("scheduler.executingtask");
            foreach (var task in fetchedTasks)
            {
                var taskName = task.TaskDisplayName;
                if (String.IsNullOrWhiteSpace(taskName))
                {
                    taskName = task.TaskID.ToString();
                }

                var message = String.Format(localizedMessageFormat, taskName, DateTime.Now);
                LogContext.AppendLine(message, "Scheduler");

                ExecuteTask(task, siteName);
                lastExecutedTaskId = task.TaskID;
            }

            return lastExecutedTaskId;
        }


        /// <summary>
        /// Executes specified task.
        /// </summary>
        /// <param name="taskInfo">Task to execute</param>
        private static void ExecuteTaskInternal(TaskInfo taskInfo)
        {
            // Check whether task condition is defined
            if (!AllowsTaskConditionItsExecution(taskInfo))
            {
                ReplanExecutedTask(taskInfo, taskExecutions: 0, taskStartTime: null);
                return;
            }

            // Number of task executions
            var taskExecutions = 0;

            // Indicates when task was started
            var taskStartTime = DateTime.Now;

            SchedulingHelper.LogTask(taskInfo);

            bool globalCounterIncremented = false;
            DataSet sites = null;
            try
            {
                // Ensure the task instance and run the task
                Action<TaskInfo> taskCodeDelegate = GetTaskExecutionDelegate(taskInfo);

                // Get user context
                var user = (IUserInfo)ProviderHelper.GetInfoById(PredefinedObjectType.USER, taskInfo.TaskUserID);

                using (CMSActionContext context = new CMSActionContext())
                {
                    // Set user context
                    if (user != null)
                    {
                        context.User = user;
                    }

                    if (taskInfo.TaskRunIndividuallyForEachSite)
                    {
                        sites = SiteInfoProvider.GetSites().WhereEquals("SiteStatus", SiteStatusEnum.Running.ToStringRepresentation());
                        taskExecutions = ExecuteTaskPerSite(taskInfo, sites, taskCodeDelegate);
                    }
                    else
                    {
                        // Increment counter
                        taskExecutions =  ExecuteTaskGlobally(taskInfo, taskCodeDelegate);
                        globalCounterIncremented = true;
                    }
                }
            }
            catch (Exception ex)
            {
                // Show error in last result in task and event log
                taskInfo.TaskLastResult = ResHelper.GetString("general.error", CultureHelper.EnglishCulture.Name) + ": " + ex.Message;
                EventLogProvider.LogException("SchedulingExecutor", "EXCEPTION", ex, taskInfo.TaskSiteID);
            }
            finally
            {
                ClearRunningTasksCounters(taskInfo, globalCounterIncremented, sites);
            }

            ReplanExecutedTask(taskInfo, taskExecutions, taskStartTime);
        }


        /// <summary>
        /// Returns <c>true</c> if there is no macro condition or there is a macro contion for <paramref name="taskInfo"/> and the condition resolved to <c>true</c>.
        /// </summary>
        private static bool AllowsTaskConditionItsExecution(TaskInfo taskInfo)
        {
            // Resolve task condition and if is not match -> do not execute task
            return !MacroProcessor.ContainsMacro(taskInfo.TaskCondition)
                || ValidationHelper.GetBoolean(MacroResolver.Resolve(taskInfo.TaskCondition), false);

        }


        /// <summary>
        /// Executes provided <paramref name="taskCodeDelegate"/> for every site in <paramref name="sites"/> and returns the number of executions that there were.
        /// For execution on each site, <paramref name="taskInfo"/> with properly set <see cref="TaskInfo.TaskSiteID"/> is provided to the <paramref name="taskCodeDelegate"/>.
        /// </summary>
        private static int ExecuteTaskPerSite(TaskInfo taskInfo, DataSet sites, Action<TaskInfo> taskCodeDelegate)
        {
            if (DataHelper.DataSourceIsEmpty(sites))
            {
                // No sites, no executions
                return 0;
            }

            var taskExecutions = 0;
            foreach (DataRow dr in sites.Tables[0].Rows)
            {
                SiteInfo si = new SiteInfo(dr);
                if (!SystemContext.IsWebSite)
                {
                    // Set site name
                    SiteContext.CurrentSiteName = si.SiteName;
                }

                // Set site ID for task 
                taskInfo.TaskSiteID = si.SiteID;

                // Increment counter
                RunningTasks.Increment(si.SiteName);

                // Execute task's code
                taskCodeDelegate(taskInfo);

                taskExecutions++;
            }

            return taskExecutions;
        }


        /// <summary>
        /// Executes <paramref name="taskCodeDelegate"/> for one time (returning <c>1</c> as number of executions) without amending <paramref name="taskInfo"/>'s <see cref="TaskInfo.TaskSiteID"/>.
        /// If specified, <see cref="TaskInfo.CurrentSiteName"/> is used to put the <paramref name="taskInfo"/> into proper context. 
        /// </summary>
        /// <remarks><paramref name="taskInfo"/> is passed to the <paramref name="taskCodeDelegate"/> for execution.</remarks>
        private static int ExecuteTaskGlobally(TaskInfo taskInfo, Action<TaskInfo> taskCodeDelegate)
        {
            // Run from external application
            if (!SystemContext.IsWebSite)
            {
                // Clear hash tables to ensure fresh data
                ModuleManager.ClearHashtables(false);

                // Set context of task site
                string siteName = taskInfo.CurrentSiteName;
                if (!string.IsNullOrEmpty(siteName))
                {
                    // Set site name
                    SiteContext.CurrentSiteName = siteName;
                }
            }

            RunningTasks.Increment(null);

            // Execute task
            taskCodeDelegate(taskInfo);

            return 1;
        }


        /// <summary>
        /// Decrements <see cref="RunningTasks"/> performance counters. Method is supposed to be executed once the <paramref name="taskInfo"/> in question is completely processed.
        /// </summary>
        private static void ClearRunningTasksCounters(TaskInfo taskInfo, bool globalCounterIncremented, DataSet sites)
        {
            if (globalCounterIncremented)
            {
                // Decrement site-independent counter, meaning task was not executed individually per site
                RunningTasks.Decrement(null);
                return;
            }

            if (!taskInfo.TaskRunIndividuallyForEachSite)
            {
                // Global counter was not incremented, yet task was not executed per site anyway, no more cleaning required
                return;
            }

            // Set site ID back to global
            taskInfo.TaskSiteID = 0;

            if (DataHelper.DataSourceIsEmpty(sites))
            {
                // There were no site to execute task at
                return;
            }

            // Decrement for all run sites
            foreach (DataRow dr in sites.Tables[0].Rows)
            {
                //Decrement counter
                RunningTasks.Decrement(dr["SiteName"].ToString());
            }
        }


        /// <summary>
        /// Prepares <paramref name="taskInfo"/> for next execution based on its <see cref="TaskInfo.TaskInterval"/> or deletes if there are no more executions to be planned and task is set so.
        /// Method also updates <paramref name="taskInfo"/> with number of <paramref name="taskExecutions"/> and eventually writes <paramref name="taskStartTime"/> to the <see cref="TaskInfo.TaskLastRunTime"/>.
        /// </summary>
        private static void ReplanExecutedTask(TaskInfo taskInfo, int taskExecutions, DateTime? taskStartTime)
        {
            if (taskInfo.TaskID <= 0)
            {
                // Do not save on-the-fly tasks
                return;
            }

            // Plans next task run time
            taskInfo.TaskIsRunning = false;
            taskInfo.TaskExecutingServerName = null;
            taskInfo.TaskNextRunTime = SchedulingHelper.GetNextTime(SchedulingHelper.DecodeInterval(taskInfo.TaskInterval), taskInfo.TaskNextRunTime);

            if ((taskInfo.TaskNextRunTime == TaskInfoProvider.NO_TIME) && String.IsNullOrEmpty(taskInfo.TaskLastResult) && taskInfo.TaskDeleteAfterLastRun)
            {
                // If there is no next run and the task has been executed without error and should be deleted after last run, delete it
                TaskInfoProvider.DeleteTaskInfo(taskInfo.TaskID);
                return;
            }

            // Set task's last run time when it has run
            if (taskStartTime.HasValue)
            {
                taskInfo.TaskLastRunTime = taskStartTime.Value;
            }

            // Increment number of task executions
            taskInfo.TaskExecutions += taskExecutions;

            // Save the task result and next run time
            TaskInfoProvider.SetTaskInfo(taskInfo);
        }


        /// <summary>
        /// Returns delegate that executes code of the given task info.
        /// </summary>
        /// <param name="taskInfo">Task info to execute code of</param>
        /// <remarks>
        /// If the implementation extends <see cref="ITask"/>, the <see cref="TaskInfo"/> object provided to the returned delegate is passed to the <see cref="ITask.Execute(TaskInfo)"/> method.
        /// The delegate accepts the <see cref="TaskInfo"/> only for clarity reasons; reference to the <paramref name="taskInfo"/> could be easily wired into the returned delegate anyway.
        /// </remarks>
        private static Action<TaskInfo> GetTaskExecutionDelegate(TaskInfo taskInfo)
        {
            object taskImplementation = GetTaskInstance(taskInfo);

            if (taskImplementation == null)
            {
                throw new InvalidOperationException(String.Format("[SchedulingExecutor.ExecuteTask]: Cannot load the provider class '{0}' from the assembly 'taskInfo.TaskAssemblyName'.", taskInfo.TaskClass));
            }

            var taskDelegate = GetTaskDelegate(taskImplementation);
            if (taskDelegate == null)
            {
                throw new NotSupportedException("[SchedulingExecutor.ExecuteTask]: Only classes that implement ITask or IWorkerTask can be executed as scheduled tasks.");
            }

            return executedTaskInfo => executedTaskInfo.TaskLastResult = taskDelegate(executedTaskInfo);
        }


        /// <summary>
        /// Returns the collection of names of all servers that are positively operating.
        /// </summary>
        private static IEnumerable<string> GetOperatingServerNames()
        {
            // ServerEnabled column needs to be retrieved because of Status property implementation
            return WebFarmServerInfoProvider
                .GetWebFarmServers()
                .Columns("ServerName", "ServerEnabled")
                .ToList()
                .Where(webFarmServer => webFarmServer.Status == WebFarmServerStatusEnum.Healthy ||
                                        webFarmServer.Status == WebFarmServerStatusEnum.Transitioning)
                .Select(webFarmServerInfo => webFarmServerInfo.ServerName);
        }


        /// <summary>
        /// Re-initializes given tasks by re-planning their next run time
        /// (see <see cref="SchedulingHelper.GetNextTime(CMS.Scheduler.TaskInterval,System.Nullable{System.DateTime},System.Nullable{System.DateTime})"/>).
        /// </summary>
        /// <remarks>Re-initialization takes place during materialization, not on the method call.</remarks>
        /// <param name="corruptedTasks">Query identifying tasks that need to be re-planned.</param>
        /// <returns>Enumerable collection of just-in-time re-initialized <see cref="TaskInfo"/>s.</returns>
        internal static IEnumerable<TaskInfo> ReInitializeTasks(this ObjectQuery<TaskInfo> corruptedTasks)
        {
            foreach (var corruptedTask in corruptedTasks)
            {
                // Re-plan next run time of the task
                corruptedTask.TaskNextRunTime = SchedulingHelper.GetNextTime(
                    SchedulingHelper.DecodeInterval(corruptedTask.TaskInterval),
                    corruptedTask.TaskNextRunTime,
                    corruptedTask.TaskLastRunTime);

                // Clear executing server name (new server executing the task will fill-in its name during the task execution)
                corruptedTask.TaskExecutingServerName = null;
                corruptedTask.TaskIsRunning = false;

                yield return corruptedTask;
            }
        }


        /// <summary>
        /// Re-initialize all scheduled task which are corrupted.
        /// </summary>
        public static void ReInitCorruptedTasks()
        {
            GetRelevantCorruptedTasks()
                .ReInitializeTasks()
                .ToList()
                .ForEach(TaskInfoProvider.SetTaskInfo);
        }

        #endregion
    }
}
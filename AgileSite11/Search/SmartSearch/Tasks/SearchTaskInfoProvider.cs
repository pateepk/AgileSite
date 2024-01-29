using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using System.Linq;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.Base;
using CMS.Core;
using CMS.IO;
using CMS.Search.Internal;

namespace CMS.Search
{
    /// <summary>
    /// Class providing SearchTaskInfo management.
    /// </summary>
    public class SearchTaskInfoProvider : AbstractInfoProvider<SearchTaskInfo, SearchTaskInfoProvider>
    {
        #region "Variables and constants"

        /// <summary>
        /// File name for the external lock
        /// </summary>
        public const string EXTERNAL_REBUILD_LOCK_NAME = "_external.lock";

        private const int MAX_NUMBER_OF_THREAD_RESTORING = 5;

        // Indicates if the smart search indexer should start.
        private static bool mEnableIndexer = true;

        // Indicates whether task creation is enabled.
        private static bool mEnableTasks = true;

        // Indicates whether thread is running.
        private static bool mIsIndexThreadRunning;

        // Contains GUID of indexer thread when indexer is running.
        private static Guid mIndexerThreadGuid = Guid.Empty;

        // Current number of thread restoring.
        private static int mNumberOfThreadRestoring;

        // If true, smart search tasks are processed by scheduler.
        private static bool? mProcessSearchTasksByScheduler;

        private static WindowsIdentity mWindowsIdentity;
        private static readonly object locker = new object();

        #endregion


        #region "Public static properties"

        /// <summary>
        /// Indicates if the smart search indexer should start.
        /// </summary>
        public static bool EnableIndexer
        {
            get
            {
                return mEnableIndexer && CMSActionContext.CurrentEnableSmartSearchIndexer;
            }
            set
            {
                mEnableIndexer = value;
            }
        }


        /// <summary>
        /// Indicates whether task creation is enabled.
        /// </summary>
        public static bool EnableTasks
        {
            get
            {
                // Consider action context
                return mEnableTasks && CMSActionContext.CurrentCreateSearchTask;
            }
            set
            {
                mEnableTasks = value;
            }
        }


        /// <summary>
        /// If true, smart search tasks which supports various task processing are processed by scheduler.
        /// </summary>
        /// <remarks>Applies only on local search tasks.</remarks>
        public static bool ProcessSearchTasksByScheduler
        {
            get
            {
                if (mProcessSearchTasksByScheduler == null)
                {
                    mProcessSearchTasksByScheduler = ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSProcessSearchTasksByScheduler"], false);
                }
                return mProcessSearchTasksByScheduler.Value;
            }
            set
            {
                mProcessSearchTasksByScheduler = value;
            }
        }
        

        /// <summary>
        /// Contains GUID of indexer thread when indexer is running.
        /// </summary>
        public static Guid IndexerThreadGuid
        {
            get
            {
                return mIndexerThreadGuid;
            }
        }


        /// <summary>
        /// Indicates whether indexer thread is running.
        /// </summary>
        public static bool IsIndexThreadRunning
        {
            get
            {
                return mIsIndexThreadRunning;
            }
        }

        #endregion


        #region "Public static methods"

        /// <summary>
        /// Returns all search tasks.
        /// </summary>
        public static ObjectQuery<SearchTaskInfo> GetSearchTasks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the SearchTaskInfo structure for the specified searchTask.
        /// </summary>
        /// <param name="searchTaskId">SearchTask id</param>
        public static SearchTaskInfo GetSearchTaskInfo(int searchTaskId)
        {
            return ProviderObject.GetInfoById(searchTaskId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified searchTask.
        /// </summary>
        /// <param name="searchTaskObj">SearchTask to set</param>
        public static void SetSearchTaskInfo(SearchTaskInfo searchTaskObj)
        {
            ProviderObject.SetSearchTaskInfoInternal(searchTaskObj);
        }


        /// <summary>
        /// Deletes specified searchTask.
        /// </summary>
        /// <param name="searchTaskObj">SearchTask object</param>
        public static void DeleteSearchTaskInfo(SearchTaskInfo searchTaskObj)
        {
            ProviderObject.DeleteInfo(searchTaskObj);
        }


        /// <summary>
        /// Deletes specified searchTask.
        /// </summary>
        /// <param name="searchTaskId">SearchTask id</param>
        public static void DeleteSearchTaskInfo(int searchTaskId)
        {
            SearchTaskInfo searchTask = GetSearchTaskInfo(searchTaskId);
            DeleteSearchTaskInfo(searchTask);
        }


        /// <summary>
        /// Creates task in the database and start indexer if it is required.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="objectType">Object type. Not required for rebuild or optimize tasks.</param>
        /// <param name="objectField">Object field</param>
        /// <param name="objectValue">Object value</param>
        /// <param name="objectID">ID of an object this task relates to, 0 if there is none</param>
        /// <param name="runIndexer">Whether indexer starts after creation. If not specified this behavior depends on current settings.</param>
        public static void CreateTask(SearchTaskTypeEnum taskType, string objectType, string objectField, string objectValue, int objectID, bool? runIndexer = null)
        {
            var taskCreationParameters = new SearchTaskCreationParameters
            {
                TaskType = taskType,
                ObjectType = objectType,
                ObjectField = objectField,
                TaskValue = objectValue,
                RelatedObjectID = objectID
            };

            CreateTask(taskCreationParameters, runIndexer);
        }


        /// <summary>
        /// Creates task in the database and start indexer if it is required.
        /// </summary>
        /// <param name="parameters">Search task creation parameters</param>
        /// <param name="runIndexer">Whether indexer starts after creation. If not specified this behavior depends on current settings.</param>
        public static void CreateTask(SearchTaskCreationParameters parameters, bool? runIndexer = null)
        {
            CreateTasks(new[] { parameters }, runIndexer);
        }


        /// <summary>
        /// Creates tasks in the database and start indexer if it is required.
        /// </summary>
        /// <param name="parameters">Search task creation parameters</param>
        /// <param name="runIndexer">Whether indexer starts after creation. If not specified this behavior depends on current settings.</param>
        public static void CreateTasks(ICollection<SearchTaskCreationParameters> parameters, bool? runIndexer = null)
        {
            bool runIndexerInternal = runIndexer.HasValue ? runIndexer.Value : CMSActionContext.CurrentEnableSmartSearchIndexer;

            // If creation of tasks is enabled
            if (!EnableTasks)
            {
                return;
            }

            SearchEvents.SearchTaskCreationHandler.StartEvent(parameters);

            parameters = parameters.Where(p => IsLuceneRelatedTask(p)).ToList();
            if (!parameters.Any())
            {
                return;
            }

            // Create tasks
            ProviderObject.CreateTaskInternal(parameters);

            // Create webfarm tasks
            if (EnableIndexer && runIndexerInternal && (SystemContext.IsRunningOnAzure || !SearchHelper.IndexesInSharedStorage))
            {
                // Web farm task is logged in the cloud services too, but task is processed only by its worker role. 
                // This is not ideal scenario, unfortunately we are unable to target web farm task right now.
                WebFarmHelper.CreateTask(SearchTaskType.RunSmartSearchIndexer, "RunSmartSearchIndexer");
            }

            // Start indexing
            if (runIndexerInternal)
            {
                if (CMSTransactionScope.IsInTransaction)
                {
                    ConnectionContext.CurrentConnectionScope.CallOnDispose(ProcessTasks);
                }
                else
                {
                    ProcessTasks();
                }
            }
        }


        /// <summary>
        /// Filters Rebuild and Optimize tasks based on their index provider.
        /// </summary>
        private static bool IsLuceneRelatedTask(SearchTaskCreationParameters parameters)
        {
            if (parameters.TaskType == SearchTaskTypeEnum.Rebuild || parameters.TaskType == SearchTaskTypeEnum.Optimize)
            {
                var index = SearchIndexInfoProvider.GetSearchIndexInfo(parameters.RelatedObjectID);
                if (index != null && index.IsLuceneIndex())
                {
                    return true;
                }
                
                return false;
            }

            return true;
        }


        /// <summary>
        /// Processes tasks (starts indexer).
        /// </summary>
        public static void ProcessTasks()
        {
            ProcessTasks(false);
        }


        /// <summary>
        /// Processes tasks (starts indexer).
        /// </summary>
        /// <param name="createWebFarmTask">Indicates whether web farm task should be created</param>
        public static void ProcessTasks(bool createWebFarmTask)
        {
            ProcessTasks(createWebFarmTask, false);
        }


        /// <summary>
        /// Processes tasks (starts indexer).
        /// </summary>
        /// <param name="createWebFarmTask">Indicates whether web farm task should be created</param>
        /// <param name="workerRole">Indicates whether process tasks are run by Azure worker role</param>
        public static void ProcessTasks(bool createWebFarmTask, bool workerRole)
        {
            // Do not process tasks if they should be processed by scheduled task.
            if (ProcessSearchTasksByScheduler)
            {
                return;
            }

            // Only worker role should process tasks on Azure
            if (SystemContext.IsRunningOnAzure && !workerRole)
            {
                return;
            }

            RunAsync(WindowsIdentity.GetCurrent());

            // Create web farm task if needed
            if (createWebFarmTask && EnableIndexer && SearchIndexInfoProvider.SearchEnabled && !SearchHelper.IndexesInSharedStorage)
            {
                WebFarmHelper.CreateTask(SearchTaskType.RunSmartSearchIndexer, "RunSmartSearchIndexer");
            }
        }


        /// <summary>
        /// Starts thread of indexer.
        /// </summary>
        /// <param name="wi">Windows identity</param>
        public static void RunAsync(WindowsIdentity wi)
        {
            // Check whether indexation is enabled
            if (!EnableIndexer || !SearchIndexInfoProvider.SearchEnabled)
            {
                return;
            }

            // We want only one indexer to run.
            if (!mIsIndexThreadRunning)
            {
                lock (locker)
                {
                    if (!mIsIndexThreadRunning)
                    {
                        mWindowsIdentity = wi;

                        // The flag is set here to prevent concurrency because a started thread which fails fast could set it back to false too soon, and result would be incorrectly true
                        mIsIndexThreadRunning = true;

                        try
                        {
                            StartAsyncThread();
                        }
                        catch
                        {
                            // Reset the flag in case the thread was not able to start for some reason
                            mIsIndexThreadRunning = false;
                            throw;
                        }
                    }
                }
            }
        }


        private static void StartAsyncThread()
        {
            // Force the indexer to run in separate async thread. By default new CMSThreads will run synchronously when they are created in async process.
            // For instance starting the indexer in the end of import process. Import is already running in async thread and starting the indexer will block the original process. 
            using (var context = new CMSActionContext())
            {
                context.AllowAsyncActions = true;

                var indexThread = new CMSThread(RunInternal, true);

                mIndexerThreadGuid = indexThread.ThreadGUID;
                indexThread.Start();
            }
        }


        /// <summary>
        /// Returns object type of the task related object. 
        /// </summary>
        /// <param name="taskObjectType">Search task object type</param>
        /// <param name="taskStatus">Search task status</param>
        /// <returns>Search task related object's object type.</returns>
        public static string GetSearchTaskRelatedObjectType(string taskObjectType, SearchTaskTypeEnum taskStatus)
        {
            if ((taskStatus == SearchTaskTypeEnum.Rebuild) || (taskStatus == SearchTaskTypeEnum.Optimize))
            {
                return SearchIndexInfo.OBJECT_TYPE;
            }

            if (taskObjectType == PredefinedObjectType.FORUM)
            {
                return PredefinedObjectType.FORUMPOST;
            }

            return taskObjectType;
        }

        #endregion


        #region "Indexer"

        /// <summary>
        /// Returns batch of tasks that are ready to be processed.
        /// </summary>
        internal ObjectQuery<SearchTaskInfo> GetSearchTaskToProcess()
        {
            // Get all search tasks for current server name that are ready (Error status task is still ready. This status only indicates that error occured in previous run.) 
            var readyStatuses = new[]
            {
                SearchTaskStatusEnum.Ready.ToStringRepresentation(),
                SearchTaskStatusEnum.Error.ToStringRepresentation()
            };

            var tasks = GetSearchTasks()
                .WhereIn("SearchTaskStatus", readyStatuses);

            // If web farms are enabled and instance is not using shared storage, consider server name
            if (WebFarmHelper.WebFarmEnabled && !SearchHelper.IndexesInSharedStorage)
            {
                tasks.Where("SearchTaskServerName", QueryOperator.Equals, WebFarmHelper.ServerName);
            }

            // Load top X tasks from database ordered by priority
            return tasks
                .OrderByDescending("SearchTaskPriority")
                .OrderByAscending("SearchTaskID")
                .TopN(SearchManager.TaskProcessingBatchSize);
        }


        /// <summary>
        /// Retrieves search tasks from DB and processes them.
        /// </summary>
        private static void ProcessSearchTasks()
        {
            var tasks = ProviderObject.GetSearchTaskToProcess();

            var fileLockPath = Path.Combine(SystemContext.WebApplicationPhysicalPath, SearchHelper.SearchPath);
            var fileLock = new FileLock(fileLockPath, "searchtaskindexer.lock");

            // If using shared storage, handle file lock
            if (SearchHelper.IndexesInSharedStorage && tasks.Any() && !fileLock.ObtainForInstance(WebFarmHelper.ServerName))
            {
                return;
            }

            try
            {
                // If dataset empty nothing to do
                while (tasks.Any())
                {
                    // Go trough all search tasks
                    foreach (var task in tasks)
                    {
                        try
                        {
                            SearchIndexers.GetIndexer(task.SearchTaskObjectType).ExecuteTask(task);
                        }
                        catch (Exception ex)
                        {
                            task.SearchTaskStatus = SearchTaskStatusEnum.Error;
                            task.SearchTaskErrorMessage = ex.Message;

                            SetSearchTaskInfo(task);

                            throw;
                        }

                        DeleteSearchTaskInfo(task);
                    }

                    tasks.Reset();
                }
            }
            finally
            {
                // If using shared storage, handle file lock
                if (SearchHelper.IndexesInSharedStorage)
                {
                    fileLock.Release();
                }
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Provides delete query.
        /// </summary>
        internal static ObjectQuery<SearchTaskInfo> DeleteQuery()
        {
            return ProviderObject.GetDeleteQuery();
        }


        /// <summary>
        /// Sets (updates or inserts) specified search task.
        /// </summary>
        /// <param name="searchTask">Search task to set</param>
        protected virtual void SetSearchTaskInfoInternal(SearchTaskInfo searchTask)
        {
            if (searchTask != null)
            {
                // Set task priority automatically, rebuild has higher priority
                searchTask.SearchTaskPriority = (searchTask.SearchTaskType == SearchTaskTypeEnum.Rebuild) ? 1 : 0;

                // Set creation time
                if (searchTask.SearchTaskCreated == DateTimeHelper.ZERO_TIME)
                {
                    searchTask.SetValue("SearchTaskCreated", DateTime.Now);
                }

                // Save the object
                if (searchTask.SearchTaskID > 0)
                {
                    searchTask.Generalized.UpdateData();
                }
                else
                {
                    searchTask.Generalized.InsertData();
                }
            }
            else
            {
                throw new Exception("[SearchTaskProvider.SetSearchTask]: No SearchTaskInfo object set.");
            }
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(SearchTaskInfo info)
        {
            if (info != null)
            {
                // Delete the object
                base.DeleteInfo(info);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Creates search tasks in the database.
        /// </summary>
        /// <param name="parameters">Search task creation parameters</param>     
        private void CreateTaskInternal(ICollection<SearchTaskCreationParameters> parameters)
        {
            using (var transactionScope = new CMSLateBoundTransaction(GetType()))
            {
                List<string> enabledWebFarmServers = null;

                // Create web farm tasks when running in web farm environment and not using shared storage
                if (WebFarmHelper.WebFarmEnabled && !SearchHelper.IndexesInSharedStorage)
                {
                    enabledWebFarmServers = CoreServices.WebFarm.GetEnabledServerNames().ToList();
                }

                // When multiple task are being created, they should by inserted in one transaction
                if (parameters.Count > 1)
                {
                    transactionScope.BeginTransaction();
                }

                // Loop through all parameters. Create task for each parameter.
                foreach (var parameter in parameters)
                {
                    if (!string.IsNullOrEmpty(parameter.TaskValue))
                    {
                        // When web farms are enabled search task must be created for each web farm server
                        if (enabledWebFarmServers != null)
                        {
                            // Create search task for all web farm servers
                            foreach (string serverName in enabledWebFarmServers)
                            {
                                var sti = CreateSearchTaskInfo(parameter, SearchTaskStatusEnum.Ready, serverName);
                                SetSearchTaskInfo(sti);
                            }
                        }
                        else
                        {
                            // Create new search task
                            var sti = CreateSearchTaskInfo(parameter, SearchTaskStatusEnum.Ready, null);
                            SetSearchTaskInfo(sti);
                        }
                    }
                }

                // Commit the transaction when transaction scope exists
                transactionScope.Commit();
            }
        }


        /// <summary>
        /// Creates new search task info object and fills it with given values.
        /// </summary>
        /// <param name="parameters">Creation parameters </param>
        /// <param name="status">Task status</param>
        /// <param name="serverName">Web farm server name</param>
        /// <returns>New search task info object</returns>
        private static SearchTaskInfo CreateSearchTaskInfo(SearchTaskCreationParameters parameters, SearchTaskStatusEnum status, string serverName)
        {
            SearchTaskInfo sti = new SearchTaskInfo();
            sti.SearchTaskType = parameters.TaskType;
            sti.SearchTaskObjectType = parameters.ObjectType;
            sti.SearchTaskField = parameters.ObjectField;
            sti.SearchTaskValue = parameters.TaskValue;
            sti.SearchTaskRelatedObjectID = parameters.RelatedObjectID;
            sti.SearchTaskStatus = status;

            if (!string.IsNullOrEmpty(serverName))
            {
                sti.SearchTaskServerName = serverName;
            }

            return sti;
        }


        /// <summary>
        /// Represents indexer thread.
        /// </summary>
        private static void RunInternal()
        {
            // Check whether indexation is enabled
            if ((!EnableIndexer) || (!SearchIndexInfoProvider.SearchEnabled))
            {
                return;
            }

            // Disable file debug for this thread
            FileDebug.DebugCurrentRequest = false;

            bool threadFailed = false;

            // Impersonation context
            WindowsImpersonationContext ctx = null;

            try
            {
                // Impersonate current thread
                if (!ProcessSearchTasksByScheduler)
                {
                    ctx = mWindowsIdentity.Impersonate();
                }

                // Indexer process
                ProcessSearchTasks();
            }
            // Thread execution was canceled.
            catch (ThreadAbortException ex)
            {
                EventLogProvider.LogEvent(EventType.INFORMATION, "Smart search", "PROCESSSEARCHTASK", ex.Message);
            }
            catch (Exception ex)
            {
                // Indicate that thread failed
                threadFailed = true;
                EventLogProvider.LogException("Smart search", "PROCESSSEARCHTASK", ex);
            }
            finally
            {
                FinalizeThread(threadFailed, ctx);
            }
        }


        private static void FinalizeThread(bool threadFailed, WindowsImpersonationContext ctx)
        {
            var threadContinues = false;

            try
            {
                if (!ProcessSearchTasksByScheduler)
                {
                    // Undo impersonation
                    if (ctx != null)
                    {
                        ctx.Undo();
                    }

                    if (threadFailed)
                    {
                        threadContinues = RetryRunInternal();
                    }
                    else
                    {
                        // Thread ends successfully - set counter of thread restoring back to 0.
                        mNumberOfThreadRestoring = 0;
                    }
                }
            }
            finally
            {
                if (!threadContinues)
                {
                    // Thread is not running anymore, reset flags so another thread may start next time
                    mIsIndexThreadRunning = false;
                    mIndexerThreadGuid = Guid.Empty;
                }
            }
        }


        private static bool RetryRunInternal()
        {
            // If thread failed start it again - specified max times
            if (mNumberOfThreadRestoring < MAX_NUMBER_OF_THREAD_RESTORING)
            {
                mNumberOfThreadRestoring++;
                RunInternal();

                return true;
            }

            return false;
        }

        #endregion
    }
}
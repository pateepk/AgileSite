using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Synchronization
{
    using ConnectorList = List<AbstractIntegrationConnector>;

    /// <summary>
    /// Class covering functionality shared across the integration module.
    /// </summary>
    public static class IntegrationHelper
    {
        #region "Variables"

        private static bool? mIntegrationIsDisabled;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates if system integration is enabled.
        /// </summary>
        public static bool IntegrationEnabled
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSIntegrationEnabled") && !IntegrationIsDisabled;
            }
        }


        /// <summary>
        /// Indicates if system integration is disabled in web.config.
        /// </summary>
        private static bool IntegrationIsDisabled
        {
            get
            {
                if (mIntegrationIsDisabled == null)
                {
                    mIntegrationIsDisabled = !ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSIntegrationEnabled"], true);
                }

                return mIntegrationIsDisabled.Value;
            }
        }


        /// <summary>
        /// Indicates if processing of integration tasks for inbound direction is enabled.
        /// Reflects also IntegrationEnabled setting.
        /// </summary>
        public static bool IntegrationProcessExternal
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSIntegrationProcessExternal") && IntegrationEnabled;
            }
        }


        /// <summary>
        /// Indicates if processing of integration tasks for outbound direction is enabled.
        /// Reflects also IntegrationEnabled setting.
        /// </summary>
        public static bool IntegrationProcessInternal
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSIntegrationProcessInternal") && IntegrationEnabled;
            }
        }


        /// <summary>
        /// Indicates if logging of integration tasks for inbound direction is enabled.
        /// Reflects also IntegrationEnabled setting.
        /// </summary>
        public static bool IntegrationLogExternal
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSIntegrationLogExternal") && IntegrationEnabled;
            }
        }


        /// <summary>
        /// Indicates if logging of integration tasks for outbound direction is enabled.
        /// Reflects also IntegrationEnabled setting.
        /// </summary>
        public static bool IntegrationLogInternal
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSIntegrationLogInternal") && IntegrationEnabled;
            }
        }

        #endregion


        #region "Connectors"


        #region "Variables"

        /// <summary>
        /// Lock for connector loading.
        /// </summary>
        private static readonly object connectorLock = new object();

        /// <summary>
        /// Item name for storing connector names in request stock.
        /// </summary>
        internal const string TOUCHED_CONNECTOR_NAMES = "TouchedConnectorNames";

        /// <summary>
        /// Inner connector storage
        /// </summary>
        private static readonly CMSStatic<ConnectorList> mConnectors = new CMSStatic<ConnectorList>();

        #endregion


        #region "Properties"

        /// <summary>
        /// List of enabled (and initialized) connectors
        /// </summary>
        public static List<AbstractIntegrationConnector> Connectors
        {
            get
            {
                LoadConnectors();

                return mConnectors;
            }
        }


        /// <summary>
        /// List of connector names for which the task was logged
        /// </summary>
        public static List<string> TouchedConnectorNames
        {
            get
            {
                List<string> touchedConnectorNames = (List<string>)RequestStockHelper.GetItem(TOUCHED_CONNECTOR_NAMES, true);
                if (touchedConnectorNames == null)
                {
                    touchedConnectorNames = new List<string>();
                    RequestStockHelper.Add(TOUCHED_CONNECTOR_NAMES, touchedConnectorNames, true);
                }
                return touchedConnectorNames;
            }
            set
            {
                RequestStockHelper.Add(TOUCHED_CONNECTOR_NAMES, value, true);
            }
        }

        #endregion


        #region "Methods

        /// <summary>
        /// Initializes integration bus
        /// </summary>
        public static void Init()
        {
            if (IntegrationProcessExternal || IntegrationProcessInternal || IntegrationLogExternal || IntegrationLogInternal)
            {
                // Load connectors
                LoadConnectors();
            }
        }


        /// <summary>
        /// Loads connectors
        /// </summary>
        public static void LoadConnectors()
        {
            LoadConnectors(false);
        }


        /// <summary>
        /// Loads connectors
        /// </summary>
        /// <param name="force">Whether to load from database</param>
        public static void LoadConnectors(bool force)
        {
            if ((mConnectors.Value == null) || force)
            {
                lock (connectorLock)
                {
                    if ((mConnectors.Value == null) || force)
                    {
                        var tempConnectors = new List<AbstractIntegrationConnector>();

                        // Get enabled connectors
                        DataSet connectors = IntegrationConnectorInfoProvider.GetIntegrationConnectors().WhereEquals("ConnectorEnabled", 1).Columns("ConnectorDisplayName, ConnectorAssemblyName, ConnectorClassName");

                        if (!DataHelper.DataSourceIsEmpty(connectors))
                        {
                            foreach (DataRow connector in connectors.Tables[0].Rows)
                            {
                                string assemblyName = ValidationHelper.GetString(connector["ConnectorAssemblyName"], String.Empty);
                                string className = ValidationHelper.GetString(connector["ConnectorClassName"], String.Empty);

                                if (!string.IsNullOrEmpty(assemblyName) && !string.IsNullOrEmpty(className))
                                {
                                    try
                                    {
                                        // Load instance of connector
                                        AbstractIntegrationConnector connectorInstance = ClassHelper.GetClass<AbstractIntegrationConnector>(assemblyName, className);

                                        if (connectorInstance != null)
                                        {
                                            // Initialize connector
                                            connectorInstance.Init();

                                            // Add connector to collection
                                            tempConnectors.Add(connectorInstance);
                                        }
                                    }
                                    catch
                                    {
                                        string displayName = ValidationHelper.GetString(connector["ConnectorDisplayName"], String.Empty);
                                
                                        // Log the error
                                        var message = String.Format(ResHelper.GetAPIString("integration.loadconnectorsfailed", "Failed to load connector '{2}' with assembly name '{0}' and class name '{1}'. Please check connector settings."), assemblyName, className, displayName);

                                        EventLogProvider.LogEvent(EventType.WARNING, "Integration", "LOADCONNECTORS", message);
                                    }
                                }
                            }
                        }

                        // Synchronize memory access
                        Thread.MemoryBarrier();

                        mConnectors.Value = tempConnectors;
                    }
                }
            }
        }


        /// <summary>
        /// Gets connector by name
        /// </summary>
        /// <param name="connectorName">Name of connector to get</param>
        /// <returns>Connector of specified name</returns>
        public static AbstractIntegrationConnector GetConnector(string connectorName)
        {
            return Connectors.FirstOrDefault(c => c.ConnectorName == connectorName);
        }


        /// <summary>
        /// Gets connector by ID
        /// </summary>
        /// <param name="connectorId">ID of connector to get</param>
        /// <returns>Connector of specified ID</returns>
        public static AbstractIntegrationConnector GetConnector(int connectorId)
        {
            return Connectors.FirstOrDefault(c => c.ConnectorID == connectorId);
        }


        /// <summary>
        /// Invalidates collections with connectors and subscriptions
        /// </summary>
        public static void InvalidateConnectors()
        {
            InvalidateSubscriptions();
            mConnectors.Value = null;
        }

        #endregion


        #endregion


        #region "Subscriptions"


        #region "Variables"

        /// <summary>
        /// Inner subscription storage
        /// </summary>
        private static List<AbstractIntegrationSubscription> mSubscriptions;

        #endregion


        #region "Properties"

        /// <summary>
        /// List of subscriptions for all connectors
        /// </summary>
        private static List<AbstractIntegrationSubscription> Subscriptions
        {
            get
            {
                return mSubscriptions ?? (mSubscriptions = new List<AbstractIntegrationSubscription>());
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Adds subscription to collection of subscriptions
        /// </summary>
        /// <param name="subscription">Subscription to add</param>
        public static void SubscribeTo(AbstractIntegrationSubscription subscription)
        {
            if (subscription != null)
            {
                // Add subscription to list
                if (!Subscriptions.Contains(subscription))
                {
                    Subscriptions.Add(subscription);
                }
            }
        }


        /// <summary>
        /// Filters given set of connectors.
        /// </summary>
        /// <param name="connectors">Connector collection</param>
        /// <returns>Async connectors</returns>
        public static IEnumerable<string> FilterAsyncConnectors(Dictionary<string, TaskProcessTypeEnum> connectors)
        {
            IEnumerable<string> asyncSimpleConnectors = FilterConnectors(connectors, TaskProcessTypeEnum.AsyncSimple);
            IEnumerable<string> asyncSimpleSnapshotConnectors = FilterConnectors(connectors, TaskProcessTypeEnum.AsyncSimpleSnapshot);
            IEnumerable<string> asyncSnapshotConnectors = FilterConnectors(connectors, TaskProcessTypeEnum.AsyncSnapshot);

            return asyncSimpleConnectors.Union(asyncSnapshotConnectors).Union(asyncSimpleSnapshotConnectors);
        }


        /// <summary>
        /// Filters given set of connectors by task process type.
        /// </summary>
        /// <param name="connectors">Connector collection</param>
        /// <param name="taskProcessType">Task process type</param>
        /// <returns>Dictionary filtered by task process type</returns>
        public static IEnumerable<string> FilterConnectors(Dictionary<string, TaskProcessTypeEnum> connectors, TaskProcessTypeEnum taskProcessType)
        {
            return connectors.Where(c => c.Value == taskProcessType).Select(c => c.Key);
        }


        /// <summary>
        /// Gets dictionary of connector names whose subscriptions match given CMS object and task type.
        /// The type of task processing is stored as dictionary value.
        /// </summary>
        /// <param name="cmsObject">CMS object to match</param>
        /// <param name="taskType">Task type to match</param>
        /// <returns>Dictionary of connector names whose subscriptions match given document/object and task type</returns>
        public static Dictionary<string, TaskProcessTypeEnum> GetMatchingConnectors(ICMSObject cmsObject, TaskTypeEnum taskType)
        {
            // Ensure that connectors are initialized
            LoadConnectors();

            Dictionary<string, TaskProcessTypeEnum> connectors = new Dictionary<string, TaskProcessTypeEnum>();

            foreach (AbstractIntegrationSubscription subscription in Subscriptions)
            {
                string connectorName = subscription.ConnectorName;
                TaskProcessTypeEnum taskProcessType = default(TaskProcessTypeEnum);
                bool isMatch = subscription.IsMatch(cmsObject, taskType, ref taskProcessType);

                if (isMatch)
                {
                    if (taskProcessType == TaskProcessTypeEnum.SyncSnapshot)
                    {
                        // Add the highest priority type and skip the remaining subscriptions
                        connectors[connectorName] = taskProcessType;
                        break;
                    }
                    else
                    {
                        if (connectors.ContainsKey(connectorName))
                        {
                            TaskProcessTypeEnum existingProcessType = connectors[connectorName];

                            // If existing type has lower priority
                            if ((int)existingProcessType > (int)taskProcessType)
                            {
                                // Add the type with higher priority
                                connectors[connectorName] = taskProcessType;
                            }
                        }
                        else
                        {
                            connectors[connectorName] = taskProcessType;
                        }
                    }
                }
            }

            return connectors;
        }


        /// <summary>
        /// Invalidates collection with subscriptions
        /// </summary>
        public static void InvalidateSubscriptions()
        {
            mSubscriptions = null;
        }

        #endregion


        #endregion


        #region "Synchronization log"

        /// <summary>
        /// Logs the integration synchronization error.
        /// </summary>
        /// <param name="synchronization">Synchronization info object</param>
        /// <param name="result">Result</param>
        public static void LogSynchronizationError(IntegrationSynchronizationInfo synchronization, string result)
        {
            if (synchronization == null)
            {
                throw new Exception("[IntegrationHelper.LogSynchrnoizationError]: Synchronization is not set.");
            }

            // Get the task
            try
            {
                if (!string.IsNullOrEmpty(result))
                {
                    // Log the error
                    IntegrationSyncLogInfo logInfo = new IntegrationSyncLogInfo();
                    logInfo.SyncLogSynchronizationID = synchronization.SynchronizationID;
                    logInfo.SyncLogErrorMessage = result;
                    logInfo.SyncLogTime = DateTime.Now;
                    IntegrationSyncLogInfoProvider.SetIntegrationSyncLogInfo(logInfo);

                    // Update synchronization record
                    synchronization.SynchronizationLastRun = DateTime.Now;
                    synchronization.SynchronizationErrorMessage = result;
                    IntegrationSynchronizationInfoProvider.SetIntegrationSynchronizationInfo(synchronization);
                }
            }
            catch
            {
                // Error wasn't logged most probably because the task has been already deleted
            }
        }

        #endregion


        #region "Processing methods"

        /// <summary>
        /// Processes incoming task.
        /// </summary>
        /// <param name="connectorName">Name of connector</param>
        /// <param name="obj">Object or document to process</param>
        /// <param name="result">What to do if the processing fails</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of input data</param>
        /// <param name="siteName">Name of the target site</param>
        public static void ProcessExternalTask(string connectorName, object obj, IntegrationProcessTypeEnum result, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName)
        {
            AbstractIntegrationConnector connector = GetConnector(connectorName);
            if (connector != null)
            {
                connector.ProcessExternalTask(obj, result, taskType, dataType, siteName);
            }
        }


        /// <summary>
        /// Ensures separate threads for connectors to process the logged internal tasks.
        /// Thread won't be started if processing is already running.
        /// </summary>
        /// <param name="connectorNames">Names of connectors whose tasks to process</param>
        public static void ProcessInternalTasksAsync(List<string> connectorNames)
        {
            foreach (string connectorName in connectorNames)
            {
                ProcessTasks(connectorName, true);
            }
        }


        /// <summary>
        /// Ensures separate threads for connectors to process the pending logged external tasks.
        /// Thread won't be started if processing is already running.
        /// </summary>
        public static void ProcessExternalTasksAsync()
        {
            // Get connector IDs for pending tasks
            List<int> connectorIds = IntegrationSynchronizationInfoProvider.GetConnectorIdsForExternalTasks();
            if (connectorIds != null)
            {
                foreach (int connectorId in connectorIds)
                {
                    // Process tasks
                    ProcessExternalTasksAsync(connectorId);
                }
            }
        }


        /// <summary>
        /// Processes external tasks of connector specified by name
        /// </summary>
        /// <param name="connectorName">Name of connector whose tasks to process</param>
        public static void ProcessExternalTasksAsync(string connectorName)
        {
            ProcessTasks(connectorName, false);
        }


        /// <summary>
        /// Processes external tasks of connector specified by ID
        /// </summary>
        /// <param name="connectorId">ID of connector whose tasks to process</param>
        public static void ProcessExternalTasksAsync(int connectorId)
        {
            ProcessTasks(connectorId, false);
        }


        /// <summary>
        /// Processes pending internal or external tasks for specified connector.
        /// </summary>
        /// <param name="connectorName">Name of connector whose tasks to process</param>
        /// <param name="internalTasks">Whether to process internal or external tasks</param>
        public static void ProcessTasks(string connectorName, bool internalTasks)
        {
            // Get connector
            AbstractIntegrationConnector connector = GetConnector(connectorName);
            ProcessTasks(connector, internalTasks);
        }


        /// <summary>
        /// Processes pending internal or external tasks for specified connector.
        /// </summary>
        /// <param name="connectorId">ID of connector whose tasks to process</param>
        /// <param name="internalTasks">Whether to process internal or external tasks</param>
        public static void ProcessTasks(int connectorId, bool internalTasks)
        {
            // Get connector
            AbstractIntegrationConnector connector = GetConnector(connectorId);
            ProcessTasks(connector, internalTasks);
        }


        /// <summary>
        /// Processes pending internal or external tasks for specified connector.
        /// </summary>
        /// <param name="connector">Connector whose tasks to process</param>
        /// <param name="internalTasks">Whether to process internal or external tasks</param>
        private static void ProcessTasks(AbstractIntegrationConnector connector, bool internalTasks)
        {
            if ((connector == null) || ((connector.ProcessingExternalTasks || internalTasks || !IntegrationProcessExternal) && (connector.ProcessingInternalTasks || !internalTasks || !IntegrationProcessInternal)))
            {
                return;
            }

            var tasksWorker = new IntegrationTasksWorker
            {
                Connector = connector,
                InternalTasks = internalTasks,
            };

            // Process connector's tasks in separate thread
            tasksWorker.RunAsync();
        }

        #endregion


        #region "Integration bus methods"

        /// <summary>
        /// Processes synchronous task subscriptions.
        /// </summary>
        /// <param name="infoObj">Info object to match</param>
        /// <param name="siteId">Site identifier</param>
        /// <param name="taskType">Type of task to match</param>
        /// <param name="connectors">Connectors to process</param>
        public static void ProcessSyncTasks(GeneralizedInfo infoObj, TaskTypeEnum taskType, int siteId, Dictionary<string, TaskProcessTypeEnum> connectors)
        {
            // Get connectors for synchronous processing
            IEnumerable<string> syncSnapshotConnectors = FilterConnectors(connectors, TaskProcessTypeEnum.SyncSnapshot);

            // Synchronously process the task
            foreach (string connectorName in syncSnapshotConnectors)
            {
                AbstractIntegrationConnector connector = GetConnector(connectorName);
                string errorMessage = null;
                IntegrationProcessResultEnum result;

                try
                {
                    string siteName = ProviderHelper.GetCodeName(PredefinedObjectType.SITE, siteId);

                    // Process the task
                    result = connector.ProcessInternalTaskSync(infoObj, taskType, siteName, out errorMessage);
                }
                catch
                {
                    result = IntegrationProcessResultEnum.Error;
                }

                if ((result == IntegrationProcessResultEnum.Error) || (result == IntegrationProcessResultEnum.ErrorAndSkip))
                {
                    // Get some identifier
                    string objectIdentifier;

                    var ti = infoObj.TypeInfo;
                    if (ti.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        objectIdentifier = infoObj.ObjectCodeName;
                    }
                    else if (ti.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        objectIdentifier = infoObj.ObjectGUID.ToString();
                    }
                    else
                    {
                        objectIdentifier = infoObj.ObjectID.ToString();
                    }
                    // Prepare error message
                    errorMessage = "[IntegrationHelper.ProcessSyncTasks]: Connector '" + connector.ConnectorName +
                                   "' failed to synchronously process task '" + TaskHelper.GetTaskTypeString(taskType) + "' for object of type '" +
                                   ti.ObjectType + "' identified as '" + objectIdentifier + "'. Original message: " + errorMessage;

                    // Log error
                    EventLogProvider.LogEvent(EventType.ERROR, "Integration", "PROCESSINTERNALTASKSYNC", errorMessage);
                    switch (result)
                    {
                        // End processing
                        case IntegrationProcessResultEnum.Error:
                            return;

                        // Continue processing task for other connectors
                        case IntegrationProcessResultEnum.ErrorAndSkip:
                            break;
                    }
                }
            }
        }


        /// <summary>
        /// Logs integration task (only asynchronous process types).
        /// </summary>
        /// <param name="infoObj">Info object to log</param>
        /// <param name="taskType">Type of task to log</param>
        /// <param name="siteId">Site identifier</param>
        /// <param name="processTypes">Task process types</param>
        public static List<IntegrationTaskInfo> LogIntegration(GeneralizedInfo infoObj, TaskTypeEnum taskType, int siteId, params TaskProcessTypeEnum[] processTypes)
        {
            var tasks = new List<IntegrationTaskInfo>();
            if (IntegrationLogInternal)
            {
                // Ensure default types
                if (processTypes == null)
                {
                    processTypes = new[] { TaskProcessTypeEnum.AsyncSimple, TaskProcessTypeEnum.AsyncSimpleSnapshot, TaskProcessTypeEnum.AsyncSnapshot };
                }

                // Get matching connectors
                var connectors = GetMatchingConnectors(infoObj, taskType);
                TouchAsyncConnectors(connectors);

                // Asynchronously log integration bus synchronization for each process type
                foreach (var processType in processTypes)
                {
                    // Asynchronously log integration bus synchronization for simple objects
                    var procTypeConnectors = FilterConnectors(connectors, processType);

                    var procTypeTasks = IntegrationTaskInfoProvider.LogInternalIntegration(infoObj, taskType, processType, siteId, procTypeConnectors.ToList());
                    if (procTypeTasks != null)
                    {
                        tasks.Add(procTypeTasks);
                    }
                }
            }

            return tasks;
        }


        /// <summary>
        /// Adds connectors to collection to be processed.
        /// </summary>
        /// <param name="connectors">Connectors to process</param>
        /// <returns>True if at least one connector was touched</returns>
        public static bool TouchAsyncConnectors(Dictionary<string, TaskProcessTypeEnum> connectors)
        {
            // Get only async connectors
            List<string> asyncConnectors = FilterAsyncConnectors(connectors).ToList();
            bool touched = (asyncConnectors.Count > 0);

            // Add connector names for which the task was logged
            TouchedConnectorNames = TouchedConnectorNames.Union(asyncConnectors).ToList();

            return touched;
        }

        #endregion


        #region "IntegrationProcessResultEnum manipulation"

        /// <summary>
        /// Returns the process result as a string.
        /// </summary>
        /// <param name="result">Process result enumeration value</param>
        public static string GetIntegrationProcessResultString(IntegrationProcessResultEnum result)
        {
            switch (result)
            {
                case IntegrationProcessResultEnum.Error:
                    return "ERROR";

                case IntegrationProcessResultEnum.ErrorAndSkip:
                    return "ERRORANDSKIP";

                case IntegrationProcessResultEnum.SkipNow:
                    return "SKIPNOW";

                default:
                    return "OK";
            }
        }


        /// <summary>
        /// Returns the process result enumeration value.
        /// </summary>
        /// <param name="result">String representing process result</param>
        public static IntegrationProcessResultEnum GetIntegrationProcessResultEnum(string result)
        {
            if (result == null)
            {
                return IntegrationProcessResultEnum.OK;
            }

            switch (result.ToUpperCSafe())
            {
                case "ERROR":
                    return IntegrationProcessResultEnum.Error;

                case "ERRORANDSKIP":
                    return IntegrationProcessResultEnum.ErrorAndSkip;

                case "SKIPNOW":
                    return IntegrationProcessResultEnum.SkipNow;

                default:
                    return IntegrationProcessResultEnum.OK;
            }
        }

        #endregion


        #region "IntegrationProcessTypeEnum manipulation"

        /// <summary>
        /// Returns the process type as a string.
        /// </summary>
        /// <param name="result">Process type enumeration value</param>
        public static string GetIntegrationProcessTypeString(IntegrationProcessTypeEnum result)
        {
            switch (result)
            {
                case IntegrationProcessTypeEnum.Error:
                    return "ERROR";

                case IntegrationProcessTypeEnum.DeleteOnError:
                    return "DELETEONERROR";

                case IntegrationProcessTypeEnum.SkipOnError:
                    return "SKIPONERROR";

                case IntegrationProcessTypeEnum.SkipOnce:
                    return "SKIPONCE";

                default:
                    return "DEFAULT";
            }
        }


        /// <summary>
        /// Returns the process type enumeration value.
        /// </summary>
        /// <param name="result">String representing process type</param>
        public static IntegrationProcessTypeEnum GetIntegrationProcessTypeEnum(string result)
        {
            if (result == null)
            {
                return IntegrationProcessTypeEnum.Default;
            }

            switch (result.ToUpperCSafe())
            {
                case "ERROR":
                    return IntegrationProcessTypeEnum.Error;

                case "SKIPONERROR":
                    return IntegrationProcessTypeEnum.SkipOnError;

                case "SKIPONCE":
                    return IntegrationProcessTypeEnum.SkipOnce;

                case "DELETEONERROR":
                    return IntegrationProcessTypeEnum.DeleteOnError;

                default:
                    return IntegrationProcessTypeEnum.Default;
            }
        }

        #endregion


        #region "TaskDataTypeEnum manipulation"

        /// <summary>
        /// Returns the task data type as a string.
        /// </summary>
        /// <param name="type">Task data type enumeration value</param>
        public static string GetTaskDataTypeString(TaskDataTypeEnum type)
        {
            switch (type)
            {
                case TaskDataTypeEnum.SimpleSnapshot:
                    return "SIMPLESNAPSHOT";

                case TaskDataTypeEnum.Snapshot:
                    return "SNAPSHOT";

                default:
                    return "SIMPLE";
            }
        }


        /// <summary>
        /// Returns the task data type enumeration value.
        /// </summary>
        /// <param name="type">String representing task data type</param>
        public static TaskDataTypeEnum GetTaskDataTypeEnum(string type)
        {
            if (type == null)
            {
                return TaskDataTypeEnum.Simple;
            }

            switch (type.ToUpperCSafe())
            {
                case "SIMPLESNAPSHOT":
                    return TaskDataTypeEnum.SimpleSnapshot;

                case "SNAPSHOT":
                    return TaskDataTypeEnum.Snapshot;

                default:
                    return TaskDataTypeEnum.Simple;
            }
        }


        /// <summary>
        /// Returns the task data type enumeration value based on task process type.
        /// </summary>
        /// <param name="taskProcessType">Task process type value</param>
        /// <returns>Task data type</returns>
        public static TaskDataTypeEnum GetTaskDataTypeEnum(TaskProcessTypeEnum taskProcessType)
        {
            TaskDataTypeEnum dataType;

            switch (taskProcessType)
            {
                case TaskProcessTypeEnum.AsyncSimpleSnapshot:
                    dataType = TaskDataTypeEnum.SimpleSnapshot;
                    break;

                case TaskProcessTypeEnum.AsyncSnapshot:
                    dataType = TaskDataTypeEnum.Snapshot;
                    break;

                case TaskProcessTypeEnum.SyncSnapshot:
                    dataType = TaskDataTypeEnum.Snapshot;
                    break;

                default:
                    dataType = TaskDataTypeEnum.Simple;
                    break;
            }

            return dataType;
        }

        #endregion
    }
}
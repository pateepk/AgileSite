using System;
using System.Collections.Generic;
using System.Data;

using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing IntegrationTaskInfo management.
    /// </summary>
    public class IntegrationTaskInfoProvider : AbstractInfoProvider<IntegrationTaskInfo, IntegrationTaskInfoProvider>
    {
        #region "Public methods - Basic"

        /// <summary>
        /// Returns all integration tasks.
        /// </summary>
        public static ObjectQuery<IntegrationTaskInfo> GetIntegrationTasks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns integration task with specified ID.
        /// </summary>
        /// <param name="taskId">Integration task ID</param>        
        public static IntegrationTaskInfo GetIntegrationTaskInfo(int taskId)
        {
            return ProviderObject.GetInfoById(taskId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified integration task.
        /// </summary>
        /// <param name="taskObj">Integration task to be set</param>
        public static void SetIntegrationTaskInfo(IntegrationTaskInfo taskObj)
        {
            ProviderObject.SetInfo(taskObj);
        }


        /// <summary>
        /// Deletes specified integration task.
        /// </summary>
        /// <param name="taskObj">Integration task to be deleted</param>
        public static void DeleteIntegrationTaskInfo(IntegrationTaskInfo taskObj)
        {
            ProviderObject.DeleteInfo(taskObj);
        }


        /// <summary>
        /// Deletes integration task with specified ID.
        /// </summary>
        /// <param name="taskId">Integration task ID</param>
        public static void DeleteIntegrationTaskInfo(int taskId)
        {
            IntegrationTaskInfo taskObj = GetIntegrationTaskInfo(taskId);
            DeleteIntegrationTaskInfo(taskObj);
        }


        /// <summary>
        /// Returns dataset of all integration tasks from view matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>
        /// <param name="columns">Columns to be selected</param>
        public static DataSet GetIntegrationTasksView(string where, string orderBy, int topN, string columns)
        {
            return ProviderObject.GetIntegrationTasksViewInternal(where, orderBy, topN, columns);
        }

        #endregion


        #region "Public methods - Advanced"

        /// <summary>
        /// Logs the integration task for the given object.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Task type</param>
        /// <param name="taskProcessType">Processing type</param>
        /// <param name="siteId">Task site identifier</param>
        /// <param name="connectorNames">Integration connector names for which to log the synchronization (nothing is logged when no connector is specified)</param>
        /// <returns>Returns new integration task</returns>
        public static IntegrationTaskInfo LogInternalIntegration(GeneralizedInfo infoObj, TaskTypeEnum taskType, TaskProcessTypeEnum taskProcessType, int siteId, List<string> connectorNames)
        {
            return ProviderObject.LogInternalIntegrationInternal(infoObj, taskType, taskProcessType, siteId, connectorNames);
        }


        /// <summary>
        /// Logs the integration task for the given object.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Task type</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="result">What to do when processing fails</param>
        /// <param name="connectorName">Integration connector names for which to log the synchronization (nothing is logged when no connector is specified)</param>
        /// <param name="th">Translation helper</param>
        /// <param name="siteName">Site name of the target site (for site objects)</param>
        /// <returns>Returns new integration task</returns>
        public static IntegrationTaskInfo LogExternalIntegration(GeneralizedInfo infoObj, TaskTypeEnum taskType, TaskDataTypeEnum dataType, IntegrationProcessTypeEnum result, string connectorName, TranslationHelper th, string siteName)
        {
            return ProviderObject.LogExternalIntegrationInternal(infoObj, taskType, dataType, result, connectorName, th, siteName);
        }


        /// <summary>
        /// Fetches internal task for processing.
        /// </summary>
        /// <param name="connectorId">Connector identifier</param>
        /// <param name="minIntegrationTaskId">Task identifier to start from</param>
        /// <returns>Internal task to be processed</returns>
        public static IntegrationTaskInfo FetchInternalTask(int connectorId, int minIntegrationTaskId)
        {
            return FetchTask(connectorId, true, minIntegrationTaskId);
        }


        /// <summary>
        /// Fetches external task for processing.
        /// </summary>
        /// <param name="connectorId">Connector identifier</param>
        /// <param name="minIntegrationTaskId">Task identifier to start from</param>
        /// <returns>External task to be processed</returns>
        public static IntegrationTaskInfo FetchExternalTask(int connectorId, int minIntegrationTaskId)
        {
            return FetchTask(connectorId, false, minIntegrationTaskId);
        }


        /// <summary>
        /// Creates synchronization for given task and connector
        /// </summary>
        /// <param name="connectorName">Name of connector</param>
        /// <param name="taskInfo">Task info object</param>
        public static void CreateSynchronization(string connectorName, IntegrationTaskInfo taskInfo)
        {
            IntegrationConnectorInfo connector = IntegrationConnectorInfoProvider.GetIntegrationConnectorInfo(connectorName);
            IntegrationSynchronizationInfo syncInfo = new IntegrationSynchronizationInfo();
            syncInfo.SynchronizationConnectorID = connector.ConnectorID;
            syncInfo.SynchronizationTaskID = taskInfo.TaskID;
            IntegrationSynchronizationInfoProvider.SetIntegrationSynchronizationInfo(syncInfo);
        }

        #endregion


        #region "Internal methods - Basic"

        /// <summary>
        /// Returns dataset of all integration tasks from view matching the specified parameters.
        /// </summary>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by expression</param>
        /// <param name="topN">Number of records to be selected</param>
        /// <param name="columns">Columns to be selected</param>
        protected virtual DataSet GetIntegrationTasksViewInternal(string where, string orderBy, int topN, string columns)
        {
            return ConnectionHelper.ExecuteQuery("integration.task.selectlist", null, where, orderBy, topN, columns);
        }

        #endregion


        #region "Internal methods - Advanced"

        /// <summary>
        /// Logs the integration task for the given object.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Task type</param>
        /// <param name="taskProcessType">Processing type</param>
        /// <param name="siteId">Task site identifier</param>
        /// <param name="connectorNames">Integration connector names for which to log the synchronization (nothing is logged when no connector is specified)</param>
        /// <returns>Returns new integration task</returns>
        protected virtual IntegrationTaskInfo LogInternalIntegrationInternal(GeneralizedInfo infoObj, TaskTypeEnum taskType, TaskProcessTypeEnum taskProcessType, int siteId, List<string> connectorNames)
        {
            // Check object instance
            if (infoObj == null)
            {
                throw new Exception("[IntegrationTaskInfoProvider.LogInternalIntegration]: Missing object instance.");
            }
            else if ((connectorNames == null) || (connectorNames.Count == 0) || !IntegrationHelper.IntegrationLogInternal)
            {
                return null;
            }

            try
            {
                // Lock on the object instance to ensure only single running logging for the object
                lock (infoObj.GetLockObject())
                {
                    // Change task type and object for site binding
                    SynchronizationHelper.ChangeSiteBindingObject(ref infoObj, ref siteId, ref taskType);

                    // Check allowed task types
                    switch (taskType)
                    {
                        case TaskTypeEnum.UpdateObject:
                        case TaskTypeEnum.CreateObject:
                        case TaskTypeEnum.AddToSite:
                        case TaskTypeEnum.DeleteObject:
                        case TaskTypeEnum.RemoveFromSite:
                            break;

                        default:
                            throw new Exception("[IntegrationTaskInfoProvider.LogInternalIntegration]: Task type '" + taskType + "' not supported.");
                    }

                    var settings = new SynchronizationObjectSettings();
                    settings.IncludeOtherBindings = false;

                    TaskDataTypeEnum dataType = IntegrationHelper.GetTaskDataTypeEnum(taskProcessType);

                    switch (taskProcessType)
                    {
                        case TaskProcessTypeEnum.AsyncSimpleSnapshot:
                            settings.HandleBoundObjects = false;
                            break;

                        case TaskProcessTypeEnum.AsyncSimple:
                            settings.IncludeTranslations = false;
                            settings.HandleBoundObjects = false;
                            break;

                        case TaskProcessTypeEnum.SyncSnapshot:
                            throw new Exception("[IntegrationTaskInfoProvider.LogInternalIntegration]: Task process type '" + taskProcessType + "' not supported.");
                    }

                    // Get object XML
                    settings.Operation = OperationTypeEnum.Synchronization;
                    settings.IncludeSiteBindings = false;
                    settings.TranslationHelper = new TranslationHelper();

                    string xml = SynchronizationHelper.GetObjectXml(settings, infoObj, taskType);

                    // Create task info object
                    var ti = new IntegrationTaskInfo()
                        {
                            TaskObjectID = infoObj.ObjectID,
                            TaskObjectType = infoObj.TypeInfo.ObjectType,
                            TaskData = xml,
                            TaskTime = DateTime.Now,
                            TaskTitle = StagingTaskInfoProvider.GetTaskTitle(taskType, infoObj, settings.TranslationHelper),
                            TaskType = taskType,
                            TaskIsInbound = false,
                            TaskSiteID = siteId,
                            TaskDataType = dataType,
                        };

                    using (var h = IntegrationEvents.LogInternalTask.StartEvent(ti, infoObj))
                    {
                        if (h.CanContinue())
                        {
                            // Save within transaction to keep multithreaded consistency in DB
                            using (var tr = new CMSTransactionScope())
                            {
                                SetIntegrationTaskInfo(ti);

                                foreach (string connectorName in connectorNames)
                                {
                                    CreateSynchronization(connectorName, ti);
                                }

                                tr.Commit();
                            }
                        }

                        h.FinishEvent();
                    }

                    return ti;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("Integration", "LogObject", ex);
            }

            return null;
        }


        /// <summary>
        /// Logs the integration task for the given object.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Task type</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="result">What to do when processing fails</param>
        /// <param name="connectorName">Integration connector names for which to log the synchronization (nothing is logged when no connector is specified)</param>
        /// <param name="th">Translation helper</param>
        /// <param name="siteName">Site name of the target site (for site objects)</param>
        /// <returns>Returns new integration task</returns>
        protected virtual IntegrationTaskInfo LogExternalIntegrationInternal(GeneralizedInfo infoObj, TaskTypeEnum taskType, TaskDataTypeEnum dataType, IntegrationProcessTypeEnum result, string connectorName, TranslationHelper th, string siteName)
        {
            // Check object instance
            if (infoObj == null)
            {
                throw new Exception("[IntegrationTaskInfoProvider.LogExternalIntegration]: Missing object instance.");
            }
            else if ((IntegrationHelper.GetConnector(connectorName) == null) || !IntegrationHelper.IntegrationLogExternal)
            {
                return null;
            }

            try
            {
                // Lock on the object instance to ensure only single running logging for the object
                lock (infoObj.GetLockObject())
                {
                    var settings = new SynchronizationObjectSettings();
                    settings.IncludeOtherBindings = false;
                    switch (dataType)
                    {
                        case TaskDataTypeEnum.Simple:
                            settings.HandleBoundObjects = false;
                            settings.IncludeTranslations = false;
                            break;

                        case TaskDataTypeEnum.SimpleSnapshot:
                            settings.HandleBoundObjects = false;
                            break;
                    }

                    settings.Operation = OperationTypeEnum.Synchronization;
                    settings.TranslationHelper = th;
                    settings.IncludeSiteBindings = false;
                    settings.ProcessTranslations = false;

                    // Object is complete, do not gather any additional information
                    infoObj.Disconnect();

                    // Get object XML
                    string xml = SynchronizationHelper.GetObjectXml(settings, infoObj, taskType);

                    // Create task info object
                    var ti = new IntegrationTaskInfo()
                        {
                            TaskObjectID = infoObj.ObjectID,
                            TaskObjectType = infoObj.TypeInfo.ObjectType,
                            TaskData = xml,
                            TaskTime = DateTime.Now,
                            TaskTitle = StagingTaskInfoProvider.GetTaskTitle(taskType, infoObj, th),
                            TaskType = taskType,
                            TaskIsInbound = true,
                            TaskProcessType = result,
                            TaskDataType = dataType
                        };

                    // Ensure site assignment
                    if (siteName != null)
                    {
                        int siteId = ProviderHelper.GetId(PredefinedObjectType.SITE, siteName);
                        if (siteId <= 0)
                        {
                            throw new Exception(string.Format("[IntegrationTaskInfoProvider.LogExternalIntegration]: Target site '{0}' not found!", siteName));
                        }
                        ti.TaskSiteID = siteId;
                    }
                    else
                    {
                        ti.TaskSiteID = 0;
                    }

                    using (var h = IntegrationEvents.LogExternalTask.StartEvent(ti, infoObj))
                    {
                        if (h.CanContinue())
                        {
                            // Save within transaction to keep multithreaded consistency in DB
                            using (var tr = new CMSTransactionScope())
                            {
                                SetIntegrationTaskInfo(ti);

                                CreateSynchronization(connectorName, ti);

                                tr.Commit();
                            }
                        }

                        h.FinishEvent();
                    }

                    return ti;
                }
            }
            catch (Exception ex)
            {
                // Log the error
                EventLogProvider.LogException("Integration", "LogObject", ex);
            }
            return null;
        }


        /// <summary>
        /// Fetches task for processing.
        /// </summary>
        /// <param name="connectorId">Connector identifier</param>
        /// <param name="internalTask">Whether to fetch internal or external task</param>
        /// <param name="minIntegrationTaskId">Task identifier to start from</param>
        /// <returns>Task to be processed</returns>
        private static IntegrationTaskInfo FetchTask(int connectorId, bool internalTask, int minIntegrationTaskId)
        {
            // Build where and create parameters
            string where = "TaskIsInbound = " + Convert.ToInt32(!internalTask);
            where = SqlHelper.AddWhereCondition(where, "TaskID >= " + minIntegrationTaskId);
            QueryDataParameters dataParams = new QueryDataParameters();
            dataParams.Add("@SynchronizationConnectorID", connectorId);

            // Explicitly name columns
            string columns = "[" + string.Join("],[", new IntegrationTaskInfo().ColumnNames.ToArray()) + "]" + ",[SynchronizationIsRunning]";

            // Fetch task
            DataSet ds = ConnectionHelper.ExecuteQuery("integration.task.fetchtask", dataParams, where, "TaskID ASC", 1, columns);
            IntegrationTaskInfo task = null;
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                DataRow taskRow = ds.Tables[0].Rows[0];
                bool isRunning = ValidationHelper.GetBoolean(taskRow["SynchronizationIsRunning"], false);
                if (!isRunning)
                {
                    task = new IntegrationTaskInfo(taskRow);
                }
            }
            return task;
        }

        #endregion
    }
}
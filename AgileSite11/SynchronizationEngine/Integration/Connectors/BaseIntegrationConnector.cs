using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Net;
using System.Reflection;

using CMS.Base;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.SynchronizationEngine
{
    /// <summary>
    /// Class that needs to be inherited during implementing custom integration connector.
    /// Several members have to be implemented in order to achieve desired functionality.
    /// </summary>
    public abstract class BaseIntegrationConnector : AbstractIntegrationConnector
    {
        #region "Private variables"

        // ID translation table [className.ToLowerCSafe()] -> Hashtable [OldID -> NewID]
        private Hashtable internalTranslations = null;

        // ID translation table [className.ToLowerCSafe()] -> Hashtable [ID -> "codename|sitename|parentid"]
        private Hashtable externalTranslations = null;

        // SyncManager instance.
        private SyncManager mSyncManager = null;

        // Says whether to clear cached translation information.
        private bool mClearTranslationsAfterProcessing = true;

        #endregion


        #region "Properties"

        /// <summary>
        /// Determines whether to log integration tasks when processing external tasks.
        /// False by default.
        /// </summary>
        public bool LogIntegrationForExternalTasks
        {
            get;
            set;
        }


        /// <summary>
        /// Fetches internal integration tasks for processing.
        /// </summary>
        private IEnumerable<IntegrationTaskInfo> FetchedInternalTasks
        {
            get
            {
                IntegrationTaskInfo task = null;
                int minTaskId = 0;
                while ((task = IntegrationTaskInfoProvider.FetchInternalTask(ConnectorID, minTaskId)) != null)
                {
                    minTaskId = task.TaskID + 1;
                    yield return task;
                }
            }
        }


        /// <summary>
        /// Fetches external integration tasks for processing.
        /// </summary>
        private IEnumerable<IntegrationTaskInfo> FetchedExternalTasks
        {
            get
            {
                IntegrationTaskInfo task = null;
                int minTaskId = 0;
                while ((task = IntegrationTaskInfoProvider.FetchExternalTask(ConnectorID, minTaskId)) != null)
                {
                    minTaskId = task.TaskID + 1;
                    yield return task;
                }
            }
        }


        /// <summary>
        /// SyncManager instance.
        /// </summary>
        protected SyncManager SyncManager
        {
            get
            {
                if (mSyncManager == null)
                {
                    var man = SyncManager.GetInstance();
                    man.OperationType = OperationTypeEnum.Integration;

                    mSyncManager = man;
                }

                return mSyncManager;
            }
        }


        /// <summary>
        /// Says whether to clear cached translation information (true by default). Applies both to internal and external tasks.
        /// If switched to false, translations can still be cleared by calling ClearInternalTranslations() or ClearExternalTranslations().
        /// </summary>
        protected bool ClearTranslationsAfterProcessing
        {
            get
            {
                return mClearTranslationsAfterProcessing;
            }
            set
            {
                mClearTranslationsAfterProcessing = value;
            }
        }

        #endregion


        #region "Public processing methods"

        /// <summary>
        /// Processes incoming task.
        /// </summary>
        /// <param name="obj">Object or document to process (either already prepared ICMSObject or raw external object)</param>
        /// <param name="result">What to do if the processing fails</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of input data</param>
        /// <param name="siteName">Name of the target site</param>
        public override void ProcessExternalTask(object obj, IntegrationProcessTypeEnum result, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName)
        {
            // Try to get ICMSObject or prepare it from external object
            ICMSObject cmsObject = obj as ICMSObject ?? PrepareInternalObject(obj, taskType, dataType, siteName);
            if (cmsObject != null)
            {
                // Process task with prepared ICMSObject
                ProcessExternalTask(cmsObject, result, taskType, dataType, siteName);
            }
        }


        /// <summary>
        /// Processes incoming task.
        /// </summary>
        /// <param name="cmsObject">Object or document to process</param>
        /// <param name="result">What to do if the processing fails</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of input data</param>
        /// <param name="siteName">Name of the target site</param>
        private void ProcessExternalTask(ICMSObject cmsObject, IntegrationProcessTypeEnum result, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName)
        {
            if (IntegrationHelper.IntegrationLogExternal)
            {
                TranslationHelper th = null;
                bool isDocument = cmsObject is TreeNode;
                // Snapshot is not supported for documents
                if (isDocument && (dataType == TaskDataTypeEnum.Snapshot))
                {
                    dataType = TaskDataTypeEnum.SimpleSnapshot;
                }

                // Prepare translations
                switch (dataType)
                {
                    case TaskDataTypeEnum.SimpleSnapshot:
                    case TaskDataTypeEnum.Snapshot:
                        th = PrepareInternalTranslations(cmsObject, dataType);
                        break;
                }

                // Log task
                if (isDocument)
                {
                    DocumentSynchronizationHelper.LogExternalIntegration(cmsObject as TreeNode, null, taskType, dataType, result, ConnectorName, th, siteName);
                }
                else
                {
                    if (cmsObject is BaseInfo)
                    {
                        // Make sure method accepts also BaseInfo
                        cmsObject = ((BaseInfo)cmsObject).Generalized;
                    }
                    IntegrationTaskInfoProvider.LogExternalIntegration(cmsObject as GeneralizedInfo, taskType, dataType, result, ConnectorName, th, siteName);
                }
            }
        }


        /// <summary>
        /// Suitable for implementing outcoming CMS object processing. Identifiers of object is already prepared to match external application.
        /// </summary>
        /// <param name="cmsObject">CMS object to process (accepts TreeNode or GeneralizedInfo)</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="errorMessage">Possible error message</param>
        /// <param name="siteName">Name of site</param>
        /// <param name="translations">Translation helper object containing translations for given object</param>
        /// <returns>Processing result</returns>
        public override IntegrationProcessResultEnum ProcessInternalTaskAsync(ICMSObject cmsObject, TranslationHelper translations, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName, out string errorMessage)
        {
            if (cmsObject is GeneralizedInfo)
            {
                return ProcessInternalTaskAsync(cmsObject as GeneralizedInfo, translations, taskType, dataType, siteName, out errorMessage);
            }
            else if (cmsObject is TreeNode)
            {
                return ProcessInternalTaskAsync(cmsObject as TreeNode, translations, taskType, dataType, siteName, out errorMessage);
            }
            else
            {
                errorMessage = "[BaseIntegrationConnector.ProcessInternalTaskAsync]: Type of object was not recognized.";
                return IntegrationProcessResultEnum.Error;
            }
        }


        /// <summary>
        /// Suitable for implementing synchronous outcoming CMS object processing.
        /// </summary>
        /// <param name="cmsObject">CMS object to process</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="errorMessage">Possible error message</param>
        /// <param name="siteName">Name of site</param>
        /// <returns>Processing result</returns>
        public override IntegrationProcessResultEnum ProcessInternalTaskSync(ICMSObject cmsObject, TaskTypeEnum taskType, string siteName, out string errorMessage)
        {
            if (cmsObject is GeneralizedInfo)
            {
                return ProcessInternalTaskSync(cmsObject as GeneralizedInfo, taskType, siteName, out errorMessage);
            }
            else if (cmsObject is TreeNode)
            {
                return ProcessInternalTaskSync(cmsObject as TreeNode, taskType, siteName, out errorMessage);
            }
            else
            {
                errorMessage = "[BaseIntegrationConnector.ProcessInternalTaskSync]: Type of object was not recognized.";
                return IntegrationProcessResultEnum.Error;
            }
        }


        /// <summary>
        /// Performs a request to a special page on server defined by given url causing immediate task processing for all connectors.
        /// </summary>
        /// <param name="serverUrl">URL of the server to make the request to</param>
        public HttpStatusCode RequestTasksProcessing(string serverUrl)
        {
            return RequestTasksProcessing(serverUrl, null);
        }


        /// <summary>
        /// Performs a request to a special page on server defined by given url causing immediate task processing for connector specified by name.
        /// </summary>
        /// <param name="serverUrl">URL of the server to make the request to</param>
        /// <param name="connectorName">Name of connector to notify</param>
        public HttpStatusCode RequestTasksProcessing(string serverUrl, string connectorName)
        {
            string url = serverUrl.TrimEnd('/') + "/CMSPages/IntegrationNotify.aspx";
            if (!string.IsNullOrEmpty(connectorName))
            {
                URLHelper.AddParameterToUrl(url, "connectorName", connectorName);
            }
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            if (request != null)
            {
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                if (response != null)
                {
                    return response.StatusCode;
                }
            }
            return HttpStatusCode.InternalServerError;
        }


        /// <summary>
        /// Processes single internal task.
        /// </summary>
        /// <param name="task">Task info object</param>
        /// <returns>Processing result</returns>
        public string ProcessInternalTask(IntegrationTaskInfo task)
        {
            List<IntegrationTaskInfo> tasks = new List<IntegrationTaskInfo>();
            tasks.Add(task);
            return ProcessInternalTasks(tasks);
        }


        /// <summary>
        /// Processes single external task.
        /// </summary>
        /// <param name="task">Task info object</param>
        /// <returns>Processing result</returns>
        public string ProcessExternalTask(IntegrationTaskInfo task)
        {
            List<IntegrationTaskInfo> tasks = new List<IntegrationTaskInfo>();
            tasks.Add(task);
            return ProcessExternalTasks(tasks);
        }


        /// <summary>
        /// Fetches internal tasks and processes them one by one.
        /// </summary>
        public override void ProcessInternalTasks()
        {
            ProcessInternalTasks(FetchedInternalTasks);
        }


        /// <summary>
        /// Fetches external tasks and processes them one by one.
        /// </summary>
        public override void ProcessExternalTasks()
        {
            ProcessExternalTasks(FetchedExternalTasks);
        }


        /// <summary>
        /// Processes given internal tasks one by one.
        /// </summary>
        /// <param name="tasks">Tasks to process</param>
        protected string ProcessInternalTasks(IEnumerable<IntegrationTaskInfo> tasks)
        {
            string errorMessage = null;

            if (IntegrationHelper.IntegrationProcessInternal)
            {
                foreach (IntegrationTaskInfo task in tasks)
                {
                    bool clearRunningStatus = false;

                    try
                    {
                        IntegrationSynchronizationInfo synchronization = null;
                        IntegrationProcessResultEnum result = default(IntegrationProcessResultEnum);

                        if (!LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.IntegrationBus))
                        {
                            errorMessage = ResHelper.GetAPIString("integration.errorlicense", "Current license does not allow using the Integration module.");
                            result = IntegrationProcessResultEnum.Error;
                        }
                        else
                        {
                            try
                            {
                                if (WebFarmHelper.WebFarmEnabled)
                                {
                                    // Set flag indicating that task is being processed
                                    IntegrationSynchronizationInfoProvider.SetIsRunning(ConnectorID, task.TaskID, true);
                                    clearRunningStatus = true;
                                }

                                string objectType = task.TaskObjectType;
                                ICMSObject cmsObject = null;
                                TranslationHelper th = null;

                                // Document task
                                if (string.IsNullOrEmpty(objectType))
                                {
                                    string className = DocumentHierarchyHelper.GetNodeClassName(task.TaskData, ExportFormatEnum.XML);

                                    // Create new node instance
                                    TreeNode node = TreeNode.New(className);

                                    // Fill instance with data
                                    th = DocumentHierarchyHelper.LoadObjectFromXML(node, task.TaskData);

                                    // Do not remove - will be ensured in the future
                                    node.Generalized.Disconnect();
                                    cmsObject = node;
                                }
                                // Object task
                                else
                                {
                                    // Create info object of specified type
                                    GeneralizedInfo infoObj = ModuleManager.GetObject(objectType);

                                    // Load data into info object and get translation helper
                                    th = HierarchyHelper.LoadObjectFromXML(OperationTypeEnum.Integration, infoObj, task.TaskData);

                                    // Disconnect object to prevent loading any additional data
                                    infoObj.Disconnect();

                                    cmsObject = infoObj;
                                }

                                string siteName = null;

                                // Get site name
                                if (task.TaskSiteID > 0)
                                {
                                    siteName = SiteInfoProvider.GetSiteName(task.TaskSiteID);
                                }

                                // Process task
                                result = ProcessInternalTaskAsync(cmsObject, th, task.TaskType, task.TaskDataType, siteName, out errorMessage);
                            }
                            catch (Exception ex)
                            {
                                result = IntegrationProcessResultEnum.Error;
                                errorMessage = "[BaseIntegrationConnector.ProcessInternalTask]: Error processing task. Original message: " + ex.Message;
                            }
                            finally
                            {
                                // Clear translations
                                if (ClearTranslationsAfterProcessing)
                                {
                                    ClearInternalTranslations();
                                }
                            }
                        }
                        switch (result)
                        {
                            // Processing was successful, delete synchronization and continue
                            case IntegrationProcessResultEnum.OK:
                                synchronization = IntegrationSynchronizationInfoProvider.GetIntegrationSynchronizationInfo(ConnectorID, task.TaskID);
                                IntegrationSynchronizationInfoProvider.DeleteIntegrationSynchronizationInfo(synchronization);
                                clearRunningStatus = false;
                                continue;

                            // Log error and stop processing
                            case IntegrationProcessResultEnum.Error:
                                synchronization = IntegrationSynchronizationInfoProvider.GetIntegrationSynchronizationInfo(ConnectorID, task.TaskID);
                                IntegrationHelper.LogSynchronizationError(synchronization, errorMessage);
                                return errorMessage;

                            // Log error and continue
                            case IntegrationProcessResultEnum.ErrorAndSkip:
                                synchronization = IntegrationSynchronizationInfoProvider.GetIntegrationSynchronizationInfo(ConnectorID, task.TaskID);
                                IntegrationHelper.LogSynchronizationError(synchronization, errorMessage);
                                continue;

                            // Do not react
                            case IntegrationProcessResultEnum.SkipNow:
                                continue;
                        }
                    }
                    finally
                    {
                        if (clearRunningStatus)
                        {
                            // Task is no longer being processed
                            IntegrationSynchronizationInfoProvider.SetIsRunning(ConnectorID, task.TaskID, false);
                        }
                    }
                }
            }

            return errorMessage;
        }


        /// <summary>
        /// Processes given external tasks one by one.
        /// </summary>
        /// <param name="tasks">Tasks to process</param>
        protected string ProcessExternalTasks(IEnumerable<IntegrationTaskInfo> tasks)
        {
            string errorMessage = null;

            if (IntegrationHelper.IntegrationProcessExternal)
            {
                foreach (IntegrationTaskInfo task in tasks)
                {
                    bool clearRunningStatus = false;

                    try
                    {
                        IntegrationSynchronizationInfo synchronization = null;

                        IntegrationProcessTypeEnum result = task.TaskProcessType;
                        if (!LicenseHelper.IsFeatureAvailableInBestLicense(FeatureEnum.IntegrationBus))
                        {
                            errorMessage = ResHelper.GetAPIString("integration.errorlicense", "Current license does not allow using the Integration module.");
                            result = IntegrationProcessTypeEnum.Error;
                        }
                        // Do not process the task and log error
                        if (result == IntegrationProcessTypeEnum.Error)
                        {
                            synchronization = IntegrationSynchronizationInfoProvider.GetIntegrationSynchronizationInfo(ConnectorID, task.TaskID);
                            IntegrationHelper.LogSynchronizationError(synchronization, errorMessage);
                            return errorMessage;
                        }
                        // Do not process the task now and change state so it can be processed next time
                        if (result == IntegrationProcessTypeEnum.SkipOnce)
                        {
                            task.TaskProcessType = IntegrationProcessTypeEnum.Default;
                            IntegrationTaskInfoProvider.SetIntegrationTaskInfo(task);
                            continue;
                        }

                        bool errorOccurred = false;

                        try
                        {
                            if (WebFarmHelper.WebFarmEnabled)
                            {
                                // Set flag indicating that task is being processed
                                IntegrationSynchronizationInfoProvider.SetIsRunning(ConnectorID, task.TaskID, true);
                                clearRunningStatus = true;
                            }
                            // Ensure site name
                            if (task.TaskSiteID > 0)
                            {
                                SyncManager.SiteName = SiteInfoProvider.GetSiteName(task.TaskSiteID);
                            }

                            // Process task
                            bool processChildren = (task.TaskDataType == TaskDataTypeEnum.Snapshot);
                            using (new CMSActionContext { LogIntegration = LogIntegrationForExternalTasks })
                            {
                                SyncManager.ProcessTask(task.TaskType, task.TaskObjectType, task.TaskData, null, processChildren);
                            }
                        }
                        catch (Exception ex)
                        {
                            errorOccurred = true;
                            errorMessage = "[BaseIntegrationConnector.ProcessExternalTasks]: Error processing task. Original message: " + ex.Message;
                        }
                        finally
                        {
                            // Clear translations
                            if (ClearTranslationsAfterProcessing)
                            {
                                ClearExternalTranslations();
                            }
                        }
                        switch (result)
                        {
                            // Error and SkipOnce were already handled 
                            case IntegrationProcessTypeEnum.DeleteOnError:
                            case IntegrationProcessTypeEnum.Default:
                            case IntegrationProcessTypeEnum.SkipOnError:
                                synchronization = IntegrationSynchronizationInfoProvider.GetIntegrationSynchronizationInfo(ConnectorID, task.TaskID);
                                if (!errorOccurred)
                                {
                                    // Processing was successful, delete synchronization.
                                    IntegrationSynchronizationInfoProvider.DeleteIntegrationSynchronizationInfo(synchronization);
                                    clearRunningStatus = false;
                                }
                                else
                                {
                                    if (result == IntegrationProcessTypeEnum.Default)
                                    {
                                        // When failed, set process type to error
                                        task.TaskProcessType = IntegrationProcessTypeEnum.Error;
                                        IntegrationTaskInfoProvider.SetIntegrationTaskInfo(task);

                                        // Log processing error
                                        IntegrationHelper.LogSynchronizationError(synchronization, errorMessage);
                                        return errorMessage;
                                    }
                                    else if (result == IntegrationProcessTypeEnum.DeleteOnError)
                                    {
                                        // Delete synchronization
                                        IntegrationSynchronizationInfoProvider.DeleteIntegrationSynchronizationInfo(synchronization);
                                    }
                                    else if (result == IntegrationProcessTypeEnum.SkipOnError)
                                    {
                                        // Log error and continue processing
                                        synchronization = IntegrationSynchronizationInfoProvider.GetIntegrationSynchronizationInfo(ConnectorID, task.TaskID);
                                        IntegrationHelper.LogSynchronizationError(synchronization, errorMessage);
                                    }
                                }
                                break;
                        }
                    }
                    finally
                    {
                        if (clearRunningStatus)
                        {
                            // Task is no longer being processed
                            IntegrationSynchronizationInfoProvider.SetIsRunning(ConnectorID, task.TaskID, false);
                        }
                    }
                }
            }

            return errorMessage;
        }

        #endregion


        #region "Public subscribing methods"

        /// <summary>
        /// Assigns given subscription to this connector.
        /// </summary>
        /// <param name="match">Subscription to add</param>
        public void SubscribeTo(AbstractIntegrationSubscription match)
        {
            IntegrationHelper.SubscribeTo(match);
        }


        /// <summary>
        /// Subscribes this connector to given type of task.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskType">Type of task to subscribe to</param>
        public void SubscribeTo(TaskProcessTypeEnum taskProcessType, TaskTypeEnum taskType)
        {
            BaseIntegrationSubscription bis = new BaseIntegrationSubscription(ConnectorName, taskProcessType, taskType, null);
            SubscribeTo(bis);
        }


        /// <summary>
        /// Subscribes this connector to given types of tasks for object processing.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskTypes">Types of tasks to subscribe to</param>
        public void SubscribeToAllObjects(TaskProcessTypeEnum taskProcessType, params TaskTypeEnum[] taskTypes)
        {
            SubscribeToObjects(taskProcessType, null, taskTypes);
        }


        /// <summary>
        /// Subscribes this connector to given types of object.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="objectType">Type of object to subscribe to</param>
        public void SubscribeToObjects(TaskProcessTypeEnum taskProcessType, string objectType)
        {
            ObjectIntegrationSubscription ois = new ObjectIntegrationSubscription(ConnectorName, taskProcessType, TaskTypeEnum.All, null, objectType, null);
            SubscribeTo(ois);
        }


        /// <summary>
        /// Subscribes this connector to process tasks matching given conditions.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="objectType">Type of object to subscribe to</param>
        /// <param name="taskTypes">Types of tasks to subscribe to</param>
        public void SubscribeToObjects(TaskProcessTypeEnum taskProcessType, string objectType, params TaskTypeEnum[] taskTypes)
        {
            SubscribeToObjects(taskProcessType, objectType, null, taskTypes);
        }


        /// <summary>
        /// Subscribes this connector to process tasks matching given conditions.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="objectType">Type of object to subscribe to</param>
        /// <param name="siteName">Name of site to subscribe to</param>
        /// <param name="taskTypes">Types of tasks to subscribe to</param>
        public void SubscribeToObjects(TaskProcessTypeEnum taskProcessType, string objectType, string siteName, params TaskTypeEnum[] taskTypes)
        {
            foreach (TaskTypeEnum taskType in taskTypes)
            {
                ObjectIntegrationSubscription ois = new ObjectIntegrationSubscription(ConnectorName, taskProcessType, taskType, siteName, objectType, null);
                SubscribeTo(ois);
            }
        }


        /// <summary>
        /// Subscribes this connector to process tasks matching given conditions.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskType">Type of task to subscribe to</param>
        /// <param name="objectTypes">Types of objects to subscribe to</param>
        public void SubscribeToObjects(TaskProcessTypeEnum taskProcessType, TaskTypeEnum taskType, params string[] objectTypes)
        {
            SubscribeToObjects(taskProcessType, taskType, null, objectTypes);
        }


        /// <summary>
        /// Subscribes this connector to process tasks matching given conditions.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskType">Type of task to subscribe to</param>
        /// <param name="siteName">Name of site to subscribe to</param>
        /// <param name="objectTypes">Types of objects to subscribe to</param>
        public void SubscribeToObjects(TaskProcessTypeEnum taskProcessType, TaskTypeEnum taskType, string siteName, params string[] objectTypes)
        {
            foreach (string objectType in objectTypes)
            {
                ObjectIntegrationSubscription ois = new ObjectIntegrationSubscription(ConnectorName, taskProcessType, taskType, siteName, objectType, null);
                SubscribeTo(ois);
            }
        }


        /// <summary>
        /// Subscribes this connector to given types of tasks for document processing.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskTypes">Types of tasks to subscribe to</param>
        public void SubscribeToAllDocuments(TaskProcessTypeEnum taskProcessType, params TaskTypeEnum[] taskTypes)
        {
            SubscribeToDocuments(taskProcessType, null, null, taskTypes);
        }


        /// <summary>
        /// Subscribes this connector to given class names.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="className">Name of class to subscribe to</param>
        public void SubscribeToDocuments(TaskProcessTypeEnum taskProcessType, string className)
        {
            DocumentIntegrationSubscription dic = new DocumentIntegrationSubscription(ConnectorName, taskProcessType, TaskTypeEnum.All, null, null, null, className);
            SubscribeTo(dic);
        }


        /// <summary>
        /// Subscribes this connector to process tasks matching given conditions.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="className">Name of class to subscribe to</param>
        /// <param name="taskTypes">Types of tasks to subscribe to</param>
        public void SubscribeToDocuments(TaskProcessTypeEnum taskProcessType, string className, params TaskTypeEnum[] taskTypes)
        {
            SubscribeToDocuments(taskProcessType, className, null, taskTypes);
        }


        /// <summary>
        /// Subscribes this connector to process tasks matching given conditions.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="className">Name of class to subscribe to</param>
        /// <param name="siteName">Name of site to subscribe to</param>
        /// <param name="taskTypes">Types of tasks to subscribe to</param>
        public void SubscribeToDocuments(TaskProcessTypeEnum taskProcessType, string className, string siteName, params TaskTypeEnum[] taskTypes)
        {
            foreach (TaskTypeEnum taskType in taskTypes)
            {
                DocumentIntegrationSubscription dic = new DocumentIntegrationSubscription(ConnectorName, taskProcessType, taskType, siteName, null, null, className);
                SubscribeTo(dic);
            }
        }


        /// <summary>
        /// Subscribes this connector to process tasks matching given conditions.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskType">Type of task to subscribe to</param>
        /// <param name="classNames">Names of classes to subscribe to</param>
        public void SubscribeToDocuments(TaskProcessTypeEnum taskProcessType, TaskTypeEnum taskType, params string[] classNames)
        {
            SubscribeToDocuments(taskProcessType, taskType, null, classNames);
        }


        /// <summary>
        /// Subscribes this connector to process tasks matching given conditions.
        /// </summary>
        /// <param name="taskProcessType">Type of task processing (sync/async etc.)</param>
        /// <param name="taskType">Type of task to subscribe to</param>
        /// <param name="siteName">Name of site to subscribe to</param>
        /// <param name="classNames">Names of classes to subscribe to</param>
        public void SubscribeToDocuments(TaskProcessTypeEnum taskProcessType, TaskTypeEnum taskType, string siteName, params string[] classNames)
        {
            foreach (string className in classNames)
            {
                DocumentIntegrationSubscription dic = new DocumentIntegrationSubscription(ConnectorName, taskProcessType, taskType, siteName, null, null, className);
                SubscribeTo(dic);
            }
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Prepares translations for external document or object to enable identifying corresponding internal object.
        /// </summary>
        /// <param name="cmsObject">External CMS object</param>
        /// <param name="dataType">Type of data</param>
        /// <returns>Translations</returns>
        private TranslationHelper PrepareInternalTranslations(ICMSObject cmsObject, TaskDataTypeEnum dataType)
        {
            // Prepare settings depending on task data type
            var settings = new TraverseObjectSettings();

            settings.IncludeOtherBindings = false;
            settings.ProcessIDCallback = ProcessIDForExternalObject;

            switch (dataType)
            {
                case TaskDataTypeEnum.Simple:
                    settings.HandleBoundObjects = false;
                    settings.IncludeTranslations = false;
                    break;

                case TaskDataTypeEnum.SimpleSnapshot:
                    settings.HandleBoundObjects = false;
                    break;

                case TaskDataTypeEnum.Snapshot:
                    // Snapshot includes all data/translations
                    break;
            }

            // Go through object/document
            if (cmsObject is TreeNode)
            {
                TreeNode node = cmsObject as TreeNode;

                // Disconnect object to prevent loading of any additional data
                node.Generalized.Disconnect();
                
                DocumentHierarchyHelper.TraverseNodeStructure(settings, node);
            }
            else if (cmsObject is GeneralizedInfo)
            {
                GeneralizedInfo infoObj = cmsObject as GeneralizedInfo;

                // Disconnect object to prevent loading of any additional data
                infoObj.Disconnect();
                HierarchyHelper.TraverseObjectStructure(settings, infoObj);
            }
            else if (cmsObject is BaseInfo)
            {
                BaseInfo infoObj = cmsObject as BaseInfo;

                // Disconnect object to prevent loading of any additional data
                infoObj.Generalized.Disconnect();
                
                HierarchyHelper.TraverseObjectStructure(settings, infoObj);
            }

            return settings.TranslationHelper;
        }


        /// <summary>
        /// Translates column values of given object to match external ones.
        /// </summary>
        /// <param name="infoObj">Internal object</param>
        /// <param name="th">Translations</param>
        /// <param name="processChildren">Whether to translate columns of child objects</param>
        protected void TranslateColumnsToExternal(GeneralizedInfo infoObj, TranslationHelper th, bool processChildren)
        {
            TranslateColumnsToExternal(infoObj as ICMSObject, th, processChildren);
        }


        /// <summary>
        /// Translates column values of given document to match external ones.
        /// </summary>
        /// <param name="node">Internal document</param>
        /// <param name="th">Translations</param>
        /// <param name="processChildren">Whether to translate columns of child objects</param>
        protected void TranslateColumnsToExternal(TreeNode node, TranslationHelper th, bool processChildren)
        {
            TranslateColumnsToExternal(node as ICMSObject, th, processChildren);
        }


        /// <summary>
        /// Translates column values of given CMS object to match external ones.
        /// </summary>
        /// <param name="cmsObject">Internal CMS object</param>
        /// <param name="th">Translations</param>
        /// <param name="processChildren">Whether to translate columns of child objects</param>
        protected void TranslateColumnsToExternal(ICMSObject cmsObject, TranslationHelper th, bool processChildren)
        {
            var settings = new TraverseObjectSettings();
            settings.IncludeOtherBindings = false;

            // Register callback
            settings.ProcessIDCallback = ProcessIDForInternalObject;
            settings.TranslationHelper = th;
            settings.IncludeCategories = false;
            settings.IncludeChildren = processChildren;
            settings.IncludeBindings = processChildren;

            bool isDocument = cmsObject is TreeNode;

            // Go through document/object
            if (isDocument)
            {
                settings.IncludeChildren = false;
                
                DocumentHierarchyHelper.TraverseNodeStructure(settings, cmsObject as TreeNode);
            }
            else
            {
                if (cmsObject is BaseInfo)
                {
                    // Make sure method accepts also BaseInfo
                    cmsObject = ((BaseInfo)cmsObject).Generalized;
                }
                
                HierarchyHelper.TraverseObjectStructure(settings, cmsObject as GeneralizedInfo);
            }
        }


        /// <summary>
        /// Handles FK translations
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="obj">Object (TreeNode / Info object) to process</param>
        /// <param name="columnName">Column name of the dependency</param>
        /// <param name="objectType">Object type of the dependency</param>
        /// <param name="required">Determines whether the dependency is required (reflects required flag from TypeInfo).</param>
        private void ProcessIDForInternalObject(SynchronizationObjectSettings settings, ICMSObject obj, string columnName, string objectType, bool required)
        {
            // Get translation helper
            TranslationHelper th = settings.TranslationHelper;
            // Translate only columns containing some value
            int id = ValidationHelper.GetInteger(obj.GetValue(columnName), 0);
            if (id > 0)
            {
                // Get class name and id to get the record
                string codeName = null;

                // Try to get id from hashtable
                int? newId = GetInternalTranslation(objectType, id);
                if (newId == null)
                {
                    // Try to get translation record from translation helper
                    DataRow record = th.GetRecord(objectType, id);
                    if (record != null)
                    {
                        if (DocumentHelper.IsDocumentObjectType(objectType) || (objectType == PredefinedObjectType.NODE))
                        {
                            codeName = ValidationHelper.GetString(record["CodeName"], null);
                            var siteName = ValidationHelper.GetString(record["SiteName"], null);

                            // Parse parameters
                            Guid nodeGuid = Guid.Empty;
                            string cultureCode = null;

                            string[] parameters = codeName.Split(';');
                            switch (parameters.Length)
                            {
                                case 1:
                                    nodeGuid = new Guid(parameters[0]);
                                    break;

                                case 2:
                                    nodeGuid = new Guid(parameters[0]);
                                    cultureCode = parameters[1];
                                    break;
                            }

                            // Try to get identifier of corresponding external document
                            newId = GetExternalDocumentID(nodeGuid, cultureCode, siteName, (objectType == PredefinedObjectType.DOCUMENT));
                        }
                        else
                        {
                            GeneralizedInfo infoObject = obj as GeneralizedInfo;

                            // Get translation information
                            codeName = ValidationHelper.GetString(record["CodeName"], null);
                            string siteName = ValidationHelper.GetString(record["SiteName"], null);

                            string parentType = null;

                            int parentId = ValidationHelper.GetInteger(record["ParentID"], 0);
                            if (parentId > 0)
                            {
                                string info = ValidationHelper.GetString(record["Info"], null);
                                if (info == "ClassID")
                                {
                                    parentType = DataClassInfo.OBJECT_TYPE;
                                }
                                else if (info == "ResourceID")
                                {
                                    parentType = PredefinedObjectType.RESOURCE;
                                }
                                else if (infoObject != null)
                                {
                                    GeneralizedInfo dependencyInfo = ModuleManager.GetReadOnlyObject(objectType);
                                    parentType = dependencyInfo.ParentObjectType;
                                }
                            }
                            int newParentId = 0;
                            if (!string.IsNullOrEmpty(parentType))
                            {
                                DataRow parentRecord = th.GetRecord(parentType, parentId);
                                if (parentRecord != null)
                                {
                                    string parentCodeName = ValidationHelper.GetString(parentRecord["CodeName"], null);
                                    newParentId = GetExternalObjectID(parentType, parentCodeName, siteName, null, 0, 0);
                                }
                            }

                            int groupId = ValidationHelper.GetInteger(record["GroupID"], 0);
                            int newGroupId = 0;
                            if (groupId > 0)
                            {
                                DataRow parentRecord = th.GetRecord(PredefinedObjectType.GROUP, groupId);
                                string groupCodeName = ValidationHelper.GetString(parentRecord["CodeName"], null);
                                newGroupId = GetExternalObjectID(PredefinedObjectType.GROUP, groupCodeName, siteName, null, 0, 0);
                            }

                            // Try to get identifier of corresponding external object
                            newId = GetExternalObjectID(objectType, codeName, siteName, parentType, newParentId, newGroupId);
                        }
                    }
                }
                if (newId != null)
                {
                    // Add translation to hashtable
                    SetInternalTranslation(objectType, id, newId.Value);

                    // Set new identifier to corresponding column
                    obj.SetValue(columnName, newId.Value);
                }
                else
                {
                    // Throw exception if identifier is needed
                    if (required)
                    {
                        throw new IDNotTranslatedException(columnName, codeName, objectType, id);
                    }
                }
            }
        }


        /// <summary>
        /// Handles FK translations
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="obj">Object (TreeNode / Info object) to process</param>
        /// <param name="columnName">Column name of the dependency</param>
        /// <param name="objectType">Object type of the dependency</param>
        /// <param name="required">Determines whether the dependency is required (reflects required flag from TypeInfo).</param>
        private void ProcessIDForExternalObject(SynchronizationObjectSettings settings, ICMSObject obj, string columnName, string objectType, bool required)
        {
            // Translate only columns containing some value
            int id = ValidationHelper.GetInteger(obj.GetValue(columnName), 0);
            if (id > 0)
            {
                // Get translation helper
                TranslationHelper th = settings.TranslationHelper ?? (settings.TranslationHelper = new TranslationHelper());

                // Try to get translation from hashtable
                string translation = GetExternalTranslation(objectType, id);
                string codeName = null;
                string siteName = null;
                int parentId = 0;
                int groupId = 0;
                string info = null;
                bool addToHashtable = false;
                bool addToTranslationHelper = false;

                // If translation does not exist in hashtable
                if (string.IsNullOrEmpty(translation))
                {
                    // Try to get record from translation helper
                    DataRow record = th.GetRecord(objectType, id);
                    if (record == null)
                    {
                        if (DocumentHelper.IsDocumentObjectType(objectType) || (objectType == PredefinedObjectType.NODE))
                        {
                            Guid nodeGuid = Guid.Empty;
                            string cultureCode = null;
                            // Load parameters for translation helper
                            GetInternalDocumentParams(id, objectType, out nodeGuid, out cultureCode, out siteName);
                            // Set code name
                            codeName = nodeGuid + ";" + cultureCode;
                        }
                        else
                        {
                            // Load parameters for translation helper
                            GetInternalObjectParams(id, objectType, out codeName, out siteName, ref parentId, ref groupId);

                            // Process parent ID
                            if (parentId > 0)
                            {
                                GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);
                                ProcessIDForExternalObject(settings, parentId, infoObj.ParentObjectType);
                            }

                            // Process group ID
                            if (groupId > 0)
                            {
                                ProcessIDForExternalObject(settings, groupId, PredefinedObjectType.GROUP);
                            }
                        }
                        addToTranslationHelper = true;
                    }
                    else
                    {
                        // Add existing translation to hashtable
                        codeName = ValidationHelper.GetString(record["CodeName"], null);
                        siteName = ValidationHelper.GetString(record["SiteName"], null);
                        parentId = ValidationHelper.GetInteger(record["ParentID"], 0);
                        groupId = ValidationHelper.GetInteger(record["GroupID"], 0);
                    }
                    addToHashtable = true;
                }
                if (addToHashtable)
                {
                    // Add translation to hashtable
                    SetExternalTranslation(objectType, id, codeName + "|" + siteName + "|" + parentId + "|" + groupId);
                }
                if (addToTranslationHelper)
                {
                    if (obj is GeneralizedInfo)
                    {
                        GeneralizedInfo infoObj = obj as GeneralizedInfo;

                        var ti = infoObj.TypeInfo;
                        if (ti.OriginalObjectType == PredefinedObjectType.PERMISSION)
                        {
                            info = ti.ParentIDColumn;
                        }
                    }
                    // Create translation record
                    var parameters = new TranslationParameters(objectType)
                    {
                        CodeName = codeName,
                        SiteName = siteName,
                        ParentId = parentId,
                        GroupId = groupId
                    };
                    th.RegisterRecord(id, parameters, info);
                }
            }
        }


        /// <summary>
        /// Handles FK translations
        /// </summary>
        /// <param name="settings">Export settings object</param>
        /// <param name="objectId">Object ID of the dependency</param>
        /// <param name="objectType">Object type of the dependency</param>
        private void ProcessIDForExternalObject(SynchronizationObjectSettings settings, int objectId, string objectType)
        {
            if (objectId > 0)
            {
                // Get translation helper
                TranslationHelper th = settings.TranslationHelper;

                bool addToTranslationHelper = false;
                bool addToHashtable = false;

                string translation = GetExternalTranslation(objectType, objectId);
                string codeName = null;
                string siteName = null;
                int parentId = 0;
                int groupId = 0;

                if (string.IsNullOrEmpty(translation))
                {
                    // Try to get record
                    DataRow record = th.GetRecord(objectType, objectId);

                    if (record == null)
                    {
                        if (DocumentHelper.IsDocumentObjectType(objectType) || (objectType == PredefinedObjectType.NODE))
                        {
                            Guid nodeGuid = Guid.Empty;
                            string cultureCode = null;
                            // Load parameters for translation helper
                            GetInternalDocumentParams(objectId, objectType, out nodeGuid, out cultureCode, out siteName);
                            // Set code name
                            codeName = nodeGuid + ";" + cultureCode;
                        }
                        else
                        {
                            // Load parameters for translation helper
                            GetInternalObjectParams(objectId, objectType, out codeName, out siteName, ref parentId, ref groupId);
                        }

                        addToTranslationHelper = true;
                    }
                    else
                    {
                        // Add existing translation to hashtable
                        codeName = ValidationHelper.GetString(record["CodeName"], null);
                        siteName = ValidationHelper.GetString(record["SiteName"], null);
                        parentId = ValidationHelper.GetInteger(record["ParentID"], 0);
                        groupId = ValidationHelper.GetInteger(record["GroupID"], 0);
                    }
                    addToHashtable = true;
                }

                if (addToHashtable)
                {
                    // Add translation to hashtable
                    SetExternalTranslation(objectType, objectId, codeName + "|" + siteName + "|" + parentId + "|" + groupId);
                }

                if (addToTranslationHelper)
                {
                    // Create translation record
                    th.RegisterRecord(objectId, new TranslationParameters(objectType) { CodeName = codeName, SiteName = siteName, ParentId = parentId, GroupId = groupId });
                }
            }
        }


        /// <summary>
        /// Gets message customized for NotImplementedException (for internal purpopses only).
        /// </summary>
        /// <param name="incoming">Direction (true for external tasks, false for internal tasks)</param>
        private string GetNotImplementedException(bool incoming)
        {
            // Get call stack
            StackTrace stackTrace = new StackTrace();

            // Get calling method (the one that needs to be implemented)
            MethodBase trace = stackTrace.GetFrame(1).GetMethod();

            // Create message
            string message = "[" + trace.DeclaringType.Name + "." + trace.Name + "]: Method needs to be implemented in order to support ";
            message += incoming ? "external" : "internal";
            message += " tasks.";

            return message;
        }

        #endregion


        #region "Translation hashtable operations"


        #region "Internal translations"

        /// <summary>
        /// Sets translation for class name and identifier.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="oldId">Old identifier</param>
        /// <param name="newId">New identifier</param>
        protected void SetInternalTranslation(string className, int oldId, int newId)
        {
            if (className != null)
            {
                className = TranslationHelper.GetSafeClassName(className).ToLowerCSafe();
                if (internalTranslations == null)
                {
                    internalTranslations = new Hashtable();
                }
                if (!internalTranslations.ContainsKey(className))
                {
                    internalTranslations[className] = new Hashtable();
                }
                Hashtable classTable = internalTranslations[className] as Hashtable;
                if (classTable != null)
                {
                    classTable[oldId] = newId;
                }
            }
        }


        /// <summary>
        /// Gets translation for class name and identifier.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="oldId">Old identifier</param>
        /// <returns>New identifier if present</returns>
        protected int? GetInternalTranslation(string className, int oldId)
        {
            if (className != null)
            {
                className = TranslationHelper.GetSafeClassName(className).ToLowerCSafe();
                if (internalTranslations != null)
                {
                    if (internalTranslations.ContainsKey(className))
                    {
                        Hashtable classTable = internalTranslations[className] as Hashtable;
                        if ((classTable != null) && classTable.ContainsKey(oldId))
                        {
                            return ((int)classTable[oldId]);
                        }
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Clears hashtable with internal translations
        /// </summary>
        protected void ClearInternalTranslations()
        {
            if (internalTranslations != null)
            {
                internalTranslations.Clear();
            }
        }

        #endregion


        #region "External translations"

        /// <summary>
        /// Sets translation for class name and identifier.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="id">External identifier</param>
        /// <param name="translationRecord">Translation record ("codename|sitename|parentid")</param>
        protected void SetExternalTranslation(string className, int id, string translationRecord)
        {
            if (className != null)
            {
                className = TranslationHelper.GetSafeClassName(className).ToLowerCSafe();
                if (externalTranslations == null)
                {
                    externalTranslations = new Hashtable();
                }
                if (!externalTranslations.ContainsKey(className))
                {
                    externalTranslations[className] = new Hashtable();
                }
                Hashtable classTable = externalTranslations[className] as Hashtable;
                if (classTable != null)
                {
                    classTable[id] = translationRecord;
                }
            }
        }


        /// <summary>
        /// Gets translation for class name and identifier.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <param name="id">External identifier</param>
        /// <returns>Translation record ("codename|sitename|parentid|groupid")</returns>
        protected string GetExternalTranslation(string className, int id)
        {
            if (className != null)
            {
                className = TranslationHelper.GetSafeClassName(className).ToLowerCSafe();
                if (externalTranslations != null)
                {
                    if (externalTranslations.ContainsKey(className))
                    {
                        Hashtable classTable = externalTranslations[className] as Hashtable;
                        if ((classTable != null) && classTable.ContainsKey(id))
                        {
                            return ((string)classTable[id]);
                        }
                    }
                }
            }
            return null;
        }


        /// <summary>
        /// Clears hashtable with external translations
        /// </summary>
        protected void ClearExternalTranslations()
        {
            if (externalTranslations != null)
            {
                externalTranslations.Clear();
            }
        }

        #endregion


        #endregion


        #region "Virtual methods that need to be implemented"


        #region "Internal (outcoming) tasks"

        /// <summary>
        /// Suitable for implementation of asynchronous outcoming object processing. Identifiers of object are already prepared to match external application.
        /// </summary>
        /// <param name="infoObj">Object to process</param>
        /// <param name="translations">Translation helper object containing translations for given object</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="siteName">Name of site</param>
        /// <param name="errorMessage">Possible error message</param>
        /// <returns>Processing result</returns>
        public virtual IntegrationProcessResultEnum ProcessInternalTaskAsync(GeneralizedInfo infoObj, TranslationHelper translations, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName, out string errorMessage)
        {
            throw new NotImplementedException(GetNotImplementedException(false));
        }


        /// <summary>
        /// Suitable for implementation of asynchronous outcoming document processing. Identifiers of document are already prepared to match external application.
        /// </summary>
        /// <param name="node">Document to process</param>
        /// <param name="translations">Translation helper object containing translations for given document</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of data</param>
        /// <param name="siteName">Name of site</param>
        /// <param name="errorMessage">Possible error message</param>
        /// <returns>Processing result</returns>
        public virtual IntegrationProcessResultEnum ProcessInternalTaskAsync(TreeNode node, TranslationHelper translations, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName, out string errorMessage)
        {
            throw new NotImplementedException(GetNotImplementedException(false));
        }


        /// <summary>
        /// Suitable for implementation of synchronous outcoming object processing. Identifiers of object are in their original state.
        /// </summary>
        /// <param name="infoObj">Object to process</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="siteName">Name of site</param>
        /// <param name="errorMessage">Possible error message</param>
        /// <returns>Processing result</returns>
        public virtual IntegrationProcessResultEnum ProcessInternalTaskSync(GeneralizedInfo infoObj, TaskTypeEnum taskType, string siteName, out string errorMessage)
        {
            throw new NotImplementedException(GetNotImplementedException(false));
        }


        /// <summary>
        /// Suitable for implementation of synchronous outcoming document processing. Identifiers of object are in their original state.
        /// </summary>
        /// <param name="node">Document to process</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="errorMessage">Possible error message</param>
        /// <param name="siteName">Name of site</param>
        /// <returns>Processing result</returns>
        public virtual IntegrationProcessResultEnum ProcessInternalTaskSync(TreeNode node, TaskTypeEnum taskType, string siteName, out string errorMessage)
        {
            throw new NotImplementedException(GetNotImplementedException(false));
        }


        /// <summary>
        /// Based on parameters this method will find out identifier of the object matching external application.
        /// </summary>
        /// <param name="objectType">Type of object</param>
        /// <param name="codeName">Code name of object</param>
        /// <param name="siteName">Site name of object</param>
        /// <param name="parentType">Type of parent object</param>
        /// <param name="parentId">Parent object identifier</param>
        /// <param name="groupId">Group identifier</param>
        /// <returns>Object identifier for external usage</returns>
        public virtual int GetExternalObjectID(string objectType, string codeName, string siteName, string parentType, int parentId, int groupId)
        {
            throw new NotImplementedException(GetNotImplementedException(false));
        }


        /// <summary>
        /// Based on parameters this method will find out identifier of the document matching external application.
        /// </summary>
        /// <param name="nodeGuid">Document unique identifier</param>
        /// <param name="cultureCode">Document culture code</param>
        /// <param name="siteName">Document site name</param>
        /// <param name="returnDocumentId">Whether to return document or node identifier</param>
        /// <returns>Document identifier for external usage</returns>
        public virtual int GetExternalDocumentID(Guid nodeGuid, string cultureCode, string siteName, bool returnDocumentId)
        {
            throw new NotImplementedException(GetNotImplementedException(false));
        }

        #endregion


        #region "External (incoming) tasks"

        /// <summary>
        /// By supplying object type and identifier the method ensures filling output parameters needed for correct translation between external and internal object.
        /// </summary>
        /// <param name="id">Identifier of the object</param>
        /// <param name="objectType">Type of the object</param>
        /// <param name="codeName">Returns code name</param>
        /// <param name="siteName">Returns site name</param>
        /// <param name="parentId">Returns identifier of parent object</param>
        /// <param name="groupId">Returns identifier of object community group</param>
        public virtual void GetInternalObjectParams(int id, string objectType, out string codeName, out string siteName, ref int parentId, ref int groupId)
        {
            throw new NotImplementedException(GetNotImplementedException(true));
        }


        /// <summary>
        /// By supplying document identifier and class name the method ensures filling output parameters needed for correct translation between external and internal document.
        /// </summary>
        /// <param name="id">Identifier of the document</param>
        /// <param name="className">Class name of the document</param>
        /// <param name="nodeGuid">Returns document unique identifier</param>
        /// <param name="cultureCode">Returns culture code</param>
        /// <param name="siteName">Returns site name</param>
        public virtual void GetInternalDocumentParams(int id, string className, out Guid nodeGuid, out string cultureCode, out string siteName)
        {
            throw new NotImplementedException(GetNotImplementedException(true));
        }


        /// <summary>
        /// Transforms given external object to internal (to TreeNode or GeneralizedInfo).
        /// </summary>
        /// <param name="obj">Object or document to transform</param>
        /// <param name="taskType">Type of task</param>
        /// <param name="dataType">Type of input data</param>
        /// <param name="siteName">Name of site</param>
        public virtual ICMSObject PrepareInternalObject(object obj, TaskTypeEnum taskType, TaskDataTypeEnum dataType, string siteName)
        {
            throw new NotImplementedException(GetNotImplementedException(true));
        }

        #endregion


        #endregion
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml;

using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.LicenseProvider;

namespace CMS.Synchronization
{
    /// <summary>
    /// Class providing management for the synchronization.
    /// </summary>
    public static class SynchronizationHelper
    {
        #region "Variables"

        // Set of setting key names which will be excluded from synchronization
        private static HashSet<string> mExcludedSettingKeyNames;

        #endregion


        #region "Properties"

        /// <summary>
        /// Indicates whether object locking is enabled within the system.
        /// </summary>
        public static bool UseCheckinCheckout
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSUseObjectCheckinCheckout") && LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ObjectVersioning);
            }
        }
        
        #endregion


        #region "Public methods"

        #region "Excluded settings methods"

        /// <summary>
        /// Returns set of excluded setting key names. Can return null if there are no excluded keys.
        /// </summary>
        public static HashSet<string> GetExcludedSettingKeys()
        {
            return mExcludedSettingKeyNames;
        }


        /// <summary>
        /// Adds given setting key to the excluded keys. For excluded keys synchronization tasks are not logged.
        /// </summary>
        /// <param name="names">Names of the settings key</param>
        public static void AddExcludedSettingKey(params string[] names)
        {
            if (names == null)
            {
                return;
            }

            if (mExcludedSettingKeyNames == null)
            {
                mExcludedSettingKeyNames = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            }

            foreach (var name in names)
            {
                mExcludedSettingKeyNames.Add(name);
            }
        }


        /// <summary>
        /// Removes given setting key from the excluded keys. For excluded keys synchronization tasks are not logged.
        /// </summary>
        /// <param name="names">Names of the settings key</param>
        public static void RemoveExcludedSettingKey(params string[] names)
        {
            if ((mExcludedSettingKeyNames == null) || (names == null))
            {
                return;
            }

            foreach (var name in names)
            {
                mExcludedSettingKeyNames.Remove(name);
            }
        }


        /// <summary>
        /// Determines whether the given setting key is excluded from synchronization.
        /// </summary>
        /// <param name="name">Name of the settings key</param>
        public static bool IsSettingKeyExcluded(string name)
        {
            if (mExcludedSettingKeyNames == null)
            {
                return false;
            }

            return mExcludedSettingKeyNames.Contains(name);
        }

        #endregion

        /// <summary>
        /// Returns true if the object is checked out by other user than a current user (and use checkin/out is used).
        /// </summary>
        public static bool IsCheckedOutByOtherUser(BaseInfo info)
        {
            if (info == null)
            {
                return false;
            }

            if (!UseCheckinCheckout || !info.TypeInfo.SupportsLocking || info.Generalized.IsCheckedOutByUser(CMSActionContext.CurrentUser))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Gets XML with data for specified object.
        /// </summary>
        /// <param name="settings">Object export settings (some settings are internally set according to task type)</param>
        /// <param name="infoObj">Object to get data for</param>
        /// <param name="taskType">Task type</param>
        public static string GetObjectXml(SynchronizationObjectSettings settings, GeneralizedInfo infoObj, TaskTypeEnum taskType)
        {
            // Check object instance
            if (infoObj == null)
            {
                throw new ArgumentException("Missing object instance.", nameof(infoObj));
            }

            if (settings == null)
            {
                settings = new SynchronizationObjectSettings();
                settings.Operation = OperationTypeEnum.Synchronization;
            }

            // Prepare where condition for the object
            string whereCondition;

            var ti = infoObj.TypeInfo;
            if (ti.IsBinding)
            {
                // Change task type and object for site binding
                int siteId = 0;
                if (ChangeSiteBindingObject(ref infoObj, ref siteId, ref taskType))
                {
                    whereCondition = ti.IDColumn + " = " + infoObj.ObjectID;
                }
                else
                {
                    // Binding
                    whereCondition = ti.ParentIDColumn + " = " + infoObj.ObjectParentID;

                    // ### Special case - User culture ###
                    if (ti.ObjectType == PredefinedObjectType.USERCULTURE)
                    {
                        whereCondition = SqlHelper.AddWhereCondition(whereCondition, "CultureID = " + infoObj.GetValue("CultureID"));
                    }
                }
            }
            else
            {
                // Standard object
                whereCondition = ti.IDColumn + " = " + infoObj.ObjectID;
            }

            settings.WhereCondition.Where(whereCondition);

            // Set up additional settings
            switch (taskType)
            {
                // Object update - full DataSet
                case TaskTypeEnum.UpdateObject:
                case TaskTypeEnum.CreateObject:
                case TaskTypeEnum.AddToSite:
                    {
                        // Include binary data
                        settings.Binary = true;
                    }
                    break;

                // Object deletion - only the main deleted object
                case TaskTypeEnum.DeleteObject:
                case TaskTypeEnum.RemoveFromSite:
                    {
                        // Do not include binary data, child objects nor categories
                        settings.IncludeChildren = false;
                        settings.Binary = false;
                        settings.IncludeCategories = false;
                    }
                    break;

                default:
                    throw new InvalidOperationException("Unknown task type '" + taskType + "'.");
            }

            return ObjectXmlHelper.GetObjectXml(settings, infoObj);
        }


        /// <summary>
        /// Gets object key for specified task type.
        /// </summary>
        /// <param name="operationType">Operation type</param>
        /// <param name="infoObj">Info object instance</param>
        /// <param name="taskType">Task type</param>
        public static string GetObjectKey(OperationTypeEnum operationType, GeneralizedInfo infoObj, TaskTypeEnum taskType)
        {
            if (infoObj == null)
            {
                throw new Exception("[SynchronizationHelper.GetObjectKey]: Missing object instance.");
            }

            return TaskHelper.GetTaskTypeString(taskType) + "_" + operationType + "_" + infoObj.TypeInfo.ObjectType + "_" + infoObj.ObjectID;
        }


        /// <summary>
        /// Gets the DataSet of the objects data and their child objects.
        /// </summary>
        /// <param name="operationType">Operation type</param>
        /// <param name="infoObj">Main info object</param>
        /// <param name="whereCondition">Where condition</param>
        /// <param name="orderBy">Order by columns for the main objects</param>
        /// <param name="childData">If true, child objects data are included</param>
        /// <param name="binaryData">If true, binary data are included</param>
        /// <param name="th">Translation table to initialize with data bindings</param>
        public static DataSet GetObjectsData(OperationTypeEnum operationType, BaseInfo infoObj, string whereCondition, string orderBy, bool childData, bool binaryData, TranslationHelper th)
        {
            // Get the data by external function
            return ObjectHelper.GetObjectsData(operationType, infoObj, whereCondition, orderBy, childData, binaryData, th);
        }


        /// <summary>
        /// Gets the object data for synchronization.
        /// </summary>
        /// <param name="operationType">Operation type</param>
        /// <param name="infoObj">Main info object</param>
        /// <param name="childData">If true, child objects data are included</param>
        /// <param name="binaryData">If true, binary data are included</param>
        /// <param name="th">Translation table to initialize with data bindings</param>
        public static DataSet GetObjectData(OperationTypeEnum operationType, GeneralizedInfo infoObj, bool childData, bool binaryData, TranslationHelper th)
        {
            if (infoObj == null)
            {
                return null;
            }

            return GetObjectsData(operationType, infoObj, infoObj.TypeInfo.IDColumn + " = " + infoObj.ObjectID, null, childData, binaryData, th);
        }


        /// <summary>
        /// Gets binary XML data for given object.
        /// </summary>
        /// <param name="operationType">Operation type</param>
        /// <param name="objectType">Object type</param>
        /// <param name="objectId">Object ID</param>
        /// <param name="taskType">Task type</param>
        public static string GetObjectBinaryXml(OperationTypeEnum operationType, string objectType, int objectId, TaskTypeEnum taskType)
        {
            string binaryXml = null;

            var typeInfo = ObjectTypeManager.GetTypeInfo(objectType);
            if (typeInfo != null)
            {
                // Do not process data tasks
                if (!typeInfo.IsDataObjectType)
                {
                    switch (objectType)
                    {
                        case PredefinedObjectType.MEDIAFILE:
                            // Synchronize physical files within update or create task
                            if ((taskType == TaskTypeEnum.UpdateObject) || (taskType == TaskTypeEnum.CreateObject))
                            {
                                // Get the info object
                                GeneralizedInfo infoObj = GetObject(null, objectType);
                                infoObj = infoObj.GetObject(objectId);

                                if (infoObj != null)
                                {
                                    // Get the list of physical files
                                    DataSet dsData = infoObj.GetPhysicalFiles(operationType, false);
                                    if (!DataHelper.DataSourceIsEmpty(dsData))
                                    {
                                        // Writer setting
                                        XmlWriterSettings ws = new XmlWriterSettings();
                                        ws.CloseOutput = true;
                                        ws.Indent = true;
                                        ws.OmitXmlDeclaration = true;
                                        ws.CheckCharacters = false;

                                        // Open writer
                                        StringBuilder sb = new StringBuilder();
                                        XmlWriter xml = XmlWriter.Create(sb, ws);

                                        // Write data
                                        DataHelper.WriteDataSetToXml(dsData, xml, "FileBinaryData", null, null);

                                        xml.Flush();
                                        xml.Close();

                                        // Get the binary data
                                        binaryXml = sb.ToString();
                                    }
                                }
                            }
                            break;
                    }
                }
            }

            return binaryXml;
        }


        /// <summary>
        /// Gets the where condition for specified type of object.
        /// </summary>
        /// <param name="objectType">Object type</param>
        /// <param name="siteId">Site ID for the site objects</param>
        public static WhereCondition GetObjectWhereCondition(string objectType, int siteId)
        {
            var whereCondition = new WhereCondition();

            // Site ID where condition
            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(objectType);

            var ti = infoObj.TypeInfo;
            if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if (siteId > 0)
                {
                    whereCondition.Where(ti.SiteIDColumn, QueryOperator.Equals, siteId);
                }
                else
                {
                    whereCondition.WhereNull(ti.SiteIDColumn);
                }
            }

            if (!string.IsNullOrEmpty(ti.WhereCondition))
            {
                whereCondition.Where(ti.WhereCondition);
            }

            return whereCondition;
        }


        /// <summary>
        /// Logs object change.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Type of the task</param>
        public static void LogObjectChange(BaseInfo infoObj, TaskTypeEnum taskType)
        {
            LogObjectChange(infoObj, taskType, true);
        }


        /// <summary>
        /// Logs object change.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Type of the task</param>
        /// <param name="runAsync">If true, the change log should run asynchronously</param>
        public static void LogObjectChange(BaseInfo infoObj, TaskTypeEnum taskType, bool runAsync)
        {
            var settings = new LogObjectChangeSettings(infoObj, taskType)
            {
                RunAsynchronously = runAsync
            };

            LogObjectChange(settings);
        }


        /// <summary>
        /// Logs object change for different type of synchronization.
        /// </summary>
        /// <param name="settings">Log object change settings</param>
        /// <returns>List of synchronization tasks</returns>
        public static List<ISynchronizationTask> LogObjectChange(LogObjectChangeSettings settings)
        {
            int siteId = settings.SiteID;
            TaskTypeEnum taskType = settings.TaskType;
            GeneralizedInfo infoObj = settings.InfoObj;

            // Check object instance
            if (infoObj == null)
            {
                throw new Exception("[SynchronizationHelper.LogObjectChange]: Missing object instance.");
            }

            var tasks = new List<ISynchronizationTask>();

            var ti = infoObj.TypeInfo;

            // Do not log any actions if not necessary
            if (CMSActionContext.LogObjectChange())
            {
                // Handle excluded settings keys
                if (ti.ObjectType == SettingsKeyInfo.OBJECT_TYPE)
                {
                    if (IsSettingKeyExcluded(infoObj.ObjectCodeName))
                    {
                        return tasks;
                    }
                }

                // Handle object type condition for synchronization
                var condition = ti.SynchronizationSettings.LogCondition;
                if ((condition != null) && !condition(infoObj))
                {
                    return tasks;
                }

                // Change task type and object for site binding
                ChangeSiteBindingObject(ref infoObj, ref siteId, ref taskType);

                // Handle the event
                settings.SiteID = siteId;
                settings.TaskType = taskType;
                settings.InfoObj = infoObj;

                // Handle the event
                using (var h = ti.Events.LogChange.StartEvent(settings))
                {
                    if (h.CanContinue())
                    {
                        // Check if the object change should be logged
                        if (SynchronizationActionManager.CanExecuteActions(settings))
                        {
                            // Method is not called from worker
                            if (!settings.WorkerCall)
                            {
                                // Check if asynchronous run
                                settings.RunAsynchronously &= (CMSActionContext.CurrentAllowAsyncActions && !TaskHelper.IsExcludedAsyncTask(taskType)) && !infoObj.IsCheckedOut;

                                if (settings.LogIntegration)
                                {
                                    using (var ctx = new CMSActionContext())
                                    {
                                        ctx.LogIntegration = false;

                                        // Get matching connectors
                                        var connectors = IntegrationHelper.GetMatchingConnectors(infoObj, taskType);

                                        // Touch asynchronous connectors
                                        settings.LogIntegration &= IntegrationHelper.TouchAsyncConnectors(connectors);

                                        // Log simple tasks only if object changed
                                        settings.LogIntegrationSimpleTasks &= (taskType != TaskTypeEnum.UpdateObject) || settings.DataChanged;

                                        // Process sync connectors
                                        IntegrationHelper.ProcessSyncTasks(infoObj, taskType, siteId, connectors);
                                    }
                                }

                                // Logging will be processed, create clone to ensure fresh data
                                infoObj = infoObj.Clone();

                                settings.InfoObj = infoObj;
                            }

                            settings.InitUserAndTaskGroups();

                            if (settings.RunAsynchronously)
                            {
                                if (settings.CreateVersion)
                                {
                                    // Mark a task created for create version action
                                    RequestStockHelper.AddToStorage(ObjectVersionManager.CREATE_VERSION_STORAGE, settings.InfoObj.GetObjectKey(), true);
                                }

                                SynchronizationQueueWorker.Current.Enqueue(settings.GetDuplicityKey("SynchronizationWorker"), () =>
                                {
                                    // Check if the object is available
                                    if (settings.InfoObj == null)
                                    {
                                        throw new Exception("[SynchronizationWorker]: Object is not specified.");
                                    }

                                    // Log the synchronization
                                    settings.RunAsynchronously = false;
                                    settings.WorkerCall = true;

                                    LogObjectChange(settings);
                                });
                            }
                            else
                            {
                                ObjectXmlHelper.ExecuteWithEmptyXMLCacheForObject(infoObj, () => {
                                    // Execute all registered synchronization actions
                                    tasks = SynchronizationActionManager.ExecuteActions(settings);
                                    LogTasksWithUserAndTaskGroups(tasks, settings.TaskGroups, settings.User);
                                });
                            }
                        }
                    }

                    // Finish the event
                    h.FinishEvent();
                }
            }

            return tasks;
        }


        /// <summary>
        /// Logs <see cref="StagingTaskInfo"/>s with <see cref="IUserInfo"/> and <see cref="TaskGroupInfo"/>s.
        /// </summary>
        /// <param name="tasks">Staging tasks to log</param>
        /// <param name="taskGroups">Task groups in which staging tasks belong</param>
        /// <param name="user">User to log staging tasks with</param>
        public static void LogTasksWithUserAndTaskGroups(IEnumerable<ISynchronizationTask> tasks, IEnumerable<IStagingTaskGroup> taskGroups, IUserInfo user)
        {
            var taskGroupsArray = taskGroups.ToArray();

            // If synchronization task is not staging task don't log user or task group
            foreach (var task in tasks.OfType<StagingTaskInfo>().Where(i => i.Generalized.GetExisting() != null))
            {
                if (user != null)
                {
                    // Log user to staging task
                    StagingTaskUserInfoProvider.AddStagingTaskToUser(task.TaskID, user.UserID);
                }

                foreach (var taskGroup in taskGroupsArray)
                {
                    // If task group does not exists (task from source env. is staged to target env.), create it
                    if (taskGroup.TaskGroupID == 0)
                    {
                        var newTaskGroup = new TaskGroupInfo
                        {
                            TaskGroupCodeName = taskGroup.TaskGroupCodeName,
                            TaskGroupDescription = taskGroup.TaskGroupDescription,
                            TaskGroupGuid = taskGroup.TaskGroupGuid,
                        };
                        TaskGroupInfoProvider.SetTaskGroupInfo(newTaskGroup);
                        taskGroup.TaskGroupID = newTaskGroup.TaskGroupID;
                    }

                    var taskGroupTask = new TaskGroupTaskInfo
                    {
                        TaskGroupID = taskGroup.TaskGroupID,
                        TaskID = task.TaskID
                    };
                    TaskGroupTaskInfoProvider.SetTaskGroupTask(taskGroupTask);
                }
            }
        }

        /// <summary>
        /// Logs the synchronization for specified group of objects.
        /// </summary>
        /// <param name="objectTypes">Object types (list of object type constants separated by semicolon)</param>
        /// <param name="objectsSiteId">Objects site ID</param>
        /// <param name="modifiedFrom">Time from which the objects were modified</param>
        /// <param name="taskType">Task type</param>
        /// <param name="logStaging">Indicates if the staging task should be logged</param>
        /// <param name="logIntegration">Indicates if the integration task should be logged</param>
        /// <param name="logExport">Indicates if the export task should be logged</param>
        /// <param name="createVersion">Indicates if the version should be created</param>
        /// <param name="runAsync">Indicates if the logging should be asynchronous</param>
        /// <param name="siteId">Site ID for synchronization</param>
        /// <param name="serverId">Server ID for synchronization</param>
        public static List<ISynchronizationTask> LogObjectChange(string objectTypes, int objectsSiteId, DateTime modifiedFrom, TaskTypeEnum taskType, bool logStaging, bool logIntegration, bool logExport, bool createVersion, bool runAsync, int siteId, int serverId)
        {
            List<ISynchronizationTask> tasks = new List<ISynchronizationTask>();

            // Process all types
            var types = TypeHelper.GetTypes(objectTypes);
            foreach (string type in types)
            {
                GeneralizedInfo infoObj = ModuleManager.GetObject(type);
                if (infoObj != null)
                {
                    // Filter Site ID
                    var whereCondition = GetObjectWhereCondition(type, objectsSiteId);

                    var ti = infoObj.TypeInfo;

                    // ### Special case - Synchronize only current site, not all
                    if ((ti.ObjectType == PredefinedObjectType.SITE) && (objectsSiteId > 0))
                    {
                        whereCondition.Where("SiteID", QueryOperator.Equals, objectsSiteId);
                    }

                    // Get the objects
                    var q = infoObj.GetModifiedFrom(modifiedFrom, s => s
                        .Where(whereCondition)
                    );

                    var ds = q.Result;
                    if (!DataHelper.DataSourceIsEmpty(ds))
                    {
                        List<int> assigned = null;

                        // Get objects assigned to the current site
                        GeneralizedInfo siteBinding = ti.SiteBindingObject;
                        if (siteBinding != null)
                        {
                            var siteTypeInfo = siteBinding.TypeInfo;
                            DataSet dsObjs = siteBinding.GetData(null, siteTypeInfo.SiteIDColumn + "=" + siteId, null, 0, siteTypeInfo.ParentIDColumn, false);
                            if (!DataHelper.DataSourceIsEmpty(dsObjs))
                            {
                                assigned = new List<int>();
                                foreach (DataRow drObj in dsObjs.Tables[0].Rows)
                                {
                                    assigned.Add(ValidationHelper.GetInteger(drObj[0], 0));
                                }
                            }
                        }

                        // Process all objects
                        foreach (DataRow dr in ds.Tables[0].Rows)
                        {
                            // Log object synchronization
                            infoObj = ModuleManager.GetObject(dr, type);

                            TaskTypeEnum objTaskType = taskType;
                            // Log add to site task if there is site binding
                            if ((assigned != null) && (objTaskType == TaskTypeEnum.UpdateObject))
                            {
                                objTaskType = (assigned.Contains(infoObj.ObjectID)) ? TaskTypeEnum.AddToSite : objTaskType;
                            }

                            // Log task
                            var settings = new LogObjectChangeSettings(infoObj, objTaskType)
                            {
                                LogStaging = logStaging,
                                LogIntegration = logIntegration,
                                LogIntegrationSimpleTasks = logIntegration,
                                LogExportTask = logExport,
                                CreateVersion = createVersion,
                                RunAsynchronously = runAsync,
                                SiteID = siteId,
                                ServerID = serverId,
                                IsTouchParent = false,
                                WorkerCall = false
                            };

                            var syncTasks = LogObjectChange(settings);

                            tasks.AddRange(syncTasks);
                        }
                    }
                }
            }

            return tasks;
        }


        /// <summary>
        /// Changes site binding object to the parent object.
        /// </summary>
        /// <param name="infoObj">Info object instance</param>
        /// <param name="siteId">Original site ID</param>
        /// <param name="taskType">Original task type</param>
        /// <returns>Returns TRUE if site binding object was changed.</returns>
        public static bool ChangeSiteBindingObject(ref GeneralizedInfo infoObj, ref int siteId, ref TaskTypeEnum taskType)
        {
            bool changed = false;
            if (infoObj.TypeInfo.IsSiteBinding && !infoObj.TypeInfo.IsMultipleBinding)
            {
                switch (taskType)
                {
                    case TaskTypeEnum.CreateObject:
                    case TaskTypeEnum.UpdateObject:
                        siteId = infoObj.ObjectSiteID;

                        // Log add to site only for site bindings without attributes
                        if ((infoObj.TypeInfo.IDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN) || (taskType == TaskTypeEnum.CreateObject))
                        {
                            taskType = TaskTypeEnum.AddToSite;
                            changed = true;
                        }
                        break;

                    case TaskTypeEnum.DeleteObject:
                        siteId = infoObj.ObjectSiteID;
                        taskType = TaskTypeEnum.RemoveFromSite;
                        changed = true;
                        break;
                }

                // Change object to parent
                if (changed)
                {
                    infoObj = infoObj.GetParent();
                }
            }

            return changed;
        }

        #endregion


        #region "Synchronization tasks methods"

        /// <summary>
        /// Logs the object deletion.
        /// </summary>
        /// <param name="infoObj">Info object to log</param>
        public static void LogObjectDelete(BaseInfo infoObj)
        {
            // Log the object deletion
            EventLogHelper.LogDelete(infoObj);

            // Log synchronization
            LogObjectChange(infoObj, TaskTypeEnum.DeleteObject);
        }


        /// <summary>
        /// Touch the parent.
        /// </summary>
        /// <param name="infoObj">Info object to log</param>
        /// <param name="taskType">Type of the task</param>
        public static void TouchParent(BaseInfo infoObj, TaskTypeEnum taskType)
        {
            if (infoObj.Generalized.AllowTouchParent)
            {
                using (CMSActionContext context = new CMSActionContext())
                {
                    context.LogSynchronization = (infoObj.Generalized.LogSynchronization == SynchronizationTypeEnum.TouchParent);

                    // Touch parent
                    infoObj.Generalized.TouchParent();
                }
            }
        }


        /// <summary>
        /// Touch the parent when inserting an object.
        /// </summary>
        /// <param name="infoObj">Info object to log</param>
        public static void TouchParentInsert(BaseInfo infoObj)
        {
            // Create object version
            LogObjectChange(GetTouchParentSettings(infoObj, TaskTypeEnum.CreateObject));

            // Touch parent
            TouchParent(infoObj, TaskTypeEnum.CreateObject);

            // Log event
            EventLogHelper.LogInsert(infoObj);
        }


        /// <summary>
        /// Touch the parent when updating an object.
        /// </summary>
        /// <param name="infoObj">Info object to log</param>
        public static void TouchParentUpdate(BaseInfo infoObj)
        {
            // Create object version
            LogObjectChange(GetTouchParentSettings(infoObj, TaskTypeEnum.UpdateObject));

            // Touch parent
            TouchParent(infoObj, TaskTypeEnum.UpdateObject);

            // Log event
            EventLogHelper.LogUpdate(infoObj);
        }


        /// <summary>
        /// Touch the parent when deleting an object.
        /// </summary>
        /// <param name="infoObj">Info object to log</param>
        public static void TouchParentDelete(BaseInfo infoObj)
        {
            // Log event
            EventLogHelper.LogDelete(infoObj);

            // Touch parent
            TouchParent(infoObj, TaskTypeEnum.DeleteObject);

            // Create object version
            LogObjectChange(GetTouchParentSettings(infoObj, TaskTypeEnum.DeleteObject));
        }


        /// <summary>
        /// Gets the object change settings for touch parent actions
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="taskType">Task type</param>
        private static LogObjectChangeSettings GetTouchParentSettings(BaseInfo infoObj, TaskTypeEnum taskType)
        {
            return new LogObjectChangeSettings(infoObj, taskType)
            {
                LogStaging = false,
                LogExportTask = false
            };
        }


        /// <summary>
        /// Logs the object update.
        /// </summary>
        /// <param name="infoObj">Info object to log</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        /// <param name="addToSite">If true, object is being assigned to the site</param>
        public static void LogObjectUpdate(BaseInfo infoObj, bool runAsync = true, bool addToSite = false)
        {
            TaskTypeEnum taskType = TaskTypeEnum.UpdateObject;

            // Log add to site
            if (addToSite)
            {
                taskType = TaskTypeEnum.AddToSite;
            }

            // Log synchronization
            LogObjectChange(infoObj, taskType, runAsync);

            // Log the object update
            EventLogHelper.LogUpdate(infoObj);
        }


        /// <summary>
        /// Logs the object insert.
        /// </summary>
        /// <param name="infoObj">Info object to log</param>
        /// <param name="runAsync">Indicates if the logging should run asynchronously</param>
        /// <param name="addToSite">If true, object is being assigned to the site</param>
        public static void LogObjectInsert(BaseInfo infoObj, bool runAsync = true, bool addToSite = false)
        {
            TaskTypeEnum taskType = TaskTypeEnum.CreateObject;

            // Log add to site
            if (addToSite)
            {
                taskType = TaskTypeEnum.AddToSite;
            }

            // Log synchronization
            LogObjectChange(infoObj, taskType, runAsync);

            // Log the object update
            EventLogHelper.LogInsert(infoObj);
        }


        /// <summary>
        /// Indicates if the staging task should be logged.
        /// </summary>
        /// <param name="infoObj">Info object instance</param>
        public static bool CheckStagingLogging(GeneralizedInfo infoObj)
        {
            bool logChanges =
                infoObj.TypeInfo.IsDataObjectType ?
                StagingTaskInfoProvider.LogDataChanges() :
                StagingTaskInfoProvider.LogObjectChanges(infoObj.ObjectSiteName);

            return (infoObj.LogSynchronization == SynchronizationTypeEnum.LogSynchronization) && logChanges;
        }


        /// <summary>
        /// Indicates if the integration task should be logged.
        /// </summary>
        /// <param name="infoObj">Info object instance</param>
        public static bool CheckIntegrationLogging(GeneralizedInfo infoObj)
        {
            return infoObj.LogIntegration && IntegrationHelper.IntegrationLogInternal;
        }


        /// <summary>
        /// Indicates if the object version should be created.
        /// </summary>
        /// <param name="infoObj">Info object instance</param>
        /// <param name="taskType">Task type</param>
        public static bool CheckCreateVersion(GeneralizedInfo infoObj, TaskTypeEnum taskType)
        {
            // Check license
            if (!string.IsNullOrEmpty(RequestContext.CurrentDomain))
            {
                if (!LicenseKeyInfoProvider.IsFeatureAvailable(RequestContext.CurrentDomain, FeatureEnum.ObjectVersioning))
                {
                    return false;
                }
            }

            // Special case for object deletion
            switch (taskType)
            {
                case TaskTypeEnum.DeleteObject:
                    return ObjectVersionManager.AllowObjectRestore(infoObj);

                case TaskTypeEnum.CreateObject:
                case TaskTypeEnum.UpdateObject:
                    return ObjectVersionManager.AllowObjectVersioning(infoObj);
            }

            return false;
        }


        /// <summary>
        /// Ensure object version if not existing.
        /// </summary>
        /// <param name="infoObj">Object which version should be ensured</param>
        public static void EnsureObjectVersion(BaseInfo infoObj)
        {
            if (CheckCreateVersion(infoObj, TaskTypeEnum.UpdateObject))
            {
                using (CMSActionContext context = new CMSActionContext())
                {
                    context.UseCacheForSynchronizationXMLs = false;

                    ObjectVersionManager.EnsureVersion(infoObj, true);
                }
            }
        }


        /// <summary>
        /// Destroy object and child object versions
        /// </summary>
        /// <param name="infoObj">IInfo object which versions will be destroyed</param>
        public static void RemoveObjectVersions(BaseInfo infoObj)
        {
            bool destroy = !ObjectVersionManager.AllowObjectRestore(infoObj);

            // Remove object child versions by special query if exists
            string queryName = infoObj.TypeInfo.ObjectClassName + ".removechildversions";
            if (QueryInfoProvider.GetQueryInfo(queryName, false) != null)
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ID", infoObj.Generalized.ObjectID);

                // Remove the dependencies
                ConnectionHelper.ExecuteQuery(queryName, parameters);
            }

            // Destroy object versions if necessary
            if (destroy)
            {
                ObjectVersionManager.DestroyObjectHistory(infoObj.TypeInfo.ObjectType, infoObj.Generalized.ObjectID);
            }
        }


        /// <summary>
        /// Indicates whether object can be post-processed within the process of Import/Export or Staging because of potential reference to the object of the same object type which wasn't processed yet.
        /// </summary>
        /// <param name="typeInfo">Processed object type information.</param>
        /// <param name="failedTranslationColumnNames">Collection of column names whose values could not be translated.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="typeInfo"/> is not defined.</exception>
        public static bool IsPostProcessingAllowedForFailedTranslation(ObjectTypeInfo typeInfo, IEnumerable<string> failedTranslationColumnNames)
        {
            if (typeInfo == null)
            {
                throw new ArgumentNullException(nameof(typeInfo));
            }

            var dependencyColumnNames = GetSelfDependencyColumnNames(typeInfo)
                .ToHashSet(StringComparer.InvariantCultureIgnoreCase);

            // If there is no dependency column to compare the post processing cannot be used
            if (!dependencyColumnNames.Any())
            {
                return false;
            }

            if ((failedTranslationColumnNames != null) && failedTranslationColumnNames.Any(x => !dependencyColumnNames.Contains(x)))
            {
                // Do not allow post-processing for object if failed column is not in collection of dependency column related to the object of the same object type
                return false;
            }

            // Indicates that failed columns were not provided and there is at least one dependency column related to the object of the same object type
            // or all failed columns are in collection of dependency columns related to the object of the same object type
            return true;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Returns a collection of column names that reference to a dependency of the same object type as the given object type.
        /// </summary>
        /// <remarks>Dynamic object types are skipped.</remarks>
        /// <param name="typeInfo">Type info object.</param>
        private static IEnumerable<string> GetSelfDependencyColumnNames(ObjectTypeInfo typeInfo)
        {
            var isSelfBinding = typeInfo.IsSelfBinding;

            if (typeInfo.ObjectDependencies != null)
            {
                // Collection of dependency column names referencing to itself used for for comparison with failed translation column names
                foreach (var column in typeInfo.ObjectDependencies.Where(dependency =>
                    !dependency.HasDynamicObjectType()
                    && ((dependency.DependencyType == ObjectDependencyEnum.Binding) || (dependency.DependencyType == ObjectDependencyEnum.Required))
                    && IsSelfDependencyObjectType(dependency, typeInfo, isSelfBinding)).Select(dependency => dependency.DependencyColumn))
                {
                    yield return column;
                }
            }

            // Include parent column name if object is referencing to itself as parent or binding
            if (isSelfBinding || typeInfo.ObjectType.Equals(typeInfo.ParentObjectType, StringComparison.InvariantCultureIgnoreCase))
            {
                yield return typeInfo.ParentIDColumn;
            }
        }


        /// <summary>
        /// Returns true if <paramref name="dependency"/> is referencing to the same object type as defined in <paramref name="typeInfo"/> or to the <see cref="ObjectTypeInfo.ParentObjectType"/> for binding object referencing to itself. 
        /// </summary>
        /// <param name="dependency">Dependency to be examined.</param>
        /// <param name="typeInfo">Type information object holding the <paramref name="dependency"/>.</param>
        /// <param name="isSelfBinding">Optional parameter for performance optimization. If not provided self binding object is detected automatically.</param>
        private static bool IsSelfDependencyObjectType(ObjectDependency dependency, ObjectTypeInfo typeInfo, bool? isSelfBinding = null)
        {
            return dependency.DependencyObjectType.Equals(typeInfo.ObjectType, StringComparison.InvariantCultureIgnoreCase)
                        || ((isSelfBinding ?? typeInfo.IsSelfBinding) && dependency.DependencyObjectType.Equals(typeInfo.ParentObjectType, StringComparison.InvariantCultureIgnoreCase));
        }


        /// <summary>
        /// Gets the object created from the given DataRow.
        /// </summary>
        /// <param name="objectRow">Object DataRow</param>
        /// <param name="objectType">Object type</param>
        private static GeneralizedInfo GetObject(DataRow objectRow, string objectType)
        {
            GeneralizedInfo infoObj = ModuleManager.GetObject(objectRow, objectType);
            if (infoObj == null)
            {
                throw new Exception("[SynchronizationHelper.GetObject]: Unknown object type '" + objectType + "'.");
            }

            return infoObj;
        }

        #endregion
    }
}
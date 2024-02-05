using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;

using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.DocumentEngine;
using CMS.DocumentEngine.Internal;
using CMS.EventLog;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.LicenseProvider;
using CMS.Localization;
using CMS.Membership;
using CMS.OnlineForms;
using CMS.PortalEngine;
using CMS.Relationships;
using CMS.Search;
using CMS.SiteProvider;
using CMS.Taxonomy;
using CMS.WorkflowEngine;

namespace CMS.Synchronization
{
    /// <summary>
    /// Helper class for synchronization methods.
    /// </summary>
    public class SyncManager : AbstractManager<SyncManager>, ISyncManager
    {
        #region "Variables"

        private bool mUseAutomaticOrdering = true;
        private bool mLogTasks = true;
        private UserInfo mCurrentUser;
        private OperationTypeEnum mOperationType = OperationTypeEnum.Synchronization;
        private TreeProvider mTreeProvider;
        private VersionManager mVersionManager;
        private WorkflowManager mWorkflowManager;
        private int? mSiteID;
        private int? mAdministratorId;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Administrator user ID.
        /// </summary>
        public int AdministratorId
        {
            get
            {
                if (mAdministratorId == null)
                {
                    // Get administrator
                    var ui = UserInfoProvider.AdministratorUser;
                    if (ui != null)
                    {
                        mAdministratorId = ui.UserID;
                    }
                    else
                    {
                        throw new InvalidOperationException("Unable to resolve user ID.");
                    }
                }

                return mAdministratorId.Value;
            }
            set
            {
                mAdministratorId = value;
            }
        }


        /// <summary>
        /// TreeProvider object to use.
        /// </summary>
        public TreeProvider TreeProvider
        {
            get
            {
                if (mTreeProvider == null)
                {
                    mTreeProvider = new TreeProvider();

                    mTreeProvider.GenerateNewGuid = false;
                    mTreeProvider.UseCustomHandlers = UseTreeCustomHandlers;
                    mTreeProvider.UseAutomaticOrdering = UseAutomaticOrdering;
                    mTreeProvider.UpdateUser = false;
                    mTreeProvider.UpdateTimeStamps = false;

                    mTreeProvider.UserInfo = AdministratorUser;
                    mTreeProvider.LogSynchronization = false;
                }
                return mTreeProvider;
            }
            protected set
            {
                mTreeProvider = value;
            }
        }


        /// <summary>
        /// Workflow manager instance.
        /// </summary>
        protected virtual WorkflowManager WorkflowManager
        {
            get
            {
                return mWorkflowManager ?? (mWorkflowManager = WorkflowManager.GetInstance(TreeProvider));
            }
        }


        /// <summary>
        /// Version manager instance.
        /// </summary>
        protected virtual VersionManager VersionManager
        {
            get
            {
                return mVersionManager ?? (mVersionManager = VersionManager.GetInstance(TreeProvider));
            }
        }


        /// <summary>
        /// Indicates if logging staging tasks is enabled.
        /// </summary>
        public bool LogTasks
        {
            get
            {
                return mLogTasks;
            }
            set
            {
                mLogTasks = value;
            }
        }


        /// <summary>
        /// Indicates if custom handlers should be used for document operations.
        /// </summary>
        public bool UseTreeCustomHandlers
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if automatic ordering for documents should be used on target server.
        /// </summary>
        public bool UseAutomaticOrdering
        {
            get
            {
                return mUseAutomaticOrdering;
            }
            set
            {
                mUseAutomaticOrdering = value;
            }
        }


        /// <summary>
        /// Defines the operation type.
        /// </summary>
        public OperationTypeEnum OperationType
        {
            get
            {
                return mOperationType;
            }
            set
            {
                mOperationType = value;
            }
        }


        /// <summary>
        /// Gets or sets current site name.
        /// </summary>
        public string SiteName
        {
            get;
            set;
        }

        #endregion


        #region "Protected properties"

        /// <summary>
        /// Gets site ID.
        /// </summary>
        public int SiteID
        {
            get
            {
                // Site name is not set
                if (SiteName == null)
                {
                    return 0;
                }

                if (mSiteID == null)
                {
                    mSiteID = SiteInfoProvider.GetSiteID(SiteName);
                }

                return mSiteID.Value;
            }
        }


        /// <summary>
        /// Gets site ID for translation.
        /// </summary>
        protected int TranslationSiteID
        {
            get
            {
                // Bindings and child object can be from several different sites
                if (OperationType == OperationTypeEnum.Versioning)
                {
                    return TranslationHelper.AUTO_SITEID;
                }

                return SiteID;
            }
        }

        #endregion


        #region "Private properties"

        /// <summary>
        /// Gets the default administrator use based on <see cref="AdministratorId"/>
        /// </summary>
        private UserInfo AdministratorUser
        {
            get
            {
                if (mCurrentUser == null)
                {
                    // Get default administrator user
                    mCurrentUser = UserInfoProvider.GetUserInfo(AdministratorId);
                    if (mCurrentUser == null)
                    {
                        throw new InvalidOperationException("Cannot find default user with ID " + AdministratorId + ".");
                    }
                }

                return mCurrentUser;
            }
        }

        #endregion


        #region "Public methods"

        /// <summary>
        /// Processes the given task.
        /// </summary>
        /// <param name="taskType">Type of the task</param>
        /// <param name="taskObjectType">Task object type</param>
        /// <param name="taskData">Document/object data for the synchronization task</param>
        /// <param name="taskBinaryData">Binary data for the synchronization task</param>
        public ICMSObject ProcessTask(TaskTypeEnum taskType, string taskObjectType, string taskData, string taskBinaryData)
        {
            return ProcessTask(taskType, taskObjectType, taskData, taskBinaryData, true, null);
        }


        /// <summary>
        /// Processes the given task.
        /// </summary>
        /// <param name="taskType">Type of the task</param>
        /// <param name="taskObjectType">Task object type</param>
        /// <param name="taskData">Document/object data for the synchronization task</param>
        /// <param name="taskBinaryData">Binary data for the synchronization task</param>
        /// <param name="processChildren">Indicates if also child objects should be processed</param>
        public ICMSObject ProcessTask(TaskTypeEnum taskType, string taskObjectType, string taskData, string taskBinaryData, bool processChildren)
        {
            return ProcessTask(taskType, taskObjectType, taskData, taskBinaryData, processChildren, null);
        }


        /// <summary>
        /// Processes the given task.
        /// </summary>
        /// <param name="taskType">Type of the task</param>
        /// <param name="taskObjectType">Task object type</param>
        /// <param name="taskData">Document/object data for the synchronization task</param>
        /// <param name="taskBinaryData">Binary data for the synchronization task</param>
        /// <param name="processChildren">Indicates if also child objects should be processed</param>
        /// <param name="handler">Synchronization handler</param>
        public ICMSObject ProcessTask(TaskTypeEnum taskType, string taskObjectType, string taskData, string taskBinaryData, bool processChildren, StagingSynchronizationHandler handler)
        {
            return ProcessTask(new StagingTaskData { TaskType = taskType, TaskObjectType = taskObjectType, TaskData = taskData, TaskBinaryData = taskBinaryData }, processChildren, handler);
        }


        /// <summary>
        /// Processes the given task.
        /// </summary>
        /// <param name="stagingTaskData">StagingTaskData that encapsulates staging task</param>
        /// <param name="processChildren">Indicates if also child objects should be processed</param>
        /// <param name="handler">Synchronization handler</param>
        public ICMSObject ProcessTask(IStagingTaskData stagingTaskData, bool processChildren, StagingSynchronizationHandler handler)
        {
            return ProcessTaskInternal(stagingTaskData, processChildren, handler);
        }


        /// <summary>
        /// Gets synchronization task empty DataSet for specified task type.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="className">Document class name (optional)</param>
        /// <param name="taskObjectType">Task object type (optional)</param>
        public DataSet GetSynchronizationTaskDataSet(TaskTypeEnum taskType, string className, string taskObjectType)
        {
            return GetSynchronizationTaskDataSetInternal(taskType, className, taskObjectType);
        }


        /// <summary>
        /// Returns the dataset loaded from the given task data.
        /// </summary>
        /// <param name="taskData">Task data to make the DataSet from</param>
        /// <param name="taskType">Task type</param>
        /// <param name="objectType">Object type</param>
        public DataSet GetDataSet(string taskData, TaskTypeEnum taskType, string objectType)
        {
            return GetDataSetInternal(taskData, taskType, objectType);
        }

        #endregion


        #region "Objects methods"

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="objectDS">Object data</param>
        /// <param name="binaryData">Binary data</param>
        /// <param name="taskObjectType">Task object type</param>
        /// <param name="th">Translation helper</param>
        /// <param name="addToSite">If true, assigns the object to the site</param>
        /// <param name="updateChildren">Indicates if child objects should be updated</param>
        protected virtual ICMSObject UpdateObject(DataSet objectDS, DataSet binaryData, string taskObjectType, TranslationHelper th, bool updateChildren, bool addToSite)
        {
            // Get the object definition
            BaseInfo infoObj = ModuleManager.GetObject(taskObjectType);

            // Get the translation table
            if (th == null)
            {
                DataTable transTable = objectDS.Tables[TranslationHelper.TRANSLATION_TABLE];
                th = new TranslationHelper(transTable);
            }

            // Update the objects
            return UpdateObjects(infoObj, objectDS, th, updateChildren, binaryData, addToSite, true);
        }


        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="infoObj">Object type definition to process</param>
        /// <param name="ds">DataSet with the object data</param>
        /// <param name="th">Translation table</param>
        /// <param name="updateChildren">Indicates if child objects should be updated</param>
        /// <param name="binaryData">DataSet with binary data</param>
        /// <param name="addToSite">If true, assigns the objects to the site</param>
        /// <param name="logTasks">Indicates if synchronization tasks should be logged</param>
        /// <returns>Returns last processed object</returns>
        protected virtual ICMSObject UpdateObjects(BaseInfo infoObj, DataSet ds, TranslationHelper th, bool updateChildren, DataSet binaryData, bool addToSite, bool logTasks)
        {
            var ti = infoObj.TypeInfo;

            if (updateChildren && !ti.IsCategory)
            {
                // Process categories first
                BaseInfo categoryObj = ti.CategoryObject;
                if (categoryObj != null)
                {
                    // Update all categories
                    UpdateObjects(categoryObj, ds, th, false, null, addToSite, false);
                }
            }

            var dt = GetTable(ds, infoObj);

            string objectType = ti.ObjectType;
            List<GeneralizedInfo> postProcessList = new List<GeneralizedInfo>();

            // Update the objects within table
            foreach (DataRow objectRow in dt.Rows)
            {
                // Load the object
                infoObj = GetObject(objectRow, objectType);

                // Disconnect the external columns from FS - the data we need are the data from DataRow
                infoObj.Generalized.IgnoreExternalColumns = true;

                // Update the data
                UpdateResultEnum result = UpdateObject(ref infoObj, ds, th, updateChildren, binaryData, addToSite, postProcessList);

                // If imported, process additional actions
                switch (result)
                {
                    case UpdateResultEnum.PostProcess:
                        {
                            // Add to post process list
                            GeneralizedInfo postObj = GetObject(objectRow, objectType);
                            postProcessList.Add(postObj);
                        }
                        break;

                    case UpdateResultEnum.OK:
                        if (logTasks)
                        {
                            LogSyncTasks(infoObj, addToSite);
                        }
                        break;
                }
            }

            // Process post list
            if (postProcessList.Count > 0)
            {
                foreach (GeneralizedInfo po in postProcessList)
                {
                    UpdateResultEnum postResult;

                    var postObject = po;
                    if (postObject.TypeInfo.IsBinding)
                    {
                        // Update the binding
                        string error = th.TranslateColumns(postObject, false, true, true, TranslationSiteID, null);

                        // Update/Insert the binding if translation is successful
                        if (error == "")
                        {
                            postObject.ObjectID = 0;
                            postObject.SetObject();
                            postResult = UpdateResultEnum.OK;
                        }
                        else
                        {
                            postResult = UpdateResultEnum.Error;
                        }
                    }
                    else
                    {
                        // Update the object
                        postResult = UpdateObject(ref postObject, ds, th, updateChildren, binaryData, addToSite, postProcessList);
                    }

                    // Log synchronization tasks
                    if (postResult == UpdateResultEnum.OK)
                    {
                        LogSyncTasks(postObject, addToSite);
                    }
                }
            }

            // Action after update of all objects
            switch (objectType)
            {
                // Clear resource strings
                case ResourceStringInfo.OBJECT_TYPE:
                    ProviderHelper.ClearHashtables(ResourceStringInfo.OBJECT_TYPE, true);
                    break;
            }

            // Remove read-only object from cache
            ModuleManager.RemoveReadOnlyObject(objectType, true);

            return infoObj;
        }


        /// <summary>
        /// Creates a new object instance from the given data row
        /// </summary>
        /// <param name="dr">Data row</param>
        /// <param name="objectType">Object type</param>
        private static BaseInfo GetObject(DataRow dr, string objectType)
        {
            return ModuleManager.GetObject(new LoadDataSettings(dr, objectType)
            {
                DataIsExternal = true
            });
        }


        private static DataTable GetTable(DataSet ds, BaseInfo infoObj)
        {
            // Get the object data table
            var dt = ObjectHelper.GetTable(ds, infoObj);
            if (dt == null)
            {
                var tableName = ObjectHelper.GetSerializationTableName(infoObj);

                //## Special case - backward compatibility for custom tables tasks
                if (infoObj.TypeInfo.IsDataObjectType)
                {
                    DataHelper.RenameTable(ds, "CustomTableData", tableName);
                    dt = ds.Tables[tableName];
                }

                if (dt == null)
                {
                    throw new InvalidOperationException("Object data not found - table '" + tableName + "'.");
                }
            }
            return dt;
        }


        private void LogSyncTasks(GeneralizedInfo infoObj, bool addToSite)
        {
            using (CMSActionContext context = new CMSActionContext())
            {
                context.LogSynchronization = LogTasks;
                context.LogIntegration = false;
                context.LogEvents = false;
                // Create version for versioning only if the child object is not versioned separately
                context.CreateVersion = (OperationType != OperationTypeEnum.Versioning) || ((infoObj.TypeInfo.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && !infoObj.VersioningEnabled);
                context.UpdateSystemFields = false;

                bool touchParent = (infoObj.LogSynchronization == SynchronizationTypeEnum.TouchParent);
                context.TouchParent = touchParent;

                // Log the synchronization for next iterations
                if (touchParent)
                {
                    SynchronizationHelper.TouchParent(infoObj, TaskTypeEnum.UpdateObject);
                }
                else
                {
                    SynchronizationHelper.LogObjectUpdate(infoObj, true, addToSite);
                }
            }
        }


        /// <summary>
        /// Indicates if the object supports post processing
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="failedTranslationColumnNames">Collection of column names whose values could not be translated.</param>
        private static bool IsObjectPostProcess(BaseInfo infoObj, IEnumerable<string> failedTranslationColumnNames)
        {
            return SynchronizationHelper.IsPostProcessingAllowedForFailedTranslation(infoObj.TypeInfo, failedTranslationColumnNames);
        }


        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="infoObj">Object to update</param>
        /// <param name="ds">DataSet with the object data</param>
        /// <param name="th">Translation table</param>
        /// <param name="updateChildren">Indicates if child objects should be updated</param>
        /// <param name="binaryData">DataSet with binary data</param>
        /// <param name="addToSite">If true, assigns the object to the site</param>
        /// <param name="postProcessList">List of objects which should be processed after finishing of main objects</param>
        protected virtual UpdateResultEnum UpdateObject<TInfo>(ref TInfo infoObj, DataSet ds, TranslationHelper th, bool updateChildren, DataSet binaryData, bool addToSite, List<GeneralizedInfo> postProcessList)
            where TInfo : BaseInfo
        {
            GeneralizedInfo genCi = infoObj;

            var result = UpdateObject(ref genCi, ds, th, updateChildren, binaryData, addToSite, postProcessList);

            infoObj = (TInfo)genCi.MainObject;

            return result;
        }


        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="infoObj">Object to update</param>
        /// <param name="ds">DataSet with the object data</param>
        /// <param name="th">Translation table</param>
        /// <param name="updateChildren">Indicates if child objects should be updated</param>
        /// <param name="binaryData">DataSet with binary data</param>
        /// <param name="addToSite">If true, assigns the object to the site</param>
        /// <param name="postProcessList">List of objects which should be processed after finishing of main objects</param>
        protected virtual UpdateResultEnum UpdateObject(ref GeneralizedInfo infoObj, DataSet ds, TranslationHelper th, bool updateChildren, DataSet binaryData, bool addToSite, List<GeneralizedInfo> postProcessList)
        {
            using (var context = new CMSActionContext())
            {
                var ti = infoObj.TypeInfo;

                // Create version for child objects which support versioning
                context.CreateVersion = ((ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && ObjectVersionManager.AllowObjectVersioning(infoObj));

                int originalId = infoObj.ObjectID;
                GeneralizedInfo existing = null;

                // Init default columns
                // Site ID is not set in case of versioning to avoid overriding original object Site ID
                if ((OperationType != OperationTypeEnum.Versioning) && (infoObj.ObjectSiteID > 0))
                {
                    infoObj.ObjectSiteID = SiteID;
                }
                infoObj.ObjectID = 0;


                // ### Special case for Biz Forms - update the class ID
                int formClassId = 0;
                if ((infoObj.MainObject is BizFormInfo) && (updateChildren || (OperationType == OperationTypeEnum.Versioning)))
                {
                    BizFormInfo formObj = (BizFormInfo)infoObj.MainObject;
                    formClassId = formObj.FormClassID;
                    formObj.FormClassID = 0;
                }
                string error = string.Empty;

                // Translate the object columns
                if (ProceedWithTranslations(th))
                {
                    th.SetDefaultValue(PredefinedObjectType.USER, AdministratorId);
                    error = th.TranslateColumns(infoObj, false, true, true, TranslationSiteID, null);
                    th.RemoveDefaultValue(PredefinedObjectType.USER);
                }

                if (error != string.Empty)
                {
                    if ((postProcessList != null) && (!postProcessList.Contains(infoObj)))
                    {
                        return UpdateResultEnum.PostProcess;
                    }

                    throw new InvalidOperationException("Cannot translate columns '" + error + "', import the dependent objects first.");
                }

                // ### Special case Site - update the site
                if (infoObj.MainObject is SiteInfo)
                {
                    PrepareSiteInfo((SiteInfo)infoObj);
                }
                else
                {
                    // Update ID if object exists
                    existing = infoObj.GetExisting();

                    if (existing != null)
                    {
                        // For integration
                        if ((OperationType == OperationTypeEnum.Integration) && (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                        {
                            UpdateParentFromExisting(infoObj, existing);
                        }

                        infoObj.ObjectID = GetUpdatedInfoObjectId(infoObj, th, originalId);
                        ValidateVersioningUniqueCodeName(infoObj);

                        if (OperationType != OperationTypeEnum.Versioning)
                        {
                            PreserveExcludedValues(infoObj, existing);
                            PreserveMediaFileValues(infoObj, existing);
                        }

                        // Copy data from infoObj (the new version) to the existing object and further work with the existing one
                        // This has to be done so the column data changes are correctly detected to existing objects.
                        CopyData(infoObj, existing);
                        infoObj = existing;
                    }
                    else
                    {
                        if (OperationType != OperationTypeEnum.Versioning)
                        {
                            SetDefaultValuesInExcludedColumns(infoObj);
                        }
                    }
                }
                
                #region "### Special cases"

                ProcessCreateUpdateTaskSpecialCases(infoObj, ds, th, updateChildren, addToSite, postProcessList, formClassId, existing);

                #endregion
                
                // Ensure complete object data for new object
                if (infoObj.ObjectID == 0)
                {
                    infoObj.MakeComplete(false);
                }

                // Force the IDPath to be regenerated on the target in all cases
                if (ti.ObjectIDPathColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    infoObj.SetValue(ti.ObjectIDPathColumn, null);
                }

                // Handle binary data
                infoObj.UpdatePhysicalFiles(binaryData);

                // Update the object to the database
                infoObj.SetObject();

                // Add ID translation
                if ((ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && ProceedWithTranslations(th))
                {
                    th.AddIDTranslation(ti.ObjectType, originalId, infoObj.ObjectID, infoObj.ObjectGroupID);
                }

                // Update child objects
                if (updateChildren)
                {
                    UpdateChildren(infoObj, ds, th, addToSite, postProcessList, originalId);

                    UpdateBindings(infoObj, ds, th, addToSite, postProcessList, originalId);
                }
                else if ((OperationType == OperationTypeEnum.Integration) && addToSite && (SiteID > 0))
                {
                    AddToSite(infoObj);
                }

                UpdateMetaFiles(infoObj, ds, originalId);

                // Post process the object
                PostProcessObject(infoObj, th, updateChildren, existing);

                return UpdateResultEnum.OK;
            }
        }


        private void ProcessCreateUpdateTaskSpecialCases(GeneralizedInfo infoObj, DataSet ds, TranslationHelper th, bool updateChildren, bool addToSite, List<GeneralizedInfo> postProcessList, int formClassId, GeneralizedInfo existing)
        {
            // ### Special case for Biz Forms - update the class
            if (infoObj.MainObject is BizFormInfo)
            {
                if (updateChildren || (OperationType == OperationTypeEnum.Versioning))
                {
                    BizFormInfo formObj = (BizFormInfo)infoObj;

                    UpdateFormClass(ds, th, addToSite, postProcessList, formClassId, formObj, existing);
                }
            }

            // ### Special case for user - make sure the user settings are not initialized
            else if (infoObj.MainObject is UserInfo)
            {
                UserInfo userObj = (UserInfo)infoObj;
                userObj.UserSettings = null;
            }

            // ### Special case for scheduled task - make sure that incoming change is again logged to the next stage
            else if (infoObj.MainObject is Scheduler.TaskInfo)
            {
                infoObj.LogSynchronization = SynchronizationTypeEnum.LogSynchronization;
            }
        }


        private static void UpdateParentFromExisting(GeneralizedInfo infoObj, GeneralizedInfo existing)
        {
            var ti = infoObj.TypeInfo;

            // Get parent identifier from existing object (if exists)
            int parentId = ValidationHelper.GetInteger(existing.GetValue(ti.ParentIDColumn), 0);
            if (parentId > 0)
            {
                infoObj.SetValue(ti.ParentIDColumn, parentId);
            }
        }


        private void UpdateFormClass(DataSet ds, TranslationHelper th, bool addToSite, List<GeneralizedInfo> postProcessList, int formClassId, BizFormInfo formObj, GeneralizedInfo existing)
        {
            var ti = formObj.TypeInfo;

            // Update the class
            var classDT = ds.Tables["CMS_Class"];

            if (!DataHelper.DataSourceIsEmpty(classDT))
            {
                // Get the data
                DataRow[] classRow = classDT.Select("ClassID = " + formClassId);
                if (classRow.Length > 0)
                {
                    // Update class object
                    var ci = DataClassInfo.New(classRow[0]);
                    var className = ci.ClassName;

                    if (formObj.FormID == 0)
                    {
                        var tm = new TableManager(ci.ClassConnectionString);

                        // New - create unique class
                        ci.ClassGUID = Guid.NewGuid();
                        ci.ClassTableName = tm.GetUniqueTableName(ci.ClassTableName);
                        ci.ClassName = DataClassInfoProvider.GetUniqueClassName(className);
                    }
                    else if (existing != null)
                    {
                        var existingObj = (BizFormInfo)existing;

                        // Get data from existing class
                        var existingClass = DataClassInfoProvider.GetDataClassInfo(existingObj.FormClassID);
                        if (existingClass != null)
                        {
                            ci.ClassGUID = existingClass.ClassGUID;
                            ci.ClassTableName = existingClass.ClassTableName;
                            ci.ClassName = existingClass.ClassName;
                        }
                    }

                    if (ProceedWithTranslations(th))
                    {
                        // Change class translation
                        th.ChangeCodeName(new TranslationParameters(ti) { CodeName = className }, ci.ClassName);
                    }

                    UpdateObject(ref ci, ds, th, true, null, addToSite, postProcessList);

                    // Set new class ID
                    formObj.FormClassID = ci.ClassID;
                }
            }
        }


        /// <summary>
        /// Gets ID to be used for the updated <see cref="BaseInfo.GeneralizedInfoWrapper.ObjectID"/>.
        /// </summary>
        /// <remarks>
        /// If the updated object is not allowed to be stored in recycle bin, it gets new ID using <see cref="TranslationHelper.GetNewID(string,int,string,int,string,string,string)"/>,
        /// otherwise original ID is used to prevent overwriting another object, which has only same codename. 
        /// </remarks>
        /// <param name="infoObj">Object to be restored from bin or minor versions with id equals zero</param>
        /// <param name="th">Translation table</param>
        /// <param name="originalId">Id that should be checked with code name</param>
        /// <returns>ID for the updated <see cref="BaseInfo.GeneralizedInfoWrapper.ObjectID"/></returns>
        private static int GetUpdatedInfoObjectId(GeneralizedInfo infoObj, TranslationHelper th, int originalId)
        {
            ObjectTypeInfo ti = infoObj.TypeInfo;
            int updatedInfoObjectId = 0;

            if (!ti.AllowRestore)
            {
                updatedInfoObjectId = th.GetNewID(ti.ObjectType, originalId, infoObj.CodeNameColumn, infoObj.ObjectSiteID, ti.SiteIDColumn, ti.ParentIDColumn, ti.GroupIDColumn);
            }

            return updatedInfoObjectId > 0 ? updatedInfoObjectId : originalId;
        }


        /// <summary>
        /// Validates that object to be restored from bin or from minor version has unique code name.
        /// </summary>
        /// <remarks>
        /// Make sure that <paramref name="infoObj"/> has properly set its <see cref="BaseInfo.GeneralizedInfoWrapper.ObjectID"/>.
        /// </remarks>
        /// <param name="infoObj">Object to be restored from bin or from minor version</param>
        private void ValidateVersioningUniqueCodeName(GeneralizedInfo infoObj)
        {
            if (OperationType == OperationTypeEnum.Versioning && !infoObj.CheckUniqueCodeName())
            {
                throw new CodeNameNotUniqueException(infoObj);
            }
        }


        private void AddToSite(GeneralizedInfo infoObj)
        {
            var ti = infoObj.TypeInfo;

            // Update bindings
            foreach (string bindingType in ti.BindingObjectTypes)
            {
                GeneralizedInfo bindingObj = ModuleManager.GetObject(bindingType);
                if (bindingObj == null)
                {
                    // Skip missing object type
                    continue;
                }

                var bindingTypeInfo = bindingObj.TypeInfo;
                if (bindingTypeInfo.IncludeToParentDataSet(OperationType) != IncludeToParentEnum.None)
                {
                    // Check column definition
                    if (bindingTypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        throw new InvalidOperationException("Parent ID column for object type '" + bindingType + "' is not defined.");
                    }

                    if (bindingTypeInfo.IsSiteBinding)
                    {
                        CreateSiteBinding(bindingObj, infoObj.ObjectID);
                    }
                }
            }
        }


        private void UpdateBindings(GeneralizedInfo infoObj, DataSet ds, TranslationHelper th, bool addToSite, List<GeneralizedInfo> postProcessList, int originalId)
        {
            var ti = infoObj.TypeInfo;

            // Update bindings
            var bindingTypes = new List<string>();
            bindingTypes.AddRange(ti.BindingObjectTypes);

            // Restore data include other bindings
            if (OperationType == OperationTypeEnum.Versioning)
            {
                TypeHelper.AddTypes(bindingTypes, ti.OtherBindingObjectTypes);
            }

            foreach (string bindingType in bindingTypes)
            {
                GeneralizedInfo bindingObj = ModuleManager.GetObject(bindingType);
                if (bindingObj == null)
                {
                    // Skip missing object type
                    continue;
                }

                var bindingTypeInfo = bindingObj.TypeInfo;
                
                
                var processingType = GetChildProcessingType(ti.ObjectType, bindingTypeInfo);
                if (processingType == IncludeToParentEnum.None)
                {
                    // Do not process any incoming child objects of that type
                    continue;
                }

                if (!bindingTypeInfo.IsSiteBinding)
                {
                    // Get where condition for all related bindings
                    string bindingColumn;
                    if (bindingTypeInfo.IsSelfBinding && (OperationType != OperationTypeEnum.Versioning))
                    {
                        // Check if parent ID column is defined so it can be used
                        if (bindingTypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            throw new InvalidOperationException("Parent ID column for object type '" + bindingType + "' is not defined.");
                        }

                        bindingColumn = bindingTypeInfo.ParentIDColumn;
                    }
                    else
                    {
                        // Get the corresponding binding column
                        bindingColumn = bindingTypeInfo.GetTypeColumns(ti).FirstOrDefault();
                    }

                    if (String.IsNullOrEmpty(bindingColumn))
                    {
                        throw new InvalidOperationException("Binding columns of object type '" + bindingType + "' for object type '" + ti.ObjectType + "' not found.");
                    }

                    // The 'Complete' option means that all old binding on target server will be removed
                    if (processingType == IncludeToParentEnum.Complete)
                    {
                        // Get existing bindings
                        var bindingWhere = new WhereCondition().WhereEquals(bindingColumn, infoObj.ObjectID);

                        // Some data should be selected
                        if (!bindingWhere.ReturnsNoResults)
                        {
                            // Handle all bindings for versioning
                            if (OperationType != OperationTypeEnum.Versioning)
                            {
                                // Add site condition
                                if (bindingTypeInfo.ObjectDependencies != null)
                                {
                                    foreach (var dep in bindingTypeInfo.ObjectDependencies)
                                    {
                                        string depType = bindingObj.GetDependencyObjectType(dep);

                                        var depObj = ModuleManager.GetReadOnlyObject(depType);

                                        var siteIdColumn = depObj.TypeInfo.SiteIDColumn;
                                        if (siteIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                        {
                                            var global = new Tuple<string, int?>(PredefinedObjectType.SITE, null);
                                            var site = (SiteID > 0) ? new Tuple<string, int?>(PredefinedObjectType.SITE, SiteID) : null;

                                            bindingWhere.Where(bindingTypeInfo.GetDependencyWhereCondition(depType, "OR", global, site));
                                        }
                                    }
                                }
                            }

                            var existingBindingDS = bindingObj.GetDataQuery(true, q => q.Where(bindingWhere), false).Result;

                            // Delete all bindings
                            if (!DataHelper.DataSourceIsEmpty(existingBindingDS))
                            {
                                foreach (DataRow dr in existingBindingDS.Tables[0].Rows)
                                {
                                    // Delete the binding
                                    BaseInfo binding = GetObject(dr, bindingType);
                                    binding.Generalized.DeleteObject();
                                }
                            }
                        }
                    }

                    // Get binding objects table
                    var bindingDT = ObjectHelper.GetTable(ds, bindingObj);

                    // Get binding rows
                    DataRow[] bindingRows = null;
                    if (!DataHelper.DataSourceIsEmpty(bindingDT))
                    {
                        // Please note that filter expression must be used due to column type mishmash in some tables (integer is represented as string).
                        // Without the change in object manager strongly-typed selection cannot be used
                        bindingRows = bindingDT.Select(bindingColumn + " = " + originalId);
                    }

                    // Add current bindings
                    if ((bindingRows != null) && (bindingRows.Length > 0))
                    {
                        foreach (DataRow dr in bindingRows)
                        {
                            GeneralizedInfo binding = GetObject(dr, bindingType);

                            // Make sure the parent ID of the binding is set to correct parent (this needs to be explicitly here because of 
                            // situations when new object is created along with the binding in the Bindings collection and those are therefore not 
                            // connected by IDs - since the main object does not have ID yet.
                            if (binding.ObjectParentID == 0)
                            {
                                binding.SetValue(bindingTypeInfo.ParentIDColumn, infoObj.ObjectID);
                                th.AddIDTranslation(bindingTypeInfo.ParentObjectType, infoObj.ObjectID, infoObj.ObjectID, 0);
                            }

                            string error = th.TranslateColumns(binding, false, true, true, TranslationSiteID, null);

                            // Update / insert the binding if translation is successful
                            if (error == string.Empty)
                            {
                                // Ensure complete object data
                                binding.MakeComplete(false);

                                binding.ObjectID = 0;
                                binding.SetObject();
                            }
                            else if (IsObjectPostProcess(binding, error.Split(';')))
                            {
                                // Restore original object data
                                binding = GetObject(dr, bindingType);

                                // Add to post process list
                                postProcessList.Add(binding);
                            }
                        }
                    }
                }
                else if (addToSite && (SiteID > 0))
                {
                    CreateSiteBinding(bindingObj, infoObj.ObjectID);
                }
            }
        }


        private void UpdateChildren(GeneralizedInfo infoObj, DataSet ds, TranslationHelper th, bool addToSite, List<GeneralizedInfo> postProcessList, int originalId)
        {
            var ti = infoObj.TypeInfo;

            foreach (string childType in ti.ChildObjectTypes)
            {
                GeneralizedInfo childObj = ModuleManager.GetReadOnlyObject(childType);
                if (childObj == null)
                {
                    // Skip missing object type
                    continue;
                }

                var childTypeInfo = childObj.TypeInfo;

                var processingType = GetChildProcessingType(ti.ObjectType, childTypeInfo);
                if (processingType == IncludeToParentEnum.None)
                {
                    // Do not process any incoming child objects of that type
                    continue;
                }

                // Check columns definition
                if (childTypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    throw new InvalidOperationException("Parent ID column for object type '" + childType + "' is not defined.");
                }

                // Get existing child objects
                DataSet existingChildDS = null;
                if (processingType == IncludeToParentEnum.Complete)
                {
                    var addSiteCondition = OperationType != OperationTypeEnum.Versioning;

                    existingChildDS = ObjectHelper.GetExistingChildren(infoObj, addSiteCondition, SiteID, childType).Result;
                }

                // Get child objects table
                var childDT = ObjectHelper.GetTable(ds, childObj);

                // Get child rows
                DataRow[] childRows = null;
                string sourceChildrenWhere = null;

                if (!DataHelper.DataSourceIsEmpty(childDT))
                {
                    // Add custom where condition
                    int parentId = infoObj.ObjectID;
                    infoObj.ObjectID = originalId;

                    var childWhere = new WhereCondition().WhereEquals(childTypeInfo.ParentIDColumn, originalId);

                    sourceChildrenWhere = infoObj.GetChildWhereCondition(childWhere, childTypeInfo.ObjectType).ToString(true);

                    infoObj.ObjectID = parentId;
                    childRows = childDT.Select(sourceChildrenWhere);
                }

                // If some child objects are present, process
                if ((childRows != null) && (childRows.Length > 0))
                {
                    // Remove existing child objects
                    if (processingType == IncludeToParentEnum.Complete)
                    {
                        if (!DataHelper.DataSourceIsEmpty(existingChildDS))
                        {
                            foreach (DataRow dr in existingChildDS.Tables[0].Rows)
                            {
                                if (!CanProcessObject(childType, dr))
                                {
                                    continue;
                                }

                                // Get new record
                                string codeNameWhere = string.Empty;

                                // Add GUID where
                                if (childTypeInfo.GUIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                {
                                    Guid guid = ValidationHelper.GetGuid(dr[childTypeInfo.GUIDColumn], Guid.Empty);
                                    if (guid != Guid.Empty)
                                    {
                                        codeNameWhere = SqlHelper.AddWhereCondition(codeNameWhere, childTypeInfo.GUIDColumn + " = '" + guid + "'", "OR");
                                    }
                                }

                                // Add code name where (secondary lookup)
                                if (childTypeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                {
                                    if (childTypeInfo.CodeNameColumn != childTypeInfo.GUIDColumn)
                                    {
                                        string codeName = ValidationHelper.GetString(dr[childTypeInfo.CodeNameColumn], string.Empty);
                                        codeNameWhere = SqlHelper.AddWhereCondition(codeNameWhere, childTypeInfo.CodeNameColumn + " = '" + SqlHelper.GetSafeQueryString(codeName, false) + "'", "OR");
                                    }
                                }

                                // Get existing record
                                DataRow[] newRows = null;
                                if (codeNameWhere != string.Empty)
                                {
                                    codeNameWhere = SqlHelper.AddWhereCondition(sourceChildrenWhere, codeNameWhere);
                                    newRows = childDT.Select(codeNameWhere);
                                }

                                if ((newRows == null) || (newRows.Length == 0))
                                {
                                    // Delete existing child
                                    GeneralizedInfo child = GetObject(dr, childType);

                                    using (CMSActionContext childContext = new CMSActionContext())
                                    {
                                        childContext.CreateVersion = true;
                                        child.DeleteObject();
                                    }
                                }
                            }
                        }
                    }

                    // Update child objects
                    foreach (DataRow dr in childRows)
                    {
                        // Update the child object
                        GeneralizedInfo child = GetObject(dr, childType);

                        // Make sure the parent ID of the child is set to correct parent (this needs to be explicitly here because of 
                        // situations when new object is created along with the child in the Children collection and those are therefore not 
                        // connected by IDs - since the main object does not have ID yet.
                        if (child.ObjectParentID == 0)
                        {
                            child.SetValue(childTypeInfo.ParentIDColumn, infoObj.ObjectID);
                            th.AddIDTranslation(childTypeInfo.ParentObjectType, infoObj.ObjectID, infoObj.ObjectID, 0);
                        }

                        if (!CanProcessObject(childType, dr))
                        {
                            continue;
                        }

                        // Update the child object
                        using (new CMSActionContext())
                        {
                            var updated = UpdateObject(ref child, ds, th, true, null, addToSite, postProcessList);

                            switch (updated)
                            {
                                case UpdateResultEnum.PostProcess:
                                {
                                    // Restore original object data
                                    child = GetObject(dr, childType);

                                    // Add to post process list
                                    postProcessList.Add(child);
                                }
                                    break;
                            }
                        }
                    }
                }
                else if (processingType == IncludeToParentEnum.Complete)
                {
                    // Delete all child objects
                    if (!DataHelper.DataSourceIsEmpty(existingChildDS))
                    {
                        foreach (DataRow dr in existingChildDS.Tables[0].Rows)
                        {
                            if (!CanProcessObject(childType, dr))
                            {
                                continue;
                            }

                            // Delete the child object
                            using (CMSActionContext childContext = new CMSActionContext())
                            {
                                childContext.CreateVersion = true;

                                // Delete the child object
                                BaseInfo child = GetObject(dr, childType);

                                child.Generalized.DeleteObject();
                            }
                        }
                    }
                }
            }
        }


        private IncludeToParentEnum GetChildProcessingType(string parentObjectType, ObjectTypeInfo childTypeInfo)
        {
            var processingType = childTypeInfo.IncludeToParentDataSet(OperationType);

            // If processing is set to 'None' by default from TypeInfo it doesn't make sense to call handler because data are not available
            if (processingType == IncludeToParentEnum.None)
            {
                return processingType;
            }

            if (OperationType != OperationTypeEnum.Synchronization)
            {
                return processingType;
            }

            var handler = StagingEvents.GetChildProcessingType;
            if (handler.IsBound)
            {
                var e = new StagingChildProcessingTypeEventArgs
                {
                    ParentObjectType = parentObjectType,
                    ObjectType = childTypeInfo.ObjectType,
                    ProcessingType = processingType
                };

                handler.StartEvent(e);

                processingType = e.ProcessingType;
            }

            return processingType;
        }


        private static bool CanProcessObject(string objectType, DataRow dr)
        {
            // ### Special case - Skip system queries if set ##

            if (objectType == QueryInfo.OBJECT_TYPE)
            {
                return ValidationHelper.GetBoolean(dr["QueryIsCustom"], false);
            }

            return true;
        }


        private void UpdateMetaFiles(GeneralizedInfo infoObj, DataSet ds, int originalId)
        {
            var ti = infoObj.TypeInfo;
            if (ti.HasMetaFiles)
            {
                DataTable filesDT = ds.Tables["CMS_MetaFile"];
                MetaFileInfoProvider.UpdateMetaFiles(infoObj, filesDT, originalId, null, LogTasks);
            }
        }


        private void PostProcessObject(GeneralizedInfo infoObj, TranslationHelper th, bool updateChildren, GeneralizedInfo existing)
        {
            var ti = infoObj.TypeInfo;

            #region "### Special cases"

            // ### Special case for class - update the default queries and view
            if (infoObj.MainObject is DataClassInfo)
            {
                PostProcessClass((DataClassInfo)infoObj.MainObject);
            }

            switch (ti.ObjectType)
            {
                case PredefinedObjectType.OPTIONCATEGORY:
                    // ### Special case for product options - update the default values
                    {
                        bool translated = false;

                        if (ProceedWithTranslations(th))
                        {
                            translated = th.TranslateListColumn(infoObj, "CategoryDefaultOptions", PredefinedObjectType.SKU, 0, ',');
                        }

                        // Translate default values list
                        if (translated || (OperationType == OperationTypeEnum.Integration))
                        {
                            infoObj.SetObject();
                        }
                    }
                    break;

                case WorkflowInfo.OBJECT_TYPE:
                    // ### Special case for workflow - Re-init workflow step orders
                    {
                        WorkflowInfo wi = (WorkflowInfo)infoObj;

                        // Create default steps for restore from recycle bin
                        if ((OperationType == OperationTypeEnum.Versioning) && !updateChildren && (existing == null))
                        {
                            WorkflowStepInfoProvider.CreateDefaultWorkflowSteps(wi);
                        }

                        WorkflowStepInfoProvider.InitStepOrders(wi);
                    }
                    break;

                case PageTemplateInfo.OBJECT_TYPE:
                    // ### Special case for page template - Refresh child objects count
                    {
                        PageTemplateInfo pti = (PageTemplateInfo)infoObj;
                        PageTemplateCategoryInfoProvider.UpdateCategoryChildCount(0, pti.CategoryID);
                        PageTemplateCategoryInfoProvider.UpdateCategoryTemplateChildCount(0, pti.CategoryID);
                    }
                    break;

                case WidgetInfo.OBJECT_TYPE:
                    // ### Special case for widget - Refresh child objects count
                    {
                        WidgetInfo wi = (WidgetInfo)infoObj;
                        WidgetCategoryInfoProvider.UpdateCategoryChildCount(0, wi.WidgetCategoryID);
                        WidgetCategoryInfoProvider.UpdateCategoryWidgetChildCount(0, wi.WidgetCategoryID);
                    }
                    break;

                case WebPartInfo.OBJECT_TYPE:
                    // ### Special case for web part - Refresh child objects count
                    {
                        WebPartInfo wpi = (WebPartInfo)infoObj;
                        WebPartCategoryInfoProvider.UpdateCategoryChildCount(0, wpi.WebPartCategoryID);
                        WebPartCategoryInfoProvider.UpdateCategoryWebPartChildCount(0, wpi.WebPartCategoryID);
                    }
                    break;

                case PredefinedObjectType.REPORT:
                    // ### Special case for report - Refresh child objects count
                    {
                        ModuleCommands.ReportingRefreshCategoryDataCount(infoObj);
                    }
                    break;

                case PredefinedObjectType.REPORTCATEGORY:
                    // ### Special case for report category - Refresh child objects count
                    {
                        ModuleCommands.ReportingRefreshCategoryDataCount(infoObj);
                    }
                    break;

                case DeviceProfileInfo.OBJECT_TYPE:
                    // ### Special case for device profiles - Re-init orders
                    {
                        DeviceProfileInfoProvider.InitProfilesOrder();
                    }
                    break;
            }

            #endregion
        }


        private static void PostProcessClass(DataClassInfo ci)
        {
            int inherits = ci.ClassInheritsFromClassID;

            // Ensure (update) the inheritance
            if (inherits > 0)
            {
                // Update the inherited fields
                var parentClass = DataClassInfoProvider.GetDataClassInfo(inherits);
                if (parentClass != null)
                {
                    FormHelper.UpdateInheritedClass(parentClass, ci);
                }
            }
            else
            {
                // Remove the inherited fields
                FormHelper.RemoveInheritance(ci, false);
            }

            // Generate default queries, if class already has the schema
            if (!string.IsNullOrEmpty(ci.ClassXmlSchema) || (ci.ClassIsDocumentType && !ci.ClassIsCoupledClass))
            {
                QueryInfoProvider.ClearDefaultQueries(ci, true, true);
            }

            // Update object structures
            ClassStructureInfo.Remove(ci.ClassName, true);
        }


        private static void PreserveMediaFileValues(GeneralizedInfo infoObj, GeneralizedInfo existing)
        {
            // Ensure media file deletion if name changed
            if (infoObj.TypeInfo.ObjectType == PredefinedObjectType.MEDIAFILE)
            {
                string existingFilePath = ValidationHelper.GetString(existing.GetValue("FilePath"), string.Empty).ToLowerInvariant();
                string newFilePath = ValidationHelper.GetString(infoObj.GetValue("FilePath"), string.Empty).ToLowerInvariant();

                // Name or extension has been changed or file has been moved - delete existing file
                if (existingFilePath != newFilePath)
                {
                    ModuleCommands.MediaLibraryDeleteMediaFile(existing.ObjectSiteID, existing.ObjectParentID, existingFilePath, true, false);
                    ModuleCommands.MediaLibraryDeleteMediaFilePreview(existing.ObjectSiteName, existing.ObjectParentID, existingFilePath, false);
                }
            }
        }


        /// <summary>
        /// Prepares site info for the import process by applying the existing values that should be preserved
        /// </summary>
        private void PrepareSiteInfo(SiteInfo si)
        {
            var existingSite = SiteInfoProvider.GetSiteInfo(SiteID);
            if (existingSite == null)
            {
                throw new InvalidOperationException("Target site not found.");
            }

            // Preserve values
            si.SiteID = existingSite.SiteID;
            si.DomainName = existingSite.DomainName;
            si.SiteName = existingSite.SiteName;
            si.Status = existingSite.Status;
            si.SiteGUID = existingSite.SiteGUID;
        }


        /// <summary>
        /// Copies data from source object to target.
        /// </summary>
        /// <param name="source">Source of the data</param>
        /// <param name="target">Target object where the data is copied to</param>
        private void CopyData(BaseInfo source, BaseInfo target)
        {
            var syncColumns = source.Generalized.GetSynchronizedColumns();
            foreach (var column in syncColumns)
            {
                target.SetValue(column, source.GetValue(column));
            }
        }


        /// <summary>
        /// Creates a site binding. (Site is specified by SyncManager.SiteID property.)
        /// </summary>
        /// <param name="bindingObj">Binding object to create.</param>
        /// <param name="boundObjectId">Identifier of bound object.</param>
        protected virtual void CreateSiteBinding(GeneralizedInfo bindingObj, int boundObjectId)
        {
            // Create site binding if the target site is set
            bindingObj.ObjectID = 0;

            var ti = bindingObj.TypeInfo;

            bindingObj.SetValue(ti.SiteIDColumn, SiteID);
            bindingObj.SetValue(ti.ParentIDColumn, boundObjectId);

            // Ensure complete object data
            bindingObj.MakeComplete(false);

            // Update / insert the site binding
            bindingObj.SetObject();
        }


        /// <summary>
        /// Deletes the object.
        /// </summary>
        /// <param name="objectDS">Object data</param>
        /// <param name="taskObjectType">Object type</param>
        protected virtual void DeleteObject(DataSet objectDS, string taskObjectType)
        {
            // Get the object definition
            GeneralizedInfo infoObj = ModuleManager.GetObject(taskObjectType);

            // Get the translation table
            DataTable transTable = objectDS.Tables[TranslationHelper.TRANSLATION_TABLE];
            TranslationHelper th = new TranslationHelper(transTable);

            // Get the object table
            var dt = GetTable(objectDS, infoObj);

            // Delete the objects within table
            foreach (DataRow objectRow in dt.Rows)
            {
                // Load the object
                GeneralizedInfo deleteObj = GetObject(objectRow, taskObjectType);

                // Delete the object
                // Init default columns
                if (deleteObj.ObjectSiteID > 0)
                {
                    deleteObj.ObjectSiteID = SiteID;
                }
                deleteObj.ObjectID = 0;

                if (ProceedWithTranslations(th))
                {
                    // Translate object columns
                    th.SetDefaultValue(PredefinedObjectType.USER, AdministratorId);
                    th.TranslateColumns(deleteObj, false, true, false, TranslationSiteID, null);
                    th.RemoveDefaultValue(PredefinedObjectType.USER);
                }

                // Get existing object
                BaseInfo existingObj = deleteObj.GetExisting();
                if (existingObj != null)
                {
                    using (CMSActionContext childContext = new CMSActionContext())
                    {
                        childContext.LogSynchronization = LogTasks;

                        // Staged objects should always be destroyed
                        childContext.CreateVersion = false;

                        #region "Special cases"

                        ProcessDeleteTaskSpecialCases(existingObj);

                        #endregion

                        // Delete the existing object
                        existingObj.Generalized.DeleteObject();
                    }
                }
            }
        }


        private void ProcessDeleteTaskSpecialCases(BaseInfo existingObj)
        {
            // ### Special case for scheduled task - make sure that incoming change is again logged to the next stage
            if (LogTasks && existingObj is Scheduler.TaskInfo)
            {
                existingObj.Generalized.LogSynchronization = SynchronizationTypeEnum.LogSynchronization;
            }

            // ## Special case - Do not delete group including documents (only object)
            if (existingObj.TypeInfo.ObjectType == PredefinedObjectType.GROUP)
            {
                existingObj.SetValue("GroupNodeGUID", Guid.Empty);
            }
        }


        /// <summary>
        /// Removes the objects from site.
        /// </summary>
        /// <param name="infoObj">Object type definition to process</param>
        /// <param name="ds">DataSet with the object data</param>
        /// <param name="th">Translation table</param>
        protected virtual void RemoveObjectsFromSite(GeneralizedInfo infoObj, DataSet ds, TranslationHelper th)
        {
            var ti = infoObj.TypeInfo;

            // Get the object data table
            var dt = GetTable(ds, infoObj);

            // Update the objects within table
            foreach (DataRow objectRow in dt.Rows)
            {
                // Load the object
                if (infoObj != null)
                {
                    infoObj = GetObject(objectRow, ti.ObjectType);
                }

                // Update the object
                if (infoObj != null)
                {
                    // Update the data
                    RemoveObjectFromSite(infoObj, ds, th);
                }
            }
        }


        /// <summary>
        /// Removes the object from site.
        /// </summary>
        /// <param name="infoObj">Object to update</param>
        /// <param name="ds">DataSet with the object data</param>
        /// <param name="th">Translation table</param>
        protected virtual void RemoveObjectFromSite(GeneralizedInfo infoObj, DataSet ds, TranslationHelper th)
        {
            int originalId = infoObj.ObjectID;

            // Init default columns
            if (infoObj.ObjectSiteID > 0)
            {
                infoObj.ObjectSiteID = SiteID;
            }
            infoObj.ObjectID = 0;

            string error = string.Empty;
            if (ProceedWithTranslations(th))
            {
                // Translate the object columns
                th.SetDefaultValue(PredefinedObjectType.USER, AdministratorId);
                error = th.TranslateColumns(infoObj, false, true, false, TranslationSiteID, null);
                th.RemoveDefaultValue(PredefinedObjectType.USER);
            }

            if (error != string.Empty)
            {
                throw new InvalidOperationException("Cannot translate columns '" + error + "', synchronize the dependent objects first.");
            }

            // Update ID if object exists
            GeneralizedInfo existing = infoObj.GetExisting();

            if (existing != null)
            {
                infoObj.ObjectID = existing.ObjectID;
            }
            else
            {
                // Remove for non-existing objects always succeeds
                return;
            }

            // Add ID translation
            var ti = infoObj.TypeInfo;

            if (ProceedWithTranslations(th) && (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
            {
                th.AddIDTranslation(ti.ObjectType, originalId, infoObj.ObjectID, infoObj.ObjectGroupID);
            }

            // Update bindings
            foreach (string bindingType in ti.BindingObjectTypes)
            {
                // Get the binding definition
                GeneralizedInfo bindingObj = ModuleManager.GetObject(bindingType);

                // Only process site bindings
                var bindingTypeInfo = bindingObj.TypeInfo;
                if (bindingTypeInfo.IsSiteBinding)
                {
                    // Check column definition
                    if (bindingTypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        throw new InvalidOperationException("Parent ID column for object type '" + bindingType + "' is not defined.");
                    }

                    // Create site binding if the target site is set
                    if (SiteID > 0)
                    {
                        // Set the binding
                        bindingObj.SetValue(bindingTypeInfo.SiteIDColumn, SiteID);
                        bindingObj.SetValue(bindingTypeInfo.ParentIDColumn, infoObj.ObjectID);

                        // Get existing binding
                        if (bindingTypeInfo.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                        {
                            bindingObj = bindingObj.GetExisting();
                        }

                        if (bindingObj != null)
                        {
                            using (CMSActionContext context = new CMSActionContext())
                            {
                                context.LogSynchronization = LogTasks;

                                // Remove object from site
                                bindingObj.DeleteObject();
                            }
                        }
                    }
                    // Do not continue - there should be always only one site binding
                    break;
                }
            }
        }


        /// <summary>
        /// Removes the object from the site.
        /// </summary>
        /// <param name="objectDS">Object data</param>
        /// <param name="taskObjectType">Task object type</param>
        /// <param name="th">Translation helper</param>
        protected virtual void RemoveObjectFromSite(DataSet objectDS, string taskObjectType, TranslationHelper th)
        {
            // Get the object definition
            GeneralizedInfo infoObj = ModuleManager.GetObject(taskObjectType);
            if (infoObj != null)
            {
                // Get the translation table
                if (th == null)
                {
                    DataTable transTable = objectDS.Tables[TranslationHelper.TRANSLATION_TABLE];
                    th = new TranslationHelper(transTable);
                }

                // Remove objects from site
                RemoveObjectsFromSite(infoObj, objectDS, th);
            }
        }

        #endregion


        #region "Document methods"

        /// <summary>
        /// Updates the document.
        /// </summary>
        /// <param name="documentDS">Document data</param>
        /// <param name="safeClassName">Class name of document in safe format</param>
        /// <param name="processChildren">Indicates if also child objects should be processed</param>
        protected virtual void UpdateDocument(DataSet documentDS, string safeClassName, bool processChildren)
        {
            // Get the document table
            DataTable documentTable = documentDS.Tables[safeClassName];
            if (documentTable == null)
            {
                throw new InvalidOperationException("Page data table not found.");
            }

            // Get the translation table
            DataTable transTable = documentDS.Tables[TranslationHelper.TRANSLATION_TABLE];

            if (transTable == null)
            {
                throw new InvalidOperationException("Object translation table not found.");
            }
            TranslationHelper th = new TranslationHelper(transTable);

            // Update page template
            if (HasObjectTable(PageTemplateInfo.OBJECT_TYPE, documentDS))
            {
                UpdateObject(documentDS, null, PageTemplateInfo.OBJECT_TYPE, th, true, false);
            }

            // Update SKU
            if (HasObjectTable(PredefinedObjectType.SKU, documentDS) && ModuleEntryManager.IsModuleLoaded(ModuleName.ECOMMERCE))
            {
                UpdateObject(documentDS, null, PredefinedObjectType.SKU, th, true, false);
            }

            // Update Group
            if (HasObjectTable(PredefinedObjectType.GROUP, documentDS) && ModuleEntryManager.IsModuleLoaded(ModuleName.COMMUNITY))
            {
                UpdateObject(documentDS, null, PredefinedObjectType.GROUP, th, true, false);
            }

            // Update Tag group
            if (HasObjectTable(TagGroupInfo.OBJECT_TYPE, documentDS))
            {
                UpdateObject(documentDS, null, TagGroupInfo.OBJECT_TYPE, th, true, false);
            }

            // Update the documents within table
            TreeNode node = null;

            foreach (DataRow nodeRow in documentTable.Rows)
            {
                // Indicates if document is inserted
                bool insertDocument = false;

                // Get the document class
                int classId = DataHelper.GetIntValue(nodeRow, "NodeClassID");
                string className = th.GetCodeName(DataClassInfo.OBJECT_TYPE, classId);

                if (string.IsNullOrEmpty(className) && (OperationType == OperationTypeEnum.Integration))
                {
                    className = DataClassInfoProvider.GetClassName(ValidationHelper.GetInteger(nodeRow["NodeClassID"], 0));
                }

                var ci = DataClassInfoProvider.GetDataClassInfo(className);
                if (ci == null)
                {
                    // Class not found
                    throw new InvalidOperationException("Class name '" + className + "' not found.");
                }

                // Create the document node
                node = TreeNode.New(className, nodeRow, TreeProvider);

                //Check if target culture exists
                if (!CultureSiteInfoProvider.IsCultureOnSite(node.DocumentCulture, SiteName))
                {
                    throw new InvalidOperationException("This page can not be synchronized because '" + node.DocumentCulture + "' culture is not assigned to the target site.");
                }

                // Force preferred culture to document culture
                TreeProvider.PreferredCultureCode = node.DocumentCulture;

                int nodeId = node.NodeID;
                int parentNodeId = node.NodeParentID;
                int documentId = node.DocumentID;
                int checkedOutVersionHistoryId = node.DocumentCheckedOutVersionHistoryID;
                bool wasPublished = node.IsPublished;

                if (OperationType != OperationTypeEnum.Integration)
                {
                    // Remove excluded column values
                    RemoveExcludedValues(node);
                }

                // Translate dependent columns
                TranslateNodeColumns(node, th);

                // Get current document
                var currentNode = GetExistingDocument(node.NodeAliasPath, node.NodeGUID);

                // Node not exists, insert new node
                if (currentNode == null)
                {
                    // Make complete data
                    node.MakeComplete(false);

                    // Get node parent
                    string parentAliasPath = TreePathUtils.GetParentPath(node.NodeAliasPath);
                    TreeNode parentNode = TreeProvider.SelectSingleNode(SiteName, parentAliasPath, TreeProvider.ALL_CULTURES, false, null, false);

                    // Parent not found using node alias path. Try to get parent by it's GUID - it could be moved to different location
                    if ((parentNode == null) && (parentNodeId > 0))
                    {
                        var parentGuid = ValidationHelper.GetGuid(th.GetCodeName(DocumentNodeDataInfo.OBJECT_TYPE, parentNodeId), Guid.Empty);
                        if (parentGuid != Guid.Empty)
                        {
                            parentNode = TreeProvider.SelectSingleNode(parentGuid, TreeProvider.ALL_CULTURES, SiteName);
                        }
                    }

                    bool root = (node.NodeAliasPath == "/");
                    if (parentNode == null)
                    {
                        if ((OperationType == OperationTypeEnum.Integration) && root)
                        {
                            TreeProvider.CreateSiteRoot(SiteName);
                            parentNode = TreeProvider.SelectSingleNode(SiteName, parentAliasPath, TreeProvider.ALL_CULTURES, false, null, false);
                            DocumentHelper.CopyNodeData(node, parentNode, new CopyNodeDataSettings(true, null) { CopySKUData = false });
                            node.Update();
                        }
                        else
                        {
                            // Parent not found
                            throw new InvalidOperationException("Parent node does not exist, please synchronize parent node first.");
                        }
                    }

                    // Insert the document
                    if (node.IsLink)
                    {
                        node.InsertAsLink(parentNode);

                        // Get original document node to get the document ID
                        TreeNode originalNode = TreeProvider.GetOriginalNode(node);
                        if (originalNode != null)
                        {
                            node.SetValue("DocumentID", originalNode.DocumentID);
                        }
                    }
                    else
                    {
                        if (!root)
                        {
                            node.Insert(parentNode);
                        }
                    }

                    // Ensure version for document under workflow
                    var workflow = WorkflowManager.GetNodeWorkflow(node);
                    if (workflow != null)
                    {
                        VersionManager.EnsureVersion(node, false);

                        // Automatically publish if the document was published on the source server or is under versioning workflow ('Automatically publish changes' setting is applied) and does not use Check-in/out
                        if (wasPublished || (workflow.WorkflowAutoPublishChanges && !workflow.UseCheckInCheckOut(node.NodeSiteName)))
                        {
                            node.MoveToPublishedStep();
                        }
                    }

                    insertDocument = true;
                }
                else
                {
                    ICollection<string> excludeColumns = null;
                    if (OperationType != OperationTypeEnum.Integration)
                    {
                        // Copy the original column collection since it's going to be modified
                        excludeColumns = (TreeNode.TYPEINFO.SynchronizationSettings.ExcludedStagingColumns ?? Enumerable.Empty<string>()).ToList();

                        if (node.IsCoupled)
                        {
                            // Exclude also the primary key of the coupled table
                            excludeColumns.Add(node.CoupledClassIDColumn);
                        }
                    }

                    // Get current document culture
                    var cultureNode = TreeProvider.SelectSingleNode(SiteName, null, node.DocumentCulture, false, className, "NodeGUID='" + currentNode.NodeGUID + "'", null, -1, false);
                    if (cultureNode != null)
                    {
                        // Propagate tree provider to culture node
                        cultureNode.TreeProvider = TreeProvider;

                        // Clear workflow information if the node is not under workflow anymore
                        if (checkedOutVersionHistoryId == 0)
                        {
                            DocumentHelper.ClearWorkflowInformation(cultureNode);
                        }

                        // Document culture exists, update
                        DocumentHelper.CopyNodeData(node, cultureNode, new CopyNodeDataSettings(true, excludeColumns) { CopySKUData = false });

                        cultureNode.Update();
                        node = cultureNode;

                        // Automatically publish if the document is under versioning workflow ('Automatically publish changes' setting is applied) and does not use Check-in/out
                        var workflow = node.GetWorkflow();
                        if (workflow != null)
                        {
                            if (workflow.WorkflowAutoPublishChanges && !workflow.UseCheckInCheckOut(node.NodeSiteName))
                            {
                                node.MoveToPublishedStep();
                            }
                        }
                    }
                    else
                    {
                        // Document culture not exists, insert new culture version
                        var copySettings = new CopyNodeDataSettings(true, excludeColumns)
                        {
                            CopySKUData = false,
                            ResetChanges = true
                        };

                        DocumentHelper.CopyNodeData(node, currentNode, copySettings);

                        if (checkedOutVersionHistoryId == 0)
                        {
                            // Clear workflow data
                            DocumentHelper.ClearWorkflowInformation(currentNode);
                        }

                        var settings = new NewCultureDocumentSettings(currentNode, node.DocumentCulture, TreeProvider)
                        {
                            // Do not clear attachment fields, since the attachments are synchronized within the task as well
                            ClearAttachmentFields = false,
                            // Do not lock document after the synchronization
                            AllowCheckOut = false
                        };

                        DocumentHelper.InsertNewCultureVersion(settings);

                        node = currentNode;
                    }
                }

                // Register translation records
                th.AddIDTranslation(PredefinedObjectType.NODE, nodeId, node.NodeID, 0);
                th.AddIDTranslation(TreeNode.OBJECT_TYPE, documentId, node.DocumentID, 0);
                th.AddIDTranslation(DocumentCultureDataInfo.OBJECT_TYPE, documentId, node.DocumentID, 0);

                bool updateIndexForAllCultures = false;

                if (processChildren)
                {
                    new AttachmentSynchronizationManager(node, th).Synchronize(documentDS, documentId);

                    UpdateWidgetPersonalizationVariants(documentDS, currentNode, th);

                    // Get relationships table
                    DataTable relationshipsTable = documentDS.Tables["CMS_Relationship"];
                    // Update relationships
                    UpdateRelationships(node, relationshipsTable);

                    // Get categories table
                    DataTable categoriesTable = documentDS.Tables["CMS_DocumentCategory"];
                    // Update categories
                    UpdateCategories(node, categoriesTable, th);

                    // Get aliases table
                    DataTable aliasesTable = documentDS.Tables["CMS_DocumentAlias"];

                    updateIndexForAllCultures = !DataHelper.DataSourceIsEmpty(aliasesTable);

                    // Update document aliases and do not create search tasks
                    UpdateAliases(node, aliasesTable, th);

                    // Update MVT variants and combinations
                    if (HasObjectTable(PredefinedObjectType.DOCUMENTMVTVARIANT, documentDS) && ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING))
                    {
                        UpdateObject(documentDS, null, PredefinedObjectType.DOCUMENTMVTVARIANT, th, true, false);
                    }
                    if (HasObjectTable(PredefinedObjectType.DOCUMENTMVTCOMBINATION, documentDS) && ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING))
                    {
                        UpdateObject(documentDS, null, PredefinedObjectType.DOCUMENTMVTCOMBINATION, th, true, false);
                    }
                }

                // Update message boards if module loaded
                if (HasObjectTable(PredefinedObjectType.BOARD, documentDS) && ModuleEntryManager.IsModuleLoaded(ModuleName.MESSAGEBOARD))
                {
                    UpdateObject(documentDS, null, PredefinedObjectType.BOARD, th, true, false);
                }

                // Log the update task for next stage
                if (LogTasks)
                {
                    DocumentSynchronizationHelper.LogDocumentChange(node, insertDocument ? TaskTypeEnum.CreateDocument : TaskTypeEnum.UpdateDocument, TreeProvider);
                }

                // Update search index for node
                if (DocumentHelper.IsSearchTaskCreationAllowed(node))
                {
                    if (updateIndexForAllCultures)
                    {
                        // If document aliases were processed all culture versions of the document should be rebuilded in the index
                        var cultureVersionsData = DocumentCultureDataInfoProvider.GetDocumentCultures().WhereEquals("DocumentNodeID", node.OriginalNodeID);
                        foreach (var cultureVersion in cultureVersionsData)
                        {
                            SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, PredefinedObjectType.DOCUMENT, SearchFieldsConstants.ID, node.GetSearchID(), cultureVersion.DocumentID);
                        }
                    }
                    else
                    {
                        SearchTaskInfoProvider.CreateTask(SearchTaskTypeEnum.Update, PredefinedObjectType.DOCUMENT, SearchFieldsConstants.ID, node.GetSearchID(), node.DocumentID);
                    }
                }
            }

            // Update document ACLs
            if (node != null)
            {
                UpdateACLs(node, documentDS, th);
            }
        }


        /// <summary>
        /// Updates widget personalization variants.
        /// </summary>
        /// <remarks>
        /// Compares existing document personalization variants given as children of <paramref name="currentNode"/> with the staged ones obtained from
        /// <paramref name="documentDS"/> and removes those no longer existing in <paramref name="documentDS"/>.
        /// Updating and creating new variants is performed using <see cref="UpdateObject(DataSet, DataSet, string, TranslationHelper, bool, bool)"/> method.
        /// </remarks>
        /// <param name="documentDS">Dataset containing currently staged document</param>
        /// <param name="currentNode">Refers to current node corresponding to the document given in <paramref name="documentDS"/></param>
        /// <param name="translationHelper">Class to provide objects translation interface ID - CodeName</param>
        private void UpdateWidgetPersonalizationVariants(DataSet documentDS, TreeNode currentNode, TranslationHelper translationHelper)
        {
            if (ModuleEntryManager.IsModuleLoaded(ModuleName.ONLINEMARKETING) && LicenseHelper.CheckFeature(RequestContext.CurrentDomain, FeatureEnum.ContentPersonalization))
            {
                if (currentNode != null)
                {
                    var stagedPersonalizationVariantGUIDs = DataHelper
                    .GetValues<Guid>(
                        ObjectHelper.GetTable(
                            documentDS,
                            ModuleManager.GetObject(PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT)
                        ),
                        "VariantGUID"
                    );

                    var existingPersonalizationVariantGUIDs = currentNode
                        .Generalized
                        .Children[PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT]
                        .ToList();

                    var personalizationVariantsToBeDeleted = existingPersonalizationVariantGUIDs
                        .Where(variant => !stagedPersonalizationVariantGUIDs.Contains(variant.Generalized.ObjectGUID))
                        .ToList();

                    personalizationVariantsToBeDeleted.ForEach(variant => variant.Delete());
                }

                // Update Content personalization variants
                if (HasObjectTable(PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT, documentDS))
                {
                    UpdateObject(documentDS, null, PredefinedObjectType.DOCUMENTCONTENTPERSONALIZATIONVARIANT, translationHelper, true, false);
                }
            }
        }


        /// <summary>
        /// Publishes the document.
        /// </summary>
        /// <param name="documentDS">Document data</param>
        /// <param name="safeClassName">Class name of document in safe format</param>
        protected virtual void PublishDocument(DataSet documentDS, string safeClassName)
        {
            // Get the document table
            DataTable documentTable = documentDS.Tables[safeClassName];
            if (documentTable == null)
            {
                throw new InvalidOperationException("Page data table not found.");
            }

            // Get Version history data table
            DataTable versionHistoryTable = documentDS.Tables["CMS_VersionHistory"];

            // Get Attachment history table
            DataTable attachmentHistoryTable = documentDS.Tables["CMS_AttachmentHistory"];

            // Get the translation table
            DataTable transTable = documentDS.Tables[TranslationHelper.TRANSLATION_TABLE];
            if (transTable == null)
            {
                throw new InvalidOperationException("Object translation table not found.");
            }
            TranslationHelper th = new TranslationHelper(transTable);

            // Publish the document versions
            foreach (DataRow nodeRow in documentTable.Rows)
            {
                // Get node GUID
                Guid nodeGuid = DataHelper.GetGuidValue(nodeRow, "NodeGUID", Guid.Empty);
                if (nodeGuid == Guid.Empty)
                {
                    throw new InvalidOperationException("Node GUID not specified.");
                }

                // Get document culture
                string culture = DataHelper.GetStringValue(nodeRow, "DocumentCulture");
                if (culture == string.Empty)
                {
                    throw new InvalidOperationException("Page culture not specified.");
                }

                // Get current node by GUID
                TreeNode currentNode = TreeProvider.SelectSingleNode(nodeGuid, culture, SiteName);
                if (currentNode == null)
                {
                    throw new InvalidOperationException("Page node not found, please synchronize the page node first.");
                }

                // Create the document node
                TreeNode node = TreeNode.New(currentNode.NodeClassName, nodeRow, TreeProvider);
                // Force prefered culture to document culture
                TreeProvider.PreferredCultureCode = node.DocumentCulture;

                int documentId = node.DocumentID;

                // This needs to be called before RemoveExcludedColumns, since WFStepID is excluded
                int workflowStepId = node.DocumentWorkflowStepID;

                // Remove excluded column values
                RemoveExcludedValues(node);

                // Translate dependent columns
                TranslateNodeColumns(node, th);

                if (versionHistoryTable == null)
                {
                    continue;
                }

                // Get the version data
                DataRow[] versions = versionHistoryTable.Select("DocumentID = " + documentId);
                if (versions.Length <= 0)
                {
                    continue;
                }

                // Save all the versions
                foreach (DataRow versionRow in versions)
                {
                    // Update current document with not versioned columns
                    DocumentHelper.CopyNodeData(node, currentNode, new CopyNodeDataSettings(true, TreeNode.TYPEINFO.SynchronizationSettings.ExcludedStagingColumns)
                    {
                        CopyCoupledData = false,
                        CopyVersionedData = false,
                        CopySKUData = false
                    });
                    currentNode.Update();

                    // Clear the document version history queue
                    VersionManager.DeleteOlderVersions(currentNode.DocumentID, SiteName);

                    // Delete version for document in edit step
                    if (currentNode.WorkflowStepType == WorkflowStepTypeEnum.DocumentEdit)
                    {
                        VersionManager.DestroyDocumentVersion(currentNode.DocumentCheckedOutVersionHistoryID);
                        // The CanBePublished flag is updated by stored procedure, but not in the instance
                        currentNode = TreeProvider.SelectSingleNode(currentNode.NodeGUID, currentNode.DocumentCulture, SiteName);
                    }

                    // Insert the version object
                    var version = new VersionHistoryInfo(versionRow)
                    {
                        VersionHistoryID = 0,
                        DocumentID = currentNode.DocumentID,
                        NodeSiteID = currentNode.OriginalNodeSiteID,
                        ModifiedByUserID = node.GetValue("DocumentModifiedByUserID", 0),
                        ToBePublished = true,
                        WasPublishedFrom = DateTimeHelper.ZERO_TIME,
                        WasPublishedTo = DateTimeHelper.ZERO_TIME,
                        VersionWorkflowID = 0,
                        VersionWorkflowStepID = 0,
                        VersionClassID = node.GetValue("NodeClassID", 0)
                    };

                    // Set data
                    // Do not load missing data from the database since the node instance is constructed based on complete data of the staging task 
                    // where ID columns are not translated to current ones.
                    version.SetData(node, false);

                    // Set current step to published
                    WorkflowInfo wi = WorkflowManager.GetNodeWorkflow(currentNode);
                    WorkflowStepInfo currentStep = null;
                    WorkflowStepInfo targetStep = null;
                    if (wi != null)
                    {
                        // Get current step info, do not update document
                        if (!currentNode.IsPublished || (currentNode.DocumentCheckedOutVersionHistoryID > 0))
                        {
                            currentStep = WorkflowManager.GetStepInfo(currentNode);
                        }

                        // Get target step (first try to translate the original publish step - this has to be done, since in advanced WF more than one publish step can exist and 
                        // the document should move to correct one)
                        var stepName = th.GetCodeName(WorkflowStepInfo.OBJECT_TYPE, workflowStepId);
                        targetStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepName, wi.WorkflowID) ?? WorkflowStepInfoProvider.GetPublishedStep(wi.WorkflowID);

                        if (targetStep != null)
                        {
                            currentNode.SetValue("DocumentLastPublished", DateTime.Now);
                            currentNode.SetValue("DocumentWorkflowStepID", targetStep.StepID);
                            currentNode.SetValue("DocumentIsArchived", false);
                            version.VersionWorkflowID = targetStep.StepWorkflowID;
                            version.VersionWorkflowStepID = targetStep.StepID;
                        }
                    }

                    VersionHistoryInfoProvider.SetVersionHistoryInfo(version);
                    int newVersionHistoryId = version.VersionHistoryID;

                    // Update the node data
                    currentNode.SetValue("DocumentCheckedOutVersionHistoryID", newVersionHistoryId);
                    TreeProvider.ClearCheckoutInformation(currentNode);
                    currentNode.Update();

                    // Get the version attachments
                    if (attachmentHistoryTable != null)
                    {
                        var mainWhere = new WhereCondition()
                          .WhereEquals("AttachmentDocumentID", documentId)
                          .WhereNull("AttachmentVariantParentID");

                        var variantsWhere = new WhereCondition()
                            .WhereEquals("AttachmentDocumentID", documentId)
                            .WhereNotNull("AttachmentVariantParentID");

                        var mainAttachments = attachmentHistoryTable.Select(mainWhere.ToString(true));
                        var attachmentVariants = attachmentHistoryTable.Select(variantsWhere.ToString(true));

                        // key is sn oldID
                        var attachmentsTranslationTable = new Dictionary<int, int>();

                        foreach (DataRow mainAttachment in mainAttachments)
                        {
                            var attachment = new DocumentAttachment(mainAttachment);
                            attachment.AttachmentDocumentID = currentNode.DocumentID;
                            attachment.AttachmentSiteID = currentNode.OriginalNodeSiteID;

                            VersionManager.SaveAttachmentVersion(attachment, newVersionHistoryId);

                            attachmentsTranslationTable.Add(
                                ValidationHelper.GetInteger(mainAttachment["AttachmentHistoryID"], 0),
                                attachment.AttachmentHistoryID);
                        }

                        foreach (DataRow attachmentVariant in attachmentVariants)
                        {
                            var history = new AttachmentHistoryInfo(attachmentVariant);

                            int newParentId;
                            attachmentsTranslationTable.TryGetValue(history.AttachmentVariantParentID, out newParentId);

                            if (newParentId != 0)
                            {
                                history.AttachmentVariantParentID = newParentId;
                                history.AttachmentSiteID = currentNode.OriginalNodeSiteID;
                                history.AttachmentHistoryID = 0;

                                AttachmentHistoryInfoProvider.SetAttachmentHistory(history);
                            }
                            else
                            {
                                EventLogProvider.LogInformation(
                                    "SyncManager",
                                    "AttachmentHistorySynchronization",
                                    String.Format("Can't find AttachmentVariantParentID for oldId = {0}", history.AttachmentVariantParentID));
                            }

                        }
                    }

                    // Get published document to log synchronization task
                    TreeNode publishedNode = VersionManager.GetVersion(newVersionHistoryId, currentNode);

                    // If the document is scheduled for now, publish it immediately
                    if (version.PublishFrom <= DateTime.Now)
                    {
                        VersionManager.PublishVersion(newVersionHistoryId, false);
                        currentNode.SetValue("DocumentPublishedVersionHistoryID", newVersionHistoryId);
                    }

                    // Get document scope
                    var scope = WorkflowManager.GetNodeWorkflowScope(currentNode);

                    // If workflow scope not defined anymore, convert the document to not versioned
                    if (scope == null)
                    {
                        // Remove workflow
                        VersionManager.RemoveWorkflow(currentNode);

                        // Do not log workflow history
                        targetStep = null;
                    }

                    // Log synchronization for the next stage
                    if (publishedNode != null)
                    {
                        if (LogTasks)
                        {
                            using (new CMSActionContext { EnableLogContext = false })
                            {
                                DocumentSynchronizationHelper.LogDocumentChange(publishedNode, TaskTypeEnum.PublishDocument, TreeProvider);
                            }
                        }
                    }

                    // Log the history
                    if ((targetStep != null) && (currentStep != null))
                    {
                        // Prepare log settings
                        WorkflowLogSettings settings = new WorkflowLogSettings(PredefinedObjectType.DOCUMENT, currentNode.DocumentID)
                        {
                            VersionHistoryId = newVersionHistoryId,
                            User = AdministratorUser,
                            SourceStep = currentStep,
                            TargetStep = targetStep
                        };

                        // Log the history
                        WorkflowManager.LogWorkflowHistory(settings);
                    }
                }
            }
        }


        /// <summary>
        /// Archives the document.
        /// </summary>
        /// <param name="documentDS">Document data</param>
        /// <param name="safeClassName">Class name of document in safe format</param>
        protected virtual void ArchiveDocument(DataSet documentDS, string safeClassName)
        {
            // Get the document table
            DataTable documentTable = documentDS.Tables[safeClassName];
            if (documentTable == null)
            {
                throw new InvalidOperationException("Page data table not found.");
            }

            // Publish the document versions
            foreach (DataRow nodeRow in documentTable.Rows)
            {
                // Get node GUID
                Guid nodeGuid = DataHelper.GetGuidValue(nodeRow, "NodeGUID", Guid.Empty);
                if (nodeGuid == Guid.Empty)
                {
                    throw new InvalidOperationException("Node GUID not specified.");
                }

                // Get document culture
                string culture = DataHelper.GetStringValue(nodeRow, "DocumentCulture");
                if (culture == string.Empty)
                {
                    throw new InvalidOperationException("Page culture not specified.");
                }

                // Get current node by GUID
                TreeNode currentNode = TreeProvider.SelectSingleNode(nodeGuid, culture, SiteName);
                if (currentNode == null)
                {
                    throw new InvalidOperationException("Page node not found, please synchronize the page node first.");
                }

                // Get document workflow
                WorkflowInfo workflow = WorkflowManager.GetNodeWorkflow(currentNode);
                if (workflow != null)
                {
                    WorkflowStepInfo targetStep = null;

                    // Get the translation table
                    DataTable transTable = documentDS.Tables[TranslationHelper.TRANSLATION_TABLE];
                    if (transTable != null)
                    {
                        TranslationHelper th = new TranslationHelper(transTable);

                        // Get target step (first try to translate the original archive step - this has to be done, since in advanced WF more than one archive step can exist and 
                        // the document should move to correct one)
                        var stepName = th.GetCodeName(WorkflowStepInfo.OBJECT_TYPE, ValidationHelper.GetInteger(nodeRow["DocumentWorkflowStepID"], 0));
                        targetStep = WorkflowStepInfoProvider.GetWorkflowStepInfo(stepName, workflow.WorkflowID);
                    }

                    // Archive the document
                    if (targetStep != null)
                    {
                        WorkflowManager.MoveToSpecificStep(currentNode, targetStep);
                    }
                    else
                    {
                        WorkflowManager.ArchiveDocument(currentNode);
                    }

                    if (LogTasks)
                    {
                        using (new CMSActionContext { EnableLogContext = false })
                        {
                            DocumentSynchronizationHelper.LogDocumentChange(currentNode, TaskTypeEnum.ArchiveDocument, TreeProvider);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Rejects the document.
        /// </summary>
        /// <param name="documentDS">Document data</param>
        /// <param name="safeClassName">Class name of document in safe format</param>
        protected virtual void RejectDocument(DataSet documentDS, string safeClassName)
        {
            // Get the document table
            DataTable documentTable = documentDS.Tables[safeClassName];
            if (documentTable == null)
            {
                throw new InvalidOperationException("Page data table not found.");
            }

            // Reject the document versions
            foreach (DataRow nodeRow in documentTable.Rows)
            {
                // Get node GUID
                Guid nodeGuid = DataHelper.GetGuidValue(nodeRow, "NodeGUID", Guid.Empty);
                if (nodeGuid == Guid.Empty)
                {
                    throw new InvalidOperationException("Node GUID not specified.");
                }

                // Get document culture
                string culture = DataHelper.GetStringValue(nodeRow, "DocumentCulture");
                if (culture == string.Empty)
                {
                    throw new InvalidOperationException("Page culture not specified.");
                }

                // Get current node by GUID
                TreeNode currentNode = TreeProvider.SelectSingleNode(nodeGuid, culture, SiteName);
                if (currentNode == null)
                {
                    throw new InvalidOperationException("Page node not found, please synchronize the page node first.");
                }

                // Get document workflow
                WorkflowInfo workflow = WorkflowManager.GetNodeWorkflow(currentNode);
                if (workflow != null)
                {
                    // Reject the document
                    WorkflowManager.MoveToPreviousStep(currentNode);

                    if (LogTasks)
                    {
                        using (new CMSActionContext { EnableLogContext = false })
                        {
                            DocumentSynchronizationHelper.LogDocumentChange(currentNode, TaskTypeEnum.RejectDocument, TreeProvider);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Deletes the document.
        /// </summary>
        /// <param name="documentDS">Document data</param>
        /// <param name="deleteAllCultures">Delete all culture versions</param>
        /// <param name="safeClassName">Class name of document in safe format</param>
        protected virtual void DeleteDocument(DataSet documentDS, bool deleteAllCultures, string safeClassName)
        {
            // Get the document table
            DataTable documentTable = documentDS.Tables[safeClassName];
            if (documentTable == null)
            {
                throw new InvalidOperationException("Page data table not found.");
            }

            // Delete the documents within table
            foreach (DataRow nodeRow in documentTable.Rows)
            {
                // Get node GUID
                Guid nodeGuid = DataHelper.GetGuidValue(nodeRow, "NodeGUID", Guid.Empty);
                if (nodeGuid == Guid.Empty)
                {
                    throw new InvalidOperationException("Node GUID not specified.");
                }

                // Get document culture
                string culture = DataHelper.GetStringValue(nodeRow, "DocumentCulture");
                if (culture == string.Empty)
                {
                    throw new InvalidOperationException("Page culture not specified.");
                }

                // Get node which will be deleted
                var node = TreeProvider.SelectSingleNode(nodeGuid, TreeProvider.ALL_CULTURES, SiteName);
                if (node != null)
                {
                    if (deleteAllCultures)
                    {
                        DocumentHelper.DeleteDocument(node, TreeProvider, true, true);
                    }
                    else
                    {
                        // Get node for the specific document culture
                        node = TreeProvider.SelectSingleNode(SiteName, null, culture, false, node.NodeClassName, "NodeGUID='" + node.NodeGUID + "'", null, -1, false);
                        if (node != null)
                        {
                            // Propagate tree provider to culture node
                            node.TreeProvider = TreeProvider;

                            // Delete document
                            DocumentHelper.DeleteDocument(node, TreeProvider, false, true);
                        }
                    }

                    // Log the update task for next stage
                    if (LogTasks)
                    {
                        DocumentSynchronizationHelper.LogDocumentChange(node, TaskTypeEnum.DeleteDocument, TreeProvider);
                    }
                }
            }
        }


        /// <summary>
        /// Moves the document.
        /// </summary>
        /// <param name="documentDS">Document data</param>
        /// <param name="safeClassName">Class name of document in safe format</param>
        protected virtual void MoveDocument(DataSet documentDS, string safeClassName)
        {
            // Get the document table
            DataTable documentTable = documentDS.Tables[safeClassName];
            if (documentTable == null)
            {
                throw new InvalidOperationException("Page data table not found.");
            }

            // Move the documents within table
            foreach (DataRow nodeRow in documentTable.Rows)
            {
                // Get node GUID
                Guid nodeGuid = DataHelper.GetGuidValue(nodeRow, "NodeGUID", Guid.Empty);
                if (nodeGuid == Guid.Empty)
                {
                    throw new InvalidOperationException("Node GUID not specified.");
                }

                // Get alias path
                string aliasPath = DataHelper.GetStringValue(nodeRow, "NodeAliasPath");

                var currentNode = GetExistingDocument(aliasPath, nodeGuid);
                if (currentNode != null)
                {
                    // Get target parent node
                    string parentAliasPath = TreePathUtils.GetParentPath(aliasPath);
                    TreeNode parentNode = TreeProvider.SelectSingleNode(SiteName, parentAliasPath, TreeProvider.ALL_CULTURES, false, null, false);
                    if (parentNode == null)
                    {
                        // Parent not found
                        throw new InvalidOperationException("The target location doesn't exist on the target server. Please synchronize the page node for the target location first.");
                    }

                    // Check if permission should be copied
                    bool copyPermissions = false;
                    DataTable taskParamsTable = documentDS.Tables["TaskParameters"];
                    if (taskParamsTable != null)
                    {
                        TaskParameters taskParams = new TaskParameters(taskParamsTable);
                        copyPermissions = ValidationHelper.GetBoolean(taskParams.GetParameter("copyPermissions"), false);
                    }

                    // Move the document
                    DocumentHelper.MoveDocument(currentNode, parentNode, TreeProvider, copyPermissions);

                    // Log the update task for next stage
                    if (LogTasks)
                    {
                        // Create task parameters
                        TaskParameters taskParams = null;

                        if (copyPermissions)
                        {
                            taskParams = new TaskParameters();
                            taskParams.SetParameter("copyPermissions", true);
                        }

                        DocumentSynchronizationHelper.LogDocumentChange(currentNode, TaskTypeEnum.MoveDocument, TreeProvider, SynchronizationInfoProvider.ENABLED_SERVERS, taskParams, true);
                    }
                }
                else
                {
                    throw new InvalidOperationException("Page node not found, please synchronize the page node first.");
                }
            }
        }


        /// <summary>
        /// Gets existing document
        /// </summary>
        /// <param name="nodeAliasPath">Node alias path</param>
        /// <param name="nodeGuid">Node GUID</param>
        private TreeNode GetExistingDocument(string nodeAliasPath, Guid nodeGuid)
        {
            // Root needs special handling
            if (nodeAliasPath == "/")
            {
                // Get current root node
                return TreeProvider.SelectSingleNode(SiteName, "/", TreeProvider.ALL_CULTURES);
            }

            // Get all candidates matching either NodeGUID or node alias path, only one from each site culture
            var candidates = TreeProvider.SelectNodes()
                                         .OnSite(SiteName)
                                         .Culture(CultureSiteInfoProvider.GetSiteCultureCodes(SiteName).ToArray())
                                         .CombineWithDefaultCulture()
                                         .Published(false)
                                         .Where(new WhereCondition().WhereEquals("NodeGUID", nodeGuid).Or().WhereEquals("NodeAliasPath", nodeAliasPath));

            var document = candidates.FirstOrDefault();

            // Check if there is just one existing document which matches the original one from the source server
            if ((document != null) && ((candidates.Count > 1) || (document.NodeGUID != nodeGuid)))
            {
                throw new NotSupportedException("There is an existing page on the target server in the same location which doesn't originate from the source server. This scenario is not supported for the staging process. Move the page to a different location or delete it on the target server.");
            }

            return document;
        }


        /// <summary>
        /// Updates the relationships.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="relationshipsTable">Relationships table</param>
        protected virtual void UpdateRelationships(TreeNode node, DataTable relationshipsTable)
        {
            // New version of document does not contain any relationships -> delete all
            if (relationshipsTable == null)
            {
                RelationshipInfoProvider.DeleteRelationships(node.NodeID, true, true);
                return;
            }

            // Get current relationships
            DataSet currentRelationshipsDS = RelationshipInfoProvider.GetRelationships(node.NodeID);
            DataTable currentRelationshipsTable = null;
            if (!DataHelper.DataSourceIsEmpty(currentRelationshipsDS))
            {
                currentRelationshipsTable = currentRelationshipsDS.Tables[0];
            }

            // Process all the relationships
            foreach (DataRow relationshipRow in relationshipsTable.Rows)
            {
                Guid leftNodeGUID = DataHelper.GetGuidValue(relationshipRow, "LeftNodeGUID", Guid.Empty);
                Guid rightNodeGUID = DataHelper.GetGuidValue(relationshipRow, "RightNodeGUID", Guid.Empty);
                string relationshipName = DataHelper.GetStringValue(relationshipRow, "RelationshipName");
                // In case staging task contains only standard relationships, RelationshipOrder column is not included in the xml
                int relationshipOrder = DataHelper.GetIntValue(relationshipRow, "RelationshipOrder");
                string relationshipCustomData = DataHelper.GetStringValue(relationshipRow, "RelationshipCustomData");

                var relationshipNameInfo = RelationshipNameInfoProvider.GetRelationshipNameInfo(relationshipName);

                // Do not update/insert relationship if relationship name does not exist
                if (relationshipName == null)
                {
                    continue;
                }

                var relationshipNameIsAdHoc = relationshipNameInfo.RelationshipNameIsAdHoc;
                // Get current relationship
                DataRow currentRelationship = null;
                if (currentRelationshipsTable != null)
                {
                    DataRow[] currentRelationships = currentRelationshipsTable.Select("LeftNodeGUID = '" + leftNodeGUID + "' AND RightNodeGUID = '" + rightNodeGUID + "' AND RelationshipName = '" + SqlHelper.GetSafeQueryString(relationshipName, false) + "'");
                    if (currentRelationships.Length > 0)
                    {
                        currentRelationship = currentRelationships[0];
                    }
                }

                // Relationship exists
                if (currentRelationship != null)
                {
                    // Update relationship custom data or relationship order if it differs
                    string currentRelationshipCustomData = DataHelper.GetStringValue(currentRelationship, "RelationshipCustomData");
                    int currentRelationshipOrder = DataHelper.GetIntValue(currentRelationship, "RelationshipOrder");
                    bool update = (relationshipCustomData != currentRelationshipCustomData) ||
                                  (relationshipOrder != currentRelationshipOrder);
                    if (update)
                    {
                        // Update relation
                        var relation = new RelationshipInfo(currentRelationship);
                        relation.RelationshipIsAdHoc = relationshipNameIsAdHoc;
                        relation.RelationshipOrder = relationshipOrder;
                        relation.RelationshipCustomData.LoadData(relationshipCustomData);
                        RelationshipInfoProvider.SetRelationshipInfo(relation);
                    }

                    // Remove from the table
                    currentRelationshipsTable.Rows.Remove(currentRelationship);
                }
                // Not exists, add relationship if relationship name exists
                else
                {
                    int leftNodeId = TreePathUtils.GetNodeIdByNodeGUID(leftNodeGUID, SiteName);
                    int rightNodeId = TreePathUtils.GetNodeIdByNodeGUID(rightNodeGUID, SiteName);

                    if ((leftNodeId > 0) && (rightNodeId > 0) && (relationshipNameInfo.IsOnSite(SiteID) || relationshipNameIsAdHoc))
                    {
                        // Add relation
                        var relation = new RelationshipInfo();
                        relation.LeftNodeId = leftNodeId;
                        relation.RightNodeId = rightNodeId;
                        relation.RelationshipNameId = relationshipNameInfo.RelationshipNameId;
                        relation.RelationshipCustomData.LoadData(relationshipCustomData);
                        relation.RelationshipIsAdHoc = relationshipNameIsAdHoc;
                        relation.RelationshipOrder = relationshipOrder;

                        // Set relationship info object
                        RelationshipInfoProvider.SetRelationshipInfo(relation);
                    }
                }
            }

            // Delete all remaining relationships
            if (currentRelationshipsTable != null)
            {
                foreach (DataRow dr in currentRelationshipsTable.Rows)
                {
                    // Remove the relationship
                    int leftNodeId = ValidationHelper.GetInteger(dr["LeftNodeID"], 0);
                    int rightNodeId = ValidationHelper.GetInteger(dr["RightNodeID"], 0);
                    int relationshipNameId = ValidationHelper.GetInteger(dr["RelationshipNameID"], 0);
                    if ((leftNodeId > 0) && (rightNodeId > 0) && (relationshipNameId > 0))
                    {
                        // Remove relationship
                        RelationshipInfo ri = RelationshipInfoProvider.GetRelationshipInfo(leftNodeId, rightNodeId,
                            relationshipNameId);
                        if (ri != null)
                        {
                            RelationshipInfoProvider.DeleteRelationshipInfo(ri);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Updates the categories.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="categoriesTable">Categories table</param>
        /// <param name="th">Translation helper</param>
        protected virtual void UpdateCategories(TreeNode node, DataTable categoriesTable, TranslationHelper th)
        {
            int documentId = node.DocumentID;

            // Delete all categories assignments
            DocumentCategoryInfoProvider.RemoveDocumentFromCategories(documentId, LogTasks);

            // Update categories
            if (categoriesTable != null)
            {
                // Process all the categories
                foreach (DataRow categoryRow in categoriesTable.Rows)
                {
                    int oldCategoryId = ValidationHelper.GetInteger(categoryRow["CategoryID"], 0);
                    int categoryId = th.GetNewID(CategoryInfo.OBJECT_TYPE, oldCategoryId, "CategoryName", node.OriginalNodeSiteID, "CategorySiteID", null, null);
                    if (categoryId != 0)
                    {
                        // Create new binding
                        DocumentCategoryInfo infoObj = new DocumentCategoryInfo();
                        infoObj.CategoryID = categoryId;
                        infoObj.DocumentID = documentId;

                        using (CMSActionContext childContext = new CMSActionContext())
                        {
                            childContext.CreateVersion = true;

                            // Save to the database
                            DocumentCategoryInfoProvider.SetDocumentCategoryInfo(infoObj);
                        }
                    }
                }
            }
        }


        /// <summary>
        /// Updates the document aliases.
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="aliasesTable">Aliases table</param>
        /// <param name="th">Translation helper</param>
        protected virtual void UpdateAliases(TreeNode node, DataTable aliasesTable, TranslationHelper th)
        {
            int nodeId = node.NodeID;

            using (new CMSActionContext { CreateSearchTask = false })
            {
                // Delete all aliases assignments
                DocumentAliasInfoProvider.DeleteNodeAliases(nodeId);
            }

            // Update aliases
            if (aliasesTable == null)
            {
                return;
            }

            // Process all the aliases
            foreach (DataRow aliasRow in aliasesTable.Rows)
            {
                DocumentAliasInfo aliasInfo = new DocumentAliasInfo(aliasRow);
                aliasInfo.AliasID = 0;
                aliasInfo.AliasNodeID = nodeId;
                aliasInfo.AliasSiteID = node.NodeSiteID;

                using (new CMSActionContext { CreateVersion = true, CreateSearchTask = false })
                {
                    // Save to the database
                    DocumentAliasInfoProvider.SetDocumentAliasInfo(aliasInfo);
                }
            }
        }


        /// <summary>
        /// Translates given node columns using data from helper.
        /// </summary>
        /// <param name="node">Node to translate</param>
        /// <param name="th">Translation helper with the data</param>
        protected virtual void TranslateNodeColumns(TreeNode node, TranslationHelper th)
        {
            // Set current site ID
            node.SetValue("NodeSiteID", SiteID);

            if (!ProceedWithTranslations(th))
            {
                return;
            }

            th.SetDefaultValue(PredefinedObjectType.USER, AdministratorId);

            // Translate Node linked node ID
            int linkedNodeId = ValidationHelper.GetInteger(node.GetValue("NodeLinkedNodeID"), 0);
            if (linkedNodeId > 0)
            {
                // Get the record
                DataRow drRecord = th.GetRecord(PredefinedObjectType.NODE, linkedNodeId);
                if (drRecord == null)
                {
                    // No translation record, make physical document
                    node.SetValue("NodeLinkedNodeID", null);
                    node.SetValue("NodeLinkedNodeSiteID", null);
                }
                else
                {
                    Guid linkedNodeGuid = DataHelper.GetGuidValue(drRecord, TranslationHelper.RECORD_GUID_COLUMN, Guid.Empty);
                    string linkedNodeSiteName = DataHelper.GetStringValue(drRecord, TranslationHelper.RECORD_SITE_NAME_COLUMN, null);

                    bool linkedNodeSiteNameValid = false;
                    if (!string.IsNullOrEmpty(linkedNodeSiteName))
                    {
                        // Validate site
                        SiteInfo linkedSi = SiteInfoProvider.GetSiteInfo(linkedNodeSiteName);
                        linkedNodeSiteNameValid = (linkedSi != null);
                    }

                    // Site is not valid
                    if (!linkedNodeSiteNameValid)
                    {
                        // Use current site name
                        linkedNodeSiteName = SiteName;
                    }

                    if (linkedNodeGuid != Guid.Empty)
                    {
                        // Get the original node ID
                        int nodeId = TreePathUtils.GetNodeIdByNodeGUID(linkedNodeGuid, linkedNodeSiteName);
                        if (nodeId > 0)
                        {
                            node.SetValue("NodeLinkedNodeID", nodeId);

                            // Get the linked node site
                            SiteInfo si = SiteInfoProvider.GetSiteInfo(linkedNodeSiteName);
                            if (si != null)
                            {
                                node.SetValue("NodeLinkedNodeSiteID", si.SiteID);
                            }
                        }
                        else
                        {
                            throw new InvalidOperationException("Original node for linked page does not exist, please synchronize the original node first.");
                        }
                    }
                    else
                    {
                        // No translation record, make physical document
                        node.SetValue("NodeLinkedNodeID", null);
                        node.SetValue("NodeLinkedNodeSiteID", null);
                    }
                }
            }

            // Translate page template IDs to the new value
            TranslateTemplateColumn(node, th, "DocumentPageTemplateID");
            TranslateTemplateColumn(node, th, "NodeTemplateID");

            // Translate CSS style sheet to the new value
            if (node.DocumentStylesheetID > 0)
            {
                string stylesheetName = th.GetCodeName(CssStylesheetInfo.OBJECT_TYPE, node.DocumentStylesheetID);
                if (stylesheetName != string.Empty)
                {
                    CssStylesheetInfo si = CssStylesheetInfoProvider.GetCssStylesheetInfo(stylesheetName);
                    node.DocumentStylesheetID = (si != null) ? si.StylesheetID : 0;
                }
                else
                {
                    node.DocumentStylesheetID = 0;
                }
            }

            // User references
            TranslateUserColumn(node, th, "DocumentCreatedByUserID");
            TranslateUserColumn(node, th, "DocumentModifiedByUserID");
            TranslateUserColumn(node, th, "NodeOwner");

            // SKU ID
            int nodeSKUId = ValidationHelper.GetInteger(node.GetValue("NodeSKUID"), 0);
            if (nodeSKUId > 0)
            {
                // Get GUID of SKU (SKU does not have codename column)
                Guid skuGuid = ValidationHelper.GetGuid(th.GetCodeName(PredefinedObjectType.SKU, nodeSKUId), Guid.Empty);
                GeneralizedInfo si = null;
                if (skuGuid != Guid.Empty)
                {
                    // Get the product
                    si = ProviderHelper.GetInfoByGuid(PredefinedObjectType.SKU, skuGuid);
                }
                if (si != null)
                {
                    node.SetValue("NodeSKUID", si.ObjectID);
                }
                else
                {
                    node.SetValue("NodeSKUID", null);
                }
            }

            // Group ID
            int groupId = ValidationHelper.GetInteger(node.GetValue("NodeGroupID"), 0);
            if (groupId > 0)
            {
                string groupCodeName = th.GetCodeName(PredefinedObjectType.GROUP, groupId);
                GeneralizedInfo gi = ModuleCommands.CommunityGetGroupInfoByName(groupCodeName, SiteName);

                if (gi == null)
                {
                    // ### Special case: Backward compatibility
                    Guid groupGuid = ValidationHelper.GetGuid(groupCodeName, Guid.Empty);
                    if (groupGuid != Guid.Empty)
                    {
                        // Get the group
                        gi = ModuleCommands.CommunityGetGroupInfoByGuid(groupGuid);
                    }
                }
                if (gi != null)
                {
                    node.SetValue("NodeGroupID", gi.ObjectID);
                }
                else
                {
                    node.SetValue("NodeGroupID", null);
                }
            }

            // Tag group ID
            int nodeTagGroupId = ValidationHelper.GetInteger(node.GetValue("DocumentTagGroupID"), 0);
            if (nodeTagGroupId > 0)
            {
                string tagGroupCodeName = th.GetCodeName(TagGroupInfo.OBJECT_TYPE, nodeTagGroupId);
                TagGroupInfo tgi = TagGroupInfoProvider.GetTagGroupInfo(tagGroupCodeName, node.OriginalNodeSiteID);
                if (tgi == null)
                {
                    // ### Special case: Backward compatibility
                    Guid tagGroupGuid = ValidationHelper.GetGuid(tagGroupCodeName, Guid.Empty);
                    if (tagGroupGuid != Guid.Empty)
                    {
                        // Get the tag group
                        tgi = TagGroupInfoProvider.GetTagGroupInfo(tagGroupGuid, node.OriginalNodeSiteID);
                    }
                }
                if (tgi != null)
                {
                    node.SetValue("DocumentTagGroupID", tgi.TagGroupID);
                }
                else
                {
                    node.SetValue("DocumentTagGroupID", null);
                }
            }

            th.RemoveDefaultValue(PredefinedObjectType.USER);
        }


        /// <summary>
        /// Translates the user reference column of the document
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="th">Translation helper</param>
        /// <param name="colName">Column name</param>
        private static void TranslateUserColumn(TreeNode node, TranslationHelper th, string colName)
        {
            int userId = node.GetValue(colName, 0);
            if (userId > 0)
            {
                // Get new ID
                userId = th.GetNewID(UserInfo.OBJECT_TYPE, userId, "UserName", 0, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN, ObjectTypeInfo.COLUMN_NAME_UNKNOWN);

                node.SetIntegerValue(colName, userId, false);
            }
        }


        /// <summary>
        /// Translates the given page template column of the document
        /// </summary>
        /// <param name="node">Document node</param>
        /// <param name="th">Translation helper</param>
        /// <param name="colName">Column name</param>
        private static void TranslateTemplateColumn(TreeNode node, TranslationHelper th, string colName)
        {
            // Translate node page template ID to the new value
            int templateId = node.GetValue(colName, 0);
            if (templateId > 0)
            {
                DataRow dr = th.GetRecord(PredefinedObjectType.PAGETEMPLATE, templateId);
                if (dr != null)
                {
                    string templateName = DataHelper.GetStringValue(dr, TranslationHelper.RECORD_CODE_NAME_COLUMN);
                    string templateSiteName = DataHelper.GetStringValue(dr, TranslationHelper.RECORD_SITE_NAME_COLUMN, null);

                    // Get correct site ID
                    int templateSiteId = (templateSiteName == null) ? 0 : node.OriginalNodeSiteID;

                    PageTemplateInfo ti = PageTemplateInfoProvider.GetPageTemplateInfo(templateName, templateSiteId);

                    templateId = ((ti != null) ? ti.PageTemplateId : 0);
                }
                else
                {
                    templateId = 0;
                }

                node.SetIntegerValue(colName, templateId, false);
            }
        }


        /// <summary>
        /// Determines whether to continue with translation operations.
        /// </summary>
        /// <param name="th">Current translation helper object</param>
        /// <returns>TRUE if OperationType is other than Integration. When OperationType is  Integration there also have to be some translations present.</returns>
        public bool ProceedWithTranslations(TranslationHelper th)
        {
            return (OperationType != OperationTypeEnum.Integration) || ((OperationType == OperationTypeEnum.Integration) && !TranslationHelper.IsEmpty(th));
        }


        /// <summary>
        /// Removes excluded column values from the given document.
        /// </summary>
        private void RemoveExcludedValues(TreeNode node)
        {
            var excludedColumns = TreeNode.TYPEINFO.SynchronizationSettings.ExcludedStagingColumns;
            if (excludedColumns == null)
            {
                return;
            }

            foreach (var column in excludedColumns)
            {
                node.SetValue(column, null);
            }
        }


        /// <summary>
        /// Indicates if source has object table.
        /// </summary>
        /// <param name="objectType">Type of the object</param>
        /// <param name="ds">Source data</param>
        private bool HasObjectTable(string objectType, DataSet ds)
        {
            GeneralizedInfo infoObj = ModuleManager.GetObject(objectType);

            // Get the object data table
            return ObjectHelper.GetTable(ds, infoObj) != null;
        }


        /// <summary>
        /// Update node with permissions from DataSet.
        /// </summary>
        /// <param name="node">Node to process</param>
        /// <param name="documentDS">DataSet with permissions tables</param>
        /// <param name="th">TranslationHelper</param>
        protected virtual void UpdateACLs(TreeNode node, DataSet documentDS, TranslationHelper th)
        {
            // Get ACL tables
            DataTable aclTable = documentDS.Tables["CMS_ACL"];
            DataTable aclItemTable = documentDS.Tables["CMS_ACLItem"];

            if (DataHelper.DataSourceIsEmpty(aclTable))
            {
                return;
            }

            // Get ACL data
            DataRow drNodeAcl = aclTable.Rows[0];
            string inheritedAcls = DataHelper.GetStringValue(drNodeAcl, "ACLInheritedACLs").Trim();
            Guid aclGUID = DataHelper.GetGuidValue(drNodeAcl, "ACLGUID", Guid.Empty);

            // Ensure own ACL, keep ACL GUID
            int aclId = AclInfoProvider.EnsureOwnAcl(node);
            var acl = AclInfoProvider.GetAclInfo(aclId);

            // If node doesn't inherit permissions break permission inheritance
            if (inheritedAcls == string.Empty)
            {
                // Get current inherited ACLs
                DataRow drOriginalNodeAcl = AclInfoProvider.GetNodeAndAcl(node.NodeID, "ACLInheritedACLs");

                // Clear inheritance from the ACL
                acl.SetValue("ACLInheritedACLs", string.Empty);

                // Remove old ACLID values from the child nodes
                if (drOriginalNodeAcl != null)
                {
                    string originalNodeACL = ValidationHelper.GetString(drOriginalNodeAcl["ACLInheritedACLs"], string.Empty);
                    AclInfoProvider.RemoveAclIds(node, originalNodeACL);
                }
            }

            // Keep ACL GUID
            acl.SetValue("ACLGUID", aclGUID);
            acl.Update();

            // Prepare condition to clear not valid ACL items
            var deleteCondition = new WhereCondition()
                                    .WhereEquals("ACLID", aclId);

            var deleteACLItems = false;

            if (!DataHelper.DataSourceIsEmpty(aclItemTable))
            {
                var sourceGUIDs = new HashSet<Guid>();

                // Process ACL item list
                foreach (DataRow dr in aclItemTable.Rows)
                {
                    // Get ACLItem data from row
                    var originalACLItem = new AclItemInfo(dr);

                    originalACLItem.ACLID = aclId;
                    originalACLItem.LastModified = DateTime.Now;
                    originalACLItem.LastModifiedByUserID = MembershipContext.AuthenticatedUser.UserID;

                    int userId = originalACLItem.UserID;
                    int roleId = originalACLItem.RoleID;

                    sourceGUIDs.Add(ValidationHelper.GetGuid(dr["ACLItemGUID"], Guid.Empty));

                    // Create new user permission 
                    if (userId > 0)
                    {
                        string userName = th.GetCodeName(PredefinedObjectType.USER, userId, string.Empty);
                        if (userName == string.Empty)
                        {
                            continue;
                        }

                        UserInfo ui = UserInfoProvider.GetUserInfo(userName);
                        if (ui != null)
                        {
                            // Update user identifier
                            originalACLItem.UserID = ui.UserID;
                        }
                    }
                    // Create new role permission
                    else if (roleId > 0)
                    {
                        string roleName = th.GetCodeName(PredefinedObjectType.ROLE, roleId, string.Empty);
                        if (roleName == string.Empty)
                        {
                            continue;
                        }

                        RoleInfo ri = RoleInfoProvider.GetRoleInfo(roleName, SiteName, true);
                        if (ri != null)
                        {
                            // Update role identifier
                            originalACLItem.RoleID = ri.RoleID;
                        }
                    }
                    else
                    {
                        // Continue to next item
                        continue;
                    }

                    // Check if ACL item already exist or new one should be created
                    var existingObject = originalACLItem.Generalized.GetExisting();
                    originalACLItem.ACLItemID = existingObject != null ? existingObject.Generalized.ObjectID : 0;

                    AclItemInfoProvider.SetAclItemInfo(originalACLItem);
                }

                if (sourceGUIDs.Count > 0)
                {
                    // Delete existing ACL items which were not in the staging task
                    deleteACLItems = true;

                    deleteCondition.WhereNotIn("ACLItemGUID", sourceGUIDs);
                }
            }
            else
            {
                // Delete all ACL items assigned to current ACL
                deleteACLItems = true;
            }

            if (deleteACLItems)
            {
                AclItemInfoProvider.DeleteAclItems(deleteCondition);
            }

            // Log the update task for next stage
            if (LogTasks)
            {
                DocumentSynchronizationHelper.LogDocumentChange(node, TaskTypeEnum.UpdateDocument, TreeProvider);
            }
        }

        #endregion


        #region "ACL methods"

        /// <summary>
        /// Break ACL inheritance.
        /// </summary>
        /// <param name="documentDS">Document data</param>
        /// <param name="taskType">Task type information</param>
        /// <param name="safeClassName">Class name of document in safe format</param>
        protected virtual void ProcessACLItems(DataSet documentDS, TaskTypeEnum taskType, string safeClassName)
        {
            // Get the document table
            DataTable documentTable = documentDS.Tables[safeClassName];
            if (documentTable == null)
            {
                throw new InvalidOperationException("Page data table not found.");
            }

            if (DataHelper.DataSourceIsEmpty(documentTable))
            {
                throw new InvalidOperationException("Page table doesn't contain any data.");
            }

            // Permissions are shared through different culture versions, use first culture
            DataRow nodeRow = documentTable.Rows[0];

            // Get node GUID
            Guid nodeGuid = DataHelper.GetGuidValue(nodeRow, "NodeGUID", Guid.Empty);

            // Get document culture
            string culture = DataHelper.GetStringValue(nodeRow, "DocumentCulture");

            // Get current node by GUID
            TreeNode currentNode = TreeProvider.SelectSingleNode(nodeGuid, culture, SiteName);

            if (currentNode == null)
            {
                throw new InvalidOperationException("Page not found.");
            }

            // Process ACL item with particular action
            switch (taskType)
            {
                // Breaking ACL inheritance
                case TaskTypeEnum.BreakACLInheritance:

                    // Get additional task parameters
                    bool copyParentPerm = false;
                    DataTable taskParametersTable = documentDS.Tables["TaskParameters"];
                    TaskParameters taskParam = null;

                    if (taskParametersTable != null)
                    {
                        taskParam = new TaskParameters(taskParametersTable);
                        copyParentPerm = ValidationHelper.GetBoolean(taskParam.GetParameter("copyPermissions"), false);
                    }

                    // Break inheritance
                    AclInfoProvider.BreakInheritance(currentNode, copyParentPerm);

                    if (LogTasks)
                    {
                        // Log staging task
                        DocumentSynchronizationHelper.LogDocumentChange(currentNode, TaskTypeEnum.BreakACLInheritance, true, true, currentNode.TreeProvider, SynchronizationInfoProvider.ENABLED_SERVERS, taskParam, currentNode.TreeProvider.AllowAsyncActions);
                    }
                    break;

                // Restoring ACL inheritance
                case TaskTypeEnum.RestoreACLInheritance:

                    AclInfoProvider.RestoreInheritance(currentNode);

                    if (LogTasks)
                    {
                        // Log staging task and flush cache
                        DocumentSynchronizationHelper.LogDocumentChange(currentNode, TaskTypeEnum.RestoreACLInheritance, true, true, currentNode.TreeProvider, SynchronizationInfoProvider.ENABLED_SERVERS, null, true);
                    }
                    break;
            }
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Gets synchronization task empty DataSet for specified task type.
        /// </summary>
        /// <param name="taskType">Task type</param>
        /// <param name="className">Document class name (optional)</param>
        /// <param name="taskObjectType">Task object type (optional)</param>
        protected virtual DataSet GetSynchronizationTaskDataSetInternal(TaskTypeEnum taskType, string className, string taskObjectType)
        {
            DataSet ds;

            // Process the task
            switch (taskType)
            {
                // Document tasks
                case TaskTypeEnum.CreateDocument:
                case TaskTypeEnum.UpdateDocument:
                case TaskTypeEnum.DeleteDocument:
                case TaskTypeEnum.DeleteAllCultures:
                case TaskTypeEnum.PublishDocument:
                case TaskTypeEnum.ArchiveDocument:
                case TaskTypeEnum.MoveDocument:
                    {
                        var classInfo = DataClassInfoProvider.GetDataClassInfo(className);
                        ds = DocumentHelper.GetTreeNodeDataSet(className, classInfo != null && classInfo.ClassIsCoupledClass, false);

                        switch (taskType)
                        {
                            case TaskTypeEnum.CreateDocument:
                            case TaskTypeEnum.UpdateDocument:
                                {
                                    // Attachment table
                                    var ai = new AttachmentInfo();

                                    DataSet attachmentsDS = ObjectHelper.GetObjectsDataSet(OperationType, ai, false);
                                    DataHelper.TransferTables(ds, attachmentsDS);

                                    // Page template tables
                                    BaseInfo pti = ModuleManager.GetObject(PredefinedObjectType.PAGETEMPLATE);
                                    DataSet templatesDS = ObjectHelper.GetObjectsDataSet(OperationType, pti, true);
                                    DataHelper.TransferTables(ds, templatesDS);

                                    // Include data if module loaded
                                    if (ModuleEntryManager.IsModuleLoaded(ModuleName.ECOMMERCE))
                                    {
                                        // SKU tables
                                        BaseInfo si = ModuleManager.GetObject(PredefinedObjectType.SKU);
                                        if (si != null)
                                        {
                                            DataSet skuDS = ObjectHelper.GetObjectsDataSet(OperationType, si, true);
                                            DataHelper.TransferTables(ds, skuDS);
                                        }
                                    }

                                    // Include data if module loaded
                                    if (ModuleEntryManager.IsModuleLoaded(ModuleName.COMMUNITY))
                                    {
                                        // Group tables
                                        BaseInfo gi = ModuleManager.GetObject(PredefinedObjectType.GROUP);
                                        DataSet groupDS = ObjectHelper.GetObjectsDataSet(OperationType, gi, true);
                                        DataHelper.TransferTables(ds, groupDS);
                                    }

                                    // Tag group tables
                                    TagGroupInfo tgi = new TagGroupInfo();
                                    DataSet tagGroupDS = ObjectHelper.GetObjectsDataSet(OperationType, tgi, true);
                                    DataHelper.TransferTables(ds, tagGroupDS);

                                    // Include data if module loaded
                                    if (ModuleEntryManager.IsModuleLoaded(ModuleName.MESSAGEBOARD))
                                    {
                                        // Board tables
                                        BaseInfo bi = ModuleManager.GetObject(PredefinedObjectType.BOARD);
                                        DataSet boardDS = ObjectHelper.GetObjectsDataSet(OperationType, bi, true);
                                        DataHelper.TransferTables(ds, boardDS);
                                    }

                                    // Alias tables
                                    BaseInfo ali = ModuleManager.GetObject(DocumentAliasInfo.OBJECT_TYPE);
                                    DataSet aliasDS = ObjectHelper.GetObjectsDataSet(OperationType, ali, true);
                                    DataHelper.TransferTables(ds, aliasDS);

                                    // Document ACLs table
                                    BaseInfo acl = ModuleManager.GetObject(AclInfo.OBJECT_TYPE);
                                    DataSet aclsDS = ObjectHelper.GetObjectsDataSet(OperationType, acl, true);
                                    DataHelper.TransferTables(ds, aclsDS);

                                    // Document ACL items table
                                    BaseInfo aclItem = ModuleManager.GetObject(AclItemInfo.OBJECT_TYPE);
                                    DataSet aclItemsDS = ObjectHelper.GetObjectsDataSet(OperationType, aclItem, true);
                                    DataHelper.TransferTables(ds, aclItemsDS);
                                }
                                break;

                            case TaskTypeEnum.PublishDocument:
                                {
                                    // Attachment history table
                                    AttachmentHistoryInfo ahi = new AttachmentHistoryInfo(null);
                                    DataSet attachmentVersionsDS = ObjectHelper.GetObjectsDataSet(OperationType, ahi, true);
                                    DataHelper.TransferTables(ds, attachmentVersionsDS);

                                    // Version history table
                                    VersionHistoryInfo hi = new VersionHistoryInfo(null);
                                    DataSet historiesDS = ObjectHelper.GetObjectsDataSet(OperationType, hi, true);
                                    DataHelper.TransferTables(ds, historiesDS);

                                    // Attachment table (When a document is published outside the scope of workflow)
                                    var ai = new AttachmentInfo();
                                    DataSet attachmentsDS = ObjectHelper.GetObjectsDataSet(OperationType, ai, false);
                                    DataHelper.TransferTables(ds, attachmentsDS);
                                }
                                break;
                        }
                    }
                    break;

                // Update object
                case TaskTypeEnum.CreateObject:
                case TaskTypeEnum.UpdateObject:
                case TaskTypeEnum.AddToSite:
                case TaskTypeEnum.RemoveFromSite:
                    {
                        // Get the object definition
                        BaseInfo infoObj = ModuleManager.GetObject(taskObjectType);

                        // Get the DataSet
                        ds = ObjectHelper.GetObjectsDataSet(OperationType, infoObj, true);
                    }
                    break;

                // Delete object
                case TaskTypeEnum.DeleteObject:
                    {
                        // Get the object definition
                        BaseInfo infoObj = ModuleManager.GetObject(taskObjectType);

                        // Get the DataSet
                        ds = ObjectHelper.GetObjectsDataSet(OperationType, infoObj, false);
                    }
                    break;

                // Unknown task
                default:
                    ds = new DataSet();
                    break;
            }

            if (ds != null)
            {
                ds.Tables.Add(TranslationHelper.GetEmptyTable());
            }

            return ds;
        }


        /// <summary>
        /// Processes the given task.
        /// </summary>
        /// <param name="stagingTaskData">StagingTaskData for processing</param>
        /// <param name="processChildren">Indicates if also child objects should be processed</param>
        /// <param name="handler">Synchronization handler</param>
        protected virtual ICMSObject ProcessTaskInternal(IStagingTaskData stagingTaskData, bool processChildren, StagingSynchronizationHandler handler)
        {
            ICMSObject returnValue = null;

            using (var syncContext = new SynchronizationActionContext())
            {
                // If user who synchronized changes on source does not exists on target, do not log him with current staging task
                var user = TryGetUserSynchronizator(stagingTaskData.UserGuid, stagingTaskData.UserName);
                syncContext.LogUserWithTask = user != null;

                // Set sent task groups into synchronization context when we synchronize changes we can create given task groups
                syncContext.TaskGroups = GetTaskGroupsFromSentTasks(stagingTaskData.TaskGroups);

                using (new CMSActionContext(user ?? AdministratorUser) { UseGlobalAdminContext = true })
                {
                    // Get the task data
                    if (string.IsNullOrEmpty(stagingTaskData.TaskData))
                    {
                        throw new InvalidOperationException("Missing task data.");
                    }

                    // Get the task data
                    DataSet taskDS = GetDataSetInternal(stagingTaskData.TaskData, stagingTaskData.TaskType, stagingTaskData.TaskObjectType);

                    // Get the physical files
                    DataSet binaryDataDS = GetPhysicalFilesDataSet(stagingTaskData.TaskBinaryData);

                    using (CMSActionContext ctx = new CMSActionContext())
                    {
                        // Do not allow asynchronous actions
                        ctx.AllowAsyncActions = false;
                        ctx.TouchParent = false;
                        ctx.LogSynchronization = LogTasks;

                        string className = DocumentHierarchyHelper.GetNodeClassName(stagingTaskData.TaskData, ExportFormatEnum.XML, false);
                        string safeClassName = TranslationHelper.GetSafeClassName(className);
                        if (string.IsNullOrEmpty(safeClassName))
                        {
                            // ### Special case - backward compatibility (should not be needed)
                            safeClassName = "DocumentData";
                        }

                        // Handle the event
                        StagingSynchronizationHandler h = null;
                        if (handler != null)
                        {
                            h = handler.StartEvent(stagingTaskData.TaskType, stagingTaskData.TaskObjectType, taskDS, binaryDataDS, this);
                        }

                        using (h)
                        {
                            if ((h == null) || (h.CanContinue() && !h.EventArguments.TaskHandled))
                            {
                                // Process the task
                                switch (stagingTaskData.TaskType)
                                {
                                    // Create and update document
                                    case TaskTypeEnum.CreateDocument:
                                    case TaskTypeEnum.UpdateDocument:
                                        UpdateDocument(taskDS, safeClassName, processChildren);
                                        // Ensure document to be archived
                                        WorkflowStepTypeEnum stepType = (WorkflowStepTypeEnum)DataHelper.GetIntValue(taskDS.Tables[safeClassName].Rows[0], "StepType", 1);
                                        if (stepType == WorkflowStepTypeEnum.DocumentArchived)
                                        {
                                            ArchiveDocument(taskDS, safeClassName);
                                        }
                                        break;

                                    // Document deletion
                                    case TaskTypeEnum.DeleteDocument:
                                        DeleteDocument(taskDS, false, safeClassName);
                                        break;

                                    // Delete all culture versions of the document
                                    case TaskTypeEnum.DeleteAllCultures:
                                        DeleteDocument(taskDS, true, safeClassName);
                                        break;

                                    // Publish document
                                    case TaskTypeEnum.PublishDocument:
                                        // Document was published outside the scope of a workflow
                                        DataTable versionsTable = taskDS.Tables["CMS_VersionHistory"];
                                        if (DataHelper.DataSourceIsEmpty(versionsTable))
                                        {
                                            // Update document
                                            UpdateDocument(taskDS, safeClassName, processChildren);
                                        }
                                        // Standard publish
                                        else
                                        {
                                            PublishDocument(taskDS, safeClassName);
                                        }
                                        break;

                                    // Archive document
                                    case TaskTypeEnum.ArchiveDocument:
                                        ArchiveDocument(taskDS, safeClassName);
                                        break;

                                    // Reject document
                                    case TaskTypeEnum.RejectDocument:
                                        RejectDocument(taskDS, safeClassName);
                                        break;

                                    // Move document
                                    case TaskTypeEnum.MoveDocument:
                                        MoveDocument(taskDS, safeClassName);
                                        break;

                                    // Update object
                                    case TaskTypeEnum.UpdateObject:
                                    case TaskTypeEnum.CreateObject:
                                        using (CMSActionContext context = new CMSActionContext())
                                        {
                                            // Disable partial logging of synchronization tasks and versioning
                                            context.LogSynchronization = false;
                                            context.CreateVersion = false;
                                            context.UpdateTimeStamp = false;
                                            context.UpdateSystemFields = false;

                                            // Update the object
                                            returnValue = UpdateObject(taskDS, binaryDataDS, stagingTaskData.TaskObjectType, null, processChildren, false);
                                        }
                                        break;

                                    // Update object
                                    case TaskTypeEnum.DeleteObject:
                                        using (CMSActionContext context = new CMSActionContext())
                                        {
                                            // Disable partial logging of synchronization tasks and versioning
                                            context.LogSynchronization = false;
                                            context.CreateVersion = false;
                                            context.UpdateTimeStamp = false;
                                            context.UpdateSystemFields = false;

                                            // Do not allow delete site task
                                            if (String.Equals(stagingTaskData.TaskObjectType, "cms.site", StringComparison.InvariantCultureIgnoreCase))
                                            {
                                                throw new NotSupportedException("This operation is not supported.");
                                            }

                                            // Delete the object
                                            DeleteObject(taskDS, stagingTaskData.TaskObjectType);
                                        }
                                        break;

                                    // Add object to site
                                    case TaskTypeEnum.AddToSite:
                                        using (CMSActionContext context = new CMSActionContext())
                                        {
                                            // Disable partial logging of synchronization tasks and versioning
                                            context.LogSynchronization = false;
                                            context.CreateVersion = false;
                                            context.UpdateTimeStamp = false;
                                            context.UpdateSystemFields = false;

                                            UpdateObject(taskDS, binaryDataDS, stagingTaskData.TaskObjectType, null, processChildren, true);
                                        }
                                        break;

                                    // Remove object from site
                                    case TaskTypeEnum.RemoveFromSite:
                                        using (CMSActionContext context = new CMSActionContext())
                                        {
                                            // Disable partial logging of synchronization tasks and versioning
                                            context.LogSynchronization = false;
                                            context.CreateVersion = false;
                                            context.UpdateTimeStamp = false;
                                            context.UpdateSystemFields = false;

                                            RemoveObjectFromSite(taskDS, stagingTaskData.TaskObjectType, null);
                                        }
                                        break;

                                    case TaskTypeEnum.BreakACLInheritance:
                                    case TaskTypeEnum.RestoreACLInheritance:
                                        ProcessACLItems(taskDS, stagingTaskData.TaskType, safeClassName);
                                        break;

                                    // Unknown task
                                    default:
                                        throw new NotSupportedException("Unknown synchronization task.");
                                }
                            }

                            if (h != null)
                            {
                                h.FinishEvent();
                            }
                        }
                    }
                }
            }

            return returnValue;
        }


        /// <summary>
        /// Returns the dataset loaded from the given task data.
        /// </summary>
        /// <param name="taskData">Task data to make the DataSet from</param>
        /// <param name="taskType">Task type</param>
        /// <param name="objectType">Object type</param>
        protected virtual DataSet GetDataSetInternal(string taskData, TaskTypeEnum taskType, string objectType)
        {
            DataSet ds;

            try
            {
                string className = DocumentHierarchyHelper.GetNodeClassName(taskData, ExportFormatEnum.XML, false);
                // Get typed DataSet
                ds = GetSynchronizationTaskDataSet(taskType, className, objectType);

                // Read the entire DataSet from XML
                XmlParserContext xmlContext = new XmlParserContext(null, null, null, XmlSpace.None);
                XmlReader reader = new XmlTextReader(taskData, XmlNodeType.Element, xmlContext);

                ds = DataHelper.ReadDataSetFromXml(ds, reader, null, null);
                reader.Close();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error parsing task data - " + ex.Message, ex);
            }

            return ds;
        }


        /// <summary>
        /// Gets physical files DataSet.
        /// </summary>
        /// <param name="binaryData">String representation of physical files data</param>
        protected virtual DataSet GetPhysicalFilesDataSet(string binaryData)
        {
            if (!string.IsNullOrEmpty(binaryData))
            {
                try
                {
                    // Convert to DataSet
                    DataSet ds = ObjectHelper.GetBinaryDataSet(true);

                    // Read the entire DataSet from XML
                    XmlParserContext xmlContext = new XmlParserContext(null, null, null, XmlSpace.None);
                    XmlReader reader = new XmlTextReader(binaryData, XmlNodeType.Element, xmlContext);

                    ds = DataHelper.ReadDataSetFromXml(ds, reader, null, null);
                    reader.Close();

                    return ds;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException("Error parsing page data - " + ex.Message, ex);
                }
            }

            return null;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Removes excluded column values from the given info object according to <see cref="SynchronizationSettings.ExcludedStagingColumns"/> definition.
        /// </summary>
        /// <remarks>
        /// Use only for newly created objects.
        /// </remarks>
        private void SetDefaultValuesInExcludedColumns(GeneralizedInfo info)
        {
            var typeInfo = info.TypeInfo;
            var excludedColumns = typeInfo.SynchronizationSettings.ExcludedStagingColumns;
            if (excludedColumns == null)
            {
                return;
            }

            foreach (var column in excludedColumns)
            {
                // Clear the column value if the column is foreign key, otherwise use the default value of the column type.
                var defaultValue = typeInfo.IsForeignKey(column) ? null : GetDefaultColumnValue(info, column);

                info.SetValue(column, defaultValue);
            }
        }


        /// <summary>
        /// Returns default value based on the type of given <paramref name="column" />.
        /// </summary>
        private static object GetDefaultColumnValue(GeneralizedInfo info, string column)
        {
            var columnType = info.GetColumnType(column);
            if (columnType != null)
            {
                var dataType = DataTypeManager.GetDataType(columnType);
                if (dataType != null)
                {
                    return dataType.ObjectDefaultValue;
                }
            }

            return null;
        }


        /// <summary>
        /// Applies the data from the existing object that should be preserved.
        /// </summary>
        /// <remarks>
        /// Preserved columns are defined in property <see cref="SynchronizationSettings.ExcludedStagingColumns"/> in each particular <see cref="ObjectTypeInfo"/>.
        /// </remarks>
        private void PreserveExcludedValues(GeneralizedInfo infoObj, GeneralizedInfo existingInfo)
        {
            var excludedColumns = infoObj.TypeInfo.SynchronizationSettings.ExcludedStagingColumns;
            if (excludedColumns == null)
            {
                return;
            }

            var preservedColumns = infoObj.ColumnNames.Intersect(excludedColumns, StringComparer.OrdinalIgnoreCase);
            foreach (var column in preservedColumns)
            {
                infoObj.SetValue(column, existingInfo.GetValue(column));
            }
        }


        /// <summary>
        /// Gets UserID under which the task processing will be executed.
        /// If user with given GUID or Username does not exists or is disabled, zero is returned, else UserID is returned.
        /// </summary>
        /// <param name="userGuid">Guid that belongs to user who has started the synchronization process on source server</param>
        /// <param name="username">Username that belongs to user who has started the synchronization process on source server</param>
        private UserInfo TryGetUserSynchronizator(Guid userGuid, string username)
        {
            var user = UserInfoProvider.GetUserInfoByGUID(userGuid) ?? UserInfoProvider.GetUserInfo(username);
            return user == null || !user.Enabled ? null : user;
        }


        /// <summary>
        /// Create task groups from data sent from source server when synchronizing changes.
        /// </summary>
        private static IEnumerable<TaskGroupInfo> GetTaskGroupsFromSentTasks(IEnumerable<TaskGroupInfo> sentTaskGroups)
        {
            sentTaskGroups = sentTaskGroups.ToArray();

            // Get task groups that exist in current environment
            var existingTaskGroups = TaskGroupInfoProvider.GetTaskGroups()
                .WhereIn("TaskGroupGuid", sentTaskGroups.Select(t => t.TaskGroupGuid).ToArray())
                .Or()
                .WhereIn("TaskGroupCodeName", sentTaskGroups.Select(t => t.TaskGroupCodeName).ToArray())
                .TypedResult.ToList();

            var result = FilterRedundantExistingTaskGroups(sentTaskGroups, existingTaskGroups);

            return result;
        }


        /// <summary>
        /// When synchronizing changes with task groups, which code names were changed on source, but they already exists in target with the same code name
        /// then some redundant task groups occur between existing task groups (SQL Select in <see cref="GetTaskGroupsFromSentTasks"/> method), that need to be filtered.
        /// </summary>
        /// <param name="sentTaskGroups">Task groups sent from source environment</param>
        /// <param name="existingTaskGroups">Task groups from current environment which has the same code name or guid as the sent task groups</param>
        private static IEnumerable<TaskGroupInfo> FilterRedundantExistingTaskGroups(IEnumerable<TaskGroupInfo> sentTaskGroups, List<TaskGroupInfo> existingTaskGroups)
        {
            var result = new List<TaskGroupInfo>();

            // Create a dictionaries of sent task groups to achieve constant access time when looking for a task group
            var existingTaskGroupsByCodeName = existingTaskGroups.ToDictionary(t => t.TaskGroupCodeName);
            var existingTaskGroupsByGuid = existingTaskGroups.ToDictionary(t => t.TaskGroupGuid);
            var sentTaskGroupsByGuid = sentTaskGroups.ToDictionary(t => t.TaskGroupGuid);

            // Prioritize existing groups matched by their GUID
            foreach (var sentTaskGroup in sentTaskGroupsByGuid.Values
                .Where(sg => existingTaskGroupsByGuid.ContainsKey(sg.TaskGroupGuid))
                .ToArray())
            {
                sentTaskGroupsByGuid.Remove(sentTaskGroup.TaskGroupGuid);
                result.Add(existingTaskGroupsByGuid[sentTaskGroup.TaskGroupGuid]);
            }

            // Then existing groups matched by their code name
            foreach (var sentTaskGroup in sentTaskGroupsByGuid.Values
                .Where(sg => existingTaskGroupsByCodeName.ContainsKey(sg.TaskGroupCodeName))
                .ToArray())
            {
                sentTaskGroupsByGuid.Remove(sentTaskGroup.TaskGroupGuid);
                result.Add(existingTaskGroupsByCodeName[sentTaskGroup.TaskGroupCodeName]);
            }

            // The rest of the groups does not exist on the target
            return result.Union(sentTaskGroupsByGuid.Values.Select(group =>
            {
                // Remove group ID that was carried from the source
                group.TaskGroupID = 0;
                return group;
            }));
        }

        #endregion
    }
}
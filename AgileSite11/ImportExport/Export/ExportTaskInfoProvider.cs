using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.DataEngine;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.Synchronization;

namespace CMS.CMSImportExport
{
    /// <summary>
    /// Class providing ExportTaskInfo management.
    /// </summary>
    public class ExportTaskInfoProvider : AbstractInfoProvider<ExportTaskInfo, ExportTaskInfoProvider>
    {
        #region "Public static methods"

        /// <summary>
        /// Returns all export tasks.
        /// </summary>
        public static ObjectQuery<ExportTaskInfo> GetExportTasks()
        {
            return ProviderObject.GetObjectQuery();
        }


        /// <summary>
        /// Returns the ExportTaskInfo structure for the specified exportTask.
        /// </summary>
        /// <param name="exportTaskId">ExportTask id</param>
        public static ExportTaskInfo GetExportTaskInfo(int exportTaskId)
        {
            return ProviderObject.GetInfoById(exportTaskId);
        }


        /// <summary>
        /// Sets (updates or inserts) specified exportTask.
        /// </summary>
        /// <param name="exportTask">ExportTask to set</param>
        public static void SetExportTaskInfo(ExportTaskInfo exportTask)
        {
            ProviderObject.SetInfo(exportTask);
        }


        /// <summary>
        /// Deletes specified exportTask.
        /// </summary>
        /// <param name="exportTaskObj">ExportTask object</param>
        public static void DeleteExportTaskInfo(ExportTaskInfo exportTaskObj)
        {
            ProviderObject.DeleteInfo(exportTaskObj);
        }


        /// <summary>
        /// Deletes export tasks for given site.
        /// </summary>
        /// <param name="site">Site identifier: ID or site name</param>
        public static void DeleteExportTasks(SiteInfoIdentifier site)
        {
            ProviderObject.DeleteExportTasksInternal(site);
        }


        /// <summary>
        /// Deletes specified exportTask.
        /// </summary>
        /// <param name="exportTaskId">ExportTask id</param>
        public static void DeleteExportTaskInfo(int exportTaskId)
        {
            ProviderObject.DeleteExportTaskInfoInternal(exportTaskId);
        }


        /// <summary>
        /// Returns the tasks where condition.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        public static WhereCondition GetTasksWhereCondition(int siteId, string objectTypes, WhereCondition where)
        {
            return ProviderObject.GetTasksWhereConditionInternal(siteId, objectTypes, where);
        }


        /// <summary>
        /// Returns all ExportTaskInfos of specified object types and site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        public static DataSet SelectTaskList(int siteId, string objectTypes, string where, string orderBy, int topN = -1, string columns = null)
        {
            return ProviderObject.SelectTaskListInternal(siteId, objectTypes, where, orderBy, topN, columns);
        }


        /// <summary>
        /// Returns all ExportTaskInfos of specified object types and site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        public static DataSet SelectTaskList(int siteId, string objectTypes, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            return ProviderObject.SelectTaskListInternal(siteId, objectTypes, where, orderBy, topN, columns, offset, maxRecords, ref totalRecords);
        }


        /// <summary>
        /// Logs the task for the given object.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Task type</param>
        /// <returns>Returns new task</returns>
        public static ExportTaskInfo LogTask(GeneralizedInfo infoObj, TaskTypeEnum taskType)
        {
            return ProviderObject.LogTaskInternal(infoObj, taskType);
        }


        /// <summary>
        /// Indicates if the object changes should be logged for the export.
        /// </summary>
        /// <param name="siteName">Site name</param>
        public static bool LogObjectChanges(string siteName)
        {
            return ProviderObject.LogObjectChangesInternal(siteName);
        }
        

        /// <summary>
        /// Indicates if the export task should be logged.
        /// </summary>
        /// <param name="infoObj">Info object instance</param>
        /// <param name="taskType">Task type</param>
        public static bool CheckExportLogging(GeneralizedInfo infoObj, TaskTypeEnum taskType)
        {
            return ProviderObject.CheckExportLoggingInternal(infoObj, taskType);
        }

        #endregion


        #region "Internal methods"

        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(ExportTaskInfo info)
        {
            if (info != null)
            {
                base.DeleteInfo(info);
            }
        }


        /// <summary>
        /// Deletes export tasks for given site.
        /// </summary>
        /// <param name="site">Site identifier: ID or site name. If not provided, deletes all histories.</param>
        protected virtual void DeleteExportTasksInternal(SiteInfoIdentifier site)
        {
            var where = (site != null) ? new WhereCondition().WhereID("TaskSiteID", site.ObjectID) : null;

            BulkDelete(where);
        }


        /// <summary>
        /// Deletes specified exportTask.
        /// </summary>
        /// <param name="exportTaskId">ExportTask id</param>
        protected virtual void DeleteExportTaskInfoInternal(int exportTaskId)
        {
            ExportTaskInfo exportTaskObj = GetExportTaskInfo(exportTaskId);
            DeleteExportTaskInfo(exportTaskObj);
        }


        /// <summary>
        /// Returns the tasks where condition.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        protected virtual WhereCondition GetTasksWhereConditionInternal(int siteId, string objectTypes, WhereCondition where)
        {
            // typeWhere condition to select tasks for specified type and site (or global)
            var typeWhere = new WhereCondition().WhereNotNull("TaskObjectID");
            if (siteId > 0)
            {
                typeWhere.Where("TaskSiteID", QueryOperator.Equals, siteId);
            }
            else
            {
                typeWhere.WhereNull("TaskSiteID");
            }

            if (!string.IsNullOrEmpty(objectTypes))
            {
                List<string> types = SqlHelper.GetSafeQueryString(objectTypes, false).Split(';').ToList();
                typeWhere.WhereIn("TaskObjectType", types);

            }

            typeWhere.Where(where);

            return typeWhere;
        }


        /// <summary>
        /// Returns all ExportTaskInfos of specified object types and site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        protected virtual DataSet SelectTaskListInternal(int siteId, string objectTypes, string where, string orderBy, int topN, string columns)
        {
            int totalRecords = 0;
            return SelectTaskListInternal(siteId, objectTypes, where, orderBy, topN, columns, 0, 0, ref totalRecords);
        }


        /// <summary>
        /// Returns all ExportTaskInfos of specified object types and site.
        /// </summary>
        /// <param name="siteId">Site ID</param>
        /// <param name="objectTypes">Object types</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns</param>
        /// <param name="topN">Select only top N rows</param>
        /// <param name="columns">Select only specified columns</param>
        /// <param name="offset">Index of first record to get</param>
        /// <param name="maxRecords">Maximum number of records to get. If maxRecords is zero or less, all records are returned (no paging is used)</param>
        /// <param name="totalRecords">Returns total records</param>
        protected virtual DataSet SelectTaskListInternal(int siteId, string objectTypes, string where, string orderBy, int topN, string columns, int offset, int maxRecords, ref int totalRecords)
        {
            var safeWhere = GetTasksWhereConditionInternal(siteId, objectTypes, new WhereCondition(where));

            var query = GetExportTasks().Where(safeWhere).OrderBy(orderBy).TopN(topN).Columns(columns).BinaryData(false);
            query.Offset = offset;
            query.MaxRecords = maxRecords;

            var result = query.TypedResult;
            totalRecords = query.TotalRecords;

            return result;
        }


        /// <summary>
        /// Logs the task for the given object.
        /// </summary>
        /// <param name="infoObj">Object to log</param>
        /// <param name="taskType">Task type</param>
        /// <returns>Returns new task</returns>
        protected virtual ExportTaskInfo LogTaskInternal(GeneralizedInfo infoObj, TaskTypeEnum taskType)
        {
            if (CheckExportLogging(infoObj, taskType))
            {
                string xml;

                // Add additional data based on the task type
                switch (taskType)
                {
                    // Object deletion
                    case TaskTypeEnum.DeleteObject:
                        {
                            // Uses same XML as for staging
                            var settings = new SynchronizationObjectSettings();
                            settings.Operation = OperationTypeEnum.Synchronization;
                            settings.IncludeOtherBindings = false;
                            settings.IncludeChildren = false;
                            settings.IncludeCategories = false;
                            xml = SynchronizationHelper.GetObjectXml(settings, infoObj, taskType);
                        }
                        break;

                    default:
                        throw new Exception("[ExportTaskInfoProvider.LogTask]: Unknown task type '" + taskType + "'.");
                }

                // Create synchronization task
                ExportTaskInfo ti = new ExportTaskInfo();
                ti.TaskData = xml;
                ti.TaskObjectID = infoObj.ObjectID;
                ti.TaskObjectType = infoObj.TypeInfo.ObjectType;
                ti.TaskTime = DateTime.Now;
                if (!string.IsNullOrEmpty(infoObj.ObjectSiteName))
                {
                    ti.TaskSiteID = ProviderHelper.GetId(SiteInfo.OBJECT_TYPE, infoObj.ObjectSiteName);
                }

                string prefferedUICulture = CultureHelper.PreferredUICultureCode;
                string name = ResHelper.GetAPIString("ObjectType." + infoObj.TypeInfo.ObjectType.Replace(".", "_"), prefferedUICulture, infoObj.TypeInfo.ObjectType) + " '" + ResHelper.LocalizeString(infoObj.ObjectDisplayName) + "'";

                // Task title
                string title;
                switch (taskType)
                {
                    case TaskTypeEnum.DeleteObject:
                        title = ResHelper.GetAPIString("TaskTitle.DeleteObject", prefferedUICulture, "Delete {0}");
                        break;

                    default:
                        title = ResHelper.GetAPIString("TaskTitle.Unknown", prefferedUICulture, "[Unknown] {0}");
                        break;
                }

                ti.TaskTitle = TextHelper.LimitLength(String.Format(title, name), 450);
                ti.TaskType = taskType;

                // Save the task
                SetExportTaskInfo(ti);

                return ti;
            }

            return null;
        }


        /// <summary>
        /// Indicates if the object changes should be logged for the export.
        /// </summary>
        /// <param name="siteName">Site name</param>
        protected virtual bool LogObjectChangesInternal(string siteName)
        {
            return SettingsKeyInfoProvider.GetBoolValue(siteName + ".CMSExportLogObjectChanges");
        }


        /// <summary>
        /// Indicates if the export task should be logged.
        /// </summary>
        /// <param name="infoObj">Info object instance</param>
        /// <param name="taskType">Task type</param>
        protected virtual bool CheckExportLoggingInternal(GeneralizedInfo infoObj, TaskTypeEnum taskType)
        {
            return infoObj.LogExport && (taskType == TaskTypeEnum.DeleteObject) && LogObjectChanges(infoObj.ObjectSiteName);
        }

        #endregion
    }
}
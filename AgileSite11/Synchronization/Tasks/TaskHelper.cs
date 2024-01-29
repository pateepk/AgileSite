using CMS.DataEngine;
using CMS.Synchronization;

namespace CMS.Base
{
    /// <summary>
    /// Task management methods.
    /// </summary>
    public static class TaskHelper
    {
        #region "Constants"

        /// <summary>
        /// Enum category in TaskTypeEnum to select content related task types.
        /// </summary>
        public const string TASK_TYPE_CATEGORY_CONTENT = "ContentStaging";

        /// <summary>
        /// Enum category in TaskTypeEnum to select objects and data related task types.
        /// </summary>
        public const string TASK_TYPE_CATEGORY_OBJECTS = "ObjectStaging";

        /// <summary>
        /// Enum category in TaskTypeEnum to select general task types.
        /// </summary>
        public const string TASK_TYPE_CATEGORY_DATA = "DataStaging";

        /// <summary>
        /// Enum category in TaskTypeEnum to select general task types.
        /// </summary>
        public const string TASK_TYPE_CATEGORY_GENERAL = "General";

        /// <summary>
        /// Prefix for resource strings that localize TaskTypeEnum
        /// </summary>
        public const string TASK_TYPE_RESOURCE_STRING_PREFIX = "tasktypeenum";

        #endregion


        /// <summary>
        /// Returns the task type string.
        /// </summary>
        /// <param name="taskType">Task type</param>
        public static string GetTaskTypeString(TaskTypeEnum taskType)
        {
            switch (taskType)
            {
                case TaskTypeEnum.DeleteDocument:
                    return "DELETEDOC";

                case TaskTypeEnum.PublishDocument:
                    return "PUBLISHDOC";

                case TaskTypeEnum.ArchiveDocument:
                    return "ARCHIVEDOC";

                case TaskTypeEnum.RejectDocument:
                    return "REJECTDOC";

                case TaskTypeEnum.UpdateDocument:
                    return "UPDATEDOC";

                case TaskTypeEnum.CreateDocument:
                    return "CREATEDOC";

                case TaskTypeEnum.DeleteAllCultures:
                    return "DELETEALLCULTURES";

                case TaskTypeEnum.MoveDocument:
                    return "MOVEDOC";

                case TaskTypeEnum.CreateObject:
                    return "CREATEOBJ";

                case TaskTypeEnum.UpdateObject:
                    return "UPDATEOBJ";

                case TaskTypeEnum.DeleteObject:
                    return "DELETEOBJ";

                case TaskTypeEnum.CreateMediaFolder:
                    return "CREATEFOLDER";

                case TaskTypeEnum.CopyMediaFolder:
                    return "COPYFOLDER";

                case TaskTypeEnum.MoveMediaFolder:
                    return "MOVEFOLDER";

                case TaskTypeEnum.DeleteMediaFolder:
                    return "DELETEFOLDER";

                case TaskTypeEnum.DeleteMediaRootFolder:
                    return "DELETEROOTFOLDER";

                case TaskTypeEnum.RenameMediaFolder:
                    return "RENAMEFOLDER";

                case TaskTypeEnum.AddToSite:
                    return "ADDTOSITE";

                case TaskTypeEnum.RemoveFromSite:
                    return "REMOVEFROMSITE";

                case TaskTypeEnum.BreakACLInheritance:
                    return "BREAKACLINHERITANCE";

                case TaskTypeEnum.RestoreACLInheritance:
                    return "RESTOREACLINHERITANCE";

                default:
                    return "UNKNOWN";
            }
        }


        /// <summary>
        /// Returns the task type enumeration value.
        /// </summary>
        /// <param name="taskType">String task type representation</param>
        public static TaskTypeEnum GetTaskTypeEnum(string taskType)
        {
            if (taskType == null)
            {
                return TaskTypeEnum.Unknown;
            }

            switch (taskType.ToLowerCSafe())
            {
                case "deletedoc":
                    return TaskTypeEnum.DeleteDocument;

                case "updatedoc":
                    return TaskTypeEnum.UpdateDocument;

                case "createdoc":
                    return TaskTypeEnum.CreateDocument;

                case "publishdoc":
                    return TaskTypeEnum.PublishDocument;

                case "archivedoc":
                    return TaskTypeEnum.ArchiveDocument;

                case "rejectdoc":
                    return TaskTypeEnum.RejectDocument;

                case "deleteallcultures":
                    return TaskTypeEnum.DeleteAllCultures;

                case "movedoc":
                    return TaskTypeEnum.MoveDocument;

                case "createobj":
                    return TaskTypeEnum.CreateObject;

                case "updateobj":
                    return TaskTypeEnum.UpdateObject;

                case "deleteobj":
                    return TaskTypeEnum.DeleteObject;

                case "createfolder":
                    return TaskTypeEnum.CreateMediaFolder;

                case "copyfolder":
                    return TaskTypeEnum.CopyMediaFolder;

                case "movefolder":
                    return TaskTypeEnum.MoveMediaFolder;

                case "deletefolder":
                    return TaskTypeEnum.DeleteMediaFolder;

                case "deleterootfolder":
                    return TaskTypeEnum.DeleteMediaRootFolder;

                case "renamefolder":
                    return TaskTypeEnum.RenameMediaFolder;

                case "addtosite":
                    return TaskTypeEnum.AddToSite;

                case "removefromsite":
                    return TaskTypeEnum.RemoveFromSite;

                case "breakaclinheritance":
                    return TaskTypeEnum.BreakACLInheritance;

                case "restoreaclinheritance":
                    return TaskTypeEnum.RestoreACLInheritance;

                default:
                    return TaskTypeEnum.Unknown;
            }
        }


        /// <summary>
        /// Indicates if the task type shouldn't be processed asynchronously.
        /// </summary>
        /// <param name="taskType">Task type</param>
        public static bool IsExcludedAsyncTask(TaskTypeEnum taskType)
        {
            switch (taskType)
            {
                case TaskTypeEnum.DeleteObject:
                case TaskTypeEnum.DeleteDocument:
                case TaskTypeEnum.DeleteAllCultures:
                case TaskTypeEnum.DeleteMediaFolder:
                case TaskTypeEnum.DeleteMediaRootFolder:
                    return true;
            }

            return false;
        }
    }
}
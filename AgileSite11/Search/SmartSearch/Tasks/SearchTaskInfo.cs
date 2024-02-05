using System;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Search;

[assembly: RegisterObjectType(typeof(SearchTaskInfo), SearchTaskInfo.OBJECT_TYPE)]

namespace CMS.Search
{
    /// <summary>
    /// SearchTaskInfo data container class.
    /// </summary>
    public class SearchTaskInfo : AbstractInfo<SearchTaskInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.searchtask";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SearchTaskInfoProvider), OBJECT_TYPE, "CMS.SearchTask", "SearchTaskID", null, null, null, null, null, null, null, null)
        {
            SupportsVersioning = false,
            LogIntegration = false,
            SupportsCloning = false,
            ContainsMacros = false
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the task creation time.
        /// </summary>
        [DatabaseField]
        public virtual DateTime SearchTaskCreated
        {
            get
            {
                return GetDateTimeValue("SearchTaskCreated", DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Search task field.
        /// </summary>
        [DatabaseField]
        public virtual string SearchTaskField
        {
            get
            {
                return GetStringValue("SearchTaskField", "");
            }
            set
            {
                SetValue("SearchTaskField", value);
            }
        }


        /// <summary>
        /// Object type of search task.
        /// </summary>
        [DatabaseField]
        public virtual string SearchTaskObjectType
        {
            get
            {
                return GetStringValue("SearchTaskObjectType", "");
            }
            set
            {
                SetValue("SearchTaskObjectType", value);
            }
        }


        /// <summary>
        /// Search task type.
        /// </summary>
        [DatabaseField(ColumnName = "SearchTaskType", ValueType = typeof(string))]
        public virtual SearchTaskTypeEnum SearchTaskType
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SearchTaskType"), "").ToEnum<SearchTaskTypeEnum>();
            }
            set
            {
                SetValue("SearchTaskType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Search task status.
        /// </summary>
        [DatabaseField(ColumnName = "SearchTaskStatus", ValueType = typeof(string))]
        public virtual SearchTaskStatusEnum SearchTaskStatus
        {
            get
            {
                return ValidationHelper.GetString(GetValue("SearchTaskStatus"), "").ToEnum<SearchTaskStatusEnum>();
            }
            set
            {
                SetValue("SearchTaskStatus", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Search task server name.
        /// </summary>
        [DatabaseField]
        public virtual string SearchTaskServerName
        {
            get
            {
                return GetStringValue("SearchTaskServerName", "");
            }
            set
            {
                SetValue("SearchTaskServerName", value);
            }
        }


        /// <summary>
        /// Search task ID.
        /// </summary>
        [DatabaseField]
        public virtual int SearchTaskID
        {
            get
            {
                return GetIntegerValue("SearchTaskID", 0);
            }
            set
            {
                SetValue("SearchTaskID", value);
            }
        }


        /// <summary>
        /// Search task value.
        /// </summary>
        [DatabaseField]
        public virtual string SearchTaskValue
        {
            get
            {
                return GetStringValue("SearchTaskValue", "");
            }
            set
            {
                SetValue("SearchTaskValue", value);
            }
        }


        /// <summary>
        /// SearchTaskPriority determines which task should be processed earlier, higher value = higher priority.
        /// </summary>
        [DatabaseField]
        public virtual int SearchTaskPriority
        {
            get
            {
                return GetIntegerValue("SearchTaskPriority", 0);
            }
            set
            {
                SetValue("SearchTaskPriority", value);
            }
        }


        /// <summary>
        /// Contains error message if error occured while processing the task. 
        /// </summary>
        [DatabaseField]
        public virtual string SearchTaskErrorMessage
        {
            get
            {
                return GetStringValue("SearchTaskErrorMessage", "");
            }
            set
            {
                SetValue("SearchTaskErrorMessage", value);
            }
        }


        /// <summary>
        /// ID of the object this task relates to. e.g. rebuild task - ID of an index, update task - ID of object that was changed. 
        /// </summary>
        [DatabaseField]
        public virtual int SearchTaskRelatedObjectID
        {
            get
            {
                return GetIntegerValue("SearchTaskRelatedObjectID", 0);
            }
            set
            {
                SetValue("SearchTaskRelatedObjectID", value);
            }
        }


        /// <summary>
        /// Returns object type of the task related object. 
        /// </summary>
        [DatabaseField]
        public virtual string SearchTaskRelatedObjectType
        {
            get
            {
                return SearchTaskInfoProvider.GetSearchTaskRelatedObjectType(SearchTaskObjectType, SearchTaskType);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SearchTaskInfoProvider.DeleteSearchTaskInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SearchTaskInfoProvider.SetSearchTaskInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SearchTaskInfo object.
        /// </summary>
        public SearchTaskInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SearchTaskInfo object from the given DataRow.
        /// </summary>
        public SearchTaskInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
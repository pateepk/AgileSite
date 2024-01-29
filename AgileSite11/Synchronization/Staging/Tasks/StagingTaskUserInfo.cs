using System;
using System.Data;
using System.Runtime.Serialization;
using System.Collections.Generic;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(StagingTaskUserInfo), StagingTaskUserInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// StagingTaskUserInfo data container class.
    /// </summary>
    [Serializable]
    public class StagingTaskUserInfo : AbstractInfo<StagingTaskUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "staging.taskuser";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(StagingTaskUserInfoProvider), OBJECT_TYPE, "Staging.TaskUser", "TaskUserID", null, null, null, null, null, null, "TaskID", "staging.task")
        {
            IsBinding = true,
            ModuleName = "CMS.Staging",
            TouchCacheDependencies = true,
            DependsOn = new List<ObjectDependency>() 
			{
			    new ObjectDependency("UserID", "cms.user", ObjectDependencyEnum.Binding), 
            },
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Identifies binding between StagingTaskInfo and UserInfo.
        /// </summary>
        [DatabaseField]
        public virtual int TaskUserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TaskUserID"), 0);
            }
            set
            {
                SetValue("TaskUserID", value);
            }
        }


        /// <summary>
        /// Identifies StagingTaskInfo in binding.
        /// </summary>
        [DatabaseField]
        public virtual int TaskID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("TaskID"), 0);
            }
            set
            {
                SetValue("TaskID", value);
            }
        }


        /// <summary>
        /// Identifies UserInfo in binding.
        /// </summary>
        [DatabaseField]
        public virtual int UserID
        {
            get
            {
                return ValidationHelper.GetInteger(GetValue("UserID"), 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            StagingTaskUserInfoProvider.DeleteStagingTaskUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            StagingTaskUserInfoProvider.SetStagingTaskUserInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates instance for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public StagingTaskUserInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO)
        {
        }


        /// <summary>
        /// Creates an empty TaskUserInfo object.
        /// </summary>
        public StagingTaskUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Creates a new TaskUserInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public StagingTaskUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
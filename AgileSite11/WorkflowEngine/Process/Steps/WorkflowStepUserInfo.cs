using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowStepUserInfo), WorkflowStepUserInfo.OBJECT_TYPE)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// WorkflowStepUserInfo data container class.
    /// </summary>
    public class WorkflowStepUserInfo : AbstractInfo<WorkflowStepUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.WORKFLOWSTEPUSER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowStepUserInfoProvider), OBJECT_TYPE, "CMS.WorkflowStepUser", "WorkflowStepUserID", null, null, null, null, null, null, "StepID", WorkflowStepInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("UserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsBinding = true,
            RegisterAsBindingToObjectTypes = new List<string>() { WorkflowStepInfo.OBJECT_TYPE, WorkflowStepInfo.OBJECT_TYPE_AUTOMATION },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// WorkflowStepUser ID.
        /// </summary>
        public virtual int WorkflowStepUserID
        {
            get
            {
                return GetIntegerValue("WorkflowStepUserID", 0);
            }
            set
            {
                SetValue("WorkflowStepUserID", value);
            }
        }


        /// <summary>
        /// Step ID.
        /// </summary>
        public virtual int StepID
        {
            get
            {
                return GetIntegerValue("StepID", 0);
            }
            set
            {
                SetValue("StepID", value);
            }
        }


        /// <summary>
        /// User ID.
        /// </summary>
        public virtual int UserID
        {
            get
            {
                return GetIntegerValue("UserID", 0);
            }
            set
            {
                SetValue("UserID", value);
            }
        }


        /// <summary>
        /// Step source point GUID.
        /// </summary>
        public virtual Guid StepSourcePointGUID
        {
            get
            {
                return GetGuidValue("StepSourcePointGUID", Guid.Empty);
            }
            set
            {
                SetValue("StepSourcePointGUID", value, Guid.Empty);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WorkflowStepUserInfoProvider.DeleteWorkflowStepUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowStepUserInfoProvider.SetWorkflowStepUserInfo(this);
        }


        /// <summary>
        /// Gets existing object
        /// </summary>
        /// <returns>Existing workflow step user object</returns>
        protected override BaseInfo GetExisting()
        {
            // Get existing item
            return WorkflowStepUserInfoProvider.GetWorkflowStepUserInfo(StepID, UserID, StepSourcePointGUID);
        }


        /// <summary>
        /// Gets a where condition to find an existing object based on current object
        /// </summary>
        protected override WhereCondition GetExistingWhereCondition()
        {
            var where = base.GetExistingWhereCondition();
            where.WhereEquals("StepSourcePointGUID", StepSourcePointGUID);

            return where;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowStepUserInfo object.
        /// </summary>
        public WorkflowStepUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WorkflowStepUserInfo object from the given DataRow.
        /// </summary>
        public WorkflowStepUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
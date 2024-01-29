using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowStepRoleInfo), WorkflowStepRoleInfo.OBJECT_TYPE)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// WorkflowStepRoleInfo data container class.
    /// </summary>
    public class WorkflowStepRoleInfo : AbstractInfo<WorkflowStepRoleInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.WORKFLOWSTEPROLE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowStepRoleInfoProvider), OBJECT_TYPE, "CMS.WorkflowStepRole", "WorkflowStepRoleID", null, null, null, null, null, null, "StepID", WorkflowStepInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("RoleID", RoleInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },   
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsBinding = true,
            RegisterAsOtherBindingToObjectTypes = new List<string>() { RoleInfo.OBJECT_TYPE, RoleInfo.OBJECT_TYPE_GROUP },
            RegisterAsBindingToObjectTypes = new List<string>() { WorkflowStepInfo.OBJECT_TYPE, WorkflowStepInfo.OBJECT_TYPE_AUTOMATION },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// WorkflowStepRole ID.
        /// </summary>
        public virtual int WorkflowStepRoleID
        {
            get
            {
                return GetIntegerValue("WorkflowStepRoleID", 0);
            }
            set
            {
                SetValue("WorkflowStepRoleID", value);
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
        /// Role ID.
        /// </summary>
        public virtual int RoleID
        {
            get
            {
                return GetIntegerValue("RoleID", 0);
            }
            set
            {
                SetValue("RoleID", value);
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
            WorkflowStepRoleInfoProvider.DeleteWorkflowStepRoleInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowStepRoleInfoProvider.SetWorkflowStepRoleInfo(this);
        }


        /// <summary>
        /// Gets existing object
        /// </summary>
        /// <returns>Existing workflow step role object</returns>
        protected override BaseInfo GetExisting()
        {
            // Get existing item
            return WorkflowStepRoleInfoProvider.GetWorkflowStepRoleInfo(StepID, RoleID, StepSourcePointGUID);
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
        /// Constructor - Creates an empty WorkflowStepRoleInfo object.
        /// </summary>
        public WorkflowStepRoleInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WorkflowStepRoleInfo object from the given DataRow.
        /// </summary>
        public WorkflowStepRoleInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
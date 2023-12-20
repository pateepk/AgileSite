using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Membership;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowUserInfo), WorkflowUserInfo.OBJECT_TYPE)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// WorkflowUserInfo data container class.
    /// </summary>
    public class WorkflowUserInfo : AbstractInfo<WorkflowUserInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.WORKFLOWUSER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowUserInfoProvider), OBJECT_TYPE, "CMS.WorkflowUser", null, null, null, null, null, null, null, "WorkflowID", WorkflowInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("UserID", UserInfo.OBJECT_TYPE, ObjectDependencyEnum.Binding) },
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Workflow ID.
        /// </summary>
        public virtual int WorkflowID
        {
            get
            {
                return GetIntegerValue("WorkflowID", 0);
            }
            set
            {
                SetValue("WorkflowID", value);
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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WorkflowUserInfoProvider.DeleteWorkflowUserInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowUserInfoProvider.SetWorkflowUserInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowUserInfo object.
        /// </summary>
        public WorkflowUserInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WorkflowUserInfo object from the given DataRow.
        /// </summary>
        public WorkflowUserInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
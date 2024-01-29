using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.Synchronization;

[assembly: RegisterObjectType(typeof(SynchronizationInfo), SynchronizationInfo.OBJECT_TYPE)]

namespace CMS.Synchronization
{
    /// <summary>
    /// SynchronizationInfo data container class.
    /// </summary>
    public class SynchronizationInfo : AbstractInfo<SynchronizationInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "staging.synchronization";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SynchronizationInfoProvider), OBJECT_TYPE, "Staging.Synchronization", "SynchronizationID", null, null, null, null, null, null, "SynchronizationTaskID", StagingTaskInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("SynchronizationServerID", ServerInfo.OBJECT_TYPE, ObjectDependencyEnum.Required) },
            SupportsVersioning = false,
            LogIntegration = false,
            AllowRestore = false,
            ModuleName = "cms.staging",
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Synchronization ID.
        /// </summary>
        public virtual int SynchronizationID
        {
            get
            {
                return GetIntegerValue("SynchronizationID", 0);
            }
            set
            {
                SetValue("SynchronizationID", value);
            }
        }


        /// <summary>
        /// Synchronization error message.
        /// </summary>
        public virtual string SynchronizationErrorMessage
        {
            get
            {
                return GetStringValue("SynchronizationErrorMessage", "");
            }
            set
            {
                SetValue("SynchronizationErrorMessage", value);
            }
        }


        /// <summary>
        /// Synchronization last run.
        /// </summary>
        public virtual DateTime SynchronizationLastRun
        {
            get
            {
                return GetDateTimeValue("SynchronizationLastRun", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("SynchronizationLastRun", value);
            }
        }


        /// <summary>
        /// Synchronization task ID.
        /// </summary>
        public virtual int SynchronizationTaskID
        {
            get
            {
                return GetIntegerValue("SynchronizationTaskID", 0);
            }
            set
            {
                SetValue("SynchronizationTaskID", value);
            }
        }


        /// <summary>
        /// Synchronization server ID.
        /// </summary>
        public virtual int SynchronizationServerID
        {
            get
            {
                return GetIntegerValue("SynchronizationServerID", 0);
            }
            set
            {
                SetValue("SynchronizationServerID", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SynchronizationInfoProvider.DeleteSynchronizationInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SynchronizationInfoProvider.SetSynchronizationInfo(this);
        }


        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            bool allowed = false;
            switch (permission)
            {
                case PermissionsEnum.Read:
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                    allowed = userInfo.IsAuthorizedPerResource(TypeInfo.ModuleName, "ManageAllTasks", siteName, false);
                    break;
            }

            return allowed || base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty SynchronizationInfo object.
        /// </summary>
        public SynchronizationInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new SynchronizationInfo object from the given DataRow.
        /// </summary>
        public SynchronizationInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
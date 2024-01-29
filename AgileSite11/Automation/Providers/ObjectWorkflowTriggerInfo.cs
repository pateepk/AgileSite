using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Base;
using CMS.WorkflowEngine;
using CMS.Automation;

[assembly: RegisterObjectType(typeof(ObjectWorkflowTriggerInfo), ObjectWorkflowTriggerInfo.OBJECT_TYPE)]

namespace CMS.Automation
{
    /// <summary>
    /// ObjectWorkflowTriggerInfo data container class.
    /// </summary>
    public class ObjectWorkflowTriggerInfo : AbstractInfo<ObjectWorkflowTriggerInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.AUTOMATIONWORKFLOWTRIGGER;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ObjectWorkflowTriggerInfoProvider), OBJECT_TYPE, "CMS.ObjectWorkflowTrigger", "TriggerID", "TriggerLastModified", "TriggerGUID", null, "TriggerDisplayName", null, null, "TriggerWorkflowID", WorkflowInfo.OBJECT_TYPE_AUTOMATION)
        {
            MacroCollectionName = "CMS.WorkflowTrigger",
            ModuleName = ModuleName.ONLINEMARKETING,
            SupportsCloning = false,
            SupportsVersioning = false,
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, AutomationModule.ONLINEMARKETING),
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsGlobalObjects = true,
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("TriggerTargetObjectID", null, ObjectDependencyEnum.Required, "TriggerTargetObjectType")
            },
            DefaultData = new DefaultDataSettings(),
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields =
                {
                    "TriggerDisplayName"
                },
                DependencyColumns =
                {
                    "TriggerTargetObjectID",
                    "TriggerTargetObjectType"
                }
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("TriggerParameters")
                }
            }
        };

        #endregion


        #region "Variables"

        private ObjectParameters mTriggerParameters = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Trigger ID
        /// </summary>
        public virtual int TriggerID
        {
            get
            {
                return GetIntegerValue("TriggerID", 0);
            }
            set
            {
                SetValue("TriggerID", value);
            }
        }


        /// <summary>
        /// Trigger GUID
        /// </summary>
        public virtual Guid TriggerGUID
        {
            get
            {
                return GetGuidValue("TriggerGUID", Guid.Empty);
            }
            set
            {         
                SetValue("TriggerGUID", value);
            }
        }


        /// <summary>
        /// Trigger last modified
        /// </summary>
        public virtual DateTime TriggerLastModified
        {
            get
            {
                return GetDateTimeValue("TriggerLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {         
                SetValue("TriggerLastModified", value);
            }
        }


        /// <summary>
        /// Trigger display name
        /// </summary>
        public virtual string TriggerDisplayName
        {
            get
            {
                return GetStringValue("TriggerDisplayName", "");
            }
            set
            {
                SetValue("TriggerDisplayName", value);
            }
        }


        /// <summary>
        /// Trigger macro condition
        /// </summary>
        public virtual string TriggerMacroCondition
        {
            get
            {
                return GetStringValue("TriggerMacroCondition", "");
            }
            set
            {         
                SetValue("TriggerMacroCondition", value);
            }
        }


        /// <summary>
        /// Trigger type
        /// </summary>
        public virtual WorkflowTriggerTypeEnum TriggerType
        {
            get
            {
                return (WorkflowTriggerTypeEnum)GetIntegerValue("TriggerType", (int)WorkflowTriggerTypeEnum.Creation);
            }
            set
            {         
                SetValue("TriggerType", (int)value);
            }
        }


        /// <summary>
        /// Trigger workflow ID
        /// </summary>
        public virtual int TriggerWorkflowID
        {
            get
            {
                return GetIntegerValue("TriggerWorkflowID", 0);
            }
            set
            {         
                SetValue("TriggerWorkflowID", value);
            }
        }


        /// <summary>
        /// Object type trigger can be applied to.
        /// </summary>
        public virtual string TriggerObjectType
        {
            get
            {
                return GetStringValue("TriggerObjectType", "");
            }
            set
            {
                SetValue("TriggerObjectType", value);
            }
        }


        /// <summary>
        /// Type of concrete object of trigger.
        /// </summary>
        public virtual string TriggerTargetObjectType
        {
            get
            {
                return GetStringValue("TriggerTargetObjectType", "");
            }
            set
            {
                SetValue("TriggerTargetObjectType", value);
            }
        }


        /// <summary>
        /// ID of concrete object of trigger.
        /// </summary>
        public virtual int TriggerTargetObjectID
        {
            get
            {
                return GetIntegerValue("TriggerTargetObjectID", 0);
            }
            set
            {
                SetValue("TriggerTargetObjectID", value);
            }
        }


        /// <summary>
        /// Parameters of trigger.
        /// </summary>
        public virtual ObjectParameters TriggerParameters
        {
            get
            {
                if (mTriggerParameters == null)
                {
                    mTriggerParameters = new ObjectParameters();
                    mTriggerParameters.LoadData(ValidationHelper.GetString(GetStringValue("TriggerParameters", String.Empty), String.Empty));
                }
                return mTriggerParameters;
            }
            set
            {
                mTriggerParameters = value;
                if (value == null)
                {
                    SetValue("TriggerParameters", DBNull.Value);
                }
                else
                {
                    SetValue("TriggerParameters", value.GetData(), "");
                }
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ObjectWorkflowTriggerInfoProvider.DeleteObjectWorkflowTriggerInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ObjectWorkflowTriggerInfoProvider.SetObjectWorkflowTriggerInfo(this);
        }


        /// <summary>
        /// Updates the object.
        /// </summary>
        protected override void UpdateData()
        {
            base.UpdateData();

            TouchParent();
        }


        /// <summary>
        /// Inserts the object.
        /// </summary>
        protected override void InsertData()
        {
            base.InsertData();

            TouchParent();
        }


        /// <summary>
        /// Deletes the object.
        /// </summary>
        protected override void DeleteData()
        {
            base.DeleteData();

            TouchParent();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ObjectWorkflowTriggerInfo object.
        /// </summary>
        public ObjectWorkflowTriggerInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ObjectWorkflowTriggerInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public ObjectWorkflowTriggerInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            // Always return true for global administrator
            if (userInfo.CheckPrivilegeLevel(UserPrivilegeLevelEnum.Admin))
            {
                return true;
            }

            switch (permission)
            {
                case PermissionsEnum.Read:
                    return userInfo.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "ReadProcesses", siteName, exceptionOnFailure);

                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Destroy:
                    return userInfo.IsAuthorizedPerResource(ModuleName.ONLINEMARKETING, "ManageProcesses", siteName, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }

        #endregion
    }
}

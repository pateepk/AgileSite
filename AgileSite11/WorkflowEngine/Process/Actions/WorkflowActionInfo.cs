using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowActionInfo), WorkflowActionInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(WorkflowActionInfo), WorkflowActionInfo.OBJECT_TYPE_AUTOMATION)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// WorkflowActionInfo data container class.
    /// </summary>
    public class WorkflowActionInfo : AbstractInfo<WorkflowActionInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.workflowaction";

        /// <summary>
        /// Object type for automation action
        /// </summary>
        public const string OBJECT_TYPE_AUTOMATION = "ma.automationaction";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowActionInfoProvider), OBJECT_TYPE, "CMS.WorkflowAction", "ActionID", "ActionLastModified", "ActionGUID", "ActionName", "ActionDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            ThumbnailGUIDColumn = "ActionThumbnailGUID",
            IconGUIDColumn = "ActionIconGUID",
            HasMetaFiles = true,
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ActionResourceID", PredefinedObjectType.RESOURCE) },
            ResourceIDColumn = "ActionResourceID",
            AssemblyNameColumn = "ActionAssemblyName",
            ModuleName = ModuleName.WORKFLOWENGINE,
            TypeCondition = new TypeCondition().WhereNotEqualsOrNull("ActionWorkflowType", (int)WorkflowTypeEnum.Automation),
            EnabledColumn = "ActionEnabled",
            FormDefinitionColumn = "ActionParameters",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            DefaultData = new DefaultDataSettings(),
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<DataDefinition>("ActionParameters")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };


        /// <summary>
        /// Type information for automation action.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO_AUTOMATION = new ObjectTypeInfo(typeof(WorkflowActionInfoProvider), OBJECT_TYPE_AUTOMATION, "CMS.WorkflowAction", "ActionID", "ActionLastModified", "ActionGUID", "ActionName", "ActionDisplayName", null, null, null, null)
        {
            OriginalTypeInfo = TYPEINFO,
            MacroCollectionName = "MA.AutomationAction",
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, WorkflowModule.ONLINEMARKETING)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            ThumbnailGUIDColumn = "ActionThumbnailGUID",
            IconGUIDColumn = "ActionIconGUID",
            HasMetaFiles = true,
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ActionResourceID", PredefinedObjectType.RESOURCE) },
            ResourceIDColumn = "ActionResourceID",
            AssemblyNameColumn = "ActionAssemblyName",
            ModuleName = ModuleName.ONLINEMARKETING,
            TypeCondition = new TypeCondition().WhereEquals("ActionWorkflowType", (int)WorkflowTypeEnum.Automation),
            EnabledColumn = "ActionEnabled",
            FormDefinitionColumn = "ActionParameters",
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, WorkflowModule.ONLINEMARKETING)
                }
            },
            DefaultData = new DefaultDataSettings(),
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<DataDefinition>("ActionParameters")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Type based properties"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (ActionWorkflowType == WorkflowTypeEnum.Automation)
                {
                    return TYPEINFO_AUTOMATION;
                }
                else
                {
                    return TYPEINFO;
                }
            }
        }


        #endregion


        #region "Properties"

        /// <summary>
        /// ID of action.
        /// </summary>
        [DatabaseField]
        public virtual int ActionID
        {
            get
            {
                return GetIntegerValue("ActionID", 0);
            }
            set
            {
                SetValue("ActionID", value);
            }
        }


        /// <summary>
        /// Action GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ActionGUID
        {
            get
            {
                return GetGuidValue("ActionGUID", Guid.Empty);
            }
            set
            {
                SetValue("ActionGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ActionLastModified
        {
            get
            {
                return GetDateTimeValue("ActionLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ActionLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Parameters of action.
        /// </summary>
        [DatabaseField]
        public virtual string ActionParameters
        {
            get
            {
                return GetStringValue("ActionParameters", "");
            }
            set
            {
                SetValue("ActionParameters", value, "");
            }
        }


        /// <summary>
        /// Class name of action.
        /// </summary>
        [DatabaseField]
        public virtual string ActionClass
        {
            get
            {
                return GetStringValue("ActionClass", "");
            }
            set
            {
                SetValue("ActionClass", value);
            }
        }


        /// <summary>
        /// Code name of action.
        /// </summary>
        [DatabaseField]
        public virtual string ActionName
        {
            get
            {
                return GetStringValue("ActionName", "");
            }
            set
            {
                SetValue("ActionName", value);
            }
        }


        /// <summary>
        /// Display name of action.
        /// </summary>
        [DatabaseField]
        public virtual string ActionDisplayName
        {
            get
            {
                return GetStringValue("ActionDisplayName", "");
            }
            set
            {
                SetValue("ActionDisplayName", value);
            }
        }


        /// <summary>
        /// Resource of action.
        /// </summary>
        [DatabaseField]
        public virtual int ActionResourceID
        {
            get
            {
                return GetIntegerValue("ActionResourceID", 0);
            }
            set
            {
                SetValue("ActionResourceID", value, (value > 0));
            }
        }


        /// <summary>
        /// Action description.
        /// </summary>
        [DatabaseField]
        public virtual string ActionDescription
        {
            get
            {
                return GetStringValue("ActionDescription", "");
            }
            set
            {
                SetValue("ActionDescription", value, "");
            }
        }


        /// <summary>
        /// Assembly name of action.
        /// </summary>
        [DatabaseField]
        public virtual string ActionAssemblyName
        {
            get
            {
                return GetStringValue("ActionAssemblyName", "");
            }
            set
            {
                SetValue("ActionAssemblyName", value);
            }
        }


        /// <summary>
        /// Thumbnail GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ActionThumbnailGUID
        {
            get
            {
                return GetGuidValue("ActionThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("ActionThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Thumbnail CSS font icon class name.
        /// </summary>
        [DatabaseField]
        public virtual string ActionThumbnailClass
        {
            get
            {
                return GetStringValue("ActionThumbnailClass", null);
            }
            set
            {
                SetValue("ActionThumbnailClass", value, "");
            }
        }


        /// <summary>
        /// Toolbar icon GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid ActionIconGUID
        {
            get
            {
                return GetGuidValue("ActionIconGUID", Guid.Empty);
            }
            set
            {
                SetValue("ActionIconGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Icon CSS font icon class name.
        /// </summary>
        [DatabaseField]
        public virtual string ActionIconClass
        {
            get
            {
                return GetStringValue("ActionIconClass", null);
            }
            set
            {
                SetValue("ActionIconClass", value, "");
            }
        }


        /// <summary>
        /// Indicates if action is enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool ActionEnabled
        {
            get
            {
                return GetBooleanValue("ActionEnabled", true);
            }
            set
            {
                SetValue("ActionEnabled", value);
            }
        }


        /// <summary>
        /// Gets or sets the workflow action allowed objects.
        /// </summary>
        [DatabaseField]
        public virtual string ActionAllowedObjects
        {
            get
            {
                return GetStringValue("ActionAllowedObjects", null);
            }
            set
            {
                SetValue("ActionAllowedObjects", value);
            }
        }


        /// <summary>
        /// Type of the workflow
        /// </summary>
        [DatabaseField(ValueType = typeof(int))]
        public WorkflowTypeEnum ActionWorkflowType
        {
            get
            {
                return (WorkflowTypeEnum)GetIntegerValue("ActionWorkflowType", (int)WorkflowTypeEnum.Approval);
            }
            set
            {
                SetValue("ActionWorkflowType", (int)value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Object full name if defined
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return ObjectHelper.BuildFullName(ActionName, ActionWorkflowType.ToString());
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WorkflowActionInfoProvider.DeleteWorkflowActionInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowActionInfoProvider.SetWorkflowActionInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowActionInfo object.
        /// </summary>
        public WorkflowActionInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WorkflowActionInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data.</param>
        public WorkflowActionInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}

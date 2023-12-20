using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Localization;
using CMS.WorkflowEngine;

[assembly: RegisterObjectType(typeof(WorkflowScopeInfo), WorkflowScopeInfo.OBJECT_TYPE)]

namespace CMS.WorkflowEngine
{
    /// <summary>
    /// Class to hold the workflow scope data.
    /// </summary>
    public class WorkflowScopeInfo : AbstractInfo<WorkflowScopeInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.workflowscope";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WorkflowScopeInfoProvider), OBJECT_TYPE, "CMS.WorkflowScope", "ScopeID", "ScopeLastModified", "ScopeGUID", null, "ScopeStartingPath", null, "ScopeSiteID", "ScopeWorkflowID", WorkflowInfo.OBJECT_TYPE)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ScopeClassID", DataClassInfo.OBJECT_TYPE), new ObjectDependency("ScopeCultureID", CultureInfo.OBJECT_TYPE) },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT)
                },
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,

            SupportsCloning = false,
            ImportExportSettings = { IsExportable = true, IsAutomaticallySelected = true, IncludeToExportParentDataSet = IncludeToParentEnum.None},
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                ObjectFileNameFields = { "ScopeStartingPath" }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Scope macro condition.
        /// </summary>
        public string ScopeMacroCondition
        {
            get
            {
                return GetStringValue("ScopeMacroCondition", "");
            }
            set
            {
                SetValue("ScopeMacroCondition", value);
            }
        }


        /// <summary>
        /// Scope starting path - The scope applies to the starting path subtree.
        /// </summary>
        public string ScopeStartingPath
        {
            get
            {
                return GetStringValue("ScopeStartingPath", "/");
            }
            set
            {
                SetValue("ScopeStartingPath", value);
            }
        }


        /// <summary>
        /// Scope ID.
        /// </summary>
        public int ScopeID
        {
            get
            {
                return GetIntegerValue("ScopeID", 0);
            }
            set
            {
                SetValue("ScopeID", value);
            }
        }


        /// <summary>
        /// Scope class ID.
        /// </summary>
        public int ScopeClassID
        {
            get
            {
                return GetIntegerValue("ScopeClassID", 0);
            }
            set
            {
                SetValue("ScopeClassID", value, (value > 0));
            }
        }


        /// <summary>
        /// Scope workflow ID.
        /// </summary>
        public int ScopeWorkflowID
        {
            get
            {
                return GetIntegerValue("ScopeWorkflowID", 0);
            }
            set
            {
                SetValue("ScopeWorkflowID", value);
            }
        }


        /// <summary>
        /// Scope site ID.
        /// </summary>
        public int ScopeSiteID
        {
            get
            {
                return GetIntegerValue("ScopeSiteID", 0);
            }
            set
            {
                SetValue("ScopeSiteID", value);
            }
        }


        /// <summary>
        /// Scope step GUID.
        /// </summary>
        public virtual Guid ScopeGUID
        {
            get
            {
                return GetGuidValue("ScopeGUID", Guid.Empty);
            }
            set
            {
                SetValue("ScopeGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime ScopeLastModified
        {
            get
            {
                return GetDateTimeValue("ScopeLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ScopeLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Scope culture ID.
        /// </summary>
        public int ScopeCultureID
        {
            get
            {
                return GetIntegerValue("ScopeCultureID", 0);
            }
            set
            {
                SetValue("ScopeCultureID", value, (value > 0));
            }
        }


        /// <summary>
        /// Indicates if child nodes should be excluded from scope.
        /// </summary>
        public bool ScopeExcludeChildren
        {
            get
            {
                return GetBooleanValue("ScopeExcludeChildren", false);
            }
            set
            {
                SetValue("ScopeExcludeChildren", value);
            }
        }


        /// <summary>
        /// Indicates if the scope is used to exclude the workflow.
        /// </summary>
        public bool ScopeExcluded
        {
            get
            {
                return GetBooleanValue("ScopeExcluded", false);
            }
            set
            {
                SetValue("ScopeExcluded", value);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WorkflowScopeInfoProvider.DeleteWorkflowScopeInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WorkflowScopeInfoProvider.SetWorkflowScopeInfo(this);
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


        #region "Methods"

        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Keep the scope path (even though it's display name, it should not be changed)
            ScopeStartingPath = originalObject.GetStringValue("ScopeStartingPath", ScopeStartingPath);
            Insert();
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WorkflowScopeInfo structure.
        /// </summary>
        public WorkflowScopeInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates the WorkflowScopeInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the workflow scope info data</param>
        public WorkflowScopeInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
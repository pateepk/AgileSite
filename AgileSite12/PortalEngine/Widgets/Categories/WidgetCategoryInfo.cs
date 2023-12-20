using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using CMS.Core;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WidgetCategoryInfo), WidgetCategoryInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Widget category info class.
    /// </summary>
    public class WidgetCategoryInfo : AbstractInfo<WidgetCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.widgetcategory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WidgetCategoryInfoProvider), OBJECT_TYPE, "CMS.WidgetCategory", "WidgetCategoryID", "WidgetCategoryLastModified", "WidgetCategoryGUID", "WidgetCategoryName", "WidgetCategoryDisplayName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("WidgetCategoryParentID", OBJECT_TYPE, ObjectDependencyEnum.Required) },
            ModuleName = ModuleName.WIDGETS,
            ImportExportSettings =
            {
                LogExport = false,
                OrderBy = "WidgetCategoryLevel ASC",
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
                ExcludedStagingColumns = new List<string>
                {
                    "WidgetCategoryChildCount",
                    "WidgetCategoryWidgetChildCount"
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsCategory = true,
            AllowRestore = false,
            DefaultOrderBy = "WidgetCategoryLevel ASC, WidgetCategoryName",
            ObjectNamePathColumn = "WidgetCategoryPath",
            ObjectLevelColumn = "WidgetCategoryLevel",
            DefaultData = new DefaultDataSettings
            {
                OrderBy = "WidgetCategoryLevel, WidgetCategoryParentID",
                ChildDependencies = new List<DefaultDataChildDependency>
                {
                    new DefaultDataChildDependency("WidgetCategoryID", "WidgetCategoryChildCount", OBJECT_TYPE, "WidgetCategoryParentID"),
                    new DefaultDataChildDependency("WidgetCategoryID", "WidgetCategoryWidgetChildCount", WidgetInfo.OBJECT_TYPE, "WidgetCategoryID")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "WidgetCategoryChildCount",
                    "WidgetCategoryWidgetChildCount"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Widget category ID.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetCategoryID
        {
            get
            {
                return GetIntegerValue("WidgetCategoryID", 0);
            }
            set
            {
                SetValue("WidgetCategoryID", value);
            }
        }


        /// <summary>
        /// Widget category name.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetCategoryName
        {
            get
            {
                return GetStringValue("WidgetCategoryName", String.Empty);
            }
            set
            {
                SetValue("WidgetCategoryName", value);
            }
        }


        /// <summary>
        /// Display name.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetCategoryDisplayName
        {
            get
            {
                return GetStringValue("WidgetCategoryDisplayName", String.Empty);
            }
            set
            {
                SetValue("WidgetCategoryDisplayName", value);
            }
        }


        /// <summary>
        /// Widget category GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WidgetCategoryGUID
        {
            get
            {
                return GetGuidValue("WidgetCategoryGUID", Guid.Empty);
            }
            set
            {
                SetValue("WidgetCategoryGUID", value);
            }
        }


        /// <summary>
        /// Time of last modification.
        /// </summary>
        [DatabaseField]
        public virtual DateTime WidgetCategoryLastModified
        {
            get
            {
                return GetDateTimeValue("WidgetCategoryLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WidgetCategoryLastModified", value);
            }
        }


        /// <summary>
        /// Gets or sets the ID of parent widget category.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetCategoryParentID
        {
            get
            {
                return GetIntegerValue("WidgetCategoryParentID", 0);
            }
            set
            {
                SetValue("WidgetCategoryParentID", value);
            }
        }


        /// <summary>
        /// Gets or set the level of widget category.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetCategoryLevel
        {
            get
            {
                return GetIntegerValue("WidgetCategoryLevel", 0);
            }
            set
            {
                SetValue("WidgetCategoryLevel", value);
            }
        }


        /// <summary>
        /// Gets or sets the child count of widget category.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetCategoryChildCount
        {
            get
            {
                return GetIntegerValue("WidgetCategoryChildCount", 0);
            }
            set
            {
                SetValue("WidgetCategoryChildCount", value);
            }
        }


        /// <summary>
        /// Gets or sets the child count of widgets under widget category.
        /// </summary>
        [DatabaseField]
        public virtual int WidgetCategoryWidgetChildCount
        {
            get
            {
                return GetIntegerValue("WidgetCategoryWidgetChildCount", 0);
            }
            set
            {
                SetValue("WidgetCategoryWidgetChildCount", value);
            }
        }


        /// <summary>
        /// Gets or sets the widget category path.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetCategoryPath
        {
            get
            {
                return GetStringValue("WidgetCategoryPath", String.Empty);
            }
            set
            {
                SetValue("WidgetCategoryPath", value);
            }
        }


        /// <summary>
        /// Gets or sets the widget category image path.
        /// </summary>
        [DatabaseField]
        public virtual string WidgetCategoryImagePath
        {
            get
            {
                return GetStringValue("WidgetCategoryImagePath", String.Empty);
            }
            set
            {
                SetValue("WidgetCategoryImagePath", value);
            }
        }


        /// <summary>
        /// If true, the code name is validated upon saving.
        /// </summary>
        protected override bool ValidateCodeName
        {
            get
            {
                return (base.ValidateCodeName && (WidgetCategoryParentID > 0));
            }
            set
            {
                base.ValidateCodeName = value;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WidgetCategoryInfoProvider.DeleteWidgetCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WidgetCategoryInfoProvider.SetWidgetCategoryInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty WidgetCategoryInfo object.
        /// </summary>
        public WidgetCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new WidgetCategoryInfo object from the given DataRow.
        /// </summary>
        public WidgetCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
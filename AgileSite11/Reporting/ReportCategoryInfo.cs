using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using CMS.Reporting;

[assembly: RegisterObjectType(typeof(ReportCategoryInfo), ReportCategoryInfo.OBJECT_TYPE)]

namespace CMS.Reporting
{
    /// <summary>
    /// ReportCategoryInfo data container class.
    /// </summary>
    public class ReportCategoryInfo : AbstractInfo<ReportCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.REPORTCATEGORY;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(ReportCategoryInfoProvider), OBJECT_TYPE, "Reporting.ReportCategory", "CategoryID", "CategoryLastModified", "CategoryGUID", "CategoryCodeName", "CategoryDisplayName", null, null, null, null)
        {
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            IsCategory = true,
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("CategoryParentID", OBJECT_TYPE, ObjectDependencyEnum.Required) },
            ModuleName = "cms.reporting",
            AllowRestore = false,
            DefaultOrderBy = "CategoryLevel ASC, CategoryDisplayName",
            ObjectNamePathColumn = "CategoryPath",
            ObjectLevelColumn = "CategoryLevel",
            ImportExportSettings =
            {
                OrderBy = "CategoryLevel ASC",
            },
            DefaultData = new DefaultDataSettings
            {
                OrderBy = "CategoryLevel, CategoryParentID",
                ChildDependencies = new List<DefaultDataChildDependency>
                {
                    new DefaultDataChildDependency("CategoryID", "CategoryChildCount", OBJECT_TYPE, "CategoryParentID"),
                    new DefaultDataChildDependency("CategoryID", "CategoryReportChildCount", ReportInfo.OBJECT_TYPE, "ReportCategoryID")
                }
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "CategoryChildCount",
                    "CategoryReportChildCount"
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// CategoryDisplayName.
        /// </summary>
        public virtual string CategoryDisplayName
        {
            get
            {
                return GetStringValue("CategoryDisplayName", "");
            }
            set
            {
                SetValue("CategoryDisplayName", value);
            }
        }


        /// <summary>
        /// CategoryID.
        /// </summary>
        public virtual int CategoryID
        {
            get
            {
                return GetIntegerValue("CategoryID", 0);
            }
            set
            {
                SetValue("CategoryID", value);
            }
        }


        /// <summary>
        /// CategoryCodeName.
        /// </summary>
        public virtual string CategoryCodeName
        {
            get
            {
                return GetStringValue("CategoryCodeName", "");
            }
            set
            {
                SetValue("CategoryCodeName", value);
            }
        }


        /// <summary>
        /// Category GUID.
        /// </summary>
        public virtual Guid CategoryGUID
        {
            get
            {
                return GetGuidValue("CategoryGUID", Guid.Empty);
            }
            set
            {
                SetValue("CategoryGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        public virtual DateTime CategoryLastModified
        {
            get
            {
                return GetDateTimeValue("CategoryLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("CategoryLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Image for category.
        /// </summary>
        public string CategoryImagePath
        {
            get
            {
                return GetStringValue("CategoryImagePath", "");
            }
            set
            {
                SetValue("CategoryImagePath", value);
            }
        }


        /// <summary>
        /// Hierarchical category path.
        /// </summary>
        public string CategoryPath
        {
            get
            {
                return GetStringValue("CategoryPath", "");
            }
            set
            {
                SetValue("CategoryPath", value);
            }
        }


        /// <summary>
        /// Hierarchical level.
        /// </summary>
        public int CategoryLevel
        {
            get
            {
                return GetIntegerValue("CategoryLevel", 0);
            }
            set
            {
                SetValue("CategoryLevel", value);
            }
        }


        /// <summary>
        /// Category's child count.
        /// </summary>
        public int CategoryChildCount
        {
            get
            {
                return GetIntegerValue("CategoryChildCount", 0);
            }
            set
            {
                SetValue("CategoryChildCount", value);
            }
        }


        /// <summary>
        /// Category's report's child count.
        /// </summary>
        public int CategoryReportChildCount
        {
            get
            {
                return GetIntegerValue("CategoryReportChildCount", 0);
            }
            set
            {
                SetValue("CategoryReportChildCount", value);
            }
        }


        /// <summary>
        /// Hierarchical category parent id.
        /// </summary>
        public int CategoryParentID
        {
            get
            {
                return GetIntegerValue("CategoryParentID", 0);
            }
            set
            {
                SetValue("CategoryParentID", value, 0);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            ReportCategoryInfoProvider.DeleteReportCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            ReportCategoryInfoProvider.SetReportCategoryInfo(this);
        }


        /// <summary>
        /// If true, the code name is validated upon saving.
        /// </summary>
        protected override bool ValidateCodeName
        {
            get
            {
                return (base.ValidateCodeName && (CategoryParentID > 0));
            }
            set
            {
                base.ValidateCodeName = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ReportCategoryInfo object.
        /// </summary>
        public ReportCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ReportCategoryInfo object from the given DataRow.
        /// </summary>
        public ReportCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
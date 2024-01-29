using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using CMS.Core;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WebPartCategoryInfo), WebPartCategoryInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// WebPartCategory info data container class.
    /// </summary>
    public class WebPartCategoryInfo : AbstractInfo<WebPartCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.WEBPARTCATEGORY;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebPartCategoryInfoProvider), OBJECT_TYPE, "CMS.WebPartCategory", "CategoryID", "CategoryLastModified", "CategoryGUID", "CategoryName", "CategoryDisplayName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("CategoryParentID", OBJECT_TYPE, ObjectDependencyEnum.Required) },
            ModuleName = ModuleName.DESIGN,
            ImportExportSettings =
            {
                LogExport = false,
                OrderBy = "CategoryLevel ASC",
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
                    "CategoryChildCount",
                    "CategoryWebPartChildCount"
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsCategory = true,
            AllowRestore = false,
            DefaultOrderBy = "CategoryLevel ASC, CategoryName",
            ObjectNamePathColumn = "CategoryPath",
            ObjectLevelColumn = "CategoryLevel",
            DefaultData = new DefaultDataSettings
            {
                OrderBy = "CategoryPath",
                ChildDependencies = new List<DefaultDataChildDependency>
                {
                    new DefaultDataChildDependency("CategoryID", "CategoryChildCount", OBJECT_TYPE, "CategoryParentID"),
                    new DefaultDataChildDependency("CategoryID", "CategoryWebPartChildCount", WebPartInfo.OBJECT_TYPE, "WebPartCategoryID")
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
                    "CategoryChildCount",
                    "CategoryWebPartChildCount"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// The category ID.
        /// </summary>
        [DatabaseField]
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
        /// The Category Display Name.
        /// </summary>
        [DatabaseField]
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
        /// The Category Parent ID.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryParentID
        {
            get
            {
                return GetIntegerValue("CategoryParentID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("CategoryParentID", null);
                }
                else
                {
                    SetValue("CategoryParentID", value);
                }
            }
        }


        /// <summary>
        /// The Category Name.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryName
        {
            get
            {
                return GetStringValue("CategoryName", "");
            }
            set
            {
                SetValue("CategoryName", value);
            }
        }


        /// <summary>
        /// Category GUID.
        /// </summary>
        [DatabaseField]
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
        [DatabaseField]
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
        /// Gets or sets category image path.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryImagePath
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
        /// Gets or sets category children count.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryChildCount
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
        /// Gets or sets category webpart children count.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryWebPartChildCount
        {
            get
            {
                return GetIntegerValue("CategoryWebPartChildCount", 0);
            }
            set
            {
                SetValue("CategoryWebPartChildCount", value);
            }
        }


        /// <summary>
        /// Gets or sets category path.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryPath
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
        /// Gets or sets category level.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryLevel
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

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            WebPartCategoryInfoProvider.DeleteCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            WebPartCategoryInfoProvider.SetWebPartCategoryInfo(this);
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
        /// Constructor, creates an empty WebPartCategoryInfo structure.
        /// </summary>
        public WebPartCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an empty LayoutInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public WebPartCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
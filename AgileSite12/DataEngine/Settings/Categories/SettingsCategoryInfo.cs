using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DataEngine.Internal;

[assembly: RegisterObjectType(typeof(SettingsCategoryInfo), SettingsCategoryInfo.OBJECT_TYPE)]

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a Settings key.
    /// </summary>
    public class SettingsCategoryInfo : AbstractInfo<SettingsCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.settingscategory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(SettingsCategoryInfoProvider), OBJECT_TYPE, "CMS.SettingsCategory", "CategoryID", null, null, "CategoryName", "CategoryDisplayName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>() { new ObjectDependency("CategoryParentID", OBJECT_TYPE, ObjectDependencyEnum.Required), new ObjectDependency("CategoryResourceID", PredefinedObjectType.RESOURCE, ObjectDependencyEnum.Required) },
            SynchronizationSettings = 
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>()
                {
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION)
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            IsCategory = true,
            AllowRestore = false,
            DefaultOrderBy = "CategoryLevel ASC, CategoryIsGroup DESC, CategoryOrder",
            ObjectIDPathColumn = "CategoryIDPath",
            ObjectLevelColumn = "CategoryLevel",
            OrderColumn = "CategoryOrder",
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.Incremental,
                OrderBy = "CategoryLevel ASC, CategoryIsGroup DESC"
            },
            ResourceIDColumn = "CategoryResourceID",
            DefaultData = new DefaultDataSettings
            {
                ChildDependencies = new List<DefaultDataChildDependency>
                {
                    new DefaultDataChildDependency("CategoryID", "CategoryChildCount", OBJECT_TYPE, "CategoryParentID")
                }
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "CategoryChildCount"
                }
            },
            ContinuousIntegrationSettings = 
            {
                Enabled = true,
                FilterDependencies =
                {
                     new ObjectReference("CategoryResourceID", PredefinedObjectType.RESOURCE)
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Category ID.
        /// </summary>
        public int CategoryID
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
        /// Category display name.
        /// </summary>
        public string CategoryDisplayName
        {
            get
            {
                return GetStringValue("CategoryDisplayName", null);
            }
            set
            {
                SetValue("CategoryDisplayName", value);
            }
        }


        /// <summary>
        /// Category name.
        /// </summary>
        public string CategoryName
        {
            get
            {
                return GetStringValue("CategoryName", null);
            }
            set
            {
                SetValue("CategoryName", value);
            }
        }


        /// <summary>
        /// Category order.
        /// </summary>
        public int CategoryOrder
        {
            get
            {
                return GetIntegerValue("CategoryOrder", 0);
            }
            set
            {
                SetValue("CategoryOrder", value);
            }
        }


        /// <summary>
        /// Category parent ID.
        /// </summary>
        public int CategoryParentID
        {
            get
            {
                return GetIntegerValue("CategoryParentID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("CategoryParentID", value);
                }
                else
                {
                    SetValue("CategoryParentID", null);
                }
            }
        }


        /// <summary>
        /// Category ID path within the category tree.
        /// </summary>
        public string CategoryIDPath
        {
            get
            {
                return GetStringValue("CategoryIDPath", String.Empty);
            }
            set
            {
                SetValue("CategoryIDPath", value);
            }
        }


        /// <summary>
        /// Category level in the category tree.
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
        /// Number of category child categories.
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
        /// Category icon path.
        /// </summary>
        public string CategoryIconPath
        {
            get
            {
                return GetStringValue("CategoryIconPath", String.Empty);
            }
            set
            {
                SetValue("CategoryIconPath", value);
            }
        }


        /// <summary>
        /// Indicates if category represents group. True -> It is a settings group, False -> It is a settings category.
        /// </summary>
        public bool CategoryIsGroup
        {
            get
            {
                return GetBooleanValue("CategoryIsGroup", false);
            }
            set
            {
                SetValue("CategoryIsGroup", value);
            }
        }


        /// <summary>
        /// Indicates if category is custom. True -> It is a custom category, False -> It is a system category.
        /// </summary>
        public bool CategoryIsCustom
        {
            get
            {
                return GetBooleanValue("CategoryIsCustom", false);
            }
            set
            {
                SetValue("CategoryIsCustom", value);
            }
        }


        /// <summary>
        /// Category resource identifier.
        /// </summary>
        public int CategoryResourceID
        {
            get
            {
                return GetIntegerValue("CategoryResourceID", 0);
            }
            set
            {
                SetValue("CategoryResourceID", value, value > 0);
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            SettingsCategoryInfoProvider.DeleteSettingsCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            SettingsCategoryInfoProvider.SetSettingsCategoryInfo(this);
        }


        /// <summary>
        /// Creates where condition according to Parent, Group and Site settings.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            return base.GetSiblingsWhereCondition().WhereEquals("CategoryIsGroup", CategoryIsGroup).Where("ISNULL(CategoryParentID, 0) = " + CategoryParentID);
        }


        /// <summary>
        /// Builds the path from the given column.
        /// </summary>
        /// <param name="parentColumName">Column of the parent ID</param>
        /// <param name="pathColumnName">Column name to build the path from</param>
        /// <param name="levelColumnName">Column name of the level</param>
        /// <param name="level">Level of the object within the tree hierarchy</param>
        /// <param name="pathPartColumn">Name of the column which creates the path (IDColumn for IDPath, CodeNameColumn for name path)</param>
        protected override string BuildObjectPath(string parentColumName, string pathColumnName, string levelColumnName, string pathPartColumn, out int level)
        {
            BaseInfo parent = Generalized.GetObject(GetIntegerValue(parentColumName, 0));
            if (parent != null)
            {
                level = parent.GetIntegerValue(levelColumnName, 0) + 1;
                return parent.GetStringValue(pathColumnName, "").TrimEnd('/') + "/" + GetCurrentObjectPathPart(pathPartColumn);
            }
            else
            {
                level = 0;
                return "/" + GetCurrentObjectPathPart(pathPartColumn);
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty SettingsKeyInfo structure.
        /// </summary>
        public SettingsCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates the DataClassInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public SettingsCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Membership;
using CMS.Taxonomy;

[assembly: RegisterObjectType(typeof(CategoryInfo), CategoryInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(CategoryInfo), CategoryInfo.OBJECT_TYPE_USERCATEGORY)]

namespace CMS.Taxonomy
{
    /// <summary>
    /// CategoryInfo data container class.
    /// </summary>
    public class CategoryInfo : AbstractInfo<CategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.CATEGORY;

        /// <summary>
        /// Object type for usercategory
        /// </summary>
        public const string OBJECT_TYPE_USERCATEGORY = "cms.usercategory";


        /// <summary>
        /// Type information for global categories.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(CategoryInfoProvider), OBJECT_TYPE, "CMS.Category", "CategoryID", "CategoryLastModified", "CategoryGUID", "CategoryName", "CategoryDisplayName", null, "CategorySiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                    new ObjectDependency("CategoryParentID", OBJECT_TYPE, ObjectDependencyEnum.Required)
            },

            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                }
            },

            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,

            IsCategory = true,
            AllowRestore = false,

            DefaultOrderBy = "CategoryLevel ASC, CategoryOrder ASC",
            ModuleName = ModuleName.CATEGORIES,
            SupportsGlobalObjects = true,
            ImportExportSettings =
            {
                AlwaysCheckExisting = true,
                AllowSingleExport = false,
                LogExport = true,
                IsExportable = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, CONFIGURATION),
                    new ObjectTreeLocation(GLOBAL, CONFIGURATION),
                },
                OrderBy = "CategoryLevel ASC, CategoryOrder ASC",
            },
            TypeCondition = new TypeCondition().WhereIsNull("CategoryUserID"),
            NameGloballyUnique = true,
            ObjectIDPathColumn = "CategoryIDPath",
            ObjectLevelColumn = "CategoryLevel",
            OrderColumn = "CategoryOrder",
            EnabledColumn = "CategoryEnabled",
            ContinuousIntegrationSettings =
            {
                Enabled = true,
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "CategoryCount",
                    "CategoryNamePath"
                }
            }
        };

        /// <summary>
        /// Type information for user categories.
        /// </summary>
        public static ObjectTypeInfo TYPEINFOUSERCATEGORY = new ObjectTypeInfo(typeof(CategoryInfoProvider), OBJECT_TYPE_USERCATEGORY, "CMS.Category", "CategoryID", "CategoryLastModified", "CategoryGUID", "CategoryName", "CategoryDisplayName", null, null, "CategoryUserID", PredefinedObjectType.USER)
        {
            OriginalTypeInfo = TYPEINFO,
            DependsOn = new List<ObjectDependency>
                {
                    new ObjectDependency("CategoryParentID", OBJECT_TYPE, ObjectDependencyEnum.Required)
                },

            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.TouchParent,
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            IsCategory = true,
            AllowRestore = false,
            DefaultOrderBy = "CategoryLevel ASC, CategoryOrder ASC",
            ModuleName = ModuleName.CATEGORIES,
            TypeCondition = new TypeCondition().WhereIsNotNull("CategoryUserID"),
            ObjectIDPathColumn = "CategoryIDPath",
            ObjectLevelColumn = "CategoryLevel",
            OrderColumn = "CategoryOrder",
            EnabledColumn = "CategoryEnabled",
            ImportExportSettings =
            {
                LogExport = true,
                AllowSingleExport = false,
                OrderBy = "CategoryLevel ASC, CategoryOrder ASC",
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "CategoryCount",
                    "CategoryNamePath"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Category last modified.
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
                SetValue("CategoryLastModified", value);
            }
        }


        /// <summary>
        /// Category code name.
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
        /// Category enabled.
        /// </summary>
        [DatabaseField]
        public virtual bool CategoryEnabled
        {
            get
            {
                return GetBooleanValue("CategoryEnabled", false);
            }
            set
            {
                SetValue("CategoryEnabled", value);
            }
        }


        /// <summary>
        /// Category ID.
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
        /// Category display name.
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
                SetValue("CategoryGUID", value);
            }
        }


        /// <summary>
        /// Category count.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryCount
        {
            get
            {
                return GetIntegerValue("CategoryCount", 0);
            }
            set
            {
                SetValue("CategoryCount", value);
            }
        }


        /// <summary>
        /// Category description.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryDescription
        {
            get
            {
                return GetStringValue("CategoryDescription", "");
            }
            set
            {
                SetValue("CategoryDescription", value);
            }
        }


        /// <summary>
        /// Category User ID.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryUserID
        {
            get
            {
                return GetIntegerValue("CategoryUserID", 0);
            }
            set
            {
                SetValue("CategoryUserID", value, value > 0);
            }
        }


        /// <summary>
        /// ID of the site on which category can be used. 0 for global categories.
        /// </summary>
        [DatabaseField]
        public virtual int CategorySiteID
        {
            get
            {
                return GetIntegerValue("CategorySiteID", 0);
            }
            set
            {
                SetValue("CategorySiteID", value, value > 0);
            }
        }


        /// <summary>
        /// ID of the parent category. 0 for top-level categories.
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
                SetValue("CategoryParentID", value, value > 0);
            }
        }


        /// <summary>
        /// Path consisting of IDs of categories preceding this category.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryIDPath
        {
            get
            {
                return GetStringValue("CategoryIDPath", "");
            }
            set
            {
                SetValue("CategoryIDPath", value);
            }
        }


        /// <summary>
        /// Path consisting of display names of categories preceding this category.
        /// </summary>
        [DatabaseField]
        public virtual string CategoryNamePath
        {
            get
            {
                return GetStringValue("CategoryNamePath", "");
            }
            set
            {
                SetValue("CategoryNamePath", value);
            }
        }


        /// <summary>
        /// Level of nesting for this category. 0 for top-level categories.
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


        /// <summary>
        /// Order of the category.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryOrder
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
        /// Indicates whether category is global.
        /// </summary>
        public virtual bool CategoryIsGlobal
        {
            get
            {
                return CategorySiteID == 0;
            }
        }


        /// <summary>
        /// Indicates whether category is personal.
        /// </summary>
        public virtual bool CategoryIsPersonal
        {
            get
            {
                return CategoryUserID > 0;
            }
        }

        #endregion


        #region "GeneralizedInfo properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (CategoryUserID > 0)
                {
                    return TYPEINFOUSERCATEGORY;
                }

                return TYPEINFO;
            }
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            CategoryInfoProvider.DeleteCategoryInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            CategoryInfoProvider.SetCategoryInfo(this);
        }


        /// <summary>
        /// Creates where condition according to Parent, Group and Site settings.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            return base.GetSiblingsWhereCondition().Where("ISNULL(CategoryParentID, 0) = " + CategoryParentID);
        }


        /// <summary>
        /// Builds the path from the given column.
        /// </summary>
        /// <param name="parentColumName">Column of the parent ID</param>
        /// <param name="pathColumnName">Column name to build the path from</param>
        /// <param name="levelColumnName">Column name of the level</param>
        /// <param name="pathPartColumn">Name of the column which creates the path (IDColumn for IDPath, CodeNameColumn for name path)</param>
        /// <param name="level">Level of the object within the tree hierarchy</param>
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


        /// <summary>
        /// Returns the name of the column which is used to build the NamePath
        /// </summary>
        protected override string GetNamePathPartColumn()
        {
            return "CategoryDisplayName";
        }


        /// <summary>
        /// Returns automatic code name. If user is specified, code name has user name as prefix.
        /// </summary>
        protected override string GetAutomaticCodeName()
        {
            if (CategoryUserID > 0)
            {
                // Format personal category code name with user name prefix
                var name = String.Format("{0}_{1}", UserInfoProvider.GetUserNameById(CategoryUserID), ResHelper.LocalizeString(ObjectDisplayName));
                return ValidationHelper.GetCodeName(name, useUnicode: false);
            }
            else
            {
                return base.GetAutomaticCodeName();
            }
        }


        /// <summary>
        /// Checks if the category has unique code name. Returns true if the object has unique code name.
        /// </summary>
        public override bool CheckUniqueCodeName()
        {
            // Create condition for existing category 
            WhereCondition condition = new WhereCondition().WhereEquals("CategoryName", CategoryName);
            if (CategorySiteID > 0)
            {
                condition.WhereEqualsOrNull("CategorySiteID", CategorySiteID);
            }

            // Get existing category
            DataSet ds = CategoryInfoProvider.GetCategories()
                                                .TopN(1)
                                                .Column("CategoryID")
                                                .Where(condition)
                                                .OrderBy("CategoryID");

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return true;
            }

            int id = ValidationHelper.GetInteger(ds.Tables[0].Rows[0]["CategoryID"], 0);

            // If the existing category is updated the code name already exists
            return (id > 0) && (CategoryID == id);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty CategoryInfo object.
        /// </summary>
        public CategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new CategoryInfo object from the given DataRow.
        /// </summary>
        public CategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Permissions"

        /// <summary>
        /// Checks the permissions of the object.
        /// </summary>
        /// <param name="permission">Permission type</param>
        /// <param name="siteName">Name of the site</param>
        /// <param name="userInfo">UserInfo object</param>
        /// <param name="exceptionOnFailure">If true, PermissionCheckException is thrown whenever a permission check fails</param>
        protected override bool CheckPermissionsInternal(PermissionsEnum permission, string siteName, IUserInfo userInfo, bool exceptionOnFailure)
        {
            switch (permission)
            {
                case PermissionsEnum.Create:
                case PermissionsEnum.Delete:
                case PermissionsEnum.Modify:
                case PermissionsEnum.Destroy:
                    if (CategoryIsGlobal && !CategoryIsPersonal)
                    {
                        return userInfo.IsAuthorizedPerResource("cms.categories", "GlobalModify", siteName, exceptionOnFailure);
                    }
                    else
                    {
                        return userInfo.IsAuthorizedPerResource("cms.categories", "Modify", siteName, exceptionOnFailure);
                    }

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
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
            bool subcategories = true;
            int parentCategory = CategoryParentID;

            Hashtable p = settings.CustomParameters;
            var ti = TypeInfo;

            if (p != null)
            {
                subcategories = ValidationHelper.GetBoolean(p[ti.ObjectType + ".subcategories"], true);
                parentCategory = ValidationHelper.GetInteger(p[ti.ObjectType + ".parentcategory"], parentCategory);
            }

            CategoryParentID = parentCategory;

            if (ItemChanged("CategoryUserID") && (CategoryUserID > 0))
            {
                // Update user's category name according to the new user
                CategoryName = GetAutomaticCodeName();
            }

            Insert();

            // Clone subcategories
            if (subcategories)
            {
                DataSet ds = CategoryInfoProvider.GetCategories("CategoryParentID = " + originalObject.Generalized.ObjectID + " AND CategoryLevel = " + (originalObject.GetIntegerValue("CategoryLevel", 0) + 1), null);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    int origSiteId = settings.CloneToSiteID;
                    string originalName = settings.CodeName;
                    string originalDisplayName = settings.DisplayName;
                    settings.DisplayName = null;
                    settings.CodeName = null;
                    settings.CustomParameters.Remove(ti.ObjectType + ".parentcategory");

                    foreach (DataRow dr in ds.Tables[0].Rows)
                    {
                        CategoryInfo child = new CategoryInfo(dr);
                        child.CategoryParentID = CategoryID;

                        settings.CloneToSiteID = child.CategorySiteID;

                        child.InsertAsClone(settings, result);
                    }

                    settings.CloneToSiteID = origSiteId;
                    settings.DisplayName = originalName;
                    settings.CodeName = originalDisplayName;
                    settings.CustomParameters[ti.ObjectType + ".parentcategory"] = parentCategory;
                }
            }
        }

        #endregion
    }
}
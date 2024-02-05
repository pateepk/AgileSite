using System;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using CMS.Core;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(PageTemplateCategoryInfo), PageTemplateCategoryInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page template category info data container class.
    /// </summary>
    public class PageTemplateCategoryInfo : AbstractInfo<PageTemplateCategoryInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.pagetemplatecategory";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PageTemplateCategoryInfoProvider), OBJECT_TYPE, "CMS.PageTemplateCategory", "CategoryID", "CategoryLastModified", "CategoryGUID", "CategoryName", "CategoryDisplayName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("CategoryParentID", OBJECT_TYPE, ObjectDependencyEnum.Required) },
            Extends = new List<ExtraColumn>
            {
                new ExtraColumn(PredefinedObjectType.DOCUMENTTYPE, "ClassPageTemplateCategoryID"), 
            },
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
                    "CategoryTemplateChildCount"
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
                    new DefaultDataChildDependency("CategoryID", "CategoryTemplateChildCount", PageTemplateInfo.OBJECT_TYPE, "PageTemplateCategoryID")
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
                    "CategoryTemplateChildCount"
                }
            }
        };

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the category ID.
        /// </summary>
        [DatabaseField("CategoryID")]
        public virtual int CategoryId
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
        /// Gets or sets the category display name.
        /// </summary>
        [DatabaseField("CategoryDisplayName")]
        public virtual string DisplayName
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
        /// Gets or sets the category parent id.
        /// </summary>
        [DatabaseField("CategoryParentID")]
        public virtual int ParentId
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
        /// Gets or sets the category code name.
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
        /// Page template category GUID.
        /// </summary>
        [DatabaseField("CategoryGUID")]
        public virtual Guid PageTemplateCategoryGUID
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
        [DatabaseField("CategoryLastModified")]
        public virtual DateTime PageTemplateCategoryLastModified
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
        /// Gets or sets count of child page template categories.
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
        /// Gets or sets count of child page templates.
        /// </summary>
        [DatabaseField]
        public virtual int CategoryTemplateChildCount
        {
            get
            {
                return GetIntegerValue("CategoryTemplateChildCount", 0);
            }
            set
            {
                SetValue("CategoryTemplateChildCount", value);
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
            PageTemplateCategoryInfoProvider.DeletePageTemplateCategory(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            PageTemplateCategoryInfoProvider.SetPageTemplateCategoryInfo(this);
        }


        /// <summary>
        /// If true, the code name is validated upon saving.
        /// </summary>
        protected override bool ValidateCodeName
        {
            get
            {
                return (base.ValidateCodeName && (ParentId > 0));
            }
            set
            {
                base.ValidateCodeName = value;
            }
        }

        #endregion


        #region "Contructors"

        /// <summary>
        /// Constructor - Creates an empty PageTemplateCategoryInfo object.
        /// </summary>
        public PageTemplateCategoryInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new PageTemplateCategoryInfo object from the given DataRow.
        /// </summary>
        public PageTemplateCategoryInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes object dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Get page templates from current category
            DataSet templatesDescendants = PageTemplateInfoProvider.GetTemplatesInCategory(CategoryId);
            if (!DataHelper.DataSourceIsEmpty(templatesDescendants))
            {
                // Delete all descendants
                foreach (DataRow descendant in templatesDescendants.Tables[0].Rows)
                {
                    PageTemplateInfoProvider.DeletePageTemplate(DataHelper.GetIntValue(descendant, "PageTemplateID"));
                }
            }

            // Get all descendant categories
            DataSet categoryDescendants = PageTemplateCategoryInfoProvider.GetDescendantCategories(CategoryId);
            if (!DataHelper.DataSourceIsEmpty(categoryDescendants))
            {
                // Delete all descendants
                foreach (DataRow descendant in categoryDescendants.Tables[0].Rows)
                {
                    PageTemplateCategoryInfoProvider.DeletePageTemplateCategory(DataHelper.GetIntValue(descendant, "CategoryID"));
                }
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Gets the where condition to filter out the default installation data
        /// </summary>
        /// <param name="recursive">Indicates whether where condition should contain further dependency conditions.</param>
        /// <param name="globalOnly">Indicates whether only objects with null in their site ID column should be included.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected override string GetDefaultDataWhereCondition(bool recursive = true, bool globalOnly = true, IEnumerable<string> excludedNames = null)
        {
            string where = "(CategoryParentID IS NULL) OR (CategoryName = 'AdHocUI')";

            if (recursive)
            {
                BaseInfo pageTemplate = ModuleManager.GetReadOnlyObject(PageTemplateInfo.OBJECT_TYPE);
                where = AddDependencyDefaultDataWhereCondition(where, pageTemplate, "CategoryID", true, "OR", "DISTINCT PageTemplateCategoryID", excludedNames);
            }

            return where;
        }

        #endregion
    }
}
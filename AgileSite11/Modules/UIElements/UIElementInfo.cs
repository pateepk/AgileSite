using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS;
using CMS.DataEngine;
using CMS.DataEngine.Internal;
using CMS.Helpers;
using CMS.Base;
using CMS.Modules;

[assembly: RegisterObjectType(typeof(UIElementInfo), UIElementInfo.OBJECT_TYPE)]

namespace CMS.Modules
{
    /// <summary>
    /// Permission info data container.
    /// </summary>
    public class UIElementInfo : AbstractInfo<UIElementInfo>
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.uielement";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(UIElementInfoProvider), OBJECT_TYPE, "CMS.UIElement", "ElementID", "ElementLastModified", "ElementGUID", "ElementName", "ElementDisplayName", null, null, "ElementResourceID", ResourceInfo.OBJECT_TYPE)
        {
            DeleteObjectWithAPI = true,
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ElementParentID", OBJECT_TYPE, ObjectDependencyEnum.Required) },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.Complete
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            DefaultOrderBy = "ElementLevel ASC, ElementOrder",
            ObjectIDPathColumn = "ElementIDPath",
            ObjectLevelColumn = "ElementLevel",
            OrderColumn = "ElementOrder",
            SupportsInvalidation = true,
            ImportExportSettings =
            {
                IncludeToExportParentDataSet = IncludeToParentEnum.Incremental,
                OrderBy = "ElementLevel ASC, ElementGUID",
            },
            DefaultData = new DefaultDataSettings
            {
                OrderBy = "ElementLevel ASC, ElementParentID ASC",
                ChildDependencies = new List<DefaultDataChildDependency>
                {
                    new DefaultDataChildDependency("ElementID", "ElementChildCount", OBJECT_TYPE, "ElementParentID")
                }
            },
            SerializationSettings =
            {
                ExcludedFieldNames =
                {
                    "ElementChildCount"
                },
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("ElementProperties")
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Constants"

        /// <summary>
        /// Code name of CMS Desk in UIElements tree
        /// </summary>
        private const string UIELEMENT_ADMINISTRATION_CODENAME = "administration";

        #endregion


        #region "Variables"

        private String mElementFullName;

        private IInfoObjectCollection<UIElementInfo> mChildElements;

        private bool mApplicationSeeked;
        private UIElementInfo mApplication;

        #endregion


        #region "Database field properties"

        /// <summary>
        /// Description of the UI Element (used for example in the UI Guide).
        /// </summary>
        [DatabaseField]
        public virtual string ElementDescription
        {
            get
            {
                return GetStringValue("ElementDescription", "");
            }
            set
            {
                SetValue("ElementDescription", value);
            }
        }


        /// <summary>
        /// Defines if UI Element can be accessed only by global administrator.
        /// </summary>
        [DatabaseField]
        public virtual bool ElementRequiresGlobalAdminPriviligeLevel
        {
            get
            {
                return GetBooleanValue("ElementRequiresGlobalAdminPriviligeLevel", false);
            }
            set
            {
                SetValue("ElementRequiresGlobalAdminPriviligeLevel", value);
            }
        }


        /// <summary>
        /// UI element's properties
        /// </summary>
        [DatabaseField]
        public virtual string ElementProperties
        {
            get
            {
                return GetStringValue("ElementProperties", "");
            }
            set
            {
                SetValue("ElementProperties", value);
            }
        }


        /// <summary>
        /// UI element's webparts XML definition
        /// </summary>
        [DatabaseField]
        public virtual string ElementWebparts
        {
            get
            {
                return GetStringValue("ElementWebparts", "");
            }
            set
            {
                SetValue("ElementWebparts", value);
            }
        }


        /// <summary>
        /// Gets or sets a macro condition that must be fulfilled for the UI element to be visible in the system's user interface.
        /// </summary>
        [DatabaseField]
        public virtual string ElementVisibilityCondition
        {
            get
            {
                return GetStringValue("ElementVisibilityCondition", "");
            }
            set
            {
                SetValue("ElementVisibilityCondition", value);
            }
        }


        /// <summary>
        /// Element's feature.
        /// </summary>
        [DatabaseField]
        public String ElementFeature
        {
            get
            {
                return GetStringValue("ElementFeature", "");
            }
            set
            {
                SetValue("ElementFeature", value, String.Empty);
            }
        }


        /// <summary>
        /// Size of the UI Element when it is element of UIToolbar.
        /// </summary>
        [DatabaseField]
        public virtual UIElementSizeEnum ElementSize
        {
            get
            {
                int size = ValidationHelper.GetInteger(GetValue("ElementSize"), 0);
                switch (size)
                {
                    case 1:
                        return UIElementSizeEnum.Regular;

                    default:
                        return UIElementSizeEnum.Large;
                }
            }
            set
            {
                switch (value)
                {
                    case UIElementSizeEnum.Large:
                        SetValue("ElementSize", 0);
                        break;

                    case UIElementSizeEnum.Regular:
                        SetValue("ElementSize", 1);
                        break;
                }
            }
        }


        /// <summary>
        /// Element's page template ID
        /// </summary>
        [DatabaseField]
        public virtual int ElementPageTemplateID
        {
            get
            {
                return GetIntegerValue("ElementPageTemplateID", 0);
            }
            set
            {
                SetValue("ElementPageTemplateID", value, 0);
            }
        }


        /// <summary>
        /// Element's type (url, control, template, javascript,..)
        /// </summary>
        [DatabaseField]
        public virtual UIElementTypeEnum ElementType
        {
            get
            {
                return Convert.ToString(GetValue("ElementType")).ToEnum<UIElementTypeEnum>();
            }
            set
            {
                SetValue("ElementType", value.ToStringRepresentation());
            }
        }


        /// <summary>
        /// Indicates if UI element is custom or system.
        /// </summary>
        [DatabaseField]
        public virtual bool ElementIsCustom
        {
            get
            {
                return GetBooleanValue("ElementIsCustom", false);
            }
            set
            {
                SetValue("ElementIsCustom", value);
            }
        }


        /// <summary>
        /// UI Element code name (unique within the parent resource).
        /// </summary>
        [DatabaseField]
        public virtual string ElementName
        {
            get
            {
                return GetStringValue("ElementName", "");
            }
            set
            {
                SetValue("ElementName", value);
                mElementFullName = null;
            }
        }


        /// <summary>
        /// URL of the page which is displayed when UI element (menu item / tab) is clicked. 
        /// If element does not represent menu item / tab it is empty.
        /// </summary>
        [DatabaseField]
        public virtual string ElementTargetURL
        {
            get
            {
                return GetStringValue("ElementTargetURL", "");
            }
            set
            {
                SetValue("ElementTargetURL", value);
            }
        }


        /// <summary>
        /// Element full name.
        /// </summary>
        [DatabaseField]
        public string ElementFullName
        {
            get
            {
                if (mElementFullName == null)
                {
                    //  Get resource info
                    var ri = ResourceInfoProvider.GetResourceInfo(ElementResourceID);
                    if (ri == null)
                    {
                        return "";
                    }

                    // Create full name
                    mElementFullName = ObjectHelper.BuildFullName(ri.ResourceName, ElementName, "|");
                }

                return mElementFullName;
            }
            set
            {
                mElementFullName = value;
            }
        }


        /// <summary>
        /// ID of the UIElement.
        /// </summary>
        [DatabaseField]
        public virtual int ElementID
        {
            get
            {
                return GetIntegerValue("ElementID", 0);
            }
            set
            {
                SetValue("ElementID", value);
            }
        }


        /// <summary>
        /// Unique identifier of the UI element.
        /// </summary>
        [DatabaseField]
        public virtual Guid ElementGUID
        {
            get
            {
                return GetGuidValue("ElementGUID", Guid.Empty);
            }
            set
            {
                SetValue("ElementGUID", value);
            }
        }


        /// <summary>
        /// Path representing the tree path of the UI element.
        /// </summary>
        [DatabaseField]
        public virtual string ElementIDPath
        {
            get
            {
                return GetStringValue("ElementIDPath", "");
            }
            set
            {
                SetValue("ElementIDPath", value);
            }
        }


        /// <summary>
        /// UI element level within the tree of all UI elements of the resource.
        /// </summary>
        [DatabaseField]
        public virtual int ElementLevel
        {
            get
            {
                return GetIntegerValue("ElementLevel", 0);
            }
            set
            {
                SetValue("ElementLevel", value);
            }
        }


        /// <summary>
        /// Data and time the element was last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime ElementLastModified
        {
            get
            {
                return GetDateTimeValue("ElementLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("ElementLastModified", value);
            }
        }


        /// <summary>
        /// UI Element display name.
        /// </summary>
        [DatabaseField]
        public virtual string ElementDisplayName
        {
            get
            {
                return GetStringValue("ElementDisplayName", "");
            }
            set
            {
                SetValue("ElementDisplayName", value);
            }
        }


        /// <summary>
        /// ID of the parent UI element.
        /// </summary>
        [DatabaseField]
        public virtual int ElementParentID
        {
            get
            {
                return GetIntegerValue("ElementParentID", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("ElementParentID", value);
                }
                else
                {
                    SetValue("ElementParentID", null);
                }
            }
        }


        /// <summary>
        /// Number of child UI elements.
        /// </summary>
        [DatabaseField]
        public virtual int ElementChildCount
        {
            get
            {
                return GetIntegerValue("ElementChildCount", 0);
            }
            set
            {
                SetValue("ElementChildCount", value);
            }
        }


        /// <summary>
        /// ID of the resource the UI element belongs to.
        /// </summary>
        [DatabaseField]
        public virtual int ElementResourceID
        {
            get
            {
                return GetIntegerValue("ElementResourceID", 0);
            }
            set
            {
                SetValue("ElementResourceID", value);
                mElementFullName = null;
            }
        }


        /// <summary>
        /// Order within the parent UI element.
        /// </summary>
        [DatabaseField]
        public virtual int ElementOrder
        {
            get
            {
                return GetIntegerValue("ElementOrder", 0);
            }
            set
            {
                if (value > 0)
                {
                    SetValue("ElementOrder", value);
                }
                else
                {
                    SetValue("ElementOrder", null);
                }
            }
        }


        /// <summary>
        /// Relative path to the UI element icon.
        /// </summary>
        [DatabaseField]
        public virtual string ElementIconPath
        {
            get
            {
                return GetStringValue("ElementIconPath", "");
            }
            set
            {
                SetValue("ElementIconPath", value);
            }
        }


        /// <summary>
        /// Element's font icon class name
        /// </summary>
        [DatabaseField]
        public virtual string ElementIconClass
        {
            get
            {
                return GetStringValue("ElementIconClass", "");
            }
            set
            {
                SetValue("ElementIconClass", value);
            }
        }


        /// <summary>
        /// UI element caption. If set, UI element represents menu item / tab.
        /// </summary>
        [DatabaseField]
        public virtual string ElementCaption
        {
            get
            {
                return GetStringValue("ElementCaption", "");
            }
            set
            {
                SetValue("ElementCaption", value);
            }
        }


        /// <summary>
        /// Gets or sets a macro condition that must be fulfilled to view the UI element's content. 
        /// If the condition is false, the element displays an access denied error instead of the content. 
        /// </summary>
        [DatabaseField]
        public virtual string ElementAccessCondition
        {
            get
            {
                return GetStringValue("ElementAccessCondition", "");
            }
            set
            {
                SetValue("ElementAccessCondition", value);
            }
        }


        /// <summary>
        /// Version from which is UI element present in CMS.
        /// </summary>
        [DatabaseField]
        public virtual string ElementFromVersion
        {
            get
            {
                return GetStringValue("ElementFromVersion", "");
            }
            set
            {
                SetValue("ElementFromVersion", value);
            }
        }


        /// <summary>
        /// Indicates whether the element is available without an operating site.
        /// </summary>
        [DatabaseField]
        public bool ElementIsGlobalApplication
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ElementIsGlobalApplication"), false);
            }
            set
            {
                SetValue("ElementIsGlobalApplication", value);
            }
        }


        /// <summary>
        /// Indicates if the system should check the "read" permission for the module to which the current UI element is assigned.
        /// When the "read" permission doesn't exist for the current module, this setting has no effect.
        /// </summary>
        [DatabaseField]
        public bool ElementCheckModuleReadPermission
        {
            get
            {
                return ValidationHelper.GetBoolean(GetValue("ElementCheckModuleReadPermission"), true);
            }
            set
            {
                SetValue("ElementCheckModuleReadPermission", value);
            }
        }

        #endregion


        #region "Properties"

        /// <summary>
        /// Returns true if the UI element represents a new object element
        /// </summary>
        [DatabaseMapping("(ElementName LIKE 'New%')")]
        public bool RepresentsNew
        {
            get
            {
                return ElementName.StartsWithCSafe("new", true);
            }
        }


        /// <summary>
        /// Returns true if the UI element represents the edit object element
        /// </summary>
        [DatabaseMapping("(ElementName LIKE 'Edit%')")]
        public bool RepresentsEdit
        {
            get
            {
                return ElementName.StartsWithCSafe("edit", true);
            }
        }


        /// <summary>
        /// Direct child elements of object
        /// </summary>
        public IInfoObjectCollection<UIElementInfo> ChildElements
        {
            get
            {
                if (mChildElements == null)
                {
                    // Create collection of child elements for current element
                    var col = new InfoObjectCollection<UIElementInfo>();

                    col.IsCachedObject = true;
                    col.Where = new WhereCondition().WhereEquals("ElementParentID", ElementID);
                    col.OrderByColumns = "ElementOrder ASC";

                    mChildElements = col;
                }

                return mChildElements;
            }
        }


        /// <summary>
        /// Help topics related to UI element.
        /// </summary>
        public IInfoObjectCollection<BaseInfo> HelpTopics
        {
            get
            {
                return Children[HelpTopicInfo.OBJECT_TYPE];
            }
        }


        /// <summary>
        /// Element's full name
        /// </summary>
        protected override string ObjectFullName
        {
            get
            {
                return ElementFullName;
            }
        }
        

        /// <summary>
        /// Application this UIElement belongs to, or null.
        /// </summary>
        public virtual UIElementInfo Application
        {
            get
            {
                if (mApplicationSeeked)
                {
                    return mApplication;
                }

                mApplicationSeeked = true;

                if (ElementLevel < 3)
                {
                    return null;
                }

                // An application is UIElement on 3rd level with "administration" UIElement as predecessor on 1st level
                SafeDictionary<int, BaseInfo> parentElements = GetParentElements();
                BaseInfo possibleCmsDeskUIElement = parentElements.TypedValues.FirstOrDefault(el => 1.Equals(el.GetValue("ElementLevel")));

                if (possibleCmsDeskUIElement == null || !UIELEMENT_ADMINISTRATION_CODENAME.Equals(possibleCmsDeskUIElement.GetValue("ElementName") as string, StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }

                if (ElementLevel == 3)
                {
                    mApplication = this;
                }
                else
                {
                    mApplication = (UIElementInfo)parentElements.TypedValues.FirstOrDefault(el => 3.Equals(el.GetValue("ElementLevel")));
                }

                return mApplication;
            }
        }


        /// <summary>
        /// Gets the value that indicates whether element is in administration scope (anywhere under the administration element)
        /// </summary>
        public virtual bool IsInAdministrationScope
        {
            get
            {
                var administration = UIElementInfoProvider.GetUIElementInfo("cms", UIELEMENT_ADMINISTRATION_CODENAME);
                if (administration != null)
                {
                    if (ElementIDPath.StartsWith(administration.ElementIDPath + "/", StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }


        /// <summary>
        /// Tells you whether UIElement is application or not.
        /// </summary>
        public virtual bool IsApplication
        {
            get
            {
                if ((Application != null) && (Application.ElementID == ElementID))
                {
                    return true;
                }
                return false;
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Returns the existing object based on current object data.
        /// </summary>
        protected override BaseInfo GetExisting()
        {
            return UIElementInfoProvider.GetUIElementInfo(ElementGUID);
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            UIElementInfoProvider.DeleteUIElementInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            UIElementInfoProvider.SetUIElementInfo(this);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty UIElementInfo structure.
        /// </summary>
        public UIElementInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an empty UIElementInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public UIElementInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Removes object dependencies. Removes ad-hoc page templates that are used in UIElement.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearHashtables">If true, hashtables of all objecttypes which were potentially modified are cleared</param>
        /// <remarks>UI ad-hoc templates are inside UIElement because they extend it from the page template side of view.</remarks>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // If the UIElement has some template, delete it if it's ad-hoc
            if (ElementPageTemplateID > 0)
            {
                var templateToDelete = new ObjectQuery(PredefinedObjectType.PAGETEMPLATE).WhereEquals("PageTemplateID", ElementPageTemplateID).WhereFalse("PageTemplateIsReusable").FirstOrDefault();
                if (templateToDelete != null)
                {
                    templateToDelete.Delete();
                }
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
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
            string result = base.BuildObjectPath(parentColumName, pathColumnName, levelColumnName, pathPartColumn, out level);
            
            // Ensure root element ID path
            if (level == 0)
            {
                return "/" + GetCurrentObjectPathPart(pathPartColumn);
            }

            return result;
        }


        /// <summary>
        /// Registers properties of the object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty("ChildElements", m => m.ChildElements);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Get the parent module
            var parentModule = ResourceInfoProvider.GetResourceInfo(ElementResourceID);
            if (parentModule != null)
            {
                // New element is custom in a client environment for modules under development (custom modules)
                ElementIsCustom = parentModule.ResourceIsInDevelopment && !SystemContext.DevelopmentMode;
            }

            // If it's clone of a root UIElement, change it's codename according to it's module codename
            if ((ElementParentID <= 0) && (parentModule != null))
            {
                ElementName = parentModule.ResourceName.Replace(".", "");
                ElementDisplayName = parentModule.ResourceDisplayName;
            }

            base.InsertAsCloneInternal(settings, result, originalObject);

            // Clone child elements
            DataSet ds = UIElementInfoProvider.GetChildUIElements(originalObject.Generalized.ObjectID);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                string originalCodeName = settings.CodeName;
                string originalDisplayName = settings.DisplayName;

                settings.CodeName = null;
                settings.DisplayName = null;

                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    var elem = new UIElementInfo(dr);

                    elem.ElementParentID = ElementID;
                    elem.InsertAsClone(settings, result);
                }

                settings.CodeName = originalCodeName;
                settings.DisplayName = originalDisplayName;
            }
        }


        /// <summary>
        /// Creates where condition according to Parent, Group and Site settings.
        /// </summary>
        protected override WhereCondition GetSiblingsWhereCondition()
        {
            var ti = TypeInfo;
            // Make sure WHERE condition is surrounded by brackets because of complex conditions with OR operator
            var where = new WhereCondition().Where(ti.WhereCondition);

            if (ti.GroupIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if (ObjectGroupID == 0)
                {
                    where.WhereNull(ti.GroupIDColumn);
                }
                else
                {
                    where.WhereEquals(ti.GroupIDColumn, ObjectGroupID);
                }
            }

            if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                if (ObjectSiteID == 0)
                {
                    where.WhereNull(ti.SiteIDColumn);
                }
                else
                {
                    where.WhereEquals(ti.SiteIDColumn, ObjectSiteID);
                }
            }

            return where.Where("ISNULL(ElementParentID, 0) = " + ElementParentID);
        }


        /// <summary>
        /// Finds first element from children collection starting with prefix 'new'
        /// </summary>
        public UIElementInfo GetNewElement()
        {
            ObjectProperty op = ChildElements.CodeNames.FirstOrDefault(m => ((UIElementInfo)m.Object).RepresentsNew);
            return (op != null) ? (UIElementInfo)op.Object : null;
        }


        /// <summary>
        /// Finds first element from children collection starting with prefix 'edit'
        /// </summary>
        public UIElementInfo GetEditElement()
        {
            ObjectProperty op = ChildElements.CodeNames.FirstOrDefault(m => ((UIElementInfo)m.Object).RepresentsEdit);
            return (op != null) ? (UIElementInfo)op.Object : null;
        }


        /// <summary>
        /// Return element's parent objects
        /// </summary>
        public SafeDictionary<int, BaseInfo> GetParentElements()
        {
            // Split ID path and collects all elements included in it.
            // Filter zero ID and element's ID itself.
            IEnumerable<int> ids = Array.ConvertAll(ElementIDPath.Split('/'), s => ValidationHelper.GetInteger(s, 0))
                                        .Where(x => ((x != 0) && (x != ElementID)));

            return ProviderHelper.GetInfosByIds("cms.uielement", ids);
        }


        /// <summary>
        /// Returns navigation string that describes a route to the application represented by UI element.
        /// </summary>
        /// <param name="cultureCode">Culture code</param>
        public string GetApplicationNavigationString(string cultureCode = null)
        {
            return ResHelper.LocalizeString(UIElementInfoProvider.GetElementCaption(this, false));
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            ElementCheckModuleReadPermission = true;
        }

        #endregion
    }
}
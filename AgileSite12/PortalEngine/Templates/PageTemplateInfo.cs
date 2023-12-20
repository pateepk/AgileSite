using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DeviceProfiles;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.Membership;
using CMS.Modules;
using CMS.PortalEngine;
using CMS.SiteProvider;

[assembly: RegisterObjectType(typeof(PageTemplateInfo), PageTemplateInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// Page template info data container class.
    /// </summary>
    public class PageTemplateInfo : AbstractInfo<PageTemplateInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = PredefinedObjectType.PAGETEMPLATE;


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(PageTemplateInfoProvider), OBJECT_TYPE, "CMS.PageTemplate", "PageTemplateID", "PageTemplateLastModified", "PageTemplateGUID", "PageTemplateCodeName", "PageTemplateDisplayName", null, "PageTemplateSiteID", null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("PageTemplateLayoutID", LayoutInfo.OBJECT_TYPE),
                new ObjectDependency("PageTemplateCategoryID", PageTemplateCategoryInfo.OBJECT_TYPE),
            },
            Extends = new List<ExtraColumn>
            {
                new ExtraColumn(UIElementInfo.OBJECT_TYPE, "ElementPageTemplateID"),
                new ExtraColumn(PredefinedObjectType.DOCUMENTTYPE, "ClassDefaultPageTemplateID"),
            },
            ModuleName = ModuleName.DESIGN,
            ThumbnailGUIDColumn = "PageTemplateThumbnailGUID",
            HasMetaFiles = true,
            ImportExportSettings =
            {
                IsExportable = true,
                LogExport = true,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT),
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(SITE, DEVELOPMENT),
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = true,
            SupportsGlobalObjects = true,
            SupportsLocking = true,
            HasExternalColumns = true,
            VersionGUIDColumn = "PageTemplateVersionGUID",
            FormDefinitionColumn = "PageTemplateProperties",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            CSSColumn = EXTERNAL_COLUMN_CSS,
            DefaultData = new DefaultDataSettings(),
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<PageTemplateInstance>("PageTemplateWebParts"),
                    new StructuredField<FormInfo>("PageTemplateProperties"),
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true
            }
        };

        #endregion


        #region "Variables"

        /// <summary>
        /// Page template instance.
        /// </summary>
        protected PageTemplateInstance mTemplateInstance;

        /// <summary>
        ///  External column name for Layout Code
        /// </summary>
        public const string EXTERNAL_COLUMN_CODE = "PageTemplateLayout";

        /// <summary>
        ///  External column name for Layout CSS
        /// </summary>
        public const string EXTERNAL_COLUMN_CSS = "PageTemplateCSS";

        /// <summary>
        /// Merged properties form info (with default XML data)
        /// </summary>
        private FormInfo mPageTemplatePropertiesForm;

        #endregion


        #region "Public properties"

        /// <summary>
        /// Indicates whether current headers can be inherited by child templates
        /// </summary>
        [DatabaseField]
        public bool PageTemplateAllowInheritHeader
        {
            get
            {
                return GetBooleanValue("PageTemplateAllowInheritHeader", false);
            }
            set
            {
                SetValue("PageTemplateAllowInheritHeader", value);
            }
        }


        /// <summary>
        /// If true, page template is considered as layout
        /// </summary>
        [DatabaseField]
        public bool PageTemplateIsLayout
        {
            get
            {
                return GetBooleanValue("PageTemplateIsLayout", false);
            }
            set
            {
                SetValue("PageTemplateIsLayout", value);
            }
        }


        /// <summary>
        /// Template's properties form (used in UI)
        /// </summary>
        public FormInfo PageTemplatePropertiesForm
        {
            get
            {
                return mPageTemplatePropertiesForm ?? (mPageTemplatePropertiesForm = LoadTemplateUIProperties());
            }
            set
            {
                mPageTemplatePropertiesForm = value;
            }
        }


        /// <summary>
        /// Indicates whether current template should inherit headers from parent templates
        /// </summary>
        [DatabaseField]
        public bool PageTemplateInheritParentHeader
        {
            get
            {
                return GetBooleanValue("PageTemplateInheritParentHeader", true);
            }
            set
            {
                SetValue("PageTemplateInheritParentHeader", value);
            }
        }


        /// <summary>
        /// Parent page info
        /// </summary>
        public IPageInfo ParentPageInfo
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets the page template ID.
        /// </summary>
        [DatabaseField("PageTemplateID")]
        public int PageTemplateId
        {
            get
            {
                return GetIntegerValue("PageTemplateID", 0);
            }
            set
            {
                SetValue("PageTemplateID", value);
            }
        }


        /// <summary>
        /// UI page template properties.
        /// </summary>
        [DatabaseField]
        public String PageTemplateProperties
        {
            get
            {
                return GetStringValue("PageTemplateProperties", String.Empty);
            }
            set
            {
                SetValue("PageTemplateProperties", value);
            }
        }


        /// <summary>
        /// Gets or sets the page template code name.
        /// </summary>
        [DatabaseField("PageTemplateCodeName")]
        public string CodeName
        {
            get
            {
                return GetStringValue("PageTemplateCodeName", "generic");
            }
            set
            {
                SetValue("PageTemplateCodeName", value);
            }
        }


        /// <summary>
        /// Indicates whether template has design mode
        /// </summary>
        [RegisterProperty]
        public bool HasDesignMode
        {
            get
            {
                return ((PageTemplateType == PageTemplateTypeEnum.Portal) || (PageTemplateType == PageTemplateTypeEnum.UI) || (PageTemplateType == PageTemplateTypeEnum.Dashboard) || (PageTemplateType == PageTemplateTypeEnum.AspxPortal));
            }
        }


        /// <summary>
        /// Indicates whether display ASPX tab
        /// </summary>
        [RegisterProperty]
        public bool ShowTemplateASPXTab
        {
            get
            {
                return ValidationHelper.GetBoolean(SettingsHelper.AppSettings["CMSShowTemplateASPXTab"], false);
            }
        }


        /// <summary>
        /// Gets or sets the page template display name.
        /// </summary>
        [DatabaseField("PageTemplateDisplayName")]
        public string DisplayName
        {
            get
            {
                return GetStringValue("PageTemplateDisplayName", "Generic");
            }
            set
            {
                SetValue("PageTemplateDisplayName", value);
            }
        }


        /// <summary>
        /// Gets or sets the page template description.
        /// </summary>
        [DatabaseField("PageTemplateDescription")]
        public string Description
        {
            get
            {
                return GetStringValue("PageTemplateDescription", "Generic default template");
            }
            set
            {
                SetValue("PageTemplateDescription", value);
            }
        }


        /// <summary>
        /// Gets or sets the page template CSS.
        /// </summary>
        [DatabaseField]
        public string PageTemplateCSS
        {
            get
            {
                return GetStringValue("PageTemplateCSS", "");
            }
            set
            {
                SetValue("PageTemplateCSS", value);
            }
        }


        /// <summary>
        /// Gets or sets the page template filename.
        /// </summary>
        [DatabaseField("PageTemplateFile")]
        public string FileName
        {
            get
            {
                return GetStringValue("PageTemplateFile", "~/default.aspx");
            }
            set
            {
                SetValue("PageTemplateFile", value);
            }
        }


        /// <summary>
        /// Indicates whether the <see cref="PageTemplateType"/> is <see cref="PageTemplateTypeEnum.Portal"/>.
        /// </summary>
        [RegisterProperty]
        public bool IsPortal
        {
            get
            {
                return (PageTemplateType == PageTemplateTypeEnum.Portal);
            }
        }


        /// <summary>
        /// Gets or sets flag indicating that this template can be used for product section.
        /// </summary>
        [DatabaseField]
        public bool PageTemplateIsAllowedForProductSection
        {
            get
            {
                return GetBooleanValue("PageTemplateIsAllowedForProductSection", false);
            }
            set
            {
                SetValue("PageTemplateIsAllowedForProductSection", value);
            }
        }


        /// <summary>
        /// Indicates whether the <see cref="PageTemplateType"/> is <see cref="PageTemplateTypeEnum.Aspx"/> or <see cref="PageTemplateTypeEnum.AspxPortal"/> (mixed mode).
        /// </summary>
        [RegisterProperty]
        public bool IsAspx
        {
            get
            {
                switch (PageTemplateType)
                {
                    case PageTemplateTypeEnum.Aspx:
                    case PageTemplateTypeEnum.AspxPortal:
                        return true;

                    default:
                        return false;
                }
            }
        }


        /// <summary>
        /// Page template type.
        /// </summary>
        [DatabaseField(ValueType = typeof(string))]
        public PageTemplateTypeEnum PageTemplateType
        {
            get
            {
                // Get by the code
                string code = ValidationHelper.GetString(GetValue("PageTemplateType"), "");
                PageTemplateTypeEnum result = code.ToEnum<PageTemplateTypeEnum>();

                return result;
            }
            set
            {
                // Set as string
                SetValue("PageTemplateType", PageTemplateInfoProvider.GetPageTemplateTypeCode(value));
            }
        }


        /// <summary>
        /// Gets or sets the page template CategoryID.
        /// </summary>
        [DatabaseField("PageTemplateCategoryID")]
        public int CategoryID
        {
            get
            {
                return GetIntegerValue("PageTemplateCategoryID", 0);
            }
            set
            {
                SetValue("PageTemplateCategoryID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the page template LayoutID.
        /// </summary>
        [DatabaseField("PageTemplateLayoutID")]
        public int LayoutID
        {
            get
            {
                return GetIntegerValue("PageTemplateLayoutID", 0);
            }
            set
            {
                SetValue("PageTemplateLayoutID", value, (value > 0));
            }
        }


        /// <summary>
        /// Gets or sets the page template WebParts.
        /// </summary>
        [DatabaseField("PageTemplateWebParts")]
        public string WebParts
        {
            get
            {
                return TemplateInstance.GetZonesXML();
            }
            set
            {
                SetValue("PageTemplateWebParts", value);
                TemplateInstance.WebParts = value;
            }
        }


        /// <summary>
        /// Gets or sets flag whether page template is reusable.
        /// </summary>
        [DatabaseField("PageTemplateIsReusable")]
        public bool IsReusable
        {
            get
            {
                return GetBooleanValue("PageTemplateIsReusable", false);
            }
            set
            {
                SetValue("PageTemplateIsReusable", value);
            }
        }


        /// <summary>
        /// Gets or sets flag whether show page template as master template.
        /// </summary>
        [DatabaseField("PageTemplateShowAsMasterTemplate")]
        public bool ShowAsMasterTemplate
        {
            get
            {
                return GetBooleanValue("PageTemplateShowAsMasterTemplate", false);
            }
            set
            {
                SetValue("PageTemplateShowAsMasterTemplate", value);
            }
        }


        /// <summary>
        /// Page levels expression that the page inherits.
        /// </summary>
        [DatabaseField("PageTemplateInheritPageLevels")]
        public string InheritPageLevels
        {
            get
            {
                return GetStringValue("PageTemplateInheritPageLevels", "");
            }
            set
            {
                SetValue("PageTemplateInheritPageLevels", value);
            }
        }


        /// <summary>
        /// Web part zones contained within the Page template.
        /// </summary>
        public List<WebPartZoneInstance> WebPartZones
        {
            get
            {
                return TemplateInstance.WebPartZones;
            }
        }


        /// <summary>
        /// Template structure instance.
        /// </summary>
        public PageTemplateInstance TemplateInstance
        {
            get
            {
                if (mTemplateInstance == null)
                {
                    // Prepare the template instance
                    string webPartsXml = ValidationHelper.GetString(GetValue("PageTemplateWebParts"), "<page></page>");
                    mTemplateInstance = new PageTemplateInstance(webPartsXml);

                    mTemplateInstance.ParentPageTemplate = this;
                }

                return mTemplateInstance;
            }
            set
            {
                mTemplateInstance = value;
            }
        }


        /// <summary>
        /// Page template layout type.
        /// </summary>
        [DatabaseField]
        public LayoutTypeEnum PageTemplateLayoutType
        {
            get
            {
                return LayoutInfoProvider.GetLayoutTypeEnum(GetStringValue("PageTemplateLayoutType", ""));
            }
            set
            {
                SetValue("PageTemplateLayoutType", LayoutInfoProvider.GetLayoutTypeCode(value));
            }
        }


        /// <summary>
        /// Page template layout.
        /// </summary>
        [DatabaseField]
        public string PageTemplateLayout
        {
            get
            {
                return GetStringValue("PageTemplateLayout", null);
            }
            set
            {
                SetValue("PageTemplateLayout", value);
            }
        }


        /// <summary>
        /// Page template version GUID.
        /// </summary>
        [DatabaseField]
        public string PageTemplateVersionGUID
        {
            get
            {
                return GetStringValue("PageTemplateVersionGUID", null);
            }
            set
            {
                SetValue("PageTemplateVersionGUID", value);
            }
        }


        /// <summary>
        /// Page template header.
        /// </summary>
        [DatabaseField]
        public string PageTemplateHeader
        {
            get
            {
                return GetStringValue("PageTemplateHeader", null);
            }
            set
            {
                SetValue("PageTemplateHeader", value);
            }
        }


        /// <summary>
        /// PageTemplate GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid PageTemplateGUID
        {
            get
            {
                return GetGuidValue("PageTemplateGUID", Guid.Empty);
            }
            set
            {
                SetValue("PageTemplateGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime PageTemplateLastModified
        {
            get
            {
                return GetDateTimeValue("PageTemplateLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("PageTemplateLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Page template site ID.
        /// </summary>
        [DatabaseField]
        public int PageTemplateSiteID
        {
            get
            {
                return GetIntegerValue("PageTemplateSiteID", 0);
            }
            set
            {
                SetValue("PageTemplateSiteID", value, (value > 0));
            }
        }


        /// <summary>
        /// Page for all pages.
        /// </summary>
        [DatabaseField]
        public bool PageTemplateForAllPages
        {
            get
            {
                return GetBooleanValue("PageTemplateForAllPages", true);
            }
            set
            {
                SetValue("PageTemplateForAllPages", value);
            }
        }


        /// <summary>
        /// Page template thumbnail metafile GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid PageTemplateThumbnailGUID
        {
            get
            {
                return GetGuidValue("PageTemplateThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("PageTemplateThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Page template icon class defining the page template thumbnail.
        /// </summary>
        [DatabaseField]
        public virtual string PageTemplateIconClass
        {
            get
            {
                return GetStringValue("PageTemplateIconClass", null);
            }
            set
            {
                SetValue("PageTemplateIconClass", value, string.Empty);
            }
        }


        /// <summary>
        /// If true, the page template is cloned as ad-hoc when selected for a new document
        /// </summary>
        [DatabaseField]
        public bool PageTemplateCloneAsAdHoc
        {
            get
            {
                return GetBooleanValue("PageTemplateCloneAsAdHoc", false);
            }
            set
            {
                SetValue("PageTemplateCloneAsAdHoc", value);
            }
        }


        /// <summary>
        /// Gets or sets the page template node GUID - For Ad-hoc templates
        /// </summary>
        [DatabaseField]
        public Guid PageTemplateNodeGUID
        {
            get
            {
                return GetGuidValue("PageTemplateNodeGUID", Guid.Empty);
            }
            set
            {
                SetValue("PageTemplateNodeGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Master page template ID of the template, if set, the page template uses this template as extra master template in the view hierarchy
        /// </summary>
        [DatabaseField]
        public int PageTemplateMasterPageTemplateID
        {
            get
            {
                return GetIntegerValue("PageTemplateMasterPageTemplateID", 0);
            }
            set
            {
                SetValue("PageTemplateMasterPageTemplateID", value);
            }
        }


        /// <summary>
        /// Indicates whether the theme path points at an external storage.
        /// </summary>
        [RegisterProperty(Hidden = true)]
        public bool UsesExternalStorage
        {
            get
            {
                return StorageHelper.IsExternalStorage(GetThemePath());
            }
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            PageTemplateInfoProvider.DeletePageTemplate(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            // Do not store empty IconClass, use NULL value
            if (string.IsNullOrEmpty(PageTemplateIconClass))
            {
                PageTemplateIconClass = null;
            }

            PageTemplateInfoProvider.SetPageTemplateInfo(this);
        }


        /// <summary>
        /// Indicates if the object supports deleting to recycle bin.
        /// </summary>
        protected override bool AllowRestore
        {
            get
            {
                return base.AllowRestore && IsReusable;
            }
            set
            {
                base.AllowRestore = value;
            }
        }


        /// <summary>
        /// Returns virtual relative path for specific column
        /// </summary>
        /// <param name="externalColumnName">External column name</param>
        /// <param name="versionGuid">Version GUID. If not defined physical path is generated</param>
        protected override string GetVirtualFileRelativePath(string externalColumnName, string versionGuid)
        {
            // Ensure extension
            string extension = (PageTemplateLayoutType == LayoutTypeEnum.Html) ? ".html" : ".ascx";
            if (EXTERNAL_COLUMN_CSS.EqualsCSafe(externalColumnName, true))
            {
                extension = ".css";
            }

            // Keep original version GUID
            string originalVersionGuid = versionGuid;

            // Do not use version GUID for files stored externally
            if (PageTemplateInfoProvider.StorePageTemplatesInExternalStorage || SettingsKeyInfoProvider.DeploymentMode)
            {
                versionGuid = null;
            }

            string directory = PageTemplateInfoProvider.TemplateLayoutsDirectory;
            string prefix = String.Empty;
            if (!IsReusable)
            {
                directory = PageTemplateInfoProvider.AdhocTemplateLayoutsDirectory;
                prefix = SiteInfoProvider.GetSiteName(PageTemplateSiteID) + "/" + CodeName.Substring(0, 2);
            }
            else if (CodeName.Contains("."))
            {
                prefix = CodeName.Substring(0, CodeName.IndexOf('.'));
            }

            string path = VirtualPathHelper.GetVirtualFileRelativePath(CodeName, extension, directory, prefix, versionGuid);
            if (!SettingsKeyInfoProvider.DeploymentMode && PageTemplateInfoProvider.StorePageTemplatesInExternalStorage && !FileHelper.FileExists(path))
            {
                path = VirtualPathHelper.GetVirtualFileRelativePath(CodeName, extension, directory, prefix, originalVersionGuid);
            }

            return path;
        }


        /// <summary>
        /// Returns path to externally stored template layout codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            ExternalColumnSettings<PageTemplateInfo> settings = new ExternalColumnSettings<PageTemplateInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CODE, null),
                StoreInExternalStorageSettingsKey = "CMSStorePageTemplatesInFS",
                SetDataTransformation = (m, data, readOnly) => LayoutInfoProvider.AddLayoutDirectives(ValidationHelper.GetString(data, ""), m.PageTemplateLayoutType),
                GetDataTransformation = (m, data) => VirtualPathHelper.RemoveDirectives(ValidationHelper.GetString(data, ""), LayoutInfoProvider.DefaultDirectives)
            };

            // CSS Component
            ExternalColumnSettings<PageTemplateInfo> cssSettings = new ExternalColumnSettings<PageTemplateInfo>()
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CSS, null),
                StoreInExternalStorageSettingsKey = "CMSStorePageTemplatesInFS"
            };

            RegisterExternalColumn(EXTERNAL_COLUMN_CODE, settings);
            RegisterExternalColumn(EXTERNAL_COLUMN_CSS, cssSettings);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty PageTemplateInfo structure.
        /// </summary>
        public PageTemplateInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an empty PageTemplateInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public PageTemplateInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Loads the object default data
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            IsReusable = false;
            FileName = String.Empty;
            PageTemplateType = PageTemplateTypeEnum.Portal;
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the theme path for the object
        /// </summary>
        public string GetThemePath()
        {
            return "~/App_Themes/Components/PageTemplates/" + ValidationHelper.GetSafeFileName(CodeName);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            bool copyFiles = false;
            bool copyScopes = false;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                copyFiles = ValidationHelper.GetBoolean(p[PredefinedObjectType.PAGETEMPLATE + ".appthemes"], false);
                copyScopes = ValidationHelper.GetBoolean(p[PredefinedObjectType.PAGETEMPLATE + ".templatecope"], false);
            }

            if (copyFiles)
            {
                // Copy files from App_Themes
                string sourcePath = "~/App_Themes/Components/PageTemplates/" + originalObject.Generalized.ObjectCodeName;
                string targetPath = "~/App_Themes/Components/PageTemplates/" + ObjectCodeName;

                FileHelper.CopyDirectory(sourcePath, targetPath);
            }

            IsReusable = true;
            PageTemplateSiteID = 0;
            PageTemplateNodeGUID = Guid.Empty;
            Insert();

            if (!copyScopes && !settings.ExcludedChildTypes.Contains(PageTemplateScopeInfo.OBJECT_TYPE))
            {
                settings.ExcludedChildTypes.Add(PageTemplateScopeInfo.OBJECT_TYPE);
            }
        }


        /// <summary>
        /// Loads template's UI properties
        /// </summary>
        private FormInfo LoadTemplateUIProperties()
        {
            // Add 'Properties' category at the beginning if none is specified
            String properties = PageTemplateProperties;
            properties = FormHelper.EnsureDefaultCategory(properties, ResHelper.GetString("general.properties"));

            FormInfo fiProperties = new FormInfo(properties);
            FormInfo fiBefore = PortalFormHelper.GetUIElementDefaultPropertiesForm(UIElementPropertiesPosition.Before);
            FormInfo fiAfter = PortalFormHelper.GetUIElementDefaultPropertiesForm(UIElementPropertiesPosition.After);

            if (fiBefore != null)
            {
                FormInfo beforeFI = fiBefore.Clone();
                beforeFI.CombineWithForm(fiProperties, true, null, true);
                fiProperties = beforeFI;
            }

            // Combine with global after schema, don't overwrite existing fields
            if (fiAfter != null)
            {
                fiProperties.CombineWithForm(fiAfter, false, null, true);
            }

            return fiProperties;
        }


        /// <summary>
        /// Converts PermissionEnum to permission codename which will be checked when CheckPermission() is called.
        /// </summary>
        /// <param name="permission">Permission to convert to string</param>
        protected override string GetPermissionName(PermissionsEnum permission)
        {
            switch (permission)
            {
                case PermissionsEnum.Modify:
                case PermissionsEnum.Read:
                    return "design";
            }
            return base.GetPermissionName(permission);
        }


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
                case PermissionsEnum.Destroy:
                    return UserInfoProvider.IsAuthorizedPerResource("cms.globalpermissions", "DestroyObjects", siteName, (UserInfo)userInfo) ||
                           UserInfoProvider.IsAuthorizedPerResource(ModuleName.DESIGN, "Destroy" + TypeInfo.ObjectType.Replace(".", ""), siteName, (UserInfo)userInfo, exceptionOnFailure);

                default:
                    return base.CheckPermissionsInternal(permission, siteName, userInfo, exceptionOnFailure);
            }
        }


        /// <summary>
        /// Gets the child object where condition.
        /// </summary>
        /// <param name="where">Original where condition</param>
        /// <param name="objectType">Object type of the child object</param>
        protected override WhereCondition GetChildWhereCondition(WhereCondition where, string objectType)
        {
            // Ensure base where condition
            where = where ?? new WhereCondition();

            switch (objectType)
            {
                case PredefinedObjectType.MVTVARIANT:
                    // Get the MVT variants for web parts and zones only (no widgets)
                    where.WhereNull("MVTVariantDocumentID");
                    break;

                case PredefinedObjectType.MVTCOMBINATION:
                    // Get the MVT combinations for web parts and zones only (no widgets)
                    where.WhereNull("MVTCombinationDocumentID");
                    break;

                case PredefinedObjectType.CONTENTPERSONALIZATIONVARIANT:
                    // Get the Content personalization variants for web parts and zones only (no widgets)
                    where.WhereNull("VariantDocumentID");
                    break;
            }

            return base.GetChildWhereCondition(where, objectType);
        }


        /// <summary>
        /// Gets the where condition to filter out the default installation data
        /// </summary>
        /// <param name="recursive">Indicates whether where condition should contain further dependency conditions.</param>
        /// <param name="globalOnly">Indicates whether only objects with null in their site ID column should be included.</param>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected override string GetDefaultDataWhereCondition(bool recursive = true, bool globalOnly = true, IEnumerable<string> excludedNames = null)
        {
            // Ignore dependencies since page template has reverted category dependency (category depends on contents)
            string where = base.GetDefaultDataWhereCondition(false, globalOnly, excludedNames);

            if (globalOnly)
            {
                // Include templates for administration UI and dashboard
                var systemWhere = new WhereCondition().WhereIn("PageTemplateType", new[] { "ui", "dashboard" });
                where = SqlHelper.AddWhereCondition(where, systemWhere.ToString(true));
            }

            return where;
        }

        #endregion
    }
}

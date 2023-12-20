using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;

using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.Query;
using CMS.DeviceProfiles;
using CMS.FormEngine;
using CMS.Helpers;
using CMS.IO;
using CMS.MacroEngine;
using CMS.Modules;
using CMS.PortalEngine;

[assembly: RegisterObjectType(typeof(WebPartInfo), WebPartInfo.OBJECT_TYPE)]

namespace CMS.PortalEngine
{
    /// <summary>
    /// WebPart info data container class.
    /// </summary>
    public class WebPartInfo : AbstractInfo<WebPartInfo>, IThemeInfo
    {
        #region "Type information"

        /// <summary>
        /// Object type
        /// </summary>
        public const string OBJECT_TYPE = "cms.webpart";


        /// <summary>
        /// Type information.
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(WebPartInfoProvider), OBJECT_TYPE, "CMS.WebPart", "WebPartID", "WebPartLastModified", "WebPartGUID", "WebPartName", "WebPartDisplayName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency>
            {
                new ObjectDependency("WebPartParentID", OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("WebPartCategoryID", WebPartCategoryInfo.OBJECT_TYPE, ObjectDependencyEnum.Required),
                new ObjectDependency("WebPartResourceID", ResourceInfo.OBJECT_TYPE)
            },
            DeleteObjectWithAPI = true,
            ModuleName = ModuleName.DESIGN,
            ImportExportSettings =
            {
                IncludeToWebTemplateExport = ObjectRangeEnum.Site,
                IsExportable = true,
                IncludeToExportParentDataSet = IncludeToParentEnum.None,
                LogExport = true,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
                // Keep order to ensure export of inherited web parts
                OrderBy = "WebPartParentID, WebPartGUID",
            },
            SynchronizationSettings =
            {
                IncludeToSynchronizationParentDataSet = IncludeToParentEnum.None,
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                }
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            DefaultOrderBy = "WebPartParentID, WebPartDisplayName",
            ResourceIDColumn = "WebPartResourceID",
            ThumbnailGUIDColumn = "WebPartThumbnailGUID",
            FormDefinitionColumn = "WebPartProperties",
            HasMetaFiles = true,
            DefaultData = new DefaultDataSettings
            {
                OrderBy = "WebPartParentID, WebPartID",
                ExcludedColumns = new List<string> { "WebPartDefaultConfiguration" }
            },
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField("WebPartProperties"),
                    new StructuredField<PageTemplateInstance>("WebPartDefaultConfiguration"),
                    new StructuredField("WebPartDefaultValues")
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
        /// Form info definition
        /// </summary>
        internal FormInfo mFormInfo;

        /// <summary>
        /// Default web part configuration
        /// </summary>
        internal PageTemplateInstance mDefaultConfiguration;

        /// <summary>
        /// Name of the web part in the web part default configuration
        /// </summary>
        public const string DEFAULT_CONFIG_WEBPARTNAME = "webpart";

        /// <summary>
        /// Name of the web part in the web part default configuration
        /// </summary>
        public static Guid DEFAULT_CONFIG_INSTANCEGUID = new Guid("ccbaf0fc-dae1-4614-967f-119229871b9f");

        /// <summary>
        /// Name of the web part zone in the web part default configuration
        /// </summary>
        public const string DEFAULT_CONFIG_ZONENAME = "zone";

        #endregion


        #region "Properties"

        /// <summary>
        /// The WebPart ID.
        /// </summary>
        [DatabaseField]
        public int WebPartID
        {
            get
            {
                return GetIntegerValue("WebPartID", 0);
            }
            set
            {
                SetValue("WebPartID", value);
            }
        }


        /// <summary>
        /// The WebPartName.
        /// </summary>
        [DatabaseField]
        public string WebPartName
        {
            get
            {
                return GetStringValue("WebPartName", "");
            }
            set
            {
                SetValue("WebPartName", value);
            }
        }


        /// <summary>
        /// The WebPartDisplayName.
        /// </summary>
        [DatabaseField]
        public string WebPartDisplayName
        {
            get
            {
                return GetStringValue("WebPartDisplayName", "");
            }
            set
            {
                SetValue("WebPartDisplayName", value);
            }
        }


        /// <summary>
        /// The WebPartDescription.
        /// </summary>
        [DatabaseField]
        public string WebPartDescription
        {
            get
            {
                return GetStringValue("WebPartDescription", "");
            }
            set
            {
                SetValue("WebPartDescription", ValidationHelper.GetString(value, ""));
            }
        }


        /// <summary>
        /// The WebPartCSS.
        /// </summary>
        [DatabaseField]
        public string WebPartCSS
        {
            get
            {
                return GetStringValue("WebPartCSS", "");
            }
            set
            {
                SetValue("WebPartCSS", ValidationHelper.GetString(value, ""));
            }
        }


        /// <summary>
        /// The WebPartProperties.
        /// </summary>
        [DatabaseField]
        public string WebPartProperties
        {
            get
            {
                return GetStringValue("WebPartProperties", "");
            }
            set
            {
                SetValue("WebPartProperties", value);
            }
        }


        /// <summary>
        /// Web part default properties.
        /// </summary>
        [DatabaseField]
        public string WebPartDefaultValues
        {
            get
            {
                return GetStringValue("WebPartDefaultValues", "");
            }
            set
            {
                SetValue("WebPartDefaultValues", value);
            }
        }


        /// <summary>
        /// The WebPartFileName.
        /// </summary>
        [DatabaseField]
        public string WebPartFileName
        {
            get
            {
                return GetStringValue("WebPartFileName", "");
            }
            set
            {
                SetValue("WebPartFileName", value);
            }
        }


        /// <summary>
        /// The WebPartCategoryID.
        /// </summary>
        [DatabaseField]
        public int WebPartCategoryID
        {
            get
            {
                return GetIntegerValue("WebPartCategoryID", 0);
            }
            set
            {
                SetValue("WebPartCategoryID", value);
            }
        }


        /// <summary>
        /// Gets or sets the web part type.
        /// </summary>
        [DatabaseField]
        public int WebPartType
        {
            get
            {
                return GetIntegerValue("WebPartType", 0);
            }
            set
            {
                SetValue("WebPartType", value);
            }
        }


        /// <summary>
        /// The WebPartParentID.
        /// </summary>
        [DatabaseField]
        public int WebPartParentID
        {
            get
            {
                return GetIntegerValue("WebPartParentID", 0);
            }
            set
            {
                SetValue("WebPartParentID", value, (value > 0));
            }
        }


        /// <summary>
        /// Webpart documentation field.
        /// </summary>
        [DatabaseField]
        public string WebPartDocumentation
        {
            get
            {
                return GetStringValue("WebPartDocumentation", "");
            }
            set
            {
                SetValue("WebPartDocumentation", value);
            }
        }


        /// <summary>
        /// WebPart GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WebPartGUID
        {
            get
            {
                return GetGuidValue("WebPartGUID", Guid.Empty);
            }
            set
            {
                SetValue("WebPartGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// Object last modified.
        /// </summary>
        [DatabaseField]
        public virtual DateTime WebPartLastModified
        {
            get
            {
                return GetDateTimeValue("WebPartLastModified", DateTimeHelper.ZERO_TIME);
            }
            set
            {
                SetValue("WebPartLastModified", value, DateTimeHelper.ZERO_TIME);
            }
        }


        /// <summary>
        /// Web part resource (module) ID.
        /// </summary>
        [DatabaseField]
        public int WebPartResourceID
        {
            get
            {
                return GetIntegerValue("WebPartResourceID", 0);
            }
            set
            {
                if (value <= 0)
                {
                    SetValue("WebPartResourceID", null);
                }
                else
                {
                    SetValue("WebPartResourceID", value);
                }
            }
        }


        /// <summary>
        /// Indicates whether the web part properties dialog should be displayed when inserting a web part to the page.
        /// </summary>
        [DatabaseField]
        public virtual bool WebPartSkipInsertProperties
        {
            get
            {
                return GetBooleanValue("WebPartSkipInsertProperties", false);
            }
            set
            {
                SetValue("WebPartSkipInsertProperties", value);
            }
        }


        /// <summary>
        /// WebPart thumbnail metafile GUID.
        /// </summary>
        [DatabaseField]
        public virtual Guid WebPartThumbnailGUID
        {
            get
            {
                return GetGuidValue("WebPartThumbnailGUID", Guid.Empty);
            }
            set
            {
                SetValue("WebPartThumbnailGUID", value, Guid.Empty);
            }
        }


        /// <summary>
        /// WebPart icon class defining the web part thumbnail.
        /// </summary>
        [DatabaseField]
        public virtual string WebPartIconClass
        {
            get
            {
                return GetStringValue("WebPartIconClass", null);
            }
            set
            {
                SetValue("WebPartIconClass", value, string.Empty);
            }
        }


        /// <summary>
        /// Gets or sets the web part default configuration
        /// </summary>
        [DatabaseField]
        public string WebPartDefaultConfiguration
        {
            get
            {
                return DefaultConfiguration.GetZonesXML();
            }
            set
            {
                SetValue("WebPartDefaultConfiguration", value);
                DefaultConfiguration.WebParts = value;
            }
        }


        /// <summary>
        /// Template structure instance.
        /// </summary>
        public PageTemplateInstance DefaultConfiguration
        {
            get
            {
                if (mDefaultConfiguration == null)
                {
                    // Prepare the template instance
                    string webPartsXml = ValidationHelper.GetString(GetValue("WebPartDefaultConfiguration"), "<page></page>");
                    mDefaultConfiguration = new PageTemplateInstance(webPartsXml);
                }

                return mDefaultConfiguration;
            }
            set
            {
                mDefaultConfiguration = value;
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
            WebPartInfoProvider.DeleteWebPartInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            string filename = WebPartFileName;
            string rootDirectory = WebPartInfoProvider.WebPartsDirectory.ToLowerCSafe();
            if (!String.IsNullOrEmpty(filename) && filename.ToLowerCSafe().StartsWithCSafe(rootDirectory + "/"))
            {
                WebPartFileName = filename.Substring(rootDirectory.Length + 1);
            }

            // Do not store empty IconClass, use NULL value
            if (string.IsNullOrEmpty(WebPartIconClass))
            {
                WebPartIconClass = null;
            }

            WebPartInfoProvider.SetWebPartInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Register the properties of the object
        /// </summary>
        protected sealed override void RegisterProperties()
        {
            base.RegisterProperties();

            RegisterProperty<WebPartInfo>("IsLayout", m => ((WebPartTypeEnum)m.WebPartType == WebPartTypeEnum.Layout));
        }


        /// <summary>
        /// Gets the web part info for current web part
        /// </summary>
        public FormInfo GetWebPartFormInfo()
        {
            if (mFormInfo == null)
            {
                string wpProperties = WebPartProperties;

                // Use parent web part if is defined
                if (WebPartParentID > 0)
                {
                    WebPartInfo parentWpi = WebPartInfoProvider.GetWebPartInfo(WebPartParentID);
                    if (parentWpi != null)
                    {
                        wpProperties = FormHelper.MergeFormDefinitions(parentWpi.WebPartProperties, WebPartProperties);
                    }
                }

                // Get before FormInfo
                FormInfo beforeFI = PortalFormHelper.GetPositionFormInfo((WebPartTypeEnum)WebPartType, PropertiesPosition.Before);
                // Get after FormInfo
                FormInfo afterFI = PortalFormHelper.GetPositionFormInfo((WebPartTypeEnum)WebPartType, PropertiesPosition.After);

                // Add 'General' category at the beginning if no one is specified
                wpProperties = FormHelper.EnsureDefaultCategory(wpProperties, ResHelper.GetString("general.general"));

                // Get merged web part FormInfo
                mFormInfo = PortalFormHelper.GetWebPartFormInfo(WebPartName, wpProperties, beforeFI, afterFI, true, WebPartDefaultValues);
            }

            return mFormInfo.Clone();
        }


        /// <summary>
        /// Returns true if the web part has the default configuration available
        /// </summary>
        public bool HasDefaultConfiguration()
        {
            return (DefaultConfiguration.GetWebPart(DEFAULT_CONFIG_WEBPARTNAME) != null);
        }


        /// <summary>
        /// Resets the default web part configuration to the original state
        /// </summary>
        public void ResetDefaultConfiguration()
        {
            RemoveDefaultConfiguration();

            if (WebPartParentID > 0)
            {
                // If web part is inherited, reset to the parent web part configuration
                var parent = WebPartInfoProvider.GetWebPartInfo(WebPartParentID);
                if (parent != null)
                {
                    DefaultConfiguration = parent.DefaultConfiguration.Clone();
                }
            }

            // Ensure empty if not loaded
            EnsureDefaultConfiguration();
        }


        /// <summary>
        /// Removes the default configuration from the web part
        /// </summary>
        public void RemoveDefaultConfiguration()
        {
            WebPartDefaultConfiguration = "";
            WebPartInfoProvider.SetWebPartInfo(this);
        }


        /// <summary>
        /// Ensures the default configuration within the web part
        /// </summary>
        public void EnsureDefaultConfiguration()
        {
            // Ensure that the web part is in the default configuration
            var config = DefaultConfiguration;

            var zone = config.EnsureZone(DEFAULT_CONFIG_ZONENAME);

            if (zone.GetWebPart(DEFAULT_CONFIG_WEBPARTNAME) == null)
            {
                var wp = zone.AddWebPart(WebPartID);
                // Load default properties
                var props = PortalFormHelper.GetDefaultWebPartProperties(this);
                wp.LoadProperties(props);

                wp.SetValue("WebPartTitle", WebPartDisplayName);
                wp.ControlID = DEFAULT_CONFIG_WEBPARTNAME;
                wp.InstanceGUID = DEFAULT_CONFIG_INSTANCEGUID;

                // Save the web part
                WebPartInfoProvider.SetWebPartInfo(this);
            }
        }


        /// <summary>
        /// Gets the virtual page template provided by this object
        /// </summary>
        public PageTemplateInfo GetVirtualPageTemplate()
        {
            // Prepare the page template
            var pt = new WebPartVirtualPageTemplateInfo(this);
            pt.TemplateInstance = DefaultConfiguration;
            pt.PageTemplateId = PageTemplateInfoProvider.WEBPART_TEMPLATE_STARTID + WebPartID;

            return pt;
        }


        /// <summary>
        /// Gets the theme path for the object
        /// </summary>
        public string GetThemePath()
        {
            return "~/App_Themes/Components/WebParts/" + ValidationHelper.GetSafeFileName(WebPartName);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Set special values
            bool copyAppThemes = false;
            bool copyFiles = false;
            bool copyLayouts = true;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                WebPartCategoryID = ValidationHelper.GetInteger(p["cms.webpart" + ".categoryid"], -1);
                if (WebPartParentID <= 0)
                {
                    WebPartFileName = ValidationHelper.GetString(p["cms.webpart" + ".filename"], "");
                }
                copyFiles = ValidationHelper.GetBoolean(p["cms.webpart" + ".files"], false);
                copyAppThemes = ValidationHelper.GetBoolean(p["cms.webpart" + ".appthemes"], false);
                copyLayouts = ValidationHelper.GetBoolean(p["cms.webpart" + ".layouts"], true);
            }

            if (copyLayouts)
            {
                if (settings.ExcludedChildTypes.Contains(WebPartLayoutInfo.OBJECT_TYPE))
                {
                    settings.ExcludedChildTypes.Remove(WebPartLayoutInfo.OBJECT_TYPE);
                }
            }
            else
            {
                settings.ExcludedChildTypes.Add(WebPartLayoutInfo.OBJECT_TYPE);
            }

            Insert();

            // Try to copy files first
            if (copyFiles)
            {
                // Get source file path
                string srcFile = WebPartInfoProvider.GetWebPartPhysicalPath(originalObject.GetStringValue("WebPartFileName", ""));

                if (File.Exists(srcFile))
                {
                    // Get destination file path
                    string dstFile = WebPartInfoProvider.GetWebPartPhysicalPath(WebPartFileName);
                    string dstRelative = WebPartFileName;

                    // Combine only if webpart don't start with '~'
                    if (!dstRelative.StartsWithCSafe("~/"))
                    {
                        dstRelative = Path.Combine(WebPartInfoProvider.WebPartsDirectory, Path.EnsureSlashes(WebPartFileName));
                    }

                    // Read .aspx file, replace classname and save as new file
                    FileHelper.CloneControlSource(srcFile, dstFile, dstRelative);

                    // Copy web part subfolder
                    string srcDirectory = srcFile.Remove(srcFile.Length - Path.GetFileName(srcFile).Length) + Path.GetFileNameWithoutExtension(srcFile) + "_files";
                    if (Directory.Exists(srcDirectory))
                    {
                        string dstDirectory = dstFile.Remove(dstFile.Length - Path.GetFileName(dstFile).Length) + Path.GetFileNameWithoutExtension(dstFile) + "_files";
                        if (srcDirectory.ToLowerCSafe() != dstDirectory.ToLowerCSafe())
                        {
                            DirectoryHelper.EnsureDiskPath(srcDirectory, SystemContext.WebApplicationPhysicalPath);
                            DirectoryHelper.CopyDirectory(srcDirectory, dstDirectory);
                        }
                    }
                }
            }

            if (copyAppThemes)
            {
                // Copy files from App_Themes
                string sourcePath = "~/App_Themes/Components/Webparts/" + originalObject.Generalized.ObjectCodeName;
                string targetPath = "~/App_Themes/Components/Webparts/" + ObjectCodeName;

                FileHelper.CopyDirectory(sourcePath, targetPath);
            }
        }


        /// <summary>
        /// Removes web part dependencies.
        /// </summary>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            // Get web part ID
            int webPartId = WebPartID;

            // Delete all inherited webparts
            WebPartInfoProvider.DeleteChildWebParts(webPartId);

            // Delete all layouts
            DataSet ds = WebPartLayoutInfoProvider.GetWebPartLayouts(webPartId);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Delete the layout
                    WebPartLayoutInfo li = new WebPartLayoutInfo(dr);
                    li.Generalized.LogSynchronization = SynchronizationTypeEnum.None;

                    WebPartLayoutInfoProvider.DeleteWebPartLayoutInfo(li);
                }
            }

            // Delete all depending widgets
            ds = WidgetInfoProvider.GetWidgets()
                .WhereEquals("WidgetWebPartID", webPartId)
                .Columns("WidgetID");

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Get widget info (cache)
                    WidgetInfo widget = WidgetInfoProvider.GetWidgetInfo(Convert.ToInt32(dr["WidgetID"]));

                    // Delete widget
                    WidgetInfoProvider.DeleteWidgetInfo(widget);
                }
            }

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor, creates an empty WebPartInfo structure.
        /// </summary>
        public WebPartInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an empty WebPartInfo object from the given DataRow data.
        /// </summary>
        /// <param name="dr">Datarow with the class info data</param>
        public WebPartInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }

        #endregion


        #region "Macro methods"

        /// <summary>
        /// Gets the list of object types that may use the web part
        /// </summary>
        [MacroMethod]
        public IEnumerable<string> GetUsageObjectTypes()
        {
            var objectTypes = new List<string>
            {
                OBJECT_TYPE,
                WidgetInfo.OBJECT_TYPE,
                PageTemplateInfo.OBJECT_TYPE,
            };

            if (ModuleEntryManager.IsModuleLoaded(ModuleName.MVTEST))
            {
                objectTypes.Add(PredefinedObjectType.MVTVARIANT);
            }

            if (ModuleEntryManager.IsModuleLoaded(ModuleName.CONTENTPERSONALIZATION))
            {
                objectTypes.Add(PredefinedObjectType.CONTENTPERSONALIZATIONVARIANT);
            }

            return objectTypes;
        }


        /// <summary>
        /// Gets the objects using the web part as a query with result columns ObjectType, ObjectID.
        /// </summary>
        [MacroMethod]
        public IDataQuery GetUsages()
        {
            var q = new MultiObjectQuery();

            // Find all inherited web parts made from current web part
            AddTypeToUsageQuery(q, OBJECT_TYPE, TypeInfo.IDColumn, GetWebPartChildCondition("WebPartParentID"));

            // Find all widgets made from current web part
            AddTypeToUsageQuery(q, WidgetInfo.OBJECT_TYPE, WidgetInfo.TYPEINFO.IDColumn, GetWebPartChildCondition("WidgetWebPartID"));

            var searchPattern = String.Format("type=\"{0}\"", WebPartName);

            // Find all page templates where current web part is used
            AddTypeToUsageQuery(q, PageTemplateInfo.OBJECT_TYPE, PageTemplateInfo.TYPEINFO.IDColumn, GetWebPartSearchCondition("PageTemplateWebParts", searchPattern));

            // Check if MVT feature is loaded
            if (ModuleEntryManager.IsModuleLoaded(ModuleName.MVTEST))
            {
                // Find web part in MVT variants
                AddTypeToUsageQuery(q, PredefinedObjectType.MVTVARIANT, "MVTVariantID", GetWebPartSearchCondition("MVTVariantWebParts", searchPattern));
            }

            // Check if Content personalization feature is loaded
            if (ModuleEntryManager.IsModuleLoaded(ModuleName.CONTENTPERSONALIZATION))
            {
                // Find web part in Content personalization variants
                AddTypeToUsageQuery(q, PredefinedObjectType.CONTENTPERSONALIZATIONVARIANT, "VariantID", GetWebPartSearchCondition("VariantWebParts", searchPattern));
            }

            // Order by source type by default to keep usages from various types grouped
            q.DefaultOrderByType = true;

            // Data is generally inconsistent, so any global condition should be use on the results
            q.UseGlobalWhereOnResult = true;

            return q;
        }

        #endregion


        #region "Private helper methods"

        /// <summary>
        /// Creates where condition to find current web part as a parent (usable for inherited web parts and widgets)
        /// </summary>
        /// <param name="columnName">Parent ID column name</param>
        private WhereCondition GetWebPartChildCondition(string columnName)
        {
            return new WhereCondition().WhereEquals(columnName, WebPartID);
        }


        /// <summary>
        /// Creates where condition to find current web part in form definition (usable for page template, MVT variants and CP variants)
        /// </summary>
        /// <param name="columnName">Web part definition column name</param>
        /// <param name="searchPattern">Search pattern</param>
        private WhereCondition GetWebPartSearchCondition(string columnName, string searchPattern)
        {
            return new WhereCondition().WhereContains(columnName, searchPattern);
        }


        /// <summary>
        /// Adds type to query to get all usages of current web part
        /// </summary>
        /// <param name="query">Multi object query</param>
        /// <param name="objectType">Object type to add to search in</param>
        /// <param name="idColumn">ID column name of the object</param>
        /// <param name="condition">Condition how to find the current web part</param>
        private void AddTypeToUsageQuery(MultiObjectQuery query, string objectType, string idColumn, IWhereCondition condition)
        {
            query.Type(objectType, t => t
                                    .Columns
                                    (
                                        objectType.AsValue(true).AsColumn("ObjectType"),
                                        new QueryColumn(idColumn).As("ObjectID")
                                    )
                                    .Where(condition)
                );
        }

        #endregion
    }
}
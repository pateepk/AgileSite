using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml;

using CMS;
using CMS.Core;
using CMS.DataEngine;
using CMS.DataEngine.CollectionExtensions;
using CMS.Helpers;
using CMS.IO;

[assembly: RegisterObjectType(typeof(DataClassInfo), DataClassInfo.OBJECT_TYPE)]
[assembly: RegisterObjectType(typeof(DataClassInfo), DataClassInfo.OBJECT_TYPE_SYSTEMTABLE)]

namespace CMS.DataEngine
{
    /// <summary>
    /// Describes Data Class configuration.
    /// </summary>
    [Serializable]
    public class DataClassInfo : DataClassInfoBase<DataClassInfo>
    {
        #region "Type information properties"

        /// <summary>
        /// Main object type to cover all class types
        /// </summary>
        public const string OBJECT_TYPE = "cms.class";

        /// <summary>
        /// Object type for system table (customizable module class)
        /// </summary>
        public const string OBJECT_TYPE_SYSTEMTABLE = "cms.systemtable";


        /// <summary>
        /// Columns excluded from staging
        /// </summary>
        private static readonly string[] ExcludedStagingColumns =
        {
            "ClassID",
            "ClassDisplayName",
            "ClassName",
            "ClassUsesVersioning",
            "ClassIsDocumentType",
            "ClassIsCoupledClass",
            "ClassEditingPageUrl",
            "ClassListPageUrl",
            "ClassNodeNameSource",
            "ClassTableName",
            "ClassViewPageUrl",
            "ClassPreviewPageUrl",
            "ClassNewPageUrl",
            "ClassShowAsSystemTable",
            "ClassUsePublishFromTo",
            "ClassShowTemplateSelection",
            "ClassIsMenuItemType",
            "ClassNodeAliasSource",
            "ClassDefaultPageTemplateID",
            "ClassLastModified",
            "ClassGUID",
            "ClassCreateSKU",
            "ClassIsProduct",
            "ClassIsCustomTable",
            "ClassShowColumns",
            "ClassInheritsFromClassID",
            "ClassSKUDefaultDepartmentName",
            "ClassSKUDefaultDepartmentID",
            "ClassContactOverwriteEnabled",
            "ClassSKUDefaultProductType",
            "ClassConnectionString",
            "ClassIsProductSection",
            "ClassPageTemplateCategoryID",
            "ClassVersionGUID",
            "ClassDefaultObjectType",
            "ClassIsForm",
            "ClassResourceID",
            "ClassCodeGenerationSettings",
            "ClassIconClass",
            "ClassIsContentOnly",
            "ClassURLPattern"
        };


        /// <summary>
        /// Type information for class covering all classes
        /// </summary>
        public static ObjectTypeInfo TYPEINFO = new ObjectTypeInfo(typeof(DataClassInfoProvider), OBJECT_TYPE, "CMS.Class", "ClassID", "ClassLastModified", "ClassGUID", "ClassName", "ClassDisplayName", null, null, null, null)
        {
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ClassResourceID", PredefinedObjectType.RESOURCE) },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            HasExternalColumns = true,
            DeleteObjectWithAPI = true,
            VersionGUIDColumn = "ClassVersionGUID",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            CustomizedColumnsColumn = "ClassCustomizedColumns",
            ResourceIDColumn = "ClassResourceID",
            FormDefinitionColumn = "ClassFormDefinition",
            DefaultData = new DefaultDataSettings
            {
                // Export all default classes through the general class type so that they end up in the same default data file
                Where = "((ClassName LIKE 'cms.%' OR ClassName LIKE 'newsletter.%' OR ClassName LIKE 'forums.%' OR ClassName LIKE 'ecommerce.%' OR ClassName LIKE 'staging.%' OR ClassName LIKE 'polls.%' OR ClassName LIKE 'reporting.%' OR ClassName LIKE 'analytics.%' OR ClassName LIKE 'blog.%' OR ClassName LIKE 'export.%' OR ClassName LIKE 'board.%' OR ClassName LIKE 'badwords.%' OR ClassName LIKE 'notification.%' OR ClassName LIKE 'community.%' OR ClassName LIKE 'media.%' OR ClassName LIKE 'om.%' OR ClassName IN ('temp.file', 'temp.pagebuilderwidgets') OR ClassName LIKE 'pm.%' OR ClassName LIKE 'integration.%' OR ClassName LIKE 'chat.%' OR ClassName LIKE 'sm.%' OR ClassName LIKE 'sharepoint.%' OR ClassName LIKE 'personas.%' OR ClassName LIKE 'ci.%') AND ClassIsDocumentType=0) OR ((ClassName = 'cms.root' OR ClassName='cms.file' OR ClassName='chat.transformations') AND ClassIsDocumentType=1)",
                ExcludedColumns = new List<string> { "ClassCodeGenerationSettings", "ClassCustomizedColumns" }
            },
            HasMetaFiles = true,
            ImportExportSettings =
            {
                IsExportable = true,
                IsAutomaticallySelected = true,
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                WhereCondition = "ClassResourceID IS NOT NULL AND ClassIsDocumentType = 0" // Restrict export only to module classes, this type overlaps several class object types. Page types are handled by separate type info
            },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
                LogCondition = CanSynchronizeClass,
                ExcludedStagingColumns = new List<string>(ExcludedStagingColumns.Union(new [] { "ClassXmlSchema", "ClassFormDefinition" }))
            },
            SerializationSettings =
            {
                StructuredFields = new List<IStructuredField>
                {
                    new StructuredField<DataDefinition>("ClassFormDefinition"),
                    new StructuredField<SearchSettings>("ClassSearchSettings"),
                    new StructuredField("ClassContactMapping"),
                    new StructuredField<ClassCodeGenerationSettings>("ClassCodeGenerationSettings")
                },
                ExcludedFieldNames =
                {
                    "ClassXmlSchema"
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                FilterDependencies =
                {
                     new ObjectReference("ClassResourceID", PredefinedObjectType.RESOURCE)
                }
            }
        };


        /// <summary>
        /// Type information for system table (customizable module class)
        /// </summary>
        public static ObjectTypeInfo TYPEINFOSYSTEMTABLE = new ObjectTypeInfo(typeof(DataClassInfoProvider), OBJECT_TYPE_SYSTEMTABLE, "CMS.Class", "ClassID", "ClassLastModified", "ClassGUID", "ClassName", "ClassDisplayName", null, null, null, null)
        {
            OriginalTypeInfo = TYPEINFO,
            MacroCollectionName = "CMS.SystemTable",
            DependsOn = new List<ObjectDependency> { new ObjectDependency("ClassResourceID", PredefinedObjectType.RESOURCE, ObjectDependencyEnum.Required) },
            SynchronizationSettings =
            {
                LogSynchronization = SynchronizationTypeEnum.LogSynchronization,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
                LogCondition = CanSynchronizeClass,
                ExcludedStagingColumns = new List<string>(ExcludedStagingColumns)
            },
            LogEvents = true,
            TouchCacheDependencies = true,
            SupportsVersioning = false,
            SupportsCloning = false,
            ImportExportSettings =
            {
                LogExport = true,
                IsExportable = true,
                IncludeToWebTemplateExport = ObjectRangeEnum.None,
                ObjectTreeLocations = new List<ObjectTreeLocation>
                {
                    new ObjectTreeLocation(GLOBAL, DEVELOPMENT),
                },
            },
            DeleteObjectWithAPI = true,
            VersionGUIDColumn = "ClassVersionGUID",
            CodeColumn = EXTERNAL_COLUMN_CODE,
            CustomizedColumnsColumn = "ClassCustomizedColumns",
            ResourceIDColumn = "ClassResourceID",
            FormDefinitionColumn = "ClassFormDefinition",
            TypeCondition = new TypeCondition().WhereEquals("ClassShowAsSystemTable", true),
            HasExternalColumns = true,
            HasMetaFiles = true,
            SerializationSettings =
            {
                StructuredFields = new IStructuredField[]
                {
                    new StructuredField<DataDefinition>("ClassFormDefinition"),
                    new StructuredField<SearchSettings>("ClassSearchSettings"),
                    new StructuredField("ClassContactMapping"),
                    new StructuredField<ClassCodeGenerationSettings>("ClassCodeGenerationSettings")
                },
                ExcludedFieldNames =
                {
                    "ClassXmlSchema"
                }
            },
            ContinuousIntegrationSettings =
            {
                Enabled = true,
                FilterDependencies =
                {
                     new ObjectReference("ClassResourceID", PredefinedObjectType.RESOURCE)
                }
            }
        };

        #endregion


        #region "Variables"

        private SearchSettings mClassSearchSettingsInfos;

        private ClassCodeGenerationSettings mClassCodeGenerationSettingsInfo;

        private IDictionary<string, Type> mClassSearchColumnsTypes;

        /// <summary>
        /// Source schema dataset.
        /// </summary>
        protected DataSet mSchemaDataSet;


        /// <summary>
        ///  External column name for Form Layout
        /// </summary>
        public const string EXTERNAL_COLUMN_CODE = "ClassFormLayout";

        #endregion


        #region "Properties"

        /// <summary>
        /// Dictionary containing <see cref="Type"/>s of search columns that belongs to class represented by this <see cref="DataClassInfo"/>.
        /// </summary>
        /// <seealso cref="GetSearchColumnType(string)"/>
        internal IDictionary<string, Type> SearchColumnsTypes
        {
            get
            {
                if (mClassSearchColumnsTypes == null)
                {
                    mClassSearchColumnsTypes = GetSearchIndexColumns().ToDictionary(col => col.ColumnName, col => col.ColumnType, StringComparer.OrdinalIgnoreCase);
                }

                return mClassSearchColumnsTypes;
            }
            set
            {
                mClassSearchColumnsTypes = value;
            }
        }


        /// <summary>
        /// Class xml schema
        /// </summary>
        public override string ClassXmlSchema
        {
            get
            {
                return base.ClassXmlSchema;
            }
            set
            {
                base.ClassXmlSchema = value;

                mSchemaDataSet = null;
            }
        }


        /// <summary>
        /// Class search settings
        /// </summary>
        public override string ClassSearchSettings
        {
            get
            {
                return base.ClassSearchSettings;
            }
            set
            {
                base.ClassSearchSettings = value;

                mClassSearchSettingsInfos = null;
                mClassSearchColumnsTypes = null;
            }
        }


        /// <summary>
        /// Class code generation settings
        /// </summary>
        public override string ClassCodeGenerationSettings
        {
            get
            {
                return base.ClassCodeGenerationSettings;
            }
            set
            {
                base.ClassCodeGenerationSettings = value;

                mClassCodeGenerationSettingsInfo = null;
            }
        }


        /// <summary>
        /// Class form layout type
        /// </summary>
        [DatabaseField]
        public new LayoutTypeEnum ClassFormLayoutType
        {
            get
            {
                return LayoutHelper.GetLayoutTypeEnum(base.ClassFormLayoutType);
            }
            set
            {
                base.ClassFormLayoutType = value.ToString();
            }
        }


        /// <summary>
        /// Schema dataset.
        /// </summary>
        private DataSet SchemaDataSet
        {
            get
            {
                return mSchemaDataSet ?? (mSchemaDataSet = CreateSchemaDataSet());
            }
        }


        /// <summary>
        /// Gets the SearchSettings infos object.
        /// </summary>
        public SearchSettings ClassSearchSettingsInfos
        {
            get
            {
                if (mClassSearchSettingsInfos == null)
                {
                    SearchSettings ss = new SearchSettings();
                    ss.LoadData(ClassSearchSettings);
                    mClassSearchSettingsInfos = ss;
                }

                return mClassSearchSettingsInfos;
            }
        }


        /// <summary>
        /// Gets or sets the code generation settings.
        /// </summary>
        public ClassCodeGenerationSettings ClassCodeGenerationSettingsInfo
        {
            get
            {
                if (mClassCodeGenerationSettingsInfo == null)
                {
                    mClassCodeGenerationSettingsInfo = new ClassCodeGenerationSettings(ClassCodeGenerationSettings);
                    PreselectColumns();
                }

                return mClassCodeGenerationSettingsInfo;
            }
            set
            {
                ClassCodeGenerationSettings = value?.ToString();
                mClassCodeGenerationSettingsInfo = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor - Creates an empty ClassInfo object.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use method DataClassInfo.New. For inheritance, use DataClassInfo(dummy).")]
        public DataClassInfo()
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor, creates an empty DataClassInfo structure.
        /// </summary>
        protected DataClassInfo(bool dummy)
            : base(TYPEINFO)
        {
        }


        /// <summary>
        /// Constructor - Creates a new ClassInfo object from the given DataRow.
        /// </summary>
        /// <param name="dr">DataRow with the object data</param>
        public DataClassInfo(DataRow dr)
            : base(TYPEINFO, dr)
        {
        }


        /// <summary>
        /// Constructor for de-serialization.
        /// </summary>
        /// <param name="info">Serialization info</param>
        /// <param name="context">Streaming context</param>
        public DataClassInfo(SerializationInfo info, StreamingContext context)
            : base(info, context, TYPEINFO, TYPEINFOSYSTEMTABLE)
        {
        }

        #endregion


        #region "Type based properties and methods"

        /// <summary>
        /// Type information.
        /// </summary>
        public override ObjectTypeInfo TypeInfo
        {
            get
            {
                if (ClassShowAsSystemTable)
                {
                    return TYPEINFOSYSTEMTABLE;
                }

                return TYPEINFO;
            }
        }


        /// <summary>
        /// Gets the child object where condition.
        /// </summary>
        /// <param name="where">Original where condition</param>
        /// <param name="objectType">Object type of the child object</param>
        protected override WhereCondition GetChildWhereCondition(WhereCondition where, string objectType)
        {
            var w = base.GetChildWhereCondition(where, objectType);

            // ## Special case - get only custom queries
            if (objectType == QueryInfo.OBJECT_TYPE)
            {
                w.WhereTrue("QueryIsCustom");
            }

            return w;
        }


        /// <summary>
        /// Gets the data query for this object type
        /// </summary>
        protected override IDataQuery GetDataQueryInternal()
        {
            return DataClassInfoProvider.GetClasses();
        }


        /// <summary>
        /// Deletes the object using appropriate provider.
        /// </summary>
        protected override void DeleteObject()
        {
            DataClassInfoProvider.DeleteDataClassInfo(this);
        }


        /// <summary>
        /// Updates the object using appropriate provider.
        /// </summary>
        protected override void SetObject()
        {
            DataClassInfoProvider.SetDataClassInfo(this);
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns all columns along with their types that should be included in the search index for current <see cref="DataClassInfo"/>.
        /// </summary>
        /// <seealso cref="GetSearchColumnType(string)"/>
        public IEnumerable<ColumnDefinition> GetSearchIndexColumns()
        {
            var result = new List<ColumnDefinition>(new ClassStructureInfo(ClassName, ClassXmlSchema, ClassTableName).ColumnDefinitions);

            switch (ClassName.ToLowerInvariant())
            {
                case PredefinedObjectType.DOCUMENT:
                    var ecom = DataClassInfoProvider.GetDataClassInfo(PredefinedObjectType.SKU);
                    var tree = DataClassInfoProvider.GetDataClassInfo("cms.tree");

                    result.InsertRange(0, new ClassStructureInfo(ecom.ClassName, ecom.ClassXmlSchema, ecom.ClassTableName).ColumnDefinitions);
                    result.AddRange(new ClassStructureInfo(tree.ClassName, tree.ClassXmlSchema, tree.ClassTableName).ColumnDefinitions);
                    break;

                case PredefinedObjectType.USER:
                    var userSettings = DataClassInfoProvider.GetDataClassInfo(PredefinedObjectType.USERSETTINGS);
                    result.AddRange(new ClassStructureInfo(userSettings.ClassName, userSettings.ClassXmlSchema, userSettings.ClassTableName).ColumnDefinitions);
                    break;
            }

            return result;
        }


        /// <summary>
        /// Returns the existing object based on current object data.
        /// </summary>
        protected override BaseInfo GetExisting()
        {
            var where = GetExistingWhereCondition();

            // Get the data, DataClassInfo may change its object type, therefore it needs to get existing object without checking the type condition
            return GetExistingBase(where, false);
        }


        /// <summary>
        /// Gets the list of synchronized columns for this object.
        /// </summary>
        /// <param name="excludeColumns">When true values is passed, columns from <see cref="SynchronizationSettings.ExcludedStagingColumns"/> are removed</param>
        protected sealed override IEnumerable<string> GetSynchronizedColumns(bool excludeColumns = true)
        {
            excludeColumns = excludeColumns & !IsFullySynchronized();
            return base.GetSynchronizedColumns(excludeColumns);
        }


        /// <summary>
        /// Returns true if the class is fully synchronized with all its data and fields including the system fields
        /// </summary>
        private bool IsFullySynchronized()
        {
            // Non-module classes are always fully synchronized
            var resourceId = ClassResourceID;
            if (resourceId <= 0)
            {
                return true;
            }

            // Exception for classes in module "Custom" - let all columns to be synchronized
            var ri = GetModule(resourceId);
            if ((ri != null) && IsCustomModule(ri))
            {
                return true;
            }

            return false;
        }


        /// <summary>
        /// Returns true if the given class object can be synchronized
        /// </summary>
        /// <param name="classObj">Class object</param>
        private static bool CanSynchronizeClass(BaseInfo classObj)
        {
            var dci = (DataClassInfo)classObj;

            // For module classes only classes of modules that aren't under development can be synchronized
            var resourceId = dci.ClassResourceID;
            if (resourceId > 0)
            {
                var ri = GetModule(resourceId);
                if (ri != null)
                {
                    // Custom module classes are ad-hoc and can be always synchronized
                    if (IsCustomModule(ri))
                    {
                        return true;
                    }

                    // Other modules can be only synchronized if they are installed modules (not under development)
                    return !ValidationHelper.GetBoolean(ri.GetValue("ResourceIsInDevelopment"), false);
                }

                // Module not found, do not synchronize
                return false;
            }

            return true;
        }


        private static BaseInfo GetModule(int resourceId)
        {
            return ProviderHelper.GetInfoById(PredefinedObjectType.RESOURCE, resourceId);
        }


        private static bool IsCustomModule(BaseInfo ri)
        {
            // Custom module classes are ad-hoc and can be always synchronized
            var resourceName = ValidationHelper.GetString(ri.GetValue("ResourceName"), "");

            return resourceName.Equals(ModuleName.CUSTOMSYSTEM, StringComparison.OrdinalIgnoreCase);
        }


        /// <summary>
        /// Returns list of columns used during object cloning.
        /// </summary>
        private static QueryColumnList GetClonedColumns(string origClassName)
        {
            var structure = ClassStructureInfo.GetClassInfo(origClassName);
            var columns = structure.ColumnNames.Where(c => !c.Equals(structure.IDColumn, StringComparison.OrdinalIgnoreCase)).Select(c => new QueryColumn(c, true));

            var columnList = new QueryColumnList();
            columnList.AddRange(columns);

            return columnList;
        }


        private static string EscapeTableNameForCloning(string classTableName)
        {
            return classTableName.Replace("[", "[[");
        }


        /// <summary>
        /// Loads the default data to the object.
        /// </summary>
        protected override void LoadDefaultData()
        {
            base.LoadDefaultData();

            ClassUsesVersioning = false;
            ClassIsDocumentType = false;
            ClassIsCoupledClass = true;
            ClassNodeNameSource = "";
            ClassIsForm = false;
            ClassShowAsSystemTable = false;
            ClassIsCustomTable = false;
            ClassXmlSchema = "";
            ClassFormDefinition = "";
        }


        /// <summary>
        /// Returns an empty DataSet created by the class schema.
        /// </summary>
        public DataSet GetDataSet()
        {
            // Return the clone of current DataSet
            return SchemaDataSet.Clone();
        }


        /// <summary>
        /// Returns class xml schema based on the class connection string and class table name.
        /// </summary>
        public string GetClassXmlSchema()
        {
            return new TableManager(ClassConnectionString).GetXmlSchema(ClassTableName);
        }


        /// <summary>
        /// Inserts cloned object to DB.
        /// </summary>
        /// <param name="settings">Cloning settings</param>
        /// <param name="result">Cloning result</param>
        /// <param name="originalObject">Original source BaseInfo (object being cloned)</param>
        protected internal override void InsertAsCloneInternal(CloneSettings settings, CloneResult result, BaseInfo originalObject)
        {
            // Creates new table with or without data
            bool copyData = false;
            bool alternativeForms = true;
            string iconsToCopy = null;
            string tableName = null;

            Hashtable p = settings.CustomParameters;
            if (p != null)
            {
                tableName = ValidationHelper.GetString(p["cms.class" + ".tablename"], "");
                copyData = ValidationHelper.GetBoolean(p["cms.class" + ".data"], false);
                iconsToCopy = ValidationHelper.GetString(p["cms.documenttype" + ".icons"], "");
                alternativeForms = ValidationHelper.GetBoolean(p["cms.class" + ".alternativeforms"], true);
            }

            bool isContainer = string.IsNullOrEmpty(tableName);
            string origClassName = originalObject.GetStringValue("ClassName", "");
            string originalTableName = ClassTableName;

            // Clone DB table
            if (!isContainer)
            {
                var tm = new TableManager(ClassConnectionString);
                var cloneTableName = string.IsNullOrEmpty(tableName) ? tm.GetUniqueTableName(ClassTableName) : tableName;
                ClassTableName = cloneTableName;
            }

            string bizFormPrefix = "bizform.";

            // Preserve the prefix
            if (origClassName.StartsWith(bizFormPrefix, StringComparison.OrdinalIgnoreCase) && !ClassName.StartsWith(bizFormPrefix, StringComparison.OrdinalIgnoreCase))
            {
                ClassName = bizFormPrefix + ClassName;
            }

            // Insert main object
            Insert();

            // Handle alternative forms
            if (!isContainer)
            {
                if (copyData)
                {
                    var columns = GetClonedColumns(origClassName);
                    var insertSelectQuery = $"INSERT INTO [{EscapeTableNameForCloning(ClassTableName)}] ({columns.Columns}) SELECT {columns.Columns} FROM [{EscapeTableNameForCloning(originalTableName)}]";

                    DataConnectionFactory.GetConnection().ExecuteQuery(insertSelectQuery, null, QueryTypeEnum.SQLQuery, false);
                }

                if (alternativeForms)
                {
                    if (settings.ExcludedChildTypes.Contains(PredefinedObjectType.ALTERNATIVEFORM))
                    {
                        settings.ExcludedChildTypes.Remove(PredefinedObjectType.ALTERNATIVEFORM);
                    }
                }
                else
                {
                    settings.ExcludedChildTypes.Add(PredefinedObjectType.ALTERNATIVEFORM);
                }
            }

            // Copy document type icons
            if (!string.IsNullOrEmpty(iconsToCopy))
            {
                string originalFileName = TranslationHelper.GetSafeClassName(originalObject.GetStringValue("ClassName", ""));
                string newFileName = TranslationHelper.GetSafeClassName(ClassName);
                string[] icons = iconsToCopy.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string icon in icons)
                {
                    if (File.Exists(icon))
                    {
                        File.Copy(icon, icon.Replace(originalFileName, newFileName));
                    }
                }
            }

            // If there was at least one query cloned, save warning to result
            if ((result != null) && settings.IncludeChildren && ((settings.ExcludedChildTypes == null) || (!settings.ExcludedChildTypes.Contains(QueryInfo.OBJECT_TYPE))))
            {
                var col = Children?[QueryInfo.OBJECT_TYPE];
                if (col != null && (col.Count > 0))
                {
                    result.Warnings.Add("{$cloning.warning.queries$}");
                }
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
            string extension = (ClassFormLayoutType == LayoutTypeEnum.Html) ? ".html" : ".ascx";

            // Keep original version GUID
            string originalVersionGuid = versionGuid;
            bool storedExternally = (DataClassInfoProvider.StoreFormLayoutsInExternalStorage || SettingsKeyInfoProvider.DeploymentMode);

            // Do not use version GUID for files stored externally
            if (storedExternally)
            {
                versionGuid = null;
            }

            string directory = DataClassInfoProvider.FormLayoutsDirectory;

            // If file should be in FS but wasn't found, use DB version
            string path = VirtualPathHelper.GetVirtualFileRelativePath(ClassName, extension, directory, null, versionGuid);

            if (!SettingsKeyInfoProvider.DeploymentMode && DataClassInfoProvider.StoreFormLayoutsInExternalStorage && !FileHelper.FileExists(path))
            {
                path = VirtualPathHelper.GetVirtualFileRelativePath(ClassName, extension, directory, null, originalVersionGuid);
            }

            return path;
        }


        /// <summary>
        /// Creates the class data set based on the class schema
        /// </summary>
        private DataSet CreateSchemaDataSet()
        {
            DataSet ds = new DataSet();

            // Load the schema
            StringReader sr = new StringReader(ClassXmlSchema);
            XmlReader xml = XmlReader.Create(sr);

            ds.ReadXmlSchema(xml);

            // Set all the columns to allow nulls
            DataTable dt = ds.Tables[0];
            for (int i = 0; i < dt.Columns.Count; i++)
            {
                dt.Columns[i].AllowDBNull = true;
                dt.Columns[i].ReadOnly = false;
            }

            return ds;
        }


        /// <summary>
        /// Returns path to externally stored layout codes.
        /// </summary>
        protected override void RegisterExternalColumns()
        {
            base.RegisterExternalColumns();

            // Code
            var settings = new ExternalColumnSettings<DataClassInfo>
            {
                StoragePath = m => m.GetVirtualFileRelativePath(EXTERNAL_COLUMN_CODE, null),
                StoreInExternalStorageSettingsKey = "CMSStoreFormLayoutsInFS",
                SetDataTransformation = (m, data, readOnly) => LayoutHelper.AddLayoutDirectives(ValidationHelper.GetString(data, String.Empty), m.ClassFormLayoutType),
                GetDataTransformation = (m, data) => VirtualPathHelper.RemoveDirectives(ValidationHelper.GetString(data, String.Empty), LayoutHelper.DefaultDirectives)
            };

            RegisterExternalColumn(EXTERNAL_COLUMN_CODE, settings);
        }


        /// <summary>
        /// Returns the default object installation data
        /// </summary>
        /// <param name="excludedNames">Objects with display names and code names starting with these expressions are filtered out.</param>
        protected override DataSet GetDefaultData(IEnumerable<string> excludedNames = null)
        {
            var data = base.GetDefaultData(excludedNames);

            if (!DataHelper.DataSourceIsEmpty(data))
            {
                var customizedColumnsColumn = TypeInfo.CustomizedColumnsColumn;

                // Set ClassCustomizedColumns column to its correct default value instead of NULL (i.e. "")
                var dt = data.Tables[0];
                DataHelper.EnsureColumn(dt, customizedColumnsColumn, typeof(string));
                DataHelper.SetColumnValues(dt, customizedColumnsColumn, "");
            }

            return data;
        }

        /// <summary>
        /// Removes search settings from the class which does not correspond to any fields in the class.
        /// </summary>
        public void RemoveObsoleteSearchSettings()
        {
            var settings = ClassSearchSettingsInfos;

            // Get Class column names
            var classFields = GetSearchIndexColumns();
            if (classFields != null && settings != null)
            {
                var fieldList = classFields.Select(column => column.ColumnName)
                                           .ToHashSetCollection(StringComparer.InvariantCultureIgnoreCase);

                var resultSettings = new SearchSettings();
                settings.CopyTo(resultSettings, item => fieldList.Contains(item.Name));

                // Return filtered settings
                ClassSearchSettings = resultSettings.GetData();
            }
        }


        /// <summary>
        /// Returns <see cref="Type"/> for the given <paramref name="columnName"/> that is included in search index for current <see cref="DataClassInfo"/>.
        /// </summary>
        /// <param name="columnName">Name of the column for which <see cref="Type"/> is returned.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="columnName"/> is null or white-space.</exception>
        /// <exception cref="InvalidOperationException">Thrown if there is no type defined for given <paramref name="columnName"/> or <paramref name="columnName"/> does not exists in current <see cref="DataClassInfo"/>.</exception>
        /// <seealso cref="RemoveObsoleteSearchSettings"/>
        public Type GetSearchColumnType(string columnName)
        {
            if (String.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException($"{nameof(columnName)} can not be null, empty or consists only of white-space characters.");
            }

            Type type;
            if (!SearchColumnsTypes.TryGetValue(columnName, out type))
            {
                throw new InvalidOperationException($"Type can not be determined for column name '{columnName}' in data class '{ClassName}'. This typically occurs if search settings are not up to date.");
            }

            return type;
        }

        #endregion


        #region "Private methods"

        /// <summary>
        /// Preselects the special columns according to the suffixes of the column name.
        /// (i.e. GUIDColumn will be the first column with GUID suffix, etc.)
        /// </summary>
        private void PreselectColumns()
        {
            // Parse the form definition
            var def = new DataDefinition(ClassFormDefinition);

            // Return list of names
            var fields = def.GetFields<FieldInfo>();

            var helper = new ClassCodeGenerationSettingsHelper();
            helper.PreFillSettings(fields, ref mClassCodeGenerationSettingsInfo);
        }


        /// <summary>
        /// Removes object dependencies. First tries to execute removedependencies query, if not found, automatic process is executed.
        /// </summary>
        /// <param name="deleteAll">If false, only required dependencies are deleted, dependencies with default value are replaced with default value and nullable values are replaced with null</param>
        /// <param name="clearHashtables">If true, hashtables of all objecttypes which were potentially modified are cleared</param>
        protected override void RemoveObjectDependencies(bool deleteAll = false, bool clearHashtables = true)
        {
            DeleteClassTable();

            base.RemoveObjectDependencies(deleteAll, clearHashtables);
        }


        /// <summary>
        /// Deletes the database table
        /// </summary>
        private void DeleteClassTable()
        {
            string tableName = ClassTableName;
            if (tableName != "")
            {
                int pos = tableName.IndexOf(".", StringComparison.Ordinal);
                if (pos >= 0)
                {
                    tableName = tableName.Substring(pos + 1);
                }

                // Check if table exists
                TableManager tm = new TableManager(ClassConnectionString);
                if (tm.TableExists(tableName))
                {
                    // Deletes table of the Data Class
                    tm.DropTable(tableName);
                }
            }
        }

        #endregion
    }
}
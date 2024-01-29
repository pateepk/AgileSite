using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.Serialization;

using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object manipulation methods.
    /// </summary>
    public class ObjectHelper : CoreMethods
    {
        #region "Constants"

        /// <summary>
        /// Default binary data type.
        /// </summary>
        public const string BINARY_DATA_DEFAULT = "default";


        /// <summary>
        /// Thumbnail data type.
        /// </summary>
        public const string BINARY_DATA_PREVIEW = "preview";

        #endregion


        #region "Group type constants"

        /// <summary>
        /// Site object types - Special constant.
        /// </summary>
        public const string GROUP_SITE = "##SITE##";

        /// <summary>
        /// Document object type - Special constant.
        /// </summary>
        public const string GROUP_DOCUMENTS = PredefinedObjectType.GROUP_DOCUMENTS;

        /// <summary>
        /// All objects object type - Group constant.
        /// </summary>
        public const string GROUP_OBJECTS = PredefinedObjectType.GROUP_OBJECTS;

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets the table of specified info object from the given DataSet using the serialization table name.
        /// </summary>
        /// <param name="ds">Source data set</param>
        /// <param name="infoObj">Info object</param>
        /// <exception cref="DataClassNotFoundException">Thrown when the data class of the given object is not found.</exception>
        public static DataTable GetTable(DataSet ds, GeneralizedInfo infoObj)
        {
            if (ds == null)
            {
                return null;
            }

            var tableName = GetSerializationTableName(infoObj);

            // Try to find by the specific table name
            return FindTable(ds, tableName);
        }


        /// <summary>
        /// Finds the table with the given name in data set. Search is case insensitive, but exact case match has priority.
        /// </summary>
        /// <param name="ds">Source data set</param>
        /// <param name="tableName">Table name to find</param>
        private static DataTable FindTable(DataSet ds, string tableName)
        {
            // Try to search case sensitive first
            var dt = ds.Tables[tableName];
            if (dt != null)
            {
                return dt;
            }

            // Search by name case insensitive
            return ds.Tables.Cast<DataTable>().FirstOrDefault(table => table.TableName.EqualsCSafe(tableName, true));
        }


        /// <summary>
        /// Gets the table name of specified info object used for serialized data.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <exception cref="DataClassNotFoundException">Thrown when the data class of the given object is not found.</exception>
        public static string GetSerializationTableName(GeneralizedInfo infoObj)
        {
            if (infoObj == null)
            {
                return null;
            }

            // If class name not specified, no table name
            var ti = infoObj.TypeInfo;
            if (ti.ObjectClassName == ObjectTypeInfo.VALUE_UNKNOWN)
            {
                return null;
            }

            // Get the table name from class data
            var ci = DataClassInfoProviderBase<DataClassInfoProvider>.GetDataClassInfo(ti.ObjectClassName);
            if (ci == null)
            {
                throw new DataClassNotFoundException("[DataClassInfoProvider.GetTableName]: Class name '" + ti.ObjectClassName + "' for object type '" + ti.ObjectType + "' not found. ", ti.ObjectClassName, ti.ObjectType);
            }

            return ci.ClassTableName;
        }


        /// <summary>
        /// Returns empty objects DataSet based on the given object type.
        /// </summary>
        /// <param name="operation">Operation type</param>
        /// <param name="infoObj">Info object</param>
        /// <param name="childData">If true, child datasets are included</param>
        public static DataSet GetObjectsDataSet(OperationTypeEnum operation, GeneralizedInfo infoObj, bool childData)
        {
            if (infoObj == null)
            {
                return null;
            }

            var ti = infoObj.TypeInfo;

            // Get the object DataSet
            var ds = DataClassInfoProvider.GetDataSet(ti.ObjectClassName);
            if (ds.Tables.Count > 0)
            {
                ds.Tables[0].TableName = GetSerializationTableName(infoObj);
            }

            if (childData)
            {
                // Add child datasets
                foreach (string childType in ti.ChildObjectTypes)
                {
                    if (childType != infoObj.TypeInfo.ObjectType)
                    {
                        // Get the child object type
                        var childInfo = ModuleManager.GetReadOnlyObject(childType);
                        if (childInfo == null)
                        {
                            // Skip missing object type
                            continue;
                        }

                        if (childInfo.TypeInfo.IncludeToParentDataSet(operation) != IncludeToParentEnum.None)
                        {
                            // Get the child objects
                            DataSet childDS = GetObjectsDataSet(operation, childInfo, true);

                            // Add child data tables to the result
                            DataHelper.TransferTables(ds, childDS);
                        }
                    }
                }

                // Add binding data
                foreach (string bindingType in ti.BindingObjectTypes)
                {
                    if (bindingType != ti.ObjectType)
                    {
                        // Get the child object type
                        var bindingInfo = ModuleManager.GetReadOnlyObject(bindingType);
                        if (bindingInfo == null)
                        {
                            // Skip missing object type
                            continue;
                        }

                        // Skip the site bindings
                        if (bindingInfo.TypeInfo.IncludeToParentDataSet(operation) != IncludeToParentEnum.None)
                        {
                            // Get the binding objects
                            DataSet bindingsDS = GetObjectsDataSet(operation, bindingInfo, true);

                            // Add binding data tables to the result
                            DataHelper.TransferTables(ds, bindingsDS);
                        }
                    }
                }

                // Include child dependencies
                string[] dependencyColumns = infoObj.TypeInfo.ChildDependencyColumns.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string dependencyColumn in dependencyColumns)
                {
                    string childDependencyType = ti.GetObjectTypeForColumn(dependencyColumn);
                    if (childDependencyType == null)
                    {
                        throw new Exception("[CMSObjectHelper.GetObjectsDataSet]: Unknown child dependency ID column '" + dependencyColumn + "'.");
                    }

                    // Get dependency DataSet
                    BaseInfo dependency = ModuleManager.GetReadOnlyObject(childDependencyType);
                    DataSet dependencyDS = GetObjectsDataSet(operation, dependency, true);

                    // Add dependency data tables to the result
                    DataHelper.TransferTables(ds, dependencyDS);
                }

                // Get category
                if (ti.CategoryObject != null)
                {
                    // Add data tables for extra objects
                    DataSet addDS = GetObjectsDataSet(operation, ti.CategoryObject, false);
                    DataHelper.TransferTables(ds, addDS);
                }

                // Add metafiles data tables to the result
                if (ti.HasMetaFiles)
                {
                    if (ds.Tables["CMS_MetaFile"] == null)
                    {
                        MetaFileInfo metaObj = new MetaFileInfo();
                        DataSet filesDS = GetObjectsDataSet(operation, metaObj, false);
                        DataHelper.TransferTables(ds, filesDS);
                    }
                }
            }

            return ds;
        }


        /// <summary>
        /// Gets empty DataSet for binary data.
        /// </summary>
        /// <param name="binaryData">If false, binary data column is string column</param>
        public static DataSet GetBinaryDataSet(bool binaryData)
        {
            DataSet dsBinary = new DataSet();

            // Prepare table
            DataTable table = new DataTable("BinaryData");

            table.Columns.Add("FileName", typeof(string));
            table.Columns.Add("FileType", typeof(string));

            var binaryType = binaryData ? typeof(byte[]) : typeof(string);

            table.Columns.Add("FileBinaryData", binaryType);

            dsBinary.Tables.Add(table);

            return dsBinary;
        }


        /// <summary>
        /// Gets DataSet with binary data of given files.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        /// <param name="files">Array of files in format { {"fullPhysicalFile1Path", "fileType"} {"fullPhysicalFile2Path", "fileType"}}</param>
        /// <param name="maxFileSize">Maximal size in bytes of the file which should be included into the DataSet</param>
        /// <param name="binaryData">If true, gets the binary data to the DataSet</param>
        public static DataSet GetBinaryData(GeneralizedInfo infoObj, string[,] files, long maxFileSize, bool binaryData)
        {
            if (files == null)
            {
                return null;
            }

            // Get the DataSet for binary files
            DataSet dsBinary = GetBinaryDataSet(binaryData);
            DataTable table = dsBinary.Tables[0];

            for (int i = 0; i < files.GetLength(0); ++i)
            {
                string filePath = files[i, 0];
                string fileType = files[i, 1];

                if (!String.IsNullOrEmpty(filePath))
                {
                    FileInfo fi = FileInfo.New(filePath);

                    var ti = infoObj.TypeInfo;
                    var objectType = ti.ObjectType;

                    if (fi.Exists)
                    {
                        if (fi.Length <= maxFileSize)
                        {
                            // Add new row to the table
                            DataRow dr = table.NewRow();
                            dr["FileName"] = fi.Name;
                            dr["FileType"] = fileType;

                            // Get file contents from file system
                            if (binaryData)
                            {
                                byte[] fileContent = File.ReadAllBytes(filePath);
                                dr["FileBinaryData"] = fileContent;
                            }
                            else
                            {
                                dr["FileBinaryData"] = filePath;
                            }

                            table.Rows.Add(dr);
                        }
                        else
                        {
                            // Log that file size exceeds the maximum allowed size
                            string desc = GetAPIString("ObjectType." + objectType.Replace(".", "_"), null, objectType) + " '" + ResHelper.LocalizeString(infoObj.ObjectDisplayName) + "'. ";
                            desc += GetAPIString("binarydata.exceedsize", null, "Size of the binary data exceeded the maximum allowed size for staging") + " (" + DataHelper.GetSizeString(maxFileSize) + ")";

                            CoreServices.EventLog.LogEvent("W", "Staging", "SYNCFILE", desc);
                        }
                    }
                    else
                    {
                        // Log that physical file don't exist
                        string desc = GetAPIString("ObjectType." + objectType.Replace(".", "_"), null, objectType) + " '" + ResHelper.LocalizeString(infoObj.ObjectDisplayName) + "'. ";
                        desc += GetAPIString("binarydata.filenotfound", null, "Cannot read the binary data. File not found.");

                        CoreServices.EventLog.LogEvent("W", "Staging", "SYNCFILE", desc);
                    }
                }
            }

            if (!DataHelper.DataSourceIsEmpty(dsBinary))
            {
                return dsBinary;
            }

            return null;
        }


        /// <summary>
        /// Generate where condition due to excluded names.
        /// </summary>
        /// <param name="excludedNames">Excluded names</param>
        /// <param name="columnName">Column name</param>
        public static WhereCondition GetExcludedNamesWhereCondition(string[] excludedNames, string columnName)
        {
            var where = new WhereCondition();

            // Generate where condition
            foreach (string name in excludedNames)
            {
                @where.WhereNotStartsWith(columnName, name);
            }

            return @where;
        }


        /// <summary>
        /// Gets the existing children for the given object
        /// </summary>
        /// <param name="infoObj">Parent object</param>
        /// <param name="addSiteCondition">If true, site condition is added for the children</param>
        /// <param name="siteId">Site ID for the site condition</param>
        /// <param name="childObjectType">Child object type</param>
        public static IDataQuery GetExistingChildren(GeneralizedInfo infoObj, bool addSiteCondition, int siteId, string childObjectType)
        {
            var childObj = ModuleManager.GetReadOnlyObject(childObjectType);
            var childTypeInfo = childObj.TypeInfo;

            // Add parent condition
            var childWhere = new WhereCondition().WhereEquals(childTypeInfo.ParentIDColumn, infoObj.ObjectID);

            // Add site condition
            if (addSiteCondition)
            {
                string siteIdColumn = childTypeInfo.SiteIDColumn;
                if (siteIdColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    var siteWhere = new WhereCondition().WhereNull(siteIdColumn);
                    if (siteId > 0)
                    {
                        siteWhere.Or().WhereEquals(siteIdColumn, siteId);
                    }

                    childWhere.Where(siteWhere);
                }
            }

            // Add custom where condition
            childWhere = infoObj.GetChildWhereCondition(childWhere, childTypeInfo.ObjectType);

            var result = childObj.GetDataQuery(true, q => q.Where(childWhere), false);

            return result;
        }


        /// <summary>
        /// Gets the DataSet of the objects data and their child objects.
        /// </summary>
        /// <param name="operation">Operation type</param>
        /// <param name="infoObj">Main info object</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns for the main objects</param>
        /// <param name="childData">If true, child objects data are included</param>
        /// <param name="binaryData">If true, binary data are included</param>
        /// <param name="th">Translation table to initialize with data bindings</param>
        /// <param name="excludedNames">Objects with codename or display name starting with these names will be filtered out</param>
        public static DataSet GetObjectsData(OperationTypeEnum operation, BaseInfo infoObj, string where, string orderBy, bool childData, bool binaryData, TranslationHelper th, string[] excludedNames = null)
        {
            var settings = new GetObjectsDataSettings(operation, infoObj, new WhereCondition(@where), orderBy, childData, binaryData, th, excludedNames);

            return GetObjectsData(settings);
        }


        /// <summary>
        /// Gets the DataSet of the objects data and their child objects.
        /// </summary>
        /// <param name="operation">Operation type</param>
        /// <param name="infoObj">Main info object</param>
        /// <param name="parameters">Parameters for the selection</param>
        /// <param name="where">Where condition</param>
        /// <param name="orderBy">Order by columns for the main objects</param>
        /// <param name="childData">If true, child objects data are included</param>
        /// <param name="binaryData">If true, binary data are included</param>
        /// <param name="th">Translation table to initialize with data bindings</param>
        /// <param name="excludedNames">Objects with codename or display name starting with these names will be filtered out</param>
        /// <param name="siteId">Site ID</param>
        public static DataSet GetObjectsData(OperationTypeEnum operation, GeneralizedInfo infoObj, QueryDataParameters parameters, string where, string orderBy, bool childData, bool binaryData, TranslationHelper th, string[] excludedNames = null, int siteId = 0)
        {
            var settings = new GetObjectsDataSettings(operation, infoObj, new WhereCondition().Where(@where, parameters), orderBy, childData, binaryData, th, excludedNames)
            {
                SiteId = siteId
            };

            return GetObjectsData(settings);
        }


        /// <summary>
        /// Gets the DataSet of the objects data and their child objects.
        /// </summary>
        /// <param name="settings">Configuration for objects data selection</param>
        public static DataSet GetObjectsData(GetObjectsDataSettings settings)
        {
            var infoObj = settings.InfoObject;
            if (infoObj == null)
            {
                return null;
            }

            var ti = infoObj.TypeInfo;
            var operation = settings.Operation;
            var excludedNames = settings.ExcludedNames;
            var binaryData = settings.IncludeBinaryData;

            string columns = null;

            // Only get ID and code name for export selection
            bool selection = (operation == OperationTypeEnum.ExportSelection);

            if (selection && !ti.IsBinding)
            {
                columns = GetExportSelectionColumns(infoObj);
            }

            // Set default data ordering
            string orderBy = (String.IsNullOrEmpty(settings.OrderBy) && operation == OperationTypeEnum.Export) ? ti.ImportExportSettings.OrderBy : settings.OrderBy;

            // Get the objects data
            var query =
                infoObj.GetDataQuery(
                    true,
                    q =>
                    {
                        q.OrderBy(orderBy).Where(settings.WhereCondition).Columns(columns);

                        ApplyExcludedNamesWhere(q, excludedNames, ti);
                    },
                    false
                );

            query.IncludeBinaryData = binaryData;

            DataSet ds = query.Result;
            if (ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                dt.TableName = GetSerializationTableName(infoObj);

                // Set ID column as unique
                string idColumn = ti.IDColumn;
                if (idColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    DataColumn dc = dt.Columns[idColumn];
                    if (dc != null)
                    {
                        dc.Unique = true;
                    }
                }
            }

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Translations
                TranslationHelper th = settings.TranslationTable;
                if (th != null)
                {
                    // Add dependency translation
                    if (ti.ObjectDependencies != null)
                    {
                        foreach (var dep in ti.ObjectDependencies)
                        {
                            var dependencyColumn = dep.DependencyColumn;

                            if (dep.HasDynamicObjectType())
                            {
                                // Get object type column name
                                string objectTypeColumn = dep.ObjectTypeColumn;
                                th.RegisterDynamicRecords(ds.Tables[0], objectTypeColumn, dependencyColumn, TranslationHelper.AUTO_SITENAME, excludedNames);
                            }
                            else
                            {
                                string dependencyType = infoObj.GetDependencyObjectType(dep);
                                if (!String.IsNullOrEmpty(dependencyColumn))
                                {
                                    th.RegisterRecords(ds.Tables[0], dependencyType, dependencyColumn, TranslationHelper.AUTO_SITENAME, excludedNames);
                                }
                            }
                        }
                    }

                    // Add site translation
                    if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        th.RegisterRecords(ds.Tables[0], PredefinedObjectType.SITE, ti.SiteIDColumn, null, excludedNames);
                    }

                    // Add parent translation
                    if (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                    {
                        th.RegisterRecords(ds.Tables[0], infoObj.ParentObjectType, ti.ParentIDColumn, TranslationHelper.AUTO_SITENAME, excludedNames);
                    }

                    // Raise event to register custom translations
                    if (ColumnsTranslationEvents.RegisterRecords.IsBound)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            ColumnsTranslationEvents.RegisterRecords.StartEvent(th, ti.ObjectType, new DataRowContainer(row));
                        }
                    }
                }

                if (settings.IncludeChildData)
                {
                    // Get parent IDs
                    DataTable parentDT = ds.Tables[0];
                    IList<int> parentIDs = DataHelper.GetIntegerValues(parentDT, parentDT.Columns[0].ColumnName);

                    int siteId = settings.SiteId;
                    if (siteId == 0)
                    {
                        siteId = infoObj.ObjectSiteID;
                    }

                    // Add child data
                    foreach (string childType in ti.ChildObjectTypes)
                    {
                        // Get the child object type
                        GeneralizedInfo childInfo = ModuleManager.GetReadOnlyObject(childType);
                        if (childInfo == null)
                        {
                            // Object type not found due to separability
                            continue;
                        }

                        var childTypeInfo = childInfo.TypeInfo;
                        if (childTypeInfo.IncludeToParentDataSet(operation) != IncludeToParentEnum.None)
                        {
                            if (childTypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                            {
                                throw new Exception("[CMSObjectHelper.GetObjectsData]: Parent ID column of the type '" + childType + "' is not specified.");
                            }

                            // Create where condition for the child object
                            var childWhere = new WhereCondition().WhereIn(childTypeInfo.ParentIDColumn, parentIDs);

                            // Some data should be selected
                            if (!childWhere.ReturnsNoResults)
                            {
                                // Add site condition
                                if (childTypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                {
                                    string siteWhere;
                                    if (siteId > 0)
                                    {
                                        // Site export, include only children from the given site
                                        siteWhere = childTypeInfo.SiteIDColumn + "=" + siteId;

                                        if (infoObj.ObjectSiteID == 0)
                                        {
                                            // For global objects, add also global children
                                            siteWhere = SqlHelper.AddWhereCondition(siteWhere, childTypeInfo.SiteIDColumn + " IS NULL", "OR");
                                        }
                                    }
                                    else
                                    {
                                        // Global export, include only global children
                                        siteWhere = childTypeInfo.SiteIDColumn + " IS NULL";
                                    }

                                    childWhere.Where(siteWhere);
                                }

                                // Add custom child where condition
                                childWhere = infoObj.GetChildWhereCondition(childWhere, childType);

                                // Prepare selection settings for child object
                                var childSettings = settings.Clone();

                                childSettings.InfoObject = childInfo;
                                childSettings.WhereCondition = childWhere;
                                childSettings.OrderBy = null;
                                childSettings.IncludeChildData = true;
                                childSettings.SiteId = siteId;

                                // Get the child objects
                                DataSet childDS = GetObjectsData(childSettings);

                                // Add child data tables to the result
                                DataHelper.TransferTables(ds, childDS);
                            }
                        }
                    }


                    if (!selection)
                    {
                        // Add binding data
                        foreach (string bindingType in ti.BindingObjectTypes)
                        {
                            // Get the binding object type
                            GeneralizedInfo bindingInfo = ModuleManager.GetReadOnlyObject(bindingType);
                            if (bindingInfo == null)
                            {
                                // Object type not found due to separability
                                continue;
                            }

                            var bindingTypeInfo = bindingInfo.TypeInfo;
                            if (bindingTypeInfo.IncludeToParentDataSet(operation) != IncludeToParentEnum.None)
                            {
                                // Some data should be selected
                                var bindingWhere = new WhereCondition().WhereIn(bindingTypeInfo.ParentIDColumn, parentIDs);
                                if (!bindingWhere.ReturnsNoResults)
                                {
                                    if (bindingTypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                                    {
                                        // Binding with site ID column always has site ID set
                                        bindingWhere.WhereEquals(bindingTypeInfo.SiteIDColumn, siteId);
                                    }

                                    // ### Special case - Allowed child class (add both parent and child bindings)
                                    if (bindingTypeInfo.ObjectType == PredefinedObjectType.ALLOWEDCHILDCLASS)
                                    {
                                        // Prepare selection by parent class ID
                                        var parentWhere = new WhereCondition().WhereIn("ParentClassID", parentIDs);

                                        bindingWhere.Or(parentWhere);
                                    }

                                    // Prepare selection settings for binding object
                                    var bindingSettings = settings.Clone();

                                    bindingSettings.InfoObject = bindingInfo;
                                    bindingSettings.WhereCondition = bindingWhere;
                                    bindingSettings.OrderBy = null;
                                    bindingSettings.IncludeChildData = true;

                                    // Get the binding objects
                                    DataSet bindingsDS = GetObjectsData(bindingSettings);

                                    // Add binding data tables to the result
                                    DataHelper.TransferTables(ds, bindingsDS);
                                }
                            }
                        }
                    }


                    // Include child dependencies
                    var dependencyColumns = ti.ChildDependencyColumns.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string dependencyColumn in dependencyColumns)
                    {
                        string childDependencyType = ti.GetObjectTypeForColumn(dependencyColumn);
                        if (childDependencyType == null)
                        {
                            throw new Exception("[CMSObjectHelper.GetObjectsData]: Unknown child dependency column '" + dependencyColumn + "'.");
                        }

                        // Create where condition for the dependency object
                        IList<int> dependencyIDs = DataHelper.GetIntegerValues(parentDT, dependencyColumn);
                        BaseInfo dependency = ModuleManager.GetReadOnlyObject(childDependencyType);

                        // Prepare the selection over dependency IDs
                        var dependencyWhere = new WhereCondition().WhereIn(dependency.TypeInfo.IDColumn, dependencyIDs);

                        // Some data should be selected
                        if (!dependencyWhere.ReturnsNoResults)
                        {
                            // Prepare selection settings for dependency object
                            var dependencySettings = settings.Clone();

                            dependencySettings.InfoObject = dependency;
                            dependencySettings.WhereCondition = dependencyWhere;
                            dependencySettings.OrderBy = null;
                            dependencySettings.IncludeChildData = true;

                            // Get the data
                            DataSet classDS = GetObjectsData(dependencySettings);

                            // Add dependency data tables to the result
                            DataHelper.TransferTables(ds, classDS);
                        }
                    }

                    // Get object categories
                    var catObj = ti.CategoryObject;
                    if (catObj != null)
                    {
                        var catTypeInfo = catObj.TypeInfo;

                        TransferHierarchicalCategoryDataSet(ti.CategoryIDColumn, catTypeInfo.CategoryIDColumn, catTypeInfo.ObjectType, parentDT, binaryData, th, excludedNames, operation, ds);
                    }

                    if (!selection)
                    {
                        // Add metafiles data
                        if (ti.HasMetaFiles)
                        {
                            var metaWhere = new WhereCondition().WhereIn("MetaFileObjectID", parentIDs);

                            // Check if some data should be selected
                            if (!metaWhere.ReturnsNoResults)
                            {
                                metaWhere.Where(MetaFileInfoProvider.GetWhereCondition(0, ti.ObjectType));

                                var metaObj = new MetaFileInfo();

                                // Prepare selection settings for metafile object
                                var metaSettings = settings.Clone();

                                metaSettings.InfoObject = metaObj;
                                metaSettings.WhereCondition = metaWhere;
                                metaSettings.OrderBy = null;
                                metaSettings.IncludeChildData = false;

                                // Get the data
                                DataSet filesDS = GetObjectsData(metaSettings);

                                // Ensure binary data
                                if (binaryData && (filesDS != null))
                                {
                                    MetaFileInfoProvider.EnsureMetaFileBinaries(filesDS.Tables["CMS_MetaFile"]);
                                }

                                // Add metafiles data tables to the result
                                DataHelper.TransferTables(ds, filesDS);
                            }
                        }
                    }
                }
            }

            return ds;
        }


        /// <summary>
        /// Adds table with categories parents to data set.
        /// </summary>
        /// <param name="columnName">Column name of table ID</param>
        /// <param name="parentColumnName">Column name of parent key ID</param>
        /// <param name="objectType">Object type</param>
        /// <param name="parentDT">Data table with parent data</param>
        /// <param name="binaryData">If true, binary data are included</param>
        /// <param name="th">Translation table to initialize with data bindings</param>
        /// <param name="excludedNames">Objects with codename or display name starting with these names will be filtered out</param>
        /// <param name="operation">Operation type</param>
        /// <param name="ds">Data set to add the data</param>
        private static void TransferHierarchicalCategoryDataSet(string columnName, string parentColumnName, string objectType, DataTable parentDT, bool binaryData, TranslationHelper th, string[] excludedNames, OperationTypeEnum operation, DataSet ds)
        {
            // Create where condition for the child object
            IList<int> categoryIDs = DataHelper.GetIntegerValues(parentDT, columnName);
            GeneralizedInfo categoryObj = ModuleManager.GetReadOnlyObject(objectType);

            var categoryWhere = new WhereCondition().WhereIn(categoryObj.TypeInfo.IDColumn, categoryIDs);

            // Some data should be selected
            if (!categoryWhere.ReturnsNoResults)
            {
                // Configure selection settings
                var settings = new GetObjectsDataSettings(operation, categoryObj, categoryWhere, null, false, binaryData, th, excludedNames);

                // Get the data
                DataSet categoryDS = GetObjectsData(settings);
                if (!DataHelper.DataSourceIsEmpty(categoryDS))
                {
                    // Process parent categories if parent column specified
                    if ((parentColumnName != null) && (parentColumnName != ObjectTypeInfo.COLUMN_NAME_UNKNOWN))
                    {
                        bool process = true;

                        // While new IDs are collected
                        while (process)
                        {
                            int categoryCount = categoryDS.Tables[0].Rows.Count;

                            var parentCategoryIDs = DataHelper.GetIntegerValues(categoryDS.Tables[0], parentColumnName);
                            var parentCategoryWhere = new WhereCondition().WhereIn(categoryObj.TypeInfo.IDColumn, parentCategoryIDs);

                            settings.WhereCondition = new WhereCondition(categoryWhere).Or(parentCategoryWhere);

                            // ### Special cases - order by expression
                            settings.OrderBy = (operation == OperationTypeEnum.Export) ? categoryObj.TypeInfo.ImportExportSettings.OrderBy : categoryObj.TypeInfo.DefaultOrderBy;

                            categoryDS = GetObjectsData(settings);

                            if (categoryDS.Tables[0].Rows.Count == categoryCount)
                            {
                                process = false;
                            }
                        }
                    }

                    // Add class data tables to the result
                    DataHelper.TransferTables(ds, categoryDS);
                }
            }
        }


        /// <summary>
        /// Combines given original WHERE condition with the WHERE condition generated for excluding specified objects.
        /// </summary>
        /// <param name="settings">Query settings</param>
        /// <param name="excludedNames">Names which should be excluded</param>
        /// <param name="objectTypeInfo">ObjectTypeInfo to provide type information of code name and display name columns</param>
        private static void ApplyExcludedNamesWhere(DataQuerySettings settings, string[] excludedNames, ObjectTypeInfo objectTypeInfo)
        {
            // Add excluded name filtration
            if (excludedNames != null)
            {
                // Filter by code names
                if (objectTypeInfo.CodeNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    settings.Where(GetExcludedNamesWhereCondition(excludedNames, objectTypeInfo.CodeNameColumn));
                }

                // Filter by display names
                if (objectTypeInfo.DisplayNameColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    settings.Where(GetExcludedNamesWhereCondition(excludedNames, objectTypeInfo.DisplayNameColumn));
                }
            }
        }


        /// <summary>
        /// Extracts the object data from given DataSet.
        /// </summary>
        /// <param name="ds">Source DataSet</param>
        /// <param name="infoObj">Object type</param>
        /// <param name="where">Where condition</param>
        /// <param name="childData">Child data</param>
        public static DataSet ExtractObjectsData(DataSet ds, GeneralizedInfo infoObj, WhereCondition where, bool childData)
        {
            if (ds == null)
            {
                return null;
            }

            DataSet result = ds.Clone();

            // Get main table name
            var dt = GetTable(ds, infoObj);
            if (!DataHelper.DataSourceIsEmpty(dt))
            {
                // Get main records
                var stringWhere = where.ToString(true);

                DataRow[] objectData = dt.Select(stringWhere);
                if (objectData.Length > 0)
                {
                    var resultDT = GetTable(result, infoObj);

                    // Add main records
                    foreach (DataRow dr in objectData)
                    {
                        resultDT.Rows.Add(dr.ItemArray);
                    }

                    // Add child records
                    if (childData)
                    {
                        // Get parent IDs
                        var parentIDs = DataHelper.GetIntegerValues(resultDT, resultDT.Columns[0].ColumnName);

                        // Add child data
                        var ti = infoObj.TypeInfo;

                        foreach (string childType in ti.ChildObjectTypes)
                        {
                            // Get the child object type
                            GeneralizedInfo childInfo = ModuleManager.GetReadOnlyObject(childType);
                            if (childInfo == null)
                            {
                                throw new Exception("[CMSObjectHelper.ExtractObjectsData]: Unknown child object type '" + childType + "'.");
                            }

                            if (childInfo.TypeInfo.ParentIDColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                            {
                                throw new Exception("[CMSObjectHelper.ExtractObjectsData]: Parent ID column of the type '" + childType + "' is not specified.");
                            }

                            // Create where condition for the child object
                            var childWhere = new WhereCondition().WhereIn(childInfo.TypeInfo.ParentIDColumn, parentIDs);

                            // Some data should be selected
                            if (!childWhere.ReturnsNoResults)
                            {
                                // Add custom child where condition
                                childWhere = infoObj.GetChildWhereCondition(childWhere, childType);

                                // Get the child objects
                                DataSet childDS = ExtractObjectsData(ds, childInfo, childWhere, true);

                                // Add child data tables to the result
                                DataHelper.TransferTables(result, childDS);
                            }
                        }
                    }
                }
            }

            return result;
        }


        /// <summary>
        /// Returns the list of columns needed for export selection.
        /// </summary>
        /// <param name="infoObj">Info object for which to get the columns</param>
        public static string GetExportSelectionColumns(GeneralizedInfo infoObj)
        {
            List<string> columns = new List<string>();
            var ti = infoObj.TypeInfo;

            // ID column
            if (ti.IDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                columns.Add(ti.IDColumn);
            }

            // ID column
            if (ti.ParentIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                columns.Add(ti.ParentIDColumn);
            }

            // Depends on site (Site ID column)
            if (ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                columns.Add(ti.SiteIDColumn);
            }

            // Depends by foreign key
            if (ti.ObjectDependencies != null)
            {
                foreach (var dep in ti.ObjectDependencies)
                {
                    columns.Add(dep.DependencyColumn);
                }
            }

            return String.Join(", ", columns.ToArray());
        }


        /// <summary>
        /// Get dependingObjectType dependencies: 1. parent ID column, 2. site ID column, 3. list of object dependencies where dependency object type is dependsOnObjectType object type. 
        /// </summary>
        /// <param name="dependingObjectType">Depending object type</param>
        /// <param name="dependsOnObjectType">Depends on object type</param>
        public static IList<string> GetDependencyColumnNames(string dependingObjectType, string dependsOnObjectType)
        {
            GeneralizedInfo infoObj = ModuleManager.GetReadOnlyObject(dependingObjectType);
            if (infoObj == null)
            {
                throw new Exception("[CMSObjectHelper.ObjectDependsOn]: Object type '" + dependingObjectType + "' not found.");
            }

            var ti = infoObj.TypeInfo;
            IList<string> dependencyColumns = new List<string>();

            // Depends on parent
            if (ti.ParentObjectType.EqualsCSafe(dependsOnObjectType, true))
            {
                dependencyColumns.Add(ti.ParentIDColumn);
                return dependencyColumns;
            }

            // Depends on site (Site ID column)
            if ((ti.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN) && dependsOnObjectType.EqualsCSafe(PredefinedObjectType.SITE, true))
            {
                dependencyColumns.Add(ti.SiteIDColumn);
                return dependencyColumns;
            }

            // Depends by foreign key
            if (ti.ObjectDependencies != null)
            {
                foreach (var dep in ti.ObjectDependencies)
                {
                    // Skip dynamic dependencies
                    if (dep.HasDynamicObjectType())
                    {
                        continue;
                    }

                    // If foreign key matches, depends by foreign key
                    if (infoObj.GetDependencyObjectType(dep).EqualsCSafe(dependsOnObjectType, true))
                    {
                        dependencyColumns.Add(dep.DependencyColumn);
                    }
                }

                return (dependencyColumns.Count == 0) ? null : dependencyColumns;
            }

            return null;
        }


        /// <summary>
        /// Adds depending object types to the list.
        /// </summary>
        /// <param name="list">Object list</param>
        /// <param name="objectType">Object type</param>
        /// <param name="dependingType">Depending type</param>
        /// <param name="child">Process also child types</param>
        /// <param name="binding">Process also binding types</param>
        public static void AddDependingObjectTypes(List<string> list, string objectType, string dependingType, bool child, bool binding)
        {
            GeneralizedInfo dependingObj = ModuleManager.GetReadOnlyObject(dependingType);
            if (dependingObj == null)
            {
                // Object type not found due to separability
                return;
                //throw new Exception("[CMSObjectHelper.AddDependingObjectTypes]: Object type '" + dependingType + "' not found.");
            }

            // Try the object itself
            if (GetDependencyColumnNames(dependingType, objectType) != null)
            {
                TypeHelper.AddType(list, dependingType);
            }

            var dependingTypeInfo = dependingObj.TypeInfo;

            // Try the child objects
            if (child)
            {
                // Process child objects
                foreach (string childType in dependingTypeInfo.ChildObjectTypes)
                {
                    // Process child type
                    AddDependingObjectTypes(list, objectType, childType, true, binding);
                }
            }

            // Try the binding objects
            if (binding)
            {
                // Process binding objects
                foreach (string bindingType in dependingTypeInfo.BindingObjectTypes)
                {
                    // Process binding type
                    AddDependingObjectTypes(list, objectType, bindingType, child, true);
                }
            }
        }


        /// <summary>
        /// Gets the site binding object for specified info object.
        /// </summary>
        /// <param name="infoObj">Info object</param>
        public static GeneralizedInfo GetSiteBindingObject(GeneralizedInfo infoObj)
        {
            // Update bindings
            foreach (string bindingType in infoObj.TypeInfo.BindingObjectTypes)
            {
                GeneralizedInfo bindingObj = ModuleManager.GetReadOnlyObject(bindingType);
                if (bindingObj == null)
                {
                    throw new Exception("[CMSObjectHelper.GetSiteBinding]: Binding type '" + bindingType + "' not found.");
                }

                if (bindingObj.TypeInfo.SiteIDColumn != ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
                {
                    return bindingObj;
                }
            }

            return null;
        }


        /// <summary>
        /// Returns translated name of object type.
        /// </summary>
        /// <param name="objectType">Object type</param>
        public static string GetObjectFriendlyName(string objectType)
        {
            return GetString(TypeHelper.GetObjectTypeResourceKey(objectType));
        }


        /// <summary>
        /// Builds the full name from the given names.
        /// </summary>
        /// <param name="first">First name</param>
        /// <param name="second">Second name</param>
        /// <param name="separator">Separator</param>
        public static string BuildFullName(string first, string second, string separator = ".")
        {
            return String.Concat(first, separator, second);
        }


        /// <summary>
        /// Parses the full name in format "something.whatewer.abc" so that "something.whatever" is considered the first part, and "abc" second part.
        /// </summary>
        /// <param name="fullName">Given full name</param>
        /// <param name="firstPart">First part</param>
        /// <param name="secondPart">Second part</param>
        /// <param name="delimiter">Fullname delimiter</param>
        public static bool ParseFullName(string fullName, out string firstPart, out string secondPart, string delimiter = ".")
        {
            if (!String.IsNullOrEmpty(fullName))
            {
                // Parse the full name
                int dotIndex = fullName.LastIndexOf(delimiter, StringComparison.Ordinal);
                if (dotIndex >= 0)
                {
                    firstPart = fullName.Substring(0, dotIndex);
                    secondPart = fullName.Substring(dotIndex + delimiter.Length);

                    return true;
                }
            }

            firstPart = null;
            secondPart = null;

            return false;
        }


        /// <summary>
        /// Gets the parent ID column for the given child type in parent type
        /// </summary>
        /// <param name="childTypeInfo">Child type info</param>
        /// <param name="parentTypeInfo">Parent type info</param>
        /// <exception cref="InvalidOperationException">Thrown when parent object is not defined or could not be discovered.</exception>
        public static string GetParentIdColumn(ObjectTypeInfo childTypeInfo, ObjectTypeInfo parentTypeInfo)
        {
            var parentIdColumn = childTypeInfo.PossibleParentIDColumn;
            if (parentIdColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var parentIdColumns = childTypeInfo.GetTypeColumns(parentTypeInfo);
                if (parentIdColumns.Count != 1)
                {
                    throw new InvalidOperationException("Could not find parent ID column for nested object type '" + childTypeInfo.ObjectType + "' of object type '" + parentTypeInfo.ObjectType + "'.");
                }

                parentIdColumn = parentIdColumns.First();
            }

            return parentIdColumn;
        }


        /// <summary>
        /// Gets the parent ID column for the given binding type info for the direction of other binding (indirect parent)
        /// </summary>
        /// <param name="bindingTypeInfo">Binding type info</param>
        /// <param name="parentTypeInfo">Parent type info</param>
        public static string GetOtherBindingParentIdColumn(ObjectTypeInfo bindingTypeInfo, ObjectTypeInfo parentTypeInfo)
        {
            string parentIdColumn = ObjectTypeInfo.COLUMN_NAME_UNKNOWN;

            if (parentTypeInfo.ObjectType == PredefinedObjectType.SITE)
            {
                // For site dependencies get the SiteID column
                parentIdColumn = bindingTypeInfo.SiteIDColumn;
            }
            else
            {
                // Get the first binding object dependency with matching object type - it should be the column of the "parent binding" object ID
                var firstDependency = bindingTypeInfo.ObjectDependencies
                                                     .Where(dep => dep.DependencyType == ObjectDependencyEnum.Binding)
                                                     .FirstOrDefault(dep => parentTypeInfo.RepresentsType(dep.DependencyObjectType));

                if (firstDependency != null)
                {
                    parentIdColumn = firstDependency.DependencyColumn;
                }
            }

            // If the column was not found, try to find just by type
            if (parentIdColumn == ObjectTypeInfo.COLUMN_NAME_UNKNOWN)
            {
                var parentIdColumns = bindingTypeInfo.GetTypeColumns(parentTypeInfo);
                if (parentIdColumns.Count != 1)
                {
                    throw new Exception("[BaseInfo.OtherBindings]: Could not find single ID column for parent binding object '" + bindingTypeInfo.ObjectType + "' of object '" + parentTypeInfo.ObjectType + "'.");
                }

                parentIdColumn = parentIdColumns.First();
            }

            return parentIdColumn;
        }


        /// <summary>
        /// Tries to retrieve a value from SerializationInfo store.
        /// Returns default value if an item with given name was not found.
        /// </summary>
        /// <param name="info">Object info</param>
        /// <param name="itemName">Name of the deserialized item</param>
        /// <param name="dataType">Data type</param>
        /// <param name="defaultValue">Value to return in case item with given name was not found</param>
        internal static object GetSerializedData(SerializationInfo info, string itemName, Type dataType, object defaultValue)
        {
            try
            {
                return info.GetValue(itemName, dataType);
            }
            catch (SerializationException)
            {
                // Serialized info does not contain item with given item name
                return defaultValue;
            }
        }

        #endregion
    }
}
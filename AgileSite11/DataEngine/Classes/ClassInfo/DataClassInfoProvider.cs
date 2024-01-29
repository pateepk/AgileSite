using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using CMS.Helpers;
using CMS.IO;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Provides access to data class information.
    /// </summary>    
    public class DataClassInfoProvider : DataClassInfoProviderBase<DataClassInfoProvider>
    {
        #region "Variables"

        /// <summary>
        /// Supplementary constant to specify all class names.
        /// </summary>
        public const string ALL_CLASSNAMES = "##ALL##";


        /// <summary>
        /// Zero time constant.
        /// </summary>
        public static DateTime ZERO_TIME = DateTimeHelper.ZERO_TIME;


        /// <summary>
        /// Virtual directory where the FormLayouts are located.
        /// </summary>
        private const string mFormLayoutsDirectory = "~/CMSVirtualFiles/FormLayouts";

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the value that indicates whether class layouts should be stored externally
        /// </summary>
        public static bool StoreFormLayoutsInExternalStorage
        {
            get
            {
                return SettingsKeyInfoProvider.GetBoolValue("CMSStoreFormLayoutsInFS");
            }
            set
            {
                SettingsKeyInfoProvider.SetGlobalValue("CMSStoreFormLayoutsInFS", value);
            }
        }


        /// <summary>
        /// Form layouts directory - Read only
        /// </summary>
        public static string FormLayoutsDirectory => mFormLayoutsDirectory;


        /// <summary>
        /// If true, the data query is used for getting the class schema from the database
        /// </summary>
        public static BoolAppSetting UseDataQueryForSchema = new BoolAppSetting("CMSUseDataQueryForSchema", true);

        #endregion


        #region "Methods"

        /// <summary>
        /// Ensures the database structure based on the given class form definition. 
        /// If the database table doesn't exist, it is created together with required columns.
        /// If the database already exists, columns are updated based on the definition.
        /// </summary>
        /// <param name="classInfo">Data class info</param>
        /// <param name="updateSystemFields">If true, the system fields are updated as well</param>
        /// <exception cref="DataClassTableUpdateException">
        /// Thrown when class table is not associated with the provided <paramref name="classInfo"/>.
        /// </exception>
        private void EnsureDatabaseStructure(DataClassInfo classInfo, bool updateSystemFields = false)
        {
            // Only process if table name set
            var tableName = classInfo.ClassTableName;
            if (String.IsNullOrEmpty(tableName))
            {
                return;
            }

            var tm = new TableManager(classInfo.ClassConnectionString);
            tm.UpdateSystemFields = updateSystemFields;

            bool tableNameChanged = classInfo.ItemChanged("ClassTableName");
            bool updateTableDefinition = classInfo.ItemChanged("ClassFormDefinition");
            string originalTableName = ValidationHelper.GetString(classInfo.GetOriginalValue("ClassTableName"), String.Empty);
            
            if (!String.IsNullOrEmpty(originalTableName) && tableNameChanged && tm.TableExists(originalTableName))
            {
                // Table name changed so rename original table
                tm.RenameTable(originalTableName, tableName);
            }
            else if (!tm.TableExists(tableName))
            {
                tm.CreateTableByDefinition(classInfo);

                // In case of creating new table there is no need to also update its definition
                updateTableDefinition = false;
            }

            if (updateTableDefinition)
            {
                // Get original definition
                var originalDefinition = GetOriginalDefinition(classInfo);

                using (var transactionScope = BeginTransaction())
                {
                    var parameters = new UpdateTableParameters
                    {
                        ClassInfo = classInfo,
                        OriginalDefinition = originalDefinition,
                    };

                    tm.UpdateTableByDefinition(parameters);
                    tm.RefreshSystemViews(tableName);

                    transactionScope.Commit();
                }
            }

            // Update XML schema
            classInfo.ClassXmlSchema = tm.GetXmlSchema(tableName);
        }


        /// <summary>
        /// Gets original definition for the class
        /// </summary>
        /// <param name="classInfo">Data class info</param>
        /// <exception cref="DataClassTableUpdateException">
        /// Thrown when class table is not associated with the provided <paramref name="classInfo"/>.
        /// </exception>
        private static string GetOriginalDefinition(DataClassInfo classInfo)
        {
            var tableName = classInfo.ClassTableName;

            // Get class with same table name
            var definitions = GetClasses()
                .TopN(2)
                .Column("ClassFormDefinition")
                .WhereEquals("ClassTableName", SqlHelper.RemoveOwner(tableName))
                .GetListResult<string>();

            // There is a class with the same table name
            if (definitions.Count > 1)
            {
                var className = classInfo.ClassName;
                throw new DataClassTableUpdateException(
                    String.Format(
                        "Could not update table '{0}' required for data class '{1}' ('{2}'). Table named '{0}' already exists, but it is not associated with this data class.",
                        tableName,
                        classInfo.ClassDisplayName,
                        className
                        ),
                    className,
                    tableName
                    );
            }

            return definitions.FirstOrDefault();
        }


        /// <summary>
        /// Returns DataClassInfo object for specified path.
        /// </summary>
        /// <param name="path">Path</param>
        public static DataClassInfo GetVirtualObject(string path)
        {
            List<string> prefixes = new List<string>();
            // Get data class name
            string name = VirtualPathHelper.GetVirtualObjectName(path, FormLayoutsDirectory, ref prefixes);
            return GetDataClassInfo(name);
        }


        /// <summary>
        /// Checks if some database objects are dependent on database representation of this class. 
        /// </summary>
        public static List<string> CheckDatabaseDependencies(int classId)
        {
            var classInfo = GetDataClassInfo(classId);

            if (classInfo == null)
            {
                return new List<string>();
            }

            var tm = new TableManager(classInfo.ClassConnectionString);
            if (tm.TableExists(classInfo.ClassTableName))
            {
                return tm.GetTableDependencies(classInfo.ClassTableName);
            }

            return new List<string>();
        }


        /// <summary>
        /// Gets the specified DataClassName.
        /// </summary>
        public static string GetClassName(int classId)
        {
            var ci = GetDataClassInfo(classId);
            return ci?.ClassName;
        }


        /// <summary>
        /// Loads all the classes into the hashtable.
        /// </summary>
        public static void LoadAllClasses()
        {
            ProviderObject.LoadAllInfos();
        }


        /// <summary>
        /// Gets an empty DataSet created by class XML schema.
        /// </summary>
        /// <param name="className">Class name</param>
        /// <exception cref="DataClassNotFoundException">Thrown when the data class with the given class name is not found.</exception>
        public static DataSet GetDataSet(string className)
        {
            // Empty DataSet when className not specified
            if (className == null)
            {
                return new DataSet();
            }

            // Get the class
            DataClassInfo ci = GetDataClassInfo(className);
            if (ci == null)
            {
                throw new DataClassNotFoundException("[DataClassInfoProvider.GetDataSet]: Class '" + className + "' not found", className);
            }

            return ci.GetDataSet();
        }


        /// <summary>
        /// Returns unique class name created from the given class name.
        /// </summary>
        /// <param name="className">Base class name</param>
        public static string GetUniqueClassName(string className)
        {
            string baseName = className.TrimEnd('1', '2', '3', '4', '5', '6', '7', '8', '9', '0');
            int index = 1;

            // Iterate until the class name exists
            while (GetDataClassInfo(className) != null)
            {
                className = baseName + index;
                index++;
            }

            return className;
        }


        /// <summary>
        /// Gets the XML schema for the given class
        /// </summary>
        /// <param name="className">Class name</param>
        /// <exception cref="DataClassNotFoundException">Thrown when the data class with the given class name is not found.</exception>
        internal static ClassStructureInfo GetClassStructureInfoFromDB(string className)
        {
            if (String.IsNullOrEmpty(className))
            {
                return null;
            }

            DataSet ds;

            if (UseDataQueryForSchema)
            {
                // Get data from provider
                ds = GetClasses()
                        .Columns("ClassXmlSchema", "ClassTableName")
                        .WhereEquals("ClassName", className);
            }
            else
            {
                // Prepare the parameters
                QueryDataParameters parameters = new QueryDataParameters();
                parameters.Add("@ClassName", className);

                // Get the data
                ds = ConnectionHelper.ExecuteQuery("Proc_CMS_Class_GetXmlSchema", parameters, QueryTypeEnum.StoredProcedure);
            }

            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                // Create the class structure info
                var dr = ds.Tables[0].Rows[0];

                var schema = ValidationHelper.GetString(dr[0], "");
                var tableName = ValidationHelper.GetString(dr[1], "");

                return new ClassStructureInfo(className, schema, tableName);
            }

            throw new DataClassNotFoundException("[DataClassInfoProvider.GetClassStructureInfoFromDB]: Class information for class '" + className + "' not found.", className);
        }


        internal static IEnumerable<ObjectTypeInfo> GetClassObjectTypes(string className)
        {
            return ObjectTypeManager.GetTypeInfos(ObjectTypeManager.ExistingObjectTypes, info => info.ObjectClassName.Equals(className, StringComparison.InvariantCultureIgnoreCase));
        }

        #endregion


        #region "Overridden methods"

        /// <summary>
        /// Returns DataClassInfo with specified name.
        /// </summary>
        /// <param name="name">DataClassInfo name</param>
        /// <param name="throwIfNotFound">If true, the call throws an exception in case the data class was not found</param>
        /// <exception cref="DataClassNotFoundException">Thrown when the data class with the given class name is not found.</exception>
        public static DataClassInfo GetDataClassInfo(string name, bool throwIfNotFound)
        {
            var result = GetDataClassInfo(name);
            if ((result == null) && throwIfNotFound)
            {
                throw new DataClassNotFoundException("[SqlGenerator.GetAutomaticQuery]: Class name '" + name + "' doesn't exist.", name);
            }

            return result;
        }


        /// <summary>
        /// Inserts or Updates the object to the database.
        /// </summary>
        /// <param name="info">Object to insert / update</param>
        protected override void SetInfo(DataClassInfo info)
        {
            var update = info.Generalized.ObjectID > 0;

            bool classNameChanged = info.ItemChanged("ClassName");

            // Clear the default queries if insert or class name changed
            var clearQueries = !update || classNameChanged;

            if (CheckEnsureDatabaseStructure(info))
            {
                EnsureDatabaseStructure(info, true);
            }

            // Update version GUID if layout changed or new
            if (!update || info.ItemChanged("ClassFormLayout"))
            {
                info.ClassVersionGUID = Guid.NewGuid().ToString();
            }
            
            if (update && info.ItemChanged("ClassXmlSchema"))
            {
                info.RemoveObsoleteSearchSettings();
            }

            base.SetInfo(info);

            if (clearQueries)
            {
                QueryInfoProvider.ClearDefaultQueries(info, true, true);
            }
        }


        private bool CheckEnsureDatabaseStructure(DataClassInfo infoObj)
        {
            // Class is not coupled or table name is not specified
            if (!infoObj.ClassIsCoupledClass || String.IsNullOrEmpty(infoObj.ClassTableName))
            {
                return false;
            }

            // If class is updated check if table name or form definition was changed, if not no changes are needed
            if ((infoObj.ClassID > 0) && !infoObj.ItemChanged("ClassTableName") && !infoObj.ItemChanged("ClassFormDefinition") && !infoObj.ItemChanged("ClassXmlSchema"))
            {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Gets the object query for the provider
        /// </summary>
        protected override ObjectQuery<DataClassInfo> GetObjectQueryInternal()
        {
            var q = base.GetObjectQueryInternal();

            // Ensure query source
            q.CustomQueryText = SqlHelper.GENERAL_SELECT;
            q.ConnectionStringName = ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME;
            q.DefaultQuerySource = "CMS_Class";

            return q;
        }


        /// <summary>
        /// Deletes the object to the database.
        /// </summary>
        /// <param name="info">Object to delete</param>
        protected override void DeleteInfo(DataClassInfo info)
        {
            if ((info != null)
                && !info.ClassIsCustomTable
                && !info.ClassIsDocumentType
                && !info.ClassIsForm
                && GetClassObjectTypes(info.ClassName).Any())
            {
                throw new InvalidOperationException("You first need to delete the corresponding code (Info, Provider) and recompile the solution.");
            }

            base.DeleteInfo(info);
        }

        #endregion
    }
}
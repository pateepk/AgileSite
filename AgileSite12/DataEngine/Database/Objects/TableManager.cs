using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml;

using CMS.Core;
using CMS.Helpers;
using CMS.Base;
using CMS.DataEngine.CollectionExtensions;

namespace CMS.DataEngine
{
    using TableManagerDictionary = StringSafeDictionary<ITableManager>;


    /// <summary>
    /// Ensures management of database table and table column.
    /// </summary>
    public class TableManager
    {
        #region "Variables"

        private static CultureInfo mDatabaseCultureInfo;

        /// <summary>
        /// If true, the indexed views are used and generated within the system
        /// </summary>
        public static bool USE_INDEXED_VIEWS = true;


        /// <summary>
        /// Default table manager
        /// </summary>
        private static readonly CMSStatic<ITableManager> mDefaultTableManager = new CMSStatic<ITableManager>();

        /// <summary>
        /// Default system table manager (debug disabled)
        /// </summary>
        private static readonly CMSStatic<ITableManager> mDefaultSystemTableManager = new CMSStatic<ITableManager>();


        /// <summary>
        /// Table managers
        /// </summary>
        private static readonly CMSStatic<TableManagerDictionary> mTableManagers = new CMSStatic<TableManagerDictionary>(() => new TableManagerDictionary());


        private static string mDatabaseCulture;

        #endregion


        #region "Properties"

        /// <summary>
        /// Current connection string for the table management
        /// </summary>
        public string ConnectionString
        {
            get;
            protected set;
        }


        /// <summary>
        /// Default Table manager object
        /// </summary>
        internal static ITableManager DefaultSystemTableManagerObject
        {
            get
            {
                if (mDefaultSystemTableManager.Value == null)
                {
                    var tm = DataConnectionFactory.NewTableManagerObject(null);
                    tm.DisableDebug = true;

                    mDefaultSystemTableManager.Value = tm;
                }

                return mDefaultSystemTableManager.Value;
            }
        }


        /// <summary>
        /// Default Table manager object
        /// </summary>
        internal static ITableManager DefaultTableManagerObject
        {
            get
            {
                return mDefaultTableManager.Value ?? (mDefaultTableManager.Value = DataConnectionFactory.NewTableManagerObject(null));
            }
            set
            {
                mDefaultTableManager.Value = value;
            }
        }


        /// <summary>
        /// Table manager object
        /// </summary>
        protected ITableManager TableManagerObject
        {
            get
            {
                if (String.IsNullOrEmpty(ConnectionString))
                {
                    // Default table manager
                    return DefaultTableManagerObject;
                }

                return GetTableManager();
            }
        }


        /// <summary>
        /// Database culture setting from the web.config.
        /// </summary>
        public static string DatabaseCulture
        {
            get
            {
                return mDatabaseCulture ?? (mDatabaseCulture = DefaultTableManagerObject.DatabaseCulture);
            }
            set
            {
                mDatabaseCulture = value;
            }
        }


        /// <summary>
        /// Database culture info obtained from DatabaseCulture property.
        /// </summary>
        public static CultureInfo DatabaseCultureInfo
        {
            get
            {
                return mDatabaseCultureInfo ?? (mDatabaseCultureInfo = CultureHelper.GetCultureInfo(DatabaseCulture));
            }
        }


        /// <summary>
        /// Gets database size(including log size) in MB.
        /// </summary>
        public string DatabaseSize
        {
            get
            {
                return TableManagerObject.DatabaseSize;
            }
        }


        /// <summary>
        /// Gets database name.
        /// </summary>
        public string DatabaseName
        {
            get
            {
                return TableManagerObject.DatabaseName;
            }
        }


        /// <summary>
        /// Gets database server name.
        /// </summary>
        public string DatabaseServerName
        {
            get
            {
                return TableManagerObject.DatabaseServerName;
            }
        }


        /// <summary>
        /// Gets database server version.
        /// </summary>
        public string DatabaseServerVersion
        {
            get
            {
                return TableManagerObject.DatabaseServerVersion;
            }
        }


        /// <summary>
        /// If true, the table manager disables debug for its queries
        /// </summary>
        public bool DisableDebug
        {
            get;
            set;
        }


        /// <summary>
        /// Determines whether system fields should be updated when updating table by schema. By default updates only non-system fields.
        /// </summary>
        public bool UpdateSystemFields
        {
            get;
            set;
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionStringName">Connection string name</param>
        public TableManager(string connectionStringName)
        {
            ConnectionString = connectionStringName;
        }

        #endregion


        #region "View management"

        /// <summary>
        /// Creates specified view in database
        /// </summary>
        /// <param name="viewName">View name to create</param>
        /// <param name="selectExpression">Select expression for the view</param>
        /// <param name="indexed">If true, the view is indexed (schema bound)</param>
        /// <param name="schema">Schema of the indexed view</param>
        public void CreateView(string viewName, string selectExpression, bool indexed = false, string schema = null)
        {
            TableManagerObject.CreateView(viewName, selectExpression, indexed, schema);
        }


        /// <summary>
        /// Alters specified view in database
        /// </summary>
        /// <param name="viewName">View name to create</param>
        /// <param name="selectExpression">Select expression for the view</param>
        /// <param name="indexed">If true, the view is indexed (schema bound)</param>
        /// <param name="schema">Schema of the indexed view</param>
        public void AlterView(string viewName, string selectExpression, bool indexed = false, string schema = null)
        {
            TableManagerObject.AlterView(viewName, selectExpression, indexed, schema);
        }


        /// <summary>
        /// Executes query and returns the results in a DataSet.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="queryType">Query type</param>
        public DataSet ExecuteQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType)
        {
            return TableManagerObject.ExecuteQuery(queryText, queryParams, queryType);
        }


        /// <summary>
        /// Drop specified view from database.
        /// </summary>
        /// <param name="viewName">View name to drop</param>
        public string DropView(string viewName)
        {
            return TableManagerObject.DropView(viewName);
        }


        /// <summary>
        /// Refreshes specified view in database.
        /// </summary>
        /// <param name="viewName">View name to refresh</param>
        public void RefreshView(string viewName)
        {
            if (IsGeneratedSystemView(viewName))
            {
                // Generate new code for the view
                string indexes;
                string newSelect = SqlGenerator.GetSystemViewSqlQuery(viewName, out indexes);

                // Update views in transaction to ensure DB consistency
                using (var tr = new CMSTransactionScope())
                {
                    bool indexed = !String.IsNullOrEmpty(indexes);

                    // Drop and recreate the view with new select statement
                    var schema = DropView(viewName);

                    CreateView(viewName, newSelect, indexed, schema);

                    // Execute the extra code for the view
                    if (indexed)
                    {
                        TableManagerObject.ExecuteQuery(indexes, null, QueryTypeEnum.SQLQuery);
                    }

                    // Commit the changes if successful
                    tr.Commit();
                }
            }
            else
            {
                // Just refresh the existing view code
                TableManagerObject.RefreshView(viewName);
            }
        }


        /// <summary>
        /// Determines whether specified DB view exists or not.
        /// </summary>
        /// <param name="viewName">View name to check</param>
        public bool ViewExists(string viewName)
        {
            return TableManagerObject.ViewExists(viewName);
        }


        /// <summary>
        /// Refreshes all database views which should contain all columns of the specified system table (e.g. cms_user).
        /// Call this method after the column of that system table is added or removed.
        /// </summary>
        /// <param name="tableName">System table name</param>
        public void RefreshSystemViews(string tableName)
        {
            var viewNames = SystemViews.Instance.GetViewsForTable(tableName);
            RefreshViews(viewNames);
        }


        /// <summary>
        /// Refreshes all system views.
        /// </summary>
        /// <remarks>For upgrade procedure purposes.</remarks>
        public void RefreshAllSystemViews()
        {
            var viewNames = SystemViews.Instance.GetAllViews();
            RefreshViews(viewNames);
        }


        /// <summary>
        /// Refreshes specified views in database.
        /// </summary>
        /// <param name="viewNames">Enumeration of views.</param>
        private void RefreshViews(IEnumerable<string> viewNames)
        {
            foreach (var viewName in viewNames)
            {
                RefreshView(viewName);
            }
        }


        /// <summary>
        /// Regenerates view for documents.
        /// </summary>
        public void RefreshDocumentViews()
        {
            // Use the transaction to ensure DB consistency
            using (var tr = new CMSTransactionScope())
            {
                RefreshView(SystemViewNames.View_CMS_Tree_Joined);

                // Commit the transaction if everything went OK
                tr.Commit();
            }
        }


        /// <summary>
        /// Returns true if the given view is generated view with dynamic code.
        /// </summary>
        /// <param name="viewName">View name</param>
        public static bool IsGeneratedSystemView(string viewName)
        {
            switch (viewName)
            {
                // Generated Views
                case SystemViewNames.View_CMS_Tree_Joined:
                case SystemViewNames.View_CMS_User:
                case SystemViewNames.View_Community_Member:
                    return true;

                // Other views
                default:
                    return false;
            }
        }

        #endregion


        #region "Table management"

        /// <summary>
        /// Returns DataSet with indexes of the given object. Returns columns IndexName, DropScript, CreateScript
        /// </summary>
        /// <param name="objectName">Object name</param>
        public DataSet GetIndexes(string objectName)
        {
            return TableManagerObject.GetIndexes(objectName);
        }


        /// <summary>
        /// Gets indexes of table identified by given <paramref name="tableName"/>.
        /// Returns null when information regarding indexes is not available.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tableName"/> is null or empty string.</exception>
        internal ITableIndexes GetTableIndexes(string tableName)
        {
            ITableIndexInformationProvider iip = TableManagerObject as ITableIndexInformationProvider;

            return iip != null ? iip.GetTableIndexes(tableName) : null;
        }


        /// <summary>
        /// Gets the tables in the current database
        /// </summary>
        /// <param name="where">Tables where condition</param>
        public List<string> GetTables(string where)
        {
            return TableManagerObject.GetTables(where);
        }


        /// <summary>
        /// Gets list of object names which have foreign key constraint dependency.
        /// </summary>
        /// <param name="tableName">Table name</param>
        public List<string> GetTableDependencies(string tableName)
        {
            return TableManagerObject.GetTableDependencies(tableName);
        }


        /// <summary>
        /// Creates specified table in database. Allows specify if identity will be set on primary key column.
        /// </summary>
        /// <param name="tableName">Table name to create</param>
        /// <param name="primaryKeyName">Primary key of table to create</param>
        /// <param name="setIdentity">If true, sets identity on primary key column</param>
        public void CreateTable(string tableName, string primaryKeyName, bool setIdentity = true)
        {
            TableManagerObject.CreateTable(tableName, primaryKeyName, setIdentity);
        }


        /// <summary>
        /// Changes name of the table with original name according to the new name.
        /// </summary>
        /// <param name="oldTableName">Name of the table to rename</param>
        /// <param name="newTableName">New name of the table</param>
        public void RenameTable(string oldTableName, string newTableName)
        {
            TableManagerObject.RenameTable(oldTableName, newTableName);
        }


        /// <summary>
        /// Drop specified table from database.
        /// </summary>
        /// <param name="tableName">Table name to drop</param>
        public void DropTable(string tableName)
        {
            TableManagerObject.DropTable(tableName);
        }


        /// <summary>
        /// Returns XML schema for specified table.
        /// </summary>
        /// <param name="tableName">Name of a table to get xml schema for</param>
        public string GetXmlSchema(string tableName)
        {
            return TableManagerObject.GetXmlSchema(tableName);
        }


        /// <summary>
        /// Creates new database table and structure based on the provided data class info.
        /// </summary>
        /// <param name="classInfo">Data class info</param>
        internal void CreateTableByDefinition(DataClassInfo classInfo)
        {
            var definition = classInfo?.ClassFormDefinition;
            if (string.IsNullOrEmpty(definition))
            {
                return;
            }

            var tableName = classInfo.ClassTableName;
            var dataDefinition = GetDataDefinition(definition);
            var fields = dataDefinition.GetFields<IField>().ToHashSetCollection();

            var primaryFields = fields.Where(f => f.PrimaryKey).ToList(); 
            if (!primaryFields.Any())
            {
                throw new NotSupportedException("Missing primary key in definition.");
            }

            var multiplePK = primaryFields.Count > 1;
            var firstPKFieldName = primaryFields.First().Name;

            // Create new table with the first primary key column only
            CreateTable(tableName, firstPKFieldName, !multiplePK);

            foreach (var field in fields.Where(f => !f.Name.Equals(firstPKFieldName, StringComparison.OrdinalIgnoreCase)))
            {
                // Raise event for field addition
                using (var h = DataDefinitionItemEvents.AddItem.StartEvent(classInfo, field))
                {
                    // Do not process dummy, external and primary key columns
                    if (!field.External && !field.IsDummyField)
                    {
                        var dataType = DataTypeManager.GetDataType(TypeEnum.Field, field.DataType);
                        var columnType = dataType.GetSqlType(field.Size, field.Precision);
                        var columnDefaultValue = GetFieldDefaultValue(field, dataType);

                        AddTableColumn(tableName, field.Name, columnType, field.AllowEmpty, columnDefaultValue);
                    }
                    
                    h.FinishEvent();
                }
            }

            if (multiplePK)
            {
                RecreatePKConstraint(tableName, primaryFields.Select(x => x.Name).ToArray());
            }
        }


        /// <summary>
        /// Updates the structure of database table based on given parameters.
        /// </summary>
        /// <param name="parameters">Update table parameters</param>
        internal void UpdateTableByDefinition(UpdateTableParameters parameters)
        {
            // Validate class
            var classInfo = parameters.ClassInfo;

            // Validate new definition
            var newDefinition = classInfo?.ClassFormDefinition;
            if (string.IsNullOrEmpty(newDefinition))
            {
                return;
            }

            // Validate table name
            var tableName = classInfo.ClassTableName;
            if (string.IsNullOrEmpty(tableName))
            {
                return;
            }

            // Load new class form definition
            var newFields = GetDataDefinition(newDefinition)
                .GetFields<IField>()
                .ToList();

            var oldFields = GetOldFields(tableName, parameters.OriginalDefinition, parameters.UseOriginalDefinition)
                .ToHashSetCollection();

            var variableLengthColumnWasDropped = false;

            // First delete all columns that are not in a new table schema
            foreach (var oldField in oldFields.Where(old => newFields.All(current => !FieldsAreTheSame(old, current))))
            {
                // Raise event for field removal
                using (var h = DataDefinitionItemEvents.RemoveItem.StartEvent(classInfo, oldField))
                {
                    // Drop only not-dummy and not-external columns
                    if (IsEditableDatabaseField(oldField))
                    {
                        DropTableColumn(tableName, oldField.Name);
                    }

                    h.FinishEvent();
                }

                variableLengthColumnWasDropped |= DataTypeManager.IsVariableLengthType(oldField.DataType);
            }

            // Clean unused space of dropped columns
            if (variableLengthColumnWasDropped)
            {
                CleanTable(tableName);
            }

            // Go through all new columns and compare them to the old ones
            foreach (var field in newFields)
            {
                var oldField = oldFields.FirstOrDefault(f => FieldsAreTheSame(f, field));

                var isDbModification = IsDatabaseChangeNecessary(oldField, field);

                // Raise event for field insert or change
                using (var h = StartDataDefinitionEvent(oldField, classInfo, field))
                {
                    UpsertField(field, oldField, tableName, isDbModification);

                    h.FinishEvent();
                }
            }

            var newPkFieldNames = newFields.Where(f => f.PrimaryKey && !f.External)
                                        .Select(f => f.Name)
                                        .ToArray();
            var oldPkFieldNames = oldFields.Where(f => f.PrimaryKey && !f.External)
                                           .Select(f => f.Name)
                                           .ToArray();

            // Recreate the PK CONSTRAINT if the list of primary key columns changed
            if (newPkFieldNames.Except(oldPkFieldNames).Any() || oldPkFieldNames.Except(newPkFieldNames).Any())
            {
                RecreatePKConstraint(tableName, newPkFieldNames);
            }
        }


        private static AbstractAdvancedHandler StartDataDefinitionEvent(IField oldField, DataClassInfo classInfo, IField field)
        {
            if (oldField == null)
            {
                return DataDefinitionItemEvents.AddItem.StartEvent(classInfo, field);
            }

            return DataDefinitionItemEvents.ChangeItem.StartEvent(classInfo, oldField, field);
        }


        internal void UpsertField(IField field, IField oldField, string tableName, bool isDbModification)
        {
            // Do not allow modification of existing primary key columns
            if (!isDbModification || ((oldField != null) && field.PrimaryKey))
            {
                return;
            }

            if (oldField != null && !oldField.External && field.External)
            {
                // Drop the column if the field doesn't represent a database column anymore
                DropTableColumn(tableName, oldField.Name);
            }

            if (!IsEditableDatabaseField(field))
            {
                return;
            }

            var allowEmpty = field.AllowEmpty;
            var dataType = DataTypeManager.GetDataType(TypeEnum.Field, field.DataType);
            var columnType = dataType.GetSqlType(field.Size, field.Precision);

            string columnDefaultValue = GetFieldDefaultValue(field, dataType);

            string columnName = field.Name;

            if (oldField == null || oldField.External)
            {
                // Create the column if it didn't exist before or the field didn't represent a database column
                AddTableColumn(tableName, columnName, columnType, allowEmpty, columnDefaultValue);
            }
            else
            {
                AlterTableColumn(tableName, oldField.Name, columnName, columnType, allowEmpty, columnDefaultValue);
            }
        }


        private bool IsEditableDatabaseField(IField field)
        {
            return !field.External && !field.IsDummyField && (!field.System || UpdateSystemFields);
        }


        private static bool IsDatabaseChangeNecessary(IField oldField, IField field)
        {
            if (oldField == null)
            {
                return field != null;
            }

            var dataType = DataTypeManager.GetDataType(TypeEnum.Field, field.DataType);
            var columnType = dataType.GetSqlType(field.Size, field.Precision);

            string columnDefaultValue = GetFieldDefaultValue(field, dataType);

            // Get old column type
            var oldType = DataTypeManager.GetDataType(TypeEnum.Field, oldField.DataType);
            if (oldType == null)
            {
                throw new MissingSQLTypeException(String.Format("[TableManager.UpdateTableByDefinition] SQL type '{0}' is not registered as a default type, change type to '{1}' or register the type using DataTypeManager.RegisterDataTypes(...)", oldField.DataType, columnType),
                    field.Name, oldField.DataType, columnType);
            }

            string oldFieldDefaultValue = GetFieldDefaultValue(oldField, oldType);
            var oldColumnType = oldType.GetSqlType(oldField.Size, oldField.Precision);

            return (oldField.AllowEmpty != field.AllowEmpty) ||
                   (oldColumnType != columnType) ||
                   (oldFieldDefaultValue != columnDefaultValue) ||
                   (oldField.Name != field.Name) ||
                   (oldField.External != field.External);
        }


        /// <summary>
        /// Gets data type default value in case that field doesn't allow null and original default value is not set. 
        /// If field allows null value returns original default value.
        /// </summary>
        /// <param name="field">Form definition field</param>
        /// <param name="dataType">Data type of the field</param>
        private static string GetFieldDefaultValue(IField field, DataType dataType)
        {
            string columnDefaultValue = field.DefaultValue;
            if (((columnDefaultValue == null) && !field.AllowEmpty) || ContainsMacro(columnDefaultValue))
            {
                // Use type default value
                columnDefaultValue = dataType.GetString(dataType.ObjectDefaultValue, CultureHelper.EnglishCulture);
            }

            return columnDefaultValue;
        }


        /// <summary>
        /// Returns true if the specified text contains macro.
        /// </summary>
        /// <param name="inputText">Text to check</param>
        private static bool ContainsMacro(string inputText)
        {
            // Quick check for macro start
            if ((inputText == null) || !inputText.Contains("{"))
            {
                return false;
            }

            // Check all data macros (ony data, query and localization macros are supported since v8)
            char[] types = { '%', '?', '$' };
            return types.Any(type => ContainsMacroType(inputText, type));
        }


        /// <summary>
        /// Checks whether given text contains specified macro type
        /// </summary>
        /// <param name="inputText">Text to check</param>
        /// <param name="type">Type of the macro (%, $, ?)</param>
        private static bool ContainsMacroType(string inputText, char type)
        {
            int index = inputText.IndexOf("{" + type, StringComparison.Ordinal);
            if (index >= 0)
            {
                return inputText.IndexOf(type + "}", index + 2, StringComparison.Ordinal) >= 0;
            }
            return false;
        }


        /// <summary>
        /// Gets old fields for class info
        /// </summary>
        /// <param name="tableName">Class table name</param>
        /// <param name="definition">Form definition</param>
        /// <param name="loadOldDefinition">Indicates if old form definition should be loaded</param>
        private IEnumerable<IField> GetOldFields(string tableName, string definition, bool loadOldDefinition)
        {
            var dbFields = GetDatabaseFields(tableName);

            if (!loadOldDefinition || String.IsNullOrEmpty(definition))
            {
                // Use definition only from plain database table as a source
                return dbFields;
            }

            var definitionFields = GetDataDefinition(definition)
                .GetFields<IField>()
                .ToDictionary(f => f.Name, f => f, StringComparer.InvariantCultureIgnoreCase);

            return dbFields.Select(dbField =>
                {
                    // Use class field additional properties missing in plain database schema to evaluate changes correctly
                    IField definitionField;

                    // Note that the database can be out of sync with the definition, e.g. upgrade SQL script already handled the column changes
                    return definitionFields.TryGetValue(dbField.Name, out definitionField) ? definitionField : dbField;
                })

                // Return also fields that never have database representation so they can be processed
                .Concat(definitionFields.Values.Where(field => field.External || field.IsDummyField));
        }


        private static bool FieldsAreTheSame(IField x, IField y)
        {
            // Fall back to GUID in case the name has been changed
            return x.Name.Equals(y.Name, StringComparison.InvariantCulture) || x.Guid == y.Guid;
        }


        /// <summary>
        /// Gets fields based on database structure
        /// </summary>
        /// <param name="tableName">Table name</param>
        private IEnumerable<IField> GetDatabaseFields(string tableName)
        {
            var dbDefinition = ObjectFactory<DataDefinition>.New();
            dbDefinition.LoadFromDataStructure(tableName, this);

            return dbDefinition.GetFields<IField>();
        }


        /// <summary>
        /// Gets data definition from XML definition
        /// </summary>
        /// <param name="definition">XML definition</param>
        private static DataDefinition GetDataDefinition(string definition)
        {
            var classDefinition = ObjectFactory<DataDefinition>.New();
            classDefinition.LoadFromDefinition(definition);
            return classDefinition;
        }


        /// <summary>
        /// Deletes data from specified table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="where">Where condition, null if no condition is needed</param>
        public void DeleteDataFromTable(string tableName, string where)
        {
            TableManagerObject.DeleteDataFromTable(tableName, where);
        }


        /// <summary>
        /// Determines whether specified DB table exists or not.
        /// </summary>
        /// <param name="tableName">Table name to check</param>
        public bool TableExists(string tableName)
        {
            return TableManagerObject.TableExists(tableName);
        }


        /// <summary>
        /// Returns unique table name (automatically generated table name that not yet exist in the database).
        /// </summary>
        /// <param name="originalTableName">Original table name</param>
        public string GetUniqueTableName(string originalTableName)
        {
            string newName = originalTableName;
            int index = 0;

            // Create name
            while (TableExists(newName))
            {
                index++;
                newName = originalTableName + "_" + index;
            }

            return newName;
        }


        /// <summary>
        /// Reclaims space from dropped variable-length columns in tables or indexed views.
        /// </summary>
        /// <param name="tableName">Table name</param>
        private void CleanTable(string tableName)
        {
            string query = String.Format("ALTER TABLE {0} REBUILD", tableName);
            TableManagerObject.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }

        #endregion


        #region "Database management"

        /// <summary>
        /// Changes database object owner.
        /// </summary>
        /// <param name="dbObject">Database object name</param>
        /// <param name="newOwner">New owner name</param>
        public void ChangeDBObjectOwner(string dbObject, string newOwner)
        {
            TableManagerObject.ChangeDBObjectOwner(dbObject, newOwner);
        }

        #endregion


        #region "Column management"

        /// <summary>
        /// Returns list of column names which represent primary keys of the specified database table.
        /// Returns empty list if primary keys are not found.
        /// </summary>
        /// <param name="tableName">Database table name</param>
        public List<string> GetPrimaryKeyColumns(string tableName)
        {
            return TableManagerObject.GetPrimaryKeyColumns(tableName);
        }


        /// <summary>
        /// Returns DataSet with specified table column information retrieved from database information schema. Returns columns ColumnName, DataType, DataSize, DataPrecision, Nullable, DefaultValue.
        /// </summary>
        /// <param name="tableName">Database table name</param>
        /// <param name="columnName">Database table column name</param>
        /// <remarks>If <paramref name="columnName"/> is not specified data for all table columns are returned.</remarks>
        public DataSet GetColumnInformation(string tableName, string columnName = null)
        {
            return TableManagerObject.GetColumnInformation(tableName, columnName);
        }


        /// <summary>
        /// Add column to specified table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Name of a new column</param>
        /// <param name="columnType">Type of a new column</param>
        /// <param name="allowNull">Allow NULL values in new column or not</param>
        /// <param name="defaultValue">Default value of the column in system (en) culture. Null if no default value is set</param>
        /// <param name="forceDefaultValue">Indicates if column default value should be set if column doesn't allow NULL values</param>
        public void AddTableColumn(string tableName, string columnName, string columnType, bool allowNull, string defaultValue, bool forceDefaultValue = true)
        {
            TableManagerObject.AddTableColumn(tableName, columnName, columnType, allowNull, defaultValue, forceDefaultValue);
        }


        /// <summary>
        /// Remove column from specified table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Name of column to remove</param>
        public void DropTableColumn(string tableName, string columnName)
        {
            TableManagerObject.DropTableColumn(tableName, columnName);
        }


        /// <summary>
        /// Rename, retype or allow/not allow NULL values in column
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Name of an old column</param>
        /// <param name="newColumnName">Name of a new column</param>
        /// <param name="newColumnType">Type of a new column</param>
        /// <param name="newColumnDefaultValue">Default value of a new column in system (en) culture</param>
        /// <param name="newColumnAllowNull">Allow NULL values in new column or not</param>
        public void AlterTableColumn(string tableName, string columnName, string newColumnName, string newColumnType, bool newColumnAllowNull, string newColumnDefaultValue)
        {
            TableManagerObject.AlterTableColumn(tableName, columnName, newColumnName, newColumnType, newColumnAllowNull, newColumnDefaultValue);
        }


        /// <summary>
        /// Returns the DataSet of column indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        public DataSet GetColumnIndexes(string tableName, string columnName)
        {
            return TableManagerObject.GetColumnIndexes(tableName, columnName);
        }


        /// <summary>
        /// Drops the column indexes, returns the DataSet of indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        public DataSet DropColumnIndexes(string tableName, string columnName)
        {
            return TableManagerObject.DropColumnIndexes(tableName, columnName);
        }


        /// <summary>
        /// Creates the table indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        /// <param name="ds">DataSet with the indexes information</param>
        public void CreateColumnIndexes(string tableName, string columnName, DataSet ds)
        {
            TableManagerObject.CreateColumnIndexes(tableName, columnName, ds);
        }


        /// <summary>
        /// Drops the default constraint.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        public void DropDefaultConstraint(string tableName, string columnName)
        {
            TableManagerObject.DropDefaultConstraint(tableName, columnName);
        }


        /// <summary>
        /// Returns the name of the PK constraint.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        public string GetPKConstraintName(string tableName)
        {
            return TableManagerObject.GetPKConstraintName(tableName);
        }


        /// <summary>
        /// Drops the current PK constraint and creates new from given columns.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="primaryKeyColumns">List of columns which should be part of primary key</param>
        public void RecreatePKConstraint(string tableName, string[] primaryKeyColumns)
        {
            TableManagerObject.RecreatePKConstraint(tableName, primaryKeyColumns);
        }


        /// <summary>
        /// Checks if column name is unique in given view.
        /// </summary>
        /// <param name="viewName">Name of the view</param>
        /// <param name="columnName">Name of the column to be checked</param>
        public bool ColumnExistsInView(string viewName, string columnName)
        {
            return TableManagerObject.ColumnExistsInView(viewName, columnName);
        }

        #endregion


        #region "Common methods"

        private ITableManager GetTableManager()
        {
            // Try to get cached object
            TableManagerDictionary managers = mTableManagers.Value;

            ITableManager man = managers[ConnectionString];
            if (man == null)
            {
                // Create new table manager
                man = DataConnectionFactory.NewTableManagerObject(ConnectionString);
                managers[ConnectionString] = man;
            }

            return man;
        }


        /// <summary>
        /// Returns name of the primary key. If more columns in PK, names are separated by semicolon ";".
        /// </summary>
        /// <param name="tableName">Name of the table to get PK column(s) from.</param>
        public string GetTablePKName(string tableName)
        {
            return TableManagerObject.GetTablePKName(tableName);
        }


        /// <summary>
        /// Returns the float string using the database culture.
        /// </summary>
        /// <param name="value">Value to convert</param>
        public static string GetValueString(object value)
        {
            return DefaultTableManagerObject.GetValueString(value);
        }


        /// <summary>
        /// Selects single field node with the specified attribute value.
        /// </summary>
        /// <param name="formNode">Xml node with field nodes representing table columns</param>
        /// <param name="attributeName">Attribute name of the field node to be selected</param>
        /// <param name="attributeValue">Attribute value of the field node to be selected</param>
        public static XmlNode SelectFieldNode(XmlNode formNode, string attributeName, string attributeValue)
        {
            if ((formNode != null) && (attributeName != null) && (attributeValue != null))
            {
                return formNode.SelectSingleNode(String.Format("field[translate(@{0},'ABCDEFGHIJKLMNOPQRSTUVWXYZ', 'abcdefghijklmnopqrstuvwxyz') ='{1}']", attributeName, attributeValue.ToLowerInvariant()));
            }
            return null;
        }

        #endregion


        #region "Managing views and stored procedures"

        /// <summary>
        /// Returns SQL code of specified view or stored procedure.
        /// </summary>
        /// <param name="name">Name of the view or stored procedure</param>
        public string GetCode(string name)
        {
            return TableManagerObject.GetCode(name);
        }


        /// <summary>
        /// Determines whether specified stored procedure exists or not.
        /// </summary>
        /// <param name="procName">Name of the stored procedure</param>
        public bool StoredProcedureExists(string procName)
        {
            return TableManagerObject.StoredProcedureExists(procName);
        }


        /// <summary>
        /// Returns list of views or stored procedures.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="columns">Columns</param>
        /// <param name="getViews">If true list of views is retrieved</param>
        public DataSet GetList(string where, string columns, bool getViews)
        {
            return TableManagerObject.GetList(where, columns, getViews);
        }


        /// <summary>
        /// Removes view or stored procedure from database.
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="isView">Indicates if view is deleted</param>
        public void DeleteObject(string name, bool isView)
        {
            TableManagerObject.DeleteObject(name, isView);
        }


        /// <summary>
        /// Creates specified procedure in database
        /// </summary>
        /// <param name="procName">Procedure name to create</param>
        /// <param name="param">Parameters</param>
        /// <param name="body">Procedure body</param>
        /// <param name="schema">Database schema</param>
        public void CreateProcedure(string procName, string param, string body, string schema = null)
        {
            TableManagerObject.CreateProcedure(procName, param, body, schema);
        }


        /// <summary>
        /// Alters specified procedure in database
        /// </summary>
        /// <param name="procName">Procedure name to create</param>
        /// <param name="param">Parameters</param>
        /// <param name="body">Procedure body</param>
        /// <param name="schema">Database schema</param>
        public void AlterProcedure(string procName, string param, string body, string schema = null)
        {
            TableManagerObject.AlterProcedure(procName, param, body, schema);
        }

        #endregion
    }
}
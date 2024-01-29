using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;

using CMS.Helpers;
using CMS.Base;
using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Ensures management of database table and table column.
    /// </summary>
    public abstract class AbstractTableManager : ITableManager, ITableIndexInformationProvider
    {
        #region "Variables"

        /// <summary>
        /// Database culture setting from the web.config.
        /// </summary>
        protected string mDatabaseCulture;

        /// <summary>
        /// Database name
        /// </summary>
        private string mDatabaseName;

        /// <summary>
        /// Database server name
        /// </summary>
        private string mDatabaseServerName;

        /// <summary>
        /// Database server version
        /// </summary>
        private string mDatabaseServerVersion;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the text for N/A message
        /// </summary>
        private static string NotAvailableMessage
        {
            get
            {
                return CoreServices.Localization.GetString("general.na");
            }
        }



        /// <summary>
        /// Database culture setting from the web.config.
        /// </summary>
        public virtual string DatabaseCulture
        {
            get
            {
                return mDatabaseCulture ?? (mDatabaseCulture = ValidationHelper.GetString(SettingsHelper.AppSettings["CMSDatabaseCulture"], "en-us"));
            }
        }


        /// <summary>
        /// Database name.
        /// </summary>
        public virtual string DatabaseName
        {
            get
            {
                return mDatabaseName ?? (mDatabaseName = GetDatabaseName());
            }
        }


        /// <summary>
        /// Name of database server.
        /// </summary>
        public virtual string DatabaseServerName
        {
            get
            {
                return mDatabaseServerName ?? (mDatabaseServerName = GetDatabaseServerName());
            }
        }


        /// <summary>
        /// Version of database server.
        /// </summary>
        public virtual string DatabaseServerVersion
        {
            get
            {
                return mDatabaseServerVersion ?? (mDatabaseServerVersion = GetDatabaseServerVersion());
            }
        }


        /// <summary>
        /// Database size(including log size) in MB.
        /// </summary>
        public virtual string DatabaseSize
        {
            get
            {
                return GetDatabaseSize(DatabaseCulture);
            }
        }


        /// <summary>
        /// Database version
        /// </summary>
        public virtual string DatabaseVersion
        {
            get
            {
                return GetDatabaseVersion();
            }
        }


        /// <summary>
        /// Connection string to use for table management
        /// </summary>
        public string ConnectionString
        {
            get;
            set;
        }


        /// <summary>
        /// If true, the debug is disabled in this table manager
        /// </summary>
        public bool DisableDebug
        {
            get;
            set;
        }

        #endregion


        #region "Connection management"

        /// <summary>
        /// Gets the connection for the table management
        /// </summary>
        protected virtual IDataConnection GetConnection()
        {
            var genConn = ConnectionHelper.GetConnection();

            var conn = genConn.GetExecutingConnection(ConnectionString);

            conn.DisableConnectionDebug = DisableDebug;

            return conn;
        }


        /// <summary>
        /// Returns XML schema for specified table.
        /// </summary>
        /// <param name="tableName">Name of a table to get xml schema for</param>
        public virtual string GetXmlSchema(string tableName)
        {
            // Create the connection and get the schema
            var conn = GetConnection();

            return conn.GetXmlSchema(tableName);
        }


        /// <summary>
        /// Executes query and returns the results in a DataSet.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="queryType">Query type</param>
        public virtual object ExecuteScalar(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType)
        {
            var conn = GetConnection();

            // Log to the debug
            if (!DisableDebug)
            {
                SqlDebug.LogQueryStart(null, queryText, queryParams, conn);
            }

            var result = conn.ExecuteScalar(queryText, queryParams, queryType, false);

            if (!DisableDebug)
            {
                SqlDebug.LogQueryEnd(result);
            }

            return result;
        }


        /// <summary>
        /// Executes query and returns the results in a DataSet.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="queryType">Query type</param>
        public virtual DataSet ExecuteQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType)
        {
            var conn = GetConnection();

            // Log to the debug
            if (!DisableDebug)
            {
                SqlDebug.LogQueryStart(null, queryText, queryParams, conn);
            }

            var result = conn.ExecuteQuery(queryText, queryParams, queryType, false);

            if (!DisableDebug)
            {
                SqlDebug.LogQueryEnd(result);
            }

            return result;
        }

        #endregion


        #region "View management"

        /// <summary>
        /// Creates specified view in database
        /// </summary>
        /// <param name="viewName">View name to create</param>
        /// <param name="selectExpression">Select expression for the view</param>
        /// <param name="indexed">If true, the view is indexed (schema bound)</param>
        /// <param name="schema">Database schema</param>
        public virtual void CreateView(string viewName, string selectExpression, bool indexed, string schema)
        {
            SetView(viewName, selectExpression, indexed, schema, true);
        }


        /// <summary>
        /// Alters specified view in database
        /// </summary>
        /// <param name="viewName">View name to alter</param>
        /// <param name="selectExpression">Select expression for the view</param>
        /// <param name="indexed">If true, the view is indexed (schema bound)</param>
        /// <param name="schema">Database schema</param>
        public virtual void AlterView(string viewName, string selectExpression, bool indexed, string schema)
        {
            SetView(viewName, selectExpression, indexed, schema, false);
        }


        private void SetView(string viewName, string selectExpression, bool indexed, string schema, bool createNew)
        {
            // Ensure the view schema if given
            if (!String.IsNullOrEmpty(schema))
            {
                viewName = SqlHelper.GetSafeOwner(schema) + "." + SqlHelper.RemoveOwner(viewName);
            }

            var query = (createNew ? "CREATE" : "ALTER") + " VIEW " + viewName + (indexed ? " WITH SCHEMABINDING" : "") + " AS \n" + selectExpression;

            ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Drop specified view from database. Returns the schema of the dropped view
        /// </summary>
        /// <param name="viewName">View name to drop</param>
        public virtual string DropView(string viewName)
        {
            viewName = SqlHelper.RemoveOwner(viewName);

            // Delete the view if exists
            string schema;

            if (ViewExists(viewName, out schema))
            {
                // Drop with specific schema
                if ((schema != null) && (schema != "dbo"))
                {
                    viewName = SqlHelper.GetSafeOwner(schema) + "." + viewName;
                }

                ExecuteQuery("DROP VIEW " + SqlHelper.GetSafeQueryString(viewName, false), null, QueryTypeEnum.SQLQuery);
            }

            return schema;
        }


        /// <summary>
        /// Refreshes specified view in database.
        /// </summary>
        /// <param name="viewName">View name to refresh</param>
        public virtual void RefreshView(string viewName)
        {
            viewName = SqlHelper.RemoveOwner(viewName);

            // Refresh the view if exists
            string schema;
            if (ViewExists(viewName, out schema))
            {
                // Drop with specific schema
                if ((schema != null) && (schema != "dbo"))
                {
                    viewName = schema + "." + viewName;
                }

                ExecuteQuery("EXEC sp_refreshview N'" + SqlHelper.GetSafeQueryString(viewName, false) + "'", null, QueryTypeEnum.SQLQuery);
            }
        }


        /// <summary>
        /// Determines whether specified DB view exists or not.
        /// </summary>
        /// <param name="viewName">View name to check</param>
        public virtual bool ViewExists(string viewName)
        {
            string schema;

            return ViewExists(viewName, out schema);
        }


        /// <summary>
        /// Determines whether specified DB view exists or not.
        /// </summary>
        /// <param name="viewName">View name to check</param>
        /// <param name="schema">Returns the view schema</param>
        public virtual bool ViewExists(string viewName, out string schema)
        {
            viewName = SqlHelper.RemoveOwner(viewName);

            // Get the view
            DataSet ds = ExecuteQuery("SELECT table_name, table_schema FROM INFORMATION_SCHEMA.VIEWS WHERE table_name = N'" + SqlHelper.GetSafeQueryString(viewName, false) + "'", null, QueryTypeEnum.SQLQuery);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                schema = SqlHelper.GetSafeOwner(ValidationHelper.GetString(ds.Tables[0].Rows[0][1], "dbo"));
                return true;
            }
            else
            {
                schema = null;
                return false;
            }
        }

        #endregion


        #region "Table management"

        /// <summary>
        /// Gets the tables in the current database
        /// </summary>
        /// <param name="where">Tables where condition</param>
        public virtual List<string> GetTables(string where)
        {
            var tables = new List<string>();

            // Get the data
            string query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            if (!String.IsNullOrEmpty(where))
            {
                query += " AND " + where;
            }

            var ds = ConnectionHelper.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Add column name to the list
                    string name = ValidationHelper.GetString(dr[0], "");
                    if (!String.IsNullOrEmpty(name))
                    {
                        tables.Add(name);
                    }
                }
            }

            return tables;
        }


        /// <summary>
        /// Gets list of object names which have foreign key constraint dependency.
        /// </summary>
        public virtual List<string> GetTableDependencies(string tableName)
        {
            var query = string.Format(
@"
SELECT DISTINCT OBJECT_NAME(fk.parent_object_id) AS Name
FROM sys.foreign_keys fk
WHERE fk.referenced_object_id = OBJECT_ID('{0}')
"
            , SqlHelper.EscapeQuotes(tableName));

            var ds = ConnectionHelper.ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return DataHelper.GetStringValues(ds.Tables[0], "Name");
            }

            return new List<string>();
        }


        /// <summary>
        /// Returns name of the primary key. If more columns in PK, names are separated by semicolon ";".
        /// </summary>
        /// <param name="tableName">Name of the table to get PK column(s) from.</param>
        public virtual string GetTablePKName(string tableName)
        {
            // Prepare query for obtaining primary key columns
            string sql = string.Format(
@"
SELECT c.name
    FROM sys.indexes AS i
    INNER JOIN sys.index_columns AS ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
    INNER JOIN sys.columns AS c ON i.object_id = c.object_id AND c.column_id = ic.column_id
        WHERE i.is_primary_key = 1 AND i.object_id = OBJECT_ID('{0}');
"
            , SqlHelper.EscapeQuotes(tableName));

            // Get list of primary key columns
            DataSet pkColumnsDs = ExecuteQuery(sql, null, QueryTypeEnum.SQLQuery);

            string columns = "";
            if (!DataHelper.DataSourceIsEmpty(pkColumnsDs))
            {
                foreach (DataRow dr in pkColumnsDs.Tables[0].Rows)
                {
                    // Get column name
                    string col = ValidationHelper.GetString(dr[0], "");

                    // Concatenate column names with semicolon
                    if (string.IsNullOrEmpty(columns))
                    {
                        columns = col;
                    }
                    else
                    {
                        columns += ";" + col;
                    }
                }
            }

            return columns;
        }


        /// <summary>
        /// Returns DataSet with indexes of the given object. Returns columns IndexName, DropScript, CreateScript
        /// </summary>
        /// <param name="objectName">Object name</param>
        public DataSet GetIndexes(string objectName)
        {
            var parameters = new QueryDataParameters();

            parameters.Add("Table", objectName);
            parameters.Add("Column", null);

            return ExecuteQuery("Proc_System_GetIndexes", parameters, QueryTypeEnum.StoredProcedure);
        }


        /// <summary>
        /// Gets indexes of table identified by given <paramref name="tableName"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tableName"/> is null or empty string.</exception>
        ITableIndexes ITableIndexInformationProvider.GetTableIndexes(string tableName)
        {
            if (String.IsNullOrEmpty(tableName))
            {
                throw new ArgumentException("Table name must be provided.", "tableName");
            }
            
            var indexes = GetIndexesCore(tableName);
            
            return new TableIndexes(indexes);
        }


        /// <summary>
        /// Gets dictionary of all enabled indexes of given table. Item key is index ID which is relative within table.
        /// See https://msdn.microsoft.com/en-us/library/ms173760.aspx for details on index ID.
        /// </summary>
        private Dictionary<int, Index> GetIndexesCore(string tableName)
        {
            Dictionary<int, Index> indexes = new Dictionary<int, Index>();

            using (var data = GetIndexesDataSet(tableName))
            {
                var lastIndexId = -1;
                List<IndexColumn> lastIndexColumns = null;

                foreach (DataRow row in data.Tables[0].Rows)
                {
                    var indexId = (int)row["IndexId"];
                    if (indexId == lastIndexId)
                    {
                        // Index is defined on multiple columns, append column to last index
                        lastIndexColumns.Add(CreateIndexColumn(row));
                    }
                    else
                    {
                        lastIndexId = indexId;
                        lastIndexColumns = new List<IndexColumn>
                        {
                            CreateIndexColumn(row)
                        };

                        var indexName = (string)row["IndexName"];
                        var indexType = (byte)row["IndexType"];
                        var isUnique = (bool)row["IsUnique"];
                        var isPrimaryKey = (bool)row["IsPrimaryKey"];

                        var index = new Index(indexName, indexType, isUnique, isPrimaryKey, lastIndexColumns.AsReadOnly());
                        indexes.Add(indexId, index);
                    }        
                }
            }

            return indexes;
        }


        private IndexColumn CreateIndexColumn(DataRow row)
        {
            var columnName = (string)row["ColumnName"];
            var isDescendingKey = (bool)row["IsDescendingKey"];
            var isIncludedColumn = (bool)row["IsIncludedColumn"];

            return new IndexColumn(columnName, isDescendingKey, isIncludedColumn);
        }


        /// <summary>
        /// Gets information about all enabled indexes for given table. The result is ordered by index ID and index column ID.
        /// </summary>
        private DataSet GetIndexesDataSet(string tableName)
        {
            var parameters = new QueryDataParameters
            {
                { "TableName", tableName }
            };

            const string query = @"
SELECT
    IndexId = i.index_id,
	IndexName = i.name,
    IndexType = i.type,
	IsUnique = i.is_unique,
	IsPrimaryKey = i.is_primary_key,
	ColumnName = c.name,
	IsDescendingKey = ic.is_descending_key,
	IsIncludedColumn = ic.is_included_column
FROM
	sys.tables t
INNER JOIN
	sys.indexes i ON t.object_id = i.object_id
INNER JOIN
	sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
INNER JOIN 
    sys.columns c ON ic.column_id = c.column_id AND ic.object_id = c.object_id

WHERE t.name = @TableName
	AND i.is_disabled = 0 -- Omit disabled indexes
ORDER BY ic.index_id, ic.index_column_id
";
            return ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Creates specified table in database.
        /// </summary>
        /// <param name="tableName">Table name to create</param>
        /// <param name="primaryKeyName">Primary key of table to create</param>
        public virtual void CreateTable(string tableName, string primaryKeyName)
        {
            CreateTable(tableName, primaryKeyName, true);
        }


        /// <summary>
        /// Creates specified table in database.
        /// </summary>
        /// <param name="tableName">Table name to create</param>
        /// <param name="primaryKeyName">Primary key of table to create</param>
        /// <param name="setIdentity">If true, sets identity on primary key column</param>
        public virtual void CreateTable(string tableName, string primaryKeyName, bool setIdentity)
        {
            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            string createPK = "ALTER TABLE " + tableName + " ADD CONSTRAINT PK_" + tableName + " PRIMARY KEY CLUSTERED ([" + primaryKeyName + "]);";
            ExecuteQuery("CREATE TABLE " + tableName + " ([" + primaryKeyName + "] int NOT NULL " + (setIdentity ? "IDENTITY (1,1)" : "") + "); " + createPK, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Changes name of the table with original name according to the new name.
        /// </summary>
        /// <param name="oldTableName">Name of the table to rename</param>
        /// <param name="newTableName">New name of the table</param>
        public virtual void RenameTable(string oldTableName, string newTableName)
        {
            try
            {
                ExecuteQuery("EXEC sp_rename '" + oldTableName + "', [" + newTableName + "]", null, QueryTypeEnum.SQLQuery);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred during renaming table in database.", ex);
            }
        }


        /// <summary>
        /// Drop specified table from database.
        /// </summary>
        /// <param name="tableName">Table name to drop</param>
        public virtual void DropTable(string tableName)
        {
            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            ExecuteQuery("DROP TABLE " + tableName, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Deletes data from specified table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="where">Where condition, null if no condition is needed</param>
        public virtual void DeleteDataFromTable(string tableName, string where)
        {
            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            // Prepare the query
            string query = "DELETE FROM " + tableName;
            if (where != null)
            {
                query += " WHERE " + where;
            }

            // Delete data
            ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Determines whether specified DB table exists or not.
        /// </summary>
        /// <param name="tableName">Table name to check</param>
        public virtual bool TableExists(string tableName)
        {
            try
            {
                // Remove owner from table name
                tableName = SqlHelper.RemoveOwner(tableName);

                var parameters = new QueryDataParameters {{"tableName", tableName}};
                var existingObjects = ExecuteQuery("SELECT 1 FROM sys.objects WHERE name = @tableName AND type = 'U'", parameters, QueryTypeEnum.SQLQuery);
                            
                return !DataHelper.DataSourceIsEmpty(existingObjects);
            }
            catch
            {
                return false;
            }
        }

        #endregion


        #region "Column management"

        /// <summary>
        /// Returns list of column names which represent primary keys of the specified database table.
        /// Returns empty list if primary keys are not found.
        /// </summary>
        /// <param name="tableName">Database table name</param>
        public virtual List<string> GetPrimaryKeyColumns(string tableName)
        {
            List<string> columns = new List<string>();

            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            // Prepare parameters
            string query = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.CONSTRAINT_COLUMN_USAGE ccu WHERE EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc WHERE CONSTRAINT_TYPE = 'PRIMARY KEY' AND ccu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME) AND TABLE_NAME = @TableName ";

            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@TableName", tableName);

            // Get the data
            DataSet ds = ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                foreach (DataRow dr in ds.Tables[0].Rows)
                {
                    // Add column name to the list
                    string name = ValidationHelper.GetString(dr[0], "");
                    if (!String.IsNullOrEmpty(name))
                    {
                        columns.Add(name);
                    }
                }
            }

            return columns;
        }


        /// <summary>
        /// Returns DataSet with specified table column information retrieved from database information schema. Returns columns ColumnName, DataType, DataSize, DataPrecision, Nullable, DefaultValue.
        /// </summary>
        /// <param name="tableName">Database table name</param>
        /// <param name="columnName">Database table column name</param>
        /// <remarks>If <paramref name="columnName"/> is not specified data for all table columns are returned.</remarks>
        public virtual DataSet GetColumnInformation(string tableName, string columnName)
        {
            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            // Build where condition
            string where = "TABLE_NAME = '" + SqlHelper.GetSafeQueryString(tableName, false) + "'";
            if (!string.IsNullOrEmpty(columnName))
            {
                where += " AND COLUMN_NAME = '" + SqlHelper.GetSafeQueryString(columnName, false) + "'";
            }

            // Build query
            string query =
@"
SELECT 
    COLUMN_NAME AS ColumnName, DATA_TYPE AS DataType, 
    COALESCE(CHARACTER_MAXIMUM_LENGTH, NUMERIC_PRECISION) AS DataSize, 
    COALESCE(NUMERIC_SCALE, DATETIME_PRECISION) AS DataPrecision, 
    IS_NULLABLE AS Nullable, COLUMN_DEFAULT AS DefaultValue 
        FROM INFORMATION_SCHEMA.COLUMNS 
            WHERE " + where + @" ORDER BY ORDINAL_POSITION
";

            // Get the data
            return ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
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
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/>, <paramref name="columnName"/> or <paramref name="columnType"/> is null.</exception>
        public virtual void AddTableColumn(string tableName, string columnName, string columnType, bool allowNull, string defaultValue, bool forceDefaultValue = true)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }

            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }

            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            // NULL
            string stringNull = (allowNull) ? "NULL" : "NOT NULL";

            // Default value
            string stringDefValue = String.Empty;
            if ((defaultValue != null) || (!allowNull && forceDefaultValue))
            {
                defaultValue = EnsureDefaultValue(allowNull, columnType, defaultValue);
                stringDefValue = "CONSTRAINT " + GetConstraintName(tableName, columnName) + " DEFAULT " + defaultValue;
            }

            // Add column
            ExecuteQuery("ALTER TABLE " + tableName + " ADD [" + columnName + "] " + columnType + " " + stringNull + " " + stringDefValue, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Remove column from specified table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Name of column to remove</param>
        public virtual void DropTableColumn(string tableName, string columnName)
        {
            using (var tr = CreateTransactionScope())
            {
                // Remove owner from table name
                tableName = SqlHelper.RemoveOwner(tableName);

                // Drop default constraint
                DropDefaultConstraint(tableName, columnName);

                // Drop the indexes
                DropColumnIndexes(tableName, columnName);

                string query = "ALTER TABLE " + tableName + " DROP COLUMN [" + columnName + "]";
                ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);

                tr.Commit();
            }
        }


        /// <summary>
        /// Returns the DataSet of column indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        public virtual DataSet GetColumnIndexes(string tableName, string columnName)
        {
            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            // Prepare the query
            string query = @"
SELECT IndexName = ind.name, 
    ColumnName = col.name, 
    TableName = t.name, 
    IsStatistics = indexproperty(ind.object_id, ind.name, 'IsStatistics'), 
    IsHypothetical = indexproperty(ind.object_id, ind.name, 'IsHypothetical') , 
    IsClustered = indexproperty(ind.object_id, ind.name, 'IsClustered'),
    IsPrimaryKey = ind.is_primary_key,
    IsUnique = ind.is_unique,
    IsUniqueConstraint = ind.is_unique_constraint,
    IndexID = ind.index_id, 
    ColumnOrder = ic.key_ordinal,
    IsDescending = ic.is_descending_key,
    FilterDefinition = ind.filter_definition,
    ColumnIsIncluded = ic.is_included_column
FROM  sys.indexes ind
INNER JOIN sys.index_columns ic ON ind.object_id = ic.object_id AND ind.index_id = ic.index_id
INNER JOIN sys.columns col ON ic.object_id = col.object_id AND ic.column_id = col.column_id 
INNER JOIN sys.tables t ON ind.object_id = t.object_id
WHERE t.is_ms_shipped = 0 AND
      t.name = '{0}' AND
      ind.index_id IN
    (
    SELECT ind.index_id
	FROM  sys.indexes ind
	INNER JOIN sys.index_columns ic ON ind.object_id = ic.object_id AND ind.index_id = ic.index_id
	INNER JOIN sys.columns col ON ic.object_id = col.object_id AND ic.column_id = col.column_id 
	INNER JOIN sys.tables t ON ind.object_id = t.object_id
	WHERE t.is_ms_shipped = 0 AND t.name = '{0}' AND col.name = '{1}'
    )
ORDER BY ind.name, ind.index_id, ic.key_ordinal
";
            query = string.Format(query, SqlHelper.GetSafeQueryString(tableName, false), SqlHelper.GetSafeQueryString(columnName, false));

            // Get the data
            return ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Drops the column indexes, returns the DataSet of indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        public virtual DataSet DropColumnIndexes(string tableName, string columnName)
        {
            using (var tr = CreateTransactionScope())
            {
                // Remove owner from table name
                tableName = SqlHelper.RemoveOwner(tableName);

                // Get the indexes
                DataSet indexesDS = GetColumnIndexes(tableName, columnName);
                if (!DataHelper.DataSourceIsEmpty(indexesDS))
                {
                    Hashtable dropped = new Hashtable();

                    // Drop the indexes
                    foreach (DataRow dr in indexesDS.Tables[0].Rows)
                    {
                        string indexName = Convert.ToString(dr["IndexName"]);

                        // Drop only indexes which have not been dropped yet
                        if (!dropped.ContainsKey(indexName))
                        {
                            bool statistics = ValidationHelper.GetBoolean(dr["IsStatistics"], false);

                            string query;
                            if (statistics)
                            {
                                // Drop the statistics
                                query = "DROP STATISTICS " + tableName + "." + indexName;
                            }
                            else
                            {
                                // Drop the index
                                query = string.Format("DROP INDEX {1} ON {0}", tableName, indexName);
                            }

                            ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);

                            // Mark as dropped
                            dropped.Add(indexName, null);
                        }
                    }
                }

                tr.Commit();

                return indexesDS;
            }
        }


        /// <summary>
        /// Creates the table indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        /// <param name="ds">DataSet with the indexes information</param>
        public virtual void CreateColumnIndexes(string tableName, string columnName, DataSet ds)
        {
            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            if (DataHelper.DataSourceIsEmpty(ds))
            {
                return;
            }
            
            Dictionary<string, ColumnIndexDefinition> indexDefinitions = new Dictionary<string, ColumnIndexDefinition>();

            // Prepare 'create index...' queries
            foreach (DataRow dr in ds.Tables[0].Rows)
            {
                string indexName = Convert.ToString(dr["IndexName"]);
                string column = ValidationHelper.GetString(dr["ColumnName"], null);
                bool statistics = ValidationHelper.GetBoolean(dr["IsStatistics"], false);
                
                if (statistics || string.IsNullOrEmpty(column))
                {
                    continue;
                }

                // Prepare descending flag
                bool descending = ValidationHelper.GetBoolean(dr["IsDescending"], false);
                string desc = descending ? SqlHelper.ORDERBY_DESC : "";

                bool columnIsIncluded = ValidationHelper.GetBoolean(dr["ColumnIsIncluded"], false);
                
                // Check if the beginning of the query exists
                if (indexDefinitions.ContainsKey(indexName))
                {
                    indexDefinitions[indexName].AddColumn(column + desc, columnIsIncluded);
                }
                else
                {
                    bool clustered = ValidationHelper.GetBoolean(dr["IsClustered"], false);
                    bool unique = ValidationHelper.GetBoolean(dr["IsUnique"], false);
                    string filterDefinition = ValidationHelper.GetString(dr["FilterDefinition"], String.Empty);

                    var indexDefinition = new ColumnIndexDefinition
                    {
                        IsClustered = clustered,
                        IsUnique = unique,
                        IndexName = indexName,
                        TableName = tableName,
                        Condition = filterDefinition
                    };

                    indexDefinition.AddColumn(column + desc, columnIsIncluded);

                    indexDefinitions.Add(indexName, indexDefinition);
                }
            }

            using (var tr = CreateTransactionScope())
            {
                foreach (string query in indexDefinitions.Values.Select(indexDefinition => indexDefinition.GetQuery()))
                {
                    ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
                }

                tr.Commit();
            }
        }


        /// <summary>
        /// Drops the default constraint.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        public virtual void DropDefaultConstraint(string tableName, string columnName)
        {
            using (var tr = CreateTransactionScope())
            {
                // Ensure table name is in correct format
                string dbObjectName = tableName;
                if (tableName.Contains("."))
                {
                    dbObjectName = (tableName.Split('.'))[1];
                }

                // Get default constraint name
                var parameters = new QueryDataParameters {{"@table", dbObjectName}, {"@column", columnName}};
                var constraint = ExecuteScalar("SELECT OBJECT_NAME(default_object_id) FROM sys.columns WHERE [object_id] = OBJECT_ID(@table) AND [name] = @column", parameters, QueryTypeEnum.SQLQuery) as string;
                if (!string.IsNullOrEmpty(constraint))
                {
                    // Remove default constraint
                    var query = "ALTER TABLE " + tableName + " DROP CONSTRAINT " + constraint;
                    ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
                }

                tr.Commit();
            }
        }


        /// <summary>
        /// Alter table column with default value.
        /// </summary>        
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        /// <param name="newColumnName">New column name, null if no new column is created</param>
        /// <param name="newColumnType">New column type, null if no new column is created</param>
        /// <param name="newColumnAllowNull">Allow NULL values in new column or not</param>
        /// <param name="newColumnDefaultValue">Column default value in system (en) culture</param>
        /// <exception cref="ArgumentNullException"><paramref name="tableName"/>, <paramref name="columnName"/> or <paramref name="newColumnType"/> is null.</exception>
        public virtual void AlterTableColumn(string tableName, string columnName, string newColumnName, string newColumnType, bool newColumnAllowNull, string newColumnDefaultValue)
        {
            if (tableName == null)
            {
                throw new ArgumentNullException("tableName");
            }
            if (columnName == null)
            {
                throw new ArgumentNullException("columnName");
            }
            if (newColumnType == null)
            {
                throw new ArgumentNullException("newColumnType");
            }

            using (var tr = CreateTransactionScope())
            {
                // Remove owner from table name
                tableName = SqlHelper.RemoveOwner(tableName);

                // Drop default constraint
                DropDefaultConstraint(tableName, columnName);

                // Drop the column indexes
                DataSet indexesDS = DropColumnIndexes(tableName, columnName);

                // Change column name
                if (RenameColumn(tableName, columnName, newColumnName))
                {
                    HandleColumnRenameInIndexData(indexesDS, columnName, newColumnName);
                    columnName = newColumnName;
                }
                
                // Change all NULL values to default values
                if (!newColumnAllowNull && (newColumnDefaultValue != null))
                {
                    //Change column type first, allow nulls
                    ChangeColumnType(tableName, columnName, newColumnType, true);

                    EnsureDefaultValueForRows(tableName, columnName, newColumnType, newColumnDefaultValue);
                }

                // Change NULL constraint here
                ChangeColumnType(tableName, columnName, newColumnType, newColumnAllowNull);

                // If new default constraint is specified - > try to add it
                if ((newColumnDefaultValue != null) || !newColumnAllowNull)
                {
                    newColumnDefaultValue = EnsureDefaultValue(newColumnAllowNull, newColumnType, newColumnDefaultValue);

                    // Add default constraint            
                    AddDefaultConstraint(tableName, columnName, newColumnDefaultValue);
                }

                // Add the indexes back
                CreateColumnIndexes(tableName, columnName, indexesDS);

                tr.Commit();
            }
        }


        private void AddDefaultConstraint(string tableName, string columnName, string defaultValue)
        {
            string createDefaultConstraint = "ALTER TABLE " + tableName + " ADD CONSTRAINT " + GetConstraintName(tableName, columnName) + " DEFAULT " + defaultValue + " FOR [" + columnName + "]";
            ExecuteQuery(createDefaultConstraint, null, QueryTypeEnum.SQLQuery);
        }


        private void HandleColumnRenameInIndexData(DataSet indexData, string oldColumnName, string newColumnName)
        {
            if (!DataHelper.DataSourceIsEmpty(indexData))
            {
                foreach (DataRow dr in indexData.Tables[0].Rows)
                {
                    var oldName = ValidationHelper.GetString(dr["ColumnName"], null);
                    if (oldName == oldColumnName)
                    {
                        dr["ColumnName"] = newColumnName;
                    }
                }
            }
        }


        private void EnsureDefaultValueForRows(string tableName, string columnName, string columnType, string defaultValue)
        {
            // Prepare the parameters
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@defValue", DataTypeManager.ConvertToSystemType(TypeEnum.SQL, columnType, defaultValue, CultureHelper.EnglishCulture));

            // Run the query
            string query = "UPDATE " + tableName + " SET [" + columnName + "] = @defValue WHERE [" + columnName + "] IS NULL";
            ExecuteQuery(query, parameters, QueryTypeEnum.SQLQuery);
        }


        private void ChangeColumnType(string tableName, string columnName, string columnType, bool allowNull)
        {
            string stringNull = allowNull ? "NULL" : "NOT NULL";
            string query = "ALTER TABLE " + tableName + " ALTER COLUMN [" + columnName + "] " + columnType + " " + stringNull;
            ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Changes name of the column.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="columnName">Current name of the column</param>
        /// <param name="newColumnName">New name of the column</param>
        protected virtual bool RenameColumn(string tableName, string columnName, string newColumnName)
        {
            if (String.IsNullOrEmpty(newColumnName) || (columnName == newColumnName))
            {
                return false;
            }

            ExecuteQuery("EXEC sp_rename '" + tableName + "." + columnName + "', '" + newColumnName + "', 'COLUMN'", null, QueryTypeEnum.SQLQuery);

            return true;
        }


        /// <summary>
        /// Returns constraint's name truncated to 128 chars (128 chars is limit of database column name).
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        public static string GetConstraintName(string tableName, string columnName)
        {
            string constraintName = "DEFAULT_" + tableName.Replace('.', '_').Replace("[", "").Replace("]", "") + "_" + columnName;

            // Maximum db constraint name size is 128 chars
            return constraintName.Truncate(128).TrimEnd('_');
        }


        /// <summary>
        /// Returns the name of the primary key constraint.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        public virtual string GetPKConstraintName(string tableName)
        {
            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            string query = "SELECT TOP 1 NAME FROM sys.objects WHERE type = 'PK' AND parent_object_id = (OBJECT_ID('" + SqlHelper.GetSafeQueryString(tableName, false) + "'));";
            DataSet ds = ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ValidationHelper.GetString(ds.Tables[0].Rows[0][0], "");
            }
            return "";
        }


        /// <summary>
        /// Drops the current primary key constraint and creates new from given columns.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="primaryKeyColumns">List of columns which should be part of primary key</param>
        public virtual void RecreatePKConstraint(string tableName, string[] primaryKeyColumns)
        {
            // Remove owner from table name
            tableName = SqlHelper.RemoveOwner(tableName);

            string pkConstraintName = GetPKConstraintName(tableName).Replace("[", "").Replace("]", "");

            tableName = tableName.Replace("[", "").Replace("]", "");

            // Prepare comma separated list of column names
            string cols = "";
            foreach (string col in primaryKeyColumns)
            {
                cols += "[" + col.Replace("[", "").Replace("]", "") + "],";
            }
            cols = cols.TrimEnd(',');

            // Drop current primary key
            string query = "ALTER TABLE [" + tableName + "] DROP CONSTRAINT [" + pkConstraintName + "]; ALTER TABLE [" + tableName + "] ADD CONSTRAINT [" + pkConstraintName + "] PRIMARY KEY (" + cols + ")";
            ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Checks if column name is unique in given view.
        /// </summary>
        /// <param name="viewName">Name of the view</param>
        /// <param name="columnName">Name of the column to be checked</param>
        public virtual bool ColumnExistsInView(string viewName, string columnName)
        {
            DataSet ds = ExecuteQuery("SELECT TOP 1 * FROM " + viewName, null, QueryTypeEnum.SQLQuery);
            if (ds.Tables.Count > 0)
            {
                return ds.Tables[0].Columns.Contains(columnName);
            }
            return false;
        }

        #endregion


        #region "Database management"

        /// <summary>
        /// Changes database object owner.
        /// </summary>
        /// <param name="dbObject">Database object name</param>
        /// <param name="newOwner">New owner name</param>
        public virtual void ChangeDBObjectOwner(string dbObject, string newOwner)
        {
            QueryDataParameters parameters = new QueryDataParameters();
            parameters.Add("@objname", dbObject);
            parameters.Add("@newowner", newOwner);


            ExecuteQuery("sp_changeobjectowner", parameters, QueryTypeEnum.StoredProcedure);
        }


        /// <summary>
        /// Gets size related information about the database.
        /// </summary>
        /// <returns>DataSet with 2 tables containing related data</returns>
        protected virtual DataSet GetDatabaseInfo()
        {
            return ExecuteQuery("sp_spaceused", null, QueryTypeEnum.StoredProcedure);
        }


        /// <summary>
        /// Gets database size (including log size) or N/A string if the size cannot be retrieved
        /// </summary>
        protected virtual string GetDatabaseSize(string databaseCulture)
        {
            try
            {
                DataSet ds = GetDatabaseInfo();
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    // Get database size in megabytes
                    string dbSize = ds.Tables[0].Rows[0]["database_size"].ToString();

                    // Get size in most suitable unit
                    CultureInfo ci = CultureHelper.GetCultureInfo(databaseCulture);
                    return DataHelper.GetSizeString(Convert.ToInt64(Math.Round(Convert.ToDouble(dbSize.Remove(dbSize.Length - 4), ci) * 1024 * 1024)));
                }

                return NotAvailableMessage;
            }
            catch
            {
                return NotAvailableMessage;
            }
        }


        /// <summary>
        /// Gets the database version
        /// </summary>
        protected virtual string GetDatabaseVersion()
        {
            try
            {
                var verObj = ExecuteScalar("SELECT TOP 1 KeyValue FROM CMS_SettingsKey WHERE SiteID IS NULL AND KeyName = 'CMSDBVersion'", null, QueryTypeEnum.SQLQuery);

                return Convert.ToString(verObj);
            }
            catch
            {
                return String.Empty;
            }
        }


        /// <summary>
        /// Gets database name or N/A string if the name cannot be retrieved
        /// </summary>
        private string GetDatabaseName()
        {
            try
            {
                DataSet ds = GetDatabaseInfo();
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    return ds.Tables[0].Rows[0]["database_name"].ToString();
                }
                return NotAvailableMessage;
            }
            catch
            {
                return NotAvailableMessage;
            }
        }


        /// <summary>
        /// Gets database server name or N/A string if the name cannot be retrieved.
        /// </summary>
        private string GetDatabaseServerName()
        {
            try
            {
                DataSet ds = ExecuteQuery("select serverproperty('servername')", null, QueryTypeEnum.SQLQuery);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    return ds.Tables[0].Rows[0][0].ToString();
                }
                return NotAvailableMessage;
            }
            catch
            {
                return NotAvailableMessage;
            }
        }


        /// <summary>
        /// Gets database server version info.
        /// </summary>
        private string GetDatabaseServerVersion()
        {
            try
            {
                DataSet ds = ExecuteQuery("select serverproperty('productversion')", null, QueryTypeEnum.SQLQuery);
                if (!DataHelper.DataSourceIsEmpty(ds))
                {
                    return ds.Tables[0].Rows[0][0].ToString();
                }
                return NotAvailableMessage;
            }
            catch
            {
                return NotAvailableMessage;
            }
        }

        #endregion


        #region "Common methods"

        /// <summary>
        /// Returns the value string using the database culture. Does not include apostrophes for types that need them
        /// </summary>
        /// <param name="value">Value to convert</param>
        public virtual string GetValueString(object value)
        {
            CultureInfo cult = CultureHelper.GetCultureInfo(DatabaseCulture);

            if (value is DateTime)
            {
                return Convert.ToString(value, cult.DateTimeFormat);
            }
            else if (value is bool)
            {
                return (bool)value ? "1" : "0";
            }
            else
            {
                return Convert.ToString(value, cult.NumberFormat);
            }
        }


        /// <summary>
        /// Add apostrophes around the column default value string according to column type.
        /// </summary>
        /// <param name="allowNull">Indicates whether NULL values are allowed</param>
        /// <param name="sqlType">SQL type of the table column</param>
        /// <param name="defValue">Default to add apostrophes to</param>
        protected virtual string EnsureDefaultValue(bool allowNull, string sqlType, string defValue)
        {
            if ((defValue != null) || !allowNull)
            {
                return DataTypeManager.GetSqlValue(TypeEnum.SQL, sqlType, defValue);
            }

            return null;
        }


        /// <summary>
        /// Returns a transaction scope that can be used to maintain database consistency.
        /// </summary>
        protected virtual ITransactionScope CreateTransactionScope()
        {
            return new CMSTransactionScope();
        }

        #endregion


        #region "Managing views and stored procedures"

        /// <summary>
        /// Returns SQL code of specified view or stored procedure.
        /// </summary>
        /// <param name="name">Name of the view or stored procedure</param>
        public string GetCode(string name)
        {
            string query = String.Format("SELECT definition FROM SYS.SQL_MODULES m WHERE OBJECT_NAME(m.object_id) = N'{0}'", SqlHelper.GetSafeQueryString(name, false));

            DataSet ds = ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
            if (!DataHelper.DataSourceIsEmpty(ds))
            {
                return ValidationHelper.GetString(ds.Tables[0].Rows[0][0], null);
            }

            return null;
        }


        /// <summary>
        /// Determines whether specified stored procedure exists or not.
        /// </summary>
        /// <param name="procName">Name of the stored procedure</param>
        public bool StoredProcedureExists(string procName)
        {
            if (String.IsNullOrEmpty(procName))
            {
                return false;
            }

            // Get the objects
            string where = String.Format("ROUTINE_NAME='{0}'", SqlHelper.GetSafeQueryString(procName, false));

            DataSet ds = GetList(where, "ROUTINE_NAME", false);

            return !DataHelper.DataSourceIsEmpty(ds);
        }


        /// <summary>
        /// Returns list of views or stored procedures.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="columns">Columns</param>
        /// <param name="getViews">If true list of views is retrieved</param>
        public DataSet GetList(string where, string columns, bool getViews)
        {
            // Prepare the query
            string query = getViews ? "INFORMATION_SCHEMA.VIEWS" : "INFORMATION_SCHEMA.ROUTINES";

            if ((columns == null) || (String.IsNullOrEmpty(columns.Trim())))
            {
                columns = "*";
            }

            if ((where != null) && (!String.IsNullOrEmpty(where.Trim())))
            {
                where = " WHERE " + where;
            }

            query = String.Format("SELECT {0} FROM {1}{2}", columns, query, where);

            // Get the objects
            return ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Removes view or stored procedure from database.
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="isView">Indicates if view is deleted</param>
        public void DeleteObject(string name, bool isView)
        {
            if (String.IsNullOrEmpty(name))
            {
                return;
            }

            // Prepare the query
            name = SqlHelper.GetSafeQueryString(name, false);
            string query = String.Format("DROP {0} [{1}]", isView ? "VIEW" : "PROCEDURE", name);

            // Delete the object
            ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }


        /// <summary>
        /// Creates specified procedure in database
        /// </summary>
        /// <param name="procName">Procedure name to create</param>
        /// <param name="param">Parameters</param>
        /// <param name="body">Procedure body</param>
        /// <param name="schema">Database schema</param>
        public virtual void CreateProcedure(string procName, string param, string body, string schema)
        {
            SetProcedure(procName, param, body, schema, true);
        }


        /// <summary>
        /// Alters specified procedure in database
        /// </summary>
        /// <param name="procName">Procedure name to alter</param>
        /// <param name="param">Parameters</param>
        /// <param name="body">Procedure body</param>
        /// <param name="schema">Database schema</param>
        public virtual void AlterProcedure(string procName, string param, string body, string schema)
        {
            SetProcedure(procName, param, body, schema, false);
        }


        private void SetProcedure(string procName, string param, string body, string schema, bool createNew)
        {
            // Ensure the view schema if given
            if (!String.IsNullOrEmpty(schema))
            {
                procName = SqlHelper.GetSafeOwner(schema) + "." + SqlHelper.RemoveOwner(procName);
            }

            var query = (createNew ? "CREATE" : "ALTER") + " PROCEDURE " + procName + "\n" + param + "\nAS\nBEGIN\n" + body + "\nEND\n";

            ExecuteQuery(query, null, QueryTypeEnum.SQLQuery);
        }

        #endregion
    }
}
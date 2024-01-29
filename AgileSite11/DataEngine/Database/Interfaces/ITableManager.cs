using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.DataEngine
{
    /// <summary>
    /// Ensures management of database table and table column.
    /// </summary>
    public interface ITableManager
    {
        #region "Variables & Properties"

        /// <summary>
        /// Connection string name
        /// </summary>
        string ConnectionString
        {
            get;
            set;
        }


        /// <summary>
        /// Database culture setting from the web.config.
        /// </summary>
        string DatabaseCulture
        {
            get;
        }


        /// <summary>
        /// Gets database size(including log size).
        /// </summary>
        string DatabaseSize
        {
            get;
        }


        /// <summary>
        /// Gets database version
        /// </summary>
        string DatabaseVersion
        {
            get;
        }


        /// <summary>
        /// Gets database name.
        /// </summary>
        string DatabaseName
        {
            get;
        }


        /// <summary>
        /// Gets database server name.
        /// </summary>
        string DatabaseServerName
        {
            get;
        }


        /// <summary>
        /// Gets database server version.
        /// </summary>
        string DatabaseServerVersion
        {
            get;
        }
        

        /// <summary>
        /// If true, the debug is disabled in this table manager
        /// </summary>
        bool DisableDebug
        {
            get;
            set;
        }
        
        #endregion


        #region "View management"

        /// <summary>
        /// Creates specified view in database for given data class.
        /// </summary>
        /// <param name="viewName">View name to create</param>
        /// <param name="selectExpression">Select expression for the view</param>
        /// <param name="indexed">If true, the view is indexed (schema bound)</param>
        /// <param name="schema">Database schema</param>
        void CreateView(string viewName, string selectExpression, bool indexed, string schema);


        /// <summary>
        /// Creates specified view in database for given data class.
        /// </summary>
        /// <param name="viewName">View name to create</param>
        /// <param name="selectExpression">Select expression for the view</param>
        /// <param name="indexed">If true, the view is indexed (schema bound)</param>
        /// <param name="schema">Database schema</param>
        void AlterView(string viewName, string selectExpression, bool indexed, string schema);


        /// <summary>
        /// Drop specified view from database.
        /// </summary>
        /// <param name="viewName">View name to drop</param>
        string DropView(string viewName);


        /// <summary>
        /// Refreshes specified view in database.
        /// </summary>
        /// <param name="viewName">View name to refresh</param>
        void RefreshView(string viewName);


        /// <summary>
        /// Determines whether specified DB view exists or not.
        /// </summary>
        /// <param name="viewName">View name to check</param>
        bool ViewExists(string viewName);

        #endregion


        #region "Table management"

        /// <summary>
        /// Returns DataSet with indexes of the given object. Returns columns IndexName, DropScript, CreateScript
        /// </summary>
        /// <param name="objectName">Object name</param>
        DataSet GetIndexes(string objectName);


        /// <summary>
        /// Gets the tables in the database matching the condition
        /// </summary>
        /// <param name="where">Tables where condition</param>
        List<string> GetTables(string where);


        /// <summary>
        /// Gets list of object names which have foreign key constraint dependency.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <returns></returns>
        List<string> GetTableDependencies(string tableName); 

        /// <summary>
        /// Returns XML schema for specified table.
        /// </summary>
        /// <param name="tableName">Name of a table to get xml schema for</param>
        string GetXmlSchema(string tableName);


        /// <summary>
        /// Creates specified table in database.
        /// </summary>
        /// <param name="tableName">Table name to create</param>
        /// <param name="primaryKeyName">Primary key of table to create</param>
        void CreateTable(string tableName, string primaryKeyName);


        /// <summary>
        /// Creates specified table in database with specified primary key column with or without identity.
        /// </summary>
        /// <param name="tableName">Table name to create</param>
        /// <param name="primaryKeyName">Primary key of table to create</param>
        /// <param name="setIdentity">If true, sets identity on primary key column</param>
        void CreateTable(string tableName, string primaryKeyName, bool setIdentity);


        /// <summary>
        /// Changes name of the table with original name according to the new name.
        /// </summary>
        /// <param name="oldTableName">Name of the table to rename</param>
        /// <param name="newTableName">New name of the table</param>
        void RenameTable(string oldTableName, string newTableName);


        /// <summary>
        /// Drop specified table from database.
        /// </summary>
        /// <param name="tableName">Table name to drop</param>
        void DropTable(string tableName);


        /// <summary>
        /// Deletes data from specified table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="where">Where condition, null if no condition is needed</param>
        void DeleteDataFromTable(string tableName, string where);


        /// <summary>
        /// Determines whether specified DB table exists or not.
        /// </summary>
        /// <param name="tableName">Table name to check</param>
        bool TableExists(string tableName);

        #endregion


        #region "Database management"

        /// <summary>
        /// Changes database object owner.
        /// </summary>
        /// <param name="dbObject">Database object name</param>
        /// <param name="newOwner">New owner name</param>
        void ChangeDBObjectOwner(string dbObject, string newOwner);
        
        #endregion


        #region "Column management"

        /// <summary>
        /// Returns list of column names which represent primary keys of the specified database table.
        /// Returns empty list if primary keys are not found.
        /// </summary>
        /// <param name="tableName">Database table name</param>
        List<string> GetPrimaryKeyColumns(string tableName);


        /// <summary>
        /// Returns DataSet with specfied table column information retrieved from database information schema. Returns columns ColumnName, DataType, DataSize, DataPrecision, Nullable, DefaultValue
        /// </summary>
        /// <param name="tableName">Database table name</param>
        /// <param name="columnName">Database table column name</param>
        DataSet GetColumnInformation(string tableName, string columnName);


        /// <summary>
        /// Add column to specified table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Name of a new column</param>
        /// <param name="columnType">Type of a new column</param>
        /// <param name="allowNull">Allow NULL values in new column or not</param>
        /// <param name="defaultValue">Default value of the column in system (en) culture. Null if no default value is set</param>
        /// <param name="forceDefaultValue">Indicates if column default value should be set if column doesn't allow NULL values</param>
        void AddTableColumn(string tableName, string columnName, string columnType, bool allowNull, string defaultValue, bool forceDefaultValue);


        /// <summary>
        /// Remove column from specified table.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Name of column to remove</param>
        void DropTableColumn(string tableName, string columnName);


        /// <summary>
        /// Rename, retype or allow/not allow NULL values in column
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Name of an old column</param>
        /// <param name="newColumnName">Name of a new column</param>
        /// <param name="newColumnType">Type of a new column</param>
        /// <param name="newColumnDefaultValue">Default value of a new column in system (en) culture</param>
        /// <param name="newColumnAllowNull">Allow NULL values in new column or not</param>
        void AlterTableColumn(string tableName, string columnName, string newColumnName, string newColumnType, bool newColumnAllowNull, string newColumnDefaultValue);


        /// <summary>
        /// Returns the DataSet of column indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        DataSet GetColumnIndexes(string tableName, string columnName);


        /// <summary>
        /// Drops the column indexes, returns the DataSet of indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        DataSet DropColumnIndexes(string tableName, string columnName);


        /// <summary>
        /// Creates the table indexes.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        /// <param name="ds">DataSet with the indexes information</param>
        void CreateColumnIndexes(string tableName, string columnName, DataSet ds);


        /// <summary>
        /// Drops the default constraint.
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="columnName">Column name</param>
        void DropDefaultConstraint(string tableName, string columnName);


        /// <summary>
        /// Returns the name of the PK constraint.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        string GetPKConstraintName(string tableName);


        /// <summary>
        /// Drops the current PK constraint and creates new from given columns.
        /// </summary>
        /// <param name="tableName">Name of the table</param>
        /// <param name="primaryKeyColumns">List of columns which should be part of primary key</param>
        void RecreatePKConstraint(string tableName, string[] primaryKeyColumns);


        /// <summary>
        /// Checks if column name is unique in given view.
        /// </summary>
        /// <param name="viewName">Name of the view</param>
        /// <param name="columnName">Name of the column to be checked</param>
        bool ColumnExistsInView(string viewName, string columnName);

        #endregion


        #region "Common methods"

        /// <summary>
        /// Returns name of the primary key. If more columns in PK, names are separated by semicolon ";".
        /// </summary>
        /// <param name="tableName">Name of the table to get PK column(s) from.</param>
        string GetTablePKName(string tableName);
         

        /// <summary>
        /// Returns the value string using the database culture. 
        /// </summary>
        /// <param name="value">Value to convert</param>
        string GetValueString(object value);


        /// <summary>
        /// Executes query and returns the results in a DataSet.
        /// </summary>
        /// <param name="queryText">Query text</param>
        /// <param name="queryParams">Query parameters</param>
        /// <param name="queryType">Query type</param>
        DataSet ExecuteQuery(string queryText, QueryDataParameters queryParams, QueryTypeEnum queryType);

        #endregion


        #region "Managing views and stored procedures"

        /// <summary>
        /// Returns SQL code of specified view or stored procedure.
        /// </summary>
        /// <param name="name">Name of the view or stored procedure</param>
        string GetCode(string name);


        /// <summary>
        /// Determines whether specified stored procedure exists or not.
        /// </summary>
        /// <param name="procName">Name of the stored procedure</param>
        bool StoredProcedureExists(string procName);


        /// <summary>
        /// Returns list of views or stored procedures.
        /// </summary>
        /// <param name="where">WHERE condition</param>
        /// <param name="columns">Columns</param>
        /// <param name="getViews">If true list of views is retrieved</param>
        DataSet GetList(string where, string columns, bool getViews);


        /// <summary>
        /// Removes view or stored procedure from database.
        /// </summary>
        /// <param name="name">Name of the object</param>
        /// <param name="isView">Indicates if view is deleted</param>
        void DeleteObject(string name, bool isView);


        /// <summary>
        /// Creates specified procedure in database
        /// </summary>
        /// <param name="procName">Procedure name to create</param>
        /// <param name="param">Parameters</param>
        /// <param name="body">Procedure body</param>
        /// <param name="schema">Database schema</param>
        void CreateProcedure(string procName, string param, string body, string schema);


        /// <summary>
        /// Alters specified procedure in database
        /// </summary>
        /// <param name="procName">Procedure name to create</param>
        /// <param name="param">Parameters</param>
        /// <param name="body">Procedure body</param>
        /// <param name="schema">Database schema</param>
        void AlterProcedure(string procName, string param, string body, string schema);

        #endregion
    }
}
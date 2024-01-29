using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

using CMS.DataEngine;
using CMS.Helpers;


namespace CMS.CustomDataProvider
{
    /// <summary>
    /// Represents SQL Server data connection.
    /// </summary>
    public class DataConnection : AbstractDataConnection
    {
        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="connectionString">Connection string</param>
        internal DataConnection(string connectionString)
            : base(connectionString)
        {
        }


        /// <summary>
        /// Performs a bulk insert of the data into a target database table
        /// </summary>
        /// <param name="sourceData">Source data table</param>
        /// <param name="targetTable">Name of the target DB table</param>
        /// <param name="insertSettings">Bulk insert configuration</param>
        public override void BulkInsert(DataTable sourceData, string targetTable, BulkInsertSettings insertSettings = null)
        {
            if (insertSettings == null)
            {
                // Ensure default settings
                insertSettings = new BulkInsertSettings();
            }

            // Check the thread safety
            CheckThreadSafety();

            bool closeConnection = false;

            try
            {
                // Open the connection
                if (!IsOpen())
                {
                    Open();
                    closeConnection = true;
                }

                SqlConnection conn = (SqlConnection)NativeConnection;

                // Perform the bulk insert
                using (SqlBulkCopy sbc = new SqlBulkCopy(conn, insertSettings.Options, Transaction as SqlTransaction))
                {
                    // Set timeout for insert operation
                    sbc.BulkCopyTimeout = insertSettings.BulkCopyTimeout;

                    // Set newsletter queue table as destination table
                    sbc.DestinationTableName = targetTable;

                    // Map the Source Column from DataTable to the Destination Columns in SQL Server
                    if (insertSettings.Mappings != null)
                    {
                        foreach (var item in insertSettings.Mappings)
                        {
                            sbc.ColumnMappings.Add(item.Key, item.Value);
                        }
                    }

                    // Finally write to server
                    sbc.WriteToServer(sourceData);

                    sbc.Close();
                }
            }
            catch (Exception ex)
            {
                if (!HandleError("BulkInsert to table '" + targetTable + "'", ex))
                {
                    throw;
                }
            }
            finally
            {
                // Close connection if necessary
                if (closeConnection)
                {
                    Close();
                }
            }
        }


        /// <summary>
        /// Creates a new native connection
        /// </summary>
        protected override IDbConnection CreateNativeConnection()
        {
            return new SqlConnection(ConnectionString);
        }


        /// <summary>
        /// Creates a new SQL command
        /// </summary>
        /// <param name="cmdText">Command text</param>
        protected override DbCommand CreateCommand(string cmdText)
        {
            return new SqlCommand(cmdText, (SqlConnection)NativeConnection, (SqlTransaction)Transaction);
        }


        /// <summary>
        /// Creates a new command parameter
        /// </summary>
        /// <param name="name">Parameter name</param>
        /// <param name="value">Parameter value</param>
        protected override DbParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }


        /// <summary>
        /// Creates a data adapter
        /// </summary>
        protected override DbDataAdapter CreateDataAdapter()
        {
            return new SqlDataAdapter();
        }


        /// <summary>
        /// Handles the database error. Returns false, if the error was not handled
        /// </summary>
        /// <param name="message">Error message</param>
        /// <param name="ex">Database exception</param>
        protected override bool HandleDbError(string message, DbException ex)
        {
            // Log track for deadlock exception
            var sqlEx = ex as SqlException;
            if ((sqlEx != null) && (sqlEx.Number == 1205))
            {
                Trackable<CMSTransactionScope>.TrackAllOpen(ex.Message);
            }

            return base.HandleDbError(message, ex);
        }

        #endregion
    }
}
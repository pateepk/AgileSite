using System;
using System.Data;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Bulk insert event arguments.
    /// </summary>
    public class BulkInsertDataEventArgs : CMSEventArgs
    {
        /// <summary>
        /// Inserted data.
        /// </summary>
        public DataTable SourceData
        {
            get;
            set;
        }


        /// <summary>
        /// Target table.
        /// </summary>
        public string TargetTable
        {
            get;
            set;
        }


        /// <summary>
        /// Bulk insert configuration.
        /// </summary>
        public BulkInsertSettings InsertSettings
        {
            get;
            set;
        }


        /// <summary>
        /// Data connection.
        /// </summary>
        public IDataConnection Connection
        {
            get;
            set;
        }


        /// <summary>
        /// Creates new instance of <see cref="BulkInsertDataEventArgs"/>.
        /// </summary>
        [Obsolete("This constructor is meant for system purposes, it shouldn't be used directly. Use constructor with parameters instead. ")]
        public BulkInsertDataEventArgs()
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="BulkInsertDataEventArgs"/>.
        /// </summary>
        /// <param name="sourceData">Source data.</param>
        /// <param name="targetTable">Target table.</param>
        /// <param name="insertSettings">Bulk insert configuration.</param>
        /// <param name="connection">Data connection.</param>
        public BulkInsertDataEventArgs(DataTable sourceData, string targetTable, BulkInsertSettings insertSettings, IDataConnection connection)
        {
            SourceData = sourceData;
            TargetTable = targetTable;
            InsertSettings = insertSettings;
            Connection = connection;
        }
    }
}

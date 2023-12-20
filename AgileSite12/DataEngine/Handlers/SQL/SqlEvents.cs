using System.Data;
using System.Data.Common;

namespace CMS.DataEngine
{
    /// <summary>
    /// SQL events.
    /// </summary>
    public static class SqlEvents
    {
        /// <summary>
        /// Fires when query is executed.
        /// </summary>
        public static ExecuteQueryHandler<DataSet>  ExecuteQuery = new ExecuteQueryHandler<DataSet>
            {
                Name = "SqlEvents.ExecuteQuery", 
                Debug = false
            };


        /// <summary>
        /// Fires when non-query is executed.
        /// </summary>
        public static ExecuteQueryHandler<int> ExecuteNonQuery = new ExecuteQueryHandler<int>
            {
                Name = "SqlEvents.ExecuteNonQuery", 
                Debug = false
            };


        /// <summary>
        /// Fires when scalar query is executed.
        /// </summary>
        public static ExecuteQueryHandler<object> ExecuteScalar = new ExecuteQueryHandler<object>
            {
                Name = "SqlEvents.ExecuteScalar", 
                Debug = false
            };


        /// <summary>
        /// Fires when reader is executed.
        /// </summary>
        public static ExecuteQueryHandler<DbDataReader> ExecuteReader = new ExecuteQueryHandler<DbDataReader>
            {
                Name = "SqlEvents.ExecuteReader",
                Debug = false
            };


        /// <summary>
        /// Fires when bulk insert is executed.
        /// </summary>
        public static BulkInsertDataHandler BulkInsert = new BulkInsertDataHandler()
            {
                Name = "SqlEvents.BulkInsert",
                Debug = false
            };
    }
}
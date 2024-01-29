using System;
using System.Data;

using CMS.Helpers;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base class for Data query source
    /// </summary>
    public abstract class DataQuerySource
    {
        #region "Variables"

        /// <summary>
        /// Specifies database as a source
        /// </summary>
        public const string DATABASE_PREFIX = "Database|";

        /// <summary>
        /// Specifies memory as a source
        /// </summary>
        public const string MEMORY_PREFIX = "Memory|";
        
        /// <summary>
        /// Specifies source for a materialized query within a database
        /// </summary>
        public const string MATERIALIZED = DATABASE_PREFIX + "MATERIALIZED";

        /// <summary>
        /// Specifies default CMS database as a source
        /// </summary>
        public const string CMSDATABASE = DATABASE_PREFIX + ConnectionHelper.DEFAULT_CONNECTIONSTRING_NAME;


        private string mDataSourceName;
        private string mConnectionStringName;

        #endregion


        #region "Properties"

        /// <summary>
        /// Data source identifier.
        /// </summary>
        /// <remarks>
        /// Source identifiers are used to determine whether sub queries should be materialized or inserted directly into parent query. 
        /// </remarks>
        public virtual string DataSourceName
        {
            get
            {
                return mDataSourceName ?? (mDataSourceName = GetDataSourceName());
            }
            internal set
            {
                mDataSourceName = value;
            }
        }


        /// <summary>
        /// Connection string name
        /// </summary>
        public string ConnectionStringName
        {
            get
            {
                return mConnectionStringName ?? (mConnectionStringName = GetConnectionStringName());
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets data source identifier.
        /// </summary>
        /// <remarks>
        /// Source identifiers are used to determine whether sub queries should be materialized or inserted directly into parent query. 
        /// </remarks>
        protected abstract string GetDataSourceName();


        /// <summary>
        /// Gets the connection string name
        /// </summary>
        protected virtual string GetConnectionStringName()
        {
            return null;
        }


        /// <summary>
        /// Gets the data for data query
        /// </summary>
        /// <param name="parameters">Source parameters</param>
        public abstract DataSet GetData(DataQuerySourceParameters parameters);


        /// <summary>
        /// Returns the number of rows in the result queried by this data source if GetData is called with the same parameters
        /// </summary>
        /// <param name="parameters">Source parameters</param>
        public virtual int GetCount(DataQuerySourceParameters parameters)
        {
            var ds = GetData(parameters);

            return DataHelper.GetItemsCount(ds);
        }

        #endregion
    }
}
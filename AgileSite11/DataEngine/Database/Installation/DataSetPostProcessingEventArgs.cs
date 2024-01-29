using System;
using System.Data;

namespace CMS.DataEngine
{
    /// <summary>
    /// Custom event arguments used for default data post processing.
    /// </summary>
    public class DataSetPostProcessingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets dataset being imported.
        /// </summary>
        public DataSet Data
        {
            get;
            set;
        }


        /// <summary>
        /// Gets or sets table name.
        /// </summary>
        public string TableName
        {
            get;
            set;
        }


        /// <summary>
        /// Default constructor for event arguments used in dataset processing.
        /// </summary>
        /// <param name="data">Dataset being imported</param>
        /// <param name="tableName">Table name</param>
        public DataSetPostProcessingEventArgs(DataSet data, string tableName)
        {
            Data = data;
            TableName = tableName;
        }
    }
}

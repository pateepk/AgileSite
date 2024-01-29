using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Base
{
    /// <summary>
    /// Object encapsulating DataSet objects to be accessible via macro engine.
    /// </summary>
    public class DataSetContainer : IDataContainer
    {
        #region "Variables"

        private DataSet mDataSet = null;


        private List<string> mColumnNames = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the encapsulated DataSet.
        /// </summary>
        public DataSet DataSet
        {
            get
            {
                return mDataSet;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of DataSetContainer.
        /// </summary>
        /// <param name="ds">DataSet object to be encapsulated</param>
        public DataSetContainer(DataSet ds)
        {
            mDataSet = ds;
        }

        #endregion


        #region ISimpleDataContainer Members

        /// <summary>
        /// Gets the value of the column, setter is not implemented.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return GetValue(columnName);
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object retval = null;
            TryGetValue(columnName, out retval);
            return retval;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">New value</param>
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataContainer Members

        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                if (mColumnNames == null)
                {
                    var names = new List<string>();

                    if (DataSet != null)
                    {
                        names.Add("Tables");

                        foreach (DataTable table in DataSet.Tables)
                        {
                            names.Add(table.TableName);
                        }
                    }

                    mColumnNames = names;
                }

                return mColumnNames;
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            value = null;
            if (DataSet != null)
            {
                switch (columnName.ToLowerCSafe())
                {
                    case "tables":
                        value = DataSet.Tables;
                        return true;

                    default:
                        // Look directly to a Tables collection
                        if (DataSet.Tables.Contains(columnName))
                        {
                            value = new DataTableContainer(DataSet.Tables[columnName]);
                            return true;
                        }
                        break;
                }
            }
            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            switch (columnName.ToLowerCSafe())
            {
                case "tables":
                    return true;

                default:
                    if (DataSet != null)
                    {
                        return DataSet.Tables.Contains(columnName);
                    }
                    break;
            }
            return false;
        }

        #endregion
    }
}
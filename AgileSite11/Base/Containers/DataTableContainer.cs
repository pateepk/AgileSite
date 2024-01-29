using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Base
{
    /// <summary>
    /// Object encapsulating DataTable objects to be accessible via macro engine.
    /// </summary>
    public class DataTableContainer : IDataContainer
    {
        #region "Variables"

        private DataTable mDataTable = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the encapsulated DataTable.
        /// </summary>
        public DataTable DataTable
        {
            get
            {
                return mDataTable;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of DataTableContainer.
        /// </summary>
        /// <param name="dt">DataTable object to be encapsulated</param>
        public DataTableContainer(DataTable dt)
        {
            mDataTable = dt;
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
                return new List<string> { "TableName", "Columns", "Rows" };
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
            if (DataTable != null)
            {
                switch (columnName.ToLowerCSafe())
                {
                    case "tablename":
                        value = DataTable.TableName;
                        return true;

                    case "columns":
                        value = DataTable.Columns;
                        return true;

                    case "rows":
                        value = DataTable.Rows;
                        return true;
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
                case "tablename":
                case "columns":
                case "rows":
                    return true;
            }
            return false;
        }

        #endregion
    }
}
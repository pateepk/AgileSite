using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Base
{
    /// <summary>
    /// Object encapsulating DataRow objects to be accessible via macro engine.
    /// </summary>
    public class DataRowContainer : IDataContainer
    {
        #region "Variables"

        private DataRow mDataRow = null;

        private bool mAddTableProperty = true;

        private List<string> mColumnNames = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets the encapsulated DataRow.
        /// </summary>
        public DataRow DataRow
        {
            get
            {
                return mDataRow;
            }
        }


        /// <summary>
        /// If true, Table property of DataRow will be included in the supported columns of this container.
        /// </summary>
        public bool AddTableProperty
        {
            get
            {
                return mAddTableProperty;
            }
            set
            {
                mAddTableProperty = value;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of DataRowContainer.
        /// </summary>
        /// <param name="dr">DataRow object to be encapsulated</param>
        public DataRowContainer(DataRow dr)
        {
            mDataRow = dr;
        }


        /// <summary>
        /// Creates new instance of DataRowContainer from DataRowView.
        /// </summary>
        /// <param name="dr">DataRowView to be encapsulated</param>
        public DataRowContainer(DataRowView dr)
        {
            mDataRow = dr.Row;
        }

        #endregion


        #region ISimpleDataContainer Members

        /// <summary>
        /// Gets or sets the value of the column.
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
                SetValue(columnName, value);
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
            if (DataRow != null)
            {
                if (value == null)
                {
                    value = DBNull.Value;
                }

                // Get column index
                int colIndex = DataRow.Table.Columns.IndexOf(columnName);
                if (colIndex >= 0)
                {
                    DataRow[colIndex] = value;
                    return true;
                }
            }
            return false;
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
                // Collect column names, even if new columns were added via DataRow property (take Table column into consideration)
                if ((mColumnNames == null) || ((mDataRow != null) && (mColumnNames.Count != (mDataRow.Table.Columns.Count + (AddTableProperty ? 1 : 0)))))
                {
                    var names = new List<string>();

                    if (DataRow != null)
                    {
                        if (AddTableProperty)
                        {
                            names.Add("Table");
                        }

                        foreach (DataColumn column in DataRow.Table.Columns)
                        {
                            names.Add(column.ColumnName);
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
            if (DataRow != null)
            {
                // Check table property
                if (columnName.Equals("Table", StringComparison.InvariantCultureIgnoreCase))
                {
                    value = new DataTableContainer(DataRow.Table);
                    return true;
                }
                // Look directly to a columns values
                else if (DataRow.Table.Columns.Contains(columnName))
                {
                    value = DataRow[columnName];
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
                case "table":
                    return true;

                default:
                    if (DataRow != null)
                    {
                        return DataRow.Table.Columns.Contains(columnName);
                    }
                    break;
            }
            return false;
        }

        #endregion
    }
}
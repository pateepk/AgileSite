using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Base
{
    /// <summary>
    /// Case insensitive data container class.
    /// </summary>
    public class DataContainer : IDataContainer
    {
        /// <summary>
        /// Hashtable for storing values.
        /// </summary>
        private readonly Hashtable mHashtable = new Hashtable(StringComparer.InvariantCultureIgnoreCase);


        /// <summary>
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return mHashtable.Keys.Cast<string>().ToList();
            }
        }


        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object this[string columnName]
        {
            get
            {
                return mHashtable[columnName];
            }
            set
            {
                mHashtable[columnName] = value;
            }
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return mHashtable.Contains(columnName);
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            return this[columnName];
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            this[columnName] = value;
            return true;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            value = this[columnName];
            return ContainsColumn(columnName);
        }
    }
}
using System;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Container to wrap the IDictionary[string, object] structure
    /// </summary>
    public class DictionaryContainer : IDataContainer
    {
        #region

        /// <summary>
        /// Wrapped dictionary
        /// </summary>
        private IDictionary<string, object> mInnerDictionary = null;


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="dict">Wrapped dictionary</param>
        public DictionaryContainer(IDictionary<string, object> dict)
        {
            mInnerDictionary = dict;
        }

        #endregion


        #region "ISimpleDataContainer Members"

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
        /// Gets the value from QueryString.
        /// </summary>
        /// <param name="key">QueryString key</param>
        public object GetValue(string key)
        {
            object retval = null;
            TryGetValue(key, out retval);
            return retval;
        }


        /// <summary>
        /// Not implemented.
        /// </summary>
        /// <param name="key">QueryString key</param>
        /// <param name="value">New value</param>
        public bool SetValue(string key, object value)
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
                return new List<string>(mInnerDictionary.Keys);
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
            value = mInnerDictionary[columnName];
            return (value != null);
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return mInnerDictionary.ContainsKey(columnName);
        }

        #endregion
    }
}
using System.Collections.Generic;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Named data container (Data container with prefix).
    /// </summary>
    public class NamedDataContainer : IDataContainer
    {
        #region "Variables"

        /// <summary>
        /// Inner data container.
        /// </summary>
        protected IDataContainer mInnerContainer = null;


        /// <summary>
        /// Column prefix.
        /// </summary>
        protected string mPrefix = null;

        #endregion


        #region "Properties"

        /// <summary>
        /// Inner data container.
        /// </summary>
        public IDataContainer InnerContainer
        {
            get
            {
                return mInnerContainer;
            }
        }


        /// <summary>
        /// Column prefix.
        /// </summary>
        public string Prefix
        {
            get
            {
                return mPrefix;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="sourceData">Source data</param>
        /// <param name="prefix">Column prefix</param>
        public NamedDataContainer(IDataContainer sourceData, string prefix)
        {
            mInnerContainer = sourceData;
            mPrefix = prefix;
        }


        /// <summary>
        /// Gets the original column name without the prefix.
        /// </summary>
        /// <param name="columnName">Column name with prefix</param>
        public string GetOriginalColumnName(string columnName)
        {
            if (!columnName.StartsWithCSafe(Prefix, true))
            {
                return null;
            }
            else
            {
                return columnName.Substring(Prefix.Length);
            }
        }

        #endregion


        #region "IDataContainer Members"

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
        /// Column names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return InnerContainer.ColumnNames;
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
            columnName = GetOriginalColumnName(columnName);
            if (columnName == null)
            {
                value = null;
                return false;
            }

            return InnerContainer.TryGetValue(columnName, out value);
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            columnName = GetOriginalColumnName(columnName);
            if (columnName == null)
            {
                return null;
            }

            return InnerContainer.GetValue(columnName);
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            columnName = GetOriginalColumnName(columnName);
            if (columnName == null)
            {
                return false;
            }

            return InnerContainer.SetValue(columnName, value);
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            columnName = GetOriginalColumnName(columnName);
            if (columnName == null)
            {
                return false;
            }

            return InnerContainer.ContainsColumn(columnName);
        }

        #endregion
    }
}
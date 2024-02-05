using System.Collections.Generic;

using CMS.Base;

namespace CMS.MacroEngine
{
    /// <summary>
    /// Collection of the IDataContainers behaving like IDataContainer.
    /// </summary>
    public class DataContainerCollection : List<IDataContainer>, IDataContainer
    {
        #region "Variables"

        /// <summary>
        /// Column names.
        /// </summary>
        private List<string> mColumnNames = null;

        #endregion


        #region "Properties"

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
                if (mColumnNames == null)
                {
                    List<string> columns = new List<string>();

                    // Check all containers
                    foreach (IDataContainer container in this)
                    {
                        columns.AddRange(container.ColumnNames);
                    }

                    mColumnNames = columns;
                }

                return mColumnNames;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object value = null;

            // Get the value
            TryGetValue(columnName, out value);

            return value;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public bool TryGetValue(string columnName, out object value)
        {
            // Check all containers
            foreach (IDataContainer container in this)
            {
                // Save to the first container found
                if (container.TryGetValue(columnName, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            // Check all containers
            foreach (IDataContainer container in this)
            {
                // Save to the first container found
                if (container.SetValue(columnName, value))
                {
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
            // Check all containers
            foreach (IDataContainer container in this)
            {
                if (container.ContainsColumn(columnName))
                {
                    return true;
                }
            }

            return false;
        }

        #endregion
    }
}
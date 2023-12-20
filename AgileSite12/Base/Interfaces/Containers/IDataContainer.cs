using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// General data container interface.
    /// </summary>
    public interface IDataContainer : ISimpleDataContainer
    {
        /// <summary>
        /// Column names.
        /// </summary>
        List<string> ColumnNames
        {
            get;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        bool TryGetValue(string columnName, out object value);


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        bool ContainsColumn(string columnName);
    }
}
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Data container with advanced functionality.
    /// </summary>
    public interface IAdvancedDataContainer : IDataContainer
    {
        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        bool HasChanged
        {
            get;
        }


        /// <summary>
        /// Returns true if the object is complete (has all columns).
        /// </summary>
        bool IsComplete
        {
            get;
        }


        /// <summary>
        /// Reverts the object changes to the original values.
        /// </summary>
        void RevertChanges();


        /// <summary>
        /// Resets the object changes and keeps the new values as unchanged.
        /// </summary>
        void ResetChanges();


        /// <summary>
        /// Returns the original value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object GetOriginalValue(string columnName);


        /// <summary>
        /// Returns true if the item on specified column name changed.
        /// </summary>
        /// <param name="columnName">Column name</param>
        bool ItemChanged(string columnName);


        /// <summary>
        /// Returns list of column names which values were changed.
        /// </summary>
        /// <returns>List of column names</returns>
        List<string> ChangedColumns();


        /// <summary>
        /// Makes the object data complete.
        /// </summary>
        /// <param name="loadFromDb">If true, the data to complete the object is loaded from database</param>
        void MakeComplete(bool loadFromDb);


        /// <summary>
        /// Returns true if the object changed.
        /// </summary>
        /// <param name="excludedColumns">List of columns excluded from change (separated by ';')</param>
        bool DataChanged(string excludedColumns);
    }
}
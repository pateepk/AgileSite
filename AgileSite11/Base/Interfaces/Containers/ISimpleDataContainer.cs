namespace CMS.Base
{
    /// <summary>
    /// Simple data container interface (does not provide any information about the columns).
    /// </summary>
    public interface ISimpleDataContainer
    {
        /// <summary>
        /// Gets or sets the value of the column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object this[string columnName]
        {
            get;
            set;
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        object GetValue(string columnName);


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        bool SetValue(string columnName, object value);
    }
}
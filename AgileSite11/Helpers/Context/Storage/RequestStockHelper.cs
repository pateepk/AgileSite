using System;
using System.Collections;

using CMS.Base;

namespace CMS.Helpers
{
    /// <summary>
    /// Inserts items into HttpContext.Current.Items.
    /// </summary>
    public class RequestStockHelper : AbstractStockHelper<RequestStockHelper>, ISimpleDataContainer
    {
        #region "Properties"

        /// <summary>
        /// Gets the current items.
        /// </summary>
        public override IDictionary CurrentItems
        {
            get
            {
                return RequestItems.CurrentItems;
            }
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
                throw new NotImplementedException();
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            return GetItem(columnName, true);
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            Add(columnName, value, true);

            return true;
        }

        #endregion
    }
}
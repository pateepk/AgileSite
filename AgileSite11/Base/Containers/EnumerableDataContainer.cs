using System;
using System.Collections;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Provides wrapper for any list for usage in the macro engine. 
    /// </summary>
    public class EnumerableDataContainer<T> : IDataContainer, IEnumerable<T>
    {
        #region "Variables"

        /// <summary>
        /// Items
        /// </summary>
        protected IEnumerable<T> mItems;


        /// <summary>
        /// Items collection by name
        /// </summary>
        protected StringSafeDictionary<T> mItemsByString;


        /// <summary>
        /// List of available column names
        /// </summary>
        private List<string> mColumnNames;

        #endregion


        #region "Properties"

        /// <summary>
        /// Items collection by name
        /// </summary>
        protected StringSafeDictionary<T> ItemsByString
        {
            get
            {
                if (mItemsByString == null)
                {
                    var items = new StringSafeDictionary<T>();

                    // Add all items
                    foreach (var item in mItems)
                    {
                        if (item != null)
                        {
                            items[item.ToString()] = item;
                        }
                    }

                    mItemsByString = items;
                }

                return mItemsByString;  
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance from the given list
        /// </summary>
        /// <param name="items">Items to build data container</param>
        public EnumerableDataContainer(IEnumerable<T> items)
        {
            mItems = items;
        }

        #endregion


        #region ISimpleDataContainer Members

        /// <summary>
        /// Returns the value of given enumeration item.
        /// </summary>
        /// <param name="columnName">Name of the enumeration item</param>
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
        /// Returns the value of given enumeration item.
        /// </summary>
        /// <param name="columnName">Enumeration item name</param>
        public object GetValue(string columnName)
        {
            return mItemsByString[columnName];
        }


        /// <summary>
        /// Not implemented, throws an exception.
        /// </summary>
        /// <param name="columnName">Not supported</param>
        /// <param name="value">Not supported</param>
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataContainer Members

        /// <summary>
        /// Returns list of enumeration items.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                if (mColumnNames == null)
                {
                    var cols = new List<string>();

                    // Add all items
                    foreach (var item in mItems)
                    {
                        cols.Add(item.ToString());
                    }

                    mColumnNames = cols;
                }
                return mColumnNames;
            }
        }


        /// <summary>
        /// Returns the value of given enumeration item.
        /// </summary>
        /// <param name="columnName">Name of the item</param>
        /// <param name="value">Value</param>
        public bool TryGetValue(string columnName, out object value)
        {
            value = null;
            if (ContainsColumn(columnName))
            {
                value = GetValue(columnName);
                return true;
            }
            return false;
        }


        /// <summary>
        /// Returns true if given name is within the enumeration items.
        /// </summary>
        /// <param name="columnName">Name of the item</param>
        public bool ContainsColumn(string columnName)
        {
            foreach (var item in ColumnNames)
            {
                if (item.EqualsCSafe(columnName, true))
                {
                    return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Gets the enumerator of internal items
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            return mItems.GetEnumerator();
        }


        /// <summary>
        /// Gets the enumerator of internal items
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return mItems.GetEnumerator();
        }

        #endregion
    }
}
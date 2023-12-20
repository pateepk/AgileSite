using System;
using System.Collections;
using System.Collections.Generic;

using CMS.Helpers;
using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Search settings class.
    /// </summary>
    public class SearchSettingsInfo : IDataContainer
    {
        #region "Private properties"

        private readonly Hashtable table = new Hashtable(StringComparer.InvariantCultureIgnoreCase);

        #endregion


        #region "Public properties"

        /// <summary>
        /// GUID column.
        /// </summary>
        public virtual Guid ID
        {
            get
            {
                return ValidationHelper.GetGuid(table["id"], Guid.Empty);
            }
            set
            {
                table["id"] = value;
            }
        }


        /// <summary>
        /// Name column.
        /// </summary>
        public virtual string Name
        {
            get
            {
                return ValidationHelper.GetString(table["name"], null);
            }
            set
            {
                table["name"] = value;
            }
        }


        /// <summary>
        /// Custom field name column.
        /// </summary>
        public virtual string FieldName
        {
            get
            {
                return ValidationHelper.GetString(table["fieldname"], null);
            }
            set
            {
                table["fieldname"] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Gets value associated with flag <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of flag to return value for.</param>
        /// <returns>Returns flag value, or false when flag is not present. Use <see cref="ContainsColumn"/> to determine whether flag exists.</returns>
        /// <seealso cref="SearchSettings.CONTENT"/>
        /// <seealso cref="SearchSettings.SEARCHABLE"/>
        /// <seealso cref="SearchSettings.TOKENIZED"/>
        public virtual bool GetFlag(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            return ValidationHelper.GetBoolean(table[name], false);
        }


        /// <summary>
        /// Sets value of flag <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of flag to set value of.</param>
        /// <param name="value">Value to be set.</param>
        /// <seealso cref="SearchSettings.CONTENT"/>
        /// <seealso cref="SearchSettings.SEARCHABLE"/>
        /// <seealso cref="SearchSettings.TOKENIZED"/>
        public virtual void SetFlag(string name, bool value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            var normalizedName = name.ToLowerInvariant();

            table[normalizedName] = value;
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
        /// Returns array of property names.
        /// </summary>
        public virtual List<string> ColumnNames
        {
            get
            {
                lock (table)
                {
                    return TypeHelper.NewList(table.Keys);
                }
            }
        }


        /// <summary>
        /// Returns value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Returns the value</param>
        /// <returns>Returns true if the operation was successful (the value was present)</returns>
        public virtual bool TryGetValue(string columnName, out object value)
        {
            if ((table == null) || (table[columnName] == DBNull.Value))
            {
                value = null;
            }
            else
            {
                value = table[columnName];
            }

            return true;
        }


        /// <summary>
        /// Returns value specified by property name.
        /// </summary>
        public virtual object GetValue(string columnName)
        {
            if ((table == null) || (table[columnName] == DBNull.Value))
            {
                return null;
            }
            else
            {
                return table[columnName];
            }
        }


        /// <summary>
        /// Sets value to specified property.
        /// </summary>
        public virtual bool SetValue(string columnName, object value)
        {
            if ((table != null) && !String.IsNullOrEmpty(columnName))
            {
                table[columnName] = value;
                return true;
            }

            return false;
        }


        /// <summary>
        /// Indicates if specified property is included.
        /// </summary>
        public virtual bool ContainsColumn(string columnName)
        {
            return table.ContainsKey(columnName);
        }

        #endregion
    }
}
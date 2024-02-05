using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DocumentEngine
{
    /// <summary>
    /// Defines document URL for given culture.
    /// </summary>
    public class DocumentCultureUrl : IDataContainer
    {
        /// <summary>
        /// Culture code for which URL is generated.
        /// </summary>
        public string CultureCode
        {
            get;
            set;
        }


        /// <summary>
        /// Culture name for which URL is generated.
        /// </summary>
        public string CultureName
        {
            get;
            set;
        }


        /// <summary>
        /// Culture specific URL.
        /// </summary>
        public string Url
        {
            get;
            set;
        }


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
        /// Gets the value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public object GetValue(string columnName)
        {
            object value;
            TryGetValue(columnName, out value);
            return value;
        }


        /// <summary>
        /// Sets value of column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="value">Column value</param> 
        public bool SetValue(string columnName, object value)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region IDataContainer Members

        /// <summary>
        /// DocumentCultureURL object property names.
        /// </summary>
        public List<string> ColumnNames
        {
            get
            {
                return new List<string>
                {
                    "CultureCode",
                    "CultureName",
                    "URL"
                };
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
            switch (columnName.ToLowerCSafe())
            {
                case "culturecode":
                    value = CultureCode;
                    return true;

                case "culturename":
                    value = CultureName;
                    return true;

                case "url":
                    value = Url;
                    return true;
            }

            value = null;
            return false;
        }


        /// <summary>
        /// Returns true if the object contains specified column.
        /// </summary>
        /// <param name="columnName">Column name</param>
        public bool ContainsColumn(string columnName)
        {
            return ColumnNames.Contains(columnName);
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Base
{
    /// <summary>
    /// Provides wrapper for any Enum object for usage in the macro engine. Uses reflection to extract fields of the enum.
    /// </summary>
    public class EnumDataContainer : IDataContainer
    {
        #region "Variables"

        /// <summary>
        /// Type of the enum.
        /// </summary>
        protected Type mEnumType;

        /// <summary>
        /// Column names (enum items).
        /// </summary>
        protected Lazy<List<string>> mColumnNames;

        #endregion


        #region "Properties"

        /// <summary>
        /// Element type.
        /// </summary>
        public Type EnumType
        {
            get
            {
                return mEnumType;
            }
        }

        #endregion


        #region "Constructors"

        /// <summary>
        /// Creates new instance of enum container.
        /// </summary>
        /// <param name="enumType">Type of the enum</param>
        public EnumDataContainer(Type enumType)
        {
            if (!enumType.IsEnum)
            {
                throw new ArgumentException("Only enumeration types are allowed.");
            }
            mEnumType = enumType;
            mColumnNames = new Lazy<List<string>>(GetColumnNames);
        }

        #endregion


        #region "ISimpleDataContainer Members"

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
            try
            {
                switch (mEnumType.MemberType.GetTypeCode())
                {
                    case TypeCode.Int32:
                        return (int)Enum.Parse(mEnumType, columnName, true);

                    case TypeCode.String:
                        return (string)Enum.Parse(mEnumType, columnName, true);
                }
            }
            catch { }
            return null;
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
                return mColumnNames.Value;
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
            return ColumnNames.Any(col => col.Equals(columnName, StringComparison.OrdinalIgnoreCase));
        }


        private List<string> GetColumnNames()
        {
            return mEnumType.GetFields()
                .Where(item => !item.IsSpecialName)
                .Select(item => item.Name)
                .ToList();
        }

        #endregion
    }
}
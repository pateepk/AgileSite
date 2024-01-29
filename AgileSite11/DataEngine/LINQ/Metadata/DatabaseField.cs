using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Specifies to which database column the property maps
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DatabaseFieldAttribute : Attribute
    {
        /// <summary>
        /// Database column name
        /// </summary>
        public string ColumnName 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Type representing the stored value
        /// </summary>
        public Type ValueType
        {
            get;
            set;
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public DatabaseFieldAttribute()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="colName">Database column name</param>
        public DatabaseFieldAttribute(string colName)
        {
            ColumnName = colName;
        }
    }
}

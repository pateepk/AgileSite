using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Specifies to which database column the property maps. For simple database mappings use <see cref="DatabaseFieldAttribute"/> instead.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public sealed class DatabaseMappingAttribute : Attribute
    {
        /// <summary>
        /// Database column name or expression returning boolean value for bool properties.
        /// E.g. "NodeID" for int property or "Enabled = 1" for bool property.
        /// </summary>
        public string Expression 
        { 
            get; 
            set; 
        }


        /// <summary>
        /// If true, the member is executed on DB level, if false, then programmatically
        /// </summary>
        public bool ExecuteInDB
        { 
            get; 
            set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="executeInDb">If true, the member is executed on DB level, if false, then programmatically</param>
        public DatabaseMappingAttribute(bool executeInDb)
        {
            ExecuteInDB = executeInDb;
        }
                

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="expression">
        /// Expression defining the mapping of the attribute to database.
        /// Database column name or expression returning boolean value for bool properties. E.g.
        /// "NodeID" for int property or "Enabled = 1" for bool property.
        /// </param>
        public DatabaseMappingAttribute(string expression)
        {
            ExecuteInDB = true;
            Expression = expression;
        }
    }
}

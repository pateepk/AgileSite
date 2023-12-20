using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;

namespace CMS.Base
{
    /// <summary>
    /// Abstract class for data container objects
    /// </summary>
    public abstract class AbstractDataContainer<ParentObjectType> : AbstractSimpleDataContainer<ParentObjectType>, IDataContainer 
        where ParentObjectType : AbstractDataContainer<ParentObjectType>
    {
        /// <summary>
        /// Available column names.
        /// </summary>
        [XmlIgnore]
        public virtual List<string> ColumnNames
        {
            get
            {
                return RegisteredColumns.GetRegisteredProperties();
            }
        }


        /// <summary>
        /// Returns true if specified column is available in current structure.
        /// </summary>
        public bool ContainsColumn(string columnName)
        {
            return ColumnNames.Contains(columnName, StringComparer.InvariantCultureIgnoreCase);
        }
    }
}
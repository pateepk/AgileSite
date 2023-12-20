using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class representing database index column.
    /// </summary>
    public class IndexColumn
    {
        /// <summary>
        /// Index column name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether index key column has a descending sort direction.
        /// </summary>
        public bool IsDescendingKey
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether column is a nonkey column of the index.
        /// </summary>
        public bool IsIncluded
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new index column.
        /// </summary>
        public IndexColumn(string name, bool isDescendingKey, bool isIncluded)
        {
            Name = name;
            IsDescendingKey = isDescendingKey;
            IsIncluded = isIncluded;
        }
    }
}

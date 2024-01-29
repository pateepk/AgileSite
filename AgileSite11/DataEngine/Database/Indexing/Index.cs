using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.DataEngine
{
    /// <summary>
    /// Class representing database index.
    /// </summary>
    public class Index
    {
        /// <summary>
        /// Index name.
        /// </summary>
        public string Name
        {
            get;
            private set;
        }


        /// <summary>
        /// Index type as defined in https://msdn.microsoft.com/en-us/library/ms173760.aspx
        /// </summary>
        public int Type
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether index is unique.
        /// </summary>
        public bool IsUnique
        {
            get;
            private set;
        }


        /// <summary>
        /// Indicates whether index is part of a primary key constraint.
        /// </summary>
        public bool IsPrimaryKey
        {
            get;
            private set;
        }


        /// <summary>
        /// Read-only list of columns which are part of this index. 
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public IReadOnlyList<IndexColumn> Columns
        {
            get;
            private set;
        }


        /// <summary>
        /// Initializes a new index.
        /// </summary>
        public Index(string name, int type, bool isUnique, bool isPrimaryKey, IReadOnlyList<IndexColumn> columns)
        {
            Name = name;
            Type = type;
            IsUnique = isUnique;
            IsPrimaryKey = isPrimaryKey;
            Columns = columns;
        }
    }
}

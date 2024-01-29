using System;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    internal class ColumnIndexDefinition
    {
        private readonly HashSet<string> mColumns = new HashSet<string>();
        private readonly HashSet<string> mIncludedColumns = new HashSet<string>();  


        /// <summary>
        /// Index name
        /// </summary>
        public string IndexName
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if index is unique
        /// </summary>
        public bool IsUnique
        {
            get;
            set;
        }


        /// <summary>
        /// Indicates if index is clustered
        /// </summary>
        public bool IsClustered
        {
            get;
            set;
        }


        /// <summary>
        /// Collection of indexed columns
        /// </summary>
        public ISet<string> Columns
        {
            get
            {
                return mColumns;
            }
        } 


        /// <summary>
        /// Collection of included indexed columns
        /// </summary>
        public ISet<string> IncludedColumns
        {
            get
            {
                return mIncludedColumns;
            }
        } 


        /// <summary>
        /// Filter condition
        /// </summary>
        public string Condition
        {
            get;
            set;
        }


        /// <summary>
        /// Table name where index belongs to
        /// </summary>
        public string TableName
        {
            get;
            set;
        }


        /// <summary>
        /// Adds new column into correct collection based on <paramref name="columnIsIncluded"/> parameter
        /// </summary>
        /// <param name="columnName">Name of the new column</param>
        /// <param name="columnIsIncluded">Flag indicates if column is normal indexed or included</param>
        public void AddColumn(string columnName, bool columnIsIncluded)
        {
            if (columnIsIncluded)
            {
                IncludedColumns.Add(columnName);
            }
            else
            {
                Columns.Add(columnName);
            }
        }


        /// <summary>
        /// Gets query to create column index based on its settings
        /// </summary>
        public string GetQuery()
        {
            string type = IsUnique ? "UNIQUE " : "";
            type += IsClustered ? "CLUSTERED " : "";

            string query = string.Format("CREATE {0}INDEX {1} ON {2} ({3})", type, IndexName, TableName, String.Join(", ", Columns));

            if (IncludedColumns.Count > 0)
            {
                query += String.Format(" INCLUDE ({0})", String.Join(", ", IncludedColumns));
            }

            if (!String.IsNullOrEmpty(Condition))
            {
                query += String.Format(" WHERE {0}", String.Join(", ", Condition));
            }

            return query;
        }
    }
}

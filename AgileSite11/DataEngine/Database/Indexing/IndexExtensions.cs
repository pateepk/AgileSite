using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.DataEngine
{
    /// <summary>
    /// Extension methods for <see cref="Index"/> class.
    /// </summary>
    public static class IndexExtensions
    {
        /// <summary>
        /// Gets columns as a comma separated list where column names are properly enclosed in square brackets and each column name is appended its ordering (ASC or DESC).
        /// Included columns are omitted.
        /// </summary>
        public static string GetOrderBy(this Index index)
        {
            if (index == null)
            {
                throw new ArgumentNullException("index");
            }

            var orderByColumns = index.Columns.Where(col => !col.IsIncluded).Select(col => String.Format("[{0}] {1}", col.Name.Replace("]", "]]"), col.IsDescendingKey ? "DESC" : "ASC"));

            return String.Join(", ", orderByColumns);
        }
    }
}

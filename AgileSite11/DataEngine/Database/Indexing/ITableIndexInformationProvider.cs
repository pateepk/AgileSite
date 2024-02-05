using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.DataEngine
{
    /// <summary>
    /// Denotes classes that can provide information regarding table indexes.
    /// </summary>
    internal interface ITableIndexInformationProvider
    {
        /// <summary>
        /// Gets indexes of table identified by given <paramref name="tableName"/>.
        /// </summary>
        /// <exception cref="ArgumentException">Thrown when <paramref name="tableName"/> is null or empty string.</exception>
        ITableIndexes GetTableIndexes(string tableName);
    }
}

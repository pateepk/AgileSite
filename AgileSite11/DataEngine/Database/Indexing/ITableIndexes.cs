using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.DataEngine
{
    /// <summary>
    /// Encloses information about table indexes.
    /// </summary>
    public interface ITableIndexes
    {
        /// <summary>
        /// Gets clustered index, or null when no such exists.
        /// </summary>
        Index GetClusteredIndex();


        /// <summary>
        /// Gets index which is part of a primary key constraint, or null when no such exists.
        /// </summary>
        Index GetPrimaryKeyIndex();


        /// <summary>
        /// Gets enumeration of nonclustered indexes.
        /// </summary>
        /// <returns>Enumeration of nonclustered indexes, or empty enumeration when no such exists.</returns>
        IEnumerable<Index> GetNonclusteredIndexes();


        /// <summary>
        /// Gets enumeration of indexes.
        /// </summary>
        /// <returns>Enumeration of indexes, or empty enumeration when none exists.</returns>
        IEnumerable<Index> GetIndexes();
    }
}

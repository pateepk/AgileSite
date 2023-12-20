using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.DataEngine
{
    /// <summary>
    /// Encloses information about MSSQL database table indexes.
    /// </summary>
    internal class TableIndexes : ITableIndexes
    {
        /// <summary>
        /// Index ID of a clustered table index in MSSQL. See https://msdn.microsoft.com/en-us/library/ms173760.aspx for details.
        /// </summary>
        private const int MSSQL_CLUSTERED_INDEX_ID = 1;


        /// <summary>
        /// Index type of a nonclustered index in MSSQL. See https://msdn.microsoft.com/en-us/library/ms173760.aspx for details.
        /// </summary>
        private const int MSSQL_NONCLUSTERED_INDEX_TYPE = 2;


        private readonly Dictionary<int, Index> mIndexes;


        /// <summary>
        /// Gets clustered index, or null when no such exists.
        /// </summary>
        public Index GetClusteredIndex()
        {
            return mIndexes.ContainsKey(MSSQL_CLUSTERED_INDEX_ID) ? mIndexes[MSSQL_CLUSTERED_INDEX_ID] : null;
        }


        /// <summary>
        /// Gets index which is part of a primary key constraint, or null when no such exists.
        /// </summary>
        public Index GetPrimaryKeyIndex()
        {
            return mIndexes.Values.FirstOrDefault(index => index.IsPrimaryKey);
        }


        /// <summary>
        /// Gets enumeration of nonclustered indexes.
        /// </summary>
        /// <returns>Enumeration of nonclustered indexes, or empty enumeration when no such exists.</returns>
        public IEnumerable<Index> GetNonclusteredIndexes()
        {
            return mIndexes.Values.Where(index => index.Type == MSSQL_NONCLUSTERED_INDEX_TYPE);
        }


        /// <summary>
        /// Gets enumeration of indexes.
        /// </summary>
        /// <returns>Enumeration of indexes, or empty enumeration when none exists.</returns>
        public IEnumerable<Index> GetIndexes()
        {
            return mIndexes.Values;
        } 


        /// <summary>
        /// Initializes table indexes from a dictionary of indexes. The dictionary keys represent index ID (as defined in https://msdn.microsoft.com/en-us/library/ms173760.aspx ).
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="indexes"/> is null.</exception>
        public TableIndexes(Dictionary<int, Index> indexes)
        {
            if (indexes == null)
            {
                throw new ArgumentNullException("indexes");
            }
            mIndexes = indexes;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CMS.ContinuousIntegration
{
    /// <summary>
    /// Contains cached filter condition results organized by object types and their where conditions.
    /// </summary>
    internal interface IFilterConditionCache
    {
        /// <summary>
        /// Tries to get a value indicating whether object type and its where condition meet filter condition.
        /// </summary>
        bool TryGet(string filteredObjectType, string whereCondition, out bool meetsFilterCondition);


        /// <summary>
        /// Adds a new cache entry indicating, whether object type and its where condition meet filter condition.
        /// </summary>
        void Add(string filteredObjectType, string whereCondition, bool meetsFilterCondition);


        /// <summary>
        /// Clears the whole cache.
        /// </summary>
        void Clear();
    }
}

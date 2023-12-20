using System;
using System.Linq;
using System.Text;

using CMS.Helpers;

namespace CMS.Membership
{
    /// <summary>
    /// Represents action performed on hash table with user sessions.
    /// </summary>
    internal enum SessionsCachingActionEnum
    {
        /// <summary>
        /// Add session to hash table or update existing.
        /// </summary>
        [EnumDefaultValue]
        [EnumStringRepresentation("UpdateOrInsertSession")]
        UpsertSession,


        /// <summary>
        /// Remove session from hash table.
        /// </summary>
        [EnumStringRepresentation("RemoveSession")]
        RemoveSession,


        /// <summary>
        /// Update site name in hash table to re-index it.
        /// </summary>
        [EnumStringRepresentation("ChangeSiteName")]
        ChangeSiteName,


        /// <summary>
        /// Clear all hash tables.
        /// </summary>
        [EnumStringRepresentation("ClearHashTables")]
        Clear,
    }
}

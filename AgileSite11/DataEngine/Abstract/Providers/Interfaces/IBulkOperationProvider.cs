using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Adds bulk operation support to info object provider.
    /// </summary>
    public interface IBulkOperationProvider
    {
        /// <summary>
        /// Bulk inserts the given list of info objects
        /// </summary>
        /// <remarks>
        /// Info object ID is not set during the bulk insert operation
        /// </remarks>
        /// <param name="objects">List of objects</param>
        void BulkInsertInfos(IEnumerable<BaseInfo> objects);
    }
}
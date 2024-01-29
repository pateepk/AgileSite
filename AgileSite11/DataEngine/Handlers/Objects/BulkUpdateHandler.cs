using CMS.Base;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Bulk update handler enables update result processing.
    /// </summary>
    public class BulkUpdateHandler : AdvancedHandler<BulkUpdateHandler, BulkUpdateEventArgs>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public BulkUpdateHandler()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public BulkUpdateHandler(BulkUpdateHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="typeInfo">Type information regarding the objects updated.</param>
        /// <param name="where">Where condition for the update</param>
        /// <param name="changedColumns">Names of the columns that are changed by the bulk update. Any object's column can be changed if this collection is not set or empty.</param>
        public BulkUpdateHandler StartEvent(ObjectTypeInfo typeInfo, IWhereCondition where, IEnumerable<string> changedColumns = null)
        {
            var e = new BulkUpdateEventArgs(typeInfo, where, changedColumns);

            var h = StartEvent(e);

            return h;
        }
    }
}

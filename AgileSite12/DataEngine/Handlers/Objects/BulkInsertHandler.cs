using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Bulk insert handler enables insert result processing.
    /// </summary>
    public sealed class BulkInsertHandler : AdvancedHandler<BulkInsertHandler, BulkInsertEventArgs>
    {
        /// <summary>
        /// Creates new instance of <see cref="BulkInsertHandler"/>.
        /// </summary>
        public BulkInsertHandler()
        {
        }


        /// <summary>
        /// Creates new instance of <see cref="BulkInsertHandler"/>.
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public BulkInsertHandler(BulkInsertHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="typeInfo">Type information regarding the objects inserted.</param>
        /// <param name="objects">Collection of inserted objects</param>
        public BulkInsertHandler StartEvent(ObjectTypeInfo typeInfo, IEnumerable<IInfo> objects)
        {
            var e = new BulkInsertEventArgs(typeInfo, objects);

            var h = StartEvent(e);

            return h;
        }
    }
}

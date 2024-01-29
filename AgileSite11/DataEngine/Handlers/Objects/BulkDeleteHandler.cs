using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Bulk delete handler enables deletion processing.
    /// </summary>
    public sealed class BulkDeleteHandler : AdvancedHandler<BulkDeleteHandler, BulkDeleteEventArgs>
    {
        /// <summary>
        /// Creates instance of <see cref="BulkDeleteHandler"/>.
        /// </summary>
        public BulkDeleteHandler()
        {
        }


        /// <summary>
        /// Creates instance of <see cref="BulkDeleteHandler"/>.
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public BulkDeleteHandler(BulkDeleteHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="typeInfo">Type information regarding the objects updated.</param>
        /// <param name="where">Where condition for the update</param>
        public BulkDeleteHandler StartEvent(ObjectTypeInfo typeInfo, IWhereCondition where)
        {
            var e = new BulkDeleteEventArgs(typeInfo, where);

            var h = StartEvent(e);

            return h;
        }
    }
}

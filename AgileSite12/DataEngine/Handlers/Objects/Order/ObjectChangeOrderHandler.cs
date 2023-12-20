using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object handler
    /// </summary>
    public class ObjectChangeOrderHandler : AdvancedHandler<ObjectChangeOrderHandler, ObjectChangeOrderEventArgs>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectChangeOrderHandler()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public ObjectChangeOrderHandler(ObjectChangeOrderHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="obj">Handled object</param>
        public ObjectChangeOrderHandler StartEvent(BaseInfo obj)
        {
            ObjectChangeOrderEventArgs e = new ObjectChangeOrderEventArgs
            {
                Object = obj
            };

            return StartEvent(e);
        }
    }
}
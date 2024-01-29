using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object handler
    /// </summary>
    public class ObjectSortHandler : AdvancedHandler<ObjectSortHandler, ObjectSortEventArgs>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public ObjectSortHandler()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public ObjectSortHandler(ObjectSortHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="obj">Handled object</param>
        public ObjectSortHandler StartEvent(BaseInfo obj)
        {
            ObjectSortEventArgs e = new ObjectSortEventArgs
            {
                Object = obj
            };

            return StartEvent(e);
        }
    }
}
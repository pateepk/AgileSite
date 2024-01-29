using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Simple object handler
    /// </summary>
    public class SimpleObjectHandler : SimpleHandler<SimpleObjectHandler, ObjectEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public SimpleObjectHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public SimpleObjectHandler(SimpleObjectHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="obj">Object</param>
        public ObjectEventArgs StartEvent(BaseInfo obj)
        {
            var e = new ObjectEventArgs
                {
                    Object = obj
                };

            return StartEvent(e);
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="obj">Object</param>
        /// <param name="content">Content that is processed by the handler</param>
        public ObjectEventArgs StartEvent(BaseInfo obj, ref string content)
        {
            // Prepare the parameters
            var e = new ObjectEventArgs
                {
                    Object = obj,
                    Content = content
                };

            // Handle the event and retrieve content
            StartEvent(e);

            content = e.Content;

            return e;
        }
    }
}
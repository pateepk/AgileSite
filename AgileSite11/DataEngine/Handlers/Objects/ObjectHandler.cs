using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object handler
    /// </summary>
    public class ObjectHandler : AdvancedHandler<ObjectHandler, ObjectEventArgs>, IRecursionControlHandler<ObjectEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public ObjectHandler(ObjectHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="obj">Handled object</param>
        public ObjectHandler StartEvent(BaseInfo obj)
        {
            var e = new ObjectEventArgs()
            {
                Object = obj
            };

            var h = StartEvent(e);

            return h;
        }


        /// <summary>
        /// Gets the recursion key of the class to identify recursion
        /// </summary>
        public string GetRecursionKey(ObjectEventArgs e)
        {
            if (e != null)
            {
                var obj = e.Object;
                if (obj != null)
                {
                    return obj.Generalized.GetObjectKey();
                }
            }

            return null;
        }
    }
}
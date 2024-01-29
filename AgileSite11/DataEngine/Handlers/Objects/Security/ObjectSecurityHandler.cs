using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object handler
    /// </summary>
    public class ObjectSecurityHandler : AdvancedHandler<ObjectSecurityHandler, ObjectSecurityEventArgs>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public ObjectSecurityHandler()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="parentHandler">Parent handler</param>
        public ObjectSecurityHandler(ObjectSecurityHandler parentHandler)
        {
            Parent = parentHandler;
        }


        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="obj">Handled object</param>
        public ObjectSecurityHandler StartEvent(BaseInfo obj)
        {
            ObjectSecurityEventArgs e = new ObjectSecurityEventArgs()
            {
                Object = obj
            };

            return StartEvent(e);
        }
    }
}
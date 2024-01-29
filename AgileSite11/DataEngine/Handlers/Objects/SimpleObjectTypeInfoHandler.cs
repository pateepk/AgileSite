using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Allows to decide whether the objects of given type will be processed in a specific manner.
    /// </summary>
    internal class SimpleObjectTypeInfoHandler : SimpleHandler<SimpleObjectTypeInfoHandler, ObjectTypeInfoEventArgs>
    {
        /// <summary>
        /// Initiates the event handling
        /// </summary>
        /// <param name="typeInfo">Handled object</param>
        public ObjectTypeInfoEventArgs StartEvent(ObjectTypeInfo typeInfo)
        {
            var e = new ObjectTypeInfoEventArgs
            {
                TypeInfo = typeInfo
            };

            StartEvent(e);

            return e;
        }
    }
}

using CMS.Core;

namespace CMS.DataEngine
{
    /// <summary>
    /// Object factory producing info objects based on the object type
    /// </summary>
    public class InfoObjectFactory : ObjectFactory<object>
    {
        /// <summary>
        /// Object type
        /// </summary>
        public string ObjectType 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="objectType">Object type</param>
        public InfoObjectFactory(string objectType)
        {
            ObjectType = objectType;
        }


        /// <summary>
        /// Creates new object of the given type
        /// </summary>
        public override object CreateNewObject()
        {
            return ModuleManager.GetObject(ObjectType);
        }
    }
}

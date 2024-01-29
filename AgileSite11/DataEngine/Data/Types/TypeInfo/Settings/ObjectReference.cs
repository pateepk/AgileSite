using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Represents a reference from one object type to another. 
    /// </summary>
    public class ObjectReference : AbstractDataContainer<ObjectReference>
    {
        /// <summary>
        /// Gets or sets the name of the reference column that stores the IDs of the referenced objects.
        /// </summary>
        [RegisterColumn]
        public string DependencyColumn
        {
            get;
            private set;
        }


        /// <summary>
        /// Gets or sets the object type that is the target of the reference.
        /// </summary>
        [RegisterColumn]
        public string DependencyObjectType
        {
            get;
            private set;
        }
        

        /// <summary>
        /// Creates a new ObjectReference instance, defining a reference from one object type to another.
        /// </summary>
        /// <param name="dependencyColumn">The name of the reference column that stores the IDs of the referenced objects</param>
        /// <param name="dependencyObjectType">The object type that is the target of the reference</param>
        public ObjectReference(string dependencyColumn, string dependencyObjectType)
        {
            DependencyColumn = dependencyColumn;
            DependencyObjectType = dependencyObjectType;
        }
    }
}

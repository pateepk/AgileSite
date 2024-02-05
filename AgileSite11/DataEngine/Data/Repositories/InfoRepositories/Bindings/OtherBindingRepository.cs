using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Specialized repository for binding object types
    /// </summary>
    public class OtherBindingRepository : InfoObjectRepository
    {
        /// <summary>
        /// Bound object.
        /// </summary>
        public BaseInfo BoundObject
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns the collection of objects indexed by object type, e.g. "cms.user".
        /// </summary>
        /// <param name="name">Name of the inner collection</param>
        public new OtherBindingCollection this[string name]
        {
            get
            {
                return (OtherBindingCollection)GetCollection(name);
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="boundObject">Parent object</param>
        public OtherBindingRepository(BaseInfo boundObject)
            : base((ICMSStorage)boundObject.Generalized)
        {
            BoundObject = boundObject;
        }
        

        /// <summary>
        /// Creates a new collection
        /// </summary>
        /// <param name="type">Object type</param>
        protected override InfoObjectCollection NewCollectionInternal(string type)
        {
            return new OtherBindingCollection(type, BoundObject);
        }
    }
}

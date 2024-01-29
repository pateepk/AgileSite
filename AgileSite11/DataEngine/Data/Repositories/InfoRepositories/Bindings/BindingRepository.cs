using System;
using System.Linq;
using System.Text;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Specialized repository for binding object types
    /// </summary>
    public class BindingRepository : InfoObjectRepository
    {
        /// <summary>
        /// Site bindings
        /// </summary>
        public BindingCollection Sites
        {
            get
            {
                return this[ParentObject.TypeInfo.SiteBinding];
            }
        }


        /// <summary>
        /// Returns the collection of objects indexed by object type, e.g. "cms.user".
        /// </summary>
        /// <param name="name">Name of the inner collection</param>
        public new BindingCollection this[string name]
        {
            get
            {
                return (BindingCollection)GetCollection(name);
            }
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentObject">Parent object</param>
        public BindingRepository(BaseInfo parentObject)
            : base(parentObject)
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="parentStorage">Parent storage object</param>
        public BindingRepository(ICMSStorage parentStorage)
            : base(parentStorage)
        {
        }


        /// <summary>
        /// Creates a new collection
        /// </summary>
        /// <param name="type">Object type</param>
        protected override InfoObjectCollection NewCollectionInternal(string type)
        {
            return new BindingCollection(type);
        }
    }
}

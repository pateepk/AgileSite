using System.Collections;
using System.Collections.Generic;

using CMS.Base;

namespace CMS.DataEngine
{
    /// <summary>
    /// Base class for the collection property wrapper
    /// </summary>
    public abstract class CollectionPropertyWrapper
    {
    }


    /// <summary>
    /// Wrapper that transforms the collection to values of its object properties
    /// </summary>
    public class CollectionPropertyWrapper<ObjectType> : CollectionPropertyWrapper, IEnumerable<ObjectProperty>, IIndexable<ObjectProperty> 
        where ObjectType : BaseInfo, IHierarchicalDataContainer
    {
        #region "Properties"

        /// <summary>
        /// Collection to wrap
        /// </summary>
        public IInfoObjectCollection<ObjectType> Collection
        {
            get;
            protected set;
        }


        /// <summary>
        /// Property name to extract
        /// </summary>
        public string PropertyName
        {
            get;
            protected set;
        }


        /// <summary>
        /// Returns the property of an object registered by the specific name.
        /// </summary>
        /// <param name="name">Object name (indexer)</param>
        public virtual ObjectProperty this[string name]
        {
            get
            {
                // Get object from the collection
                ObjectType obj = Collection[name];
                if (obj != null)
                {
                    return GetProperty(obj);
                }

                return default(ObjectProperty);
            }
            set
            {
                // Get object from the collection
                ObjectType obj = Collection[name];
                if (obj != null)
                {
                    // Set supported only for simple fields
                    obj.SetValue(PropertyName, value);
                }
            }
        }


        /// <summary>
        /// Gets or sets the object on specific index.
        /// </summary>
        /// <param name="index">Object index to get</param>
        public virtual ObjectProperty this[int index]
        {
            get
            {
                // Get object from the collection
                ObjectType obj = Collection[index];
                if (obj != null)
                {
                    return GetProperty(obj);
                }

                return default(ObjectProperty);
            }
            set
            {
                // Get object from the collection
                ObjectType obj = Collection[index];
                if (obj != null)
                {
                    // Set supported only for simple fields
                    obj.SetValue(PropertyName, value.Value);
                }
            }
        }


        /// <summary>
        /// Returns the number of items.
        /// </summary>
        public int Count
        {
            get
            {
                return Collection.Count;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="collection">Collection to wrap</param>
        /// <param name="propertyName">Property to extract</param>
        public CollectionPropertyWrapper(IInfoObjectCollection<ObjectType> collection, string propertyName)
        {
            Collection = collection;
            PropertyName = propertyName;
        }


        /// <summary>
        /// Gets the property value for the given object
        /// </summary>
        /// <param name="obj">Obj from which to take the property</param>
        protected virtual ObjectProperty GetProperty(ObjectType obj)
        {
            return new ObjectProperty(obj, PropertyName);
        }

        #endregion


        #region "IEnumerable Members"

        /// <summary>
        /// Gets the general enumerator for the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        #region "IEnumerable<ObjectProperty> Members"

        /// <summary>
        /// Gets the enumerator for the collection.
        /// </summary>
        public virtual IEnumerator<ObjectProperty> GetEnumerator()
        {
            // Extract property from all items
            foreach (ObjectType item in Collection)
            {
                yield return GetProperty(item);
            }
        }

        #endregion


        #region "INameIndexable and IIndexable Members"

        /// <summary>
        /// Returns the object registered by the specific name.
        /// </summary>
        /// <param name="name">Object name (indexer)</param>
        object INameIndexable.this[string name]
        {
            get
            {
                return this[name];
            }
        }


        /// <summary>
        /// Integer indexer, gets or sets the value on the specified index
        /// </summary>
        /// <param name="index">Index</param>
        object IIndexable.this[int index]
        {
            get
            {
                return this[index];
            }
        }

        #endregion

    }
}
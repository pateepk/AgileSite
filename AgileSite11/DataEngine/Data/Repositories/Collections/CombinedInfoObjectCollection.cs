using System.Collections;
using System.Collections.Generic;

namespace CMS.DataEngine
{
    /// <summary>
    /// Collection that combines several info object collections of specific type.
    /// </summary>
    public class CombinedInfoObjectCollection : CombinedInfoObjectCollection<IInfoObjectCollection<BaseInfo>, BaseInfo>
    {
        #region "Methods"

        /// <summary>
        /// Submits the changes in the collection to the database.
        /// </summary>
        public override void SubmitChanges()
        {
            // Submit all collections individually
            foreach (var collection in Collections)
            {
                collection.SubmitChanges();
            }
        }

        #endregion
    }


    /// <summary>
    /// Collection that combines several info object collections of specific type.
    /// </summary>
    public abstract class CombinedInfoObjectCollection<CollectionType, ObjectType> :
        IEnumerable<ObjectType>
        where CollectionType : IInfoObjectCollection, IEnumerable<ObjectType>
    {
        #region "Variables"

        /// <summary>
        /// List of inner collections.
        /// </summary>
        private List<CollectionType> mCollections = new List<CollectionType>();

        #endregion


        /// <summary>
        /// List of inner collections.
        /// </summary>
        protected List<CollectionType> Collections
        {
            get
            {
                return mCollections;
            }
        }


        #region "Methods"

        /// <summary>
        /// Adds the collection to the combined collection.
        /// </summary>
        /// <param name="collection">Collection to add</param>
        public void Add(CollectionType collection)
        {
            if (collection != null)
            {
                Collections.Add(collection);
            }
        }


        /// <summary>
        /// Submits the changes in the collection to the database.
        /// </summary>
        public abstract void SubmitChanges();

        #endregion


        #region IEnumerable<ObjectType> Members

        /// <summary>
        /// Gets the enumerator over the collection.
        /// </summary>
        public IEnumerator<ObjectType> GetEnumerator()
        {
            // Enumerate through all the collections
            foreach (CollectionType collection in Collections)
            {
                // Enumerate thru all the items
                foreach (ObjectType obj in collection)
                {
                    yield return obj;
                }
            }
        }

        #endregion


        #region IEnumerable Members

        /// <summary>
        /// Gets the enumerator over the collection.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}
using System;
using System.Diagnostics;
using System.Linq;


namespace CMS.ContinuousIntegration.Internal
{
    /// <summary>
    /// Debug view for <see cref="RepositoryLocationsCollection"/>.
    /// </summary>
    internal class RepositoryLocationsCollectionDebugView
    {
        private readonly string[] mItems;
        private readonly StructuredLocation[] mStructuredItems;


        /// <summary>
        /// Items of the collection (paths).
        /// </summary>
        [DebuggerDisplay("Count = {mItems.Length}")]
        public string[] Items
        {
            get
            {
                return mItems;
            }
        }


        /// <summary>
        /// Structured items of the collection (paths per objects).
        /// </summary>
        [DebuggerDisplay("Count = {mStructuredItems.Length}")]
        public StructuredLocation[] StructuredItems
        {
            get
            {
                return mStructuredItems;
            }
        }


        /// <summary>
        /// Initializes a new instance of debug view for <see cref="RepositoryLocationsCollection"/>.
        /// </summary>
        /// <param name="collection">Repository location collection for which to initialize the debug view.</param>
        public RepositoryLocationsCollectionDebugView(RepositoryLocationsCollection collection)
        {
            if (collection == null)
                throw new ArgumentNullException("collection");

            mItems = collection.ToArray();
            mStructuredItems = collection.StructuredLocations.ToArray();
        }
    }
}

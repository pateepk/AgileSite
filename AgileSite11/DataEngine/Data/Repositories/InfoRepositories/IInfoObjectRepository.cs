using System;

namespace CMS.DataEngine
{
    /// <summary>
    /// Interface for generic info object repository of collections
    /// </summary>
    /// <typeparam name="TCollection"></typeparam>
    public interface IInfoObjectRepository<out TCollection>
    {
        /// <summary>
        /// Creates new collection for the data.
        /// </summary>
        /// <param name="type">Type of the collection</param>
        TCollection NewCollection(string type);
    }
}

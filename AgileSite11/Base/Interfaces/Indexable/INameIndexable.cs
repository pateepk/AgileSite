namespace CMS.Base
{
    /// <summary>
    /// Interface to access collections through string indexers by name
    /// </summary>
    public interface INameIndexable
    {
        /// <summary>
        /// String indexer, gets or sets the value with the specified name
        /// </summary>
        /// <param name="name">Name</param>
        object this[string name]
        {
            get;
        }
    }


    /// <summary>
    /// Interface to access collections through string indexers by name - Generic variant
    /// </summary>
    public interface INameIndexable<out T> : INameIndexable
    {
        /// <summary>
        /// String indexer, gets or sets the value with the specified name
        /// </summary>
        /// <param name="name">Name</param>
        new T this[string name]
        {
            get;
        }
    }
}
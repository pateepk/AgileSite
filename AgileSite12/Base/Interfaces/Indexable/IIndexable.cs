namespace CMS.Base
{
    /// <summary>
    /// Interface to access collections through int and string indexers
    /// </summary>
    public interface IIndexable
    {
        /// <summary>
        /// Integer indexer, gets or sets the value on the specified index
        /// </summary>
        /// <param name="index">Index</param>
        object this[int index]
        {
            get;
        }
    }
        

    /// <summary>
    /// Interface to access collections through int and string indexers
    /// </summary>
    public interface IIndexable<out T> : INameIndexable<T>, IIndexable
    {
        /// <summary>
        /// Integer indexer, gets or sets the value on the specified index
        /// </summary>
        /// <param name="index">Index</param>
        new T this[int index]
        {
            get;
        }
    }
}
namespace CMS.Base
{
    /// <summary>
    /// Interface to access collections through specific key type
    /// </summary>
    public interface IGeneralIndexable
    {
        /// <summary>
        /// General indexer, gets or sets the value on the specified index
        /// </summary>
        /// <param name="index">Index</param>
        object this[object index]
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Interface to access collections through specific key type
    /// </summary>
    public interface IGeneralIndexable<in TKey, TObject> : IGeneralIndexable
    {
        /// <summary>
        /// Integer indexer, gets or sets the value on the specified index
        /// </summary>
        /// <param name="index">Index</param>
        TObject this[TKey index]
        {
            get;
            set;
        }
    }
}
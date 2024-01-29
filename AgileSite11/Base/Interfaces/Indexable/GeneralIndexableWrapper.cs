namespace CMS.Base
{
    /// <summary>
    /// Wrapper for general indexable interface
    /// </summary>
    public class GeneralIndexableWrapper<KeyType, ValueType> : IGeneralIndexable<KeyType, ValueType>
    {
        /// <summary>
        /// Source data
        /// </summary>
        public IGeneralIndexable Source 
        {
            get; 
            protected set; 
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="source">Source data</param>
        public GeneralIndexableWrapper(IGeneralIndexable source)
        {
            Source = source;
        }
        

        /// <summary>
        /// Typed indexer
        /// </summary>
        /// <param name="index"></param>
        public ValueType this[KeyType index]
        {
            get
            {
                return (ValueType)Source[index];
            }
            set
            {
                Source[index] = value;
            }
        }


        /// <summary>
        /// General indexer
        /// </summary>
        /// <param name="index">General indexer</param>
        public object this[object index]
        {
            get
            {
                return this[(KeyType)index];
            }
            set
            {
                this[(KeyType)index] = (ValueType)value;
            }
        }
    }
}

namespace CMS.DataEngine
{
    /// <summary>
    /// Settings for provider hashtables
    /// </summary>
    public class HashtableSettings
    {
        /// <summary>
        /// General flag if hashtables are enabled or not
        /// </summary>
        public bool UseHashtables
        {
            get
            {
                return ID || Name || GUID || FullName;
            }
        }


        /// <summary>
        /// If true, ID hashtable is used for caching
        /// </summary>
        public bool ID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, name hashtable is used for caching
        /// </summary>
        public bool Name
        {
            get;
            set;
        }


        /// <summary>
        /// If true, full name hashtable is used for caching
        /// </summary>
        public bool FullName
        {
            get;
            set;
        }


        /// <summary>
        /// If true, GUID hashtable is used for caching
        /// </summary>
        public bool GUID
        {
            get;
            set;
        }


        /// <summary>
        /// If true, weak references are used within hashtables. Use in case the provider handles a lot of objects to allow releasing memory
        /// </summary>
        public bool UseWeakReferences
        {
            get;
            set;
        }


        /// <summary>
        /// Specifies the way how the hashtable values are initialized
        /// </summary>
        public LoadHashtableEnum Load
        {
            get;
            set;
        }
    }
}

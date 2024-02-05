namespace CMS.Base
{
    /// <summary>
    /// Dictionary with two levels of hierarchy
    /// </summary>
    public class TwoLevelDictionary<PrimaryKeyType, SecondaryKeyType, ValueType>
    {
        #region "Variables"

        /// <summary>
        /// Inner dictionary of dictionaries
        /// </summary>
        private SafeDictionary<PrimaryKeyType, SafeDictionary<SecondaryKeyType, ValueType>> mDictionary;

        #endregion


        #region "Properties"

        /// <summary>
        /// Inner dictionary of dictionaries
        /// </summary>
        protected SafeDictionary<PrimaryKeyType, SafeDictionary<SecondaryKeyType, ValueType>> Dictionary
        {
            get
            {
                return mDictionary ?? (mDictionary = NewDictionary<PrimaryKeyType, SafeDictionary<SecondaryKeyType, ValueType>>());
            }
        }


        /// <summary>
        /// Gets the second level of dictionary based on the given key
        /// </summary>
        /// <param name="primaryKey">Primary key</param>
        public SafeDictionary<SecondaryKeyType, ValueType> this[PrimaryKeyType primaryKey]
        {
            get
            {
                return Dictionary[primaryKey];
            }
            set
            {
                Dictionary[primaryKey] = value;
            }
        }


        /// <summary>
        /// If true, the dictionary handles string keys as case insensitive
        /// </summary>
        public bool CaseInsensitive 
        { 
            get; 
            protected set; 
        }


        /// <summary>
        /// Gets or sets the dictionary value
        /// </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="secondaryKey">Secondary key</param>
        public ValueType this[PrimaryKeyType primaryKey, SecondaryKeyType secondaryKey]
        {
            get
            {
                // Check the inner dictionary for the given type
                var dict = Dictionary[primaryKey];
                if (dict == null)
                {
                    return default(ValueType);
                }

                // Get inner value
                return dict[secondaryKey];
            }
            set
            {
                // Ensure the inner dictionary for the given type
                primaryKey = GetInnerPrimaryKey(primaryKey);
                
                var dict = Dictionary[primaryKey];
                if (dict == null)
                {
                    dict = NewDictionary<SecondaryKeyType, ValueType>();
                    Dictionary[primaryKey] = dict;
                }

                // Set inner value
                dict[secondaryKey] = value;
            }
        }

        
        /// <summary>
        /// Creates a new internal dictionary
        /// </summary>
        private SafeDictionary<KeyType, ValType> NewDictionary<KeyType, ValType>()
        {
            if (CaseInsensitive && typeof(KeyType) == typeof(string))
            {
                return (SafeDictionary<KeyType, ValType>)(object)new StringSafeDictionary<ValType>();
            }

            return new SafeDictionary<KeyType, ValType>();
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Constructor
        /// </summary>
        public TwoLevelDictionary()
        {
        }


        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="caseSensitive">If true, the dictionary is case sensitive</param>
        public TwoLevelDictionary(bool caseSensitive)
        {
            CaseInsensitive = !caseSensitive;
        }


        /// <summary>
        /// Converts the primary key to its inner representation
        /// </summary>
        /// <param name="key">Primary key</param>
        private PrimaryKeyType GetInnerPrimaryKey(PrimaryKeyType key)
        {
            return HandleCaseSensitivity(key);
        }


        /// <summary>
        /// Handles the case sensitivity settings for the given key
        /// </summary>
        /// <param name="key">Key to handle</param>
        private KeyType HandleCaseSensitivity<KeyType>(KeyType key)
        {
            if (CaseInsensitive)
            {
                // Convert key to string
                var innerKey = key as string;
                if (innerKey != null)
                {
                    innerKey = innerKey.ToLowerCSafe();

                    key = (KeyType)(object)innerKey;
                }
            }

            return key;
        }


        /// <summary>
        /// Clears the dictionary
        /// </summary>
        public void Clear()
        {
            Dictionary.Clear();
        }

        #endregion
    }
}

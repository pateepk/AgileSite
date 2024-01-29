using System;
using System.Collections;
using System.Collections.Generic;

namespace CMS.Base
{
    /// <summary>
    /// Hashtable which can use multiple keys to access the same data. The keys are separated by the separator.
    /// </summary>
    /// <remarks>
    /// This API supports the framework infrastructure and is not intended to be used directly from your code.
    /// </remarks>
    public class MultiKeyDictionary<ValueType> : StringSafeDictionary<ValueType>
    {
        #region "Variables and constants"

        // Table with main keys indexed by all the sub-keys [subKey] -> [mainKey]
        private Dictionary<string, string> mSubKeys = new Dictionary<string, string>();

        #endregion


        #region "Properties"

        /// <summary>
        /// Gets or sets the item.
        /// </summary>
        /// <param name="key">Key</param>
        public override ValueType this[string key]
        {
            get
            {
                // Try to get the main key
                string mainKey = GetMainKey(key);
                if (mainKey != null)
                {
                    if ((base[mainKey] == null) && (typeof(ValueType).IsValueType) || (AllowNulls && ((object)base[mainKey] == NullValue)))
                    {
                        return default(ValueType);
                    }

                    return base[mainKey];
                }
                
                return default(ValueType);
            }
            set
            {
                // Set all subKeys
                // Get the original main key
                string mainKey = GetMainKey(key);
                if (mainKey != null)
                {
                    // Reset the original subkeys
                    if (mainKey.Contains(SeparatorConstants.SEPARATOR))
                    {
                        string[] subKeys = mainKey.Split(SeparatorConstants.SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string subKey in subKeys)
                        {
                            // Clear the sub key
                            mSubKeys[subKey] = null;
                        }
                    }
                    else
                    {
                        mSubKeys[mainKey] = null;
                    }

                    // Clear the original main key record
                    base[mainKey] = default(ValueType);
                }


                // Set the new sub-keys
                if (key != null)
                {
                    if (key.Contains(SeparatorConstants.SEPARATOR))
                    {
                        string[] subKeys = key.Split(SeparatorConstants.SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string subKey in subKeys)
                        {
                            // Clear the sub key
                            mSubKeys[subKey] = key;
                        }
                    }
                    else
                    {
                        mSubKeys[key] = key;
                    }
                }

                // Set main record
                base[key] = value;
            }
        }

        #endregion


        #region "Methods"

        /// <summary>
        /// Default constructor.
        /// </summary>
        public MultiKeyDictionary()
        {
        }


        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="d">Source data dictionary</param>
        public MultiKeyDictionary(IDictionary d)
            : base(d)
        {
        }


        /// <summary>
        /// Gets the main object key for the given key.
        /// </summary>
        /// <param name="key">Key to find</param>
        protected string GetMainKey(string key)
        {
            // If the main table contains the key, it is considered main key
            if (base.ContainsKey(key))
            {
                return key;
            }

            // Try to find by the sub-key
            string stringKey = key;
            if (stringKey.Contains(SeparatorConstants.SEPARATOR))
            {
                string[] subKeys = stringKey.Split(SeparatorConstants.SEPARATORS, StringSplitOptions.RemoveEmptyEntries);
                foreach (string subKey in subKeys)
                {
                    // Check if the main key is present
                    string mainKey;
                    if (mSubKeys.TryGetValue(subKey, out mainKey) && (mainKey != null))
                    {
                        return mainKey;
                    }
                }
            }
            else
            {
                // Check if the main key is present in subkey table
                string mainKey;
                if (mSubKeys.TryGetValue(key, out mainKey) && (mainKey != null))
                {
                    return mainKey;
                }
            }

            return null;
        }


        /// <summary>
        /// Adds the value to the table.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="value">Value</param>
        public override void Add(object key, object value)
        {
            this[key] = value;
            
        }


        /// <summary>
        /// Clears the table.
        /// </summary>
        public override void Clear()
        {
            base.Clear();
            mSubKeys.Clear();
        }


        /// <summary>
        /// Gets or sets the value associated with key value
        /// </summary>
        /// <param name="key">Key value</param>
        public override object this[object key]
        {
            get
            {
                return this[Convert.ToString(key)];
            }
            set
            {
                this[Convert.ToString(key)] = (ValueType)value;
            }
        }


        /// <summary>
        /// Returns true if the table contains specific key.
        /// </summary>
        /// <param name="key">Key to find</param>
        public override bool Contains(object key)
        {
            return (this[key] != null);
        }


        /// <summary>
        /// Returns true if the table contains specific key.
        /// </summary>
        /// <param name="key">Key to find</param>
        public override bool ContainsKey(object key)
        {
            return Contains(key);
        }


        /// <summary>
        /// Removes the item from the table.
        /// </summary>
        /// <param name="key">Key</param>
        public override void Remove(object key)
        {
            this[key] = null;
        }


        /// <summary>
        /// Gets the cloned Hashtable.
        /// </summary>
        public override object Clone()
        {
            MultiKeyDictionary<ValueType> result = new MultiKeyDictionary<ValueType>(this);
            result.mSubKeys = new Dictionary<string, string>(mSubKeys);

            return result;
        }        

        #endregion
    }

    
    internal static class SeparatorConstants
    {
        internal const string SEPARATOR = ";";
        internal static readonly char[] SEPARATORS = new char[] { ';' };
    }
}